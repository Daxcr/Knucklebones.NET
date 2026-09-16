using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Knucklebones.DB;

namespace Knucklebones;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("config", "Configure your server to work with Knucklebones.NET. Send this message on its own for a help menu.")]
    public async Task ServerSettings(bool? pingOpponents = null, ITextChannel? restrictToChannel = null, bool? restrictToChannelEnabled = null, GameMetadata.GameState? newgameBehaviour = null)
    {
        if (Context.User is not SocketGuildUser)
        {
            await RespondAsync("This isn't a server, or at least one I can access.", ephemeral: true);
            return;
        }
        if (!(Context.User as SocketGuildUser)!.GuildPermissions.ManageGuild)
        {
            await RespondAsync("You don't have the required `Manage Guild` permissions.", ephemeral: true);
            return;
        }
        if (pingOpponents == null && restrictToChannel == null && restrictToChannelEnabled == null && newgameBehaviour == null)
        {
            Embed embed = new EmbedBuilder()
            .WithDescription("""
- `ping-opponents` => Whether server members are mentioned when somebody challenges them.
- `restrict-to-channel` => Set a channel which your server members are only allowed to start games in.
- `restrict-to-channel-enabled` => Whether the previous setting is in effect.
- `newgame-behaviour` => How the bot behaves (ie, `Threadless` creates single-message games dumped into channel, `Thread` creates individual threads for games).
""")
            .Build();
            
            await RespondAsync(embed: embed, ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        using DatabaseContext db = Database.Create();
        ServerSettings? servermeta = await Database.GetGuild(Context.Guild.Id, db);

        if (pingOpponents != null)
            servermeta.PingOpponents = (bool)pingOpponents;
        
        if (restrictToChannel != null)
            servermeta.ChannelLockID = restrictToChannel.Id;

        if (restrictToChannelEnabled != null)
            servermeta.ChannelLock = (bool)restrictToChannelEnabled;

        if (newgameBehaviour != null)
            servermeta.State = (GameMetadata.GameState)newgameBehaviour;

        await FollowupAsync("Your settings have been applied");

        await db.SaveChangesAsync();
    }
}