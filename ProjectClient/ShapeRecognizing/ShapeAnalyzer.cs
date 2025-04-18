using System;
using OpenCvSharp;
using OpenCvPoint = OpenCvSharp.Point;

namespace ProjectClient.ShapeRecognizing
{
    /// <summary>
    /// Analyzes and classifies shapes based on contour properties.
    /// Provides methods for shape recognition, classification, and comparison 
    /// using geometric properties like area, perimeter, and vertex count.
    /// </summary>
    public class ShapeAnalyzer
    {
        /// <summary>
        /// Shape type enumeration for better readability and classification.
        /// Defines the supported geometric shapes that can be recognized.
        /// </summary>
        public enum ShapeType
        {
            /// <summary>Unknown or unclassifiable shape</summary>
            Unknown = -1,

            /// <summary>Circle or near-circular shape</summary>
            Circle = 0,

            /// <summary>Triangle (3 vertices)</summary>
            Triangle = 1,

            /// <summary>Rectangle or square (4 vertices)</summary>
            Rectangle = 2,

            /// <summary>General polygon with 5+ vertices</summary>
            Polygon = 3
        }

        /// <summary>
        /// Gets the type of the reference shape used for comparison
        /// </summary>
        public ShapeType ReferenceType { get; private set; } = ShapeType.Unknown;

        /// <summary>
        /// Gets the area of the reference shape in square pixels
        /// </summary>
        public double ReferenceArea { get; private set; } = 0;

        /// <summary>
        /// Gets the compactness (circularity) of the reference shape.
        /// A perfect circle has a compactness of 1.0.
        /// </summary>
        public double ReferenceCompactness { get; private set; } = 0;

        /// <summary>
        /// Gets the number of vertices in the approximated reference shape
        /// </summary>
        public int ReferenceVertices { get; private set; } = 0;

        /// <summary>
        /// Analyzes a reference shape to extract its characteristics.
        /// Sets the reference properties that will be used for shape matching.
        /// </summary>
        /// <param name="contour">The contour points representing the shape to analyze</param>
        /// <remarks>
        /// This method calculates key geometric properties of the shape including:
        /// - Area: The enclosed area within the contour
        /// - Compactness: A measure of how circular the shape is
        /// - Vertices: The number of vertices in the approximated shape
        /// The results are stored as reference values for later comparison.
        /// </remarks>
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
        /// Calculates how well a contour matches the reference shape.
        /// Compares geometric properties to determine a similarity score.
        /// </summary>
        /// <param name="contour">The contour points representing the shape to match</param>
        /// <returns>A similarity score between 0.0 (no match) and 1.0 (perfect match)</returns>
        /// <remarks>
        /// The matching score is calculated using a weighted combination of:
        /// - Shape type match (40%)
        /// - Area similarity (20%)
        /// - Compactness similarity (20%)
        /// - Vertex count similarity (20%)
        /// </remarks>
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
        /// Gets a string name for the current reference shape type.
        /// </summary>
        /// <returns>The name of the reference shape type</returns>
        public string GetShapeTypeName()
        {
            return GetShapeTypeName(ReferenceType);
        }

        /// <summary>
        /// Gets a string name for a given shape type.
        /// Converts the ShapeType enum value to a human-readable string.
        /// </summary>
        /// <param name="shapeType">The shape type to get a name for</param>
        /// <returns>A string representing the shape type</returns>
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
        /// Classifies a shape based on its compactness and vertex count.
        /// Uses geometric properties to determine the most likely shape type.
        /// </summary>
        /// <param name="compactness">The compactness (circularity) of the shape</param>
        /// <param name="vertices">The number of vertices in the approximated shape</param>
        /// <returns>The classified shape type</returns>
        /// <remarks>
        /// Classification rules:
        /// - Circle: High compactness (>0.85) and many vertices (≥8)
        /// - Triangle: Exactly 3 vertices
        /// - Rectangle: Exactly 4 vertices
        /// - Polygon: Any other shape with distinct vertices
        /// </remarks>
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