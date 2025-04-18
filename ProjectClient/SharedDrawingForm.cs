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
    /// <summary>
    /// Main form responsible for the shared drawing board functionality.
    /// Coordinates camera input, marker detection, drawing tools, and network communication.
    /// </summary>
    public partial class SharedDrawingForm : Form
    {
        /// <summary>
        /// The point in the camera feed where calibration is being performed
        /// </summary>
        public Point? calibrationPoint = null;

        /// <summary>
        /// Timestamp when calibration was last performed
        /// </summary>
        public DateTime calibrationTime = DateTime.MinValue;

        /// <summary>
        /// Gets the marker recognizer from the camera manager
        /// </summary>
        public MarkerRecognizer markerRecognizer => cameraManager?.markerRecognizer;

        /// <summary>
        /// Timestamp when the marker was first lost, or null if the marker is currently detected
        /// </summary>
        private DateTime? markerLostStartTime = null;


        /// <summary>
        /// Object responsible for communication with the server
        /// </summary>
        public TcpServerCommunication tcpServer;

        /// <summary>
        /// Object responsible for managing the camera and marker detection
        /// </summary>
        private CameraManager cameraManager;

        /// <summary>
        /// Object responsible for managing the drawing operations
        /// </summary>
        public DrawingManager drawingManager;

        /// <summary>
        /// Flag indicating whether the application is in calibration mode
        /// </summary>
        public bool isCalibrationMode = false;

        /// <summary>
        /// Flag indicating whether marker drawing is enabled
        /// </summary>
        public bool isMarkerDrawingEnabled = false;

        /// <summary>
        /// The last detected position of the marker
        /// </summary>
        private Point? lastMarkerPosition = null;

        /// <summary>
        /// Flag indicating whether the space bar is currently pressed
        /// </summary>
        private bool isSpaceBarPressed = false;

        /// <summary>
        /// Initializes a new instance of the SharedDrawingForm class.
        /// Sets up camera, drawing manager, and network communication.
        /// </summary>
        /// <param name="tcpServer">The TCP communication object for server interaction</param>
        /// <param name="username1">The username of the current user</param>
        public SharedDrawingForm(TcpServerCommunication tcpServer, string username1)
        {
            try
            {
                InitializeComponent();
                this.tcpServer = tcpServer;
                username.Text = username1;
                MessageHandler.SetCurrentForm(this);
                drawingManager = new DrawingManager(drawingPic);
                cameraManager = new CameraManager(Camera, this);
                drawingManager.DrawingActionPerformed += DrawingManager_DrawingActionPerformed;

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
        /// Event handler for the ShowCamera button.
        /// Starts or stops the camera based on current state.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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
        /// Event handler for when a drawing action is performed.
        /// Sends the serialized drawing action to the server.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The drawing action that was performed</param>
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
        /// </summary>
        /// <param name="base64Image">The Base64 encoded image string</param>
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
        /// Applies a drawing action received from another user via the server.
        /// </summary>
        /// <param name="action">The drawing action to apply</param>
        public void ApplyDrawingAction(DrawingAction action)
        {
            try
            {
                drawingManager.ApplyDrawingAction(action);
                drawingPic.Refresh();  // Refresh the picture box
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying drawing action: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for mouse down on the drawing canvas.
        /// Initiates drawing and sets the start point.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The mouse event data</param>
        private void drawingPic_MouseDown(object sender, MouseEventArgs e)
        {
            drawingManager.isDrawing = true;
            drawingManager.lastDrawingPoint = e.Location;
        }

        /// <summary>
        /// Event handler for mouse up on the drawing canvas.
        /// Ends the current drawing operation.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The mouse event data</param>
        private void drawingPic_MouseUp(object sender, MouseEventArgs e)
        {
            drawingManager.isDrawing = false;
        }

        /// <summary>
        /// Event handler for mouse movement on the drawing canvas.
        /// Draws lines when the mouse is held down and sends drawing actions to the server.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The mouse event data</param>
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
        /// Event handler for the eraser button.
        /// Sets the drawing mode to eraser.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void btn_eraser_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(2);
        }

        /// <summary>
        /// Event handler for the clear button.
        /// Clears the canvas and sends a clear action to the server.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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
        /// Event handler for clicks on the color picker.
        /// Selects a color from the color picker and updates the current pen color.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The mouse event data</param>
        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Color selectedColor = drawingManager.PickColor(color_picker, e.Location);
            pic_color.BackColor = selectedColor;
            drawingManager.SetPenColor(selectedColor);
        }

        /// <summary>
        /// Event handler for the pencil button.
        /// Sets the drawing mode to pencil.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void btn_pencil_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(1);
        }

        /// <summary>
        /// Event handler for the fill button.
        /// Sets the drawing mode to fill.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void btn_fill_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(7);
        }

        /// <summary>
        /// Event handler for the pen size dropdown.
        /// Updates the pen size when a new size is selected.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void PenSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(PenSize.Text, out float size))
            {
                drawingManager.SetPenSize(size);
            }
        }

        /// <summary>
        /// Event handler for the eraser size dropdown.
        /// Updates the eraser size when a new size is selected.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void EraserSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(EraserSize.Text, out float size))
            {
                drawingManager.SetEraserSize(size);
            }
        }

        /// <summary>
        /// Event handler for clicks on the drawing canvas.
        /// Handles fill operations in fill mode.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The mouse event data</param>
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
        /// Event handler for form closing.
        /// Ensures the camera is stopped when the form is closed.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The form closing event data</param>
        private void SharedDrawingForm_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            cameraManager.StopCamera();
        }

        /// <summary>
        /// Event handler for the Start Drawing button.
        /// Toggles marker-based drawing mode on and off.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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

                    // Reset tracking variables
                    lastMarkerPosition = null;

                    // Reset recognizer state if available
                    if (markerRecognizer != null)
                    {
                        markerRecognizer.ResetPositionHistory();
                    }

                    // Ensure camera is visible
                    Camera.Visible = true;

                    Console.WriteLine("Drawing mode started");
                    Console.WriteLine("Hold space bar to draw");
                }
                else
                {
                    StartDrawing.Text = "Start Drawing";
                    isMarkerDrawingEnabled = false;
                    lastMarkerPosition = null;

                    // Reset recognizer state if available
                    if (markerRecognizer != null)
                    {
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

        /// <summary>
        /// Event handler for the Calibrate Color button.
        /// Initiates calibration mode for marker color detection.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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

        /// <summary>
        /// Event handler for clicks on the camera feed.
        /// Handles marker calibration when in calibration mode.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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

        /// <summary>
        /// Processes the detected marker position from the camera feed.
        /// Translates camera coordinates to drawing canvas coordinates and handles drawing.
        /// </summary>
        /// <param name="markerPosition">The detected marker position in camera coordinates</param>
        /// <param name="cameraSize">The size of the camera feed</param>
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

                // Handle marker detection
                if (markerPosition.X == 0 && markerPosition.Y == 0)
                {
                    // Just record that we don't have a marker position
                    lastMarkerPosition = null;
                    return;
                }

                // Only proceed with drawing if space bar is pressed
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

        /// <summary>
        /// Translates coordinates from one coordinate system to another.
        /// Used to convert between camera coordinates and drawing canvas coordinates.
        /// </summary>
        /// <param name="source">The source point</param>
        /// <param name="sourceSize">The size of the source coordinate system</param>
        /// <param name="targetSize">The size of the target coordinate system</param>
        /// <returns>The translated point in the target coordinate system</returns>
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

        /// <summary>
        /// Event handler for the Save Drawing button.
        /// Saves the current drawing to the server with the specified name.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
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
        /// Converts the current drawing to a Base64 encoded string.
        /// Used for saving drawings and sharing drawing state.
        /// </summary>
        /// <returns>A Base64 encoded string representation of the drawing</returns>
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

        /// <summary>
        /// Override for key down events.
        /// Handles space bar press for marker drawing.
        /// </summary>
        /// <param name="e">The key event data</param>
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

        /// <summary>
        /// Override for key up events.
        /// Handles space bar release for marker drawing.
        /// </summary>
        /// <param name="e">The key event data</param>
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

        /// <summary>
        /// Processes command keys for the form.
        /// </summary>
        /// <param name="msg">The window message to process</param>
        /// <param name="keyData">The key data</param>
        /// <returns>True if the key was processed, false otherwise</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Keep this for other command keys
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Event handler for the Load Drawing button.
        /// Requests a list of available drawings from the server.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data</param>
        private void btnLoadDrawing_Click(object sender, EventArgs e)
        {
            // Request drawings list from server
            tcpServer.SendMessage("ListDrawings", "");
        }
    }
}