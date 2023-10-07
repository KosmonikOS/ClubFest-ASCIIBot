using ClubFestBotCSharp;
using System.Text;

//Register encoding
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var bot = new ASCIIConvertTelegramBot("API Key");

using CancellationTokenSource cts = new();

await bot.StartHandlingAsync(cts.Token);

Console.ReadLine();

cts.Cancel();
