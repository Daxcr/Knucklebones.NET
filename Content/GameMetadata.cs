using Discord;
using Discord.WebSocket;

namespace Knucklebones;

public class GameMetadata
{
    public GameMetadata()
    {
        GameModule.Games.Add(this);
        ID = Guid.NewGuid()
            .ToString("N")
            .Substring(0, 21);
    }
    public string ID;
    public bool GameStarted = false;
    public bool GameDeclined = false;
    public bool InitiatorTurn;
    public required ulong InitiatorID;
    public required ulong OpponentID;
    public required ulong InitiatedChannelID;
    public SocketTextChannel? Channel;
    public Table InitiatorTable = new();
    public Table OpponentTable = new();
    public required GameState State;
    public int Bet = 0;
    public int Turn = 1;
    public byte CurrentDice = 1;

    public class Table
    {
        public List<byte> Left = [0, 0, 0];
        public List<byte> Middle = [0, 0, 0];
        public List<byte> Right = [0, 0, 0];


        public void Add(string col, byte value)
        {
            switch (col)
            {
                case "left":
                    int index = Left.IndexOf(0);
                    if (index != -1)
                        Left[index] = value;
                    break;

                case "middle":
                    index = Middle.IndexOf(0);
                    if (index != -1)
                        Middle[index] = value;
                    break;

                case "right":
                    index = Right.IndexOf(0);
                    if (index != -1)
                        Right[index] = value;
                    break;
            }
        }
    }

    public enum GameState
    {
        Thread,
        Threadless
    }

    public static Dictionary<byte, string> DiceEmojis = new()
    {
        { 0, ":black_large_square:" },
        { 1, ":one:" },
        { 2, ":two:" },
        { 3, ":three:" },
        { 4, ":four:" },
        { 5, ":five:" },
        { 6, ":six:" },
    };
    public const string HorizontalTableSplit = "  ";

    public string BuildTable(Table table, bool invert = false)
    {
        string row1;
        string row2;
        string row3;
        if (invert)
        {
            row1 = BuildRow(table.Left[0], table.Middle[0], table.Right[0]);
            row2 = BuildRow(table.Left[1], table.Middle[1], table.Right[1]);
            row3 = BuildRow(table.Left[2], table.Middle[2], table.Right[2]);
        }
        else
        {
            row1 = BuildRow(table.Left[2], table.Middle[2], table.Right[2]);
            row2 = BuildRow(table.Left[1], table.Middle[1], table.Right[1]);
            row3 = BuildRow(table.Left[0], table.Middle[0], table.Right[0]);
        }

        return row1 + "\n" + row2 + "\n" + row3 + "\n";
    }
    internal string BuildRow(byte col1, byte col2, byte col3) =>
        $"# {DiceEmojis[col1]}{HorizontalTableSplit}{DiceEmojis[col2]}{HorizontalTableSplit}{DiceEmojis[col3]}{HorizontalTableSplit}";
}