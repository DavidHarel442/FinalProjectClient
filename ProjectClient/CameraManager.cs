using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge;
// Adding fully qualified type aliases to prevent ambiguous references
using DrawingPoint = System.Drawing.Point;
using AForgePoint = AForge.Point;
using ProjectClient.CameraAndRecognizing;
using ProjectClient.ShapeRecognizing;

namespace ProjectClient
{
    /// <summary>
    /// Manages camera functionality including capture, display, and marker detection.
    /// Coordinates between hardware camera access and application UI components.
    /// Handles camera initialization, frame processing, and marker recognition.
    /// </summary>
    public class CameraManager
    {
        /// <summary>
        /// The PictureBox control where camera feed is displayed
        /// </summary>
        private PictureBox cameraPictureBox;



        /// <summary>
        /// Reference to the parent form containing this manager
        /// </summary>
        private SharedDrawingForm parentForm;

        // Components
        /// <summary>
        /// Handles camera device initialization and frame capture
        /// </summary>
        private CameraHandler cameraHandler;

        /// <summary>
        /// Handles marker detection and tracking
        /// </summary>
        public MarkerRecognizer markerRecognizer;





        // Camera settings
        /// <summary>
        /// Width of the display box
        /// </summary>
        private int boxWidth;

        /// <summary>
        /// Height of the display box
        /// </summary>
        private int boxHeight;



        /// <summary>
        /// Initializes a new instance of the CameraManager class.
        /// Sets up required references to UI components and initializes camera capture system.
        /// </summary>
        /// <param name="cameraPictureBox">The PictureBox control where camera feed will be displayed</param>
        /// <param name="drawingManager">The DrawingManager for handling drawing operations</param>
        /// <param name="parentForm">The parent form containing this manager</param>
        public CameraManager(PictureBox cameraPictureBox  , SharedDrawingForm parentForm)
        {
            this.cameraPictureBox = cameraPictureBox;
            this.parentForm = parentForm;

            boxHeight = cameraPictureBox.Height;
            boxWidth = cameraPictureBox.Width;

            // Initialize components
            InitializeComponents();
        }

        /// <summary>
        /// Initializes all camera and detection components and wires up event handlers.
        /// Creates instances of CameraHandler, ColorRecognizer, ShapeRecognizer, and MarkerRecognizer.
        /// </summary>
        private void InitializeComponents()
        {
            // Create camera handler
            cameraHandler = new CameraHandler();
            cameraHandler.FrameCaptured += CameraHandler_FrameCaptured;

            // Create recognizers
            markerRecognizer = new MarkerRecognizer();

            // Set up marker detected event
            markerRecognizer.MarkerDetected += MarkerRecognizer_MarkerDetected;
        }

        /// <summary>
        /// Event handler triggered when a marker is detected in the camera frame.
        /// Forwards marker position to the parent form for processing.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event arguments containing marker position and camera size</param>
        private void MarkerRecognizer_MarkerDetected(object sender, MarkerDetectedEventArgs e)
        {
            if (parentForm != null)
            {
                parentForm.BeginInvoke(new Action(() =>
                {
                    parentForm.ProcessMarkerPosition(e.Position, e.CameraSize);
                }));
            }
        }

