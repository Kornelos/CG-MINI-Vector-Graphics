using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    /*line drawing using midlpoint alghoritm*/
    [Serializable]
    class Line : Drawing
    {
        public Point startPoint;
        public Point endPoint;
        public int thickness;

        public Line(Point start, Point end, Color color, int thc) : base(color)
        {
            startPoint = start;
            endPoint = end;
            thickness = thc - 1;
        }

        public override List<Point> GetPixels()
        {
            // here we run our alghoritm
            return MidPointAlgorithm(startPoint, endPoint);
        }

        private List<Point> MidPointAlgorithm(Point start, Point end)
        {
            List<Point> points = new List<Point>();

            int x = start.X, y = start.Y;
            int x2 = end.X, y2 = end.Y;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                points.Add(new Point(x, y));
                if (Math.Abs(h) > Math.Abs(w))
                    for (int j = 1; j < thickness; j++)
                    {
                        points.Add(new Point(x - j, y));
                        points.Add(new Point(x + j, y));
                    }
                else if (Math.Abs(w) > Math.Abs(h))
                    for (int j = 1; j < thickness; j++)
                    {
                        points.Add(new Point(x, y - j));
                        points.Add(new Point(x, y + j));
                    }

                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

            return points;
        }
        public byte[] AALine(byte[] pixels, int stride)
        {

            var bytes = pixels;

            int x0 = startPoint.X, y0 = startPoint.Y;
            int x1 = endPoint.X, y1 = endPoint.Y;

            int sa = drawingColor.A;
            uint sg = drawingColor.G;
            uint srb = (uint)int.Parse($"00{Convert.ToString(drawingColor.R, 16)}00{Convert.ToString(drawingColor.B, 16)}", System.Globalization.NumberStyles.HexNumber);

            int pixelWidth = stride / 4;
            int pixelHeight = bytes.Length / stride;

            if ((x0 == x1) && (y0 == y1)) return new byte[0];

            if (x0 < 1) x0 = 1;
            if (x0 > pixelWidth - 2) x0 = pixelWidth - 2;
            if (y0 < 1) y0 = 1;
            if (y0 > pixelHeight - 2) y0 = pixelHeight - 2;

            if (x1 < 1) x1 = 1;
            if (x1 > pixelWidth - 2) x1 = pixelWidth - 2;
            if (y1 < 1) y1 = 1;
            if (y1 > pixelHeight - 2) y1 = pixelHeight - 2;

            int addr = y0 * pixelWidth + x0;
            int dx = x1 - x0;
            int dy = y1 - y0;

            int du;
            int dv;
            int u;
            int v;
            int uincr;
            int vincr;

            // By switching to (u,v), we combine all eight octants
            int adx = dx, ady = dy;
            if (dx < 0) adx = -dx;
            if (dy < 0) ady = -dy;

            if (adx > ady)
            {
                du = adx;
                dv = ady;
                u = x1;
                v = y1;
                uincr = 1;
                vincr = pixelWidth;
                if (dx < 0) uincr = -uincr;
                if (dy < 0) vincr = -vincr;

            }
            else
            {
                du = ady;
                dv = adx;
                u = y1;
                v = x1;
                uincr = pixelWidth;
                vincr = 1;
                if (dy < 0) uincr = -uincr;
                if (dx < 0) vincr = -vincr;
            }

            int uend = u + du;
            int d = (dv << 1) - du;        // Initial value as in Bresenham's
            int incrS = dv << 1;    // Δd for straight increments
            int incrD = (dv - du) << 1;    // Δd for diagonal increments

            double invDFloat = 1.0 / (4.0 * Math.Sqrt(du * du + dv * dv));   // Precomputed inverse denominator
            double invD2duFloat = 0.75 - 2.0 * (du * invDFloat);   // Precomputed constant

            const int PRECISION_SHIFT = 10; // result distance should be from 0 to 1 << PRECISION_SHIFT, mapping to a range of 0..1
            const int PRECISION_MULTIPLIER = 1 << PRECISION_SHIFT;
            int invD = (int)(invDFloat * PRECISION_MULTIPLIER);
            int invD2du = (int)(invD2duFloat * PRECISION_MULTIPLIER * sa);
            int ZeroDot75 = (int)(0.75 * PRECISION_MULTIPLIER * sa);

            int invDMulAlpha = invD * sa;
            int duMulInvD = du * invDMulAlpha; // used to help optimize twovdu * invD
            int dMulInvD = d * invDMulAlpha; // used to help optimize twovdu * invD
                                             //int twovdu = 0;    // Numerator of distance; starts at 0
            int twovduMulInvD = 0; // since twovdu == 0
            int incrSMulInvD = incrS * invDMulAlpha;
            int incrDMulInvD = incrD * invDMulAlpha;

            do
            {
                AlphaBlendNormalOnPremultiplied(pixels, addr, (ZeroDot75 - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
                AlphaBlendNormalOnPremultiplied(pixels, addr + vincr, (invD2du + twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
                AlphaBlendNormalOnPremultiplied(pixels, addr - vincr, (invD2du - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);

                if (d < 0)
                {
                    // choose straight (u direction)
                    twovduMulInvD = dMulInvD + duMulInvD;
                    d += incrS;
                    dMulInvD += incrSMulInvD;
                }
                else
                {
                    // choose diagonal (u+v direction)
                    twovduMulInvD = dMulInvD - duMulInvD;
                    d += incrD;
                    dMulInvD += incrDMulInvD;
                    v++;
                    addr += vincr;
                }
                u++;
                addr += uincr;
            } while (u < uend);

            return bytes;
        }

        private void AlphaBlendNormalOnPremultiplied(byte[] pixels, int index, int sa, uint srb, uint sg)
        {
            uint destPixel = (uint)pixels[index];
            uint da, dg, drb;

            da = (destPixel >> 24);
            dg = ((destPixel >> 8) & 0xff);
            drb = destPixel & 0x00FF00FF;

            uint dr = ((destPixel >> 16) & 0xff);
            uint db = ((destPixel) & 0xff);

            int sb = drawingColor.B;
            int sr = drawingColor.R;

            pixels[index * 4] = (byte)((((sb - db) * sa) >> 8) + db);
            pixels[index * 4 + 1] = (byte)(((sg - dg) * sa + (dg << 8)) & 0xFFFFFF00);
            pixels[index * 4 + 2] = (byte)(((((sr - dr) * sa) >> 8) + dr) << 16);
            pixels[index * 4 + 3] = (byte)((sa + ((da * (255 - sa) * 0x8081) >> 23)) << 24);

        }

        public Bitmap GuptaSproullAlgorithm(Bitmap bmp)
        // http://elynxsdk.free.fr/ext-docs/Rasterization/Antialiasing/Gupta%20sproull%20antialiased%20lines.htm
        // https://jamesarich.weebly.com/uploads/1/4/0/3/14035069/480xprojectreport.pdf
        {
            int x1 = startPoint.X, y1 = startPoint.Y;
            int x2 = endPoint.X, y2 = endPoint.Y;

            int dx = x2 - x1;
            int dy = y2 - y1;

            int du, dv, u, x, y, ix, iy;

            // By switching to (u,v), we combine all eight octant
            int adx = dx < 0 ? -dx : dx;
            int ady = dy < 0 ? -dy : dy;
            x = x1;
            y = y1;
            if (adx > ady)
            {
                du = adx;
                dv = ady;
                u = x2;
                ix = dx < 0 ? -1 : 1;
                iy = dy < 0 ? -1 : 1;
            }
            else
            {
                du = ady;
                dv = adx;
                u = y2;
                ix = dx < 0 ? -1 : 1;
                iy = dy < 0 ? -1 : 1;
            }

            int uEnd = u + du;
            int d = (2 * dv) - du; // Initial value as in Bresenham's
            int incrS = 2 * dv; // Δd for straight increments
            int incrD = 2 * (dv - du); // Δd for diagonal increments
            int twovdu = 0; // Numerator of distance
            double invD = 1.0 / (2.0 * Math.Sqrt(du * du + dv * dv)); // Precomputed inverse denominator
            double invD2du = 2.0 * (du * invD); // Precomputed constant

            if (adx > ady)
            {
                do
                {
                    newColorPixel(bmp, x, y, twovdu * invD);
                    newColorPixel(bmp, x, y + iy, invD2du - twovdu * invD);
                    newColorPixel(bmp, x, y - iy, invD2du + twovdu * invD);

                    if (d < 0)
                    {
                        // Choose straight
                        twovdu = d + du;
                        d += incrS;

                    }
                    else
                    {
                        // Choose diagonal
                        twovdu = d - du;
                        d += incrD;
                        y += iy;
                    }
                    u++;
                    x += ix;
                } while (u < uEnd);
            }
            else
            {
                do
                {
                    newColorPixel(bmp, x, y, twovdu * invD);
                    newColorPixel(bmp, x, y + iy, invD2du - twovdu * invD);
                    newColorPixel(bmp, x, y - iy, invD2du + twovdu * invD);

                    if (d < 0)
                    {
                        // Choose straight
                        twovdu = d + du;
                        d += incrS;
                    }
                    else
                    {
                        // Choose diagonal
                        twovdu = d - du;
                        d += incrD;
                        x += ix;
                    }
                    u++;
                    y += iy;
                } while (u < uEnd);
            }

            return bmp;
        }

        void newColorPixel(Bitmap bmp, int x, int y, double dist)
        {
            double value = 1 - Math.Pow((dist * 2 / 3), 2);

            Color old = bmp.GetPixelFast(x, y);

            Color col = ColorInterpolator.InterpolateBetween(old, drawingColor, value);

            bmp.SetPixelFast(x, y, col);
        }

        public override string ToString()
        {
            return "Line";
        }

        public override Bitmap SetAntialiasing(Bitmap bmp)
        {
            return GuptaSproullAlgorithm(bmp);
        }

        public override Point GetClosestVertex(Point point)
        {
            if (point.getAbsoluteDistance(startPoint) <= point.getAbsoluteDistance(endPoint))
                return startPoint;
            else
                return endPoint;
        }

        public override void SetVertexAt(Point currentLocation, Point newLocation)
        {
            if (startPoint.Equals(currentLocation))
                startPoint = newLocation;
            else
                endPoint = newLocation;
        }
    }
}
