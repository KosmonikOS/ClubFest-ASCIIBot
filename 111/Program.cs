using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PDFtoImage;
using System.Drawing;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

//Register encoding
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var botClient = new TelegramBotClient("Your API Key");

using CancellationTokenSource cts = new();

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    cancellationToken: cts.Token
);

Console.ReadLine();

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    if (string.IsNullOrEmpty(message.Caption))
        return;
    if (message.Photo is not { } photos)
        return;

    var chatId = message.Chat.Id;
    //Load user's photo to stream
    using var inputStream = await LoadMessagePhotoAsync(botClient, photos, cancellationToken);

    //Two numbers from caption
    var input = ParseCaption(message.Caption);

    //Create Bitmap from stream
    using var bitmap = new Bitmap(inputStream);
    //Create resized Bitmap
    using var ResisedBitmap = bitmap.ResizeBitmap(input.width);

    ResisedBitmap.ToGrayscale();

    //Convert Bitmap to ASCII symbols
    var converter = new BitmapToASCIIConverter(ResisedBitmap);
    var symbols = converter.Convert();

    //Draw ASCII symbols to Pdf
    var writer = new PdfASCIIImageDrawer(input.font,input.width);
    writer.Draw(symbols);

    //Convert Pdf to Png
    using var outputStream = ConvertPdfToPng(writer.Document);

    //Send modified photo back to users
    await botClient.SendPhotoAsync(chatId,
        InputFile.FromStream(outputStream),
        cancellationToken: cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

(int font, int width) ParseCaption(string caption)
{
    var parsed = caption
        .Split(" ")
        .Select(Int32.Parse)
        .ToArray();
    return (parsed[0], parsed[1]);
}

async Task<Stream> LoadMessagePhotoAsync(ITelegramBotClient botClient
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

MemoryStream ConvertPdfToPng(PdfDocument document)
{
    using var pdfStream = new MemoryStream();

    document.Save(pdfStream, false);

    var pngStream = new MemoryStream();

    Conversion.SavePng(pngStream, pdfStream.ToArray());

    pngStream.Position = 0;

    return pngStream;
}

internal class PdfASCIIImageDrawer
{
    public PdfDocument Document { get; private set; }
    private readonly PdfPage _page;
    private readonly XGraphics _gfx;
    private readonly XFont _font;
    private readonly int _fontSize;
    private readonly int _width;

    public PdfASCIIImageDrawer(int fontSize,int width)
    {
        Document = new PdfDocument();
        _page = Document.AddPage();
        _gfx = XGraphics.FromPdfPage(_page);
        _font = new XFont("Consolas", fontSize, XFontStyle.BoldItalic);
        _fontSize = fontSize;
        _width = width;
    }

    public void Draw(char[][] symbols)
    {
        double otstup = 1.1 * _font.Size;
        _page.Width = (_width * _fontSize) / 2;
        _page.Height = symbols.Length * 1.1 * _fontSize;
        _gfx.DrawRectangle(XBrushes.Black, new XRect(0, 0, _page.Width, _page.Height));
        for (int x = 0; x < symbols.Length; x++)
        {

            _gfx.DrawString(string.Join("", symbols[x]), _font, XBrushes.White,
              new XRect(0, x * otstup, _page.Width, _page.Height),
              XStringFormats.TopCenter);
        }
    }
}

internal class BitmapToASCIIConverter
{
    private readonly char[] _asciiTable = { '.', ',', ':', '+', '*', '?', '%', 'S', '#', '@' };
    private readonly Bitmap _bitmap;


    public BitmapToASCIIConverter(Bitmap bitmap)
    {
        _bitmap = bitmap;

    }

    public char[][] Convert()
    {
        var result = new char[_bitmap.Height][];
        for (int y = 0; y < _bitmap.Height; y++)
        {
            result[y] = new char[_bitmap.Width];

            for (int x = 0; x < _bitmap.Width; x++)
            {
                int mapIndex = (int)Map(_bitmap.GetPixel(x, y).R, 0, 255, 0, _asciiTable.Length - 1);
                result[y][x] = _asciiTable[mapIndex];
            }


        }
        return result;

    }
    private float Map(float valueToMap, float start1, float stop1, float start2, float stop2)
    {
        return ((valueToMap - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
    }

}

public static class Extensions
{

    public static void ToGrayscale(this Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                int avg = (pixel.R + pixel.G + pixel.B) / 3;
                bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(pixel.A, avg, avg, avg));
            }

        }
    }
    public static Bitmap ResizeBitmap(this Bitmap bitmap, int width)
    {
        var newHeight = bitmap.Height / 2 * width / bitmap.Width;
        if (bitmap.Width > width || bitmap.Height > newHeight)
            bitmap = new Bitmap(bitmap, new Size(width, (int)newHeight));
        return bitmap;
    }
}