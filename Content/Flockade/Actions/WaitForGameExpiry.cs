namespace CotLMinigames.Flockade;

public static partial class Actions
{
    public async static Task WaitForGameExpiry(UserContext Context, FLGameMetadata meta)
    {
        await Task.Delay(meta.GameExpiry * 1000);
        await EndGame(Context, meta);
    }
}
