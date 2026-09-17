using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public async static Task WaitForGameExpiry(KBGameMetadata meta, IUserMessage message)
    {
        await Task.Delay(meta.GameExpiry * 1000);
        EndGame(meta, message);
    }
}