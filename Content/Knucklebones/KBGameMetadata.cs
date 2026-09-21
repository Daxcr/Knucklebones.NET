using Discord;

namespace CotLMinigames.Knucklebones;

public class KBGameMetadata : GameMetadata
{
    public Table InitiatorTable { get; set; } = new();
    public Table InitiatorTableDiff { get; set; } = new();
    public Table OpponentTable { get; set; } = new();
    public Table OpponentTableDiff { get; set; } = new();
    public byte CurrentDice = 1;

    public struct Table
    {
        public Table() { }
        public List<byte> Left { get; set; } = [0, 0, 0];
        public List<byte> Middle { get; set; } = [0, 0, 0];
        public List<byte> Right { get; set; } = [0, 0, 0];

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


    public const string HorizontalTableSplit = "  ";

    public string BuildTable(Table table, Table diff, bool active, bool invert = false, bool small = false)
    {
        string row1;
        string row2;
        string row3;
        if (invert)
        {
            row1 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 0, active, small);
            row2 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 1, active, small);
            row3 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 2, active, small);
        }
        else
        {
            row1 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 2, active, small);
            row2 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 1, active, small);
            row3 = BuildRow(table.Left, table.Middle, table.Right, diff.Left, diff.Middle, diff.Right, 0, active, small);
        }

        return row1 + "\n" + row2 + "\n" + row3 + "\n";
    }
    private string BuildRow(List<byte> col1, List<byte> col2, List<byte> col3, List<byte> dif1, List<byte> dif2, List<byte> dif3, byte index, bool active, bool small)
    {
        string item1 = BuildSingle(col1, dif1, index, active);
        string item2 = BuildSingle(col2, dif2, index, active);
        string item3 = BuildSingle(col3, dif3, index, active);
        
        if (small)
            return $"{item1}{item2}{item3}";
        else
            return $"# {item1}{HorizontalTableSplit}{item2}{HorizontalTableSplit}{item3}";
    }
    private string BuildSingle(List<byte> col, List<byte> dif, byte index, bool active)
    {
        byte number = col[index];
        bool useDiff = false;

        if (number == 0)
        {
            if (dif[index] == 0)
                return active ? BotClient.Emojis.EmptyActive : BotClient.Emojis.Empty;

            useDiff = true;
            number = dif[index];
        }

        int amount = (useDiff ? dif : col).Count(n => n == number);
        if (amount > 3)
            amount = 3;

        if (useDiff)
            return BotClient.Emojis.GetDice(number, amount, DiceVariant.Delete);
        if (dif[index] == 0)
            return BotClient.Emojis.GetDice(number, amount, DiceVariant.New);

        return BotClient.Emojis.GetDice(number, amount, DiceVariant.Normal);
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