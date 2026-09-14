namespace Knucklebones.DB;

public class ServerSettings
{
    public ulong ServerID { get; set; }
    public GameMetadata.GameState State { get; set; } = GameMetadata.GameState.Thread;
    public bool PingOpponents { get; set; } = false;
    public bool ChannelLock { get; set; } = false;
    public ulong ChannelLockID { get; set; }
}