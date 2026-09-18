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
            return;

        if (component.User.Id != meta.InitiatorID && component.User.Id != meta.OpponentID && !bot)
        {
            await component.RespondAsync("Not your game", ephemeral: true);
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
                EndGame(meta, component);
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
            await Task.Delay(2000);
            string response = await CalculateBotResponse(meta);
            await PlayMove(["play", response, gameID, turn + 1], component, true);
        }
    }

    private static async Task<string> CalculateBotResponse(KBGameMetadata meta)
    {
        byte dice = meta.CurrentDice;
        KBGameMetadata.Table botTable = meta.OpponentTable;
        KBGameMetadata.Table playerTable = meta.InitiatorTable;

        int leftWeight = CalculateColumnWeight(botTable.Left, playerTable.Left, dice);
        int middleWeight = CalculateColumnWeight(botTable.Middle, playerTable.Middle, dice);
        int rightWeight = CalculateColumnWeight(botTable.Right, playerTable.Right, dice);

        return ChooseColumn(leftWeight, middleWeight, rightWeight);
    }

    private static int CalculateColumnWeight(List<byte> botColumn, List<byte> playerColumn, byte dice)
    {
        int weight = new Random().Next(0, 5);
        if (!botColumn.Contains(0))
            return int.MinValue;
        
        if (!playerColumn.Contains(0) && dice > 3)
            weight += 20;

        if (playerColumn.Count(item => item != 0) == 2 && dice > 4)
            weight += 50;

        weight += 30 * playerColumn.Count(item => item == dice);

        if (botColumn.Contains(dice))
            weight += 25;

        return weight;
    }

    private static string ChooseColumn(int leftWeight, int middleWeight, int rightWeight)
    {
        List<(string Name, int Weight)> valid = new();

        if (leftWeight >= 0) valid.Add(("left", leftWeight));
        if (middleWeight >= 0) valid.Add(("middle", middleWeight));
        if (rightWeight >= 0) valid.Add(("right", rightWeight));

        if (valid.Count == 1)
            return valid[0].Name;

        List<(string Name, int Weight)> eligible;

        if (!valid.Any(item => item.Weight >= 50))
            eligible = valid
                .OrderBy(_ => new Random().Next())
                .Take(2)
                .ToList();
        else
            eligible = valid;

        return eligible.OrderByDescending(item => item.Weight).First().Name;
    }

    public static async Task DisableLastMessage(SocketMessageComponent component, string pressedbutton)
    {
        ButtonStyle leftStyle = pressedbutton == "left" ? ButtonStyle.Primary : ButtonStyle.Secondary;
        ButtonStyle middleStyle = pressedbutton == "middle" ? ButtonStyle.Primary : ButtonStyle.Secondary;
        ButtonStyle rightStyle = pressedbutton == "right" ? ButtonStyle.Primary : ButtonStyle.Secondary;

        MessageComponent disabledComponents = new ComponentBuilder()
            .WithButton("Left", $"play/left/disabled", leftStyle, disabled: true)
            .WithButton("Middle", $"play/middle/disabled", middleStyle, disabled: true)
            .WithButton("Right", $"play/right/disabled", rightStyle, disabled: true)
            .Build();

        await component.Message.ModifyAsync(msg => { msg.Components = disabledComponents; });
    }
}