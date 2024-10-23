using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class TcpCommunicationProtocol
    {// class incharge of the communication protocol 
        /// <summary>
        /// this property is incharge of managing the encryption, both RSA and AES
        /// </summary>
        private EncryptionManager encryptionManager;
        /// <summary>
        /// constructor, initializes encryptionManager
        /// </summary>
        public TcpCommunicationProtocol()
        {
            encryptionManager = new EncryptionManager();
        }

        
        /// <summary>
        /// this function receives a command and argument and transfers it to the protocol format. 
        /// puts it all in one string with the username and seperates it with '\n'.
        /// in the protocol there is command,username,arguments
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public string ToProtocol(string command, string arguments)
        {
            string message = $"{command}\n{TcpProtocolMessage.myUsername}\n{arguments}";
            return encryptionManager.EncryptMessage(message) + '\r';
        }
        /// <summary>
        /// this function receives a string that is in the protocol format and transfers it back to a TcpProtocolMessage object that contains:
        /// a command and arguments, the username is not neccessary because the client knows his own username.
        /// in case that messages got mixed up and sent together there is a '\r' at the end of each message,
        /// and if checking after '\r' there is more it creates more then one object,
        /// thats why there is a list of the 'TcpProtocolMessage'
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<TcpProtocolMessage> FromProtocol(string text)
        {
            List<TcpProtocolMessage> messages = new List<TcpProtocolMessage>();
            string[] encryptedMessages = text.Split('\r');
            foreach (string encryptedMessage in encryptedMessages)
            {
                if (!string.IsNullOrEmpty(encryptedMessage))
                {
                    try
                    {
                        string decryptedMessage = encryptionManager.DecryptMessage(encryptedMessage);
                        string[] parts = decryptedMessage.Split('\n');
                        if (parts.Length >= 2)
                        {
                            TcpProtocolMessage message = new TcpProtocolMessage
                            {
                                Command = parts[0],
                                Arguments = parts.Length >= 2 ? string.Join("\n", parts.Skip(1)) : parts[0]
                            };
                            messages.Add(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting message: {ex.Message}");
                        // Handle unencrypted messages (e.g., initial connection messages)
                        string[] parts = encryptedMessage.Split('\n');
                        if (parts.Length >= 1)
                        {
                            TcpProtocolMessage message = new TcpProtocolMessage
                            {
                                Command = parts[0],
                                Arguments = string.Join("\n", parts.Skip(0))
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            return messages;
        }

        /// <summary>
        /// this function returns the RSA public key
        /// </summary>
        /// <returns></returns>
        public string GetPublicKey()
        {
            return encryptionManager.GetPublicKey();
        }
        /// <summary>
        /// this function sets the AES key.
        /// </summary>
        /// <param name="encryptedAesKey"></param>
        public void SetAesKey(string encryptedAesKey)
        {
            encryptionManager.SetAesKey(encryptedAesKey);
        }
    }
}

