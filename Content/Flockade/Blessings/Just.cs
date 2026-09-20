namespace CotLMinigames.Flockade.Blessings;

public class Just : IBlessing
{
    public FLGameMetadata.FlockPiece? Parent { get; set; }
    public string? Emoji { get; set; }
    public byte Position { get; set; }
    bool IBlessing.AttackOutcome(FLGameMetadata.FlockPiece? opponent, FLGameMetadata.Table table, FLGameMetadata.Table otherTable)
    {
        if (opponent?.PieceType == Parent?.PieceType)
            return true;
        else
            return (this as IBlessing).Default(opponent);
    }
}