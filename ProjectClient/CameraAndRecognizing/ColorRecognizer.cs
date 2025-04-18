using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrawingPoint = System.Drawing.Point;
using AForgePoint = AForge.Point;

namespace ProjectClient.CameraAndRecognizing
{
    /// <summary>
    /// Class responsible for color-based marker detection in camera frames.
    /// Provides functionality for calibrating to specific colors, finding markers 
    /// based on color similarity, and automatically adjusting thresholds for optimal detection.
    /// </summary>
    public class ColorRecognizer
    {


        /// <summary>
        /// More restrictive threshold used when precision is needed
        /// </summary>
        private int strictColorThreshold = 30;

        /// <summary>
        /// Current active threshold value being used for detection
        /// </summary>
        private int currentColorThreshold = 50;

        /// <summary>
        /// Timestamp of the last threshold adjustment for timing control
        /// </summary>
        private DateTime? lastColorThresholdAdjustment = null;


      

        /// <summary>
        /// Step size for pixel sampling (higher values improve performance but reduce accuracy)
        /// </summary>
        private int samplingStep = 2;

        /// <summary>
        /// Minimum time in milliseconds between threshold adjustments
        /// </summary>
        private const int THRESHOLD_ADJUSTMENT_INTERVAL_MS = 500;

        /// <summary>
        /// Maximum threshold value allowed
        /// </summary>
        private const int MAX_THRESHOLD = 150;

        /// <summary>
        /// Calibrates the detector by finding the dominant color at the specified location.
        /// Takes a sample area around the provided point and determines the most representative color.
        /// </summary>
        /// <param name="image">The bitmap image to calibrate from</param>
        /// <param name="location">The point in the image where the marker is located</param>
        /// <returns>The calibrated color for marker detection</returns>
        /// <exception cref="ArgumentNullException">Thrown when image is null</exception>
        /// <exception cref="ArgumentException">Thrown when location is outside image bounds</exception>
        public Color Calibrate(Bitmap image, DrawingPoint location)
        {
            Console.WriteLine($"Calibrating at {location.X},{location.Y} in the image");
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (location.X < 0 || location.X >= image.Width ||
                location.Y < 0 || location.Y >= image.Height)
            {
                throw new ArgumentException("Click location is outside the image bounds.");
            }

            // Take a larger sample area to better capture the marker color
            int sampleRadius = 5; // Sample a 11x11 area (5 pixels in each direction)

            // Track colors and frequencies
            Dictionary<Color, int> colorFrequency = new Dictionary<Color, int>();

            for (int y = Math.Max(0, location.Y - sampleRadius);
                 y <= Math.Min(image.Height - 1, location.Y + sampleRadius); y++)
            {
                for (int x = Math.Max(0, location.X - sampleRadius);
                     x <= Math.Min(image.Width - 1, location.X + sampleRadius); x++)
                {
                    Color pixelColor = image.GetPixel(x, y);

                    // Simplify the color to reduce noise (round to nearest 5)
                    int r = 5 * (int)Math.Round(pixelColor.R / 5.0);
                    int g = 5 * (int)Math.Round(pixelColor.G / 5.0);
                    int c = 5 * (int)Math.Round(pixelColor.B / 5.0);

                    Color simplifiedColor = Color.FromArgb(r, g, c);

                    if (colorFrequency.ContainsKey(simplifiedColor))
                        colorFrequency[simplifiedColor]++;
                    else
                        colorFrequency[simplifiedColor] = 1;
                }
            }

            // Find the most common color in the sample area
            Color dominantColor = Color.Black;
            int maxFrequency = 0;

            foreach (var kvp in colorFrequency)
            {
                if (kvp.Value > maxFrequency)
                {
                    maxFrequency = kvp.Value;
                    dominantColor = kvp.Key;
                }
            }

            // For highlighters, we need to ensure we're getting a saturated color
            // Calculate HSB values
            double h, s, b;
            RgbToHsb(dominantColor.R, dominantColor.G, dominantColor.B, out h, out s, out b);

            // If the dominant color has very low saturation, it might be the background
            // In that case, find the most saturated color with reasonable frequency
            if (s < 0.2)
            {
                Console.WriteLine("Dominant color has low saturation, looking for more saturated colors");

                double maxSaturation = 0;
                Color mostSaturatedColor = dominantColor;

                foreach (var kvp in colorFrequency)
                {
                    if (kvp.Value >= maxFrequency * 0.3) // At least 30% as frequent as dominant color
                    {
                        RgbToHsb(kvp.Key.R, kvp.Key.G, kvp.Key.B, out h, out s, out b);

                        if (s > maxSaturation)
                        {
                            maxSaturation = s;
                            mostSaturatedColor = kvp.Key;
                        }
                    }
                }

                // If we found a more saturated color, use it
                if (maxSaturation > 0.2)
                {
                    dominantColor = mostSaturatedColor;
                    Console.WriteLine($"Using more saturated color: R={dominantColor.R}, G={dominantColor.G}, B={dominantColor.B}");
                }
            }

            // Log the calibrated color
            Console.WriteLine($"Calibrated color: R={dominantColor.R}, G={dominantColor.G}, B={dominantColor.B}");
            Console.WriteLine($"Sample size: {colorFrequency.Count} unique colors, most frequent appeared {maxFrequency} times");

            return dominantColor;
        }



