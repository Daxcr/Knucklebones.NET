using Discord.WebSocket;
using CotLMinigames.DB;

namespace CotLMinigames.Admin;

public partial class AdminCommands
{
    async public static Task AddDevotion(SocketMessage message)
    {
        string[] command = message.Content.Split('/');

        ulong userID = message.Author.Id;
        if (command.Length > 2)
            userID = ulong.Parse(command[2]);

        using DatabaseContext db = Database.Create();
        UserData? usermeta = await Database.GetUser(userID, db);

        usermeta.AddDevotion(long.Parse(command[1]));
        await message.Channel.SendMessageAsync($"Gave {command[1]} devotion{BotClient.Emojis.Devotion} to <@{userID}>");
        await db.SaveChangesAsync();
    }
}