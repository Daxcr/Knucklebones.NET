using CotLMinigames.Flockade.Blessings;
using Discord;

namespace CotLMinigames.Flockade;

public class FLGameMetadata : GameMetadata
{
    public Table InitiatorTable = new();
    public Table InitiatorTableDiff = new();
    public Table OpponentTable = new();
    public Table OpponentTableDiff = new();
    public byte Round = 1;
    public byte RoundsRequiredToWin = 2;

    public struct Table
    {
        public Table() { }
        public List<FlockPiece?> Data = [null, null, null, null];

        public bool IsFull() => !Data.Contains(null);
    }

    public class FlockPiece
    {
        public Class PieceType = new();
        public byte Position = new();
        public List<IBlessing> Blessings = new();
        public FlockPiece() { }

        public enum Class
        {
            Sword,
            Shield,
            Scribe,
            Shepherd
        }
        public enum Blessing
        {
            None,
            Just,
            Fallen,
            Risen,
            Prize,
            Rotten,
            Scout,
            Mercenary,
            Marked,
            Trickster
        }
        public enum ShepherdBlessing
        {
            Lone,
            Wandering,
            Guardian,
            False,
            Champion,
            Joker,
            RottenPlus
        }

        public void ChangedPosition(byte newPosition)
        {
            Position = newPosition;
            foreach (IBlessing blessing in Blessings)
                Position = newPosition;
        }
    }

    public static Dictionary<Type, string?> BlessingRegistry = new();

    public static void RegisterBlessings()
    {
        AddBlessing<Just>(BotClient.Emojis.BlessJust);
        AddBlessing<Fallen>(BotClient.Emojis.BlessFallen);
    }

    public static void AddBlessing<Blessing>(string Emoji) where Blessing : IBlessing =>
        BlessingRegistry.Add(typeof(Blessing), Emoji);
}