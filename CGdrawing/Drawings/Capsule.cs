using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    class Capsule : Drawing
    {
        //var radius;
        Point e1, e2, e3, e4;
        Point a, b, c;
        int r;
        public Capsule(Color color, Point a, Point b, Point c) : base(color)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            // calculate point e
            var radius = b.getAbsoluteDistance(c);
            var ab = a.getAbsoluteDistance(b);
            r = (int)radius;
            Vector2 vectorAB = new Vector2(b.X-a.X, b.Y-a.Y);
            Vector2 vectorABperp = new Vector2(-vectorAB.Y, vectorAB.X);

            var step = vectorABperp / vectorABperp.Length();
            Vector2 W = new Vector2((float)(step.X * radius), (float)(step.Y * radius));

            // coordinates of points
            //near A
            /*     [Ax + Wx, Ay + Wy]
                 [Ax - Wx, Ay - Wy]
                 [Bx + Wx, By + Wy]
             [   Bx - Wx, By - Wy]*/
             e1 = new Point((int)(a.X + W.X), (int)(a.Y + W.Y));
             e2 = new Point((int)(a.X - W.X), (int)(a.Y - W.Y));
            // near B
             e3 = new Point((int)(b.X + W.X), (int)(b.Y + W.Y));
             e4 = new Point((int)(b.X - W.X), (int)(b.Y - W.Y));
        }

        public override Point GetClosestVertex(Point point)
        {
            throw new NotImplementedException();
        }

        public override List<Point> GetPixels()
        {
            List<Point> retPoint = new List<Point>();
            List<Point> c1p = new List<Point>();
            List<Point> c2p = new List<Point>();
            Line l1 = new Line(e1, e3, drawingColor, 1);
            Line l2 = new Line(e2, e4, drawingColor, 1);

            Circle c1 = new Circle(a, r, drawingColor);
            Circle c2 = new Circle(b, r, drawingColor);
            retPoint.AddRange( l1.GetPixels());
            retPoint.AddRange(l2.GetPixels());

            c1p.AddRange(c1.GetPixels());
            c2p.AddRange(c2.GetPixels());

            foreach(Point p in c1p)
            {
                if (sign(e1, e2, p) > 0)
                    retPoint.Add(p);
            }
            foreach (Point p in c2p)
            {
                if (sign(e3, e4, p) < 0)
                    retPoint.Add(p);
            }
            return retPoint;
        }

        private int sign(Point e,Point d,Point f)
        {
            // sign((Ex - Dx) * (Fy - Dy) - (Ey - Dy) * (Fx - Dx))
            return (e.X - d.X) * (f.Y - d.Y) - (e.Y - d.Y) * (f.X - d.X);
        }

        public override Bitmap SetAntialiasing(Bitmap bmp)
        {
            throw new NotImplementedException();
        }

        public override void SetVertexAt(Point currentLocation, Point newLocation)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Capsule";
        }
    }
}
