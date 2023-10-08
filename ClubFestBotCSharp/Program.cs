using ClubFestBotCSharp;
using System.Text;

//Register encoding
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var bot = new ASCIIConvertTelegramBot("6363511412:AAEvE7XPKt4aYVZnMGApLhEN0lRJ37YE-v4");

using CancellationTokenSource cts = new();

await bot.StartHandlingAsync(cts.Token);

Console.ReadLine();

cts.Cancel();
