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

    public static async Task<(int, int, int, UserData?)> CalculateBetsAndDevotion(KBGameMetadata meta, bool tie, bool initiatorWin)
    {
        using var db = Database.Create();

        UserData initiator = await Database.GetUser(meta.InitiatorID, db);
        UserData opponent = await Database.GetUser(meta.OpponentID, db);

        UserData winner = initiatorWin ? initiator : opponent;
        UserData loser = initiatorWin ? opponent : initiator;

        if (!tie) winner.Wins += 1;
        winner.GamesPlayed += 1;
        if (tie)
            winner.Streak = 0;
        else
            winner.Streak += 1;

        loser.GamesPlayed += 1;
        loser.Streak = 0;

        winner.LastTenGames.Add(meta);
        if (winner.LastTenGames.Count > 10)
            winner.LastTenGames.RemoveAt(0);

        loser.LastTenGames.Add(meta);
        if (loser.LastTenGames.Count > 10)
            loser.LastTenGames.RemoveAt(0);

        int addedDevotion = DevotionOnWin + (meta.Bet * 6);

        // string bar = ProfileModule.CalculateDevotionBar(
        //     ProfileModule.DevotionBarWidth,
        //     winner.Devotion + addedDevotion,
        //     ProfileModule.CalculateMaxDevotion(winner.Level)
        // );
        if (tie)
        {
            winner.Inventory.Coins += meta.Bet;
            loser.Inventory.Coins += meta.Bet;

            await db.SaveChangesAsync();

            return (0, 0, 0, null);
        }
        else
        {
            winner.Inventory.Coins += meta.Bet * 2;

            long level = winner.Level;
            winner.AddDevotion(addedDevotion);
            long godTearsToGive = winner.Level - level;
            winner.Inventory.GodTears += godTearsToGive;

            await db.SaveChangesAsync();

            return (meta.Bet, addedDevotion, (int)godTearsToGive, winner);
        }
    }
}
