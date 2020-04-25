using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    static class PointExtension
    {
        public static double getAbsoluteDistance(this Point point, Point point2)
        {
            return Math.Sqrt(Math.Pow((point.Y - point2.Y), 2) + Math.Pow((point.X - point2.X), 2));
        }
    }
}
