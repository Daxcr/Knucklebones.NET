using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Flockade;

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
        FLGameMetadata? meta = (FLGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);

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
            if (opponentObj.Inventory.Wool < meta.Bet)
            {
                await Context.FollowupAsync($"You don't have enough wool! {BotClient.Emojis.Wool} {opponentObj.Inventory.Wool}/{meta.Bet}");
                return;
            }
            opponentObj.Inventory.Wool -= meta.Bet;

            await db.SaveChangesAsync();

            IMessageChannel? channel = Context.Channel;

            IUser initiator = BotClient.Client.GetUser(meta.InitiatorID);
            IUser opponent = BotClient.Client.GetUser(meta.OpponentID);

            DateTimeOffset offset = DateTimeOffset.UtcNow.AddSeconds(meta.GameExpiry);
            meta.GameExpiryDisplay = TimestampTag.FromDateTimeOffset(offset, TimestampTagStyles.Relative);
            meta.GameStarted = true;

            Random rnd = new();
            meta.InitiatorTurn = rnd.Next(0, 2) == 0;

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Accepted)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Flockade by <@{meta.InitiatorID}>.\nThis request was accepted {expiry}.\n\nBet: {BotClient.Emojis.Wool} {meta.Bet}")
                .WithColor(Color.Green)
                .Build();

            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Success, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
                .Build();

            await Context.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed;
                message.Components = disabledComponents;
            });

            Embed boardembed = await BuildBoardEmbed(meta, meta.InitiatorTurn);
            List<Embed> pieces = new();
            for (int i = 0; i < 3; i++)
                pieces.Add(await BuildPieceEmbed(meta));

            MessageComponent buttons = await BuildPieceButtons(meta);

            List<Embed> embeds = [boardembed, .. pieces];

            IUserMessage? msg = await Context.FollowupAsync(embeds: embeds.ToArray(), components: buttons);

            UserContext ctx = new(msg!);
            _ = WaitForGameExpiry(ctx, meta);

            if (Context.Channel is SocketGuildChannel guildChannel && guildChannel.Guild != null)
            {
                RestUserMessage temp = (RestUserMessage)await meta.Channel!.SendMessageAsync($"<@{meta.InitiatorID}><@{meta.OpponentID}>"); // ghost ping! :D
                await temp.DeleteAsync();
            }
        }
    }
}
