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
    /// Detects markers based on both color and shape characteristics.
    /// </summary>
    public class ShapeRecognizer
    {
        /// <summary>
        /// Component for generating color masks from HSV images
        /// </summary>
        private readonly ColorMaskGenerator _colorMaskGenerator;

        /// <summary>
        /// Component for analyzing shape properties and matching
        /// </summary>
        private readonly ShapeAnalyzer _shapeAnalyzer;

        // Detection state
        /// <summary>
        /// Flag indicating if the recognizer has been calibrated
        /// </summary>
        private bool _isCalibrated = false;

        /// <summary>
        /// The target color to look for
        /// </summary>
        private Color _targetColor;



        /// <summary>
        /// The match score from the last detection
        /// </summary>
        private double _lastMatchScore = 0;

        // Constants
        /// <summary>
        /// Minimum score threshold for accepting a shape match
        /// </summary>
        private const double ACCEPTANCE_THRESHOLD = 0.5;

        /// <summary>
        /// Minimum area for contours to be considered (in pixels)
        /// </summary>
        private const double MIN_AREA = 500;

        /// <summary>
        /// Maximum area for contours to be considered (in pixels)
        /// </summary>
        private const double MAX_AREA = 2000;

        // Public state for compatibility with MarkerRecognizer
        /// <summary>
        /// Center point of the last detected marker
        /// </summary>
        public DrawingPoint lastDetectedCenter = new DrawingPoint(0, 0);

        /// <summary>
        /// Initializes a new instance of the ShapeRecognizer class.
        /// Creates and initializes the color mask generator and shape analyzer components.
        /// </summary>
        public ShapeRecognizer()
        {
            _colorMaskGenerator = new ColorMaskGenerator();
            _shapeAnalyzer = new ShapeAnalyzer();
        }

        /// <summary>
        /// Calibrates the detector with a marker's color and shape.
        /// Analyzes the image at the specified location to extract reference color and shape properties.
        /// </summary>
        /// <param name="image">The image to calibrate from</param>
        /// <param name="location">The point in the image where the marker is located</param>
        /// <param name="color">The target color to detect</param>
        /// <exception cref="ArgumentNullException">Thrown when image is null</exception>
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
        /// Finds a marker in the frame based on shape and color similarity to the calibrated reference.
        /// </summary>
        /// <param name="frame">The bitmap frame to search in</param>
        /// <returns>The position of the detected marker, or null if not found</returns>
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
                _lastMatchScore = 0;
                return null;
            }
        }


        /// <summary>
        /// Finds the contour at the calibration point in an HSV image.
        /// Analyzes color mask around the specified location to identify the target contour.
        /// </summary>
        /// <param name="hsv">The HSV image to analyze</param>
        /// <param name="location">The point to find a contour near</param>
        /// <returns>The detected contour points, or null if none found</returns>
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
        /// Finds the best matching contour in an HSV image based on color, shape, and position.
        /// Applies area constraints and shape matching to identify the most likely marker.
        /// </summary>
        /// <param name="hsv">The HSV image to analyze</param>
        /// <param name="frameWidth">Width of the frame</param>
        /// <param name="frameHeight">Height of the frame</param>
        /// <returns>The center point of the best matching contour, or null if none found</returns>
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
                    _lastMatchScore = 0;
                }

                return null;
            }
        }

        /// <summary>
        /// Calculates a score for a contour based on shape matching and position proximity.
        /// Higher scores indicate better matches to the reference shape and proximity to previous detections.
        /// </summary>
        /// <param name="contour">The contour to evaluate</param>
        /// <param name="centerX">X-coordinate of the contour center</param>
        /// <param name="centerY">Y-coordinate of the contour center</param>
        /// <returns>A score between 0.0 and approximately 1.3, where higher is better</returns>
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
        /// Applies morphological operations to clean up a binary mask.
        /// Uses opening to remove small noise and closing to fill small holes.
        /// </summary>
        /// <param name="input">The input binary mask</param>
        /// <param name="output">The processed output mask</param>
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
    }
}