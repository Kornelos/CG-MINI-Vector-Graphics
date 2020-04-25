using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace CGdrawing
{
    [Serializable]
    abstract class Drawing
    {
        public Color drawingColor;

        public Drawing(Color color)
        { drawingColor = color; }

        public abstract override string ToString();

        abstract public List<Point> GetPixels();

        abstract public Bitmap SetAntialiasing(Bitmap bmp);

        abstract public Point GetClosestVertex(Point point);

        abstract public void SetVertexAt(Point currentLocation, Point newLocation);
    }
}
