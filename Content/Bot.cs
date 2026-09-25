using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using CotLMinigames.Admin;
using System.Text.Json;
using CotLMinigames.Web;

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

        bool commandsRegistered = false;

        Client.Ready += async () =>
        {
            if (commandsRegistered) return;
            commandsRegistered = true;

            foreach (var command in interactions.SlashCommands)
                await Client.CreateGlobalApplicationCommandAsync(ToProps(command));

            foreach (var command in interactions.ContextCommands)
                await Client.CreateGlobalApplicationCommandAsync(ToProps(command));

            await RemoveStaleCommandsAsync();
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

                case "rematchkb":
                    _ = Knucklebones.KBGameModule.Rematch(buttondata, component);
                    break;
                
                case "devote":
                    _ = ProfileModule.Devote(buttondata, component);
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
        Server server = new()
        {
            Port = 12008,
            Host = "127.0.0.1",
            MaxRequestSize = 10 * 1024 * 1024,
        };

        await interactions.AddModuleAsync<Knucklebones.KBGameModule>(null);
        await interactions.AddModuleAsync<Flockade.FLGameModule>(null);
        await interactions.AddModuleAsync<ProfileModule>(null);

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        server.AcceptingConnections = true;

        await Task.Delay(-1);
    }

    public async Task RemoveStaleCommandsAsync()
    {
        var existingCommands = await Client.GetGlobalApplicationCommandsAsync();

        HashSet<string> currentNames = interactions.SlashCommands.Select(command => command.Name)
            .Concat(interactions.ContextCommands.Select(command => command.Name))
            .ToHashSet();

        foreach (SocketApplicationCommand existing in existingCommands)
        {
            if ((int)existing.Type == 4)
                continue;

            if (!currentNames.Contains(existing.Name))
                await existing.DeleteAsync();
        }
    }

    public static ApplicationCommandProperties ToProps(SlashCommandInfo command)
    {
        SlashCommandBuilder builder = new SlashCommandBuilder()
            .WithName(command.Name)
            .WithDescription(command.Description)
            .WithIntegrationTypes(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)
            .WithContextTypes(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel);

        foreach (SlashCommandParameterInfo param in command.Parameters)
            builder.AddOption(ToOptionBuilder(param));

        return builder.Build();
    }

    public static SlashCommandOptionBuilder ToOptionBuilder(SlashCommandParameterInfo param)
    {
        var option = new SlashCommandOptionBuilder
        {
            Name = param.Name,
            Description = param.Description,
            Type = param.DiscordOptionType ?? ApplicationCommandOptionType.String,
            IsRequired = param.IsRequired,
            IsAutocomplete = param.IsAutocomplete,
            MinValue = param.MinValue,
            MaxValue = param.MaxValue,
            MinLength = param.MinLength,
            MaxLength = param.MaxLength
        };

        if (param.ChannelTypes is { Count: > 0 })
            option.ChannelTypes = param.ChannelTypes.ToList();

        if (param.Choices is { Count: > 0 })
        {
            option.Choices = param.Choices.Select(c => new ApplicationCommandOptionChoiceProperties
            {
                Name = c.Name,
                Value = c.Value
            }).ToList();
        }

        return option;
    }

    public static ApplicationCommandProperties ToProps(ContextCommandInfo command)
    {
        return command.CommandType switch
        {
            ApplicationCommandType.User => new UserCommandBuilder().WithName(command.Name).Build(),
            ApplicationCommandType.Message => new MessageCommandBuilder().WithName(command.Name).Build(),
            _ => throw new NotSupportedException($"Unsupported context command type: {command.CommandType}")
        };
    }
}