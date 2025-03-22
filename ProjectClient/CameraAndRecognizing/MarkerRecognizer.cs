using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DrawingPoint = System.Drawing.Point;

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
        private DateTime? markerLostTime = null;
        private DrawingPoint lastValidPosition = new DrawingPoint(0, 0);
        private int consecutiveNoDetectionFrames = 0;
        private double maxDistanceFactorWhenLost = 0.15; // Max distance as fraction of frame diagonal
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
        public void ResetPositionHistory()
        {
            // Clear the recent positions queue
            recentPositions.Clear();

            // Reset the last position used for smoothing
            firstPos = true;
            lastPos = new PointF(0, 0);

            // Clear the last detected center
            shapeRecognizer.lastDetectedCenter = new Point(0, 0);
        }
        public void SetAdaptiveDetection(bool enable)
        {
            if (enable)
            {
                // Configure shape recognizer for tiered adaptive detection
                // Parameters: baseThreshold, mediumThreshold, strictThreshold, colorDelaySeconds, shapeDelaySeconds
                //   - Base threshold: Normal operation
                //   - Medium threshold: After 1 second (color strictness trigger)
                //   - Strict threshold: After 2 seconds (shape strictness trigger)
                shapeRecognizer.ConfigureAdaptiveThreshold(0.5, 0.65, 0.8, 1.0, 2.0);
                shapeRecognizer.EnableAdaptiveColorRange(true);

                // Configure color recognizer for adaptive thresholds
                // Parameters: baseThreshold, strictThreshold
                colorRecognizer.ConfigureAdaptiveThreshold(50, 30);

                Console.WriteLine("Tiered adaptive detection enabled with the following stages:");
                Console.WriteLine("  - Stage 1 (at 1.0s): Stricter with color and location");
                Console.WriteLine("  - Stage 2 (at 2.0s): Stricter with shape detection");
                Console.WriteLine("  - Reset: Gradual return to normal thresholds after marker is found");
            }
            else
            {
                // Reset all adaptive behavior
                shapeRecognizer.ResetAdaptiveThreshold();
                shapeRecognizer.EnableAdaptiveColorRange(false);
                colorRecognizer.ConfigureAdaptiveThreshold(50, 50);
                consecutiveNoDetectionFrames = 0;
                markerLostTime = null;

                Console.WriteLine("Adaptive detection disabled - using fixed thresholds");
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
                // Update color threshold based on marker status
                colorRecognizer.UpdateColorThreshold(markerLostTime.HasValue, markerLostTime);
                colorMarker = colorRecognizer.FindMarker(frame, targetColor, samplingStep);
            }

            if (currentMode == DetectionMode.ShapeOnly || currentMode == DetectionMode.Combined)
            {
                shapeMarker = shapeRecognizer.FindMarker(frame);
            }

            // Decision logic based on detection mode
            Point? markerCenter = null;
            string detectionSource = "";

            // Calculate maximum allowed distance between detections
            double frameDiagonal = Math.Sqrt(frame.Width * frame.Width + frame.Height * frame.Height);
            double maxAllowedDistance = frameDiagonal * maxDistanceFactorWhenLost;

            // If marker has been lost for a while, make distance constraint stricter
            if (markerLostTime.HasValue)
            {
                TimeSpan lostDuration = DateTime.Now - markerLostTime.Value;

                // Update color threshold after 1 second of marker loss
                colorRecognizer.UpdateColorThreshold(true, markerLostTime);

                // Update shape threshold with both delay stages 
                shapeRecognizer.UpdateAdaptiveThreshold(true, markerLostTime, 1.0, 2.0);

                // Add debug info to display
                if (displayBox != null && displayBox.Image != null)
                {
                    using (Graphics g = Graphics.FromImage(displayBox.Image))
                    {
                        string lostMsg = $"Marker lost for {lostDuration.TotalSeconds:F1}s";
                        Color msgColor = Color.Yellow;

                        // Change color based on strictness stage
                        if (lostDuration.TotalSeconds >= 2.0)
                        {
                            msgColor = Color.Red;
                            lostMsg += " - Stage 2 (Strict)";
                        }
                        else if (lostDuration.TotalSeconds >= 1.0)
                        {
                            msgColor = Color.Orange;
                            lostMsg += " - Stage 1 (Medium)";
                        }

                        g.DrawString(lostMsg, new Font("Arial", 12, FontStyle.Bold),
                                    new SolidBrush(msgColor), 10, displayBox.Height - 30);
                    }
                }
            }
            else
            {
                // Reset thresholds when marker is found
                colorRecognizer.UpdateColorThreshold(false, null);
                shapeRecognizer.UpdateAdaptiveThreshold(false, null, 1.0, 2.0);
            }
            if (currentMode == DetectionMode.Combined)
            {
                // If both strategies find a marker, check if they're close to each other
                if (colorMarker.HasValue && shapeMarker.HasValue)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(colorMarker.Value.X - shapeMarker.Value.X, 2) +
                        Math.Pow(colorMarker.Value.Y - shapeMarker.Value.Y, 2));

                    // Use stricter proximity requirement when marker has been lost
                    double proximityThreshold = markerLostTime.HasValue ? 30 : 50;

                    if (distance < proximityThreshold) // Markers are close, we've found the target
                    {
                        // Prefer shape for center position
                        markerCenter = shapeMarker;
                        detectionSource = "Shape+Color";
                    }
                    else
                    {
                        // Markers are far apart - check which one is closer to last position
                        if (lastValidPosition.X != 0 && lastValidPosition.Y != 0)
                        {
                            double distToColor = colorMarker.Value.DistanceTo(lastValidPosition);
                            double distToShape = shapeMarker.Value.DistanceTo(lastValidPosition);

                            // If either is too far from last position when lost, reject it
                            if (markerLostTime.HasValue &&
                                Math.Min(distToColor, distToShape) > maxAllowedDistance)
                            {
                                // Neither is close enough to last position
                                markerCenter = null;
                                detectionSource = "Rejected (too far)";
                            }
                            else if (distToShape < distToColor)
                            {
                                // Shape is closer to last position
                                markerCenter = shapeMarker;
                                detectionSource = "Shape";
                            }
                            else
                            {
                                // Color is closer to last position
                                markerCenter = colorMarker;
                                detectionSource = "Color";
                            }
                        }
                        else
                        {
                            // No last position, prefer shape
                            markerCenter = shapeMarker;
                            detectionSource = "Shape";
                        }
                    }
                }
                else if (shapeMarker.HasValue)
                {
                    // Only shape detection succeeded - check distance from last position
                    if (markerLostTime.HasValue && lastValidPosition.X != 0 && lastValidPosition.Y != 0)
                    {
                        double distance = shapeMarker.Value.DistanceTo(lastValidPosition);
                        if (distance > maxAllowedDistance)
                        {
                            // Too far from last position, reject
                            markerCenter = null;
                            detectionSource = "Shape rejected (too far)";
                        }
                        else
                        {
                            markerCenter = shapeMarker;
                            detectionSource = "Shape";
                        }
                    }
                    else
                    {
                        markerCenter = shapeMarker;
                        detectionSource = "Shape";
                    }
                }
                else if (colorMarker.HasValue)
                {
                    // Only color detection succeeded - check distance from last position
                    if (markerLostTime.HasValue && lastValidPosition.X != 0 && lastValidPosition.Y != 0)
                    {
                        double distance = colorMarker.Value.DistanceTo(lastValidPosition);
                        if (distance > maxAllowedDistance)
                        {
                            // Too far from last position, reject
                            markerCenter = null;
                            detectionSource = "Color rejected (too far)";
                        }
                        else
                        {
                            markerCenter = colorMarker;
                            detectionSource = "Color";
                        }
                    }
                    else
                    {
                        markerCenter = colorMarker;
                        detectionSource = "Color";
                    }
                }
            }
            else if (currentMode == DetectionMode.ShapeOnly)
            {
                markerCenter = shapeMarker;
                detectionSource = "Shape";

                // Additional distance check when marker was previously lost
                if (markerCenter.HasValue && markerLostTime.HasValue &&
                    lastValidPosition.X != 0 && lastValidPosition.Y != 0)
                {
                    double distance = markerCenter.Value.DistanceTo(lastValidPosition);
                    if (distance > maxAllowedDistance)
                    {
                        markerCenter = null;
                        detectionSource = "Shape rejected (too far)";
                    }
                }
            }
            else // ColorOnly
            {
                markerCenter = colorMarker;
                detectionSource = "Color";

                // Additional distance check when marker was previously lost
                if (markerCenter.HasValue && markerLostTime.HasValue &&
                    lastValidPosition.X != 0 && lastValidPosition.Y != 0)
                {
                    double distance = markerCenter.Value.DistanceTo(lastValidPosition);
                    if (distance > maxAllowedDistance)
                    {
                        markerCenter = null;
                        detectionSource = "Color rejected (too far)";
                    }
                }
            }

            if (markerCenter.HasValue)
            {
                // Reset marker lost tracking
                if (markerLostTime.HasValue)
                {
                    TimeSpan lostDuration = DateTime.Now - markerLostTime.Value;
                    if (lostDuration.TotalSeconds > 1.0)
                    {
                        Console.WriteLine($"Marker found after {lostDuration.TotalSeconds:F1}s");
                    }
                    markerLostTime = null;
                }

                consecutiveNoDetectionFrames = 0;

                // Update last valid position for future reference
                lastValidPosition = markerCenter.Value;

                // Apply smoothing if enabled
                Point finalPosition = useSmoothing ? SmoothPosition(markerCenter.Value) : markerCenter.Value;

                // Add to recent positions for drawing trail
                if (recentPositions.Count >= 20)
                    recentPositions.Dequeue();
                recentPositions.Enqueue(finalPosition);

                // Create visualization
                if (displayBox != null && displayBox.Image != null)
                {
                    float scaleX = (float)displayBox.Width / frame.Width;
                    float scaleY = (float)displayBox.Height / frame.Height;

                    using (Graphics g = Graphics.FromImage(displayBox.Image))
                    {
                        // Draw trail if needed
                        DrawPositionTrail(g, scaleX, scaleY);

                        // Draw marker indicator at current position
                        DrawMarkerIndicator(g, (int)(finalPosition.X * scaleX), (int)(finalPosition.Y * scaleY), detectionSource);

                        // Draw status information
                        DrawStatusInformation(g, detectionSource, finalPosition);

                        // Add debug visualizations if enabled
                        if (ShowDebugInfo)
                        {
                            // Draw color detector debug visuals
                            colorRecognizer.DrawDetectionVisuals(g, frame, targetColor, scaleX, scaleY);

                            // Draw shape detector debug visuals
                            shapeRecognizer.DrawDetectionVisuals(g, scaleX, scaleY);
                        }
                    }
                }

                // Trigger marker detected event
                OnMarkerDetected(finalPosition, new Size(frame.Width, frame.Height));
            }
            else
            {
                // Track consecutive frames with no detection
                consecutiveNoDetectionFrames++;

                // Set the marker lost time if not already set
                if (!markerLostTime.HasValue && consecutiveNoDetectionFrames >= 3)
                {
                    markerLostTime = DateTime.Now;
                    Console.WriteLine("Marker lost, entering adaptive detection mode");
                }

                // Clear the trail if no marker is detected for several frames
                if (consecutiveNoDetectionFrames > 10)
                {
                    recentPositions.Clear();
                }

                // Send a notification that no marker was detected (critical for stopping drawing)
                if (consecutiveNoDetectionFrames >= 3)
                {
                    // Send a sentinel value (0,0) to indicate marker is lost
                    OnMarkerDetected(new Point(0, 0), new Size(frame.Width, frame.Height));
                }

                // Draw adaptive status information
                if (displayBox != null && displayBox.Image != null && markerLostTime.HasValue)
                {
                    using (Graphics g = Graphics.FromImage(displayBox.Image))
                    {
                        TimeSpan lostTime = DateTime.Now - markerLostTime.Value;
                        string lostMsg = $"Marker lost for {lostTime.TotalSeconds:F1}s";
                        Color msgColor = lostTime.TotalSeconds > 3.0 ? Color.Red : Color.Yellow;

                        g.DrawString(lostMsg, new Font("Arial", 12, FontStyle.Bold),
                                    new SolidBrush(msgColor), 10, displayBox.Height - 30);

                        // If using stricter detection, show this info
                        if (lostTime.TotalSeconds > 1.0)
                        {
                            string modeMsg = "Using stricter detection criteria";
                            g.DrawString(modeMsg, new Font("Arial", 10, FontStyle.Italic),
                                        Brushes.Orange, 10, displayBox.Height - 50);
                        }
                    }
                }
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
