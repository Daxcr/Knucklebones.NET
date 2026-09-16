using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using CotLMinigames.Admin;

namespace CotLMinigames;

public class BotClient
{
    public static ulong ADMIN = ulong.Parse(File.ReadAllText("admin.txt"));
    public static DiscordSocketClient Client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
    InteractionService interactions = new InteractionService(Client);
    public const string CommandPrefix = "$$";
    public static List<GameMetadata> Games = new();
    public static Dictionary<string, Func<SocketMessage, Task>> Commands = new()
    {
        { "devotion", AdminCommands.AddDevotion }
    };
    public BotClient()
    {
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
                case "acceptgame":
                    _ = Knucklebones.Actions.AcceptGame(buttondata, component);
                    break;

                case "declinegame":
                    _ = Knucklebones.Actions.DeclineGame(buttondata, component);
                    break;

                case "play":
                    _ = Knucklebones.Actions.PlayMove(buttondata, component);
                    break;
            }
        };
    }

    public async Task Start(string token)
    {
        await interactions.AddModuleAsync<Knucklebones.KBGameModule>(null);
        await interactions.AddModuleAsync<ProfileModule>(null);
        await interactions.AddModuleAsync<ServerModule>(null);

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        await Task.Delay(-1);
    }
}