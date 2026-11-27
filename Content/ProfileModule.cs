using CotLMinigames.DB;
using CotLMinigames.Knucklebones;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace CotLMinigames;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class ProfileModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int DevotionBarWidth = 12;
    public const int DevotionOnDevote = 10;

    [SlashCommand("profile", "View your or somebody else's profile")]
    public async Task Profile(IUser? user = null)
    {
        UserContext ctx = new(Context);
        await Profile(ctx, user);
    }
    public static async Task Profile(UserContext Context, IUser? user = null)
    {
        try
        {
            if (user == null)
                user = Context.User;
            await Context.DeferAsync();

            using DatabaseContext db = Database.Create();
            UserData? usermeta = await Database.GetUser(user!.Id, db);

            string avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
            long maxDevotion = CalculateMaxDevotion(usermeta.Level);

            ContainerBuilder container = new ContainerBuilder()
                .WithTextDisplay($"# {(string.IsNullOrEmpty(user.GlobalName) ? user.Username : user.GlobalName)}'s Profile")

                .WithSeparator()

                .WithSection(new SectionBuilder()
                    .WithTextDisplay("## Stats")
                    .WithTextDisplay($"> **Wins:** {usermeta.Wins}\n> **Games Played:** {usermeta.GamesPlayed}\n> **Streak:** {usermeta.Streak}")
                    .WithAccessory(new ThumbnailBuilder(new UnfurledMediaItemProperties(avatarUrl))))

                .WithActionRow(new ActionRowBuilder()
                    .WithButton(new ButtonBuilder()
                        .WithLabel("View last 10 games")
                        .WithStyle(ButtonStyle.Secondary)
                        .WithCustomId($"lastfourgames/{usermeta.UserID}")))

                .WithSeparator()

                .WithTextDisplay("## Inventory")
                .WithTextDisplay($"> **{BotClient.Emojis.Coin} Coins:** {usermeta.Inventory.Coins}\n> **{BotClient.Emojis.Wool} Wool:** {usermeta.Inventory.Wool}\n> **{BotClient.Emojis.GodTear} God Tears:** {usermeta.Inventory.GodTears}")

                .WithActionRow(new ActionRowBuilder()
                    .WithButton(new ButtonBuilder()
                        .WithLabel($"Open {(string.IsNullOrEmpty(user.GlobalName) ? user.Username : user.GlobalName)}'s inventory")
                        .WithStyle(ButtonStyle.Secondary)
                        .WithCustomId($"inventory/{usermeta.UserID}")
                        .WithDisabled(true))
                    .WithButton(new ButtonBuilder()
                        .WithLabel($"Open Shop")
                        .WithStyle(ButtonStyle.Primary)
                        .WithCustomId($"shop")
                        .WithDisabled(true)))

                .WithSeparator()

                .WithTextDisplay("## Devotion")
                .WithTextDisplay($"> **Level:** {usermeta.Level}\n> **{BotClient.Emojis.Devotion} Devotion:** {usermeta.Devotion}/{maxDevotion}\n> {CalculateDevotionBar(DevotionBarWidth, usermeta.Devotion, maxDevotion)}")

                .WithMediaGallery(new MediaGalleryBuilder().AddItem("https://cdn.dax.cr/knucklebones.net/banners/default.png?t=2"));

            MessageComponent component = new ComponentBuilderV2()
                .WithContainer(container)
                .WithActionRow(new ActionRowBuilder()
                    .WithButton(new ButtonBuilder()
                        .WithLabel("Cultcord Repo")
                        .WithStyle(ButtonStyle.Link)
                        .WithUrl("https://github.com/Daxcr/Knucklebones.NET"))
                    .WithButton(new ButtonBuilder()
                        .WithLabel("Edit my profile")
                        .WithStyle(ButtonStyle.Secondary)
                        .WithCustomId("editprofile")
                        .WithDisabled(true)))
                .Build();

            await Context.FollowupAsync(components: component);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    [SlashCommand("devote", "Devote to somebody")]
    public async Task Devote(IUser user)
    {
        UserContext ctx = new(Context);
        await Devote(ctx, user);
    }
    public static async Task Devote(string[] ButtonData, SocketMessageComponent component)
    {
        UserContext ctx = new(component);
        IUser user = await BotClient.Client.GetUserAsync(ulong.Parse(ButtonData[1]));
        await Devote(ctx, user);
    }
    public static async Task Devote(UserContext Context, IUser user)
    {
        if (user.Id == Context.User?.Id)
        {
            await Context.RespondAsync("You can't devote to yourself.", ephemeral: true);
            return;
        }
        await Context.DeferAsync();

        using var db = Database.Create();
        UserData devotee = await Database.GetUser(Context.User!.Id, db);
        UserData devoted = await Database.GetUser(user.Id, db);

        IUser devoteeUser = await BotClient.Client.GetUserAsync(Context.User.Id);

        if ((DateTime.UtcNow - devotee.LastDevote).TotalHours < 1)
        {
            await Context.FollowupAsync("You've already devoted to somebody in the last hour!", ephemeral: true);
            return;
        }

        int coins = new Random().Next(1, 6);
        long maxDevotion = CalculateMaxDevotion(devoted.Level);
        long currentDevotion = devoted.Devotion;

        devotee.Inventory.Coins += coins;
        devotee.LastDevote = DateTime.UtcNow;

        long level = devoted.Level;
        devoted.AddDevotion(DevotionOnDevote);
        long godTearsToGive = devoted.Level - level;
        devoted.Inventory.GodTears += godTearsToGive;

        ContainerBuilder container = new ContainerBuilder()
            .WithTextDisplay($"## {devoteeUser.GlobalName} has devoted to {(string.IsNullOrEmpty(user.GlobalName) ? user.Username : user.GlobalName)}")
            .WithSeparator()
            .WithTextDisplay($"<@{devoted.UserID}>");

        if (godTearsToGive == 0)
        {
            container.WithTextDisplay($"""
                > **{BotClient.Emojis.Devotion} Devotion:** +{DevotionOnDevote} ({currentDevotion + DevotionOnDevote}/{maxDevotion})
                > {CalculateDevotionBar(DevotionBarWidth, currentDevotion + DevotionOnDevote, maxDevotion)}
                > **Level:** {devoted.Level}
                """);
        }
        else
        {
            container.WithTextDisplay($"""
                > **{BotClient.Emojis.Devotion} Devotion:** +{DevotionOnDevote} ({currentDevotion + DevotionOnDevote}/{maxDevotion})
                > {CalculateDevotionBar(DevotionBarWidth, currentDevotion + DevotionOnDevote, maxDevotion)}
                > **Level:** +{godTearsToGive} ({devoted.Level})
                > **{BotClient.Emojis.GodTear} God Tears:** +{godTearsToGive} ({devoted.Inventory.GodTears})
                """);
        }

        container.WithSeparator()
            .WithTextDisplay($"<@{devotee.UserID}>")
            .WithTextDisplay($"> **{BotClient.Emojis.Coin} Coins:** +{coins} ({devotee.Inventory.Coins})");

        MessageComponent component = new ComponentBuilderV2()
            .WithContainer(container)
            .Build();

        await Context.FollowupAsync(components: component);

        await db.SaveChangesAsync();
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
                    result += BotClient.Emojis.DevotionLeftEmpty;
                }
                else if (devotion < segmentSize / 2)
                {
                    result += BotClient.Emojis.DevotionLeftHalf;
                }
                else
                {
                    result += BotClient.Emojis.DevotionLeftFull;
                }
            }
            else if (i == barWidth - 1)
            {
                if (devotion < iterationValue)
                {
                    result += BotClient.Emojis.DevotionRightEmpty;
                }
                else if (devotion < maxDevotion)
                {
                    result += BotClient.Emojis.DevotionRightHalf;
                }
                else
                {
                    result += BotClient.Emojis.DevotionRightFull;
                }
            }
            else
            {
                if (devotion < iterationValue)
                {
                    result += BotClient.Emojis.DevotionCenterEmpty;
                }
                else if (devotion < iterationValue + (segmentSize / 2))
                {
                    result += BotClient.Emojis.DevotionCenterHalf;
                }
                else
                {
                    result += BotClient.Emojis.DevotionCenterFull;
                }
            }
        }

        return result;
    }

    async public static Task<string> GetProfilePicture(ulong uid)
    {
        IUser user = await BotClient.Client.GetUserAsync(uid);
        return user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
    }

    public static async Task ShowLastFourGames(string[] buttondata, SocketMessageComponent component)
    {
        await component.RespondAsync("This feature is currently lacking Components V2 support. I'll design this later.\n-# \\- Wise guy", ephemeral: true);
//         await component.DeferAsync();

//         using var db = Database.Create();
//         UserData userdata = await Database.GetUser(ulong.Parse(buttondata[1]), db);
//         IUser user = await BotClient.Client.GetUserAsync(userdata.UserID);

//         if (userdata.LastTenGames.Count == 0)
//         {
//             await component.FollowupAsync($"**{user.GlobalName}** has not played any games yet.");
//             return;
//         }

//         List<Embed> embeds = new();

//         foreach (GameMetadata meta in userdata.LastTenGames.AsEnumerable().Reverse().Take(4))
//         {
//             if (meta is KBGameMetadata kbmeta)
//             {
//                 string top = kbmeta.BuildTable(kbmeta.InitiatorTable, kbmeta.InitiatorTableDiff, false, small: true);
//                 string bottom = kbmeta.BuildTable(kbmeta.OpponentTable, kbmeta.OpponentTableDiff, false, true, true);

//                 Embed embed = new EmbedBuilder()
//                     .WithTitle("Knucklebones")
//                     .WithDescription($"""
// **Bet:** {BotClient.Emojis.Coin} {meta.Bet}

// {top}<@{meta.InitiatorID}> | **Points:** {kbmeta.BuildPoints(true)}

// {bottom}<@{meta.OpponentID}> | **Points:** {kbmeta.BuildPoints(false)}
// """)
//                     .Build();

//                 embeds.Add(embed);
//             }
//         }

//         await component.FollowupAsync(embeds: embeds.ToArray(), ephemeral: true);
    }
}
