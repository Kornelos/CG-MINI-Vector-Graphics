using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing 
{
    [Serializable]
    class Polygon : Drawing
    {
        List<Point> points = new List<Point>();
        public int thickness;
        
        // filling 
        List<Edge> AET = new List<Edge>();
        Dictionary<int,List<Edge>> ET = new Dictionary<int, List<Edge>>();
        int minY;
        public bool filled = false;
        bool clipped = false;
        List<Line> clippedLines = new List<Line>();

        

        public Polygon(List<Point> points, Color color, int thc) : base(color)
        {
            this.points.AddRange(points);
            thickness = thc;
            //initET();
        }
        public override List<Point> GetPixels()
        {
            List<Point> retPoint = new List<Point>();
            if (clipped)
            {
                foreach (Line l in clippedLines)
                    retPoint.AddRange(l.GetPixels());
                return retPoint;
            }
            for (int i = 0; i < points.Count - 1; i++)
            {
                retPoint.AddRange(new Line(points.ElementAt(i), points.ElementAt(i + 1), drawingColor, thickness).GetPixels());

            }
            retPoint.AddRange(new Line(points.Last(), points.First(), drawingColor, thickness).GetPixels());

            if (filled)
                retPoint.AddRange(fill());

            return retPoint;
        }

        public override Bitmap SetAntialiasing(Bitmap bmp)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                bmp = new Line(points.ElementAt(i), points.ElementAt(i + 1), drawingColor, thickness).SetAntialiasing(bmp);

            }
            return new Line(points.Last(), points.First(), drawingColor, thickness).SetAntialiasing(bmp);
        }

        public override string ToString()
        {
            return "polygon";
        }

        public override Point GetClosestVertex(Point point)
        {
            double minDist = double.MaxValue;
            int indexAt = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (point.getAbsoluteDistance(points.ElementAt(i)) < minDist)
                {
                    minDist = point.getAbsoluteDistance(points.ElementAt(i));
                    indexAt = i;

                }
            }
            return points.ElementAt(indexAt);

        }

        public override void SetVertexAt(Point currentLocation, Point newLocation)
        {

            for (int i = 0; i < points.Count; i++)
            {
                if (points.ElementAt(i).Equals(currentLocation))
                    points[i] = newLocation;
            }

        }

        // FILLING AET METHOD

        private List<Edge> getEdgeTable()
        {
            List<Edge> edges = new List<Edge>();
            for(int i=0; i < points.Count - 1; i++)
            {
                edges.Add(new Edge(points[i], points[i + 1]));
            }
            edges.Add(new Edge(points.Last(), points.First()));
            return edges;
        }
            
        private void initET()
        {
            List<Edge> temp = getEdgeTable().OrderBy(e => e.getYMin()).ToList();
            int y = temp.First().getYMin();
            minY = y;
            while(temp.Count != 0)
            {
                if (temp.Where(e => e.getYMin() == y).Count() != 0)
                {
                    List<Edge> bucket = temp.Where(e => e.getYMin() == y).ToList();
                    temp.RemoveAll(e => e.getYMin() == y);
                    ET.Add(y,bucket.OrderBy(e => e.getXMin()).ToList());
                }
                y++;
                
            }
        }
        public List<Point> fill()
        {
            initET();
            List<Point> fillPoints = new List<Point>();
            // 1 st value of y
            int y = minY;
            AET.Clear();
            do
            {
                List<Edge> bucket;
                if (ET.TryGetValue(y, out bucket))
                {
                    AET.AddRange(bucket);

                }
                //sort by X
                AET = AET.OrderBy(e => e.getXMin()).ToList();
                // fill values code goes here
                for (int i = 0; i < AET.Count - 1; i+=2)
                {
                    int x = AET[i].x;
                    while (x != AET[i + 1].x)
                    {
                        //TODO: color 
                        fillPoints.Add(new Point(x, y));
                        x++;
                    }
                }


                y++;
                AET.RemoveAll(e => e.getYMax() == y);
                // update value of x for all in AET
                foreach (Edge e in AET)
                {
                   
                    e.dx_c += e.dx;
                    if (e.dx_c > e.dy)
                    {
                        int div = e.dx_c / e.dy;
                        e.x += div*e.sign;
                        e.dx_c -= div*e.dy;
                    }
                        
                }
                   

            }
            while (AET.Any());

            ET.Clear();
            
            return fillPoints;
        }

        // CLIPPING Cohen Sutherland
        public void clip(Rectangle clip)
        {
            
            for(int i = 0; i < points.Count-1; i++)
            {
               CohenSutherland(points[i], points[i + 1], clip);
                
            }
              CohenSutherland(points[points.Count-1], points[0], clip);

            clipped = true;
            drawingColor = Color.Red;
           

        }


        public static class Outcodes 
        {
            public const byte LEFT = 1;
            public const byte RIGHT = 2;
            public const byte BOTTOM = 4;
            public const byte TOP = 8;
        }
        Byte computeOutcode(Point p, Rectangle clip)
        {
            byte outcode = 0;
            if (p.X > clip.Right)
                outcode |= Outcodes.RIGHT;
            else if (p.X < clip.Left)
                outcode |= Outcodes.LEFT;
            if (p.Y < clip.Top) 
                outcode |= Outcodes.TOP;
            else if (p.Y > clip.Bottom) 
                outcode |= Outcodes.BOTTOM;

            return outcode;
        }

        void CohenSutherland(Point p1, Point p2, Rectangle clip)
        {
            
            bool accept = false, done = false;
            Byte outcode1 = computeOutcode(p1, clip);
            Byte outcode2 = computeOutcode(p2, clip);
            do
            {
                if((outcode1 | outcode2) == 0)
                {
                    //accept all
                    accept = true;
                    done = true;
                }
                else if ((outcode1 & outcode2) != 0)
                {
                    //rej all
                    accept = false;
                    done = true;
                }
                else //subdivide
                {
                    Byte outcodeOut = (outcode1 != 0) ? outcode1 : outcode2;
                    Point p = new Point();
                    if((outcodeOut & Outcodes.TOP) != 0)
                    {
                        p.X = p1.X + (p2.X - p1.X) * (clip.Top - p1.Y) / (p2.Y - p1.Y);
                        p.Y = clip.Top;
                    }
                    else if ((outcodeOut & Outcodes.BOTTOM) != 0)
                    {
                        p.X = p1.X + (p2.X - p1.X) * (clip.Bottom - p1.Y) / (p2.Y - p1.Y);
                        p.Y = clip.Bottom;
                    }
                    else if ((outcodeOut & Outcodes.LEFT) != 0)
                    {
                        p.X = clip.Right;
                        p.Y = p1.Y + (p2.Y - p1.Y) * (clip.Left - p1.X) / (p2.X - p1.X);
                       
                    }
                    else if ((outcodeOut & Outcodes.RIGHT) != 0)
                    {
                        p.X = clip.Right;
                        p.Y = p1.Y + (p2.Y - p1.Y) * (clip.Right - p1.X) / (p2.X - p1.X);
                    }
                    if (outcodeOut == outcode1) 
                    {
                        p1 = p; 
                        outcode1 = computeOutcode(p1, clip);
                    } 
                    else 
                    {
                        p2 = p; 
                        outcode2 = computeOutcode(p2, clip); 
                    }
                }
            } while (!done);

            if (accept)
                clippedLines.Add(new Line(p1, p2, drawingColor, thickness));
           
                
        }
    }
}
