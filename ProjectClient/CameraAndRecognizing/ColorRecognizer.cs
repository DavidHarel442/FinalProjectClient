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
    /// Class responsible for color-based marker detection
    /// </summary>
    public class ColorRecognizer
    {
        private int baseColorThreshold = 50;
        private int strictColorThreshold = 30;
        private int currentColorThreshold = 50;
        private DateTime? lastColorThresholdAdjustment = null;
        // Color detection settings
        private const int DEFAULT_COLOR_THRESHOLD = 50;
        private int colorThreshold = DEFAULT_COLOR_THRESHOLD;
        private int samplingStep = 2;

        /// <summary>
        /// Calibrates the detector by finding the dominant color at the specified location
        /// </summary>
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
        /// Find a marker in the frame based on color similarity to the target color
        /// </summary>
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
                        if (distance < colorThreshold)
                        {
                            // Calculate a weight based on how close the color match is
                            // Closer matches get higher weights
                            int weight = colorThreshold - distance;

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
        /// Set the sampling step for color detection
        /// </summary>
        public void SetSamplingStep(int step)
        {
            this.samplingStep = Math.Max(1, step);
        }

        /// <summary>
        /// Set the color threshold for detection
        /// </summary>
        public void SetColorThreshold(int threshold)
        {
            this.colorThreshold = Math.Max(5, threshold);
        }

        /// <summary>
        /// Calculate the distance between two colors
        /// </summary>
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
        /// Convert RGB values to HSB (Hue, Saturation, Brightness)
        /// </summary>
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
        public void DrawDetectionVisuals(Graphics g, Bitmap originalFrame, Color targetColor, float scaleX, float scaleY)
        {
            // Draw a color swatch showing the target color
            int swatchSize = 30;
            Rectangle colorRect = new Rectangle(
                (int)((originalFrame.Width - swatchSize - 10) * scaleX),
                (int)(10 * scaleY),
                (int)(swatchSize * scaleX),
                (int)(swatchSize * scaleY));

            using (SolidBrush colorBrush = new SolidBrush(targetColor))
            {
                g.FillRectangle(colorBrush, colorRect);
                g.DrawRectangle(new Pen(Color.White, 2), colorRect);
            }

            // Label the color swatch
            using (Font font = new Font("Arial", 8))
            {
                string colorText = $"R:{targetColor.R} G:{targetColor.G} B:{targetColor.B}";
                g.DrawString(colorText, font, Brushes.White,
                    (originalFrame.Width - swatchSize - 10) * scaleX,
                    (10 + swatchSize + 5) * scaleY);
            }
        }
        public void ConfigureAdaptiveThreshold(int baseThreshold, int strictThreshold)
        {
            baseColorThreshold = Math.Max(20, baseThreshold);
            strictColorThreshold = Math.Min(baseColorThreshold - 10, strictThreshold);
            currentColorThreshold = baseColorThreshold;
            Console.WriteLine($"Color thresholds set: Base={baseColorThreshold}, Strict={strictColorThreshold}");
        }
        /// <summary>
        /// Update color threshold based on marker detection status with specific timing
        /// </summary>
        public void UpdateColorThreshold(bool markerLost, DateTime? lostTime)
        {
            // Reset threshold if no adaptive behavior needed
            if (!markerLost || !lostTime.HasValue)
            {
                // Gradually return to base threshold if not at base already
                if (currentColorThreshold != baseColorThreshold &&
                    (DateTime.Now - (lastColorThresholdAdjustment ?? DateTime.Now)).TotalSeconds > 0.5)
                {
                    if (currentColorThreshold < baseColorThreshold)
                    {
                        currentColorThreshold = Math.Min(baseColorThreshold, currentColorThreshold + 5);
                    }
                    else
                    {
                        currentColorThreshold = Math.Max(baseColorThreshold, currentColorThreshold - 5);
                    }
                    lastColorThresholdAdjustment = DateTime.Now;
                }
                return;
            }

            // Calculate how long the marker has been lost
            TimeSpan lostDuration = DateTime.Now - lostTime.Value;

            // Exactly at 1 second: Apply strict color threshold immediately
            // This matches the requirement to become stricter with color after 1 second
            if (lostDuration.TotalSeconds >= 1.0 && lostDuration.TotalSeconds < 1.1 &&
                currentColorThreshold > strictColorThreshold)
            {
                // Make significant immediate change to color threshold
                int previousThreshold = currentColorThreshold;
                currentColorThreshold = strictColorThreshold;
                lastColorThresholdAdjustment = DateTime.Now;

                Console.WriteLine($"STAGE 1 STRICTNESS: Color threshold changed from {previousThreshold} to {currentColorThreshold} (strict color detection active)");
            }
            // Between 1-1.5 seconds: Maintain strict color threshold
            else if (lostDuration.TotalSeconds >= 1.0 && lostDuration.TotalSeconds < 1.5)
            {
                // Keep color detection strict during this period
                if (currentColorThreshold > strictColorThreshold &&
                    (DateTime.Now - (lastColorThresholdAdjustment ?? DateTime.Now)).TotalMilliseconds > 200)
                {
                    currentColorThreshold = strictColorThreshold;
                    lastColorThresholdAdjustment = DateTime.Now;
                }
            }
            // Between 1.5-3 seconds: Keep strict but allow minor relaxation
            else if (lostDuration.TotalSeconds >= 1.5 && lostDuration.TotalSeconds < 3.0)
            {
                // Keep color detection mostly strict during this period
                // but allow minimal relaxation if needed to improve detection chances
                if (currentColorThreshold > strictColorThreshold + 5 &&
                    (DateTime.Now - (lastColorThresholdAdjustment ?? DateTime.Now)).TotalMilliseconds > 200)
                {
                    currentColorThreshold = Math.Max(strictColorThreshold, currentColorThreshold - 2);
                    lastColorThresholdAdjustment = DateTime.Now;
                }
            }
            // After 3+ seconds: Gradually relax color threshold
            else if (lostDuration.TotalSeconds >= 3.0)
            {
                // After extended loss, gradually relax color threshold to improve detection chances
                if (currentColorThreshold < baseColorThreshold &&
                    (DateTime.Now - (lastColorThresholdAdjustment ?? DateTime.Now)).TotalMilliseconds > 300)
                {
                    currentColorThreshold = Math.Min(baseColorThreshold + 10, currentColorThreshold + 3);
                    lastColorThresholdAdjustment = DateTime.Now;

                    if (DateTime.Now.Second % 5 == 0 && DateTime.Now.Millisecond < 50)
                    {
                        Console.WriteLine($"After {lostDuration.TotalSeconds:F1}s: Color threshold relaxed to {currentColorThreshold} (improving detection chances)");
                    }
                }
            }

            // Log current state occasionally
            if (DateTime.Now.Second % 5 == 0 && DateTime.Now.Millisecond < 20 &&
                lostDuration.TotalSeconds >= 1.0)
            {
                Console.WriteLine($"Current color threshold: {currentColorThreshold} " +
                                 $"(Base: {baseColorThreshold}, Strict: {strictColorThreshold})");
            }
        }
    }
}

