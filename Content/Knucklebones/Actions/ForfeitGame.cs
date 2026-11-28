using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task ForfeitGame(string[] buttondata, SocketMessageComponent component)
    {
        string gameID = buttondata[1];

        UserContext ctx = new(component);
        await ForfeitGame(ctx, gameID);
    }
    public static async Task ForfeitGame(UserContext Context, string gameID)
    {
        await Context.DeferAsync();

        KBGameMetadata meta = (KBGameMetadata)BotClient.Games.FirstOrDefault(game => gameID == game.ID)!;

        if (!new[] { meta.InitiatorID, meta.OpponentID }.Contains(Context.User!.Id))
            return;

        if (!BotClient.Games.Contains(meta))
            return;


        bool initiator = Context.User.Id == meta.InitiatorID;

        if (initiator)
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.InitiatorTable = new();
        }
        else
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.OpponentTable = new();
        }

        await EndGame(Context, meta, true);
    }
}
