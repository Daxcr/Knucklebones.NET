namespace Knucklebones.DB;

public class UserData
{
    public ulong UserID { get; set; }
    public long Coins { get; set; } = 100;
    public long Wins { get; set; } = 0;
    public long GamesPlayed { get; set; } = 0;
    public ushort Streak { get; set; } = 0;
    public bool AcceptingGames { get; set; } = true;
    public long Level { get; set; } = 0;
    public long Devotion { get; set; } = 0;
    public Banner? ActiveBanner { get; set; }
    public List<ICollectable> Inventory { get; set; } = new();
    public List<long> BlockedUserIDs { get; set; } = new();
    public List<GameMetadata> LastTenGames { get; set; } = new();
    public List<ProfileBadges> Badges { get; set; } = new();
    public enum ProfileBadges : ushort
    {
        Developer = 0,
        Contributor = 1, // Someone who contributed to the repo (not applied automatically, talk with Dax)
        Tester = 2,
        EarlySupporter = 3,

        OneWin = 1000,
        TenWins = 1001,
        TwentyWins = 1002,
        FiftyWins = 1003,
        HundredWins = 1004,

        TenGamesPlayed = 2000,
        FiftyGamesPlayed = 2001,
        HundredGamesPlayed = 2002,

        SmallBet = 3000, // < 50 coins
        LargeBet = 3001, // 50 - 200 coins
        MassiveBet = 3002, // > 200 coins
        
        WipedTheFloor = 9000, // Scored 5x more than your opponent
        UnluckyGambler = 9001 // Start a massive bet and fumble
    }

    public static Dictionary<ProfileBadges, string> BadgeEmojis = new()
    {
        { ProfileBadges.Developer, "<:developer:1548878732574589009>" },
        { ProfileBadges.Contributor, "<:contributor:1548879712825118822>" },
        { ProfileBadges.Tester, "<:tester:1548881356015276232>" },
        { ProfileBadges.EarlySupporter, "<:earlysupporter:1548880763972223066>" },

        { ProfileBadges.OneWin, "<:onewin:1548883068302135386>" },
        { ProfileBadges.TenWins, "<:tenwins:1548883070361407569>" },
        { ProfileBadges.TwentyWins, "<:twentywins:1548883400797192232>" },
        { ProfileBadges.FiftyWins, "<:fiftywins:1548884877414498325>" },
        { ProfileBadges.HundredWins, "<:hundredwins:1548884879117258833>" },

        { ProfileBadges.TenGamesPlayed, "<:tengamesplayed:1548886652414328892>" },
        { ProfileBadges.FiftyGamesPlayed, "<:fiftygamesplayed:1548886648710766652>" },
        { ProfileBadges.HundredGamesPlayed, "<:hundredgamesplayed:1548886650438950923>" },

        { ProfileBadges.SmallBet, "<:smallbet:1548888209210085426>" },
        { ProfileBadges.LargeBet, "<:largebet:1548888205732872262>" },
        { ProfileBadges.MassiveBet, "<:massivebet:1548888207372849182>" },
    };

    public void AddDevotion(long amount)
    {
        Devotion += amount;
        if (Devotion < 0)
            Devotion = 0;

        WrapDevotion();
    }
    internal void WrapDevotion()
    {
        if (Devotion > ProfileModule.CalculateMaxDevotion(Level))
        {
            Devotion -= ProfileModule.CalculateMaxDevotion(Level);
            Level += 1;
            WrapDevotion();
        }
    }
}