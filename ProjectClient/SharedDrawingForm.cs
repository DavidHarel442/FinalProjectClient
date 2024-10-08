using System;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class SharedDrawingForm : Form
    {// this form will be the main form for the shared drawing and recognizing of shapes 

        /// <summary>
        /// shared object passed through every class to communicated with server
        /// </summary>
        private TcpServerCommunication tcpServer;
        /// <summary>
        /// object responsible for camera 
        /// </summary>
        private CameraManager cameraManager;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tcpServer"></param>
        public SharedDrawingForm(TcpServerCommunication tcpServer)
        {
            InitializeComponent();
            this.tcpServer = tcpServer;
            MessageHandler.SetCurrentForm(this);
            cameraManager = new CameraManager(Camera);
        }
        /// <summary>
        /// function to start or stop camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowCamera_Click(object sender, EventArgs e)
        {
            if (ShowCamera.Text == "Start Camera")
            {
                if (cameraManager.StartCamera())
                {
                    ShowCamera.Text = "Stop Camera";
                }
            }
            else
            {
                cameraManager.StopCamera();
                ShowCamera.Text = "Start Camera";
            }
        }

        /// <summary>
        ///  what happens when form is being closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SharedDrawingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            cameraManager.StopCamera();
        }
    }
}