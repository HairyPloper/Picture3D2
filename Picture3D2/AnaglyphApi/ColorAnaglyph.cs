using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
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

            Bitmap outputImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            ProcessUsingLockbitsAndUnsafeAndParallel(image);

            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        r = image.GetPixel(x, y).R + (int)AnaglyphParameters.RedVolume;
            //        g = image.GetPixel(x, y).G + (int)AnaglyphParameters.GreenVolume;
            //        b = image.GetPixel(x, y).B + (int)AnaglyphParameters.BlueVolume;



            //        if (r > 255)
            //            r = 255;
            //        if (b > 255)
            //            b = 255;
            //        if (g > 255)
            //            g = 255;
            //        Color c = Color.FromArgb(r, g, b);
            //        outputImage.SetPixel(x, y, c);
            //    }
            //}

            return image;
        }
        private void ProcessUsingLockbitsAndUnsafeAndParallel(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x] + (int)AnaglyphParameters.RedVolume; ;
                        int oldGreen = currentLine[x + 1] + (int)AnaglyphParameters.GreenVolume; ;
                        int oldRed = currentLine[x + 2] + (int)AnaglyphParameters.RedVolume; ;

                        if (oldRed > 255)
                            oldRed = 255;
                        if (oldBlue > 255)
                            oldBlue = 255;
                        if (oldGreen > 255)
                            oldGreen = 255;

                        currentLine[x] = (byte)oldBlue ;
                        currentLine[x + 1] = (byte)oldGreen;
                        currentLine[x + 2] = (byte)oldRed;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
        }
    }
}