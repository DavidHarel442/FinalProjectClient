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
    /// Main class that coordinates marker detection using different strategies.
    /// Combines color and shape-based recognition approaches to provide robust
    /// marker tracking in camera frames.
    /// </summary>
    public class MarkerRecognizer
    {
        /// <summary>
        /// Color-based marker detection strategy
        /// </summary>
        public ColorRecognizer colorRecognizer;

        /// <summary>
        /// Shape-based marker detection strategy
        /// </summary>
        public ShapeRecognizer shapeRecognizer;

        /// <summary>
        /// Tracks and smooths marker positions over time
        /// </summary>
        private PositionTracker positionTracker;

        /// <summary>
        /// Available detection modes that determine which recognition strategies to use
        /// </summary>
        private enum DetectionMode { ColorOnly, ShapeOnly, Combined }

        /// <summary>
        /// The currently active detection mode
        /// </summary>
        private DetectionMode currentMode = DetectionMode.Combined;

        /// <summary>
        /// Flag indicating whether the recognizer has been calibrated
        /// </summary>
        private bool isCalibrated = false;

        /// <summary>
        /// The color to look for during detection, calibrated from a sample image
        /// </summary>
        private Color targetColor;



        /// <summary>
        /// Pixel sampling step size - higher values improve performance but reduce precision
        /// </summary>
        public int samplingStep = 2;

        /// <summary>
        /// Event raised when a marker is detected in a frame
        /// </summary>
        public event EventHandler<MarkerDetectedEventArgs> MarkerDetected;

        /// <summary>
        /// Initializes a new instance of the MarkerRecognizer class.
        /// Creates and initializes the detection strategies and position tracker.
        /// </summary>
        public MarkerRecognizer()
        {
            colorRecognizer = new ColorRecognizer();
            shapeRecognizer = new ShapeRecognizer();
            positionTracker = new PositionTracker();
        }

        /// <summary>
        /// Configures detection settings for marker recognition.
        /// </summary>
        /// <param name="samplingStep">Pixel sampling step (higher values are faster but less precise)</param>
        /// <param name="useSmoothing">Whether to apply position smoothing</param>
        /// <param name="smoothStrength">Strength of the smoothing effect (0-1, higher = more smoothing)</param>
        /// <param name="minMoveThreshold">Minimum pixel movement to consider a position change</param>
        public void SetDetectionSettings(int samplingStep = 2, bool useSmoothing = true,
                                        double smoothStrength = 0.7, int minMoveThreshold = 2)
        {
            this.samplingStep = samplingStep;
            colorRecognizer.SetSamplingStep(samplingStep);
            positionTracker.Configure(useSmoothing, smoothStrength, minMoveThreshold);
        }

        /// <summary>
        /// Sets the detection mode to determine which strategies are used.
        /// </summary>
        /// <param name="mode">The detection mode to use ("color", "shape", or any other value for combined mode)</param>
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
        /// Calibrates the color tracking based on a point in the image.
        /// Identifies the target color at the specified location and configures both recognizers.
        /// </summary>
        /// <param name="image">The image to calibrate from</param>
        /// <param name="clickLocation">The location in the image where the marker is located</param>
        /// <exception cref="ArgumentNullException">Thrown when image is null</exception>
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


        /// <summary>
        /// Resets the position history and tracking data.
        /// Used when starting a new detection session or when calibrating.
        /// </summary>
        public void ResetPositionHistory()
        {
            positionTracker.Reset();
            shapeRecognizer.lastDetectedCenter = new Point(0, 0);
        }

        /// <summary>
        /// Processes a frame to detect markers and triggers the MarkerDetected event.
        /// The main method that drives the marker detection workflow.
        /// </summary>
        /// <param name="frame">The frame to process</param>
        /// <param name="displayBox">The PictureBox where the frame is displayed</param>
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
        /// Detects marker position using the appropriate strategy based on current mode.
        /// Applies the detection strategy selected in the current mode.
        /// </summary>
        /// <param name="frame">The frame to process</param>
        /// <returns>The detected marker position, or null if not found</returns>
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
        /// Determines the final marker position based on the available detection results.
        /// Applies decision logic to choose between color and shape detection results
        /// when in combined mode.
        /// </summary>
        /// <param name="colorMarker">The position from color detection, or null if not found</param>
        /// <param name="shapeMarker">The position from shape detection, or null if not found</param>
        /// <returns>The determined marker position, or null if no valid position</returns>
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
        /// Updates position tracking with a new detected position.
        /// Forwards the detection to the position tracker for smoothing and validation.
        /// </summary>
        /// <param name="markerCenter">The detected marker position, or null if not found</param>
        /// <param name="frameSize">The size of the frame</param>
        /// <returns>The tracked and smoothed position, or null if invalid</returns>
        private Point? UpdatePositionTracking(Point? markerCenter, Size frameSize)
        {
            return positionTracker.Update(markerCenter, frameSize);
        }

        /// <summary>
        /// Raises the MarkerDetected event.
        /// </summary>
        /// <param name="position">The detected marker position</param>
        /// <param name="cameraSize">The size of the camera frame</param>
        protected virtual void OnMarkerDetected(Point position, Size cameraSize)
        {
            MarkerDetected?.Invoke(this, new MarkerDetectedEventArgs(position, cameraSize));
        }


        
    }
}