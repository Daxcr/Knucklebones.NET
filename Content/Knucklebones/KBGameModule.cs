using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using CotLMinigames.DB;

namespace CotLMinigames.Knucklebones;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class KBGameModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int TurnExpiry = 300;

    [SlashCommand("knucklebones", "Challenge somebody to a game of Knucklebones")]
    public async Task Knucklebones(IUser? user = null,
        [Choice("None", 0)]
        [Choice("1 coin (Small)", 1)]
        [Choice("2 coins (Small)", 2)]
        [Choice("3 coins (Small)", 3)]
        [Choice("4 coins (Small)", 4)]
        [Choice("5 coins (Small)", 5)]

        [Choice("6 coins (Small)", 6)]
        [Choice("7 coins (Small)", 7)]
        [Choice("8 coins (Small)", 8)]
        [Choice("9 coins (Small)", 9)]
        [Choice("10 coins (Large)", 10)]
        [Choice("12 coins (Large)", 12)]
        [Choice("15 coins (Large)", 15)]
        [Choice("20 coins (Large)", 20)]
        [Choice("25 coins (Large)", 25)]
        [Choice("30 coins (Massive)", 30)]
        [Choice("35 coins (Massive)", 35)]
        [Choice("40 coins (Massive)", 40)]
        [Choice("50 coins (Massive)", 50)]
        int bet = 0
    )
    {
        if (Context.User.Id == user?.Id)
        {
            await RespondAsync("You can't challenge yourself! What a disappointment.", ephemeral: true);
            return;
        }
        if (user?.Id == BotClient.Client.CurrentUser.Id)
            bet = 0;
            
        await DeferAsync();

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(Context.User.Id, db);

        if (user != null)
        {
            UserData? opponent = await Database.GetUser(user.Id, db);

            if (opponent.Inventory.Coins < bet)
            {
                await FollowupAsync($"Your opponent does not have enough coins: `{opponent.Inventory.Coins}/{bet}`");
                return;
            }

            if (!opponent.AcceptingGames)
            {
                await FollowupAsync($"Your opponent isn't accepting games right now.");
                return;
            }
        }

        if (initiator.Inventory.Coins < bet)
        {
            await FollowupAsync($"You don't have enough coins: `{initiator.Inventory.Coins}/{bet}`");
            return;
        }

        KBGameMetadata meta = new()
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
                    .WithDescription($"<@{Context.User.Id}> would like to be challenged to a game of Knucklebones.\nThis request will expire {expiry}.")
                    .WithColor(Color.Blue)
                    .Build();

                components = new ComponentBuilder()
                    .WithButton("Accept", $"acceptkb/{meta.ID}", ButtonStyle.Success)
                    .Build();
                break;

            default:
                if (user.Id == BotClient.Client.CurrentUser.Id)
                {
                    embed = new EmbedBuilder()
                        .WithTitle("Match request")
                        .WithDescription($"<@{Context.User.Id}> would like to challenge me.\nThis request will expire {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                        .WithColor(Color.Blue)
                        .Build();

                    components = new ComponentBuilder()
                        .WithButton("Go!", $"acceptbotkb/{meta.ID}", ButtonStyle.Success)
                        .Build();
                }
                else
                {
                    embed = new EmbedBuilder()
                        .WithTitle("Match request")
                        .WithDescription($"<@{user.Id}> has been challenged to a game of Knucklebones by <@{Context.User.Id}>.\nThis request will expire {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                        .WithColor(Color.Blue)
                        .Build();

                    components = new ComponentBuilder()
                        .WithButton("Accept", $"acceptkb/{meta.ID}", ButtonStyle.Success)
                        .WithButton("Decline", $"declinekb/{meta.ID}", ButtonStyle.Danger)
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


        await FollowupAsync(ping, embed: embed, components: components);
        IUserMessage message = await GetOriginalResponseAsync();
        _ = WaitForChallengeExpiry(meta, Context.Interaction, Context.User.Id, user?.Id, expiry, bet);

        initiator.Inventory.Coins -= bet;
        var entry = db.Entry(initiator.Inventory);
        
        await db.SaveChangesAsync();
    }

    public async static Task WaitForChallengeExpiry(KBGameMetadata meta, SocketInteraction interaction, ulong initiatorID, ulong? opponentID, TimestampTag expiry, int bet)
    {
        await Task.Delay(GameMetadata.ChallengeExpiry * 1000);

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(initiatorID, db);

        if (meta != null && !meta.GameStarted && !meta.GameDeclined)
        {
            initiator.Inventory.Coins += bet;

            MessageComponent disabledComponents;
            Embed embed;

            if (opponentID != null)
            {
                embed = new EmbedBuilder()
                    .WithTitle("Match request (Expired)")
                    .WithDescription($"<@{opponentID}> has been challenged to a game of Knucklebones by <@{initiatorID}>.\nThis request expired {expiry}. \n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
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
                    .WithDescription($"<@{initiatorID}> would like to be challenged to a game of Knucklebones.\nThis request expired {expiry}. \n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                    .WithColor(Color.DarkerGrey)
                    .Build();
                    
                disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();
            }

            await interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed;
                message.Components = disabledComponents;
            });

            BotClient.Games.Remove(meta);
        }

        await db.SaveChangesAsync();
    }
}