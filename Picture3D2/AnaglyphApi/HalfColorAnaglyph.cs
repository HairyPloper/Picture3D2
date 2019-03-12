using System.Drawing;
using Picture3D;
using Picture3D.AnaglyphApi;

namespace AnaglyphGenerator.Models
{
    public class HalfColorAnaglyph : IAnaglyph
    {

        public HalfColorAnaglyph()
        {
         
           // AnaglyphParameters.InitParameters(1,0.3,0.7,10,0);
        }

        public Bitmap Calc(Bitmap Image)
        {
            int width = Image.Width;
            int height = Image.Height;

            int r, g, b;

            Bitmap outputImage = new Bitmap(width, height);

            int tempX, tempY;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tempX = x + (int)AnaglyphParameters.Xaxis;
                    tempY = y + (int)AnaglyphParameters.Yaxis;

                    if (tempX >= Image.Width)
                        tempX = Image.Width - 1;
                    if (tempY >= Image.Height)
                        tempY = Image.Height - 1;

                    r = (int)(Image.GetPixel(x, y).G * AnaglyphParameters.GreenVolume + Image.GetPixel(x, y).B * AnaglyphParameters.BlueVolume);
                    g = Image.GetPixel(tempX, tempY).G;
                    b = Image.GetPixel(tempX, tempY).B;

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