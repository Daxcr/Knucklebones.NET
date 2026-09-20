namespace CotLMinigames.Flockade.Blessings;

public class Fallen : IBlessing
{
    public FLGameMetadata.FlockPiece? Parent { get; set; }
    public string? Emoji { get; set; }
    public byte Position { get; set; }
    void IBlessing.Action(FLGameMetadata.Table table, FLGameMetadata.Table otherTable)
    {
        byte swapper = (byte)((Position + 2) % 4);
        (table.Data[Position], table.Data[swapper]) = (table.Data[swapper], table.Data[Position]);

        table.Data[Position]?.ChangedPosition(swapper);
        table.Data[swapper]?.ChangedPosition(Position);
    }
}