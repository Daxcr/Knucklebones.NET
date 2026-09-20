namespace CotLMinigames.Flockade.Blessings;

public interface IBlessing
{
    public FLGameMetadata.FlockPiece? Parent { get; set; }
    public byte Position { get; set; }
    public string? Emoji { get; set; }
    public virtual void OnPlaced(FLGameMetadata.Table table, FLGameMetadata.Table otherTable) { }
    public virtual void Action(FLGameMetadata.Table table, FLGameMetadata.Table otherTable) { }
    public virtual bool AttackOutcome(FLGameMetadata.FlockPiece opponent, FLGameMetadata.Table table, FLGameMetadata.Table otherTable) => false;

    public bool Default(FLGameMetadata.FlockPiece? opponent)
    {
        if (opponent == null)
            return true;

        switch (Parent?.PieceType)
        {
            case FLGameMetadata.FlockPiece.Class.Scribe:
                return opponent.PieceType == FLGameMetadata.FlockPiece.Class.Shield;

            case FLGameMetadata.FlockPiece.Class.Shield:
                return opponent.PieceType == FLGameMetadata.FlockPiece.Class.Sword;

            case FLGameMetadata.FlockPiece.Class.Sword:
                return opponent.PieceType == FLGameMetadata.FlockPiece.Class.Scribe;
        }
        return false;
    }
}

public interface IShepherdBlessing { }