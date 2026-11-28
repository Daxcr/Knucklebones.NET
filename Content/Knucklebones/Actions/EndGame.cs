using CotLMinigames.DB;
using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task EndGame(UserContext Context, KBGameMetadata meta, bool isForfeit = false)
    {
        meta.Expired = true;
        if (!BotClient.Games.Contains(meta))
            return;

        BotClient.Games.Remove(meta);

        int initiatorScore = meta.BuildPoints(true);
        int opponentScore = meta.BuildPoints(false);

        ulong winnerID;

        if (initiatorScore == opponentScore)
        {
            winnerID = 0;
        }
        else
        {
            winnerID = initiatorScore > opponentScore ? meta.InitiatorID : meta.OpponentID;
        }

        (int, int, int, UserData?) results = await CalculateBetsAndDevotion(meta, initiatorScore == opponentScore, initiatorScore > opponentScore);

        MessageComponent endGameComponents = await BuildEndGameComponents(meta, winnerID, results.Item1, results.Item2, results.Item3, results.Item4);

        UserContext ctx = isForfeit ? meta.ForfeitSource! : Context;

        if (meta.Guild == null)
            await ctx.ModifyOriginalResponseAsync(async msg =>
            {
                msg.Content = null;
                msg.Embed = null;
                msg.Embeds = null;
                msg.Flags = MessageFlags.ComponentsV2;
                msg.Components = endGameComponents;
            });
        else
            await ctx.ModifyAsync(async msg =>
            {
                msg.Content = null;
                msg.Embed = null;
                msg.Embeds = null;
                msg.Flags = MessageFlags.ComponentsV2;
                msg.Components = endGameComponents;
            });
    }
}
