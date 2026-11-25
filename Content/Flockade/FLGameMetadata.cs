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
    public bool LargeTables = false;

    public List<FlockPiece> ServedPieces = new();

    public struct Table
    {
        public Table() { }
        public List<FlockPiece?> Data = [null, null, null, null, null, null];

        public bool IsFull(bool largeTables = false)
        {
            if (largeTables)
                return !Data.Contains(null);
            else
                return !Data.Take(4).Any(item => item == null);
        }
    }

    public class FlockPiece
    {
        public Class PieceType = new();
        public byte Position = new();
        public List<IBlessing> Blessings = new();
        public FlockPiece() { }

        private static IBlessing CreateRandomBlessing(Random rand)
        {
            if (BlessingRegistry.Count == 0)
                RegisterBlessings();

            Type[] keys = BlessingRegistry.Keys.ToArray();
            Type type = keys[rand.Next(keys.Length)];
            return (IBlessing)Activator.CreateInstance(type)!;
        }

        public static FlockPiece CreateRandomPiece()
        {
            Random rand = new();
            List<IBlessing> blessings = new();

            if (rand.Next(0, 2) < 1)
                blessings.Add(CreateRandomBlessing(rand));

            return new()
            {
                PieceType = (Class)rand.Next(0, 4),
                Blessings = blessings
            };
        }

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

    public static SortedDictionary<Type, string?> BlessingRegistry =
        new(Comparer<Type>.Create((a, b) => string.CompareOrdinal(a.FullName, b.FullName)));

    public static void RegisterBlessings()
    {
        AddBlessing<Just>(BotClient.Emojis.BlessJust);
        AddBlessing<Fallen>(BotClient.Emojis.BlessFallen);
    }

    public static void AddBlessing<Blessing>(string Emoji) where Blessing : IBlessing =>
        BlessingRegistry[typeof(Blessing)] = Emoji;
}
