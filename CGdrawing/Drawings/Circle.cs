using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGdrawing
{
    [Serializable]
    class Circle : Drawing
    {
        Point center;
        int radius;
        public Circle(Point center, Point pointOnCircle, Color color) : base(color)
        {
            this.center = center;
            radius = (int) pointOnCircle.getAbsoluteDistance(center);
        }

        public override Point GetClosestVertex(Point point)
        {
            // calculate closest point on circle
            
            int closest_x = (int)(center.X + radius * (point.X - center.X) / point.getAbsoluteDistance(center));
            int closest_y = (int)(center.Y + radius * (point.Y - center.Y) / point.getAbsoluteDistance(center));
            Point closest = new Point(closest_x, closest_y);

            if (closest.getAbsoluteDistance(point) < center.getAbsoluteDistance(point))
                return closest;
            // if center is closer return center
            else
                return center;
        }

        public override List<Point> GetPixels()
        {
            return MidPointCircle();
        }

        public override Bitmap SetAntialiasing(Bitmap bmp)
        {
            // circle does not have to be aa by requirements
            return bmp;
        }

        public override void SetVertexAt(Point currentLocation, Point newLocation)
        {
            if (currentLocation.Equals(center))
                center = newLocation;
            else
            {
                radius = (int)center.getAbsoluteDistance(newLocation);
            }
        }

        public override string ToString()
        {
            return "Circle";
        }

        private List<Point> MidPointCircle()
        {
           
            if (radius == 0)
                return new List<Point>() { center };

            var points = new List<Point>();

            int x = radius, y = 0;
            int P = 1 - x;

            while (x > y)
            {

                y++;

                if (P <= 0)
                    P = P + 2 * y + 1;
                else
                {
                    x--;
                    P = P + 2 * y - 2 * x + 1;
                }

                if (x < y)
                    break;

                points.Add(new Point(x + center.X, y + center.Y));
                points.Add(new Point(-x + center.X, y + center.Y));
                points.Add(new Point(x + center.X, -y + center.Y));
                points.Add(new Point(-x + center.X, -y + center.Y));

                if (x != y)
                {
                    points.Add(new Point(y + center.X, x + center.Y));
                    points.Add(new Point(-y + center.X, x + center.Y));
                    points.Add(new Point(y + center.X, -x + center.Y));
                    points.Add(new Point(-y + center.X, -x + center.Y));
                }
            }

            return points;
        }
    }
}
