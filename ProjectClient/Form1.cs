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
    public partial class Form1 : Form
    {
        private TcpServerCommunication tcpServer;
        public Form1()
        {
            InitializeComponent();
            tcpServer = new TcpServerCommunication();
            Console.WriteLine("Attempting to connect");
            tcpServer.ConnectToServer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!tcpServer.usernameSent)
            {
                Console.WriteLine("sending username");
                tcpServer.HandleUsernameMessage("hello");
            }
            else
            {
                Console.WriteLine("sending test message");

               // tcpServer.SendMessage("nigga", "Test message");
               // tcpServer.SendMessage("WhatsUp", "Test message");

            }
        }
    }
}
