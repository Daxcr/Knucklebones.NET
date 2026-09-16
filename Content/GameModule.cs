using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Knucklebones.DB;

namespace Knucklebones;

[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
public class GameModule : InteractionModuleBase<SocketInteractionContext>
{
    public const int ChallengeExpiry = 120;
    public const int TurnExpiry = 300;
    public const int DevotionOnWin = 25;
    public static List<GameMetadata> Games = new();

    [SlashCommand("challenge", "Challenge somebody to a game of Knucklebones")]
    public async Task Challenge(IUser user,
        [Choice("None", 0)]
        [Choice("5 coins (Small)", 5)]
        [Choice("10 coins (Small)", 10)]
        [Choice("20 coins (Small)", 20)]
        [Choice("30 coins (Small)", 30)]
        [Choice("40 coins (Small)", 40)]
        [Choice("50 coins (Large)", 50)]
        [Choice("75 coins (Large)", 75)]
        [Choice("100 coins (Large)", 100)]
        [Choice("150 coins (Large)", 150)]
        [Choice("200 coins (Massive)", 200)]
        [Choice("250 coins (Massive)", 250)]
        [Choice("300 coins (Massive)", 300)]
        [Choice("400 coins (Massive)", 400)]
        [Choice("500 coins (Massive)", 500)]
        [Choice("750 coins (Massive)", 750)]
        [Choice("1000 coins (Massive)", 1000)]
        [Choice("2000 coins (Massive)", 2000)]
        [Choice("5000 coins (Massive)", 5000)]
        int bet = 0
    )
    {
        if (Context.User.Id == user.Id)
        {
            await RespondAsync("You can't challenge yourself!", ephemeral: true);
            return;
        }
        await DeferAsync();

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(Context.User.Id, db);
        UserData? opponent = await Database.GetUser(user.Id, db);

        if (initiator.Coins < bet)
        {
            await FollowupAsync($"You don't have enough coins: `{initiator.Coins}/{bet}`");
            return;
        }

        if (opponent.Coins < bet)
        {
            await FollowupAsync($"Your opponent does not have enough coins: `{opponent.Coins}/{bet}`");
            return;
        }

        if (!opponent.AcceptingGames)
        {
            await FollowupAsync($"Your opponent isn't accepting games right now.");
            return;
        }

        bool shortGame = false;

        GameMetadata.GameState state;
        if (Context.Guild == null)
        {
            state = GameMetadata.GameState.Threadless;
            shortGame = true;
        } else
        {
            ServerSettings? servermeta = await Database.GetGuild(Context.Guild.Id, db);
            state = servermeta.State;
        }

        GameMetadata meta = new()
        {
            InitiatorID = Context.User.Id,
            OpponentID = user.Id,
            InitiatedChannelID = Context.Channel?.Id,
            State = state,
            ShortGame = shortGame
        };

        DateTimeOffset expiryoffset = DateTimeOffset.UtcNow.AddSeconds(ChallengeExpiry);
        TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);
        Embed embed = new EmbedBuilder()
            .WithTitle("Match request")
            .WithDescription($"<@{user.Id}> has been challenged to a game of Knucklebones by <@{Context.User.Id}>.\nThis request will expire {expiry}.")
            .WithColor(Color.Blue)
            .Build();

        MessageComponent components = new ComponentBuilder()
            .WithButton("Accept", $"acceptgame/{meta.ID}", ButtonStyle.Success)
            .WithButton("Decline", $"declinegame/{meta.ID}", ButtonStyle.Danger)
            .Build();

        string ping = string.Empty;

        if (Context.Guild != null)
        {
            ServerSettings servermeta = await Database.GetGuild(Context.Guild.Id, db);
            ping = servermeta.PingOpponents ? $"<@{user.Id}>" : string.Empty;
        }


        await FollowupAsync(ping, embed: embed, components: components);
        IUserMessage message = await GetOriginalResponseAsync();
        _ = WaitForChallengeExpiry(meta, Context.Interaction, Context.User.Id, user.Id, expiry, bet);

        initiator.Coins -= bet;
        Console.WriteLine(initiator.Coins);
        await db.SaveChangesAsync();
    }

    public async static Task WaitForChallengeExpiry(GameMetadata meta, SocketInteraction interaction, ulong initiatorID, ulong opponentID, TimestampTag expiry, int bet)
    {
        await Task.Delay(ChallengeExpiry * 1000);

        using DatabaseContext db = Database.Create();
        UserData? initiator = await Database.GetUser(initiatorID, db);

        await db.SaveChangesAsync();

        if (meta != null && !meta.GameStarted && !meta.GameDeclined)
        {
            initiator.Coins += bet;
            
            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Expired)")
                .WithDescription($"<@{opponentID}> has been challenged to a game of Knucklebones by <@{initiatorID}>.\nThis request expired {expiry}.")
                .WithColor(Color.DarkerGrey)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Secondary, disabled: true)
                .Build();

            await interaction.ModifyOriginalResponseAsync(message =>
            {
                message.Embed = embed;
                message.Components = disabledComponents;
            });

            Games.Remove(meta);
        }
    }

    public static async Task AcceptGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];
        IUser user = component.User;
        GameMetadata? meta = Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.OpponentID != user.Id)
            {
                await component.RespondAsync("This isn't for you.", ephemeral: true);
                return;
            }

            await component.DeferAsync();

            var db = Database.Create();

            UserData opponentObj = await Database.GetUser(meta.OpponentID, db);
            opponentObj.Coins -= meta.Bet;

            await db.SaveChangesAsync();

            IMessageChannel channel = component.Channel;

            IUser initiator = KnucklebonesBot.Client.GetUser(meta.InitiatorID);
            IUser opponent = KnucklebonesBot.Client.GetUser(meta.OpponentID);

            if (meta.State == GameMetadata.GameState.Thread && channel is SocketTextChannel textChannel)
            {
                SocketThreadChannel thread = await textChannel.CreateThreadAsync(
                    name: $"{initiator.Username} v. {opponent.Username} | {meta.ID}",
                    type: ThreadType.PublicThread,
                    autoArchiveDuration: ThreadArchiveDuration.OneHour
                );

                meta.Channel = thread;
            }
            else
            {
                meta.Channel = channel;
            }
            meta.GameStarted = true;

            Random rnd = new();
            meta.InitiatorTurn = rnd.Next(0, 2) == 0;

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Accepted)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was accepted {expiry}.")
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

            Embed initiatorembed = await BuildPlayerEmbed(meta, true);
            Embed opponentembed = await BuildPlayerEmbed(meta, false);          

            meta.CurrentDice = (byte)new Random().Next(1,7);
            Embed diceEmbed = new EmbedBuilder()
                .WithDescription($"""
<@{(meta.InitiatorTurn ? meta.InitiatorID : meta.OpponentID)}>'s turn.
# {GameMetadata.DiceEmojis[$"dice{meta.CurrentDice}_single"]}
""")
                .Build();

            MessageComponent gameActions = new ComponentBuilder()
                .WithButton("Left", $"play/left/{meta.ID}", ButtonStyle.Primary)
                .WithButton("Middle", $"play/middle/{meta.ID}", ButtonStyle.Primary)
                .WithButton("Right", $"play/right/{meta.ID}", ButtonStyle.Primary)
                .Build();

            if (meta.State == GameMetadata.GameState.Thread)
                await meta.Channel!.SendMessageAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
            else
                if (meta.ShortGame)
                    await component.FollowupAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
                else
                    await component.Message.ReplyAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);

            if (component.Channel is SocketGuildChannel guildChannel && guildChannel.Guild != null)
            {
                RestUserMessage temp = (RestUserMessage)await meta.Channel!.SendMessageAsync($"<@{meta.InitiatorID}><@{meta.OpponentID}>"); // ghost ping! :D
                await temp.DeleteAsync();
            }
        }
    }

    public static async Task DeclineGame(string[] ButtonData, SocketMessageComponent component)
    {
        if (ButtonData.Length < 2)
            return;

        string gameID = ButtonData[1];
        IUser user = component.User;
        GameMetadata? meta = Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta != null)
        {
            if (meta.OpponentID != user.Id)
            {
                await component.RespondAsync("This isn't for you.", ephemeral: true);
                return;
            }

            await component.DeferAsync();

            using DatabaseContext db = Database.Create();
            UserData? initiator = await Database.GetUser(meta.InitiatorID, db);

            initiator.Coins += meta.Bet;

            await db.SaveChangesAsync();

            DateTimeOffset expiryoffset = DateTimeOffset.UtcNow;
            TimestampTag expiry = TimestampTag.FromDateTimeOffset(expiryoffset, TimestampTagStyles.Relative);

            Embed embed = new EmbedBuilder()
                .WithTitle("Match request (Declined)")
                .WithDescription($"<@{meta.OpponentID}> has been challenged to a game of Knucklebones by <@{meta.InitiatorID}>.\nThis request was declined {expiry}.")
                .WithColor(Color.Red)
                .Build();
                
            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Accept", $"accept/disabled", ButtonStyle.Secondary, disabled: true)
                .WithButton("Decline", $"decline/disabled", ButtonStyle.Danger, disabled: true)
                .Build();

            if (meta.ShortGame)
                await component.ModifyOriginalResponseAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });
            else
                await component.Message.ModifyAsync(message =>
                {
                    message.Embed = embed;
                    message.Components = disabledComponents;
                });

            Games.Remove(meta);
            meta.GameDeclined = true;
        }
    }

    public static async Task PlayMove(string[] ButtonData, SocketMessageComponent component)
    {
        string gameID = ButtonData[2];
        GameMetadata? meta = Games.FirstOrDefault(item => item.ID == gameID);
        
        if (meta == null)
            return;

        if (component.User.Id != meta.InitiatorID && component.User.Id != meta.OpponentID)
        {
            await component.RespondAsync("Not your game", ephemeral: true);
            return;
        }

        if ((component.User.Id != meta.InitiatorID && meta.InitiatorTurn) || (component.User.Id == meta.InitiatorID && !meta.InitiatorTurn))
        {
            await component.RespondAsync("Not your turn", ephemeral: true);
            return;
        }

        await component.DeferAsync();
        string pressedbutton = ButtonData[1];

        if (meta.State == GameMetadata.GameState.Thread)
        {
            ButtonStyle leftStyle = pressedbutton == "left" ? ButtonStyle.Primary : ButtonStyle.Secondary;
            ButtonStyle middleStyle = pressedbutton == "middle" ? ButtonStyle.Primary : ButtonStyle.Secondary;
            ButtonStyle rightStyle = pressedbutton == "right" ? ButtonStyle.Primary : ButtonStyle.Secondary;

            MessageComponent disabledComponents = new ComponentBuilder()
                .WithButton("Left", $"play/left/disabled", leftStyle, disabled: true)
                .WithButton("Middle", $"play/middle/disabled", middleStyle, disabled: true)
                .WithButton("Right", $"play/right/disabled", rightStyle, disabled: true)
                .Build();

            await component.Message.ModifyAsync(message =>
            {
                message.Components = disabledComponents;
            });

            if (meta.InitiatorTurn)
            {
                meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
                meta.OpponentTableDiff = meta.OpponentTable.Clone();
                meta.InitiatorTable.Add(pressedbutton, meta.CurrentDice, meta.OpponentTable);
            }
            else
            {
                meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
                meta.OpponentTableDiff = meta.OpponentTable.Clone();
                meta.OpponentTable.Add(pressedbutton, meta.CurrentDice, meta.InitiatorTable);
            }

            if (meta.InitiatorTable.IsFull() || meta.OpponentTable.IsFull())
            {
                EndGame(meta, component);
                return;
            }

            meta.Turn += 1;
            meta.InitiatorTurn = !meta.InitiatorTurn;

            Embed initiatorembed = await BuildPlayerEmbed(meta, true);
            Embed opponentembed = await BuildPlayerEmbed(meta, false);          

            meta.CurrentDice = (byte)new Random().Next(1,7);
            Embed diceEmbed = new EmbedBuilder()
                .WithDescription($"""
<@{(meta.InitiatorTurn ? meta.InitiatorID : meta.OpponentID)}>'s turn.
# {GameMetadata.DiceEmojis[$"dice{meta.CurrentDice}_single"]}
""")
                .Build();

            MessageComponent gameActions = await BuildGameActions(meta, meta.InitiatorTurn);

            await component.Message.ReplyAsync(embeds: [initiatorembed, opponentembed, diceEmbed], components: gameActions);
        }
        else
        {
            if (meta.InitiatorTurn)
            {
                meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
                meta.OpponentTableDiff = meta.OpponentTable.Clone();
                meta.InitiatorTable.Add(pressedbutton, meta.CurrentDice, meta.OpponentTable);
            }
            else
            {
                meta.InitiatorTableDiff = meta.InitiatorTable.Clone();
                meta.OpponentTableDiff = meta.OpponentTable.Clone();
                meta.OpponentTable.Add(pressedbutton, meta.CurrentDice, meta.InitiatorTable);
            }

            if (meta.InitiatorTable.IsFull() || meta.OpponentTable.IsFull())
            {
                EndGame(meta, component);
                return;
            }

            meta.Turn += 1;
            meta.InitiatorTurn = !meta.InitiatorTurn;

            Embed initiatorembed = await BuildPlayerEmbed(meta, true);
            Embed opponentembed = await BuildPlayerEmbed(meta, false);          

            meta.CurrentDice = (byte)new Random().Next(1,7);
            Embed diceEmbed = new EmbedBuilder()
                .WithDescription($"""
<@{(meta.InitiatorTurn ? meta.InitiatorID : meta.OpponentID)}>'s turn.
# {GameMetadata.DiceEmojis[$"dice{meta.CurrentDice}_single"]}
""")
                .Build();

            MessageComponent gameActions = await BuildGameActions(meta, meta.InitiatorTurn);

            if (meta.ShortGame)
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                    msg.Components = gameActions;
                });
            else
                await component.Message.ModifyAsync(msg =>
                {
                    msg.Embeds = new Embed[] { initiatorembed, opponentembed, diceEmbed };
                    msg.Components = gameActions;
                });
        }
    }

    public static async Task<Embed> BuildPlayerEmbed(GameMetadata meta, bool initiator) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, !initiator)}
