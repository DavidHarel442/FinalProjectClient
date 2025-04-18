using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient.CameraAndRecognizing
{
    public class MarkerDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the position of the detected marker
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// Gets the size of the camera frame
        /// </summary>
        public Size CameraSize { get; }

        /// <summary>
        /// Initializes a new instance of the MarkerDetectedEventArgs class.
        /// </summary>
        /// <param name="position">The position of the detected marker</param>
        /// <param name="cameraSize">The size of the camera frame</param>
        public MarkerDetectedEventArgs(Point position, Size cameraSize)
        {
            Position = position;
            CameraSize = cameraSize;
        }
    }
}
