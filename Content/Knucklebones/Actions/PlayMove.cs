using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task PlayMove(string[] ButtonData, SocketMessageComponent component)
    {
        string gameID = ButtonData[2];
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta == null)
            return;

        if (component.User.Id != meta.InitiatorID && component.User.Id != meta.OpponentID)
        {
            await component.RespondAsync("Not your game", ephemeral: true);
            return;
        }

        if ((component.User.Id != meta.InitiatorID && meta.InitiatorTurn) || (component.User.Id == meta.InitiatorID && !meta.InitiatorTurn))
        {
            await component.RespondAsync("Not your turn", ephemeral: true);
            return;
        }

        await component.DeferAsync();
        string pressedbutton = ButtonData[1];

        RecalculateTables(meta, pressedbutton);

        if (meta.InitiatorTable.IsFull() || meta.OpponentTable.IsFull())
        {
            EndGame(meta, component);
            return;
        }

        meta.CurrentDice = (byte)new Random().Next(1,7);
        meta.Turn += 1;
        meta.InitiatorTurn = !meta.InitiatorTurn;

        await AdvanceLastMessage(meta, component);
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