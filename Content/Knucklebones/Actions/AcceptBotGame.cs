using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AcceptBotGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];

        UserContext ctx = new(component);
        await AcceptBotGame(ctx, gameID);
    }

    public static async Task AcceptBotGame(UserContext Context, string gameID)
    {
        try
        {
            IUser? user = Context.User;
            KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);

            if (meta != null)
            {
                if (meta.InitiatorID != user?.Id)
                {
                    await Context.RespondAsync("This isn't for you.", ephemeral: true);
                    return;
                }

                await Context.DeferAsync();

                IMessageChannel? channel = Context.Channel;

                IUser initiator = BotClient.Client.GetUser(meta.InitiatorID);
                IUser opponent = BotClient.Client.GetUser(meta.OpponentID);

                DateTimeOffset offset = DateTimeOffset.UtcNow.AddSeconds(meta.GameExpiry);
                meta.GameExpiryDisplay = TimestampTag.FromDateTimeOffset(offset, TimestampTagStyles.Relative);
                meta.GameStarted = true;

                Random rnd = new();
                meta.InitiatorTurn = true;

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
        } catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
