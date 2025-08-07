using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TheVault.Shared.Encryption
{
    public class GenerateKeysAndIvs
    {

        public void GenerateKeyBytes(byte[] key)
        {
            RandomNumberGenerator.Fill(key);
        }

        public void GenerateIVBytes(byte[] iv)
        {
            RandomNumberGenerator.Fill(iv);
        }

    }
}
