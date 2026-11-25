using Discord;

namespace CotLMinigames.Flockade;

public static partial class Actions
{
    public const string HorizontalTableSplit = "  ";
    public static async Task<Embed> BuildBoardEmbed(FLGameMetadata meta, bool initiatorTurn)
    {
        string description = string.Empty;
        int rows = meta.LargeTables ? 3 : 2;

        for (int i = 0; i < rows; i++)
        {
            description += "# ";
            for (int j = 0; j < 2; j++)
            {
                string slot = initiatorTurn ? BotClient.Emojis.EmptyActive : BotClient.Emojis.Empty;
                description += $"{slot}{HorizontalTableSplit}";
            }
            description += HorizontalTableSplit;
            for (int j = 0; j < 2; j++)
            {
                string slot = !initiatorTurn ? BotClient.Emojis.EmptyActive : BotClient.Emojis.Empty;
                description += $"{slot}{HorizontalTableSplit}";
            }
            description += "\n";
        }

        description += $"<@{(initiatorTurn ? meta.InitiatorID : meta.OpponentID)}>'s turn!";

        return new EmbedBuilder()
            .WithDescription(description)
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiatorTurn ? meta.InitiatorID : meta.OpponentID))
            .Build();
    }

    public static async Task<Embed> BuildPieceEmbed(FLGameMetadata meta)
    {
        FLGameMetadata.FlockPiece piece = FLGameMetadata.FlockPiece.CreateRandomPiece();
        meta.ServedPieces.Add(piece);

        string description = string.Empty;

        if (piece.Blessings.Count > 0)
            description += $"**Blessing:** {FLGameMetadata.BlessingRegistry[piece.Blessings[0].GetType()]}{piece.Blessings[0].GetType().Name}";
        else
            description += $"**Blessing:** None";

        return new EmbedBuilder()
            .WithTitle(piece.PieceType.ToString())
            .WithDescription(description)
            .Build();
    }

    public static async Task<MessageComponent> BuildPieceButtons(FLGameMetadata meta) =>
        new ComponentBuilder()
            .WithButton("Piece 1", $"choosepiecefl/{meta.ID}/0", ButtonStyle.Primary)
            .WithButton("Piece 2", $"choosepiecefl/{meta.ID}/1", ButtonStyle.Primary)
            .WithButton("Piece 3", $"choosepiecefl/{meta.ID}/2", ButtonStyle.Primary)
            .Build();
}
