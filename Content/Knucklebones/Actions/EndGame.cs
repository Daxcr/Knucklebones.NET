using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async void EndGame(KBGameMetadata meta, SocketMessageComponent component)
    {
        if (!BotClient.Games.Contains(meta))
            return;
            
        BotClient.Games.Remove(meta);

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
            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
        else
            await component.Message.ModifyAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
    }

    public static async void EndGame(KBGameMetadata meta, IUserMessage message)
    {
        if (!BotClient.Games.Contains(meta))
            return;

        BotClient.Games.Remove(meta);

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
            await (message as RestFollowupMessage)!.ModifyAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
        else
            await message.ModifyAsync(msg =>
            {
                msg.Components = null;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
    }
}