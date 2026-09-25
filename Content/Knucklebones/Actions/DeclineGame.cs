using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task DeclineGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];

        UserContext ctx = new(component);
        await DeclineGame(ctx, gameID);
    }

    public static async Task DeclineGame(UserContext Context ,string gameID)
    {
        IUser? user = Context.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.OpponentID != user?.Id)
            {
                await Context.RespondAsync("This isn't for you.", ephemeral: true);
                return;
            }

            await Context.DeferAsync();

            using DatabaseContext db = Database.Create();
            UserData? initiator = await Database.GetUser(meta.InitiatorID, db);

            initiator.Inventory.Coins += meta.Bet;

            await db.SaveChangesAsync();

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Declined)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was declined {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                .WithColor(Color.Red)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Danger, disabled: true)
                .Build();

            if (meta.Guild == null)
                await Context.ModifyOriginalResponseAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });
            else
                await Context.ModifyAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });

            BotClient.Games.Remove(meta);
            meta.GameDeclined = true;
        }
    }
}
