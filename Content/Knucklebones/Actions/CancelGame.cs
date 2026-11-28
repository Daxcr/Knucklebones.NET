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

            MessageComponent components = await MessageBuilder.BuildInviteComponents(meta, MessageBuilder.InviteState.Cancelled, expiry);

            if (meta.Guild == null)
                await Context.ModifyOriginalResponseAsync(message =>
                {
                    message.Components = components;
                });
            else
                await Context.ModifyAsync(message =>
                {
                    message.Components = components;
                });

            BotClient.Games.Remove(meta);
            meta.GameDeclined = true;
        }
    }
}
