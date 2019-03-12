using System;
using AnaglyphGenerator.Models;
using System.Drawing;
using Picture3D;

using System.Runtime.InteropServices;

namespace AnaglyphGenerator.Models
{
    public class AnaglyphAlgorithmInvoker
    {
        private IAnaglyph algorithm;
      
       
        public AnaglyphAlgorithmInvoker(string kindOfAlgorithm)
        {
            switch (kindOfAlgorithm)
            {
                case "True Anaglyph":
                    algorithm = new TrueAnaglyph();
                    break;
                case "Gray Anaglyph":
                    algorithm = new GrayAnaglyph();
                    break;
                case "Color Anaglyph":
                    System.Diagnostics.Debug.WriteLine("sssssss");
                   // algorithm = new ColorAnaglyph();
                   
                    algorithm = new ColorAnaglyph();
                    break;
                case "Half-color Anaglyph":
                    
                    algorithm = new HalfColorAnaglyph();
                    break;
                case "Optimized Anaglyph":
                default:
                    algorithm = new OptimizedAnaglyph();
                    break;
            }
        }

        public Bitmap Apply(Bitmap Image)
        {
            System.Diagnostics.Debug.WriteLine("SSSSS");
            return algorithm.Calc(Image);
        }

    }
}