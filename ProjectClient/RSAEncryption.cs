using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class RSAEncryption
    {// taken from claude
    // this class is responsible for the RSA encyption proccess. 

        /// <summary>
        /// property responsible for the RSA encryption and decryption
        /// </summary>
        private RSACryptoServiceProvider rsa;

        /// <summary>
        /// Initializes a new instance of the RSAEncryption class with the specified key size.
        /// key size is the size of the RSA key in bits
        /// </summary>
        /// <param name="keySize"></param>
        public RSAEncryption(int keySize = 2048)
        {
            rsa = new RSACryptoServiceProvider(keySize);
        }

        /// <summary>
        /// Retrieves the public key in XML format,
        /// to sending to encryption partners.
        /// </summary>
        /// <returns></returns>
        public string GetPublicKey()
        {
            return rsa.ToXmlString(false);
        }
        /// <summary>
        /// Decrypts the provided encrypted data using the private key.
        /// </summary> uses OAEP padding to enchance security
        /// <param name="dataToDecrypt"></param>
        /// <returns></returns>
        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            return rsa.Decrypt(dataToDecrypt, true);
        }
    }
}
