using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace ClubFestBotCSharp;
internal class ASCIIConvertTelegramBot
{
    private readonly TelegramBotClient _botClient;

    public ASCIIConvertTelegramBot(string apiToken)
    {
        _botClient = new TelegramBotClient(apiToken);
    }

    public async Task StartHandlingAsync(CancellationToken cancellationToken)
    {
        await _botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandleError,
                cancellationToken: cancellationToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient
        , Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;
        try
        {
            if (update.Message.Text == "/start")
            {
                await HandleStartMessage(botClient, message, cancellationToken);
                return;
            }
            await HandleASCIIConvertion(botClient, message, cancellationToken);
        }
        catch (Exception ex)
        {
            var sendTask = SendErrorMessageToUser(botClient
                , message
                , Messages.InvalidArgumentsMessage
                , cancellationToken);
            var handleTask = HandleError(botClient, ex, cancellationToken);
            await Task.WhenAll(sendTask, handleTask);
        }
    }

    private async Task HandleStartMessage(ITelegramBotClient botClient
        , Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        await botClient.SendTextMessageAsync(chatId,
                Messages.StartMessage,
                cancellationToken: cancellationToken);
    }

    private async Task HandleASCIIConvertion(ITelegramBotClient botClient
        , Message message, CancellationToken cancellationToken)
    {
        if (message.Photo is not { } photos)
            return;
        if (string.IsNullOrEmpty(message.Caption))
            return;

        //Two numbers from caption
        var input = ParseCaption(message.Caption);

        if (!input.isValid)
        {
            await SendErrorMessageToUser(botClient
                , message
                , Messages.TooLargeInputMessage
                , cancellationToken);
            return;
        }

        //Load user's photo to stream
        using var inputStream = await LoadMessagePhotoAsync(botClient, photos, cancellationToken);

        //Create Bitmap from stream
        using var bitmap = new Bitmap(inputStream);
        //Create resized Bitmap
        using var ResisedBitmap = bitmap.ResizeBitmap(input.width);

        ResisedBitmap.ToGrayscale();

        //Convert Bitmap to ASCII symbols
        var converter = new BitmapConverter();
        var symbols = converter.ConvertToASCII(ResisedBitmap);

        //Draw ASCII symbols to Pdf
        var writer = new PdfASCIIImageDrawer(input.font, input.width);
        writer.Draw(symbols);

        //Convert Pdf to Png
        var pdfConverter = new PdfConverter();
        using var outputStream = pdfConverter.ConvertToPng(writer.Document);

        var chatId = message.Chat.Id;

        //Send modified photo back to users
        await botClient.SendPhotoAsync(chatId,
            InputFile.FromStream(outputStream),
            cancellationToken: cancellationToken);
    }

    private Task HandleError(ITelegramBotClient botClient, Exception exception
        , CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private async Task SendErrorMessageToUser(ITelegramBotClient botClient, Message message
        , string errorMessage, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        await botClient.SendTextMessageAsync(chatId, errorMessage,
            cancellationToken: cancellationToken);
    }

    private  (bool isValid, int font, int width) ParseCaption(string caption)
    {
        int font;
        var isValid = Int32.TryParse(caption, out font) && font is > 0 and < 1000;
        return (isValid, font, 1000 / font);
    }

    private async Task<Stream> LoadMessagePhotoAsync(ITelegramBotClient botClient
        , PhotoSize[] photos, CancellationToken cancellationToken)
    {
        // Get fileId on Telegram Server
        var fileId = photos.Last().FileId;

        //Write photo to Memory Stream
        var stream = new MemoryStream();
        var file = await botClient.GetInfoAndDownloadFileAsync(
            fileId: fileId,
            destination: stream,
            cancellationToken: cancellationToken);
        return stream;
    }
}
