using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public class TcpServerCommunication
    {
        // this class is incharge of communicaiting with the server

        /// <summary>
        /// this property contains the port that the server runs on
        /// </summary>
        int portNo = 5000;
        /// <summary>
        /// this property contains the ip of the server
        /// </summary>
        private string ipAddress = "127.0.0.1";
        /// <summary>
        /// this property contains the TcpClient that the clients runs on
        /// </summary>
        TcpClient tcpClient;
        /// <summary>
        /// this property contains in the 'SendMessage' function the data that is sent to the server.
        /// and in the 'RecieveMessage' function the data that the client recieved from the server
        /// </summary>
        byte[] data;

        /// <summary>
        /// this property contains the object that is used to communicate with the server
        /// </summary>
        public TcpCommunicationProtocol communicationProtocol = null;
        /// <summary>
        /// this property is to check if the communication is encrypted with keys transfered and communication encryption with RSA and then AES
        /// </summary>
        public bool isInitialConnectionComplete = false;
        /// <summary>
        /// this property is to check where this client is connected  to server
        /// </summary>
        public bool usernameSent = false;
        /// <summary>
        /// this property is an object that will handle all messages
        /// </summary>
        private MessageHandler messageHandler;
        /// <summary>
        /// constructor. gives the property 'communicationProtocol' the same spot in the memory as the ManageHathatulClient communicationProtocol
        /// </summary>
        /// <param name="communicationProtocol"></param>
        public TcpServerCommunication()
        {
            this.communicationProtocol = new TcpCommunicationProtocol();
            messageHandler = new MessageHandler(this);
        }
        /// <summary>
        /// this function creates the connection with the server. Connects the client to the server
        /// </summary>
        /// <param name="nickname"></param>
        public void ConnectToServer()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ipAddress, portNo);
                data = new byte[tcpClient.ReceiveBufferSize];

                string publicKey = communicationProtocol.GetPublicKey();
                string initialMessage = $"INIT\n{publicKey}\r";
                byte[] initialData = Encoding.UTF8.GetBytes(initialMessage);
                tcpClient.GetStream().Write(initialData, 0, initialData.Length);

                tcpClient.GetStream().BeginRead(data, 0, System.Convert.ToInt32(tcpClient.ReceiveBufferSize), ReceiveMessage, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }


        /// <summary>
        /// this funcion is used when the player send a message. it converts the string into bytes and sends it using the Tcp Protocol
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string command, string arguments)
        {
            try
            {
                NetworkStream ns = tcpClient.GetStream();
                byte[] data;

                if (!isInitialConnectionComplete)
                {
                    string message = $"{command}\n{arguments}\r";
                    data = Encoding.UTF8.GetBytes(message);
                }
                else
                {
                    string encryptedMessage = communicationProtocol.ToProtocol(command, arguments);
                    data = Encoding.UTF8.GetBytes(encryptedMessage);
                }

                ns.Write(data, 0, data.Length);
                ns.Flush();

                Console.WriteLine($"Sent: Command={command},Username={TcpCommunicationProtocol.myUsername}, Arguments={arguments}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        /// <summary>
        /// this function is an Async function. it runs and reads messages. it transfers from bytes to string.
        /// and the messages it recieves are from the server
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                int bytesRead = tcpClient.GetStream().EndRead(ar);
                if (bytesRead < 1)
                {
                    return;
                }

                string textFromServer = Encoding.UTF8.GetString(data, 0, bytesRead);

                if (!isInitialConnectionComplete)
                {
                    HandleInitialConnection(textFromServer);
                }
                else
                {
                    List<TcpCommunicationProtocol> messages = communicationProtocol.FromProtocol(textFromServer);
                    foreach (TcpCommunicationProtocol message in messages)
                    {
                        HandleMessage(message);
                    }
                }

                tcpClient.GetStream().BeginRead(data, 0, System.Convert.ToInt32(tcpClient.ReceiveBufferSize), ReceiveMessage, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveMessage: {ex.Message}");
                // Handle disconnection or other errors
            }
        }
        /// <summary>
        /// this function handles the received public AES key 
        /// </summary>
        /// <param name="encryptedAesKey"></param>
        private void HandleInitialConnection(string message)
        {
            string[] parts = message.Split('\n');
            if (parts.Length >= 2 && parts[0] == "AES_KEY")
            {
                string encryptedAesKey = parts[1].TrimEnd('\r');
                communicationProtocol.SetAesKey(encryptedAesKey);
                isInitialConnectionComplete = true;
            }
            else if (parts.Length >= 2 && parts[0] == "ERROR")
            {
                Console.WriteLine($"Error from server: {parts[1]}");
                // Handle the error appropriately
            }
            else
            {
                Console.WriteLine("Invalid initial response from server");
                // Handle invalid response
            }
        }
        /// <summary>
        /// this function is responsible for calling the function that will handle the acceptence of messages
        /// </summary>
        /// <param name="message"></param>
        private void HandleMessage(TcpCommunicationProtocol message)
        {
            messageHandler.HandleMessage(message);
        }
        /// <summary>
        /// this function is responsible for calling the function that will send username
        /// </summary>
        public void HandleUsernameMessage(string username)
        {
            communicationProtocol.MyUsername = username;
            messageHandler.SendEncryptedUsername();
        }



        /// <summary>
        /// this function disconnects the link between the client and the server. closes the stream
        /// </summary>
        public void Disconnect()
        {
            if (!usernameSent)
            {
                Console.WriteLine("Not connected to server.");
                return;
            }

            try
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnect: {ex.Message}");
            }
            finally
            {
                usernameSent = false;
                isInitialConnectionComplete = false;
                Console.WriteLine("Disconnected from server");
            }
        }

    }
}
