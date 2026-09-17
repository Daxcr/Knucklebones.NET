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

    public struct FlockPiece
    {
        public List<Class> Classes = new();
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
    } 
}