using Discord;
using Discord.WebSocket;

namespace CotLMinigames.Knucklebones;

public static partial class Actions
{
    public static async Task AdvanceLastMessage(UserContext Context, KBGameMetadata meta)
    {
        meta.CurrentDice = (byte)new Random().Next(1,7);

        if (meta.Guild == null)
            await Context.ModifyOriginalResponseAsync(async msg =>
            {
                msg.Components = await BuildGameComponents(meta);
            });
        else
            await Context.ModifyAsync(async msg =>
            {
                msg.Components = await BuildGameComponents(meta);
            });
    }
}
