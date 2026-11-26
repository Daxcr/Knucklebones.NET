using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AcceptGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];

        UserContext ctx = new(component);
        await AcceptGame(ctx, gameID);
    }
    public static async Task AcceptGame(UserContext Context, string gameID)
    {
        IUser? user = Context.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);

        if (meta != null)
        {
            if (meta.OpponentID != 0)
            {
                if (meta.OpponentID != user?.Id)
                {
                    await Context.RespondAsync("This isn't for you.", ephemeral: true);
                    return;
                }
            } else
            {
                if (meta.InitiatorID == user?.Id)
                {
                    await Context.RespondAsync("You can't challenge yourself!", ephemeral: true);
                    return;
                }
                meta.OpponentID = user!.Id;
            }

            await Context.DeferAsync();

            using var db = Database.Create();

            UserData opponentObj = await Database.GetUser(meta.OpponentID, db);
            if (opponentObj.Inventory.Coins < meta.Bet)
            {
                await Context.FollowupAsync($"You don't have enough coins! {BotClient.Emojis.Coin} {opponentObj.Inventory.Coins}/{meta.Bet}");
                return;
            }
            opponentObj.Inventory.Coins -= meta.Bet;

            await db.SaveChangesAsync();

            IMessageChannel? channel = Context.Channel;

            IUser initiator = BotClient.Client.GetUser(meta.InitiatorID);
            IUser opponent = BotClient.Client.GetUser(meta.OpponentID);

            DateTimeOffset offset = DateTimeOffset.UtcNow.AddSeconds(meta.GameExpiry);
            meta.GameExpiryDisplay = TimestampTag.FromDateTimeOffset(offset, TimestampTagStyles.Relative);
            meta.GameStarted = true;

            Random rnd = new();
            meta.InitiatorTurn = rnd.Next(0, 2) == 0;

            if (meta.Guild == null)
            {
                meta.CurrentDice = (byte)new Random().Next(1, 7);

                await Context.ModifyOriginalResponseAsync(async message =>
                {
                    message.Content = null;
                    message.Embed = null;
                    message.Embeds = null;
                    message.Flags = MessageFlags.ComponentsV2;
                    message.Components = await BuildGameComponents(meta);
                });

                _ = WaitForGameExpiry(Context, meta);
            }
            else
            {
                DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
                TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

                Embed embed = new EmbedBuilder()
                    .WithTitle("Match request (Accepted)")
                    .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was accepted {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                    .WithColor(Color.Green)
                    .Build();

                MessageComponent disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Success, disabled: true)
                    .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Cancel", $"cancel/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();

                await Context.ModifyOriginalResponseAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });

                meta.CurrentDice = (byte)new Random().Next(1, 7);

                IUserMessage? msg = await Context.FollowupAsync(components: await BuildGameComponents(meta));

                UserContext ctx = new(msg!);
                _ = WaitForGameExpiry(ctx, meta);

                if (Context.Channel is SocketGuildChannel guildChannel)
                {
                    RestUserMessage temp = (RestUserMessage)await meta.Channel!.SendMessageAsync($"<@{meta.InitiatorID}><@{meta.OpponentID}>"); // ghost ping! :D
                    await temp.DeleteAsync();
                }
            }
        }
    }
}
