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
            try
            {
                // Create the SharedDrawingForm
                SharedDrawingForm sharedDrawing = new SharedDrawingForm(tcpServer, username.Text);

                // Send message to server
                tcpServer.SendMessage("openedDrawing", "");

                // Hide the current form
                this.Hide();

                // Show the form as a non-modal window instead of ShowDialog
                sharedDrawing.Show();

                // Add a handler for when the form closes to show this form again
                sharedDrawing.FormClosed += (s, args) => this.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening drawing form: {ex.Message}");
                MessageBox.Show($"Error opening shared drawing: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}
