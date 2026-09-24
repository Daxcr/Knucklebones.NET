using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task ForfeitGame(string[] buttondata, SocketMessageComponent component)
    {
        string gameID = buttondata[1];

        UserContext ctx = new(component);
        await ForfeitGame(ctx, gameID);
    }
    public static async Task ForfeitGame(UserContext Context, string gameID)
    {
        await Context.DeferAsync();

        KBGameMetadata meta = (KBGameMetadata)BotClient.Games.FirstOrDefault(game => gameID == game.ID)!;

        if (!new[] { meta.InitiatorID, meta.OpponentID }.Contains(Context.User!.Id))
            return;

        if (!BotClient.Games.Contains(meta))
            return;
            
        BotClient.Games.Remove(meta);

        bool initiator = Context.User.Id == meta.InitiatorID;

        if (initiator)
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.InitiatorTable = new();
        }
        else
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.OpponentTable = new();
        }

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
            await meta.ForfeitSource!.ModifyOriginalResponseAsync(msg =>
            {
                msg.Components = component2;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });
        else
            await meta.ForfeitSource!.ModifyAsync(msg =>
            {
                msg.Components = component2;
                if (devotionEmbed != null)
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                else
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed };
            });

        await Context.ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = "You forfeited the game :(";
            msg.Components = new ComponentBuilder().Build();
        });
    }
}