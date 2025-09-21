using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto
{
    public class Vizner
    {
        public static byte[] Encrypt(byte[] message)
        {
            string key = "LEMON";

            byte[] result = new byte[message.Length];
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < message.Length; i++)
            {
                result[i] = (byte)((message[i] + keyBytes[i % keyBytes.Length]) % 256);
            }

            return result;
        }

        public static byte[] Decrypt(byte[] data)
        {
            string key = "LEMON";
            byte[] result = new byte[data.Length];
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)((256 + data[i] - keyBytes[i % keyBytes.Length]) % 256);
            }

            return result;
        }


    }
}
