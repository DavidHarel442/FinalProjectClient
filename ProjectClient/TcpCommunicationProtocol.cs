using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class TcpCommunicationProtocol
    {
        private string command;
        public static string myUsername;
        private string arguments;
        private EncryptionManager encryptionManager;

        public TcpCommunicationProtocol()
        {
            encryptionManager = new EncryptionManager();
        }

        public string MyUsername { get => myUsername; set => myUsername = value; }
        public string Command { get => command; set => command = value; }
        public string Arguments { get => arguments; set => arguments = value; }

        public string ToProtocol(string command, string arguments)
        {
            string message = $"{command}\n{myUsername}\n{arguments}";
            return encryptionManager.EncryptMessage(message) + '\r';
        }

        public List<TcpCommunicationProtocol> FromProtocol(string text)
        {
            List<TcpCommunicationProtocol> messages = new List<TcpCommunicationProtocol>();
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
                            TcpCommunicationProtocol protocol = new TcpCommunicationProtocol
                            {
                                Command = parts[0],
                                Arguments = parts.Length >= 3 ? string.Join("\n", parts.Skip(2)) : parts[1]
                            };
                            messages.Add(protocol);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting message: {ex.Message}");
                        // Handle unencrypted messages (e.g., initial connection messages)
                        string[] parts = encryptedMessage.Split('\n');
                        if (parts.Length >= 2)
                        {
                            TcpCommunicationProtocol protocol = new TcpCommunicationProtocol
                            {
                                Command = parts[0],
                                Arguments = string.Join("\n", parts.Skip(1))
                            };
                            messages.Add(protocol);
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

        public void SetClientPublicKey(string publicKey)
        {
            encryptionManager.SetClientPublicKey(publicKey);
        }

        public string GetEncryptedAesKey()
        {
            return encryptionManager.GetEncryptedAesKey();
        }

        public void SetAesKey(string encryptedAesKey)
        {
            encryptionManager.SetAesKey(encryptedAesKey);
        }
        public string EncryptMessage(string message)
        {
            return encryptionManager.EncryptMessage(message);
        }

        public string DecryptMessage(string encryptedMessage)
        {
            return encryptionManager.DecryptMessage(encryptedMessage);
        }
    }
}

