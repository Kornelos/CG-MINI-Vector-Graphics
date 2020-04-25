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
        public Polygon(List<Point> points, Color color,int thc) : base(color)
        {
            this.points.AddRange(points);
            thickness = thc;
        }
        public override List<Point> GetPixels()
        {
            List<Point> retPoint = new List<Point>();
            for(int i = 0; i < points.Count-1; i++)
            {
                retPoint.AddRange(new Line(points.ElementAt(i), points.ElementAt(i + 1),drawingColor,thickness).GetPixels());
                
            }
            retPoint.AddRange(new Line(points.Last(), points.First(),drawingColor,thickness).GetPixels());
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
            
            for (int i= 0; i < points.Count;i++)
            {
                if (points.ElementAt(i).Equals(currentLocation))
                    points[i] = newLocation;
            }
            
        }
    }
}
