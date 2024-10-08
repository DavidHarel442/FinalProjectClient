using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class TcpCommunicationProtocol
    {
        
        private EncryptionManager encryptionManager;

        public TcpCommunicationProtocol()
        {
            encryptionManager = new EncryptionManager();
        }

        

        public string ToProtocol(string command, string arguments)
        {
            string message = $"{command}\n{TcpProtocolMessage.myUsername}\n{arguments}";
            return encryptionManager.EncryptMessage(message) + '\r';
        }

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

        public string GetPublicKey()
        {
            return encryptionManager.GetPublicKey();
        }

        public void SetAesKey(string encryptedAesKey)
        {
            encryptionManager.SetAesKey(encryptedAesKey);
        }
    }
}

