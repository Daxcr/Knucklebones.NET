using Discord.WebSocket;

namespace CotLMinigames.Flockade;

public static partial class Actions
{
    public static async Task SelectPosition(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];
        int piece = int.Parse(ButtonData[2]);

        UserContext ctx = new(component);
        await SelectPosition(ctx, gameID, piece);
    }
    public static async Task SelectPosition(UserContext Context, string gameID, int piece)
    {
        FLGameMetadata? meta = (FLGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        if (meta == null) return;

        if (!new[] { meta.InitiatorID, meta.OpponentID }.Contains(Context.User!.Id))
        {
            await Context.RespondAsync("You're not playing in this game.", ephemeral: true);
            return;
        }

        if ((meta.InitiatorTurn && Context.User.Id != meta.InitiatorID) ||
            (!meta.InitiatorTurn && Context.User.Id == meta.InitiatorID))
        {
            await Context.RespondAsync("Not your turn :(", ephemeral: true);
            return;
        }

        FLGameMetadata.FlockPiece fpiece = meta.ServedPieces[piece];

        await Context.RespondAsync("Yo you pressed a button twin", ephemeral: true);
    }
}
