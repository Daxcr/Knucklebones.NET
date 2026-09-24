using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task<Embed> BuildPlayerEmbed(KBGameMetadata meta, bool initiator, bool initiatorTurn) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, (initiatorTurn && initiator) || (!initiatorTurn && !initiator),!initiator)}
**Points:** {meta.BuildPoints(initiator)}
<@{(initiator ? meta.InitiatorID : meta.OpponentID)}> {((initiatorTurn && initiator) || (!initiatorTurn && !initiator) ? $"(Your turn!)" : string.Empty)}
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor((initiator && initiatorTurn) || (!initiator && !initiatorTurn) ? Color.LighterGrey : Color.Default)
            .Build();

    public static async Task<Embed> BuildEndPlayerEmbed(KBGameMetadata meta, bool initiator, bool winner) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, false, !initiator)}
**Points:** {meta.BuildPoints(initiator)}
<@{(initiator ? meta.InitiatorID : meta.OpponentID)}>
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor(winner ? Color.Gold : Color.Default)
            .Build();

    public static Embed BuildDiceEmbed(KBGameMetadata meta) =>
        new EmbedBuilder()
            .WithDescription($"-# Game expiring {meta.GameExpiryDisplay}")
            .WithImageUrl($"https://cdn.dax.cr/knucklebones.net/die/dice{meta.CurrentDice}_mini.png")
            .Build();

    public static MessageComponent BuildGameActions(KBGameMetadata meta, bool initiator)
    {
        KBGameMetadata.Table table = initiator ? meta.InitiatorTable : meta.OpponentTable;
        return new ComponentBuilder()
            .WithButton("Left", $"playkb/left/{meta.ID}/{meta.Turn + 1}", ButtonStyle.Primary, disabled: !table.Left.Contains(0))
            .WithButton("Middle", $"playkb/middle/{meta.ID}/{meta.Turn + 1}", ButtonStyle.Primary, disabled: !table.Middle.Contains(0))
            .WithButton("Right", $"playkb/right/{meta.ID}/{meta.Turn + 1}", ButtonStyle.Primary, disabled: !table.Right.Contains(0))
            .WithButton("Forfeit", $"playkb/forfeit/{meta.ID}/0", ButtonStyle.Danger)
            .Build();
    }
    public static MessageComponent BuildEndGameActions(KBGameMetadata meta, bool initiatorWon)
    {
        return new ComponentBuilder()
            .WithButton("Rematch", $"rematchkb/{meta.InitiatorID}/{meta.OpponentID}/{meta.Bet}", ButtonStyle.Primary)
            .WithButton("Devote to winner", $"devote/{(initiatorWon ? meta.InitiatorID : meta.OpponentID)}", ButtonStyle.Secondary)
            .Build();
    }

    public static MessageComponent BuildEndGameActionsTie(KBGameMetadata meta)
    {
        return new ComponentBuilder()
            .WithButton("Rematch", $"rematchkb/{meta.InitiatorID}/{meta.OpponentID}/{meta.Bet}", ButtonStyle.Primary)
            .Build();
    }
}