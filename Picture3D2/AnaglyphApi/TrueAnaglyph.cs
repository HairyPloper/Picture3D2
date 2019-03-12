using System.Drawing;
using Picture3D;
using Picture3D.AnaglyphApi;

namespace AnaglyphGenerator.Models
{
    public class TrueAnaglyph : IAnaglyph
    {

        public TrueAnaglyph()
        {
 //           AnaglyphParameters.InitParameters(0.299,0.587,0.114,10,0);

        }


        public Bitmap Calc(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Bitmap outputImage = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    var tempX = x + (int)AnaglyphParameters.Xaxis;
                    var tempY = y + (int)AnaglyphParameters.Yaxis;

                    if (tempX >= image.Width)
                        tempX = image.Width - 1;
                    if (tempY >= image.Height)
                        tempY = image.Height - 1;

                    var r = (int)(image.GetPixel(x, y).R * AnaglyphParameters.RedVolume + image.GetPixel(x, y).G * AnaglyphParameters.GreenVolume + image.GetPixel(x, y).B * AnaglyphParameters.RedVolume);
                    var g = 0;
                    var b = (int)(image.GetPixel(tempX, tempY).R * AnaglyphParameters.RedVolume + image.GetPixel(tempX, tempY).G * AnaglyphParameters.GreenVolume + image.GetPixel(tempX, tempY).B * AnaglyphParameters.RedVolume);

                    if (r > 255)
                        r = 255;
                    if (b > 255)
                        b = 255;
                    if (g > 255)
                        g = 255;

                    Color c = Color.FromArgb(r, g, b);
                    outputImage.SetPixel(x, y, c);
                }
            }
            return outputImage;
        }
    }
}