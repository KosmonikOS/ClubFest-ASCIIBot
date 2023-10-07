using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ClubFestBotCSharp;
internal class PdfASCIIImageDrawer
{
    public PdfDocument Document { get; private set; }
    private readonly PdfPage _page;
    private readonly XGraphics _gfx;
    private readonly XFont _font;
    private readonly int _fontSize;
    private readonly int _width;

    public PdfASCIIImageDrawer(int fontSize, int width)
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
