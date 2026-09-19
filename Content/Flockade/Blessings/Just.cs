namespace CotLMinigames.Flockade.Blessings;

public class Just : IBlessing
{
    public FLGameMetadata.FlockPiece Parent { get; set; }
    public byte Position { get; set; }
    void IBlessing.OnAttack(FLGameMetadata.FlockPiece opponent, FLGameMetadata.Table table, FLGameMetadata.Table otherTable)
    {
        if (
            !opponent.Blessings.Any(bless => bless.GetType() == GetType()) &&
            opponent.PieceType == Parent.PieceType
        ) { }
    }
}