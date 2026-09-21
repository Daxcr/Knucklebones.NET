using Discord.WebSocket;
using CotLMinigames.DB;

namespace CotLMinigames.Admin;

public partial class AdminCommands
{
    async public static Task GiveCoins(SocketMessage message)
    {
        string[] command = message.Content.Split('/');

        ulong userID = message.Author.Id;
        if (command.Length > 2)
            userID = ulong.Parse(command[2]);

        using DatabaseContext db = Database.Create();
        UserData? usermeta = await Database.GetUser(userID, db);

        usermeta.Inventory.Coins += long.Parse(command[1]);

        await message.Channel.SendMessageAsync($"Gave {command[1]} coins{BotClient.Emojis.Coin} to <@{userID}>");

        await db.SaveChangesAsync();
    }
}