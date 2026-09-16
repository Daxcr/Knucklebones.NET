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
        IUser user = component.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.OpponentID != user.Id)
            {
                await component.RespondAsync("This isn't for you.", ephemeral: true);
                return;
            }

            await component.DeferAsync();

            using DatabaseContext db = Database.Create();
            UserData? initiator = await Database.GetUser(meta.InitiatorID, db);

            initiator.Inventory.Coins += meta.Bet;

            await db.SaveChangesAsync();

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Declined)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was declined {expiry}.")
                .WithColor(Color.Red)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Danger, disabled: true)
                .Build();

            if (meta.Guild == null)
                await component.ModifyOriginalResponseAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });
            else
                await component.Message.ModifyAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });

            BotClient.Games.Remove(meta);
            meta.GameDeclined = true;
        }
    }
}