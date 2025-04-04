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

        public MarkerRecognizer markerRecognizer => cameraManager?.markerRecognizer;
        private DateTime? markerLostStartTime = null;
        private int markerLostTimeoutSeconds = 3; // Auto-stop drawing after 3 seconds of marker loss
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

        public bool isMarkerDrawingEnabled = false;
        private Point? lastMarkerPosition = null;
        private bool isSpaceBarPressed = false;

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
                cameraManager = new CameraManager(Camera, drawingManager, this);
                drawingManager.DrawingActionPerformed += DrawingManager_DrawingActionPerformed;
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(StartDrawing, "Drawing will auto-stop if marker is lost for " +
                                                   markerLostTimeoutSeconds + " seconds");
                tcpServer.SendMessage("RequestFullDrawingState", "");

                // Enable key preview so the form receives key events
                this.KeyPreview = true;
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
                        cameraManager.SetQualitySettings(
                                        width: 960,          // Middle ground resolution
                                        height: 540,         // Middle ground resolution
                                        skipFrames: false,   // Or true if still laggy
                                        samplingStep: 2,     // Increase step to reduce processing load
                                        smoothing: true,
                                        smoothStrength: 0.5,
                                        minMoveThreshold: 2
                                    );
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
                    // Create a new bitmap from the received data
                    Bitmap receivedBitmap = new Bitmap(ms);

                    // Update the picture box
                    drawingPic.Image = receivedBitmap;

                    // Important: Create a new bitmap for the drawing manager to avoid sharing references
                    // which could cause issues with clearing
                    Bitmap managerBitmap = new Bitmap(receivedBitmap);
                    drawingManager.drawingBitmap = managerBitmap;

                    // Make sure UI is updated
                    drawingPic.Refresh();
                    drawingPic.Invalidate();

                    Console.WriteLine("Full drawing state applied, size: " + imageData.Length + " bytes");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling full drawing state: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Clear local canvas
                drawingManager.Clear();

                // Update the PictureBox with the new bitmap
                drawingPic.Image = drawingManager.drawingBitmap;

                // Force refresh
                drawingPic.Invalidate();

                // Create a clear action to send to server
                DrawingAction clearAction = new DrawingAction { Type = "Clear" };
                tcpServer.SendMessage("DrawingAction", clearAction.Serialize());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing drawing: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    // Show message box with instructions
                    MessageBox.Show("Hold Space Bar to draw with the marker", "Drawing Mode",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    StartDrawing.Text = "Stop Drawing";
                    isMarkerDrawingEnabled = true;
                    isSpaceBarPressed = false; // Make sure this is reset

                    // Set keyboard focus to this form to receive key events
                    this.Focus();
                    this.KeyPreview = true; // Important: Make sure the form receives key events

                    // Rest of your existing code here
                    lastMarkerPosition = null;
                    if (markerRecognizer != null)
                    {
                        markerRecognizer.SetDrawingStatus(true);
                        if (markerRecognizer.shapeRecognizer != null)
                        {
                            markerRecognizer.shapeRecognizer.ResetAdaptiveThreshold();
                        }
                        if (markerRecognizer.colorRecognizer != null)
                        {
                            markerRecognizer.colorRecognizer.ResetThresholds();
                        }
                        markerRecognizer.ResetMarkerLostStatus();
                        markerRecognizer.ResetPositionHistory();
                    }
                    Camera.Visible = true;
                    markerLostStartTime = null;

                    Console.WriteLine("Drawing mode started - all thresholds reset to default values");
                    Console.WriteLine("Hold space bar to draw");
                }
                else
                {
                    StartDrawing.Text = "Start Drawing";
                    isMarkerDrawingEnabled = false;
                    lastMarkerPosition = null;
                    markerLostStartTime = null;

                    // Rest of your existing code here
                    if (markerRecognizer != null)
                    {
                        markerRecognizer.SetDrawingStatus(false);
                        if (markerRecognizer.shapeRecognizer != null)
                        {
                            markerRecognizer.shapeRecognizer.ResetAdaptiveThreshold();
                        }
                        markerRecognizer.ResetPositionHistory();
                    }

                    Console.WriteLine("Drawing mode stopped");
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
            // Check if drawing mode is enabled (Start Drawing button pressed)
            if (!isMarkerDrawingEnabled)
                return;

            try
            {
                // Standard marker tracking code
                if (!Camera.Visible)
                {
                    Camera.Visible = true;
                }

                // Handle marker loss detection
                if (markerPosition.X == 0 && markerPosition.Y == 0)
                {
                    // Standard marker loss code
                    if (!markerLostStartTime.HasValue)
                    {
                        markerLostStartTime = DateTime.Now;
                    }
                    else
                    {
                        TimeSpan lostTime = DateTime.Now - markerLostStartTime.Value;
                        if (lostTime.TotalSeconds >= markerLostTimeoutSeconds)
                        {
                            // Auto-stop code remains the same
                            if (isMarkerDrawingEnabled && isSpaceBarPressed)
                            {
                                this.BeginInvoke(new Action(() => {
                                    StartDrawing_Click(StartDrawing, EventArgs.Empty);
                                    Label3.Text = "Drawing auto-stopped - marker lost";
                                    Label3.ForeColor = Color.Orange;
                                    System.Threading.Tasks.Task.Delay(3000).ContinueWith(t => {
                                        this.BeginInvoke(new Action(() => {
                                            Label3.Text = "Camera View";
                                            Label3.ForeColor = SystemColors.ControlText;
                                        }));
                                    });
                                }));
                            }
                            markerLostStartTime = null;
                        }
                    }
                    lastMarkerPosition = null;
                    return;
                }
                else
                {
                    markerLostStartTime = null;
                }

                // SIMPLIFIED APPROACH: Only proceed with drawing if space bar is pressed
                if (!isSpaceBarPressed)
                {
                    // Not drawing when space bar is not pressed
                    lastMarkerPosition = null; // Reset position when not drawing
                    return;
                }

                // Convert camera coordinates to drawing canvas coordinates
                Point drawingCanvasPoint = TranslateCoordinates(markerPosition, cameraSize, drawingPic.Size);

                // If we have a previous position, draw a line from it to the current position
                if (lastMarkerPosition.HasValue)
                {
                    // Convert previous marker position to drawing coordinates
                    Point prevDrawingPoint = TranslateCoordinates(lastMarkerPosition.Value, cameraSize, drawingPic.Size);

                    // Calculate distance between consecutive points
                    double distance = Math.Sqrt(
                        Math.Pow(prevDrawingPoint.X - drawingCanvasPoint.X, 2) +
                        Math.Pow(prevDrawingPoint.Y - drawingCanvasPoint.Y, 2));

                    // Only draw if the distance is reasonable
                    if (distance < drawingPic.Width * 0.3 && isSpaceBarPressed)
                    {
                        // Use the existing drawing mechanism to draw a line
                        DrawingAction action = drawingManager.Draw(drawingCanvasPoint, prevDrawingPoint);
                        if (action != null)
                        {
                            tcpServer.SendMessage("DrawingAction", action.Serialize());
                        }

                        // Update UI
                        if (drawingPic.InvokeRequired)
                        {
                            drawingPic.BeginInvoke(new Action(() => drawingPic.Invalidate()));
                        }
                        else
                        {
                            drawingPic.Invalidate();
                        }
                    }
                }

                // IMPORTANT: Always update the last position when space bar is pressed
                lastMarkerPosition = markerPosition;
            }
            catch (Exception ex)
            {
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

        private void SaveDrawing_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(drawingName.Text))
            {
                MessageBox.Show("Please enter a name for your drawing", "Name Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string image = GetImageAsString();
                if (image != null)
                {
                    tcpServer.SendMessage("SaveDrawing", image + '\t' + drawingName.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving drawing: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// this function returns the drawing canvas, to later be sent to the one who asked for it/to save the drawing .
        /// it is done by taking the current canvas and transfering it all to a MemorySteam,
        /// then taking it from the MemoryStream into a bytes array and then to base64 and then sending it to the server. with the nickname of the one who asked for it
        /// </summary>
        /// <param name="clientNick"></param>
        public string GetImageAsString()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    drawingPic.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);
                    return base64Image;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending full drawing state: {ex.Message}");
            }
            return null;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                isSpaceBarPressed = true;
                Console.WriteLine("Space bar pressed - drawing enabled");
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                isSpaceBarPressed = false;
                Console.WriteLine("Space bar released - drawing disabled");
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Keep this for other command keys
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnLoadDrawing_Click(object sender, EventArgs e)
        {
            // Request drawings list from server
            tcpServer.SendMessage("ListDrawings", "");
        }
    }
}
