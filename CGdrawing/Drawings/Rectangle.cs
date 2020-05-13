using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    class RectangleDrawing : Drawing
    {
        // edge order from bottom left counterclockwise a,c,b,d
        Point a, b, c, d;
        int thickness;
        public RectangleDrawing(Color color, Point a, Point b,int thc) : base(color)
        {
            thickness = thc;
            this.a = a;
            this.b = b;
            c = new Point(a.X, b.Y);
            d = new Point(b.X, a.Y);
        }
        public override Point GetClosestVertex(Point point)
        {
            Dictionary<Point,double> dists = new Dictionary<Point, double>();
            dists.Add(a,a.getAbsoluteDistance(point));
            dists.Add(b,b.getAbsoluteDistance(point));
            dists.Add(c,c.getAbsoluteDistance(point));
            dists.Add(d,d.getAbsoluteDistance(point));
            return dists.OrderBy(k => k.Value).FirstOrDefault().Key;
        }

        public override List<Point> GetPixels()
        {
            List<Point> retPoint = new List<Point>();
            retPoint.AddRange(new Line(d, a, drawingColor, thickness).GetPixels());
            retPoint.AddRange(new Line(a, c, drawingColor, thickness).GetPixels());
            retPoint.AddRange(new Line(c, b, drawingColor, thickness).GetPixels());
            retPoint.AddRange(new Line(b, d, drawingColor, thickness).GetPixels());
            return retPoint;
        }

        public override Bitmap SetAntialiasing(Bitmap bmp)
        {
            throw new NotImplementedException();
        }

        public override void SetVertexAt(Point currentLocation, Point newLocation)
        {
            if (currentLocation.Equals(a))
            {
                a = newLocation;
                calcCD(a, b);
            }
            else if (currentLocation.Equals(b))
            {
                b = newLocation;
                calcCD(a, b);
            }
            else if (currentLocation.Equals(c))
            {
                
                c = a;
                a = newLocation;
                Point tmp = b;
                b = d;
                d = tmp;
                calcCD(a, b);
               
            }
            else if (currentLocation.Equals(d))
            {
                Point tmp = c;
                c = a;
                a = tmp;
                b = newLocation;
                d = tmp;
                calcCD(a, b);

            }

        }

        public override string ToString()
        {
            return "Rectangle";
        }

        private void calcCD(Point a, Point b)
        {
            c = new Point(a.X, b.Y);
            d = new Point(b.X, a.Y);
        }
    }
}
