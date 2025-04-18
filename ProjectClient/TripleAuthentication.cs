using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class TripleAuthentication : Form
    {
        /// <summary>
        /// this property checks if the user is trying to login or register
        /// </summary>
        public bool loginOrRegister;
        /// <summary>
        /// this property will keep all the information for register/login for after verification
        /// </summary>
        public string allInfo = "";
        /// <summary>
        /// a property sent through each class starting from Program. which the client uses to communicate with the server
        /// </summary>
        private TcpServerCommunication tcpServer;

        /// <summary>
        /// constructor, initializes the TripleAuthentication form.
        /// </summary>
        /// <param name="tcpServer"></param>
        /// <param name="loginOrRegister"></param>
        /// <param name="allInfo"></param>
        public TripleAuthentication(TcpServerCommunication tcpServer, bool loginOrRegister,string allInfo) // this boolean will be true if its a login attempt and false if its a register one
        {
            InitializeComponent();
            this.tcpServer = tcpServer;
            MessageHandler.SetCurrentForm(this);
            this.allInfo = allInfo;
            this.loginOrRegister    = loginOrRegister;

        }

        /// <summary>
        /// shows the captcha image.
        /// it is doing so by converting the base64 image to bytes array then puts the bytes in a memory stream
        /// and then puts it in the picturebox
        /// </summary>
        /// <param name="base64Image"></param>
        public void UpdateCaptchaImage(string base64Image)//taken from claude
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            using (var ms = new MemoryStream(imageBytes))
            {
                captchaImage.Image = Image.FromStream(ms);
            }
        }
        /// <summary>
        /// event called when pressed on the verift button, sends the server a message with the code and captcha to authenticate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void verify_Click(object sender, EventArgs e)
        {
            tcpServer.SendMessage("Verify",code.Text + '\t' + captcha.Text);
        }

        private void backToLogin_Click(object sender, EventArgs e)
        {
            if (loginOrRegister)
            {
                LoginForm BackToGame = new LoginForm(tcpServer, false);
                this.Hide();
                BackToGame.ShowDialog();
            }
        }
    }
}
