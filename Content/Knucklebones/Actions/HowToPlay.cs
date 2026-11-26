using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task HowToPlay(string[] buttondata, SocketMessageComponent component)
    {
        UserContext ctx = new(component);
        uint page = uint.Parse(buttondata[1]);
        bool editMessage = bool.Parse(buttondata[2]);

        if (editMessage)
            await component.DeferAsync();

        await HowToPlay(ctx, page, editMessage);
    }
    public async static Task HowToPlay(UserContext Context, uint page, bool editMessage)
    {
        try
        {
            // Buttons
            ButtonBuilder previousButton = new ButtonBuilder()
                .WithLabel("← Previous")
                .WithCustomId($"howtokb/{page - 1}/true")
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(page <= 0);

            ButtonBuilder nextButton = new ButtonBuilder()
                .WithLabel("Next →")
                .WithCustomId($"howtokb/{page + 1}/true")
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(page + 1 >= HTPPages.Count);

            ButtonBuilder flockadeButton = new ButtonBuilder()
                .WithLabel("Go to Flockade Manual")
                .WithCustomId($"howtofl/0/true")
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(true);

            ButtonBuilder tos = new ButtonBuilder()
                .WithLabel("Terms and Privacy Policy")
                .WithStyle(ButtonStyle.Link)
                .WithUrl("https://cdn.dax.cr/knucklebones.net/legal.html");

            ButtonBuilder issues = new ButtonBuilder()
                .WithLabel("Report an issue")
                .WithStyle(ButtonStyle.Link)
                .WithUrl("https://cdn.dax.cr/knucklebones.net/legal.html");

            // Action rows
            ActionRowBuilder navButtons = new ActionRowBuilder()
                .WithButton(previousButton)
                .WithButton(nextButton);

            ActionRowBuilder bottomButtons = new ActionRowBuilder()
                .WithButton(flockadeButton)
                .WithButton(tos);

            // Assembly
            ContainerBuilder navigation = new ContainerBuilder()
                .WithTextDisplay("# Knucklebones Manual")
                .WithSeparator()
                .WithActionRow(new ActionRowBuilder().WithSelectMenu(BuildHTPMenu(page)))
                .WithActionRow(navButtons);

            ContainerBuilder body = new ContainerBuilder()
                .WithComponents(HTPPages[(int)page].Item3);

            MessageComponent messageComponents = new ComponentBuilderV2()
                .WithContainer(navigation)
                .WithContainer(body)
                .WithActionRow(bottomButtons)
                .Build();

            if (editMessage)
                await Context.ModifyOriginalResponseAsync(msg => msg.Components = messageComponents);
            else
                await Context.RespondAsync(components: messageComponents, ephemeral: true);
        } catch (Exception ex) { Console.WriteLine(ex); }
    }

    private readonly static List<(string, string, List<IMessageComponentBuilder>)> HTPPages =
    [
        // First page
        (
            "What is Knucklebones",
            "I'll do you one better, WHY is Knucklebones?",
            [
                new SectionBuilder()
                    .WithTextDisplay("## Summary")
                    .WithTextDisplay("Knucklebones is a minigame from the hit video game Cult of the Lamb, developed by Massive Monster. Cultcord is a recreation of that as a Discord bot.\n\n> Go support Cult of the Lamb; it's a neat game :)\n> You may also report issues on my Github.\n\nThis manual covers how to play Knucklebones with Cultcord. Use the dropdown or buttons above to navigate.")
                    .WithAccessory(new ThumbnailBuilder(new UnfurledMediaItemProperties("https://cdn.dax.cr/knucklebones.net/gallery/cube.gif"))),
                new ActionRowBuilder()
                    .WithButton(new ButtonBuilder()
                        .WithLabel("Cult of the Lamb")
                        .WithStyle(ButtonStyle.Link)
                        .WithUrl("https://store.steampowered.com/app/1313140/Cult_of_the_Lamb/"))
                    .WithButton(new ButtonBuilder()
                        .WithLabel("Cultcord Repo")
                        .WithStyle(ButtonStyle.Link)
                        .WithUrl("https://github.com/Daxcr/Knucklebones.NET")),
                new SeparatorBuilder(),
                new MediaGalleryBuilder().AddItem("https://cdn.dax.cr/knucklebones.net/gallery/drunkgoat.gif"),
            ]
        ),
        // Second page
        (
            "How to Play",
            "Knucklebones rules",
            [
                // Idk if players need this to be honest
                // new TextDisplayBuilder("## How to start a game"),
                // new TextDisplayBuilder("To start a game of Knucklebones directly inside Discord, use the `/knucklebones` command in a text channel. You may optionally select a user to challenge and/or place a bet.\n\nExample:\n> /knucklebones user:`@Sorkopiko` bet:`5 Coins`"),
                // // new MediaGalleryBuilder().AddItem($"https://cdn.dax.cr/knucklebones.net/gallery/slashkb.png?t=3"),
                // new SeparatorBuilder(),
                // new TextDisplayBuilder("## How to place a dice"),
                // new TextDisplayBuilder("To place your dice, use the `Left`, `Middle`, and `Right` controls below the game boards."),
                // new MediaGalleryBuilder().AddItem($"https://cdn.dax.cr/knucklebones.net/gallery/kbbuttons.png"),
                // new SeparatorBuilder(),

                new TextDisplayBuilder("## Dice stacking"),
                new TextDisplayBuilder($"Stacking dice in the same column gives bonus points.\nIn this example, a double six ({BotClient.Emojis.Dice6Double} {BotClient.Emojis.Dice6Double}) gives 24 points instead of 12."),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.Dice6DoubleNew} {BotClient.Emojis.EmptyActive}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.Dice6Double} {BotClient.Emojis.Dice4Single}"),

                new SeparatorBuilder(),

                new TextDisplayBuilder("## Dice destroying"),
                new TextDisplayBuilder("Place a dice in the same column as your opponent to destroy theirs."),

                new TextDisplayBuilder("> You"),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive} {BotClient.Emojis.EmptyActive}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.EmptyActive} {BotClient.Emojis.Dice4SingleNew} {BotClient.Emojis.EmptyActive}"),

                new TextDisplayBuilder("> Some other guy"),
                new SectionBuilder()
                    .WithTextDisplay($"# {BotClient.Emojis.Dice6Single} {BotClient.Emojis.Dice4DoubleDelete} {BotClient.Emojis.Empty}")
                    .WithTextDisplay($"# {BotClient.Emojis.Empty} {BotClient.Emojis.Dice4DoubleDelete} {BotClient.Emojis.Empty}")
                    .WithTextDisplay($"# {BotClient.Emojis.Empty} {BotClient.Emojis.Dice3Single} {BotClient.Emojis.Empty}")
                    .WithAccessory(new ThumbnailBuilder(new UnfurledMediaItemProperties("https://cdn.dax.cr/knucklebones.net/gallery/madlamb.gif"))),
            ]
        ),
        // Third page
        (
            "Winning & Bets",
            "How to win, and sell your soul too I guess...",
            [
                new TextDisplayBuilder("## Win condition"),
                new TextDisplayBuilder("The game ends when one player's board is full, and the player with the most points wins."),

                new TextDisplayBuilder("> You (56 points) :crown:"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice6Single} {BotClient.Emojis.Dice3Triple} {BotClient.Emojis.Dice2SingleNew}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice3Single} {BotClient.Emojis.Dice3Triple} {BotClient.Emojis.Dice4Double}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice2Single} {BotClient.Emojis.Dice3Triple} {BotClient.Emojis.Dice4Double}"),

                new TextDisplayBuilder("> Opponent (33 points)"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice1Triple} {BotClient.Emojis.Dice6Double} {BotClient.Emojis.Dice2SingleDelete}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice1Triple} {BotClient.Emojis.Dice6Double} {BotClient.Emojis.Empty}"),
                new TextDisplayBuilder($"# {BotClient.Emojis.Dice1Triple} {BotClient.Emojis.Empty} {BotClient.Emojis.Empty}"),

                new SeparatorBuilder(),

                new TextDisplayBuilder("## Betting"),
                new TextDisplayBuilder($"You can specify a {BotClient.Emojis.Coin}bet before starting a game. A higher bet will generate more {BotClient.Emojis.Devotion}devotion once a game ends."),
            ]
        )
    ];

    private static SelectMenuBuilder BuildHTPMenu(uint page)
    {
        SelectMenuBuilder menu = new SelectMenuBuilder()
            .WithCustomId("howtokb-menu")
            .WithPlaceholder("Choose a page")
            .WithMinValues(1)
            .WithMaxValues(1);

        int index = 0;
        foreach (var option in HTPPages)
        {
            menu.AddOption($"{index + 1} - {option.Item1}", index.ToString(), option.Item2, isDefault: index == page);
            index += 1;
        }

        return menu;
    }

    public static async Task HowToPlayMenu(string[] menudata, SocketMessageComponent component)
    {
        UserContext ctx = new(component);
        uint page = uint.Parse(menudata[0]);

        await component.DeferAsync();

        await HowToPlay(ctx, page, true);
    }
}
