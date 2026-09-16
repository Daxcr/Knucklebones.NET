using Discord;
using Discord.WebSocket;

namespace CotLMinigames;

public class GameMetadata
{
    public static int ChallengeExpiry = 2 * 60;
    public static int ShortGameExpiry = 12 * 60;
    public GameMetadata()
    {
        BotClient.Games.Add(this);
        ID = Guid.NewGuid()
            .ToString("N")
            .Substring(0, 21);
    }
    
    public string ID { get; set; }
    public bool GameStarted = false;
    public bool GameDeclined = false;
    public bool InitiatorTurn;
    public required ulong InitiatorID;
    public required ulong OpponentID;
    public required ulong? InitiatedChannelID;
    public SocketGuild? Guild;
    public IMessageChannel? Channel;
    public int Bet = 0;
    public int Turn = 1;
    public int GameExpiry = 30 * 60;
}