using ClipperLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipperTest
{
    public partial class Form1 : Form
    {
        private List<List<IntPoint>> allPolygons = new List<List<IntPoint>>();
        private List<List<IntPoint>> clippedPolygons = new List<List<IntPoint>>();

        private List<IntPoint> newPolygon = null;
        private bool tempPoint = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void pbSource_Click(object sender, EventArgs e)
        {

        }

        private void pbSource_MouseDown(object sender, MouseEventArgs e)
        {
            if (null == newPolygon)
                return;

            newPolygon.Add(new IntPoint(e.X, e.Y));
            tempPoint = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (null == newPolygon)
                newPolygon = new List<IntPoint>();
            else
            {
                if (newPolygon.Count < 3)
                    return;

                if (tempPoint)
                    newPolygon.RemoveAt(newPolygon.Count - 1);

                allPolygons.Add(newPolygon);
                newPolygon = null;
                RedrawSource();
            }
        }

        private void RedrawSolution()
        {
            var formGraphics = pbSolution.CreateGraphics();
            formGraphics.Clear(Color.White);
            foreach (var poly in clippedPolygons)
                DrawPolygon(pbSolution, poly);
        }

        private void RedrawSource()
        {
            var formGraphics = pbSource.CreateGraphics();
            formGraphics.Clear(Color.White);
            foreach (var poly in allPolygons)
                DrawPolygon(pbSource, poly);

            DrawPolygon(pbSource, newPolygon, false);
        }

        private void DrawPolygon(PictureBox pb, List<IntPoint> polygon, bool filled = true)
        {
            if (null == polygon || polygon.Count < 3)
                return;

            var formGraphics = pb.CreateGraphics();
        
            Point[] points = new Point[polygon.Count];
            for (var i = 0; i < polygon.Count; i++)
            {
                points[i] = new Point((int)polygon[i].X, (int)polygon[i].Y);
            }

            var myBrush = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
            var myPen = new Pen(Color.FromArgb(255, 0, 0, 255));
            if (filled)
            {
                formGraphics.FillPolygon(myBrush, points);
                formGraphics.DrawPolygon(myPen, points);
            }
            else
            {
                formGraphics.DrawPolygon(myPen, points);
            }
            myBrush.Dispose();
            myPen.Dispose();
            formGraphics.Dispose();
        }

        private void pbSource_MouseMove(object sender, MouseEventArgs e)
        {
            if (null == newPolygon || newPolygon.Count == 0)
                return;

            Point end = new Point(e.X, e.Y);

            if (tempPoint)
                newPolygon.RemoveAt(newPolygon.Count - 1);

            newPolygon.Add(new IntPoint(end.X, end.Y));
            tempPoint = true;

            RedrawSource();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
        }

        private void pbSource_Resize(object sender, EventArgs e)
        {
        }

        private void pbSource_SizeChanged(object sender, EventArgs e)
        {
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            RedrawSource();
        }

        private void pbSource_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClipPolygons();
        }

        private void ClipPolygons()
        {
            clippedPolygons = new List<List<IntPoint>>();
            clippedPolygons.AddRange(allPolygons);
            bool collectionModified = true;
            while (collectionModified)
            {
                collectionModified = false;
                for (var oPoly = 0; oPoly < clippedPolygons.Count; oPoly++)
                {
                    var poly1 = clippedPolygons[oPoly];

                    for (var iPoly = 0; iPoly < clippedPolygons.Count; iPoly++)
                    {
                        if (oPoly == iPoly)
                            continue;

                        var poly2 = clippedPolygons[iPoly];

                        Clipper clip = new Clipper();
                        clip.AddPath(poly1, PolyType.ptSubject, true);
                        clip.AddPath(poly2, PolyType.ptClip, true);

                        List<List<IntPoint>> solution = new List<List<IntPoint>>();

                        clip.Execute(ClipType.ctIntersection, solution);

                        if (solution.Count == 0)
                            continue;

                        foreach (var intersection in solution)
                        {
                            clippedPolygons.Add(intersection);
                        }

                        solution = new List<List<IntPoint>>();
                        clip.Clear();
                        clip.AddPath(poly1, PolyType.ptSubject, true);
                        clip.AddPath(poly2, PolyType.ptClip, true);

                        clip.Execute(ClipType.ctDifference, solution);

                        foreach (var difference in solution)
                        {
                            clippedPolygons.Add(difference);
                        }

                        solution = new List<List<IntPoint>>();
                        clip.Clear();
                        clip.AddPath(poly1, PolyType.ptClip, true);
                        clip.AddPath(poly2, PolyType.ptSubject, true);

                        clip.Execute(ClipType.ctDifference, solution);

                        foreach (var difference in solution)
                        {
                            clippedPolygons.Add(difference);
                        }

                        clippedPolygons.Remove(poly1);
                        clippedPolygons.Remove(poly2);
                        collectionModified = true;
                        RedrawSolution();
                        Thread.Sleep(500);
                        break;
                    }

                    if (collectionModified)
                        break;
                }
            }
        }
    }
}
