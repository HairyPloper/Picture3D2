using System.Drawing;
using Picture3D;
using Picture3D.AnaglyphApi;

namespace AnaglyphGenerator.Models
{
    public class ColorAnaglyph : IAnaglyph
    {

        public ColorAnaglyph()
        {
           // AnaglyphParameters.InitParameters(1,1,1,0,0);
           
        }

        public Bitmap Calc(Bitmap image)
        {
            
            int width = image.Width;
            int height = image.Height;

            int r, g, b;
            int tempX, tempY;

            Bitmap outputImage = new Bitmap(width, height,System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    r = image.GetPixel(x, y).R+(int)AnaglyphParameters.RedVolume;
                    g = image.GetPixel(x, y).G+(int)AnaglyphParameters.GreenVolume;
                    b = image.GetPixel(x, y).B+(int)AnaglyphParameters.BlueVolume;



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