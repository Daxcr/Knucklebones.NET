using CotLMinigames.DB;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AcceptGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];
        IUser user = component.User;
        KBGameMetadata? meta = (KBGameMetadata?)BotClient.Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.OpponentID != 0)
            {
                if (meta.OpponentID != user.Id)
                {
                    await component.RespondAsync("This isn't for you.", ephemeral: true);
                    return;
                }
            } else
            {
                if (meta.InitiatorID == user.Id)
                {
                    await component.RespondAsync("You can't challenge yourself!", ephemeral: true);
                    return;
                }
                meta.OpponentID = user.Id;
            }

            await component.DeferAsync();

            using var db = Database.Create();

            UserData opponentObj = await Database.GetUser(meta.OpponentID, db);
            if (opponentObj.Inventory.Coins < meta.Bet)
            {
                await component.FollowupAsync($"You don't have enough coins! {BotClient.Emojis.Coin} {opponentObj.Inventory.Coins}/{meta.Bet}");
                return;
            }
            opponentObj.Inventory.Coins -= meta.Bet;

            await db.SaveChangesAsync();

            IMessageChannel channel = component.Channel;

            IUser initiator = BotClient.Client.GetUser(meta.InitiatorID);
            IUser opponent = BotClient.Client.GetUser(meta.OpponentID);

            DateTimeOffset offset = DateTimeOffset.UtcNow.AddSeconds(meta.GameExpiry);
            meta.GameExpiryDisplay = TimestampTag.FromDateTimeOffset(offset, TimestampTagStyles.Relative);
            meta.GameStarted = true;

            Random rnd = new();
            meta.InitiatorTurn = rnd.Next(0, 2) == 0;

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Accepted)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was accepted {expiry}.\n\nBet: {BotClient.Emojis.Coin} {meta.Bet}")
                .WithColor(Color.Green)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Success, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
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

            if (component.Channel is SocketGuildChannel guildChannel && guildChannel.Guild != null)
            {
                RestUserMessage temp = (RestUserMessage)await meta.Channel!.SendMessageAsync($"<@{meta.InitiatorID}><@{meta.OpponentID}>"); // ghost ping! :D
                await temp.DeleteAsync();
            }
        }
    }
}