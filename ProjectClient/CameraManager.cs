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

namespace ProjectClient
{
    public class CameraManager
    {
        private PictureBox cameraPictureBox;
        private DrawingManager drawingManager;
        private SharedDrawingForm parentForm;

        // Components
        private CameraHandler cameraHandler;
        public MarkerRecognizer markerRecognizer;
        private ColorRecognizer colorRecognizer;
        private ShapeRecognizer shapeRecognizer;

        // Camera settings
        private int boxWidth;
        private int boxHeight;
        private int cameraWidth;
        private int cameraHeight;

        /// <summary>
        /// Constructor
        /// </summary>
        public CameraManager(PictureBox cameraPictureBox, DrawingManager drawingManager, SharedDrawingForm parentForm)
        {
            this.cameraPictureBox = cameraPictureBox;
            this.drawingManager = drawingManager;
            this.parentForm = parentForm;

            boxHeight = cameraPictureBox.Height;
            boxWidth = cameraPictureBox.Width;

            // Initialize components
            InitializeComponents();
        }

        /// <summary>
        /// Initialize all components and wire up events
        /// </summary>
        private void InitializeComponents()
        {
            // Create camera handler
            cameraHandler = new CameraHandler();
            cameraHandler.FrameCaptured += CameraHandler_FrameCaptured;

            // Create recognizers
            colorRecognizer = new ColorRecognizer();
            shapeRecognizer = new ShapeRecognizer();
            markerRecognizer = new MarkerRecognizer();

            // Set up marker detected event
            markerRecognizer.MarkerDetected += MarkerRecognizer_MarkerDetected;
        }

        /// <summary>
        /// Event handler for when a marker is detected
        /// </summary>
        private void MarkerRecognizer_MarkerDetected(object sender, MarkerRecognizer.MarkerDetectedEventArgs e)
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
        /// Event handler for when a new frame is captured
        /// </summary>
        private void CameraHandler_FrameCaptured(object sender, CameraHandler.FrameCapturedEventArgs e)
        {
            try
            {
                // Store the camera dimensions
                cameraWidth = e.Frame.Width;
                cameraHeight = e.Frame.Height;

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
        /// Call this to start the camera
        /// </summary>
        public bool StartCamera()
        {
            bool result = cameraHandler.StartCamera();
            if (result)
            {
                // Enable adaptive detection when camera starts
                markerRecognizer.SetAdaptiveDetection(true);
                Console.WriteLine("Camera started with adaptive detection enabled");
            }

            return result;
        }

        /// <summary>
        /// Call this to stop the camera
        /// </summary>
        public void StopCamera()
        {
            cameraHandler.StopCamera();
        }

        /// <summary>
        /// Set camera quality settings
        /// </summary>
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
        /// Calibrate color tracking
        /// </summary>
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
        /// Resize an image to the specified dimensions
        /// </summary>
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