using CotLMinigames.DB;
using Discord;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public const int DevotionOnWin = 25;
    public static void RecalculateTables(KBGameMetadata meta, string pressedbutton)
    {
        if (meta.InitiatorTurn)
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.InitiatorTable.Add(pressedbutton, meta.CurrentDice, meta.OpponentTable);
        }
        else
        {
            meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
            meta.OpponentTableDiff = meta.OpponentTable.Clone();
            meta.OpponentTable.Add(pressedbutton, meta.CurrentDice, meta.InitiatorTable);
        }
    }

    async public static Task<Embed> CalculateBetsAndDevotion(KBGameMetadata meta, bool initiatorWin)
    {
        var db = Database.Create();

        UserData initiator = await Database.GetUser(meta.InitiatorID, db);
        UserData opponent = await Database.GetUser(meta.OpponentID, db);

        UserData winner = initiatorWin ? initiator : opponent;
        UserData loser = initiatorWin ? opponent : initiator;

        int addedDevotion = DevotionOnWin + (meta.Bet * 4);

        string bar = ProfileModule.CalculateDevotionBar(
            ProfileModule.DevotionBarWidth,
            winner.Devotion + addedDevotion,
            ProfileModule.CalculateMaxDevotion(winner.Level)
        );

        winner.Inventory.Coins += meta.Bet * 2;
        winner.AddDevotion(addedDevotion);

        await db.SaveChangesAsync();

        return new EmbedBuilder()
            .WithDescription($"""
**Winner:** <@{winner.UserID}>
{ProfileModule.GenericEmojis["coin"]} Coins: +{meta.Bet} ({winner.Inventory.Coins})
{ProfileModule.GenericEmojis["devotion"]} Devotion: +{addedDevotion}
{bar}

**Loser:** <@{loser.UserID}>
{ProfileModule.GenericEmojis["coin"]} Coins: -{meta.Bet} ({loser.Inventory.Coins})
""")
            .WithColor(Color.Default)
            .Build();
    }
}