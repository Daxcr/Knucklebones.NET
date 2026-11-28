using CotLMinigames.Knucklebones;
using Discord;

namespace CotLMinigames;

public static class MessageBuilder
{
    public enum InviteState { Pending, Cancelled, Declined, Expired }
    private static readonly Dictionary<InviteState, Color> InviteColours = new()
    {
        { InviteState.Cancelled, Color.DarkerGrey },
        { InviteState.Declined, Color.Red },
        { InviteState.Expired, Color.LightGrey },
    };

    public static async Task<MessageComponent> BuildInviteComponents(GameMetadata meta, InviteState state, TimestampTag expiry)
    {
        string game = meta is KBGameMetadata ? "Knucklebones" : "Flockade";

        string response = string.Empty;
        string expiremsg = string.Empty;

        switch (state)
        {
            case InviteState.Pending:
                expiremsg = $"This request will expire {expiry}.";
                break;

            case InviteState.Cancelled:
                expiremsg = $"This request was cancelled {expiry}.";
                response = "(Cancelled)";
                break;

            case InviteState.Declined:
                expiremsg = $"This request was declined {expiry}.";
                response = "(Declined)";
                break;

            case InviteState.Expired:
                expiremsg = $"This request expired {expiry}.";
                response = "(Expired)";
                break;
        }

        ContainerBuilder container = new ContainerBuilder()
            .WithTextDisplay($"## {game} request {response}")
            .WithSeparator();

        ActionRowBuilder actionrow = new ActionRowBuilder();

        if (meta.OpponentID == 0)
        {
            container
                .WithTextDisplay($"<@{meta.InitiatorID}> would like to play a game of {game}.\n" + expiremsg)
                .WithTextDisplay($"> Bet: {(meta is KBGameMetadata ? BotClient.Emojis.Coin : BotClient.Emojis.Wool)} {meta.Bet}");

            actionrow
                .AddComponent(new ButtonBuilder()
                    .WithLabel("Accept")
                    .WithCustomId($"acceptkb/{meta.ID}")
                    .WithStyle(state == InviteState.Pending ? ButtonStyle.Success : ButtonStyle.Secondary)
                    .WithDisabled(state != InviteState.Pending))
                .AddComponent(new ButtonBuilder()
                    .WithLabel("Challenge the Bot")
                    .WithCustomId($"challengebotkb/{meta.ID}")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithDisabled(state != InviteState.Pending))
                .AddComponent(new ButtonBuilder()
                    .WithLabel("Cancel")
                    .WithCustomId($"cancelkb/{meta.ID}")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithDisabled(state != InviteState.Pending));
        }
        else
        {
            if (meta.OpponentID == BotClient.Client.CurrentUser.Id)
            {
                container.WithTextDisplay($"<@{meta.InitiatorID}> would like to play a game of {game} against me.\n" + expiremsg);
                actionrow
                    .AddComponent(new ButtonBuilder()
                        .WithLabel("Go!")
                        .WithCustomId($"acceptbotkb/{meta.ID}")
                        .WithStyle(state == InviteState.Pending ? ButtonStyle.Success : ButtonStyle.Secondary)
                        .WithDisabled(state != InviteState.Pending));
            }
            else
            {
                container.WithTextDisplay($"<@{meta.OpponentID}> is challenged to play a game of {game} against <@{meta.InitiatorID}>.\n" + expiremsg);
                actionrow
                    .AddComponent(new ButtonBuilder()
                        .WithLabel("Accept")
                        .WithCustomId($"acceptkb/{meta.ID}")
                        .WithStyle(state == InviteState.Pending ? ButtonStyle.Success : ButtonStyle.Secondary)
                        .WithDisabled(state != InviteState.Pending))
                    .AddComponent(new ButtonBuilder()
                        .WithLabel("Decline")
                        .WithCustomId($"declinekb/{meta.ID}")
                        .WithStyle(state == InviteState.Pending ? ButtonStyle.Danger : ButtonStyle.Secondary)
                        .WithDisabled(state != InviteState.Pending))
                    .AddComponent(new ButtonBuilder()
                        .WithLabel("Cancel")
                        .WithCustomId($"cancelkb/{meta.ID}")
                        .WithStyle(ButtonStyle.Secondary)
                        .WithDisabled(state != InviteState.Pending));
            }

            container.WithTextDisplay($"> Bet: {(meta is KBGameMetadata ? BotClient.Emojis.Coin : BotClient.Emojis.Wool)} {meta.Bet}");
        }

        container.WithActionRow(actionrow);

        if (InviteColours.TryGetValue(state, out Color value))
            container.WithAccentColor(value);

        ComponentBuilderV2 component = new ComponentBuilderV2()
            .WithContainer(container);

        if (meta is KBGameMetadata)
        {
            component.AddComponent(new ActionRowBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("What's Knucklebones?")
                    .WithCustomId("howtokb/0/false")
                    .WithStyle(ButtonStyle.Secondary)));
        }

        return component.Build();
    }
}
