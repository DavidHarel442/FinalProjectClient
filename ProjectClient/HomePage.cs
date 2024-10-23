using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class HomePage : Form
    {
        /// <summary>
        /// a property sent through each class starting from HathatulClient. which you use to communicate with the server
        /// </summary>
        public TcpServerCommunication tcpServer = null;
        /// <summary>
        /// constructor which Initialize the form, receives the object for communication and firstname
        /// </summary>
        public HomePage(TcpServerCommunication client,string firstname)
        {
            InitializeComponent();
            tcpServer = client;
            username.Text = firstname;
            MessageHandler.SetCurrentForm(this);
        }
        /// <summary>
        /// event called when pressed on OpenDrawingForm button. it starts the SharedDrawingForm.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenDrawingForm_Click(object sender, EventArgs e)
        {
                SharedDrawingForm sharedDrawing = new SharedDrawingForm(tcpServer, username.Text);
                tcpServer.SendMessage("OpenedDrawing", "");
                this.Hide();
                sharedDrawing.ShowDialog();
            
        }
    }
}
