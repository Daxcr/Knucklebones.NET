// god help me

using CotLMinigames.DB;
using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    private static async Task<ContainerBuilder> CreateGameBody(KBGameMetadata meta, bool staticDisplay, IUser initiator, IUser opponent, int crown = -1)
    {
        KBGameMetadata.Table table = meta.InitiatorTurn ? meta.InitiatorTable : meta.OpponentTable;

        ButtonBuilder leftButton = new ButtonBuilder()
            .WithLabel("Left")
            .WithCustomId($"playkb/left/{meta.ID}/{meta.Turn}")
            .WithStyle(ButtonStyle.Primary)
            .WithDisabled(!table.Left.Contains(0));

        ButtonBuilder middleButton = new ButtonBuilder()
            .WithLabel("Middle")
            .WithCustomId($"playkb/middle/{meta.ID}/{meta.Turn}")
            .WithStyle(ButtonStyle.Primary)
            .WithDisabled(!table.Middle.Contains(0));

        ButtonBuilder rightButton = new ButtonBuilder()
            .WithLabel("Right")
            .WithCustomId($"playkb/right/{meta.ID}/{meta.Turn}")
            .WithStyle(ButtonStyle.Primary)
            .WithDisabled(!table.Right.Contains(0));

        // Action rows
        ActionRowBuilder actionRow = new ActionRowBuilder()
            .WithButton(leftButton)
            .WithButton(middleButton)
            .WithButton(rightButton);

        // Thumbnails
        ThumbnailBuilder thumbnail1 = new ThumbnailBuilder()
            .WithMedia(new UnfurledMediaItemProperties(initiator.GetAvatarUrl()));

        ThumbnailBuilder thumbnail2 = new ThumbnailBuilder()
            .WithMedia(new UnfurledMediaItemProperties(opponent.GetAvatarUrl()));

        // Sections
        string[] table1 = meta.BuildTable(meta.InitiatorTable, meta.InitiatorTableDiff, meta.InitiatorTurn);
        string[] table2 = meta.BuildTable(meta.OpponentTable, meta.OpponentTableDiff, !meta.InitiatorTurn);
        SectionBuilder section1 = new SectionBuilder()
            .WithTextDisplay(table1[0])
            .WithTextDisplay(table1[1])
            .WithTextDisplay(table1[2])
            .WithAccessory(thumbnail1);

        SectionBuilder section2 = new SectionBuilder()
            .WithTextDisplay(table2[2])
            .WithTextDisplay(table2[1])
            .WithTextDisplay(table2[0])
            .WithAccessory(thumbnail2);

        // Assembly
        int inPoints = meta.BuildPoints(true);
        string inPointsStr = inPoints.ToString() + (inPoints == 1 ? " point" : " points");
        int opPoints = meta.BuildPoints(false);
        string opPointsStr = opPoints.ToString() + (opPoints == 1 ? " point" : " points");
        ContainerBuilder body = new ContainerBuilder()
            // Player 1
            .WithTextDisplay($"## {initiator.GlobalName + (crown == 0 ? " :crown:" : "")}{(meta.InitiatorTurn && !staticDisplay ? " (Your turn)" : "")}\n-# {inPointsStr}")
            .WithSection(section1) // Board

            .WithSeparator()

            // Player 2
            .WithTextDisplay($"## {(string.IsNullOrEmpty(opponent.GlobalName) ? opponent.Username : opponent.GlobalName) + (crown == 1 ? " :crown:" : "")}{(!meta.InitiatorTurn && !staticDisplay ? " (Your turn)" : "")}\n-# {opPointsStr}")
            .WithSection(section2); // Board

        // Bottom
        if (!staticDisplay)
            body
                .WithSeparator()
                .WithTextDisplay($"Dice:\n# > {(staticDisplay ? BotClient.Emojis.Empty : BotClient.Emojis.DiceLookup![meta.CurrentDice - 1][0][0])}")
                .WithActionRow(actionRow);

        if (staticDisplay)
            body.WithAccentColor(Color.Gold);

        return body;
    }
    public static async Task<MessageComponent> BuildGameComponents(KBGameMetadata meta)
    {
        ContainerBuilder metac = new ContainerBuilder()
            .WithTextDisplay($"Game ending {meta.GameExpiryDisplay}.")

            .WithSeparator()

            .WithTextDisplay($"**{BotClient.Emojis.Dice3Single} Turn:** {meta.Turn}")
            .WithTextDisplay($"**{BotClient.Emojis.Coin} Bet:** {meta.Bet} coins");

        // Extra buttons
        ButtonBuilder helpButton = new ButtonBuilder()
            .WithLabel("How to Play")
            .WithCustomId($"howtokb/1/false")
            .WithStyle(ButtonStyle.Secondary);

        ButtonBuilder forfeitButton = new ButtonBuilder()
            .WithLabel("Forfeit")
            .WithCustomId($"playkb/forfeit/{meta.ID}/0")
            .WithStyle(ButtonStyle.Danger);

        ActionRowBuilder bottomRow = new ActionRowBuilder()
            .WithButton(helpButton)
            .WithButton(forfeitButton);

        IUser initiator = await BotClient.Client.GetUserAsync(meta.InitiatorID);
        IUser opponent = await BotClient.Client.GetUserAsync(meta.OpponentID);

        MessageComponent messageComponents = new ComponentBuilderV2()
            .WithContainer(await CreateGameBody(meta, false, initiator, opponent))
            .WithContainer(metac)
            .WithActionRow(bottomRow)
            .Build();

        return messageComponents;
    }

    public static async Task<MessageComponent> BuildEndGameComponents(KBGameMetadata meta, ulong winnerID, int coinsEarned, int devotionEarned, int godTearsEarned, UserData? winner)
    {
        ContainerBuilder metac = new ContainerBuilder()
            .WithTextDisplay($"Game ended {TimestampTag.FromDateTimeOffset(DateTimeOffset.UtcNow, TimestampTagStyles.Relative)}.")

            .WithSeparator()

            .WithTextDisplay($"**{BotClient.Emojis.Dice3Single} Turn:** {meta.Turn}")
            .WithTextDisplay($"**{BotClient.Emojis.Coin} Bet:** {meta.Bet} coins")

            .WithSeparator();

        if (winnerID != 0)
            metac.WithTextDisplay($"**Winner:** <@{winnerID}>");
        else
            metac.WithTextDisplay($"~~**Winner:** ~~**TIE**");

        if (coinsEarned != 0 && winner != null)
            metac.WithTextDisplay($"**{BotClient.Emojis.Coin} Coins:** +{coinsEarned} ({winner.Inventory.Coins})");

        if (devotionEarned != 0 && winner != null)
        {
            metac.WithTextDisplay($"**{BotClient.Emojis.Devotion} Devotion:** +{devotionEarned} ({winner.Devotion})");
            metac.WithTextDisplay(ProfileModule.CalculateDevotionBar(12, winner.Devotion, ProfileModule.CalculateMaxDevotion(winner.Level)));
        }

        if (godTearsEarned != 0 && winner != null)
        {
            metac.WithTextDisplay($"**{BotClient.Emojis.GodTear} GodTears:** +{godTearsEarned} ({winner.Inventory.GodTears})");
            metac.WithTextDisplay($"You have reached level {winner.Level}!");
        }

        IUser initiator = await BotClient.Client.GetUserAsync(meta.InitiatorID);
        IUser opponent = await BotClient.Client.GetUserAsync(meta.OpponentID);

        // Extra buttons
        ButtonBuilder devoteButton1 = new ButtonBuilder()
            .WithLabel($"Devote to {initiator.GlobalName}")
            .WithCustomId($"devote/{meta.InitiatorID}")
            .WithStyle(ButtonStyle.Primary);

        ButtonBuilder devoteButton2 = new ButtonBuilder()
            .WithLabel($"Devote to {(string.IsNullOrEmpty(opponent.GlobalName) ? opponent.Username : opponent.GlobalName)}")
            .WithCustomId($"devote/{meta.OpponentID}")
            .WithStyle(ButtonStyle.Primary);

        ButtonBuilder rematchButton = new ButtonBuilder()
            .WithLabel("Rematch")
            .WithCustomId($"rematchkb/{meta.InitiatorID}/{meta.OpponentID}/{meta.Bet}")
            .WithStyle(ButtonStyle.Secondary);

        ActionRowBuilder bottomRow = new ActionRowBuilder()
            .WithButton(devoteButton1)
            .WithButton(devoteButton2);

        ActionRowBuilder bottomerRow = new ActionRowBuilder()
            .WithButton(rematchButton);

        int crown = -1;
        if (winner != null)
            crown = winnerID == meta.InitiatorID ? 0 : 1;

        MessageComponent messageComponents = new ComponentBuilderV2()
            .WithContainer(await CreateGameBody(meta, true, initiator, opponent, crown))
            .WithContainer(metac)
            .WithActionRow(bottomRow)
            .WithActionRow(bottomerRow)
            .Build();

        return messageComponents;
    }
}
