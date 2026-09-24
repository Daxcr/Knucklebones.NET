using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public async static Task WaitForGameExpiry(UserContext Context, KBGameMetadata meta)
    {
        await Task.Delay(meta.GameExpiry * 1000);
        await EndGame(Context, meta);
    }
}