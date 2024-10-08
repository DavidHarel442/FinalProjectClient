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
        private string command;
        public static string myUsername;
        private string arguments;
        public string Command { get => command; set => command = value; }
        public string Arguments { get => arguments; set => arguments = value; }
    }
}
