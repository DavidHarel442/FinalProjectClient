using System;
using System.Drawing;
using OpenCvSharp;

namespace ProjectClient.ShapeRecognizing
{
    /// <summary>
    /// Generates color masks for shape detection in HSV color space.
    /// Converts RGB colors to HSV and creates binary masks for detecting specific colors in images.
    /// Handles special cases like red that wraps around the hue spectrum.
    /// </summary>
    public class ColorMaskGenerator
    {
        /// <summary>
        /// Range of hue values to include around the target hue (in degrees, OpenCV scale)
        /// </summary>
        private const int HUE_RANGE = 15;

        /// <summary>
        /// Range of saturation values to include around the target saturation
        /// </summary>
        private const int SAT_RANGE = 50;

        /// <summary>
        /// Range of brightness/value to include around the target brightness
        /// </summary>
        private const int VAL_RANGE = 50;

        /// <summary>
        /// Creates a binary mask for the target color in HSV color space.
        /// Identifies pixels in the image that match the specified color within tolerance ranges.
        /// </summary>
        /// <param name="hsvImage">The input image in HSV color space</param>
        /// <param name="targetRgbColor">The target color in RGB format</param>
        /// <returns>A binary mask where white pixels indicate the presence of the target color</returns>
        public Mat CreateColorMask(Mat hsvImage, Color targetRgbColor)
        {
            // Convert RGB to HSV
            int h, s, v;
            RgbToHsv(targetRgbColor.R, targetRgbColor.G, targetRgbColor.B, out h, out s, out v);

            // Handle the special case of red which wraps around the hue value
            if (h < 15 || h > 165)
            {
                return CreateRedColorMask(hsvImage, h, s, v);
            }
            else
            {
                return CreateNormalColorMask(hsvImage, h, s, v);
            }
        }

        /// <summary>
        /// Creates a mask for red colors that wrap around the hue spectrum.
        /// Handles the special case where red spans both the beginning and end of the hue circle.
        /// </summary>
        /// <param name="hsvImage">The input image in HSV color space</param>
        /// <param name="h">The hue value of the target color (0-180 scale)</param>
        /// <param name="s">The saturation value of the target color (0-255 scale)</param>
        /// <param name="v">The brightness/value of the target color (0-255 scale)</param>
        /// <returns>A binary mask for red colors</returns>
        /// <remarks>
        /// Red is a special case because it wraps around the hue spectrum (near 180/0 boundary).
        /// This method creates two masks (one for each side of the boundary) and combines them.
        /// </remarks>
        private Mat CreateRedColorMask(Mat hsvImage, int h, int s, int v)
        {
            // For red hues that wrap around (near 180/0)
            Mat maskLow = new Mat();
            Mat maskHigh = new Mat();
            Mat mask = new Mat();

            if (h > 165)
            {
                var lowerBound = new Scalar(h - HUE_RANGE, Math.Max(s - SAT_RANGE, 30), Math.Max(v - VAL_RANGE, 30));
                var upperBound = new Scalar(180, 255, 255);
                Cv2.InRange(hsvImage, lowerBound, upperBound, maskHigh);

                lowerBound = new Scalar(0, Math.Max(s - SAT_RANGE, 30), Math.Max(v - VAL_RANGE, 30));
                upperBound = new Scalar(HUE_RANGE, 255, 255);
                Cv2.InRange(hsvImage, lowerBound, upperBound, maskLow);
            }
            else // h < 15
            {
                var lowerBound = new Scalar(0, Math.Max(s - SAT_RANGE, 30), Math.Max(v - VAL_RANGE, 30));
                var upperBound = new Scalar(h + HUE_RANGE, 255, 255);
                Cv2.InRange(hsvImage, lowerBound, upperBound, maskHigh);

                lowerBound = new Scalar(180 - HUE_RANGE, Math.Max(s - SAT_RANGE, 30), Math.Max(v - VAL_RANGE, 30));
                upperBound = new Scalar(180, 255, 255);
                Cv2.InRange(hsvImage, lowerBound, upperBound, maskLow);
            }

            // Combine the masks
            Cv2.BitwiseOr(maskLow, maskHigh, mask);

            // Clean up temporary mats
            maskLow.Dispose();
            maskHigh.Dispose();

            return mask;
        }

        /// <summary>
        /// Creates a mask for normal (non-red) colors that don't wrap around the hue spectrum.
        /// </summary>
        /// <param name="hsvImage">The input image in HSV color space</param>
        /// <param name="h">The hue value of the target color (0-180 scale)</param>
        /// <param name="s">The saturation value of the target color (0-255 scale)</param>
        /// <param name="v">The brightness/value of the target color (0-255 scale)</param>
        /// <returns>A binary mask for the specified color</returns>
        private Mat CreateNormalColorMask(Mat hsvImage, int h, int s, int v)
        {
            var lowerBound = new Scalar(
                Math.Max(h - HUE_RANGE, 0),
                Math.Max(s - SAT_RANGE, 30),
                Math.Max(v - VAL_RANGE, 30));

            var upperBound = new Scalar(
                Math.Min(h + HUE_RANGE, 180),
                255,
                255);

            Mat mask = new Mat();
            Cv2.InRange(hsvImage, lowerBound, upperBound, mask);
            return mask;
        }

        /// <summary>
        /// Converts RGB color values to HSV color space using OpenCV's scale (H: 0-180, S: 0-255, V: 0-255).
        /// </summary>
        /// <param name="r">Red component (0-255)</param>
        /// <param name="g">Green component (0-255)</param>
        /// <param name="b">Blue component (0-255)</param>
        /// <param name="h">Output hue value (0-180)</param>
        /// <param name="s">Output saturation value (0-255)</param>
        /// <param name="v">Output brightness/value (0-255)</param>
        /// <remarks>
        /// Note that OpenCV uses a hue range of 0-180 (instead of the standard 0-360)
        /// to fit within an 8-bit value. This method accounts for that scale.
        /// </remarks>
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

                // Convert to degrees for OpenCV (0-180)
                hue *= 30;
                h = (int)(hue);
            }
        }
    }
}