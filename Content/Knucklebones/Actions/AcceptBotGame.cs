using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AcceptBotGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];
        IUser user = component.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.InitiatorID != user.Id)
            {
                await component.RespondAsync("This isn't for you.", ephemeral: true);
                return;
            }

            await component.DeferAsync();

            IMessageChannel channel = component.Channel;

            IUser initiator = BotClient.Client.GetUser(meta.InitiatorID);
            IUser opponent = BotClient.Client.GetUser(meta.OpponentID);

            DateTimeOffset offset = DateTimeOffset.UtcNow.AddSeconds(meta.GameExpiry);
            meta.GameExpiryDisplay = TimestampTag.FromDateTimeOffset(offset, TimestampTagStyles.Relative);
            meta.GameStarted = true;

            Random rnd = new();
            meta.InitiatorTurn = true;

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Accepted)")
                .WithDescription($"<@{meta.InitiatorID}> is wanting to challenge me.\nThis request was accepted {expiry}.")
                .WithColor(Color.Green)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Go", $"accept/disabled", ButtonStyle.Success, disabled: true)
                .Build();

            await component.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed;
                message.Components = disabledComponents;
            });

            Embed initiatorembed = await BuildPlayerEmbed(meta, true, meta.InitiatorTurn);
            Embed opponentembed = await BuildPlayerEmbed(meta, false, meta.InitiatorTurn);          

            meta.CurrentDice = (byte)new Random().Next(1,7);
            Embed diceEmbed = BuildDiceEmbed(meta);

            MessageComponent gameActions = new ComponentBuilder()
                .WithButton("Left", $"playkb/left/{meta.ID}/1", ButtonStyle.Primary)
                .WithButton("Middle", $"playkb/middle/{meta.ID}/1", ButtonStyle.Primary)
                .WithButton("Right", $"playkb/right/{meta.ID}/1", ButtonStyle.Primary)
                .WithButton("Forfeit", $"playkb/forfeit/{meta.ID}/0", ButtonStyle.Danger)
                .Build();

            IUserMessage msg;

            if (meta.Guild == null)
            {
                msg = await component.FollowupAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
            }
            else
            {
                msg = await component.Message.ReplyAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
            }

            _ = WaitForGameExpiry(meta, msg);
        }
    }
}