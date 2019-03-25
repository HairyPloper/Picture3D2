using System;

namespace Picture3D.AnaglyphApi
{
    public static class  AnaglyphParameters
    {
        public static double RedVolume { get; set; }
        public static double BlueVolume { get; set; }
        public static double GreenVolume { get; set; }
        public static string PathToRead { get; set; }
        public static string PathToWrite { get; set; }
        public static Uri VideoPath { get; set; }

        public static void ResetParameters()
        {
            RedVolume = 0;
            BlueVolume = 0;
            GreenVolume = 0;
        }
    }
}
