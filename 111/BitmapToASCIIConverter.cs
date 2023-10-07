using System.Drawing;

namespace ClubFestBotCSharp;
internal class BitmapConverter
{
    private readonly char[] _asciiTable = { '.', ',', ':', '+', '*', '?', '%', 'S', '#', '@' };

    public char[][] ConvertToASCII(Bitmap bitmap)
    {
        var result = new char[bitmap.Height][];
        for (int y = 0; y < bitmap.Height; y++)
        {
            result[y] = new char[bitmap.Width];

            for (int x = 0; x < bitmap.Width; x++)
            {
                int mapIndex = (int)Map(bitmap.GetPixel(x, y).R, 0, 255, 0, _asciiTable.Length - 1);
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