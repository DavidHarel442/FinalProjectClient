using System;
using OpenCvSharp;
using OpenCvPoint = OpenCvSharp.Point;

namespace ProjectClient.ShapeRecognizing
{
    /// <summary>
    /// Analyzes and classifies shapes based on contour properties
    /// </summary>
    public class ShapeAnalyzer
    {

        /// <summary>
        /// Shape type enumeration for better readability
        /// </summary>
        public enum ShapeType
        {
            Unknown = -1,
            Circle = 0,
            Triangle = 1,
            Rectangle = 2,
            Polygon = 3
        }



        // Reference shape properties
        public ShapeType ReferenceType { get; private set; } = ShapeType.Unknown;
        public double ReferenceArea { get; private set; } = 0;
        public double ReferenceCompactness { get; private set; } = 0;
        public int ReferenceVertices { get; private set; } = 0;



        /// <summary>
        /// Analyzes a reference shape to extract its characteristics
        /// </summary>
        public void AnalyzeReferenceShape(OpenCvPoint[] contour)
        {
            // Calculate area of the contour
            ReferenceArea = Cv2.ContourArea(contour);

            // Calculate perimeter of the contour
            double perimeter = Cv2.ArcLength(contour, true);

            // Calculate compactness (circularity)
            ReferenceCompactness = (4 * Math.PI * ReferenceArea) / (perimeter * perimeter);

            // Approximate the contour to find its shape
            OpenCvPoint[] approx = Cv2.ApproxPolyDP(contour, 0.03 * perimeter, true);
            ReferenceVertices = approx.Length;

            // Determine the shape type
            ReferenceType = ClassifyShape(ReferenceCompactness, ReferenceVertices);
        }

        /// <summary>
        /// Calculate how well a contour matches our reference shape
        /// </summary>
        public double CalculateShapeMatchScore(OpenCvPoint[] contour)
        {
            // Calculate basic shape properties
            double area = Cv2.ContourArea(contour);
            double perimeter = Cv2.ArcLength(contour, true);
            double compactness = (4 * Math.PI * area) / (perimeter * perimeter);

            // Approximate contour to get vertices
            OpenCvPoint[] approx = Cv2.ApproxPolyDP(contour, 0.03 * perimeter, true);
            int vertices = approx.Length;

            // Calculate shape scores
            double areaScore = 1.0 - Math.Min(1.0, Math.Abs(area - ReferenceArea) / ReferenceArea);
            double compactnessScore = 1.0 - Math.Min(1.0,
                Math.Abs(compactness - ReferenceCompactness) / Math.Max(0.1, ReferenceCompactness));

            // Determine shape type
            ShapeType shapeType = ClassifyShape(compactness, vertices);

            // Type matching score
            double typeScore = (shapeType == ReferenceType) ? 1.0 : 0.3;

            // For non-circles, also consider vertices
            double verticesScore = 1.0;
            if (ReferenceType != ShapeType.Circle && vertices != ReferenceVertices)
            {
                verticesScore = 0.5;
            }

            // Combine scores with weights
            return typeScore * 0.4 + areaScore * 0.2 + compactnessScore * 0.2 + verticesScore * 0.2;
        }

        /// <summary>
        /// Get a string name for the reference shape type
        /// </summary>
        public string GetShapeTypeName()
        {
            return GetShapeTypeName(ReferenceType);
        }

        /// <summary>
        /// Get a string name for a shape type
        /// </summary>
        public string GetShapeTypeName(ShapeType shapeType)
        {
            switch (shapeType)
            {
                case ShapeType.Circle: return "Circle";
                case ShapeType.Triangle: return "Triangle";
                case ShapeType.Rectangle: return "Rectangle";
                case ShapeType.Polygon: return "Polygon";
                default: return "Unknown";
            }
        }



        /// <summary>
        /// Classify a shape based on compactness and vertex count
        /// </summary>
        private static ShapeType ClassifyShape(double compactness, int vertices)
        {
            if (compactness > 0.85 && vertices >= 8)
            {
                return ShapeType.Circle;
            }
            else if (vertices == 3)
            {
                return ShapeType.Triangle;
            }
            else if (vertices == 4)
            {
                return ShapeType.Rectangle;
            }
            else
            {
                return ShapeType.Polygon;
            }
        }

    }
}