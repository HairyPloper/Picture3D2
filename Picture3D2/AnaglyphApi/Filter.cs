using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Picture3D.AnaglyphApi;

namespace AnaglyphGenerator.Models
{
    public class Fitler 
    {
        public Bitmap Calc(Bitmap image)
        {
            ProcessUsingLockbitsAndUnsafeAndParallel(image);
            return image;
        }
        private void ProcessUsingLockbitsAndUnsafeAndParallel(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x] + (int)AnaglyphParameters.BlueVolume; ;
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