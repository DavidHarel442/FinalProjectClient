using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Aruco;
using DrawingPoint = System.Drawing.Point;
using OpenCvPoint = OpenCvSharp.Point;

namespace ProjectClient.CameraAndRecognizing
{
    /// <summary>
    /// Class responsible for shape-based marker detection using OpenCV
    /// </summary>
    public class ShapeRecognizer
    {
        // Detection settings
        private bool isCalibrated = false;
        private Color targetColor;
        private int samplingStep = 2;

        // Shape detection parameters
        private double minArea = 100;  // Minimum contour area to consider
        private double maxArea = 5000; // Maximum contour area to consider
        private double minCircularity = 0.5; // Minimum circularity for blob detection

        // Reference shape properties
        private int referenceType = -1; // 0=circle, 1=triangle, 2=rectangle, 3=other
        private double referenceArea = 0;
        private double referenceCompactness = 0;
        private int referenceVertices = 0;

        // For visualization
        private OpenCvPoint[] lastDetectedContour = null;
        private DrawingPoint lastDetectedCenter = new DrawingPoint(0, 0);
        private double lastMatchScore = 0;

        /// <summary>
        /// Calibrates the detector with a marker's color and shape
        /// </summary>
        public void Calibrate(Bitmap image, DrawingPoint location, Color color)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            targetColor = color;

            // Convert Bitmap to OpenCV Mat
            using (Mat frame = BitmapConverter.ToMat(image))
            using (Mat hsv = new Mat())
            using (Mat mask = new Mat())
            using (Mat hierarchy = new Mat())
            {
                // Convert to HSV color space for better color filtering
                Cv2.CvtColor(frame, hsv, ColorConversionCodes.BGR2HSV);

                // Create color mask using HSV range
                Mat colorMask = CreateColorMask(hsv, targetColor);

                // Apply morphological operations to clean up mask
                int kernelSize = 5;
                using (Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(kernelSize, kernelSize)))
                {
                    Cv2.MorphologyEx(colorMask, mask, MorphTypes.Open, kernel, iterations: 1);
                    Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel, iterations: 2);
                }

