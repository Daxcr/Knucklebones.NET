using System.Text.Json.Serialization;
using CotLMinigames.Flockade;
using CotLMinigames.Knucklebones;
using Discord;
using Discord.WebSocket;

namespace CotLMinigames;

[JsonPolymorphic]
[JsonDerivedType(typeof(KBGameMetadata), "kb")]
[JsonDerivedType(typeof(FLGameMetadata), "fl")]
public class GameMetadata
{
    public static int ChallengeExpiry = 5 * 60;
    public static int ShortGameExpiry = 12 * 60;
    public GameMetadata()
    {
        BotClient.Games.Add(this);
        ID = Guid.NewGuid()
            .ToString("N")
            .Substring(0, 21);
    }
    
    public string ID { get; set; }
    public bool GameStarted { get; set; } = false;
    public bool GameDeclined { get; set; } = false;
    public bool InitiatorTurn { get; set; }
    public bool Busy = false;
    public bool Expired = false;
    public required ulong InitiatorID { get; set; }
    public required ulong OpponentID { get; set; }
    public required ulong? InitiatedChannelID { get; set; }
    [JsonIgnore]
    public SocketGuild? Guild;
    [JsonIgnore]
    public IMessageChannel? Channel;
    public int Bet { get; set; } = 0;
    public int Turn = 1;
    public int GameExpiry = 30 * 60;
    [JsonIgnore]
    public TimestampTag GameExpiryDisplay;
    [JsonIgnore]
    public UserContext? ForfeitSource;
}