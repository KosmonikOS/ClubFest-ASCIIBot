using ClubFestBotCSharp;
using System.Text;

//Register encoding
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var bot = new ASCIIConvertTelegramBot("6525005958:AAFiIxZcJ9LrVRt62G-bvKclUfpkF0MZOrA");

using CancellationTokenSource cts = new();

await bot.StartHandlingAsync(cts.Token);

Console.ReadLine();

cts.Cancel();
