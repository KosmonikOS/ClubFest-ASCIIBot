using System.Drawing;

namespace ClubFestBotCSharp;
public static class BitmapExtensions
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