using Discord;
using Discord.Interactions;
using Knucklebones.DB;

namespace Knucklebones;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int DevotionBarWidth = 14;
    public static Dictionary<string, string> DevotionSegments = new()
    {
        { "left_empty", "<:devotion_left_empty:1548911372106997820>" },
        { "left_half", "<:devotion_left_half:1548911393435156580>" },
        { "left_full", "<:devotion_left_full:1548911674147479602>" },

        { "center_empty", "<:devotion_center_empty:1548913073669021756>" },
        { "center_half", "<:devotion_center_half:1548913093881495624>" },
        { "center_full", "<:devotion_center_full:1548913107466719283>" },

        { "right_empty", "<:devotion_right_empty:1548912153107636305>" },
        { "right_half", "<:devotion_right_half:1548912167125000232>" },
        { "right_full", "<:devotion_right_full:1549540951247163473>" }
    };

    public static Dictionary<string, string> GenericEmojis = new()
    {
        { "devotion", "<:devotion:1548923541712543774>" },
        { "coin", "<:coin:1548928750240931900>" },
    };

    [SlashCommand("profile", "View your or somebody else's profile")]
    public async Task Profile(IUser? user = null)
    {
        if (user == null)
            user = Context.User;
        await DeferAsync();

        using DatabaseContext db = Database.Create();
        UserData? usermeta = await Database.GetUser(user.Id, db);

        Embed main = new EmbedBuilder()
            .WithTitle($"{user.GlobalName}'s profile")
            .WithDescription($"""
**Coins {GenericEmojis["coin"]}:** {usermeta.Coins}
**Wins:** {usermeta.Wins}
**Games played:** {usermeta.GamesPlayed}
""")
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithImageUrl("https://cdn.dax.cr/knucklebones.net/banners/default.png")
            .WithColor(Color.Blue)
            .Build();

        long maxDevotion = CalculateMaxDevotion(usermeta.Level);

        Embed devotion = new EmbedBuilder()
            .WithDescription($"""
{usermeta.Devotion} / {maxDevotion}{GenericEmojis["devotion"]}
{CalculateDevotionBar(DevotionBarWidth, usermeta.Devotion, maxDevotion)}
**Level:** {usermeta.Level}
""")
            .WithColor(Color.LighterGrey)
            .Build();

        MessageComponent components = new ComponentBuilder()
            .WithButton("Edit your profile", $"editprofile", ButtonStyle.Secondary)
            .WithButton("View your inventory", $"viewinventory", ButtonStyle.Secondary)
            .Build();

        await FollowupAsync(embeds: [main, devotion], components: components);
    }
    public static long CalculateMaxDevotion(long level)
    {
        if (level == 0)
            return 35;
        
        double log = Math.Log((level * 10) + 1);
        double raw = (log * 100) + 20 + (level * 40);
        return (long)Math.Floor(Math.Clamp(raw, 20, 1300));
    }
    public static string CalculateDevotionBar(int barWidth, long devotion, long maxDevotion)
    {
        float segmentSize = (float)maxDevotion / barWidth;

        string result = string.Empty;
        for (int i = 0; i < barWidth; i++)
        {
            float iterationValue = i * segmentSize;
            if (i == 0)
            {
                if (devotion == 0)
                {
                    result += DevotionSegments["left_empty"];
                } else if (devotion < segmentSize / 2)
                {
                    result += DevotionSegments["left_half"];
                } else
                {
                    result += DevotionSegments["left_full"];
                }
            } else if (i == barWidth - 1)
            {
                if (devotion < iterationValue)
                {
                    result += DevotionSegments["right_empty"];
                } else if (devotion < maxDevotion)
                {
                    result += DevotionSegments["right_half"];
                } else
                {
                    result += DevotionSegments["right_full"];
                }
            } else {
                if (devotion < iterationValue)
                {
                    result += DevotionSegments["center_empty"];
                } else if (devotion < iterationValue + (segmentSize / 2))
                {
                    result += DevotionSegments["center_half"];
                } else
                {
                    result += DevotionSegments["center_full"];
                }
            }
        }

        return result;
    }

    async public static Task<string> GetProfilePicture(ulong uid)
    {
        IUser user = await KnucklebonesBot.Client.GetUserAsync(uid);
        return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
    } 
}