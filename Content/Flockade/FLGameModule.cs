using CotLMinigames.DB;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace CotLMinigames.Flockade;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class FLGameModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int TurnExpiry = 300;

    [SlashCommand("flockade", "Challenge somebody to a game of Flockade")]
    public async Task Flockade(IUser user,
        [Choice("None", 0)]
        [Choice("1 wool (Small)", 1)]
        [Choice("2 wool (Small)", 2)]
        [Choice("3 wool (Small)", 3)]
        [Choice("4 wool (Small)", 4)]
        [Choice("5 wool (Small)", 5)]
        [Choice("6 wool (Small)", 6)]
        [Choice("7 wool (Small)", 7)]
        [Choice("8 wool (Small)", 8)]
        [Choice("9 wool (Small)", 9)]
        [Choice("10 wool (Large)", 10)]
        [Choice("12 wool (Large)", 12)]
        [Choice("15 wool (Large)", 15)]
        [Choice("20 wool (Large)", 20)]
        [Choice("25 wool (Large)", 25)]
        [Choice("30 wool (Massive)", 30)]
        [Choice("35 wool (Massive)", 35)]
        [Choice("40 wool (Massive)", 40)]
        [Choice("50 wool (Massive)", 50)]
        int bet = 0
    )
    {
        UserContext ctx = new(Context);
        await Flockade(ctx, user, bet);
    }

    public static async Task Rematch(string[] ButtonData, SocketMessageComponent component)
    {
        try
        {
            ulong uid = component.User.Id;
            ulong oldInitID = ulong.Parse(ButtonData[1]);
            ulong oldOppID = ulong.Parse(ButtonData[2]);
            int bet = int.Parse(ButtonData[3]);

            ulong[] IDs = { oldInitID, oldOppID };
            if (!IDs.Contains(uid))
            {
                await component.RespondAsync("You didn't play in this game. You can challenge a player with `/flockade`.", ephemeral: true);
                return;
            }

            ulong newOppID = IDs[1 - IDs.IndexOf(uid)];
            UserContext ctx = new(component);

            IUser opponent = await BotClient.Client.GetUserAsync(newOppID);
            if (component.Channel is IGuildChannel guildChannel)
            {
                IGuildUser? member = opponent as IGuildUser ?? await guildChannel.Guild.GetUserAsync(newOppID);
                if (member == null)
                {
                    await component.RespondAsync("Your opponent no longer has access to this channel.");
                    return;
                }
            }

            await Flockade(ctx, opponent, bet);
        } catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async Task Flockade(UserContext Context, IUser? user = null, int bet = 0)
    {
        if (Context.User!.Id == user?.Id)
        {
            await Context.RespondAsync("You can't challenge yourself! What a disappointment.", ephemeral: true);
            return;
        }
        if (user?.Id == BotClient.Client.CurrentUser.Id)
            bet = 0;

        await Context.DeferAsync();

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(Context.User.Id, db);

        if (user != null)
        {
            UserData? opponent = await Database.GetUser(user.Id, db);

            if (opponent.Inventory.Wool < bet)
            {
                await Context.FollowupAsync($"Your opponent does not have enough wool: `{opponent.Inventory.Wool}/{bet}`");
                return;
            }

            if (!opponent.AcceptingGames)
            {
                await Context.FollowupAsync($"Your opponent isn't accepting games right now.");
                return;
            }
        }

        if (initiator.Inventory.Wool < bet)
        {
            await Context.FollowupAsync($"You don't have enough wool: `{initiator.Inventory.Wool}/{bet}`");
            return;
        }

        FLGameMetadata meta = new()
        {
            InitiatorID = Context.User.Id,
            OpponentID = user != null ? user.Id : 0,
            InitiatedChannelID = Context.Channel?.Id,
            Bet = bet
        };
        if (Context.Guild == null)
        {
            meta.GameExpiry = GameMetadata.ShortGameExpiry;
        }

        DateTimeOffset expiryoffset = DateTimeOffset.UtcNow.AddSeconds(GameMetadata.ChallengeExpiry);
        TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);
        Embed embed;
        MessageComponent components;

        switch (user)
        {
            case null:
                embed = new EmbedBuilder()
                    .WithTitle("Match request")
                    .WithDescription($"<@{Context.User.Id}> would like to be challenged to a game of Flockade.\nThis request will expire {expiry}.\n\nBet: {BotClient.Emojis.Wool} {meta.Bet}")
                    .WithColor(Color.Blue)
                    .Build();

                components = new ComponentBuilder()
                    .WithButton("Accept", $"acceptfl/{meta.ID}", ButtonStyle.Success)
                    .Build();
                break;

            default:
                if (user.Id == BotClient.Client.CurrentUser.Id)
                {
                    embed = new EmbedBuilder()
                        .WithTitle("Match request")
                        .WithDescription($"<@{Context.User.Id}> would like to challenge me.\nThis request will expire {expiry}.")
                        .WithColor(Color.Blue)
                        .Build();

                    components = new ComponentBuilder()
                        .WithButton("Go!", $"acceptbotfl/{meta.ID}", ButtonStyle.Success)
                        .Build();
                }
                else
                {
                    embed = new EmbedBuilder()
                        .WithTitle("Match request")
                        .WithDescription($"<@{user.Id}> has been challenged to a game of Flockade by <@{Context.User.Id}>.\nThis request will expire {expiry}.\n\nBet: {BotClient.Emojis.Wool} {meta.Bet}")
                        .WithColor(Color.Blue)
                        .Build();

                    components = new ComponentBuilder()
                        .WithButton("Accept", $"acceptfl/{meta.ID}", ButtonStyle.Success)
                        .WithButton("Decline", $"declinefl/{meta.ID}", ButtonStyle.Danger)
                        .Build();
                }

                break;
        }


        string ping = string.Empty;

        if (Context.Guild != null && user != null)
        {
            ServerSettings servermeta = await Database.GetGuild(Context.Guild.Id, db);
            ping = servermeta.PingOpponents ? $"<@{user?.Id}>" : string.Empty;
        }


        await Context.FollowupAsync(ping, embed: embed, components: components);
        _ = WaitForChallengeExpiry(Context, meta, Context.User.Id, user?.Id, expiry, bet);

        initiator.Inventory.Wool -= bet;
        await db.SaveChangesAsync();
    }

    public async static Task WaitForChallengeExpiry(UserContext Context, FLGameMetadata meta, ulong initiatorID, ulong? opponentID, TimestampTag expiry, int bet)
    {
        await Task.Delay(GameMetadata.ChallengeExpiry * 1000);

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(initiatorID, db);

        if (meta != null && !meta.GameStarted && !meta.GameDeclined)
        {
            initiator.Inventory.Wool += bet;

            MessageComponent disabledComponents;
            Embed embed;

            if (opponentID != null)
            {
                embed = new EmbedBuilder()
                    .WithTitle("Match request (Expired)")
                    .WithDescription($"<@{opponentID}> has been challenged to a game of Flockade by <@{initiatorID}>.\nThis request expired {expiry}. \n\nBet: {BotClient.Emojis.Wool} {meta.Bet}")
                    .WithColor(Color.DarkerGrey)
                    .Build();

                disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();
            } else
            {
                embed = new EmbedBuilder()
                    .WithTitle("Match request (Expired)")
                    .WithDescription($"<@{initiatorID}> would like to be challenged to a game of Flockade.\nThis request expired {expiry}. \n\nBet: {BotClient.Emojis.Wool} {meta.Bet}")
                    .WithColor(Color.DarkerGrey)
                    .Build();

                disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();
            }

            await Context.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed;
                message.Components = disabledComponents;
            });

            BotClient.Games.Remove(meta);
        }

        await db.SaveChangesAsync();
    }
}
