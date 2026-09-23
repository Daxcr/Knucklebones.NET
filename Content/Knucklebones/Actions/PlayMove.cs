using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task PlayMove(string[] ButtonData, SocketMessageComponent component, bool bot = false)
    {
        string gameID = ButtonData[2];
        string turn = ButtonData[3];

        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
            
        if (meta == null)
        {
            await component.RespondAsync("Something went wrong", ephemeral: true);
            return;
        }

        if (component.User.Id != meta.InitiatorID && component.User.Id != meta.OpponentID && !bot)
        {
            await component.RespondAsync("Not your game", ephemeral: true);
            return;
        }

        if (ButtonData[1] == "forfeit")
        {
            await component.DeferAsync();

            meta.ForfeitSource = component;

            MessageComponent forfeitComponent = new ComponentBuilder()
                .WithButton("Yes, I forfeit", $"forfeitkb/{gameID}", ButtonStyle.Danger)
                .Build();
            await component.FollowupAsync("Forfeit game? The other player will be declared the winner.", ephemeral: true, components: forfeitComponent);
            return;
        }

        if (
            ((component.User.Id != meta.InitiatorID && meta.InitiatorTurn) ||
            (component.User.Id == meta.InitiatorID && !meta.InitiatorTurn) ||
            int.Parse(turn) != meta.Turn || meta.Busy) && !bot
        )
        {
            await component.RespondAsync("Not your turn", ephemeral: true);
            return;
        }

        meta.Busy = true;

        try
        {
            if (!bot)
                await component.DeferAsync();

            string pressedbutton = ButtonData[1];

            RecalculateTables(meta, pressedbutton);

            if (meta.InitiatorTable.IsFull() || meta.OpponentTable.IsFull())
            {
                await EndGame(meta, component);
                return;
            }

            await AdvanceLastMessage(meta, component);

            meta.Turn += 1;
            meta.InitiatorTurn = !meta.InitiatorTurn;
            meta.Busy = false;
        } catch
        {
            meta.Busy = false;
        }

        if (!bot && meta.OpponentID == BotClient.Client.CurrentUser.Id)
        {
            await Task.Delay(1000);
            string response = await CalculateBotResponse(meta);
            await PlayMove(["play", response, gameID, turn + 1], component, true);
        }
    }

    private static async Task<string> CalculateBotResponse(KBGameMetadata meta)
    {
        byte dice = meta.CurrentDice;
        KBGameMetadata.Table botTable = meta.OpponentTable;
        KBGameMetadata.Table playerTable = meta.InitiatorTable;
        
        Dictionary<byte, string> columns = new()
        {
            { 0, "left" },
            { 1, "middle" },
            { 2, "right" },
        };

        string idealColumn;

        // if (botTable.HowManyEmptySpaces() > 5 && playerTable.HowManyEmptySpaces() > 6) // Spread at the start of the game 
        //     idealColumn = columns[AISpread(meta)];
        // else
        idealColumn = columns[AIAggro(meta)];

        return idealColumn;
    }

    private static byte AISpread(KBGameMetadata meta)
    {
        byte dice = meta.CurrentDice;
        KBGameMetadata.Table botTable = meta.OpponentTable;
        KBGameMetadata.Table playerTable = meta.InitiatorTable;

        List<List<byte>> columns = new() { botTable.Left, botTable.Middle, botTable.Right };
        List<List<byte>> playerColumns = new() { playerTable.Left, playerTable.Middle, playerTable.Right };
        List<int> weights = new() { 0, 0, 0 };

        bool bigNumber = dice > 3;

        foreach (List<byte> column in columns)
        {
            int index = columns.IndexOf(column);
            List<byte> playerColumn = playerColumns[index];
            float playerColumnAverage = (playerColumn[0] + playerColumn[1] + playerColumn[2]) / 3f;

            if (!column.Contains(0))
                weights[index] = -100000;

            weights[index] += new Random().Next(0, 10);
            weights[index] += column.Count(number => number == 0) * 30;

            if (bigNumber && playerColumnAverage < 3.5)
                weights[index] += 20;
            else if (!bigNumber && playerColumnAverage < 3.5)
                weights[index] -= 8;

            if (bigNumber && playerColumn.Count(number => number == 0) == 3)
                weights[index] -= 20;
            else if (bigNumber && playerColumn.Count(number => number == 0) == 1)
                weights[index] += 20;

            weights[index] += playerColumn.Count(number => number == dice) * 10000;
        }

        return (byte)weights.IndexOf(weights.Max());
    }

    private static byte AIAggro(KBGameMetadata meta)
    {
        Random rnd = new Random();

        int conservativeWeight = rnd.Next(0, 10);
        int aggroWeight = rnd.Next(0, 10);
        
        byte dice = meta.CurrentDice;
        KBGameMetadata.Table botTable = meta.OpponentTable;
        KBGameMetadata.Table playerTable = meta.InitiatorTable;

        List<List<byte>> columns = new() { botTable.Left, botTable.Middle, botTable.Right };
        List<List<byte>> playerColumns = new() { playerTable.Left, playerTable.Middle, playerTable.Right };
        List<int> weights = new() { 0, 0, 0 };

        foreach (List<byte> column in columns)
        {
            int index = columns.IndexOf(column);
            List<byte> playerColumn = playerColumns[index];

            if (!column.Contains(0))
            {
                weights[index] = -100000;
                continue;
            }

            List<int> diffs = new();

            for (byte i = 1; i < 7; i++)
            {
                List<byte> tempColumn = column.ToList();
                List<byte> tempPlayerColumn = playerColumn.ToList();

                int prePlayerPoints = KBGameMetadata.BuildColumnPoints(tempPlayerColumn);
                int prePoints = KBGameMetadata.BuildColumnPoints(tempColumn);

                for (int j = 0; j < 3; j++)
                {
                    if (tempPlayerColumn[j] == i)
                        tempPlayerColumn[j] = 0;
                }
                tempColumn[tempColumn.IndexOf(0)] = i;

                int postPlayerPoints = KBGameMetadata.BuildColumnPoints(tempPlayerColumn);
                int postPoints = KBGameMetadata.BuildColumnPoints(tempColumn);

                diffs.Add((postPoints - prePoints) + (prePlayerPoints - postPlayerPoints));
            }

            List<byte> tempPlayerColumnB = playerColumn.ToList();
            for (int i = 0; i < 3; i++)
            {
                if (tempPlayerColumnB[i] == dice)
                    tempPlayerColumnB[i] = 0;
            }

            List<byte> tempColumnB = column.ToList();
            tempColumnB[column.IndexOf(0)] = dice;

            int cWeight = (diffs[dice - 1] - diffs.Max()) * conservativeWeight * column.Count(number => number == 0);
            int aWeight;
            if (dice < 4)
                aWeight = KBGameMetadata.BuildColumnPoints(tempPlayerColumnB) * aggroWeight;
            else
                aWeight = KBGameMetadata.BuildColumnPoints(tempPlayerColumnB) * aggroWeight;

            weights[index] = cWeight - aggroWeight;
            weights[index] += rnd.Next(-2, 2);
        }

        return (byte)weights.IndexOf(weights.Max());
    }
}