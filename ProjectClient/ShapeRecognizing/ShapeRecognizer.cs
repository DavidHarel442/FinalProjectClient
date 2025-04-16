using System;
using System.Drawing;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using DrawingPoint = System.Drawing.Point;
using OpenCvPoint = OpenCvSharp.Point;

namespace ProjectClient.ShapeRecognizing
{
    /// <summary>
    /// Class responsible for shape-based marker detection using OpenCV.
    /// Acts as a facade to coordinate the specialized components.
    /// </summary>
    public class ShapeRecognizer
    {
        #region Fields and Properties

        // Components (private implementation details)
        private readonly ColorMaskGenerator _colorMaskGenerator;
        private readonly ShapeAnalyzer _shapeAnalyzer;

        // Detection state
        private bool _isCalibrated = false;
        private Color _targetColor;
        private OpenCvPoint[] _lastDetectedContour = null;
        private double _lastMatchScore = 0;

        // Constants
        private const double ACCEPTANCE_THRESHOLD = 0.5;
        private const double MIN_AREA = 500;
        private const double MAX_AREA = 2000;

        // Public state for compatibility with MarkerRecognizer
        public DrawingPoint lastDetectedCenter = new DrawingPoint(0, 0);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new ShapeRecognizer
        /// </summary>
        public ShapeRecognizer()
        {
            _colorMaskGenerator = new ColorMaskGenerator();
            _shapeAnalyzer = new ShapeAnalyzer();
        }

        #endregion

        #region Public Methods - Interface for MarkerRecognizer

        /// <summary>
        /// Calibrates the detector with a marker's color and shape
        /// </summary>
        public void Calibrate(Bitmap image, DrawingPoint location, Color color)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            _targetColor = color;

            // Process image to find shape at the calibration point
            using (Mat frame = BitmapConverter.ToMat(image))
            using (Mat hsv = new Mat())
            {
                // Convert to HSV color space for better color filtering
                Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

                // Create mask and find contours
                OpenCvPoint[] targetContour = FindCalibrationContour(hsv, location);

                // If we found a contour, analyze its shape properties
                if (targetContour != null)
                {
                    _shapeAnalyzer.AnalyzeReferenceShape(targetContour);
                    _isCalibrated = true;

                    Console.WriteLine($"Shape recognizer calibrated with color: R={_targetColor.R}, G={_targetColor.G}, B={_targetColor.B}");
                    Console.WriteLine($"Reference shape: Type={_shapeAnalyzer.GetShapeTypeName()}, " +
                                     $"Area={_shapeAnalyzer.ReferenceArea:F1}, Vertices={_shapeAnalyzer.ReferenceVertices}");
                }
                else
                {
                    Console.WriteLine("Failed to find a contour at the calibration point");
                }
            }
        }

