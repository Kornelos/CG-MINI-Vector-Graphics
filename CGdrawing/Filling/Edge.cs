using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    class Edge
    {
        // 1/m
        public double mInv;
        // max and min by Y
        Point min;
        Point max;
        public int x;
        public int dx;
        public int dy;
        public int dx_c = 0;
        public int sign;

        public Edge(Point start, Point end)
        {
            min = start.Y > end.Y ? end : start;
            max = start.Y > end.Y ? start : end;

            x = min.X;
            dx = Math.Abs(start.X - end.X);
            dy = Math.Abs(start.Y - end.Y);

            if (max.X > min.X)
                sign = 1;
            else
                sign = -1;
        }

        public int getYMin()
        {
            return min.Y;
        }

        public int getXMin()
        {
            return x;
        }

        public int getYMax()
        {
            return max.Y;
        }
    }

}
