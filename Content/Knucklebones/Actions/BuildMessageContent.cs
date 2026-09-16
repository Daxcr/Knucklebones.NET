using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task<Embed> BuildPlayerEmbed(KBGameMetadata meta, bool initiator) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, !initiator)}
**Points:** {meta.BuildPoints(initiator)}
{((meta.InitiatorTurn && initiator) || (!meta.InitiatorTurn && !initiator) ? $"(Your turn!)" : string.Empty)}
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor((initiator && meta.InitiatorTurn) || (!initiator && !meta.InitiatorTurn) ? Color.LighterGrey : Color.Default)
            .Build();

    public static async Task<Embed> BuildEndPlayerEmbed(KBGameMetadata meta, bool initiator, bool winner) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, !initiator)}
**Points:** {meta.BuildPoints(initiator)}
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor(winner ? Color.Gold : Color.Default)
            .Build();

    public static Embed BuildDiceEmbed(KBGameMetadata meta)
    {
        if (meta.Guild == null)
            return new EmbedBuilder()
                .WithDescription($"""
# {KBGameMetadata.DiceEmojis[$"dice{meta.CurrentDice}_single"]}
Game will expire in {meta.GameExpiryDisplay}
""")
                .Build();
        else
            return new EmbedBuilder()
                .WithDescription($"# {KBGameMetadata.DiceEmojis[$"dice{meta.CurrentDice}_single"]}")
                .Build();
    }

    public static MessageComponent BuildGameActions(KBGameMetadata meta, bool initiator)
    {
        KBGameMetadata.Table table = initiator ? meta.InitiatorTable : meta.OpponentTable;
        return new ComponentBuilder()
            .WithButton("Left", $"play/left/{meta.ID}", ButtonStyle.Primary, disabled: !table.Left.Contains(0))
            .WithButton("Middle", $"play/middle/{meta.ID}", ButtonStyle.Primary, disabled: !table.Middle.Contains(0))
            .WithButton("Right", $"play/right/{meta.ID}", ButtonStyle.Primary, disabled: !table.Right.Contains(0))
            .Build();
    }
}