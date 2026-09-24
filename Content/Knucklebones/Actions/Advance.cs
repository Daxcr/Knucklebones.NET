using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AdvanceLastMessage(UserContext Context, KBGameMetadata meta)
    {
        Embed initiatorembed = await BuildPlayerEmbed(meta, true, !meta.InitiatorTurn);
        Embed opponentembed = await BuildPlayerEmbed(meta, false, !meta.InitiatorTurn);   

        meta.CurrentDice = (byte)new Random().Next(1,7);       
        Embed diceEmbed = BuildDiceEmbed(meta);

        MessageComponent gameActions = BuildGameActions(meta, !meta.InitiatorTurn);

        if (meta.Guild == null)
            await Context.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                msg.Components = gameActions;
            });
        else
            await Context.ModifyAsync(msg =>
            {
                msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                msg.Components = gameActions;
            });
    }
}