**Points:** {meta.BuildPoints(initiator)}
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor((initiator && meta.InitiatorTurn) || (!initiator && !meta.InitiatorTurn) ? Color.LighterGrey : Color.Default)
            .Build();

    public static async Task<Embed> BuildEndPlayerEmbed(GameMetadata meta, bool initiator, bool winner) =>
        new EmbedBuilder()
            .WithDescription($"""
{meta.BuildTable(initiator ? meta.InitiatorTable : meta.OpponentTable, initiator ? meta.InitiatorTableDiff : meta.OpponentTableDiff, !initiator)}
**Points:** {meta.BuildPoints(initiator)}
""")
            .WithThumbnailUrl(await ProfileModule.GetProfilePicture(initiator ? meta.InitiatorID : meta.OpponentID))
            .WithColor(winner ? Color.Gold : Color.Default)
            .Build();

    public static async Task<MessageComponent> BuildGameActions(GameMetadata meta, bool initiator)
    {
        GameMetadata.Table table = initiator ? meta.InitiatorTable : meta.OpponentTable;
        return new ComponentBuilder()
            .WithButton("Left", $"play/left/{meta.ID}", ButtonStyle.Primary, disabled: !table.Left.Contains(0))
            .WithButton("Middle", $"play/middle/{meta.ID}", ButtonStyle.Primary, disabled: !table.Middle.Contains(0))
            .WithButton("Right", $"play/right/{meta.ID}", ButtonStyle.Primary, disabled: !table.Right.Contains(0))
            .Build();
    }

    public static async void EndGame(GameMetadata meta, SocketMessageComponent component)
    {
        Games.Remove(meta);

        int initiatorScore = meta.BuildPoints(true);
        int opponentScore = meta.BuildPoints(false);

        Embed initiatorembed;
        Embed opponentembed;
        Embed? devotionEmbed = null;

        if (initiatorScore == opponentScore)
        {
            initiatorembed = await BuildEndPlayerEmbed(meta, true, false);
            opponentembed = await BuildEndPlayerEmbed(meta, false, false);   
        } else
        {
            devotionEmbed = await CalculateBetsAndDevotion(meta, initiatorScore > opponentScore);

            if (initiatorScore > opponentScore)
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, true);
                opponentembed = await BuildEndPlayerEmbed(meta, false, false);  
            } else
            {
                initiatorembed = await BuildEndPlayerEmbed(meta, true, false);
                opponentembed = await BuildEndPlayerEmbed(meta, false, true);
            }
        }

        if (meta.State == GameMetadata.GameState.Thread)
        {
            await component.Message.ReplyAsync(embeds: [initiatorembed, opponentembed, devotionEmbed]);
            await (meta.Channel as SocketThreadChannel)!.ModifyAsync(properties =>
            {
                properties.Archived = true;
            });
        } else
        {
            if (meta.ShortGame)
                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Components = null;
                    if (devotionEmbed != null)
                        msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                    else
                        msg.Embeds = new Embed[] { initiatorembed, opponentembed };
                });
            else
                await component.Message.ModifyAsync(msg =>
                {
                    msg.Components = null;
                    if (devotionEmbed != null)
                        msg.Embeds = new Embed[] { initiatorembed, opponentembed, devotionEmbed };
                    else
                        msg.Embeds = new Embed[] { initiatorembed, opponentembed };
                });
        }
    }

    async public static Task<Embed> CalculateBetsAndDevotion(GameMetadata meta, bool initiatorWin)
    {
        var db = Database.Create();

        UserData initiator = await Database.GetUser(meta.InitiatorID, db);
        UserData opponent = await Database.GetUser(meta.OpponentID, db);

        UserData winner = initiatorWin ? initiator : opponent;
        UserData loser = initiatorWin ? opponent : initiator;

        int addedDevotion = DevotionOnWin + (meta.Bet * 4);

        string bar = ProfileModule.CalculateDevotionBar(
            ProfileModule.DevotionBarWidth,
            winner.Devotion + addedDevotion,
            ProfileModule.CalculateMaxDevotion(winner.Level)
        );

        winner.Coins += meta.Bet * 2;
        winner.AddDevotion(addedDevotion);

        await db.SaveChangesAsync();

        return new EmbedBuilder()
            .WithDescription($"""
**Winner:** <@{winner.UserID}>
{ProfileModule.GenericEmojis["coin"]} Coins: +{meta.Bet} ({winner.Coins})
{ProfileModule.GenericEmojis["devotion"]} Devotion: +{addedDevotion}
{bar}

**Loser:** <@{loser.UserID}>
{ProfileModule.GenericEmojis["coin"]} Coins: -{meta.Bet} ({winner.Coins})
""")
            .WithColor(Color.Default)
            .Build();
    }
}