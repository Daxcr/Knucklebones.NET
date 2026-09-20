using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task ForfeitGame(string[] buttondata, SocketMessageComponent component)
    {
        await component.DeferAsync();

        KBGameMetadata meta = (KBGameMetadata)BotClient.Games.FirstOrDefault(game => buttondata[1] == game.ID)!;

        if (!new[] { meta.InitiatorID, meta.OpponentID }.Contains(component.User.Id))
            return;

        if (!BotClient.Games.Contains(meta))
            return;
            
        BotClient.Games.Remove(meta);

        bool initiator = component.User.Id == meta.InitiatorID;

        if (initiator)
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.InitiatorTable = new();
        }
        else
        {
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.OpponentTable = new();
        }

        int initiatorScore = meta.BuildPoints(true);
        int opponentScore = meta.BuildPoints(false);

        Embed initiatorembed;
        Embed opponentembed;
        Embed? devotionEmbed = null;

        if (initiatorScore == opponentScore)
        {
            initiatorembed = await BuildEndPlayerEmbed(meta, true, false);
            opponentembed = await BuildEndPlayerEmbed(meta, false, false);
        } else
        {
            devotionEmbed = await CalculateBetsAndDevotion(meta, initiatorScore > opponentScore);

            if (initiatorScore > opponentScore)
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, true);
                opponentembed = await BuildEndPlayerEmbed(meta, false, false);  
            } else
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, false);
                opponentembed = await BuildEndPlayerEmbed(meta, false, true);
            }
        }
    
        if (meta.Guild == null)
            await meta.ForfeitSource!.ModifyOriginalResponseAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
        else
            await meta.ForfeitSource!.Message.ModifyAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });

        await component.ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = "You forfeited the game :(";
            msg.Components = new ComponentBuilder().Build();
        });
    }
}