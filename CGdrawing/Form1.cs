using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CGdrawing
{
    public partial class Form1 : Form
    {
        BindingList<Drawing> drawings = new BindingList<Drawing>();
        List<Point> clickedPoints = new List<Point>();
        DrawingTypes currentDrawing = DrawingTypes.Line;
        Color currentColor = Color.Black;
        int currentThickness = 1;

        // moving related variables
        Drawing drawingForMove;
        Point vertexForMove;
       

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
        private void Redraw()
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

                        bmp.SetPixelFast(point.X, point.Y, d.drawingColor);
                    }
                }

            }
            mainPictureBox.Image = bmp;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void mainPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            // dont draw if its move mode
            if (dragCheckBox.Checked)
                return;

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
            polygonToolStripMenuItem.Checked = false;
            circleToolStripMenuItem.Checked = false;
            currentDrawing = DrawingTypes.Line;
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polygonToolStripMenuItem.Checked = false;
            lineToolStripMenuItem.Checked = false;
            currentDrawing = DrawingTypes.Circle;
        }

        private void polygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            circleToolStripMenuItem.Checked = false;
            lineToolStripMenuItem.Checked = false;
            currentDrawing = DrawingTypes.Polygon;
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
    }
}