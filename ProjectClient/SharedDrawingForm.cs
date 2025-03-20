using AForge.Imaging;
using ProjectClient.CameraAndRecognizing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class SharedDrawingForm : Form
    {// this form is incharge of the Shared Drawing Board
        public Point? calibrationPoint = null;
        public DateTime calibrationTime = DateTime.MinValue;
        private MarkerRecognizer markerRecognizer => cameraManager?.markerRecognizer;

        /// <summary>
        /// object incharge of communication with the server
        /// </summary>
        public TcpServerCommunication tcpServer;
        /// <summary>
        /// object incharge of managing the camera
        /// </summary>
        private CameraManager cameraManager;
        /// <summary>
        /// object incharge of managing the drawing
        /// </summary>
        public DrawingManager drawingManager;

        // Add a boolean to track calibration mode
        public bool isCalibrationMode = false;

        private bool isMarkerDrawingEnabled = false;
        private Point? lastMarkerPosition = null;

        /// <summary>
        /// constructor, inizialises form
        /// </summary>
        /// <param name="tcpServer"></param>
        /// <param name="username1"></param>
        public SharedDrawingForm(TcpServerCommunication tcpServer, string username1)
        {
            try
            {
                InitializeComponent();
                this.tcpServer = tcpServer;
                username.Text = username1;
                MessageHandler.SetCurrentForm(this);
                drawingManager = new DrawingManager(drawingPic);
                cameraManager = new CameraManager(Camera,drawingManager, this);
                drawingManager.DrawingActionPerformed += DrawingManager_DrawingActionPerformed;//This line sets up a connection (subscription) to listen for when drawing actions happens.

                tcpServer.SendMessage("RequestFullDrawingState", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing SharedDrawingForm: {ex.Message}");
            }
        }
        /// <summary>
        /// event called when pressing on the ShowCamera button, it starts/stops the camera depends on current state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCamera_Click(object sender, EventArgs e)
        {
            try
            {
                if (ShowCamera.Text == "Start Camera")
                {
                    if (cameraManager.StartCamera())
                    {
                        ShowCamera.Text = "Dont Show Camera";
                        Camera.Visible = true; // Make sure the camera is visible
                        Label3.Visible = true;
                        StartDrawing.Visible = true;
                        btnCalibrateColor.Visible = true; // Show calibration button
                    }
                }
                else
                {
                    Camera.Visible = false;
                    ShowCamera.Text = "Start Camera"; // Reset button text
                    cameraManager.StopCamera(); // Stop the camera when hiding
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Camera error: {ex.Message}");
            }
        }


        /// <summary>
        /// after action is performed this function is called, it sends the server the action serialized
        /// it is called automatically thanks to line 40
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">in this case it contains the object of the DrawingAction</param>
        private void DrawingManager_DrawingActionPerformed(object sender, DrawingAction e)
        {
            try
            {
                tcpServer.SendMessage("DrawingAction", e.Serialize());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending drawing action: {ex.Message}");
            }
        }
        /// <summary>
        /// Handles the received drawing state by converting a Base64 encoded image string back into a bitmap
        /// and updating the drawing board.
        /// it is doing so by -> converting the base64 string back to byteArray -> creates a memory stream and puts all the bytes there
        /// -> creates a bitmap with the stream -> updates the pictureBox's image with the bitmap. and then refreshes
        /// </summary>
        /// <param name="base64Image"></param>
        public void HandleFullDrawingState(string base64Image)
        {
            try
            {
                byte[] imageData = Convert.FromBase64String(base64Image);
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    Bitmap receivedBitmap = new Bitmap(ms);
                    drawingPic.Image = receivedBitmap;
                    drawingManager.drawingBitmap = receivedBitmap;  // Add this line if bm is accessible, or create a method in DrawingManager to update the bitmap
                    drawingPic.Refresh();  // Add this line
                    drawingPic.Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling full drawing state: {ex.Message}");
            }
        }
        /// <summary>
        /// this function sends to the server the drawing canvas, to later be sent to the one who asked for it.
        /// it is done by taking the current canvas and transfering it all to a MemorySteam,
        /// then taking it from the MemoryStream into a bytes array and then to base64 and then sending it to the server. with the nickname of the one who asked for it
        /// </summary>
        /// <param name="clientNick"></param>
        public void SendFullDrawingState(string clientNick)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    drawingPic.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);
                    tcpServer.SendMessage("SendFullDrawingState", $"{clientNick}\t{base64Image}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending full drawing state: {ex.Message}");
            }
        }
        /// <summary>
        /// this function is called when the user receives a drawingAction that another user performed, it will be accepted from the server.
        /// it calles the function "ApplyDrawingAction" that exists in the "DrawingManager" class,
        /// that function is responsible for actually applying the action on the canvas.
        /// </summary>
        /// <param name="action"></param>
        public void ApplyDrawingAction(DrawingAction action)
        {
            try
            {
                drawingManager.ApplyDrawingAction(action);
                drawingPic.Refresh();  // Add this line
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying drawing action: {ex.Message}");
            }
        }
        /// <summary>
        /// event called when the users starts holding on the canvas, it sets the drawing to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingPic_MouseDown(object sender, MouseEventArgs e)
        {
            drawingManager.isDrawing = true;
            drawingManager.lastDrawingPoint = e.Location;
        }
        /// <summary>
        /// event called when the users stops holding on the canvas, it sets the drawing to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingPic_MouseUp(object sender, MouseEventArgs e)
        {
            drawingManager.isDrawing = false;
            
        }
        /// <summary>
        /// event called when someone is moving their mouse on the pictureBox canvas, it will only do something when the user is holding.
        /// it calles the function "Draw" which is drawing locally, and it sends the server the drawingAction,
        /// which the server then sends it to the rest of the clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingPic_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawingManager.isDrawing)
            {
                DrawingAction action = drawingManager.Draw(e.Location, drawingManager.lastDrawingPoint);
                if (action != null)
                {
                    tcpServer.SendMessage("DrawingAction", action.Serialize());
                }
                drawingManager.lastDrawingPoint = e.Location;

                // Ensure UI update is on the UI thread
                if (drawingPic.InvokeRequired)
                {
                    drawingPic.Invoke(new Action(() => drawingPic.Invalidate()));
                }
                else
                {
                    drawingPic.Invalidate();
                }
            }
        }
        /// <summary>
        /// event called when pressing on the eraser, it sets drawing mode to eraser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_eraser_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(2);
        }
        /// <summary>
        /// event called when pressing on the clear button, it clears the canvas to all white.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            try
            {
                drawingManager.Clear();
                drawingPic.Invalidate();
                DrawingAction clearAction = new DrawingAction { Type = "Clear" };
                tcpServer.SendMessage("DrawingAction", clearAction.Serialize());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing drawing: {ex.Message}");
            }
        }
        /// <summary>
        /// event called when user presses on color_picker picture box,
        /// it takes the location he pressed on and the pixel of that location and color and changes the pencils color to the color, (function called PickColor)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Color selectedColor = drawingManager.PickColor(color_picker, e.Location);
            pic_color.BackColor = selectedColor;
            drawingManager.SetPenColor(selectedColor);
        }
        /// <summary>
        /// event called when user presses on pencil button, it sets drawing mode to pencil
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_pencil_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(1);
        }
        /// <summary>
        /// event called when pressed on the fill button, changes drawing mode to fill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_fill_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(7);
        }
        /// <summary>
        /// event called when the user changes the pen size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(PenSize.Text, out float size))
            {
                drawingManager.SetPenSize(size);
            }
        }
        /// <summary>
        /// event called when the user changes the eraser size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EraserSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(EraserSize.Text, out float size))
            {
                drawingManager.SetEraserSize(size);
            }
        }
        /// <summary>
        /// event called  when clicking on the drawing, it will only do something when the drawing mode is 7,
        /// it calles the function FillAsync which is responsible for the actual filling, 
        /// and it sends the server a notice that a fill action was performed, the server then sends it to the rest of the users
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void drawingPic_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawingManager.currentToolMode == 7)  // Fill mode
            {
                Color fillColor = pic_color.BackColor;
                DrawingAction fillAction = new DrawingAction
                {
                    Type = "Fill",
                    StartPoint = e.Location,
                    Color = fillColor
                };

                try
                {
                    await drawingManager.FillAsync(e.Location, fillColor);
                    tcpServer.SendMessage("DrawingAction", fillAction.Serialize());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during fill operation: {ex.Message}", "Fill Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        /// <summary>
        /// event that happens when they close form, its main purpose is to stop camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharedDrawingForm_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            cameraManager.StopCamera();
        }

        private void StartDrawing_Click(object sender, EventArgs e)
        {
            try
            {
                if (StartDrawing.Text == "Start Drawing")
                {
                    StartDrawing.Text = "Stop Drawing";
                    isMarkerDrawingEnabled = true;
                    lastMarkerPosition = null; // Reset the last position when starting a new drawing session

                    // ADDED: Update marker recognizer with drawing status
                    markerRecognizer.SetDrawingStatus(true);

                    // Make sure the camera stays visible during drawing
                    Camera.Visible = true;
                }
                else
                {
                    StartDrawing.Text = "Start Drawing";
                    isMarkerDrawingEnabled = false;
                    lastMarkerPosition = null; // Reset when stopping

                    // ADDED: Update marker recognizer with drawing status
                    markerRecognizer.SetDrawingStatus(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in StartDrawing_Click: " + ex.Message);
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnCalibrateColor_Click(object sender, EventArgs e)
        {
            // Enter calibration mode
            isCalibrationMode = true;
            calibrationPoint = null; // Reset any previous calibration point
            btnCalibrateColor.Text = "Click on Marker";

            // Show instructions in a non-blocking way
            Label3.Text = "Click on your marker in the camera view";
            Label3.ForeColor = Color.Yellow;
        }

        private void Camera_Click(object sender, EventArgs e)
        {
            if (isCalibrationMode)
            {
                try
                {
                    // Convert EventArgs to MouseEventArgs to get the click location
                    if (e is MouseEventArgs mouseEvent)
                    {
                        // Debug
                        Console.WriteLine($"Clicked {mouseEvent.X},{mouseEvent.Y}");

                        if (Camera.Image == null) return;
                        Bitmap bmp = (Bitmap)Camera.Image;
                        Console.WriteLine($"Image size = {bmp.Width}x{bmp.Height}");

                        // Are we within range?
                        if (mouseEvent.X < 0 || mouseEvent.X >= bmp.Width ||
                            mouseEvent.Y < 0 || mouseEvent.Y >= bmp.Height)
                        {
                            Console.WriteLine("Clicked out of bounds!");
                            return;  // Don't calibrate if out-of-bounds
                        }

                        // Store calibration point for drawing
                        calibrationPoint = new Point(mouseEvent.X, mouseEvent.Y);
                        calibrationTime = DateTime.Now;

                        // Try calibration
                        cameraManager.CalibrateColorTracking(new System.Drawing.Point(mouseEvent.X, mouseEvent.Y));

                        // Update UI to show calibration is done
                        isCalibrationMode = false;
                        btnCalibrateColor.Text = "Calibrate Marker Color";
                        btnCalibrateColor.Visible = true;
                        Label3.Text = "Camera View - Calibration successful!";
                        Label3.ForeColor = Color.Lime;

                        // After 3 seconds, revert the label
                        System.Threading.Tasks.Task.Delay(3000).ContinueWith(t =>
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                Label3.Text = "Camera View";
                                Label3.ForeColor = SystemColors.ControlText;
                            }));
                        });

                        Console.WriteLine("Color calibration completed successfully");
                    }
                    else
                    {
                        // Exit calibration mode
                        isCalibrationMode = false;
                        btnCalibrateColor.Text = "Calibrate Marker Color";
                        btnCalibrateColor.Visible = true;
                        Label3.Text = "Camera View";
                        Console.WriteLine("Calibration failed - not a mouse event");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Calibration error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);

                    // Exit calibration mode even on error
                    isCalibrationMode = false;
                    btnCalibrateColor.Text = "Calibrate Marker Color";
                    btnCalibrateColor.Visible = true;
                    Label3.Text = "Camera View - Calibration failed";
                    Label3.ForeColor = Color.Red;
                }
            }
           }
        public void ProcessMarkerPosition(Point markerPosition, Size cameraSize)
        {
            if (!isMarkerDrawingEnabled)
                return;

            try
            {
                // Keep the camera visible during marker tracking
                if (!Camera.Visible)
                {
                    Camera.Visible = true;
                }

                // Convert camera coordinates to drawing canvas coordinates
                Point drawingCanvasPoint = TranslateCoordinates(markerPosition, cameraSize, drawingPic.Size);

                // Log for debugging
                if (DateTime.Now.Millisecond < 100) // Limit logging to reduce console spam
                {
                    Console.WriteLine($"Marker at camera({markerPosition.X},{markerPosition.Y}) -> drawing({drawingCanvasPoint.X},{drawingCanvasPoint.Y})");
                }

                // If we have a previous position, draw a line from it to the current position
                if (lastMarkerPosition.HasValue)
                {
                    // Convert previous marker position to drawing coordinates
                    Point prevDrawingPoint = TranslateCoordinates(lastMarkerPosition.Value, cameraSize, drawingPic.Size);

                    // Use the existing drawing mechanism to draw a line
                    DrawingAction action = drawingManager.Draw(drawingCanvasPoint, prevDrawingPoint);
                    if (action != null)
                    {
                        // Send the drawing action to server (for multiplayer)
                        tcpServer.SendMessage("DrawingAction", action.Serialize());
                    }

                    // Make sure the UI is updated
                    if (drawingPic.InvokeRequired)
                    {
                        drawingPic.BeginInvoke(new Action(() => drawingPic.Invalidate()));
                    }
                    else
                    {
                        drawingPic.Invalidate();
                    }
                }

                // Update the last known position
                lastMarkerPosition = markerPosition;
            }
            catch (Exception ex)
            {
                // Just log the error, don't disrupt the drawing flow
                Console.WriteLine($"Error processing marker position: {ex.Message}");
            }
        }
        private Point TranslateCoordinates(Point source, Size sourceSize, Size targetSize)
        {
            // Calculate coordinates normally now that the camera view is already flipped
            int x = (int)((float)source.X / sourceSize.Width * targetSize.Width);
            int y = (int)((float)source.Y / sourceSize.Height * targetSize.Height);

            // Ensure coordinates are within bounds
            x = Math.Max(0, Math.Min(x, targetSize.Width - 1));
            y = Math.Max(0, Math.Min(y, targetSize.Height - 1));

            // Log occasionally for debugging
            if (DateTime.Now.Millisecond < 50)
            {
                Console.WriteLine($"Translating: Camera({source.X},{source.Y}) -> Canvas({x},{y})");
            }

            return new Point(x, y);
        }

    }
}
