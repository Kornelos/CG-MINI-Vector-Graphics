using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CGdrawing
{
    public partial class Form1 : Form
    {
        BindingList<Drawing> drawings = new BindingList<Drawing>();
        List<Point> clickedPoints = new List<Point>();
        DrawingTypes currentDrawing = DrawingTypes.Polygon;
        Color currentColor = Color.Black;
        int currentThickness = 1;

        // moving related variables
        Drawing drawingForMove;
        Point vertexForMove;

        //cliping mode
        bool clipingMode = false;
       

        public Form1()
        {
            InitializeComponent();
            shapeListBox.DataSource = drawings;
            // init picturebox
            mainPictureBox.Image = InitBmp();
            colorDialog1.Color = Color.Black;

        }
        private Bitmap InitBmp()
        {
            var bmp = new Bitmap(mainPictureBox.Width, mainPictureBox.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            return bmp;
        }
        private Bitmap Redraw()
        {
            Bitmap bmp = InitBmp();
            foreach (var d in drawings)
            {
                // antialias
                if (aaCheckBox.Checked && !d.ToString().Equals("Circle"))
                    bmp = d.SetAntialiasing(bmp);
                else
                {
                    foreach (var point in d.GetPixels())
                    {
                        if (point.X >= mainPictureBox.Width || point.Y >= mainPictureBox.Height || point.X <= 0 || point.Y <= 0)
                            continue;

                        bmp.SetPixelUnsafe(point.X, point.Y, d.drawingColor);
                    }
                }

            }
            mainPictureBox.Image = bmp;
            return bmp;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void mainPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            // dont draw if its move mode
            if (dragCheckBox.Checked)
                return;

            if (clipingMode)
            {
                clickedPoints.Add(e.Location);
                if (clickedPoints.Count == 2)
                {
                    Polygon p = (Polygon)shapeListBox.SelectedItem;
                    p.clip(new Rectangle(clickedPoints[0].X, clickedPoints[0].Y, clickedPoints[1].X - clickedPoints[0].X, clickedPoints[1].Y - clickedPoints[0].Y));
                    drawings.Add(new RectangleDrawing(currentColor, clickedPoints[0], clickedPoints[1], currentThickness));
                    Redraw();
                    clipingMode = false;
                    clickedPoints.Clear();
                }
                else return;
                
            }

            switch (currentDrawing)
            {
                case DrawingTypes.Line:
                    if (clickedPoints.Count == 0)
                    {
                        //add
                        clickedPoints.Add(e.Location);
                    }
                    else
                    {
                        //add and draw
                        clickedPoints.Add(e.Location);
                        drawings.Add(new Line(clickedPoints.Last(), clickedPoints.First(), currentColor, currentThickness));
                        clickedPoints.Clear();
                        //draw shape
                        Redraw();
                    }
                    break;
                case DrawingTypes.Circle:
                    if (clickedPoints.Count == 0)
                    {
                        //add
                        clickedPoints.Add(e.Location);
                    }
                    else
                    {
                        //add and draw
                        clickedPoints.Add(e.Location);
                        drawings.Add(new Circle(clickedPoints.Last(), clickedPoints.First(), currentColor));
                        clickedPoints.Clear();
                        //draw shape
                        Redraw();
                    }
                    break;
                case DrawingTypes.Polygon:
                    if (clickedPoints.Count == 0)
                    {
                        clickedPoints.Add(e.Location);
                    }
                    else if (Math.Abs(clickedPoints.First().X - e.X) < 30 && Math.Abs(clickedPoints.First().Y - e.Y) < 30)
                    {
                        drawings.Add(new Polygon(clickedPoints, currentColor, currentThickness));
                        clickedPoints.Clear();
                        //draw shape
                        Redraw();
                        clickedPoints.Clear();
                    }
                    else
                    {
                        clickedPoints.Add(e.Location);
                       
                        // temporary lines
                        List<Line> lines = new List<Line>();
                        for (int i = 0; i < clickedPoints.Count - 1;i++ )
                        {
                            Line l = new Line(clickedPoints[i], clickedPoints[i + 1], currentColor, currentThickness);
                            lines.Add(l);
                            drawings.Add(l);
                        }
                        Redraw();
                        foreach (Line l in lines)
                            drawings.Remove(l);


                    }
                    break;
                case DrawingTypes.Capsule:
                    if (clickedPoints.Count == 2)
                    {
                        clickedPoints.Add(e.Location);
                        drawings.Add(new Capsule(currentColor, clickedPoints[0], clickedPoints[1], clickedPoints[2]));
                        Redraw();
                        clickedPoints.Clear();

                    }
                    else
                    {
                        clickedPoints.Add(e.Location);
                    }
                    break;
                case DrawingTypes.Rectangle:
                    if(clickedPoints.Count == 1)
                    {
                        clickedPoints.Add(e.Location);
                        drawings.Add(new RectangleDrawing(currentColor, clickedPoints[0], clickedPoints[1], currentThickness));
                        Redraw();
                        clickedPoints.Clear();
                    }
                    else
                    {
                        clickedPoints.Add(e.Location);
                    }
                    break;

            }
        }

        private void lineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(ToolStripMenuItem item in shapesToolStripMenuItem.DropDownItems)
                item.Checked = false;

            lineToolStripMenuItem.Checked = true;
            currentDrawing = DrawingTypes.Line;
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in shapesToolStripMenuItem.DropDownItems)
                item.Checked = false;

            circleToolStripMenuItem.Checked = true;
            currentDrawing = DrawingTypes.Circle;
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in shapesToolStripMenuItem.DropDownItems)
                item.Checked = false;

            polygonToolStripMenuItem.Checked = true;
            currentDrawing = DrawingTypes.Polygon;
        }
        private void capsuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in shapesToolStripMenuItem.DropDownItems)
                item.Checked = false;

            capsuleToolStripMenuItem.Checked = true;
            currentDrawing = DrawingTypes.Capsule;
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in shapesToolStripMenuItem.DropDownItems)
                item.Checked = false;

            rectangleToolStripMenuItem.Checked = true;
            currentDrawing = DrawingTypes.Rectangle;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button3.BackColor = colorDialog1.Color;
                currentColor = colorDialog1.Color;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            currentThickness = (int)numericUpDown1.Value;
        }

        private void clearAllButton_Click(object sender, EventArgs e)
        {
            drawings.Clear();
            Redraw();
            clickedPoints.Clear();
        }

        private void removeSelectedButton_Click(object sender, EventArgs e)
        {
            if (drawings.Count > 0)
            {
                drawings.RemoveAt(shapeListBox.SelectedIndex);
                Redraw();
            }
        }

        private void changeSelectedColorButton_Click(object sender, EventArgs e)
        {
            // edit selected color and thickness
            if (drawings.Count > 0)
            {
                Drawing selectedDrawing = drawings.ElementAt(shapeListBox.SelectedIndex);
                selectedDrawing.drawingColor = colorDialog1.Color;
                if (selectedDrawing.GetType() == typeof(Line))
                {
                    Line line = (Line)selectedDrawing;
                    line.thickness = currentThickness;
                }


                else if (selectedDrawing.GetType() == typeof(Polygon))
                {
                    Polygon polygon = (Polygon)selectedDrawing;
                    polygon.thickness = currentThickness;
                }

                Redraw();
            }
        }

        private void aaCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Redraw();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                Stream outStream;
                saveFileDialog.DefaultExt = ".drawing";
                saveFileDialog.Filter = "drawing file (*.drawing)|*.drawing";
                saveFileDialog.AddExtension = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if ((outStream = saveFileDialog.OpenFile()) != null)
                    {
                        MemoryStream listStream = new MemoryStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(listStream, drawings);
                        listStream.Seek(0, SeekOrigin.Begin);
                        listStream.CopyTo(outStream);
                        listStream.Close();
                        outStream.Close();
                    }
                }

            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "drawing file (*.drawing)|*.drawing";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    drawings = (BindingList<Drawing>)formatter.Deserialize(openFileDialog.OpenFile());
                    Redraw();
                }

            }
        }


        // Edit of the existing shapes
        private void mainPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (dragCheckBox.Checked && drawings.Count != 0)
            {
                
                double distance = double.MaxValue;
                // get closest shape 
                foreach (Drawing d in drawings)
                {
                    if (e.Location.getAbsoluteDistance(d.GetClosestVertex(e.Location)) < distance)
                    {
                        distance = e.Location.getAbsoluteDistance(d.GetClosestVertex(e.Location));
                        vertexForMove = d.GetClosestVertex(e.Location);
                        drawingForMove = d;
                    }
                }
                // check if the shape is really close
                if (distance > 20)
                    drawingForMove = null;

            }
        }

        private void mainPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragCheckBox.Checked && drawings.Count != 0 && drawingForMove != null)
            {
                drawingForMove.SetVertexAt(vertexForMove, e.Location);
                Redraw();
            }
        }

        private void fillButton_Click(object sender, EventArgs e)
        {
            if(shapeListBox.SelectedItem.GetType() == typeof(Polygon))
            {
                Polygon p = (Polygon)shapeListBox.SelectedItem;
                p.filled = true;
                Redraw();
            }
        }

        private void clipButton_Click(object sender, EventArgs e)
        {
            clickedPoints.Clear();
            clipingMode = true;
        }

        private void shapeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            // disabling if not polygon
            if(shapeListBox.SelectedItem == null)
            {
                fillButton.Enabled = false;
                clipButton.Enabled = false;
                return;
            }
            if (shapeListBox.SelectedItem.GetType() == typeof(Polygon))
            {
                fillButton.Enabled = true;
                clipButton.Enabled = true;
            }
            else
            {
                fillButton.Enabled = false;
                clipButton.Enabled = false;
            }
            
        }
    }
}