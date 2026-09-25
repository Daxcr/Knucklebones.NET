using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task EndGame(UserContext Context, KBGameMetadata meta)
    {
        meta.Expired = true;
        if (!BotClient.Games.Contains(meta))
            return;

        BotClient.Games.Remove(meta);

        int initiatorScore = meta.BuildPoints(true);
        int opponentScore = meta.BuildPoints(false);

        Embed initiatorembed;
        Embed opponentembed;
        Embed? devotionEmbed = null;
        MessageComponent component2;

        if (initiatorScore == opponentScore)
        {
            initiatorembed = await BuildEndPlayerEmbed(meta, true, false);  
            opponentembed = await BuildEndPlayerEmbed(meta, false, false);
            component2 = BuildEndGameActionsTie(meta);
        } else
        {
            devotionEmbed = await CalculateBetsAndDevotion(meta, initiatorScore > opponentScore);

            if (initiatorScore > opponentScore)
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, true);
                opponentembed = await BuildEndPlayerEmbed(meta, false, false);
                component2 = BuildEndGameActions(meta, true);
            } else
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, false);
                opponentembed = await BuildEndPlayerEmbed(meta, false, true);
                component2 = BuildEndGameActions(meta, false);
            }
        }

        if (meta.Guild == null)
            await Context.ModifyOriginalResponseAsync(msg =>
            {
                msg.Components = component2;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
        else
            await Context.ModifyAsync(msg =>
            {
                msg.Components = component2;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
    }
}
