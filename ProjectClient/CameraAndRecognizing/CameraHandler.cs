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
    /// Handles camera initialization, management, and frame capture
    /// </summary>
    public class CameraHandler
    {
        // Camera devices
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        // Camera settings
        private int captureWidth = 640;
        private int captureHeight = 480;
        private bool skipFrames = false;
        private int frameCounter = 0;

        // Events
        public event EventHandler<FrameCapturedEventArgs> FrameCaptured;

        /// <summary>
        /// Event arguments for the FrameCaptured event
        /// </summary>
        public class FrameCapturedEventArgs : EventArgs
        {
            public Bitmap Frame { get; }

            public FrameCapturedEventArgs(Bitmap frame)
            {
                Frame = frame;
            }
        }

        /// <summary>
        /// Gets whether the camera is currently running
        /// </summary>
        public bool IsRunning => videoSource != null && videoSource.IsRunning;

        /// <summary>
        /// Find and start the camera
        /// </summary>
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
        /// Stop the camera
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
        /// Set camera quality settings
        /// </summary>
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
        /// Sets camera to use a lower resolution to improve performance
        /// </summary>
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
        /// Event handler for new camera frames
        /// </summary>
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
        /// Trigger the FrameCaptured event
        /// </summary>
        protected virtual void OnFrameCaptured(Bitmap frame)
        {
            FrameCaptured?.Invoke(this, new FrameCapturedEventArgs(frame));
        }

        /// <summary>
        /// Flips an image horizontally
        /// </summary>
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
