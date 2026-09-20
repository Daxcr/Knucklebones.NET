namespace CotLMinigames;

public enum DiceVariant { Normal, New, Delete }

public class Emojis
{
    public string Empty { get; set; } = "<:empty:1549744968334315590>";
    public string EmptyActive { get; set; } = "<:empty_active:1550020358390812733>";

    // Dice 1
    public string Dice1Single { get; set; } = "<:dice1_single:1549203689435168808>";
    public string Dice1Double { get; set; } = "<:dice1_double:1549203687090421790>";
    public string Dice1Triple { get; set; } = "<:dice1_triple:1549203684448276480>";
    public string Dice1SingleNew { get; set; } = "<:dice1_single_new:1549203682523091037>";
    public string Dice1DoubleNew { get; set; } = "<:dice1_double_new:1549203680161439866>";
    public string Dice1TripleNew { get; set; } = "<:dice1_triple_new:1549203678303486053>";
    public string Dice1SingleDelete { get; set; } = "<:dice1_single_delete:1549203641204871248>";
    public string Dice1DoubleDelete { get; set; } = "<:dice1_double_delete:1549203638667182180>";
    public string Dice1TripleDelete { get; set; } = "<:dice1_triple_delete:1549203636373159976>";

    // Dice 2
    public string Dice2Single { get; set; } = "<:dice2_single:1549203761124085840>";
    public string Dice2Double { get; set; } = "<:dice2_double:1549203758502912010>";
    public string Dice2Triple { get; set; } = "<:dice2_triple:1549203756519006218>";
    public string Dice2SingleNew { get; set; } = "<:dice2_single_new:1549203754107277372>";
    public string Dice2DoubleNew { get; set; } = "<:dice2_double_new:1549203752144343142>";
    public string Dice2TripleNew { get; set; } = "<:dice2_triple_new:1549203749963046912>";
    public string Dice2SingleDelete { get; set; } = "<:dice2_single_delete:1549203676089032704>";
    public string Dice2DoubleDelete { get; set; } = "<:dice2_double_delete:1549203673551478826>";
    public string Dice2TripleDelete { get; set; } = "<:dice2_triple_delete:1549203672020287530>";

    // Dice 3
    public string Dice3Single { get; set; } = "<:dice3_single:1549203748021338242>";
    public string Dice3Double { get; set; } = "<:dice3_double:1549203745827594271>";
    public string Dice3Triple { get; set; } = "<:dice3_triple:1549203743868850337>";
    public string Dice3SingleNew { get; set; } = "<:dice3_single_new:1549203741763440680>";
    public string Dice3DoubleNew { get; set; } = "<:dice3_double_new:1549203739167170621>";
    public string Dice3TripleNew { get; set; } = "<:dice3_triple_new:1549203737006837781>";
    public string Dice3SingleDelete { get; set; } = "<:dice3_single_delete:1549203669562564608>";
    public string Dice3DoubleDelete { get; set; } = "<:dice3_double_delete:1549203667394101368>";
    public string Dice3TripleDelete { get; set; } = "<:dice3_triple_delete:1549203665288691792>";

    // Dice 4
    public string Dice4Single { get; set; } = "<:dice4_single:1549203733894922430>";
    public string Dice4Double { get; set; } = "<:dice4_double:1549203731604705300>";
    public string Dice4Triple { get; set; } = "<:dice4_triple:1549203729469673513>";
    public string Dice4SingleNew { get; set; } = "<:dice4_single_new:1549203726361956494>";
    public string Dice4DoubleNew { get; set; } = "<:dice4_double_new:1549203724399022101>";
    public string Dice4TripleNew { get; set; } = "<:dice4_triple_new:1549203722482221147>";
    public string Dice4SingleDelete { get; set; } = "<:dice4_single_delete:1549203663141077123>";
    public string Dice4DoubleDelete { get; set; } = "<:dice4_double_delete:1549203659370536980>";
    public string Dice4TripleDelete { get; set; } = "<:dice4_triple_delete:1549203657382432848>";

    // Dice 5
    public string Dice5Single { get; set; } = "<:dice5_single:1549203720401854554>";
    public string Dice5Double { get; set; } = "<:dice5_double:1549203718455558294>";
    public string Dice5Triple { get; set; } = "<:dice5_triple:1549203715192389632>";
    public string Dice5SingleNew { get; set; } = "<:dice5_single_new:1549203712969285662>";
    public string Dice5DoubleNew { get; set; } = "<:dice5_double_new:1549203710629118062>";
    public string Dice5TripleNew { get; set; } = "<:dice5_triple_new:1549203708452012162>";
    public string Dice5SingleDelete { get; set; } = "<:dice5_single_delete:1549203655104663662>";
    public string Dice5DoubleDelete { get; set; } = "<:dice5_double_delete:1549203652852318238>";
    public string Dice5TripleDelete { get; set; } = "<:dice5_triple_delete:1549203650356977725>";

