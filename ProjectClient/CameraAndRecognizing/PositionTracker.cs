using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient.CameraAndRecognizing
{
    public class PositionTracker
    {
        // Recent position tracking
        private Queue<Point> recentPositions = new Queue<Point>(20);
        private Point lastValidPosition = new Point(0, 0);
        private int consecutiveNoDetectionFrames = 0;
        private DateTime? markerLostTime = null;

        // Position smoothing
        private bool useSmoothing = true;
        private double smoothingStrength = 0.7;
        private int minDistanceThreshold = 2;
        private PointF lastPos;
        private bool firstPos = true;
        private float alpha = 0.2f;

        // Tracking state
        public string DetectionSource { get; set; } = "Ready";
        private double maxDistanceFactorWhenLost = 0.15;

        /// <summary>
        /// Gets the last valid position
        /// </summary>
        public Point LastValidPosition => lastValidPosition;

        /// <summary>
        /// Configure the position tracker settings
        /// </summary>
        public void Configure(bool useSmoothing, double smoothStrength, int minMoveThreshold)
        {
            this.useSmoothing = useSmoothing;
            this.smoothingStrength = Math.Max(0, Math.Min(1.0, smoothStrength));
            this.minDistanceThreshold = Math.Max(0, minMoveThreshold);

            // Reset state
            Reset();
        }

        /// <summary>
        /// Reset the position tracker state
        /// </summary>
        public void Reset()
        {
            recentPositions.Clear();
            firstPos = true;
            lastPos = new PointF(0, 0);
        }

        /// <summary>
        /// Reset the marker lost tracking state
        /// </summary>
        public void ResetMarkerLostStatus()
        {
            markerLostTime = null;
            consecutiveNoDetectionFrames = 0;
            maxDistanceFactorWhenLost = 0.15;
            Console.WriteLine("Marker lost status reset - default detection sensitivity restored");
        }

        /// <summary>
        /// Update position tracking with a new detected position
        /// </summary>
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

                // Add to recent positions
                if (recentPositions.Count >= 20)
                    recentPositions.Dequeue();
                recentPositions.Enqueue(finalPosition);

                return finalPosition;
            }
            else
            {
                // Track consecutive frames with no detection
                consecutiveNoDetectionFrames++;

                // Clear the trail if no marker is detected for several frames
                if (consecutiveNoDetectionFrames > 10)
                {
                    recentPositions.Clear();
                }

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
        /// Get recent positions for visualization
        /// </summary>
        public Point[] GetRecentPositions()
        {
            return recentPositions.ToArray();
        }

        /// <summary>
        /// Apply smoothing to marker positions
        /// </summary>
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



