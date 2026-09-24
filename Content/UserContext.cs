using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames;

public class UserContext
{
    SocketMessageComponent? ComponentContext;
    SocketInteractionContext? InteractionContext;
    IUserMessage? MessageContext;
    public enum CtxType { Component, Interaction, Message }
    public CtxType ContextType;
    public SocketUser? User
    {
        get
        {
            switch (ContextType)
            {
                case CtxType.Interaction:
                    return InteractionContext?.User;

                case CtxType.Component:
                    return ComponentContext?.User;

                case CtxType.Message:
                    return (SocketUser?)(MessageContext?.Author);
            }
            return null;
        }
    }

    public ISocketMessageChannel? Channel
    {
        get
        {
            switch (ContextType)
            {
                case CtxType.Interaction:
                    return InteractionContext?.Channel;

                case CtxType.Component:
                    return InteractionContext?.Channel;

                case CtxType.Message:
                    return (ISocketMessageChannel?)(MessageContext?.Channel);
            }
            return null;
        }
    }

    public SocketGuild? Guild
    {
        get
        {
            switch (ContextType)
            {
                case CtxType.Interaction:
                    return InteractionContext?.Guild;

                case CtxType.Component:
                    return (ComponentContext?.User as SocketGuildUser)?.Guild;

                case CtxType.Message:
                    return (MessageContext?.Author as SocketGuildUser)?.Guild;
            }
            return null;
        }
    }

    public UserContext(SocketMessageComponent context)
    {
        ComponentContext = context;
        ContextType = CtxType.Component;
    }

    public UserContext(SocketInteractionContext context)
    {
        InteractionContext = context;
        ContextType = CtxType.Interaction;
    }

    public UserContext(IUserMessage context)
    {
        MessageContext = context;
        ContextType = CtxType.Message;
    }

    public async Task DeferAsync(bool ephemeral = false)
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                await InteractionContext?.Interaction.DeferAsync(ephemeral: ephemeral)!;
                break;

            case CtxType.Component:
                await ComponentContext?.DeferAsync(ephemeral: ephemeral)!;
                break;
        }
    }

    public async Task<RestFollowupMessage?> FollowupAsync(
        string? text = null,
        Embed? embed = null,
        Embed[]? embeds = null,
        bool ephemeral = false,
        MessageComponent? components = null
    )
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                return await InteractionContext?.Interaction.FollowupAsync(text: text, embed: embed, embeds: embeds, ephemeral: ephemeral, components: components)!;

            case CtxType.Component:
                return await ComponentContext?.FollowupAsync(text: text, embed: embed, embeds: embeds, ephemeral: ephemeral, components: components)!;

            case CtxType.Message:
                return (RestFollowupMessage?)await MessageContext?.ReplyAsync(text: text, embed: embed, embeds: embeds, components: components)!;
        }
        return null;
    }

    public async Task<RestInteractionMessage?> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions? options = null)
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                return await InteractionContext?.Interaction.ModifyOriginalResponseAsync(func, options)!;

            case CtxType.Component:
                return await ComponentContext?.ModifyOriginalResponseAsync(func, options)!;

            case CtxType.Message:
                await MessageContext?.ModifyAsync(func, options)!;
                break;
        }
        return null;
    }

    public async Task ModifyAsync(Action<MessageProperties> func, RequestOptions? options = null)
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                await InteractionContext?.Interaction.ModifyOriginalResponseAsync(func, options)!;
                break;

            case CtxType.Component:
                await ComponentContext?.Message.ModifyAsync(func, options)!;
                break;

            case CtxType.Message:
                await (MessageContext as RestFollowupMessage)?.ModifyAsync(func, options)!;
                break;
        }
    }

    public async Task RespondAsync(
        string? text = null,
        Embed? embed = null,
        Embed[]? embeds = null,
        bool ephemeral = false,
        MessageComponent? components = null
    )
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                await InteractionContext?.Interaction.RespondAsync(text: text, embed: embed, embeds: embeds, ephemeral: ephemeral, components: components)!;
                break;

            case CtxType.Component:
                await ComponentContext?.RespondAsync(text: text, embed: embed, embeds: embeds, ephemeral: ephemeral, components: components)!;
                break;

            case CtxType.Message:
                await MessageContext?.ReplyAsync(text: text, embed: embed, embeds: embeds, components: components)!;
                break;
        }
    }

    public async Task<IUserMessage?> GetOriginalResponseAsync()
    {
        switch (ContextType)
        {
            case CtxType.Interaction:
                return await InteractionContext?.Interaction.GetOriginalResponseAsync()!;

            case CtxType.Component:
                return await ComponentContext?.GetOriginalResponseAsync()!;

            case CtxType.Message:
                return MessageContext;
        }
        return null;
    }
}