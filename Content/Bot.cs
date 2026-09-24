using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using CotLMinigames.Admin;
using System.Text.Json;

namespace CotLMinigames;

public class BotClient
{
    public static ulong ADMIN = ulong.Parse(File.ReadAllText("admin.txt"));
    public static DiscordSocketClient Client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
    InteractionService interactions = new InteractionService(Client);
    public const string CommandPrefix = "$$";
    public static List<GameMetadata> Games = new();
    public static Emojis Emojis = new();
    public static Dictionary<string, Func<SocketMessage, Task>> Commands = new()
    {
        { "devotion", AdminCommands.AddDevotion },
        { "coins", AdminCommands.GiveCoins }
    };
    public BotClient()
    {
        if (File.Exists("emojis.json"))
        {
            string text = File.ReadAllText("emojis.json");
            Emojis = JsonSerializer.Deserialize<Emojis>(text)!;
        }
        Emojis.MapEmojis();
        
        Client.Log += message =>
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        };

        Client.Ready += async () =>
        {
            await interactions.RegisterCommandsGloballyAsync();
        };

        Client.InteractionCreated += async interaction =>
        {
            SocketInteractionContext ctx = new SocketInteractionContext(Client, interaction);
            await interactions.ExecuteCommandAsync(ctx, null);
        };

        Client.MessageReceived += async message =>
        {
            if (message.Author.IsBot) return;

            if (message.Author.Id == ADMIN)
            {
                string command = message.Content.Split('/')[0];
                if (command.StartsWith(CommandPrefix))
                {
                    command = command.Substring(CommandPrefix.Length);
                    if (Commands.TryGetValue($"{command}", out var handler))
                        await handler(message);
                }
            }
        };

        Client.ButtonExecuted += async component =>
        {
            string[] buttondata = component.Data.CustomId.Split('/');
            switch (buttondata[0])
            {
                case "acceptkb":
                    _ = Knucklebones.Actions.AcceptGame(buttondata, component);
                    break;

                case "acceptbotkb":
                    _ = Knucklebones.Actions.AcceptBotGame(buttondata, component);
                    break;

                case "declinekb":
                    _ = Knucklebones.Actions.DeclineGame(buttondata, component);
                    break;

                case "playkb":
                    _ = Knucklebones.Actions.PlayMove(buttondata, component);
                    break;

                case "forfeitkb":
                    _ = Knucklebones.Actions.ForfeitGame(buttondata, component);
                    break;

                case "lastfourgames":
                    _ = ProfileModule.ShowLastFourGames(buttondata, component);
                    break;

                default:
                    MessageComponent errorButtons = new ComponentBuilder()
                        .WithButton("Yell at @daxcr", style: ButtonStyle.Link, url: "https://discord.com/invite/6vbhdzmGq7")
                        .Build();
                    await component.RespondAsync($"""
Unknown component :(
If you are viewing this in production, yell at `@daxcr`
```Button ID: {component.Data.CustomId}
Component ID: {component.Id}
Guild ID: {component.GuildId}
Channel ID: {component.ChannelId}
Interaction user: @{component.User.Username} ({component.User.Id})
Context type: {component.ContextType}
Time (UTC): {DateTime.UtcNow}
```
""", components: errorButtons);
                    break;
            }
        };
    }

    public async Task Start(string token)
    {
        await interactions.AddModuleAsync<Knucklebones.KBGameModule>(null);
        await interactions.AddModuleAsync<Flockade.FLGameModule>(null);
        await interactions.AddModuleAsync<ProfileModule>(null);
        // await interactions.AddModuleAsync<ServerModule>(null);

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        await Task.Delay(-1);
    }
}