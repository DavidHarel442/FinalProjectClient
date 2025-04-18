using AForge.Video.DirectShow;
using AForge.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient.CameraAndRecognizing
{
    /// <summary>
    /// Handles camera initialization, management, and frame capture from video input devices.
    /// Provides methods for starting and stopping the camera, adjusting quality settings,
    /// and events for processing captured frames.
    /// </summary>
    public class CameraHandler
    {
        /// <summary>
        /// Collection of available video input devices on the system
        /// </summary>
        private FilterInfoCollection videoDevices;

        /// <summary>
        /// The active video capture device
        /// </summary>
        private VideoCaptureDevice videoSource;

        /// <summary>
        /// Target width for video capture (in pixels)
        /// </summary>
        private int captureWidth = 640;

        /// <summary>
        /// Target height for video capture (in pixels)
        /// </summary>
        private int captureHeight = 480;

        /// <summary>
        /// Flag indicating whether to process every frame or skip alternate frames
        /// </summary>
        private bool skipFrames = false;

        /// <summary>
        /// Counter used when frame skipping is enabled
        /// </summary>
        private int frameCounter = 0;

        /// <summary>
        /// Event raised when a new frame is captured from the camera.
        /// Subscribers receive the captured frame as a Bitmap.
        /// </summary>
        public event EventHandler<FrameCapturedEventArgs> FrameCaptured;

        /// <summary>
        /// Event arguments class for the FrameCaptured event.
        /// Contains the captured frame as a Bitmap.
        /// </summary>
        public class FrameCapturedEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the captured frame
            /// </summary>
            public Bitmap Frame { get; }

            /// <summary>
            /// Initializes a new instance of the FrameCapturedEventArgs class
            /// </summary>
            /// <param name="frame">The captured frame as a Bitmap</param>
            public FrameCapturedEventArgs(Bitmap frame)
            {
                Frame = frame;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the camera is currently running
        /// </summary>
        /// <value>
        /// <c>true</c> if the camera is running; otherwise, <c>false</c>
        /// </value>
        public bool IsRunning => videoSource != null && videoSource.IsRunning;

        /// <summary>
        /// Initializes and starts the camera capture process.
        /// Finds available video devices, selects the first one,
        /// configures resolution settings, and begins capturing frames.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the camera was successfully started; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// This method attaches the <see cref="VideoSource_NewFrame"/> event handler
        /// to process each frame captured from the camera.
        /// </remarks>
        /// <exception cref="Exception">Thrown when an error occurs during camera initialization</exception>
        public bool StartCamera()
        {
            try
            {
                // 1) Find all video input devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No camera device found!");
                    return false;
                }

                // 2) Select the first camera
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

                // Set lower resolution video capabilities
                SetLowResolutionCapabilities();

                // 3) Hook the NewFrame event to process each frame
                videoSource.NewFrame += VideoSource_NewFrame;

                // 4) Start the camera
                videoSource.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops the camera and releases associated resources.
        /// Unhooks event handlers and signals the video source to stop.
        /// </summary>
        public void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= VideoSource_NewFrame;
                videoSource = null;
            }
        }

        /// <summary>
        /// Updates camera quality settings and applies them.
        /// If the camera is running, it will be stopped and restarted with the new settings.
        /// </summary>
        /// <param name="width">The target capture width in pixels</param>
        /// <param name="height">The target capture height in pixels</param>
        /// <param name="skipFrames">Whether to enable frame skipping for performance</param>
        public void SetQualitySettings(int width, int height, bool skipFrames)
        {
            this.captureWidth = width;
            this.captureHeight = height;
            this.skipFrames = skipFrames;

            // Apply if camera is already running
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();

                // Restart with new settings
                SetLowResolutionCapabilities();
                videoSource.Start();
            }
        }

        /// <summary>
        /// Configures the camera to use an appropriate resolution close to the target dimensions.
        /// Searches through available video capabilities and selects the closest match to the
        /// configured capture width and height.
        /// </summary>
        /// <remarks>
        /// Continues with default settings if an error occurs or no suitable resolution is found.
        /// </remarks>
        /// <exception cref="Exception">May be thrown when accessing video capabilities</exception>
        private void SetLowResolutionCapabilities()
        {
            try
            {
                // Get available video capabilities
                var videoCapabilities = videoSource.VideoCapabilities;
                if (videoCapabilities != null && videoCapabilities.Length > 0)
                {
                    // Try to find a video mode close to our target resolution
                    var selectedMode = videoCapabilities
                        .OrderBy(caps => Math.Abs(caps.FrameSize.Width - captureWidth) +
                                 Math.Abs(caps.FrameSize.Height - captureHeight))
                        .FirstOrDefault();

                    if (selectedMode != null)
                    {
                        Console.WriteLine($"Selected camera mode: {selectedMode.FrameSize.Width}x{selectedMode.FrameSize.Height} at {selectedMode.AverageFrameRate} FPS");
                        videoSource.VideoResolution = selectedMode;
                    }
                    else
                    {
                        Console.WriteLine("No suitable video mode found. Using default.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting video capabilities: {ex.Message}");
                // Continue with default settings
            }
        }

        /// <summary>
        /// Event handler that processes each new frame received from the camera.
        /// Implements frame skipping if enabled, creates a copy of the frame, flips it
        /// for a more intuitive view, and raises the FrameCaptured event.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="eventArgs">The event data containing the new frame</param>
        /// <remarks>
        /// The original frame is cloned to avoid threading issues, as the source
        /// frame may be modified by the camera source before processing is complete.
        /// </remarks>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Skip frames if enabled
            if (skipFrames)
            {
                frameCounter++;
                if (frameCounter % 2 != 0)
                {
                    return; // Skip this frame
                }
            }

            try
            {
                // Create a clone of the frame to work with
                Bitmap originalFrame = (Bitmap)eventArgs.Frame.Clone();

                // Flip the frame horizontally for a more intuitive view
                Bitmap flippedFrame = FlipImageHorizontally(originalFrame);

                // Notify subscribers
                OnFrameCaptured(flippedFrame);

                // Clean up original frame
                originalFrame.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Frame processing error: {ex.Message}");
            }
        }

        /// <summary>
        /// Raises the FrameCaptured event with the provided frame
        /// </summary>
        /// <param name="frame">The captured and processed frame to provide to event subscribers</param>
        protected virtual void OnFrameCaptured(Bitmap frame)
        {
            FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(frame));
        }

        /// <summary>
        /// Creates a horizontally mirrored version of the source image
        /// </summary>
        /// <param name="source">The original bitmap to flip</param>
        /// <returns>A new Bitmap containing the flipped image</returns>
        /// <remarks>
        /// The mirroring effect is achieved by applying a transformation matrix
        /// that scales by -1 in the X direction.
        /// </remarks>
        private Bitmap FlipImageHorizontally(Bitmap source)
        {
            // Create a new bitmap with the same dimensions
            Bitmap flipped = new Bitmap(source.Width, source.Height);

            // Create graphics object to do the flipping
            using (Graphics g = Graphics.FromImage(flipped))
            {
                // Set up transformation to flip horizontally
                g.TranslateTransform(source.Width, 0);
                g.ScaleTransform(-1, 1);

                // Draw the original image with the transformation applied
                g.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            return flipped;
        }
    }
}