        /// <summary>
        /// Finds a marker in the frame based on color similarity to the target color.
        /// Uses fast bitmap access for performance and calculates weighted centroid of matching pixels.
        /// </summary>
        /// <param name="frame">The bitmap frame to search in</param>
        /// <param name="targetColor">The target color to look for</param>
        /// <param name="samplingStep">The step size for pixel sampling (higher = faster but less accurate)</param>
        /// <returns>The point where the marker was found, or null if not detected</returns>
        public DrawingPoint? FindMarker(Bitmap frame, Color targetColor, int samplingStep)
        {
            if (frame == null || targetColor == null)
                return null;

            int width = frame.Width;
            int height = frame.Height;

            // Use BitmapData and unsafe code for faster pixel access
            BitmapData bitmapData = frame.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;

            // Store sum of x, y, and total count of pixels in range
            long sumX = 0, sumY = 0;
            long inRangeCount = 0;

            // Store weighted values for better accuracy
            long weightedSumX = 0, weightedSumY = 0;
            long totalWeight = 0;

            unsafe
            {
                for (int y = 0; y < height; y += samplingStep)
                {
                    byte* row = (byte*)scan0 + (y * stride);
                    for (int x = 0; x < width; x += samplingStep)
                    {
                        // BGR format
                        byte b = row[x * 3 + 0];
                        byte g = row[x * 3 + 1];
                        byte r = row[x * 3 + 2];

                        Color currentColor = Color.FromArgb(r, g, b);

                        // Compare color distance
                        int distance = ColorDistance(currentColor, targetColor);
                        if (distance < currentColorThreshold)
                        {
                            // Calculate a weight based on how close the color match is
                            // Closer matches get higher weights
                            int weight = currentColorThreshold - distance;

                            // Regular averaging
                            sumX += x;
                            sumY += y;
                            inRangeCount++;

                            // Weighted averaging for better precision
                            weightedSumX += x * weight;
                            weightedSumY += y * weight;
                            totalWeight += weight;
                        }
                    }
                }
            }

            frame.UnlockBits(bitmapData);

            if (inRangeCount > 0)
            {
                Point rawPosition;

                // Use weighted average if we have weights, otherwise use regular average
                if (totalWeight > 0)
                {
                    int centerX = (int)(weightedSumX / totalWeight);
                    int centerY = (int)(weightedSumY / totalWeight);
                    rawPosition = new Point(centerX, centerY);
                }
                else
                {
                    int centerX = (int)(sumX / inRangeCount);
                    int centerY = (int)(sumY / inRangeCount);
                    rawPosition = new Point(centerX, centerY);
                }

                // Very minimal logging
                if (inRangeCount > 20 && DateTime.Now.Second % 10 == 0 && DateTime.Now.Millisecond < 50)
                {
                    Console.WriteLine($"Found {inRangeCount} matching pixels");
                }

                return rawPosition;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the sampling step for color detection.
        /// Larger values improve performance but reduce detection precision.
        /// </summary>
        /// <param name="step">The sampling step size (minimum 1)</param>
        public void SetSamplingStep(int step)
        {
            this.samplingStep = Math.Max(1, step);
        }



        /// <summary>
        /// Calculates the distance between two colors using a weighted combination 
        /// of RGB Euclidean distance and HSB components to better match human color perception.
        /// </summary>
        /// <param name="c1">The first color</param>
        /// <param name="c2">The second color</param>
        /// <returns>A weighted distance value representing color similarity</returns>
        private int ColorDistance(Color c1, Color c2)
        {
            // Basic Euclidean distance in RGB space
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;

            // For highlighters, the hue is most important, so we'll add a hue comparison
            // Convert to HSB/HSV to get hue
            double h1, s1, b1, h2, s2, b2;
            RgbToHsb(c1.R, c1.G, c1.B, out h1, out s1, out b1);
            RgbToHsb(c2.R, c2.G, c2.B, out h2, out s2, out b2);

            // Calculate hue difference (handle the circular nature of hue)
            double hueDiff = Math.Min(Math.Abs(h1 - h2), 1 - Math.Abs(h1 - h2));

            // For highlighters, high saturation and brightness are key
            double saturationDiff = Math.Abs(s1 - s2);
            double brightnessDiff = Math.Abs(b1 - b2);

            // Combined weighted distance (hue is most important for colored markers)
            return (int)(Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff) * 0.5 +
                        hueDiff * 100 * 0.3 +
                        saturationDiff * 100 * 0.1 +
                        brightnessDiff * 100 * 0.1);
        }

        /// <summary>
        /// Converts RGB values to HSB (Hue, Saturation, Brightness) color space.
        /// </summary>
        /// <param name="r">The red component (0-255)</param>
        /// <param name="g">The green component (0-255)</param>
        /// <param name="b">The blue component (0-255)</param>
        /// <param name="hue">Output parameter for hue (0.0-1.0)</param>
        /// <param name="saturation">Output parameter for saturation (0.0-1.0)</param>
        /// <param name="bright">Output parameter for brightness (0.0-1.0)</param>
        private void RgbToHsb(int r, int g, int b, out double hue, out double saturation, out double bright)
        {
            // Normalize R,G,B values
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));

            hue = 0; // Default hue (undefined for grayscale)
            saturation = 0;
            bright = max; // Brightness is the max value

            // If max and min are equal, the color is a shade of gray
            if (Math.Abs(max - min) < 0.0001)
            {
                hue = 0; // Undefined
                saturation = 0;
            }
            else
            {
                // Calculate saturation
                saturation = (max - min) / max;

                // Calculate hue
                double delta = max - min;

                if (red == max)
                {
                    // Between yellow and magenta
                    hue = (green - blue) / delta;
                }
                else if (green == max)
                {
                    // Between cyan and yellow
                    hue = 2 + (blue - red) / delta;
                }
                else
                {
                    // Between magenta and cyan
                    hue = 4 + (red - green) / delta;
                }

                // Convert hue to degrees then normalize to 0-1
                hue *= 60;
                if (hue < 0)
                    hue += 360;

                hue /= 360.0;
            }
        }


    }
}