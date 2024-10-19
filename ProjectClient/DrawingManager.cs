using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Threading;

namespace ProjectClient
{
    public class DrawingManager
    {//this class is incharge of managing the drawing
        private Bitmap bm;
        private Graphics g;
        public PictureBox drawingPic;
        private TcpServerCommunication tcpServer;
        private Pen pen;
        private Pen eraser;
        public int index = 0;
        private Color new_color;

        public event EventHandler<DrawingAction> DrawingActionPerformed;
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private readonly object bitmapLock = new object();

        public DrawingManager(PictureBox drawingPic, TcpServerCommunication tcpServer)
        {
            this.drawingPic = drawingPic;
            this.tcpServer = tcpServer;
            InitializeDrawing();
        }

        private void InitializeDrawing()
        {
            bm = new Bitmap(drawingPic.Width, drawingPic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(Color.White);
            drawingPic.Image = bm;
            new_color = Color.Black;

            pen = new Pen(Color.Black, 1);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            eraser = new Pen(Color.White, 10);
            eraser.StartCap = LineCap.Round;
            eraser.EndCap = LineCap.Round;
        }

        public DrawingAction Draw(Point currentLocation, Point previousLocation)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (index == 1 || index == 2)
                {
                    Point startPoint = Set_Point(bm, previousLocation);
                    Point endPoint = Set_Point(bm, currentLocation);

                    using (Graphics graphics = Graphics.FromImage(bm))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;

                        if (index == 1)
                        {
                            graphics.DrawLine(pen, startPoint, endPoint);
                        }
                        else if (index == 2)
                        {
                            graphics.DrawLine(eraser, startPoint, endPoint);
                        }
                    }

                    drawingPic.Invalidate();

                    DrawingAction action = new DrawingAction
                    {
                        Type = index == 1 ? "DrawLine" : "Erase",
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        Color = index == 1 ? pen.Color : Color.White,
                        Size = index == 1 ? pen.Width : eraser.Width
                    };

                    OnDrawingActionPerformed(action);

                    return action;
                }
                return null;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void ApplyDrawingAction(DrawingAction action)
        {
            rwLock.EnterWriteLock();
            try
            {
                using (Graphics graphics = Graphics.FromImage(bm))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    switch (action.Type)
                    {
                        case "DrawLine":
                            using (Pen actionPen = new Pen(action.Color, action.Size))
                            {
                                graphics.DrawLine(actionPen, action.StartPoint, action.EndPoint);
                            }
                            break;
                        case "Erase":
                            using (Pen actionEraser = new Pen(Color.White, action.Size))
                            {
                                graphics.DrawLine(actionEraser, action.StartPoint, action.EndPoint);
                            }
                            break;
                        case "Fill":
                            FillArea(bm, action.StartPoint.X, action.StartPoint.Y, action.Color);
                            break;
                        case "Clear":
                            graphics.Clear(Color.White);
                            break;
                    }
                }
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
            drawingPic.Invalidate();
        }

        public void SetFullDrawingState(Bitmap receivedBitmap)
        {
            rwLock.EnterWriteLock();
            try
            {
                g.Dispose();
                bm.Dispose();

                bm = new Bitmap(receivedBitmap, drawingPic.Width, drawingPic.Height);
                g = Graphics.FromImage(bm);
                drawingPic.Image = bm;
                drawingPic.Invalidate();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            g.Clear(Color.White);
            index = 0;
            drawingPic.Invalidate();
        }

        public void SetPenColor(Color color)
        {
            new_color = color;
            pen.Color = color;
        }

        public void SetPenSize(float size)
        {
            pen.Width = size;
        }

        public void SetEraserSize(float size)
        {
            eraser.Width = size;
        }

        public void SetDrawingMode(int mode)
        {
            index = mode;
        }

        public Color PickColor(PictureBox colorPicker, Point location)
        {
            Point point = Set_Point(colorPicker, location);
            return ((Bitmap)colorPicker.Image).GetPixel(point.X, point.Y);
        }

        public async Task FillAsync(Point location, Color fillColor)
        {
            await Task.Run(() =>
            {
                Point point = Set_Point(bm, location);
                rwLock.EnterWriteLock();
                try
                {
                    FillArea(bm, point.X, point.Y, fillColor);
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            });

            // Update UI on the UI thread
            if (drawingPic.InvokeRequired)
            {
                drawingPic.Invoke(new Action(() => drawingPic.Invalidate()));
            }
            else
            {
                drawingPic.Invalidate();
            }
        }

        private void FillArea(Bitmap bm, int x, int y, Color newColor)
        {
            Color oldColor = bm.GetPixel(x, y);
            if (oldColor.ToArgb() == newColor.ToArgb()) return;

            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(new Point(x, y));

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();
                if (a.X < 0 || a.X >= bm.Width || a.Y < 0 || a.Y >= bm.Height) continue;

                if (bm.GetPixel(a.X, a.Y) == oldColor)
                {
                    bm.SetPixel(a.X, a.Y, newColor);
                    pixels.Push(new Point(a.X - 1, a.Y));
                    pixels.Push(new Point(a.X + 1, a.Y));
                    pixels.Push(new Point(a.X, a.Y - 1));
                    pixels.Push(new Point(a.X, a.Y + 1));
                }
            }
        }

        private Point Set_Point(Bitmap bm, Point pt)
        {
            float px = 1f * bm.Width / bm.Width;
            float py = 1f * bm.Height / bm.Height;
            return new Point((int)(pt.X * px), (int)(pt.Y * py));
        }

        private Point Set_Point(PictureBox pb, Point pt)
        {
            float px = 1f * pb.Image.Width / pb.Width;
            float py = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * px), (int)(pt.Y * py));
        }

        protected virtual void OnDrawingActionPerformed(DrawingAction e)
        {
            DrawingActionPerformed?.Invoke(this, e);
        }
    }
}

