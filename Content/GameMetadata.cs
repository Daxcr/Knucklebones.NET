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
    public required bool ShortGame = false;
    public bool InitiatorTurn;
    public required ulong InitiatorID;
    public required ulong OpponentID;
    public required ulong? InitiatedChannelID;
    public IMessageChannel? Channel;
    public Table InitiatorTable = new();
    public Table InitiatorTableDiff = new();
    public Table OpponentTable = new();
    public Table OpponentTableDiff = new();
    public required GameState State;
    public int Bet = 0;
    public int Turn = 1;
    public byte CurrentDice = 1;

    public struct Table
    {
        public Table() { }
        public List<byte> Left = [0, 0, 0];
        public List<byte> Middle = [0, 0, 0];
        public List<byte> Right = [0, 0, 0];

        public Table Clone()
        {
            return new Table
            {
                Left = new List<byte>(Left),
                Middle = new List<byte>(Middle),
                Right = new List<byte>(Right)
            };
        }

        public bool IsFull() => !(Left.Contains(0) || Middle.Contains(0) || Right.Contains(0));

        public void Add(string col, byte value, Table toClean)
        {
            switch (col)
            {
                case "left":
                    int index = Left.IndexOf(0);
                    if (index != -1)
                    {
                        Left[index] = value;
                        ReplaceValue(toClean.Left, value, 0);
                    }
                    break;

                case "middle":
                    index = Middle.IndexOf(0);
                    if (index != -1)
                    {
                        Middle[index] = value;
                        ReplaceValue(toClean.Middle, value, 0);
                    }
                    break;

                case "right":
                    index = Right.IndexOf(0);
                    if (index != -1)
                    {
                        Right[index] = value;
                        ReplaceValue(toClean.Right, value, 0);
                    }
                    break;
            }
        }

        private void ReplaceValue(List<byte> col, byte oldValue, byte newValue)
        {
            for (int i = 0; i < col.Count; i++)
            {
                if (col[i] == oldValue)
                    col[i] = newValue;
            }
        }
    }

    public enum GameState
    {
        Thread,
        Threadless
    }

    public static Dictionary<string, string> DiceEmojis = new()
    {
        { "empty", ":black_large_square:" },

        { "dice1_single", "<:dice1_single:1549203689435168808>" },
        { "dice1_double", "<:dice1_double:1549203687090421790>" },
        { "dice1_triple", "<:dice1_triple:1549203684448276480>" },
        { "dice1_single_new", "<:dice1_single_new:1549203682523091037>" },
        { "dice1_double_new", "<:dice1_double_new:1549203680161439866>" },
        { "dice1_triple_new", "<:dice1_triple_new:1549203678303486053>" },
        { "dice1_single_delete", "<:dice1_single_delete:1549203641204871248>" },
        { "dice1_double_delete", "<:dice1_double_delete:1549203638667182180>" },
        { "dice1_triple_delete", "<:dice1_triple_delete:1549203636373159976>" },

        { "dice2_single", "<:dice2_single:1549203761124085840>" },
        { "dice2_double", "<:dice2_double:1549203758502912010>" },
        { "dice2_triple", "<:dice2_triple:1549203756519006218>" },
        { "dice2_single_new", "<:dice2_single_new:1549203754107277372>" },
        { "dice2_double_new", "<:dice2_double_new:1549203752144343142>" },
        { "dice2_triple_new", "<:dice2_triple_new:1549203749963046912>" },
        { "dice2_single_delete", "<:dice2_single_delete:1549203676089032704>" },
        { "dice2_double_delete", "<:dice2_double_delete:1549203673551478826>" },
        { "dice2_triple_delete", "<:dice2_triple_delete:1549203672020287530>" },

        { "dice3_single", "<:dice3_single:1549203748021338242>" },
        { "dice3_double", "<:dice3_double:1549203745827594271>" },
        { "dice3_triple", "<:dice3_triple:1549203743868850337>" },
        { "dice3_single_new", "<:dice3_single_new:1549203741763440680>" },
        { "dice3_double_new", "<:dice3_double_new:1549203739167170621>" },
        { "dice3_triple_new", "<:dice3_triple_new:1549203737006837781>" },
        { "dice3_single_delete", "<:dice3_single_delete:1549203669562564608>" },
        { "dice3_double_delete", "<:dice3_double_delete:1549203667394101368>" },
        { "dice3_triple_delete", "<:dice3_triple_delete:1549203665288691792>" },

        { "dice4_single", "<:dice4_single:1549203733894922430>" },
        { "dice4_double", "<:dice4_double:1549203731604705300>" },
        { "dice4_triple", "<:dice4_triple:1549203729469673513>" },
        { "dice4_single_new", "<:dice4_single_new:1549203726361956494>" },
        { "dice4_double_new", "<:dice4_double_new:1549203724399022101>" },
        { "dice4_triple_new", "<:dice4_triple_new:1549203722482221147>" },
        { "dice4_single_delete", "<:dice4_single_delete:1549203663141077123>" },
        { "dice4_double_delete", "<:dice4_double_delete:1549203659370536980>" },
        { "dice4_triple_delete", "<:dice4_triple_delete:1549203657382432848>" },

        { "dice5_single", "<:dice5_single:1549203720401854554>" },
        { "dice5_double", "<:dice5_double:1549203718455558294>" },
        { "dice5_triple", "<:dice5_triple:1549203715192389632>" },
        { "dice5_single_new", "<:dice5_single_new:1549203712969285662>" },
        { "dice5_double_new", "<:dice5_double_new:1549203710629118062>" },
        { "dice5_triple_new", "<:dice5_triple_new:1549203708452012162>" },
        { "dice5_single_delete", "<:dice5_single_delete:1549203655104663662>" },
        { "dice5_double_delete", "<:dice5_double_delete:1549203652852318238>" },
        { "dice5_triple_delete", "<:dice5_triple_delete:1549203650356977725>" },

        { "dice6_single", "<:dice6_single:1549203705914597426>" },
        { "dice6_double", "<:dice6_double:1549203703913910292>" },
        { "dice6_triple", "<:dice6_triple:1549203699316818060>" },
        { "dice6_single_new", "<:dice6_single_new:1549203697269997589>" },
        { "dice6_double_new", "<:dice6_double_new:1549203695357395024>" },
        { "dice6_triple_new", "<:dice6_triple_new:1549203692815913091>" },
        { "dice6_single_delete", "<:dice6_single_delete:1549203648540835890>" },
        { "dice6_double_delete", "<:dice6_double_delete:1549203646330183811>" },
        { "dice6_triple_delete", "<:dice6_triple_delete:1549203643943751801>" },
    };
    public const string HorizontalTableSplit = "  ";

    public string BuildTable(Table table, Table diff, bool invert = false)
    {
        string row1;
        string row2;
        string row3;
        if (invert)
        {
            row1 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 0);
            row2 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 1);
            row3 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 2);
        }
        else
        {
            row1 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 2);
            row2 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 1);
            row3 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 0);
        }

        return row1 + "\n" + row2 + "\n" + row3 + "\n";
    }
    private string BuildRow(List<byte> col1, List<byte> col2, List<byte> col3, List<byte> dif1, List<byte> dif2, List<byte> dif3, byte index)
    {
        string item1 = BuildSingle(col1, dif1, index);
        string item2 = BuildSingle(col2, dif2, index);
        string item3 = BuildSingle(col3, dif3, index);

        return $"# {item1}{HorizontalTableSplit}{item2}{HorizontalTableSplit}{item3}";
    }
    private string BuildSingle(List<byte> col, List<byte> dif, byte index)
    {
        bool useDiff = false;
        byte number = col[index];
        if (number == 0)
        {
            if (dif[index] == 0)
                return DiceEmojis["empty"];

            useDiff = true;
            number = dif[index];
        }

        string emoji = string.Empty;

        int amount = col.Count(n => n == number);
        if (useDiff)
            amount = dif.Count(n => n == number);

        switch (amount)
        {
            case 1:
                emoji = $"dice{number}_single";
                break;

            case 2:
                emoji = $"dice{number}_double";
                break;

            case 3:
                emoji = $"dice{number}_triple";
                break;

            default:
                emoji = $"dice{number}_triple";
                Console.WriteLine("List larger than 3 items!!!");
                break;
        }

        if (useDiff)
            return DiceEmojis[$"{emoji}_delete"];

        if (dif[index] == 0 && number != 0)
            return DiceEmojis[$"{emoji}_new"];

        return DiceEmojis[emoji];
    }

    public int BuildPoints(bool initiator)
    {
        Table table = initiator ? InitiatorTable : OpponentTable;
        return BuildColumnPoints(table.Left) + BuildColumnPoints(table.Middle) + BuildColumnPoints(table.Right);
    }

    private int BuildColumnPoints(List<byte> col)
    {
        List<byte> checkedValues = new();
        int total = 0;
        foreach (byte number in col)
        {
            if (checkedValues.Contains(number))
                continue;

            checkedValues.Add(number);
            switch (col.Count(n => n == number))
            {
                case 1:
                    total += number;
                    break;

                case 2:
                    total += number * 4;
                    break;

                case 3:
                    total += number * 9;
                    break;
            }
        }
        return total;
    }
}