                // Find contours in the mask
                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchyIndices;
                Cv2.FindContours(mask, out contours, out hierarchyIndices, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // Find the contour that contains the calibration point
                OpenCvPoint cvLocation = new OpenCvPoint(location.X, location.Y);
                OpenCvPoint[] targetContour = null;

                foreach (var contour in contours)
                {
                    if (Cv2.PointPolygonTest(contour, cvLocation, false) >= 0)
                    {
                        targetContour = contour;
                        break;
                    }
                }

                // If no contour found at click point, try to find the closest one
                if (targetContour == null && contours.Length > 0)
                {
                    double minDist = double.MaxValue;
                    foreach (var contour in contours)
                    {
                        // Calculate center of contour
                        Moments moments = Cv2.Moments(contour);
                        double centerX = moments.M10 / moments.M00;
                        double centerY = moments.M01 / moments.M00;

                        // Calculate distance to calibration point
                        double dist = Math.Sqrt(Math.Pow(centerX - location.X, 2) + Math.Pow(centerY - location.Y, 2));

                        if (dist < minDist)
                        {
                            minDist = dist;
                            targetContour = contour;
                        }
                    }

                    // Only accept if the contour is reasonably close to the click point
                    if (minDist > 50)
                    {
                        targetContour = null;
                    }
                }

                // If we found a contour, analyze its shape properties
                if (targetContour != null)
                {
                    AnalyzeReferenceShape(targetContour);
                    isCalibrated = true;

                    Console.WriteLine($"Shape recognizer calibrated with color: R={targetColor.R}, G={targetColor.G}, B={targetColor.B}");
                    Console.WriteLine($"Reference shape: Type={GetShapeTypeName(referenceType)}, Area={referenceArea:F1}, Vertices={referenceVertices}");
                }
                else
                {
                    Console.WriteLine("Failed to find a contour at the calibration point");
                }
            }
        }

        /// <summary>
        /// Analyzes a reference shape to extract its characteristics
        /// </summary>
        private void AnalyzeReferenceShape(OpenCvPoint[] contour)
        {
            // Calculate area of the contour
            referenceArea = Cv2.ContourArea(contour);

            // Calculate perimeter of the contour
            double perimeter = Cv2.ArcLength(contour, true);

            // Calculate compactness (circularity)
            referenceCompactness = (4 * Math.PI * referenceArea) / (perimeter * perimeter);

            // Approximate the contour to find its shape
            OpenCvPoint[] approx = Cv2.ApproxPolyDP(contour, 0.03 * perimeter, true);
            referenceVertices = approx.Length;

            // Determine the shape type based on the number of vertices and compactness
            if (referenceCompactness > 0.85 && referenceVertices >= 8)
            {
                referenceType = 0; // Circle
            }
            else if (referenceVertices == 3)
            {
                referenceType = 1; // Triangle
            }
            else if (referenceVertices == 4)
            {
                referenceType = 2; // Rectangle/Square
            }
            else
            {
                referenceType = 3; // Other polygon
            }
        }

        /// <summary>
        /// Creates a binary mask for the target color in HSV space
        /// </summary>
        private Mat CreateColorMask(Mat hsvImage, Color targetRgbColor)
        {
            // Convert RGB to HSV
            int h, s, v;
            RgbToHsv(targetRgbColor.R, targetRgbColor.G, targetRgbColor.B, out h, out s, out v);

            // Adjust HSV ranges based on the color's properties
            // For brighter colors (highlighters), we need wider hue ranges
            int hRange = 20;  // Increased hue range for better detection
            int sRange = 85;  // Increased saturation range
            int vRange = 85;  // Increased value range for better detection in varying lighting

            // For colors with low saturation (pastels, whites), focus more on value
            if (s < 50)
            {
                sRange = 100;
                vRange = 60;
            }

            // For very dark colors, focus more on hue and saturation
            if (v < 50)
            {
                hRange = 25;
                sRange = 100;
                vRange = 100;
            }

            // Debug output occasionally
            if (DateTime.Now.Second % 10 == 0 && DateTime.Now.Millisecond < 50)
            {
                Console.WriteLine($"Color mask HSV: H={h}, S={s}, V={v}, Ranges: H±{hRange}, S±{sRange}, V±{vRange}");
            }

            // Create range for color based on color type
            Scalar lowerBound, upperBound;

            // Handle the special case of red which wraps around the hue value
            if (h < 15 || h > 165)
            {
                // For red hues that wrap around (near 180/0)
                Mat maskLow = new Mat();
                Mat maskHigh = new Mat();

                if (h > 165)
                {
                    lowerBound = new Scalar(h - hRange, Math.Max(s - sRange, 30), Math.Max(v - vRange, 30));
                    upperBound = new Scalar(180, 255, 255);
                    Cv2.InRange(hsvImage, lowerBound, upperBound, maskHigh);

                    lowerBound = new Scalar(0, Math.Max(s - sRange, 30), Math.Max(v - vRange, 30));
                    upperBound = new Scalar(hRange, 255, 255);
                    Cv2.InRange(hsvImage, lowerBound, upperBound, maskLow);
                }
                else // h < 15
                {
                    lowerBound = new Scalar(0, Math.Max(s - sRange, 30), Math.Max(v - vRange, 30));
                    upperBound = new Scalar(h + hRange, 255, 255);
                    Cv2.InRange(hsvImage, lowerBound, upperBound, maskHigh);

                    lowerBound = new Scalar(180 - hRange, Math.Max(s - sRange, 30), Math.Max(v - vRange, 30));
                    upperBound = new Scalar(180, 255, 255);
                    Cv2.InRange(hsvImage, lowerBound, upperBound, maskLow);
                }

                // Combine the masks
                Mat mask = new Mat();
                Cv2.BitwiseOr(maskLow, maskHigh, mask);

                // Clean up temporary mats
                maskLow.Dispose();
                maskHigh.Dispose();

                return mask;
            }
            else
            {
                // For all other colors
                lowerBound = new Scalar(Math.Max(h - hRange, 0), Math.Max(s - sRange, 30), Math.Max(v - vRange, 30));
                upperBound = new Scalar(Math.Min(h + hRange, 180), 255, 255);

                Mat mask = new Mat();
                Cv2.InRange(hsvImage, lowerBound, upperBound, mask);
                return mask;
            }
        }

        /// <summary>
        /// Convert RGB to HSV color space
        /// </summary>
        private void RgbToHsv(int r, int g, int b, out int h, out int s, out int v)
        {
            // Convert RGB ranges from 0-255 to 0-1
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;

            // Calculate Value (Brightness)
            v = (int)(max * 255);

            // Calculate Saturation
            if (max == 0)
            {
                s = 0;
            }
            else
            {
                s = (int)((delta / max) * 255);
            }

            // Calculate Hue
            if (delta == 0)
            {
                h = 0;  // Achromatic (gray)
            }
            else
            {
                double hue;
                if (max == red)
                {
                    hue = (green - blue) / delta + (green < blue ? 6 : 0);
                }
                else if (max == green)
                {
                    hue = (blue - red) / delta + 2;
                }
                else
                {
                    hue = (red - green) / delta + 4;
                }

                hue *= 30;  // Convert to degrees (0-360) and divide by 2 for OpenCV (0-180)
                h = (int)(hue);
            }
        }

        /// <summary>
        /// Set the sampling step for shape detection
        /// </summary>
        public void SetSamplingStep(int step)
        {
            this.samplingStep = Math.Max(1, step);
        }

        /// <summary>
        /// Find a marker in the frame based on shape and color
        /// </summary>
        public DrawingPoint? FindMarker(Bitmap frame)
        {
            if (!isCalibrated || frame == null)
                return null;

            try
            {
                // Convert Bitmap to OpenCV Mat
                using (Mat cvFrame = BitmapConverter.ToMat(frame))
                using (Mat hsv = new Mat())
                {
                    // Convert to HSV color space
                    Cv2.CvtColor(cvFrame, hsv, ColorConversionCodes.BGR2HSV);

                    // Create color mask
                    using (Mat colorMask = CreateColorMask(hsv, targetColor))
                    using (Mat processedMask = new Mat())
                    {
                        // Apply morphological operations
                        int kernelSize = 5;
                        using (Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(kernelSize, kernelSize)))
                        {
                            // Enhanced morphological processing for better shape detection
                            Cv2.MorphologyEx(colorMask, processedMask, MorphTypes.Open, kernel, iterations: 1);
                            Cv2.MorphologyEx(processedMask, processedMask, MorphTypes.Close, kernel, iterations: 2);

                            // Additional dilate operation to connect nearby components
                            Cv2.Dilate(processedMask, processedMask, kernel, iterations: 1);
                        }

                        // Find contours
                        OpenCvSharp.Point[][] contours;
                        HierarchyIndex[] hierarchyIndices;
                        Cv2.FindContours(processedMask, out contours, out hierarchyIndices, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                        // Debug output occasionally
                        if (DateTime.Now.Second % 5 == 0 && DateTime.Now.Millisecond < 50)
                        {
                            Console.WriteLine($"Found {contours.Length} contours in frame");
                        }

                        // No contours found
                        if (contours.Length == 0)
                        {
                            lastDetectedContour = null;
                            lastMatchScore = 0;
                            return null;
                        }

                        // Find best matching contour
                        OpenCvPoint[] bestContour = null;
                        double bestScore = 0;
                        DrawingPoint bestCenter = new DrawingPoint(0, 0);

                        foreach (var contour in contours)
                        {
                            // Calculate area
                            double area = Cv2.ContourArea(contour);

                            // Dynamic area thresholds based on frame size
                            double frameArea = cvFrame.Width * cvFrame.Height;
                            double dynamicMinArea = Math.Max(minArea, frameArea * 0.001); // At least 0.1% of frame
                            double dynamicMaxArea = Math.Min(maxArea, frameArea * 0.2);   // At most 20% of frame

                            // Skip contours outside the area range
                            if (area < dynamicMinArea || area > dynamicMaxArea)
                                continue;

                            // Calculate center of contour
                            Moments moments = Cv2.Moments(contour);
                            if (moments.M00 < 0.001)  // Avoid division by zero
                                continue;

                            int centerX = (int)(moments.M10 / moments.M00);
                            int centerY = (int)(moments.M01 / moments.M00);

                            // Calculate match score based on shape similarity
                            double score = CalculateShapeMatchScore(contour);

                            // Apply perimeter approximation to smooth contour for better visualization
                            double perimeter = Cv2.ArcLength(contour, true);
                            OpenCvPoint[] approxContour = Cv2.ApproxPolyDP(contour, 0.02 * perimeter, true);

                            // If this contour scores better than our current best
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestContour = approxContour; // Use the approximated contour for smoother display
                                bestCenter = new DrawingPoint(centerX, centerY);
                            }
                        }

                        // Lower threshold for acceptance to show more potential matches
                        double acceptanceThreshold = 0.5; // Reduced from 0.6 to show more potential matches

                        // Return if we found a good match
                        if (bestContour != null && bestScore > acceptanceThreshold)
                        {
                            lastDetectedContour = bestContour;
                            lastDetectedCenter = bestCenter;
                            lastMatchScore = bestScore;
                            return bestCenter;
                        }
                        else if (bestContour != null)
                        {
                            // Even if score is low, store it for debug visualization
                            // but don't return it as a valid detection
                            lastDetectedContour = bestContour;
                            lastDetectedCenter = bestCenter;
                            lastMatchScore = bestScore;
                        }

                        // No suitable match found
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in shape recognition: {ex.Message}");
                lastDetectedContour = null;
                lastMatchScore = 0;
                return null;
            }
        }

        /// <summary>
        /// Calculate how well a contour matches our reference shape
        /// </summary>
        private double CalculateShapeMatchScore(OpenCvPoint[] contour)
        {
            // Calculate basic shape properties
            double area = Cv2.ContourArea(contour);
            double perimeter = Cv2.ArcLength(contour, true);
            double compactness = (4 * Math.PI * area) / (perimeter * perimeter);

            // Approximate contour to get vertices
            OpenCvPoint[] approx = Cv2.ApproxPolyDP(contour, 0.03 * perimeter, true);
            int vertices = approx.Length;

            // Calculate shape scores
            double areaScore = 1.0 - Math.Min(1.0, Math.Abs(area - referenceArea) / referenceArea);
            double compactnessScore = 1.0 - Math.Min(1.0, Math.Abs(compactness - referenceCompactness) / Math.Max(0.1, referenceCompactness));

            // Determine shape type
            int shapeType;
            if (compactness > 0.85 && vertices >= 8)
            {
                shapeType = 0; // Circle
            }
            else if (vertices == 3)
            {
                shapeType = 1; // Triangle
            }
            else if (vertices == 4)
            {
                shapeType = 2; // Rectangle/Square
            }
            else
            {
                shapeType = 3; // Other polygon
            }

            // Type matching score
            double typeScore = (shapeType == referenceType) ? 1.0 : 0.3;

            // For non-circles, also consider the number of vertices
            double verticesScore = 1.0;
            if (referenceType != 0 && vertices != referenceVertices)
            {
                verticesScore = 0.5;
            }

            // Combine scores with weights
            double score = typeScore * 0.4 + areaScore * 0.2 + compactnessScore * 0.2 + verticesScore * 0.2;

            return score;
        }

        /// <summary>
        /// Draw detection visuals on the graphics context
        /// </summary>
        public void DrawDetectionVisuals(Graphics g, float scaleX, float scaleY)
        {
            // Create a semi-transparent overlay for the debug info
            using (Brush overlayBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
            {
                g.FillRectangle(overlayBrush, 10, 120, 350, 120); // Background for debug info
            }

            // Display reference shape properties
            string refShapeInfo = $"Reference: {GetShapeTypeName(referenceType)} | Vertices: {referenceVertices} | Area: {referenceArea:F1}";
            g.DrawString(refShapeInfo, new Font("Arial", 9, FontStyle.Bold), Brushes.Yellow, 15, 125);

            // Display target color
            if (targetColor != null && isCalibrated)
            {
                using (SolidBrush colorBrush = new SolidBrush(targetColor))
                {
                    g.FillRectangle(colorBrush, 15, 145, 20, 20);
                    g.DrawRectangle(Pens.White, 15, 145, 20, 20);
                }

                string colorInfo = $"Target: R:{targetColor.R}, G:{targetColor.G}, B:{targetColor.B}";
                g.DrawString(colorInfo, new Font("Arial", 9), Brushes.White, 40, 145);
            }

            // If we have a detected contour, show it and its properties
            if (lastDetectedContour != null && lastDetectedContour.Length > 0)
            {
                // Draw the detected contour
                System.Drawing.Point[] displayPoints = lastDetectedContour
                    .Select(p => new System.Drawing.Point((int)(p.X * scaleX), (int)(p.Y * scaleY)))
                    .ToArray();

                // Use color based on detection score
                Color contourColor = GetScoreColor(lastMatchScore);

                // Draw filled contour with semi-transparency
                using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(80, contourColor)))
                {
                    g.FillPolygon(fillBrush, displayPoints);
                }

                // Draw contour outline
                using (Pen contourPen = new Pen(contourColor, 2))
                {
                    g.DrawPolygon(contourPen, displayPoints);

                    // Draw vertices as small circles
                    foreach (var point in displayPoints)
                    {
                        g.DrawEllipse(contourPen, point.X - 3, point.Y - 3, 6, 6);
                    }
                }

                // Draw center point
                int centerX = (int)(lastDetectedCenter.X * scaleX);
                int centerY = (int)(lastDetectedCenter.Y * scaleY);
                g.FillEllipse(new SolidBrush(Color.Yellow), centerX - 4, centerY - 4, 8, 8);
                g.DrawEllipse(new Pen(Color.Black, 1), centerX - 4, centerY - 4, 8, 8);

                // Display confidence score and shape type near the shape
                string scoreText = $"{lastMatchScore:P0} {GetShapeTypeName(referenceType)}";
                using (Font scoreFont = new Font("Arial", 10, FontStyle.Bold))
                {
                    // Draw shadow for better visibility
                    g.DrawString(scoreText, scoreFont, Brushes.Black,
                        centerX + 5 + 1, centerY - 5 + 1);
                    // Draw actual text
                    g.DrawString(scoreText, scoreFont, new SolidBrush(contourColor),
                        centerX + 5, centerY - 5);
                }

                // Add debug info panel
                string shapeDetails = $"Detected: {GetShapeTypeName(lastDetectedContour.Length)} | Vertices: {lastDetectedContour.Length}";
                g.DrawString(shapeDetails, new Font("Arial", 9), Brushes.White, 15, 170);

                string matchInfo = $"Match Score: {lastMatchScore:P0} | Position: ({centerX}, {centerY})";
                g.DrawString(matchInfo, new Font("Arial", 9), Brushes.White, 15, 190);

                // Add help text
                string helpText = "Debug Overlay: Shape Recognition Visualization";
                g.DrawString(helpText, new Font("Arial", 9, FontStyle.Italic), Brushes.LightGray, 15, 210);
            }
            else
            {
                // If no contour is detected, show status message
                string noDetectionMsg = "No shape detected. Try adjusting lighting or marker position.";
                g.DrawString(noDetectionMsg, new Font("Arial", 9), Brushes.Red, 15, 170);

                string helpText = "Debug Overlay: Waiting for shape detection...";
                g.DrawString(helpText, new Font("Arial", 9, FontStyle.Italic), Brushes.LightGray, 15, 190);
            }
        }

        /// <summary>
        /// Get a string name for a shape type
        /// </summary>
        private string GetShapeTypeName(int shapeType)
        {
            switch (shapeType)
            {
                case 0: return "Circle";
                case 1: return "Triangle";
                case 2: return "Rectangle";
                case 3: return "Polygon";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Get a color based on the confidence score
        /// </summary>
        private Color GetScoreColor(double score)
        {
            // Red (low confidence) to Yellow to Green (high confidence)
            if (score < 0.7)
            {
                // Red to Yellow gradient
                int green = (int)(255 * (score / 0.7));
                return Color.FromArgb(255, green, 0);
            }
            else
            {
                // Yellow to Green gradient
                int red = (int)(255 * (1.0 - (score - 0.7) / 0.3));
                return Color.FromArgb(red, 255, 0);
            }
        }
    }
}