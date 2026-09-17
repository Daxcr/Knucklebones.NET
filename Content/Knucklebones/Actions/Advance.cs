using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AdvanceLastMessage(KBGameMetadata meta, SocketMessageComponent component)
    {
        Embed initiatorembed = await BuildPlayerEmbed(meta, true);
        Embed opponentembed = await BuildPlayerEmbed(meta, false);          
        Embed diceEmbed = BuildDiceEmbed(meta);

        MessageComponent gameActions = BuildGameActions(meta, meta.InitiatorTurn);

        if (meta.Guild == null)
            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                msg.Components = gameActions;
            });
        else
            await component.Message.ModifyAsync(msg =>
            {
                msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                msg.Components = gameActions;
            });
    }

    public static async Task AdvanceAsNewMessage(KBGameMetadata meta, SocketMessageComponent component)
    {
        Embed initiatorembed = await BuildPlayerEmbed(meta, true);
        Embed opponentembed = await BuildPlayerEmbed(meta, false);          

        meta.CurrentDice = (byte)new Random().Next(1,7);
        Embed diceEmbed = BuildDiceEmbed(meta);

        MessageComponent gameActions = BuildGameActions(meta, meta.InitiatorTurn);

        await component.Message.ReplyAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
    }
}