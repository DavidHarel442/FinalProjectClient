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
        public HomePage homePage;
        private TcpServerCommunication session;


        /// <summary>
        /// after a successful login/register to close the form we will use an event. 
        /// </summary>
        public event EventHandler<string> LoginSuccess;
        public event EventHandler<string> RegistrationSuccess;

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
            Console.WriteLine($"Handling message: Command={message.Command},Arguments={message.Arguments}");
            switch (message.Command)
            {
                case "UsernameAccepted":
                    session.usernameSent = true;
                    break;
                case "Registered":
                    session.firstName = message.Arguments;
                    MessageBox.Show("Registered Succesfully");
                    RegistrationSuccess?.Invoke(this, message.Arguments);
                    break;
                case "LoggedIn":
                    session.firstName = message.Arguments;
                    MessageBox.Show("Logged in Succesfully");
                    LoginSuccess?.Invoke(this, message.Arguments);
                    break;
                case "success":
                    MessageBox.Show($"Success: {message.Arguments}");
                    break;
                case "Issue":
                    MessageBox.Show($"Issue: {message.Arguments}");
                    break;
                case "ERROR":
                    Console.WriteLine($"Error: {message.Arguments}");
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



        
       

        //private void SafeShowHomePage()
        //{
        //    if (CloseCurrentForm.Target is Form form)
        //    {
        //        form.Invoke((MethodInvoker)delegate {
        //            homePage = new HomePage();
        //            homePage.ShowDialog();
        //        });
        //    }
        //}
    }
}
