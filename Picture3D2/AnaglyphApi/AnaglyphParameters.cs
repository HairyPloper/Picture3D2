using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Picture3D.AnaglyphApi
{
    public static class  AnaglyphParameters
    {
        public static double RedVolume { get; set; }
        public static double BlueVolume { get; set; }
        public static double GreenVolume { get; set; }

        public static double Xaxis { get; set; }
        public static double Yaxis { get; set; }

        public static bool initialized { get; set; }

        public static void ResetParameters()
        {
            RedVolume = 0;
            BlueVolume = 0;
            GreenVolume = 0;
            Xaxis = 0;
            Yaxis = 0;
            initialized = false;
        }

        //public static void InitParameters(double red, double blue, double green, double x, double y)
        //{
        //    if (!initialized)
        //    {
        //        RedVolume = red;
        //        BlueVolume = blue;
        //        GreenVolume = green;
        //        Xaxis = x;
        //        Yaxis = y;
        //        initialized = true;
        //    }
        //}
    }
}
