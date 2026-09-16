namespace CotLMinigames.DB;

public class ServerSettings
{
    public ulong ServerID { get; set; }
    public bool PingOpponents { get; set; } = false;
    public bool ChannelLock { get; set; } = false;
    public ulong ChannelLockID { get; set; }
}