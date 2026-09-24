using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task PlayMove(string[] ButtonData, SocketMessageComponent component, bool bot = false)
    {
        string pressedbutton = ButtonData[1];
        string gameID = ButtonData[2];
        string turn = ButtonData[3];

        UserContext ctx = new(component);
        await PlayMove(ctx, pressedbutton, gameID, turn, bot);
    }

    public static async Task PlayMove(UserContext Context, string pressedbutton, string gameID, string turn, bool bot)
    {
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
            
        if (meta == null)
        {
            await Context.RespondAsync("Something went wrong", ephemeral: true);
            return;
        }

        if (Context.User?.Id != meta.InitiatorID && Context.User?.Id != meta.OpponentID && !bot)
        {
            await Context.RespondAsync("Not your game", ephemeral: true);
            return;
        }

        if (pressedbutton == "forfeit")
        {
            await Context.DeferAsync();

            meta.ForfeitSource = Context;

            MessageComponent forfeitComponent = new ComponentBuilder()
                .WithButton("Yes, I forfeit", $"forfeitkb/{gameID}", ButtonStyle.Danger)
                .Build();
            await Context.FollowupAsync("Forfeit game? The other player will be declared the winner.", ephemeral: true, components: forfeitComponent);
            return;
        }

        if (
            ((Context.User?.Id != meta.InitiatorID && meta.InitiatorTurn) ||
            (Context.User?.Id == meta.InitiatorID && !meta.InitiatorTurn) ||
            int.Parse(turn) != meta.Turn || meta.Busy) && !bot
        )
        {
            await Context.RespondAsync("Not your turn", ephemeral: true);
            return;
        }

        meta.Busy = true;

        try
        {
            if (!bot)
                await Context.DeferAsync();

            RecalculateTables(meta, pressedbutton);

            if (meta.InitiatorTable.IsFull() || meta.OpponentTable.IsFull())
            {
                await EndGame(Context, meta);
                return;
            }

            await AdvanceLastMessage(Context, meta);

            meta.Turn += 1;
            meta.InitiatorTurn = !meta.InitiatorTurn;
            meta.Busy = false;
        } catch
        {
            meta.Busy = false;
        }

        if (!bot && meta.OpponentID == BotClient.Client.CurrentUser.Id)
        {
            await Task.Delay(1200);
            string response = await CalculateBotResponse(meta);
            await PlayMove(Context, response, gameID, turn + 1, true);
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

        return columns[AIAggro(meta)];
    }

    private static byte AIAggro(KBGameMetadata meta)
    {
        byte dice = meta.CurrentDice;
        KBGameMetadata.Table botTable = meta.OpponentTable;
        KBGameMetadata.Table playerTable = meta.InitiatorTable;

        Random rnd = new();

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

            int cWeight = (diffs[dice - 1] - diffs.Max()) * column.Count(number => number == 0);

            weights[index] = cWeight;
            weights[index] += rnd.Next(-2, 2);
        }

        return (byte)weights.IndexOf(weights.Max());
    }
}