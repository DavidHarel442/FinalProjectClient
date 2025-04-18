using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient.CameraAndRecognizing
{
    /// <summary>
    /// Tracks marker positions over time, providing smoothing, history management, and marker lost detection.
    /// Helps stabilize detected marker positions and manages transitions when a marker is temporarily lost.
    /// </summary>
    public class PositionTracker
    {


        /// <summary>
        /// The last valid position where a marker was detected
        /// </summary>
        private Point lastValidPosition = new Point(0, 0);

        /// <summary>
        /// Counter for frames where no marker was detected
        /// </summary>
        private int consecutiveNoDetectionFrames = 0;



        // Position smoothing
        /// <summary>
        /// Flag indicating whether position smoothing is enabled
        /// </summary>
        private bool useSmoothing = true;



        /// <summary>
        /// Minimum distance threshold in pixels for position changes
        /// </summary>
        private int minDistanceThreshold = 2;

        /// <summary>
        /// Last smoothed position
        /// </summary>
        private PointF lastPos;

        /// <summary>
        /// Flag indicating if this is the first position being tracked
        /// </summary>
        private bool firstPos = true;

        /// <summary>
        /// Alpha value for exponential smoothing (0-1, lower = more smoothing)
        /// </summary>
        private float alpha = 0.2f;

        // Tracking state
        /// <summary>
        /// Gets or sets the source of the detected marker position for diagnostic purposes
        /// </summary>
        public string DetectionSource { get; set; } = "Ready";



        /// <summary>
        /// Gets the last valid position where a marker was detected
        /// </summary>
        public Point LastValidPosition => lastValidPosition;

        /// <summary>
        /// Configures the position tracker settings.
        /// </summary>
        /// <param name="useSmoothing">Whether to enable position smoothing</param>
        /// <param name="smoothStrength">Strength of smoothing (0-1, higher = more smoothing)</param>
        /// <param name="minMoveThreshold">Minimum movement threshold in pixels</param>
        public void Configure(bool useSmoothing, double smoothStrength, int minMoveThreshold)
        {
            this.useSmoothing = useSmoothing;
            this.minDistanceThreshold = Math.Max(0, minMoveThreshold);

            // Reset state
            Reset();
        }

        /// <summary>
        /// Resets the position tracker state.
        /// Clears position history and resets smoothing parameters.
        /// </summary>
        public void Reset()
        {
            firstPos = true;
            lastPos = new PointF(0, 0);
        }



        /// <summary>
        /// Updates position tracking with a new detected position.
        /// Handles smoothing, history management, and marker lost detection.
        /// </summary>
        /// <param name="markerCenter">The detected marker position, or null if not detected</param>
        /// <param name="frameSize">The size of the camera frame</param>
        /// <returns>The tracked position after processing, null if indeterminate, or Point(0,0) if marker is lost</returns>
        public Point? Update(Point? markerCenter, Size frameSize)
        {
            if (markerCenter.HasValue)
            {
                // Reset consecutive frames counter
                consecutiveNoDetectionFrames = 0;

                // Update last valid position
                lastValidPosition = markerCenter.Value;

                // Apply smoothing if enabled
                Point finalPosition = useSmoothing ? SmoothPosition(markerCenter.Value) : markerCenter.Value;

               

                return finalPosition;
            }
            else
            {
                // Track consecutive frames with no detection
                consecutiveNoDetectionFrames++;



                // Return null if marker is lost for several frames
                if (consecutiveNoDetectionFrames >= 3)
                {
                    // Return Point(0,0) to indicate marker lost
                    return new Point(0, 0);
                }

                return null;
            }
        }


        /// <summary>
        /// Applies smoothing to marker positions to reduce jitter.
        /// Uses exponential smoothing while preserving intentional movements.
        /// </summary>
        /// <param name="rawPos">The raw position to smooth</param>
        /// <returns>The smoothed position</returns>
        private Point SmoothPosition(Point rawPos)
        {
            if (firstPos)
            {
                lastPos = new PointF(rawPos.X, rawPos.Y);
                firstPos = false;
                return rawPos;
            }

            // Calculate distance
            float distance = (float)Math.Sqrt(
                Math.Pow(rawPos.X - lastPos.X, 2) +
                Math.Pow(rawPos.Y - lastPos.Y, 2));

            // Only filter very tiny movements to reduce jitter
            // Use a smaller threshold to ensure we don't reject valid small movements
            if (distance < minDistanceThreshold)
            {
                return new Point((int)lastPos.X, (int)lastPos.Y);
            }

            // Simple exponential smoothing - don't overcomplicate
            // Keep alpha between 0.2-0.4 for good balance
            alpha = 0.3f; // Fixed moderate smoothing value

            // Apply smoothing
            lastPos.X = alpha * rawPos.X + (1 - alpha) * lastPos.X;
            lastPos.Y = alpha * rawPos.Y + (1 - alpha) * lastPos.Y;

            return new Point((int)lastPos.X, (int)lastPos.Y);
        }
    }
}