namespace CotLMinigames.Flockade.Blessings;

public interface IBlessing
{
    public FLGameMetadata.FlockPiece Parent { get; set; }
    public byte Position { get; set; }
    public virtual void OnPlaced(FLGameMetadata.Table table, FLGameMetadata.Table otherTable) { }
    public virtual void Action(FLGameMetadata.Table table, FLGameMetadata.Table otherTable) { }
    public virtual void OnAttack(FLGameMetadata.FlockPiece opponent, FLGameMetadata.Table table, FLGameMetadata.Table otherTable) { }
}

public interface IShepherdBlessing { }