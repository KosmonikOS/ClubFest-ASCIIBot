using PdfSharp.Pdf;
using PDFtoImage;

namespace ClubFestBotCSharp;
internal class PdfConverter
{
    public MemoryStream ConvertToPng(PdfDocument document)
    {
        using var pdfStream = new MemoryStream();

        document.Save(pdfStream, false);

        var pngStream = new MemoryStream();

        Conversion.SavePng(pngStream, pdfStream.ToArray());

        pngStream.Position = 0;

        return pngStream;
    }
}