        /// <summary>
        /// Find a marker in the frame based on shape and color
        /// </summary>
        public DrawingPoint? FindMarker(Bitmap frame)
        {
            if (!_isCalibrated || frame == null)
                return null;

            try
            {
                // Convert Bitmap to OpenCV Mat and find best matching contour
                using (Mat cvFrame = BitmapConverter.ToMat(frame))
                using (Mat hsv = new Mat())
                {
                    Cv2.CvtColor(cvFrame, hsv, ColorConversionCodes.BGR2HSV);
                    return FindBestMatchingContour(hsv, frame.Width, frame.Height);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in shape recognition: {ex.Message}");
                _lastDetectedContour = null;
                _lastMatchScore = 0;
                return null;
            }
        }

       

        /// <summary>
        /// Set the sampling step for shape detection
        /// </summary>
        public void SetSamplingStep(int step)
        {
            // No-op for compatibility (fixed sampling now)
        }

        /// <summary>
        /// Configure adaptive threshold for shape detection
        /// </summary>
        public void ConfigureAdaptiveThreshold(double baseThreshold, double strictThreshold, double delaySeconds)
        {
            // No-op for compatibility (fixed thresholds now)
        }

        /// <summary>
        /// Enable/disable adaptive color range
        /// </summary>
        public void EnableAdaptiveColorRange(bool enable)
        {
            // No-op for compatibility (fixed color ranges now)
        }

        #endregion

        #region Private Implementation Details

        /// <summary>
        /// Find the contour at the calibration point
        /// </summary>
        private OpenCvPoint[] FindCalibrationContour(Mat hsv, DrawingPoint location)
        {
            using (Mat colorMask = _colorMaskGenerator.CreateColorMask(hsv, _targetColor))
            using (Mat processedMask = new Mat())
            {
                // Clean up the mask with morphological operations
                ApplyMorphologicalOperations(colorMask, processedMask);

                // Find contours in the mask
                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchyIndices;
                Cv2.FindContours(processedMask, out contours, out hierarchyIndices,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // Find contour that contains the point or is closest to it
                OpenCvPoint cvLocation = new OpenCvPoint(location.X, location.Y);

                // First try direct containment
                foreach (var contour in contours)
                {
                    if (Cv2.PointPolygonTest(contour, cvLocation, false) >= 0)
                    {
                        return contour;
                    }
                }

                // If no direct hit, find closest contour
                if (contours.Length > 0)
                {
                    double minDist = double.MaxValue;
                    OpenCvPoint[] closestContour = null;

                    foreach (var contour in contours)
                    {
                        Moments moments = Cv2.Moments(contour);
                        if (moments.M00 < 0.001) continue; // Avoid division by zero

                        double centerX = moments.M10 / moments.M00;
                        double centerY = moments.M01 / moments.M00;
                        double dist = Math.Sqrt(Math.Pow(centerX - location.X, 2) +
                                               Math.Pow(centerY - location.Y, 2));

                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestContour = contour;
                        }
                    }

                    // Only accept if the contour is reasonably close
                    if (minDist <= 50)
                    {
                        return closestContour;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Find the best matching contour in the frame
        /// </summary>
        private DrawingPoint? FindBestMatchingContour(Mat hsv, int frameWidth, int frameHeight)
        {
            using (Mat colorMask = _colorMaskGenerator.CreateColorMask(hsv, _targetColor))
            using (Mat processedMask = new Mat())
            {
                // Apply morphological operations
                ApplyMorphologicalOperations(colorMask, processedMask);

                // Find contours
                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchyIndices;
                Cv2.FindContours(processedMask, out contours, out hierarchyIndices,
                    RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // No contours found
                if (contours.Length == 0)
                {
                    _lastDetectedContour = null;
                    _lastMatchScore = 0;
                    return null;
                }

                // Find best matching contour with area constraints
                OpenCvPoint[] bestContour = null;
                double bestScore = 0;
                DrawingPoint bestCenter = new DrawingPoint(0, 0);

                // Set area limits based on frame size
                double frameArea = frameWidth * frameHeight;
                double dynamicMinArea = Math.Max(MIN_AREA, frameArea * 0.002);
                double dynamicMaxArea = Math.Min(MAX_AREA, frameArea * 0.03);

                foreach (var contour in contours)
                {
                    // Calculate area and check constraints
                    double area = Cv2.ContourArea(contour);
                    if (area < dynamicMinArea || area > dynamicMaxArea)
                        continue;

                    // Calculate center of contour
                    Moments moments = Cv2.Moments(contour);
                    if (moments.M00 < 0.001) continue; // Avoid division by zero

                    int centerX = (int)(moments.M10 / moments.M00);
                    int centerY = (int)(moments.M01 / moments.M00);

                    // Apply shape matching criteria
                    double score = CalculateContourScore(contour, centerX, centerY);

                    // Skip low-quality matches
                    if (score < 0.4) continue;

                    // Track best match
                    if (score > bestScore)
                    {
                        bestScore = score;

                        // Approximate contour for visualization
                        if (score > ACCEPTANCE_THRESHOLD * 0.8)
                        {
                            double perimeter = Cv2.ArcLength(contour, true);
                            bestContour = Cv2.ApproxPolyDP(contour, 0.02 * perimeter, true);
                        }
                        else
                        {
                            bestContour = contour;
                        }

                        bestCenter = new DrawingPoint(centerX, centerY);
                    }
                }

                // Update state and return result
                if (bestContour != null)
                {
                    _lastDetectedContour = bestContour;
                    lastDetectedCenter = bestCenter;
                    _lastMatchScore = bestScore;

                    // Return position only if score exceeds threshold
                    if (bestScore > ACCEPTANCE_THRESHOLD)
                    {
                        return bestCenter;
                    }
                }
                else
                {
                    _lastDetectedContour = null;
                    _lastMatchScore = 0;
                }

                return null;
            }
        }

        /// <summary>
        /// Calculate score for contour based on shape matching and position
        /// </summary>
        private double CalculateContourScore(OpenCvPoint[] contour, int centerX, int centerY)
        {
            // Calculate shape match score
            double score = _shapeAnalyzer.CalculateShapeMatchScore(contour);

            // Apply position proximity factor if we have a previous detection
            if (lastDetectedCenter.X != 0 && lastDetectedCenter.Y != 0)
            {
                // Calculate distance to last detected position
                double dx = centerX - lastDetectedCenter.X;
                double dy = centerY - lastDetectedCenter.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                // Maximum tracking distance as percentage of width
                double maxTrackingDistance = 200;

                // Weight on proximity for stability
                double proximityFactor = Math.Max(0.4, 1.0 - (distance / maxTrackingDistance));

                // Apply proximity boost to score
                score *= proximityFactor * 1.3;
            }

            return score;
        }

        /// <summary>
        /// Apply morphological operations to clean up a mask
        /// </summary>
        private static void ApplyMorphologicalOperations(Mat input, Mat output)
        {
            int kernelSize = 5;
            using (Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse,
                   new OpenCvSharp.Size(kernelSize, kernelSize)))
            {
                // Open operation removes small noise
                Cv2.MorphologyEx(input, output, MorphTypes.Open, kernel, iterations: 1);

                // Close operation fills small holes
                Cv2.MorphologyEx(output, output, MorphTypes.Close, kernel, iterations: 1);
            }
        }

        #endregion
    }
}