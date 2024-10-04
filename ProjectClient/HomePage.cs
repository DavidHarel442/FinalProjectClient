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
        /// defualt constructor which Initialize the form
        /// </summary>
        public HomePage(TcpServerCommunication client)
        {
            InitializeComponent();
            tcpServer = client;
        }
    }
}
