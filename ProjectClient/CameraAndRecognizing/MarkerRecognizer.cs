using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient.CameraAndRecognizing
{
    /// <summary>
    /// Main class that coordinates marker detection using different strategies
    /// </summary>
    public class MarkerRecognizer
    {
        // Marker detection strategies
        private ColorRecognizer colorRecognizer;
        public ShapeRecognizer shapeRecognizer;
        private Queue<Point> recentPositions = new Queue<Point>(20); // Store recent positions for trail effect
        private bool drawingActive = false; // Set this based on the isMarkerDrawingEnabled flag
        private DateTime lastStatusUpdate = DateTime.MinValue;
        private string statusMessage = "Ready";

        // DEBUG: Enhanced visualization options
        public bool ShowDebugInfo = true; // Toggle debug visualization
        public bool ShowColorMask = true; // Show color detection mask
        public bool ShowShapeOutlines = true; // Show shape outlines
        
        // Detection mode
        private enum DetectionMode
        {
            ColorOnly,
            ShapeOnly,
            Combined
        }
        private DetectionMode currentMode = DetectionMode.Combined;

        // Position smoothing
        private bool useSmoothing = true;
        private double smoothingStrength = 0.7;
        private int minDistanceThreshold = 2;
        private PointF lastPos;
        private bool firstPos = true;
        private float alpha = 0.2f; // Smoothing factor

        // Sampling settings
        private int samplingStep = 2;

        // State tracking
        private bool isCalibrated = false;
        private Color targetColor;

        // Events
        public event EventHandler<MarkerDetectedEventArgs> MarkerDetected;

        /// <summary>
        /// Event arguments for marker detection
        /// </summary>
        public class MarkerDetectedEventArgs : EventArgs
        {
            public Point Position { get; }
            public Size CameraSize { get; }

            public MarkerDetectedEventArgs(Point position, Size cameraSize)
            {
                Position = position;
                CameraSize = cameraSize;
            }
        }

        /// <summary>
        /// Constructor initializes the strategies
        /// </summary>
        public MarkerRecognizer()
        {
            colorRecognizer = new ColorRecognizer();
            shapeRecognizer = new ShapeRecognizer();
        }

        /// <summary>
        /// Configure detection settings
        /// </summary>
        public void SetDetectionSettings(int samplingStep = 2, bool useSmoothing = true,
                                        double smoothStrength = 0.7, int minMoveThreshold = 2)
        {
            this.samplingStep = samplingStep;
            this.useSmoothing = useSmoothing;
            this.smoothingStrength = Math.Max(0, Math.Min(1.0, smoothStrength)); // Clamp between 0-1
            this.minDistanceThreshold = Math.Max(0, minMoveThreshold);

            // Update settings in sub-recognizers
            colorRecognizer.SetSamplingStep(samplingStep);
            shapeRecognizer.SetSamplingStep(samplingStep);

            // Reset smoothing state
            firstPos = true;
        }

        /// <summary>
        /// Set the detection mode
        /// </summary>
        public void SetDetectionMode(string mode)
        {
            switch (mode.ToLower())
            {
                case "color":
                    currentMode = DetectionMode.ColorOnly;
                    break;
                case "shape":
                    currentMode = DetectionMode.ShapeOnly;
                    break;
                default:
                    currentMode = DetectionMode.Combined;
                    break;
            }
        }

        /// <summary>
        /// Calibrate color tracking based on a point in the image
        /// </summary>
        public void CalibrateColorTracking(Bitmap image, Point clickLocation)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // Calibrate both strategies
            targetColor = colorRecognizer.Calibrate(image, clickLocation);
            shapeRecognizer.Calibrate(image, clickLocation, targetColor);

            isCalibrated = true;
            firstPos = true;
            Console.WriteLine($"Marker calibrated at {clickLocation.X},{clickLocation.Y} with color R:{targetColor.R} G:{targetColor.G} B:{targetColor.B}");
        }

        public void SetDrawingStatus(bool isDrawing)
        {
            drawingActive = isDrawing;
            statusMessage = isDrawing ? "Drawing Active" : "Drawing Paused";
        }

        /// <summary>
        /// Process a frame to detect markers
        /// </summary>
        public void ProcessFrame(Bitmap frame, PictureBox displayBox)
        {
            if (!isCalibrated || frame == null)
                return;

            // Try to detect the marker using the appropriate strategies
            Point? colorMarker = null;
            Point? shapeMarker = null;

            // Detect based on current mode
            if (currentMode == DetectionMode.ColorOnly || currentMode == DetectionMode.Combined)
            {
                colorMarker = colorRecognizer.FindMarker(frame, targetColor, samplingStep);
            }

            if (currentMode == DetectionMode.ShapeOnly || currentMode == DetectionMode.Combined)
            {
                shapeMarker = shapeRecognizer.FindMarker(frame);
            }

            // Decision logic based on detection mode
            Point? markerCenter = null;
            string detectionSource = "";

            if (currentMode == DetectionMode.Combined)
            {
                // If both strategies find a marker, check if they're close to each other
                if (colorMarker.HasValue && shapeMarker.HasValue)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(colorMarker.Value.X - shapeMarker.Value.X, 2) +
                        Math.Pow(colorMarker.Value.Y - shapeMarker.Value.Y, 2));

                    if (distance < 50) // Markers are close, we've found the target
                    {
                        // Prefer shape for center position
                        markerCenter = shapeMarker;
                        detectionSource = "Shape+Color";
                    }
                    else
                    {
                        // Markers are far apart - prefer shape detection
                        markerCenter = shapeMarker;
                        detectionSource = "Shape";
                    }
                }
                else if (shapeMarker.HasValue)
                {
                    // Only shape detection succeeded
                    markerCenter = shapeMarker;
                    detectionSource = "Shape";
                }
                else if (colorMarker.HasValue)
                {
                    // Only color detection succeeded
                    markerCenter = colorMarker;
                    detectionSource = "Color";
                }
            }
            else if (currentMode == DetectionMode.ShapeOnly)
            {
                markerCenter = shapeMarker;
                detectionSource = "Shape";
            }
            else // ColorOnly
            {
                markerCenter = colorMarker;
                detectionSource = "Color";
            }

            if (markerCenter.HasValue)
            {
                // Update our position history for the trail effect
                recentPositions.Enqueue(markerCenter.Value);
                if (recentPositions.Count > 20) // Limit the trail length
                    recentPositions.Dequeue();

                // Apply position smoothing if enabled
                Point finalPosition = markerCenter.Value;
                if (useSmoothing)
                {
                    finalPosition = SmoothPosition(finalPosition);
                }

                // If display box is provided, draw marker indicators
                if (displayBox != null && displayBox.Image != null)
                {
                    using (Graphics g = Graphics.FromImage(displayBox.Image))
                    {
                        float scaleX = (float)displayBox.Width / frame.Width;
                        float scaleY = (float)displayBox.Height / frame.Height;

                        int scaledX = (int)(finalPosition.X * scaleX);
                        int scaledY = (int)(finalPosition.Y * scaleY);

                        // Always draw shape detection visuals when in debug mode (ShowDebugInfo)
                        // This will show shapes even when they don't meet the detection threshold
                        if (ShowDebugInfo && currentMode != DetectionMode.ColorOnly)
                        {
                            shapeRecognizer.DrawDetectionVisuals(g, scaleX, scaleY);
                        }

                        // Let color recognizer draw its visualization if used
                        if (ShowColorMask && (currentMode != DetectionMode.ShapeOnly))
                        {
                            colorRecognizer.DrawDetectionVisuals(g, frame, targetColor, scaleX, scaleY);
                        }

                        // Draw the trail effect (recent marker positions)
                        DrawPositionTrail(g, scaleX, scaleY);

                        // Draw a more visible target indicator 
                        DrawMarkerIndicator(g, scaledX, scaledY, detectionSource);

                        // Show status information
                        DrawStatusInformation(g, detectionSource, finalPosition);
                    }

                    // Force the picture box to refresh
                    displayBox.Invalidate();
                }

                // Raise the marker detected event
                OnMarkerDetected(finalPosition, new Size(frame.Width, frame.Height));
            }
            else
            {
                // Clear the trail if no marker is detected
                recentPositions.Clear();

                // Still draw status even if no marker is detected
                if (displayBox != null && displayBox.Image != null)
                {
                    using (Graphics g = Graphics.FromImage(displayBox.Image))
                    {
                        float scaleX = (float)displayBox.Width / frame.Width;
                        float scaleY = (float)displayBox.Height / frame.Height;

                        // Draw debug visualizations even when no marker is detected
                        if (ShowDebugInfo && currentMode != DetectionMode.ColorOnly)
                        {
                            shapeRecognizer.DrawDetectionVisuals(g, scaleX, scaleY);
                        }

                        if (ShowColorMask && (currentMode != DetectionMode.ShapeOnly))
                        {
                            colorRecognizer.DrawDetectionVisuals(g, frame, targetColor, scaleX, scaleY);
                        }

                        DrawStatusInformation(g, "No marker detected", null);
                    }
                    displayBox.Invalidate();
                }

                // IMPORTANT ADDITION: Send a "null" position to indicate marker is not detected
                // This will signal the drawing system to stop drawing
                OnMarkerDetected(new Point(0, 0), new Size(frame.Width, frame.Height));
            }
        }

        private void DrawMarkerIndicator(Graphics g, int x, int y, string source)
        {
            int size = 25; // Increased size for better visibility

            // Different styles based on detection source
            Color mainColor;
            if (source.Contains("Color") && source.Contains("Shape"))
                mainColor = Color.Lime; // Both methods detected - high confidence
            else if (source.Contains("Shape"))
                mainColor = Color.Yellow; // Shape only
            else
                mainColor = Color.Orange; // Color only

            // Create a more visible targeting reticle with thicker lines
            using (Pen outlinePen = new Pen(Color.Black, 4))
            {
                // Outer circle
                g.DrawEllipse(outlinePen, x - size, y - size, size * 2, size * 2);
            }

            using (Pen targetPen = new Pen(mainColor, 3)) // Thicker line
            {
                // Inner circle
                g.DrawEllipse(targetPen, x - size, y - size, size * 2, size * 2);

                // Crosshair lines
                g.DrawLine(targetPen, x - size, y, x + size, y);
                g.DrawLine(targetPen, x, y - size, x, y + size);
            }

            // Fill center dot with color indicating drawing status
            using (Brush centerBrush = new SolidBrush(drawingActive ? Color.Red : Color.White))
            {
                g.FillEllipse(centerBrush, x - 8, y - 8, 16, 16); // Larger center dot
            }

            // Add a text label showing coordinates
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (SolidBrush shadowBrush = new SolidBrush(Color.Black))
            {
                string coordText = $"({x},{y})";
                // Draw shadow for better visibility
                g.DrawString(coordText, font, shadowBrush, x + size + 2, y - 10);
                // Draw text
                g.DrawString(coordText, font, textBrush, x + size + 1, y - 11);
            }
        }
        private void DrawPositionTrail(Graphics g, float scaleX, float scaleY)
        {
            if (recentPositions.Count < 2)
                return;

            // Use different colors based on whether drawing is active
            Color trailColor = drawingActive ? Color.FromArgb(150, 0, 255, 0) : Color.FromArgb(100, 255, 255, 0);

            // Convert queue to array for indexed access
            Point[] positions = recentPositions.ToArray();

            // Draw lines connecting recent positions with fading opacity
            for (int i = 1; i < positions.Length; i++)
            {
                int opacity = 50 + (i * 150 / positions.Length); // Fade from start to end
                using (Pen trailPen = new Pen(Color.FromArgb(opacity, trailColor), 2 + (i * 3 / positions.Length)))
                {
                    int x1 = (int)(positions[i - 1].X * scaleX);
                    int y1 = (int)(positions[i - 1].Y * scaleY);
                    int x2 = (int)(positions[i].X * scaleX);
                    int y2 = (int)(positions[i].Y * scaleY);

                    g.DrawLine(trailPen, x1, y1, x2, y2);
                }
            }
        }

        
        private void DrawStatusInformation(Graphics g, string detectionSource, Point? position)
        {
            // Update status at most twice per second to avoid flickering
            if ((DateTime.Now - lastStatusUpdate).TotalMilliseconds > 500)
            {
                statusMessage = detectionSource;
                lastStatusUpdate = DateTime.Now;
            }

            // Create semi-transparent background for text
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {
                g.FillRectangle(bgBrush, 10, 10, 250, 110); // Made taller to accommodate debug toggles
            }

            // Draw status text
            using (Font statusFont = new Font("Arial", 12, FontStyle.Bold))
            {
                // Mode
                g.DrawString($"Mode: {currentMode}", statusFont, Brushes.White, 15, 15);

                // Detection info
                Color infoColor = detectionSource.Contains("No marker") ? Color.Red : Color.Lime;
                using (Brush infoBrush = new SolidBrush(infoColor))
                {
                    g.DrawString($"Detection: {statusMessage}", statusFont, infoBrush, 15, 35);
                }

                // Position info
                if (position.HasValue)
                {
                    g.DrawString($"Position: ({position.Value.X}, {position.Value.Y})", statusFont, Brushes.White, 15, 55);
                }

                // Drawing status
                string drawStatus = drawingActive ? "DRAWING ACTIVE" : "Drawing Paused";
                using (Brush drawBrush = new SolidBrush(drawingActive ? Color.Lime : Color.Yellow))
                {
                    g.DrawString(drawStatus, statusFont, drawBrush, 15, 75);
                }

                // Target color info
                string colorInfo = $"Target Color: R:{targetColor.R} G:{targetColor.G} B:{targetColor.B}";
                g.DrawString(colorInfo, statusFont, new SolidBrush(targetColor), 15, 95);
            }
        }

        /// <summary>
        /// Applies smoothing to marker positions
        /// </summary>
        private Point SmoothPosition(Point rawPos)
        {
            if (firstPos)
            {
                lastPos = new PointF(rawPos.X, rawPos.Y);
                firstPos = false;
                return rawPos;
            }

            // Check for movement threshold to avoid jitter
            float distance = (float)Math.Sqrt(
                Math.Pow(rawPos.X - lastPos.X, 2) +
                Math.Pow(rawPos.Y - lastPos.Y, 2));

            if (distance < minDistanceThreshold)
            {
                return new Point((int)lastPos.X, (int)lastPos.Y);
            }

            // Adjust alpha based on smoothingStrength (higher strength = lower alpha)
            alpha = (float)(1.0 - smoothingStrength);

            // Exponential smoothing
            lastPos.X = alpha * rawPos.X + (1 - alpha) * lastPos.X;
            lastPos.Y = alpha * rawPos.Y + (1 - alpha) * lastPos.Y;

            return new Point((int)lastPos.X, (int)lastPos.Y);
        }

        /// <summary>
        /// Raise the MarkerDetected event
        /// </summary>
        protected virtual void OnMarkerDetected(Point position, Size cameraSize)
        {
            MarkerDetected?.Invoke(this, new MarkerDetectedEventArgs(position, cameraSize));
        }

        /// <summary>
        /// Toggle debug visualization settings
        /// </summary>
        public void ToggleDebugInfo()
        {
            ShowDebugInfo = !ShowDebugInfo;
            ShowColorMask = ShowDebugInfo;
            ShowShapeOutlines = ShowDebugInfo;
        }
    }
}
