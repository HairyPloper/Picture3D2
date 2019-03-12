using System.Drawing;
using Picture3D;

namespace AnaglyphGenerator.Models
{
    interface IAnaglyph
    {
        Bitmap Calc(Bitmap leftImage);
    }
}
