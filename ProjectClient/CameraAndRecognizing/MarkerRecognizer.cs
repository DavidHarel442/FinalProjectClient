using ProjectClient.ShapeRecognizing;
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
        // Detection strategies
        public ColorRecognizer colorRecognizer;
        public ShapeRecognizer shapeRecognizer;

        // Associated helper classes
        private PositionTracker positionTracker;

        // Detection mode
        private enum DetectionMode { ColorOnly, ShapeOnly, Combined }
        private DetectionMode currentMode = DetectionMode.Combined;

        // State tracking
        private bool isCalibrated = false;
        private Color targetColor;
        private bool drawingActive = false;

        // Sampling settings
        public int samplingStep = 2;

        // Events
        public event EventHandler<MarkerDetectedEventArgs> MarkerDetected;

        /// <summary>
        /// Constructor initializes the strategies and helper objects
        /// </summary>
        public MarkerRecognizer()
        {
            colorRecognizer = new ColorRecognizer();
            shapeRecognizer = new ShapeRecognizer();
            positionTracker = new PositionTracker();
        }

        /// <summary>
        /// Configure detection settings
        /// </summary>
        public void SetDetectionSettings(int samplingStep = 2, bool useSmoothing = true,
                                        double smoothStrength = 0.7, int minMoveThreshold = 2)
        {
            this.samplingStep = samplingStep;
            colorRecognizer.SetSamplingStep(samplingStep);
            positionTracker.Configure(useSmoothing, smoothStrength, minMoveThreshold);
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
            positionTracker.Reset();

            Console.WriteLine($"Marker calibrated at {clickLocation.X},{clickLocation.Y} with color R:{targetColor.R} G:{targetColor.G} B:{targetColor.B}");
        }

        public void SetDrawingStatus(bool isDrawing)
        {
            drawingActive = isDrawing;
        }

        public void ResetPositionHistory()
        {
            positionTracker.Reset();
            shapeRecognizer.lastDetectedCenter = new Point(0, 0);
        }

        public void SetAdaptiveDetection(bool enable)
        {
            if (enable)
            {
                shapeRecognizer.ConfigureAdaptiveThreshold(0.5, 0.5, 1);
                colorRecognizer.ConfigureAdaptiveThreshold(50, 50);
                Console.WriteLine("Fixed detection thresholds enabled");
            }
            else
            {
                colorRecognizer.ConfigureAdaptiveThreshold(50, 50);
                Console.WriteLine("Fixed detection thresholds enabled");
            }
        }

        public void ResetMarkerLostStatus()
        {
            positionTracker.ResetMarkerLostStatus();
        }

        /// <summary>
        /// Process a frame to detect markers
        /// </summary>
        public void ProcessFrame(Bitmap frame, PictureBox displayBox)
        {
            if (!isCalibrated || frame == null)
                return;

            // Detect marker position using appropriate strategy
            Point? markerCenter = DetectMarkerPosition(frame);

            // Update position tracking
            Point? finalPosition = UpdatePositionTracking(markerCenter, frame.Size);



            // Trigger marker detected event if we have a position
            if (finalPosition.HasValue)
            {
                OnMarkerDetected(finalPosition.Value, new Size(frame.Width, frame.Height));
            }
        }

        /// <summary>
        /// Detect marker position using appropriate strategy based on current mode
        /// </summary>
        private Point? DetectMarkerPosition(Bitmap frame)
        {
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
            Point? markerCenter = DetermineMarkerPosition(colorMarker, shapeMarker);
            return markerCenter;
        }

        /// <summary>
        /// Determine final marker position based on available detection results
        /// </summary>
        private Point? DetermineMarkerPosition(Point? colorMarker, Point? shapeMarker)
        {
            if (currentMode == DetectionMode.Combined)
            {
                if (colorMarker.HasValue && shapeMarker.HasValue)
                {
                    double distance = Math.Sqrt(
                        Math.Pow(colorMarker.Value.X - shapeMarker.Value.X, 2) +
                        Math.Pow(colorMarker.Value.Y - shapeMarker.Value.Y, 2));

                    double proximityThreshold = 50;
                    if (distance < proximityThreshold)
                    {
                        positionTracker.DetectionSource = "Shape+Color";
                        return shapeMarker;
                    }
                    else
                    {
                        Point lastPos = positionTracker.LastValidPosition;
                        if (lastPos.X != 0 && lastPos.Y != 0)
                        {
                            double distToColor = colorMarker.Value.DistanceTo(lastPos);
                            double distToShape = shapeMarker.Value.DistanceTo(lastPos);

                            if (distToShape < distToColor)
                            {
                                positionTracker.DetectionSource = "Shape";
                                return shapeMarker;
                            }
                            else
                            {
                                positionTracker.DetectionSource = "Color";
                                return colorMarker;
                            }
                        }
                        else
                        {
                            positionTracker.DetectionSource = "Shape";
                            return shapeMarker;
                        }
                    }
                }
                else if (shapeMarker.HasValue)
                {
                    positionTracker.DetectionSource = "Shape";
                    return shapeMarker;
                }
                else if (colorMarker.HasValue)
                {
                    positionTracker.DetectionSource = "Color";
                    return colorMarker;
                }
            }
            else if (currentMode == DetectionMode.ShapeOnly)
            {
                positionTracker.DetectionSource = "Shape";
                return shapeMarker;
            }
            else // ColorOnly
            {
                positionTracker.DetectionSource = "Color";
                return colorMarker;
            }

            positionTracker.DetectionSource = "No marker";
            return null;
        }

        /// <summary>
        /// Update position tracking with new detected position
        /// </summary>
        private Point? UpdatePositionTracking(Point? markerCenter, Size frameSize)
        {
            return positionTracker.Update(markerCenter, frameSize);
        }

        /// <summary>
        /// Raise the MarkerDetected event
        /// </summary>
        protected virtual void OnMarkerDetected(Point position, Size cameraSize)
        {
            MarkerDetected?.Invoke(this, new MarkerDetectedEventArgs(position, cameraSize));
        }

 

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
    }
}