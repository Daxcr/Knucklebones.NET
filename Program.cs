using CotLMinigames;

var bot = new BotClient();
await bot.Start(File.ReadAllText("token.txt"));