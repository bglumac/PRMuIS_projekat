using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto
{
    public class Vizner
    {
        public static string Encrypt(string message)
        {
            string key = "LEMON";

            message = message.ToUpper();
            key = key.ToUpper();

            int messLen = message.Length;
            int kljucLen = key.Length;

            char[] encrypted = new char[messLen];

            string extendedKey = "";
            for (int i = 0; i < messLen; i++)
                extendedKey += key[i % kljucLen];

            for (int i = 0; i < messLen; i++)
            {
                int m = message[i] - 'A';
                int k = extendedKey[i] - 'A';
                encrypted[i] = (char)((m + k) % 26 + 'A');
            }

            return new string(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            string key = "LEMON";

            cipherText = cipherText.ToUpper();
            key = key.ToUpper();

            int textLen = cipherText.Length;
            int kljucLen = key.Length;

            char[] decrypted = new char[textLen];

            // produženi ključ
            string extendedKey = "";
            for (int i = 0; i < textLen; i++)
                extendedKey += key[i % kljucLen];

            for (int i = 0; i < textLen; i++)
            {
                int c = cipherText[i] - 'A';
                int k = extendedKey[i] - 'A';
                decrypted[i] = (char)((c - k + 26) % 26 + 'A'); 
            }

            return new string(decrypted);
        }


    }
}
