using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TheVault.Shared.Encryption
{
    public class Encryption
    {

        /// <summary>
        /// Encrypts the provided data using AES encryption with the given key and IV.
        /// </summary>
        /// <param name="datatoencrypt">The data to encrypt as a byte array.</param>
        /// <param name="key">The encryption key as a byte array.</param>
        /// <param name="iv">The initialization vector as a byte array.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        /// 
        public static byte[] Encrypt(byte[] datatoencrypt, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create(); // Create an instance of the AES algorithm

            aes.Key = key; // Set the encryption key
            aes.IV = iv; // Set the initialization vector

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV); // Create an encryptor
            return PerformCryptography(datatoencrypt, encryptor); // Perform the encryption
        }

        /// <summary>
        /// Decrypts the provided data using AES decryption with the given key and IV.
        /// </summary>
        /// <param name="datatodecrypt">The data to decrypt as a byte array.</param>
        /// <param name="key">The decryption key as a byte array.</param>
        /// <param name="iv">The initialization vector as a byte array.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        public static byte[] Decrypt(byte[] datatodecrypt, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create(); // Create an instance of the AES algorithm
            aes.Key = key; // Set the decryption key
            aes.IV = iv; // Set the initialization vector

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            // Create a decryptor
            return PerformCryptography(datatodecrypt, decryptor); // Perform the decryption
        }

        /// <summary>
        /// Performs cryptographic transformation (encryption or decryption) on the provided data.
        /// </summary>
        /// <param name="data">The data to transform as a byte array.</param>
        /// <param name="cryptoTransform">The cryptographic transform to use (encryptor or decryptor).</param>
        /// <returns>The transformed data as a byte array.</returns>
        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using var ms = new MemoryStream(); // Create a memory stream to hold the transformed data
            using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write); // Create a crypto stream to perform the transformation
            cryptoStream.Write(data, 0, data.Length); // Write the data to the crypto stream
            cryptoStream.FlushFinalBlock(); // Updates the data source with current buffer, then clears buffer
            return ms.ToArray(); // Return the transformed data as a byte array
        }
    }   
}