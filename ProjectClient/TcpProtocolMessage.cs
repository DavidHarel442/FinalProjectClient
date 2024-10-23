using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ProjectClient
{
    public class TcpProtocolMessage
    {//this class contains the fields for the messages received through the Tcp Connection
        /// <summary>
        /// property that contains the command received
        /// </summary>
        private string command;
        /// <summary>
        /// static property that contains this users username
        /// </summary>
        public static string myUsername;
        /// <summary>
        /// all the required variables that will be used to fulfill the command
        /// </summary>
        private string arguments;
        /// <summary>
        /// getter and setter for the command property
        /// </summary>
        public string Command { get => command; set => command = value; }
        /// <summary>
        /// getter and setter for the argument property
        /// </summary>
        public string Arguments { get => arguments; set => arguments = value; }
    }
}
