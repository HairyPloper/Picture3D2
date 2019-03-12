using System.Drawing;
using Picture3D;
using Picture3D.AnaglyphApi;

namespace AnaglyphGenerator.Models
{
    public class GrayAnaglyph : IAnaglyph
    {

        public GrayAnaglyph()
        {
           // AnaglyphParameters.InitParameters(0.299,0.114,0.587,10,0);
        }

        public Bitmap Calc(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            int r, g, b;

            Bitmap outputImage = new Bitmap(width, height);

            int tempX, tempY;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tempX = x + (int)AnaglyphParameters.Xaxis;
                    tempY = y + (int)AnaglyphParameters.Yaxis;

                    if (tempX >= image.Width)
                        tempX = image.Width - 1;
                    if (tempY >= image.Height)
                        tempY = image.Height - 1;


                    r =
                        (int)
                            (image.GetPixel(x, y).R * AnaglyphParameters.RedVolume + image.GetPixel(x, y).G * AnaglyphParameters.GreenVolume +
                             image.GetPixel(x, y).B * AnaglyphParameters.BlueVolume);
                    g =
                        (int)
                            (image.GetPixel(tempX, tempY).R * AnaglyphParameters.RedVolume + image.GetPixel(tempX, tempY).G * AnaglyphParameters.GreenVolume +
                             image.GetPixel(tempX, tempY).B * AnaglyphParameters.BlueVolume);
                    b = g;

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