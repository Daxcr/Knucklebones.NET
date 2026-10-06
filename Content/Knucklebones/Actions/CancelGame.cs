using CotLMinigames.DB;
using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task CancelGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];

        UserContext ctx = new(component);
        await CancelGame(ctx, gameID);
    }
    public static async Task CancelGame(UserContext Context, string gameID)
    {   
        IUser? user = Context.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.InitiatorID != user?.Id)
            {
                await Context.RespondAsync("You can't cancel. If you are being challenged, you can press Decline.", ephemeral: true);
                return;
            }

            await Context.DeferAsync();

            using DatabaseContext db = Database.Create();
            UserData? initiator = await Database.GetUser(meta.InitiatorID, db);

            initiator.Inventory.Coins += meta.Bet;

            await db.SaveChangesAsync();

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed;
            MessageComponent disabledComponents;

            if (meta.OpponentID != 0)
            {
                embed = new EmbedBuilder()
                    .WithTitle("Match request (Cancelled)")
                    .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was cancelled {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                    .Build();
                    
                disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Cancel", $"cancel/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();
            } else
            {
                embed = new EmbedBuilder()
                    .WithTitle("Match request (Cancelled)")
                    .WithDescription($"<@{meta.InitiatorID}> would like to be challenged to a game of Knucklebones.\nThis request was cancelled {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                    .Build();
                    
                disabledComponents = new ComponentBuilder()
                    .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Challenge the Bot", $"bot/disabled", ButtonStyle.Secondary, disabled: true)
                    .WithButton("Cancel", $"cancel/disabled", ButtonStyle.Secondary, disabled: true)
                    .Build();
            }

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