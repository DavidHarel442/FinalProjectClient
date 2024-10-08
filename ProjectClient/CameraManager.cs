using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectClient
{
    public class CameraManager
    {// Manages camera operations including starting, stopping, and displaying the camera feed.
     // taken from claude

        /// <summary>
        /// The video capture device used for accessing the camera.
        /// </summary>
        private VideoCaptureDevice videoSource;
        /// <summary>
        /// The PictureBox control used to display the camera feed.
        /// </summary>
        private PictureBox displayPictureBox;

        /// <summary>
        /// Initializes a new instance of the CameraManager class.
        /// </summary>
        /// <param name="pictureBox">The PictureBox control where the camera feed will be displayed.</param>
        public CameraManager(PictureBox pictureBox)
        {
            displayPictureBox = pictureBox;
        }
        /// <summary>
        /// Starts the camera and begins capturing video.
        /// </summary>
        /// <returns>True if the camera was successfully started; otherwise, false.</returns>
        public bool StartCamera()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0) return false;

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            return true;
        }

        /// <summary>
        /// Handles new frames received from the video source.
        /// This method is called for each new frame captured by the camera.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var mirroredFrame = MirrorImage((Bitmap)eventArgs.Frame.Clone());
            displayPictureBox.Invoke((MethodInvoker)delegate {
                var oldImage = displayPictureBox.Image;
                displayPictureBox.Image = mirroredFrame;
                oldImage?.Dispose();
            });
        }
        /// <summary>
        /// reates a mirrored (horizontally flipped) version of the provided image.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>A new Bitmap that is a mirrored version of the source image.
        private Bitmap MirrorImage(Bitmap source)
        {
            var mirrored = new Bitmap(source.Width, source.Height);
            using (var g = Graphics.FromImage(mirrored))
            {
                g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                    source.Width, 0, -source.Width, source.Height, GraphicsUnit.Pixel);
            }
            source.Dispose();
            return mirrored;
        }
        /// <summary>
        /// Stops the camera, ceases video capture, and cleans up resources.
        /// </summary>
        public void StopCamera()
        {
            if (videoSource?.IsRunning == true)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            displayPictureBox.Image?.Dispose();
            displayPictureBox.Image = null;
        }
    }
}