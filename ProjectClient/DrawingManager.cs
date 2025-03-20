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
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.LinkLabel;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectClient
{
    public class DrawingManager
    {//this class is incharge of managing the drawing

        /// <summary>
        /// The bitmap that stores the current drawing state
        /// </summary>
        public Bitmap drawingBitmap;
        /// <summary>
        /// Graphics object used for drawing operations on the bitmap
        /// </summary>
        private Graphics drawingGraphics;
        /// <summary>
        ///  The PictureBox control that displays the drawing
        /// </summary>
        public PictureBox drawingCanvas;
        /// <summary>
        /// Pen used for drawing operations
        /// </summary>
        public Pen drawingPen;
        /// <summary>
        /// Pen used for erasing operations
        /// </summary>
        public Pen eraser;
        /// <summary>
        /// Current active tool mode (0: None, 1: Pencil, 2: Eraser, 7: Fill)
        /// </summary>
        public int currentToolMode = 0;
        /// <summary>
        /// Last recorded point in drawing operation
        /// </summary>
        public Point lastDrawingPoint;
        /// <summary>
        /// Indicates whether a drawing operation is currently in progress
        /// </summary>
        public bool isDrawing = false;
        /// <summary>
        /// Event that fires when a drawing action is performed
        /// </summary>
        public event EventHandler<DrawingAction> DrawingActionPerformed;
        /// <summary>
        /// Lock for thread-safe drawing operations, protecting the bitmap while filling operation
        /// </summary>
        private ReaderWriterLockSlim drawingLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the DrawingManager class
        /// </summary>
        /// <param name="drawingPic"></param>
        public DrawingManager(PictureBox drawingPic)
        {
            this.drawingCanvas = drawingPic;
            InitializeDrawing();
        }
        /// <summary>
        /// Initializes the drawing surface and tools
        /// </summary>
        private void InitializeDrawing()
        {
            drawingBitmap = new Bitmap(drawingCanvas.Width, drawingCanvas.Height);
            drawingGraphics = Graphics.FromImage(drawingBitmap);
            drawingGraphics.Clear(Color.White);
            drawingCanvas.Image = drawingBitmap;

            drawingPen = new Pen(Color.Black, 1);
            drawingPen.StartCap = LineCap.Round;
            drawingPen.EndCap = LineCap.Round;

            eraser = new Pen(Color.White, 10);
            eraser.StartCap = LineCap.Round;
            eraser.EndCap = LineCap.Round;
        }


        /// <summary>
        /// Applies a drawing action to the canvas. This method handles various drawing operations
        /// including drawing lines, erasing, filling areas, and clearing the canvas. It ensures thread-safety
        /// during the drawing operation using a write lock.
        /// this function is called every time this user receives a drawing operation from the server,
        /// the drawing operation is another user drawing something
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ApplyDrawingAction(DrawingAction action)
        {
            drawingLock.EnterWriteLock();
            try
            {
                using (Graphics graphics = Graphics.FromImage(drawingBitmap))
                {
                    // Enable anti-aliasing for smooth drawing
                    //When you draw a line, instead of making it pure pixels (jagged), it adds partially transparent pixels along the edges
                    //This makes lines and shapes appear smoother and more natural
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    switch (action.Type)
                    {
                        case "DrawLine":
                            using (Pen actionPen = new Pen(action.Color, action.Size))
                            {
                                actionPen.StartCap = LineCap.Round;// Round start of line
                                actionPen.EndCap = LineCap.Round;// Round end of line
                                graphics.DrawLine(actionPen, action.StartPoint, action.EndPoint);
                            }
                            break;
                        case "Erase":
                            using (Pen actionEraser = new Pen(Color.White, action.Size))
                            {
                                actionEraser.StartCap = LineCap.Round;// Round start of line
                                actionEraser.EndCap = LineCap.Round;// Round end of line
                                graphics.DrawLine(actionEraser, action.StartPoint, action.EndPoint);
                            }
                            break;
                        case "Fill":
                            FillArea(drawingBitmap, action.StartPoint.X, action.StartPoint.Y, action.Color);
                            break;
                        case "Clear":
                            graphics.Clear(Color.White);
                            break;
                    }
                }
                drawingCanvas.Image = drawingBitmap;
            }
            finally
            {// Always release the write lock, even if an exception occurs
                drawingLock.ExitWriteLock();
            }
            drawingCanvas.Invalidate();
        }
        /// <summary>
        /// Performs a drawing operation on the canvas based on the current tool mode and cursor position.
        /// this function is activated when the current user is trying to do a drawing operation on the canvas
        /// </summary>
        /// <param name="currentLocation">current location will be the location when the function is called</param>
        /// <param name="previousLocation"> previous location will be the location where the user puts his mouse down</param>
        /// <returns></returns>
        public DrawingAction Draw(Point currentLocation, Point previousLocation)
        {
            drawingLock.EnterWriteLock();
            try
            {
                if (currentToolMode == 1 || currentToolMode == 2)
                {
                    Point startPoint = previousLocation;
                    Point endPoint = currentLocation;

                    using (Graphics graphics = Graphics.FromImage(drawingBitmap))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;

                        if (currentToolMode == 1)
                        {
                            graphics.DrawLine(drawingPen, startPoint, endPoint);
                        }
                        else if (currentToolMode == 2)
                        {
                            graphics.DrawLine(eraser, startPoint, endPoint);
                        }
                    }

                    drawingCanvas.Image = drawingBitmap;  // Add this line
                    drawingCanvas.Invalidate();

                    DrawingAction action = new DrawingAction
                    {
                        Type = currentToolMode == 1 ? "DrawLine" : "Erase",
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        Color = currentToolMode == 1 ? drawingPen.Color : Color.White,
                        Size = currentToolMode == 1 ? drawingPen.Width : eraser.Width
                    };


                    return action;
                }
                return null;
            }
            finally
            {
                drawingLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Clears the drawing canvas to white
        /// </summary>
        public void Clear()
        {
            drawingGraphics.Clear(Color.White);
            currentToolMode = 0;
            drawingCanvas.Invalidate();
        }
        /// <summary>
        /// Sets the color of the drawing pen 
        /// </summary>
        /// <param name="color"></param>
        public void SetPenColor(Color color)
        {
            drawingPen.Color = color;
        }
        /// <summary>
        /// Sets the size of the drawing pen
        /// </summary>
        /// <param name="size"></param>
        public void SetPenSize(float size)
        {
            drawingPen.Width = size;
        }
        /// <summary>
        /// Sets the size of the eraser
        /// </summary>
        /// <param name="size"></param>
        public void SetEraserSize(float size)
        {
            eraser.Width = size;
        }
        /// <summary>
        /// Sets the current drawing tool mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetDrawingMode(int mode)
        {
            currentToolMode = mode;
        }
        /// <summary>
        /// Picks a color from a specific point on a color picker, there is a picturebox with different boxes of colors,
        /// and when a user presses on one it takes the point he pressed on and checks the colors on the pixel
        /// </summary>
        /// <param name="colorPicker"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public Color PickColor(PictureBox colorPicker, Point location)
        {
            Point point = Set_Point(colorPicker, location);
            return ((Bitmap)colorPicker.Image).GetPixel(point.X, point.Y);
        }
        /// <summary>
        /// Asynchronously performs a flood fill operation at the specified location with the given color.
        /// this method performs these operations:
        /// 1. Runs the fill operation on a background thread to prevent UI freezing
        /// 2. Uses thread-safe drawing operations with a write lock
        /// 3. Updates the UI safely using cross-thread invocation when needed
        /// </summary>
        /// <param name="location"></param>
        /// <param name="fillColor"></param>
        /// <returns></returns>
        public async Task FillAsync(Point location, Color fillColor)
        {
            await Task.Run(() =>
            {
                drawingLock.EnterWriteLock();
                try
                {
                    FillArea(drawingBitmap, location.X, location.Y, fillColor);
                }
                finally
                {
                    drawingLock.ExitWriteLock();
                }
            });

            // Update UI on the UI thread
            if (drawingCanvas.InvokeRequired)
            {
                drawingCanvas.Invoke(new Action(() => drawingCanvas.Invalidate()));
            }
            else
            {
                drawingCanvas.Invalidate();
            }
        }
        /// <summary>
        /// help function for the FillAsync. this function implements the fill operation.
        /// it does so by getting the current pixel color, it saves the old color, and changes it to the new one,
        /// then, it checks boundries, and adds the 4 neighboring pixels to check next,
        /// the same operation goes on until there are no more pixels to check
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newColor"></param>
        private void FillArea(Bitmap bm, int x, int y, Color newColor)
        // flood fill algorithem. used commonly in drawing applications for the "fill" and in game development for revealing areas
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

        /// <summary>
        /// The Set_Point function converts where you click on the screen to the correct position on your actual drawing (image).
        /// Think of it like this: if your drawing is displayed twice as big on the screen,
        /// when you click at position 200, the function knows to actually draw at position 100 on your real image.
        /// It does this by figuring out how much bigger or smaller your display (PictureBox) is compared to your actual drawing (Image),
        /// then adjusts your click position accordingly.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Point Set_Point(PictureBox pb, Point pt)
        {
            drawingCanvas.Invalidate();
            float px = 1f * pb.Image.Width / pb.Width;
            float py = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * px), (int)(pt.Y * py));
        }

    }
}