        /// <summary>
        /// Event handler triggered when a new frame is captured from the camera.
        /// Processes each frame for display and marker detection.
        /// Handles resizing, calibration indicator drawing, and UI updates.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event arguments containing the captured frame</param>
        private void CameraHandler_FrameCaptured(object sender, CameraHandler.FrameCapturedEventArgs e)
        {
            try
            {


                // Create a copy for display
                Bitmap displayFrame = ResizeImage(e.Frame, boxWidth, boxHeight);

                // Only process the frame for marker detection if drawing is enabled
                // This check prevents any recognition processing when not in drawing mode
                if (markerRecognizer != null && parentForm != null && parentForm.isMarkerDrawingEnabled)
                {
                    markerRecognizer.ProcessFrame(e.Frame, cameraPictureBox);
                }

                // Draw calibration indicators if needed
                if (parentForm.calibrationPoint.HasValue &&
                    (parentForm.isCalibrationMode || (DateTime.Now - parentForm.calibrationTime).TotalSeconds < 3))
                {
                    using (Graphics g = Graphics.FromImage(displayFrame))
                    {
                        // Draw a pulsing circle at the calibration point
                        int pulse = (int)(Math.Sin((DateTime.Now - parentForm.calibrationTime).TotalMilliseconds / 200) * 5) + 15;

                        using (Pen outerPen = new Pen(Color.Black, 3))
                        {
                            g.DrawEllipse(outerPen,
                                parentForm.calibrationPoint.Value.X - pulse,
                                parentForm.calibrationPoint.Value.Y - pulse,
                                pulse * 2, pulse * 2);
                        }

                        using (Pen innerPen = new Pen(Color.Yellow, 2))
                        {
                            g.DrawEllipse(innerPen,
                                parentForm.calibrationPoint.Value.X - pulse,
                                parentForm.calibrationPoint.Value.Y - pulse,
                                pulse * 2, pulse * 2);
                        }

                        // Draw crosshairs
                        g.DrawLine(new Pen(Color.Yellow, 2),
                            parentForm.calibrationPoint.Value.X - pulse, parentForm.calibrationPoint.Value.Y,
                            parentForm.calibrationPoint.Value.X + pulse, parentForm.calibrationPoint.Value.Y);

                        g.DrawLine(new Pen(Color.Yellow, 2),
                            parentForm.calibrationPoint.Value.X, parentForm.calibrationPoint.Value.Y - pulse,
                            parentForm.calibrationPoint.Value.X, parentForm.calibrationPoint.Value.Y + pulse);

                        // Fill center
                        g.FillEllipse(Brushes.Red,
                            parentForm.calibrationPoint.Value.X - 5,
                            parentForm.calibrationPoint.Value.Y - 5,
                            10, 10);

                        // Show text
                        string message = parentForm.isCalibrationMode ? "Click here to calibrate" : "Calibration point";
                        using (Font font = new Font("Arial", 10, FontStyle.Bold))
                        {
                            // Draw text with outline for better visibility
                            g.DrawString(message, font, Brushes.Black,
                                parentForm.calibrationPoint.Value.X - 60, parentForm.calibrationPoint.Value.Y - 35);
                            g.DrawString(message, font, Brushes.White,
                                parentForm.calibrationPoint.Value.X - 61, parentForm.calibrationPoint.Value.Y - 36);
                        }
                    }
                }

                // Update the UI with the frame
                Action updateUI = () =>
                {
                    try
                    {
                        // Dispose of previous image before assigning new one
                        var oldImage = cameraPictureBox.Image;
                        cameraPictureBox.Image = displayFrame;

                        if (oldImage != null && oldImage != displayFrame)
                        {
                            oldImage.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UI update error: {ex.Message}");
                    }
                };

                // Invoke on UI thread if needed
                if (cameraPictureBox.InvokeRequired)
                {
                    try
                    {
                        cameraPictureBox.BeginInvoke(updateUI);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Form may be closing, just exit gracefully
                        displayFrame.Dispose();
                    }
                }
                else
                {
                    updateUI();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frame processing error: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts the camera capture process and initializes marker detection.
        /// Enables adaptive detection for improved marker tracking.
        /// </summary>
        /// <returns>True if camera started successfully, false otherwise</returns>
        public bool StartCamera()
        {
            bool result = cameraHandler.StartCamera();
            if (result)
            {
                // Enable adaptive detection when camera starts
                Console.WriteLine("Camera started with adaptive detection enabled");
            }

            return result;
        }

        /// <summary>
        /// Stops the camera capture process and releases associated resources.
        /// </summary>
        public void StopCamera()
        {
            cameraHandler.StopCamera();
        }

        /// <summary>
        /// Configures quality and performance settings for both camera capture and marker detection.
        /// Balances image quality, frame rate, and detection accuracy.
        /// </summary>
        /// <param name="width">Target camera capture width in pixels</param>
        /// <param name="height">Target camera capture height in pixels</param>
        /// <param name="skipFrames">Whether to skip frames for better performance</param>
        /// <param name="samplingStep">Pixel sampling step for marker detection (higher values improve performance but reduce precision)</param>
        /// <param name="smoothing">Whether to enable position smoothing for marker detection</param>
        /// <param name="smoothStrength">Strength of the smoothing effect (0-1, higher = more smoothing)</param>
        /// <param name="minMoveThreshold">Minimum pixel movement threshold to consider a position change</param>
        public void SetQualitySettings(int width, int height, bool skipFrames,
                                      int samplingStep = 2, bool smoothing = true,
                                      double smoothStrength = 0.7, int minMoveThreshold = 2)
        {
            // Update camera settings
            cameraHandler.SetQualitySettings(width, height, skipFrames);

            // Update marker detection settings
            markerRecognizer.SetDetectionSettings(samplingStep, smoothing, smoothStrength, minMoveThreshold);
        }

        /// <summary>
        /// Calibrates the color tracking system based on a point in the camera image.
        /// Identifies the target color at the specified location for marker detection.
        /// </summary>
        /// <param name="clickLocation">The location in the image to calibrate from</param>
        /// <exception cref="Exception">Thrown when no camera image is available for calibration</exception>
        public void CalibrateColorTracking(System.Drawing.Point clickLocation)
        {
            if (cameraPictureBox.Image == null)
                throw new Exception("No camera image to calibrate from!");

            using (Bitmap bmp = new Bitmap(cameraPictureBox.Image))
            {
                markerRecognizer.CalibrateColorTracking(bmp, clickLocation);
            }
        }

        /// <summary>
        /// Resizes an image to the specified dimensions while maintaining quality.
        /// Uses bilinear interpolation for a balance of speed and quality.
        /// </summary>
        /// <param name="original">The original bitmap to resize</param>
        /// <param name="newWidth">The target width in pixels</param>
        /// <param name="newHeight">The target height in pixels</param>
        /// <returns>A new Bitmap with the specified dimensions</returns>
        private Bitmap ResizeImage(Bitmap original, int newWidth, int newHeight)
        {
            Bitmap resized = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resized))
            {
                // Use bilinear for better quality while still being fast
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            return resized;
        }
    }
}