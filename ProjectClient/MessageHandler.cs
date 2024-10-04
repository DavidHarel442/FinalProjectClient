using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public class MessageHandler
    {
        private TcpServerCommunication session;
        public MessageHandler(TcpServerCommunication client)
        {
            session = client;
        }
        /// <summary>
        /// this function handles the messages
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TcpCommunicationProtocol message)
        {
            Console.WriteLine($"Handling message: Command={message.Command},Username={session.communicationProtocol.MyUsername} Arguments={message.Arguments}");
            switch (message.Command)
            {
                case "UsernameAccepted":
                    session.usernameSent = true;
                    MessageBox.Show(message.Arguments);
                    break;
                case "ERROR":
                    MessageBox.Show($"Error: {message.Arguments}");
                    break;
                default:
                    Console.WriteLine($"Unknown command received: {message.Command}");
                    break;
            }
        }
        /// <summary>
        /// sends encrypted username
        /// </summary>
        public void SendEncryptedUsername()
        {
            try
            {
                string encodedUsername = Convert.ToBase64String(Encoding.UTF8.GetBytes(session.communicationProtocol.MyUsername));
                session.SendMessage("USERNAME", encodedUsername);
                Console.WriteLine($"Sent encrypted username: {encodedUsername}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending encrypted username: {ex.Message}");
            }
        }
    }
}
