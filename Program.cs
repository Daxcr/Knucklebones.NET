using Knucklebones;

var bot = new KnucklebonesBot();
await bot.Start(File.ReadAllText("token.txt"));