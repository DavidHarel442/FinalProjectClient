using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenCvSharp;
using DrawingPoint = System.Drawing.Point;



namespace ProjectClient.CameraAndRecognizing
{
    public static class MarkerDetectionExtensions
    {
        /// <summary>
        /// Calculates distance between two points
        /// </summary>
        public static double DistanceTo(this DrawingPoint point, DrawingPoint other)
        {
            return Math.Sqrt(Math.Pow(point.X - other.X, 2) + Math.Pow(point.Y - other.Y, 2));
        }
    }
}
