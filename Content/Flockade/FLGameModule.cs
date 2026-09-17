using Discord;
using Discord.Interactions;

namespace CotLMinigames.Flockade;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class FLGameModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int TurnExpiry = 300;

    [SlashCommand("flockade", "Challenge somebody to a game of Flockade")]
    public async Task Flockade(IUser user,
        [Choice("None", 0)]
        [Choice("1 wool (Small)", 1)]
        [Choice("2 wool (Small)", 2)]
        [Choice("3 wool (Small)", 3)]
        [Choice("4 wool (Small)", 4)]
        [Choice("5 wool (Small)", 5)]
        [Choice("6 wool (Small)", 6)]
        [Choice("7 wool (Small)", 7)]
        [Choice("8 wool (Small)", 8)]
        [Choice("9 wool (Small)", 9)]
        [Choice("10 wool (Large)", 10)]
        [Choice("12 wool (Large)", 12)]
        [Choice("15 wool (Large)", 15)]
        [Choice("20 wool (Large)", 20)]
        [Choice("25 wool (Large)", 25)]
        [Choice("30 wool (Massive)", 30)]
        [Choice("35 wool (Massive)", 35)]
        [Choice("40 wool (Massive)", 40)]
        [Choice("50 wool (Massive)", 50)]
        int bet = 0
    )
    {
        if (Context.User.Id == user.Id)
        {
            await RespondAsync("You can't challenge yourself! What a disappointment.", ephemeral: true);
            return;
        }

        await RespondAsync("Coming soon...");
    }
}