    // Dice 6
    public string Dice6Single { get; set; } = "<:dice6_single:1549203705914597426>";
    public string Dice6Double { get; set; } = "<:dice6_double:1549203703913910292>";
    public string Dice6Triple { get; set; } = "<:dice6_triple:1549203699316818060>";
    public string Dice6SingleNew { get; set; } = "<:dice6_single_new:1549203697269997589>";
    public string Dice6DoubleNew { get; set; } = "<:dice6_double_new:1549203695357395024>";
    public string Dice6TripleNew { get; set; } = "<:dice6_triple_new:1549203692815913091>";
    public string Dice6SingleDelete { get; set; } = "<:dice6_single_delete:1549203648540835890>";
    public string Dice6DoubleDelete { get; set; } = "<:dice6_double_delete:1549203646330183811>";
    public string Dice6TripleDelete { get; set; } = "<:dice6_triple_delete:1549203643943751801>";

    public string[][][]? DiceLookup;
    
    public void MapEmojis()
    {
        DiceLookup =
        [
            [
                [Dice1Single, Dice1SingleNew, Dice1SingleDelete],
                [Dice1Double, Dice1DoubleNew, Dice1DoubleDelete],
                [Dice1Triple, Dice1TripleNew, Dice1TripleDelete],
            ],
            [
                [Dice2Single, Dice2SingleNew, Dice2SingleDelete],
                [Dice2Double, Dice2DoubleNew, Dice2DoubleDelete],
                [Dice2Triple, Dice2TripleNew, Dice2TripleDelete],
            ],
            [
                [Dice3Single, Dice3SingleNew, Dice3SingleDelete],
                [Dice3Double, Dice3DoubleNew, Dice3DoubleDelete],
                [Dice3Triple, Dice3TripleNew, Dice3TripleDelete],
            ],
            [
                [Dice4Single, Dice4SingleNew, Dice4SingleDelete],
                [Dice4Double, Dice4DoubleNew, Dice4DoubleDelete],
                [Dice4Triple, Dice4TripleNew, Dice4TripleDelete],
            ],
            [
                [Dice5Single, Dice5SingleNew, Dice5SingleDelete],
                [Dice5Double, Dice5DoubleNew, Dice5DoubleDelete],
                [Dice5Triple, Dice5TripleNew, Dice5TripleDelete],
            ],
            [
                [Dice6Single, Dice6SingleNew, Dice6SingleDelete],
                [Dice6Double, Dice6DoubleNew, Dice6DoubleDelete],
                [Dice6Triple, Dice6TripleNew, Dice6TripleDelete],
            ],
        ];
    }

    public string GetDice(int number, int amount, DiceVariant variant) => DiceLookup![number - 1][amount - 1][(int)variant];


    // Devotion
    public string DevotionLeftEmpty { get; set; } = "<:devotion_left_empty:1548911372106997820>";
    public string DevotionLeftHalf { get; set; } = "<:devotion_left_half:1548911393435156580>";
    public string DevotionLeftFull { get; set; } = "<:devotion_left_full:1548911674147479602>";

    public string DevotionCenterEmpty { get; set; } = "<:devotion_center_empty:1548913073669021756>";
    public string DevotionCenterHalf { get; set; } = "<:devotion_center_half:1548913093881495624>";
    public string DevotionCenterFull { get; set; } = "<:devotion_center_full:1548913107466719283>";

    public string DevotionRightEmpty { get; set; } = "<:devotion_right_empty:1548912153107636305>";
    public string DevotionRightHalf { get; set; } = "<:devotion_right_half:1548912167125000232>";
    public string DevotionRightFull { get; set; } = "<:devotion_right_full:1549540951247163473>";

    // Blessings; unfinished
    public string BlessJust { get; set; } = "";
    public string BlessFallen { get; set; } = "";

    // Items
    public string Devotion { get; set; } = "<:devotion:1548923541712543774>";
    public string Coin { get; set; } = "<:coin:1548928750240931900>";
    public string Wool { get; set; } = "<:wool:1549712729248497745>";
    public string GodTear { get; set; } = "<:godtear:1549712752166305852>";
}