using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Util
{
    public class Criptografia_AES256
    {
        public static String doEncryptAES(String plainText)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return Base64UrlEncode(Encrypt(plainBytes, getRijndaelManaged("51n4ps#3@2")));
        }
        public static String doDecryptAES(String encryptedText)
        {
            var encryptedBytes = Base64UrlDecode(encryptedText);
            return Encoding.UTF8.GetString(Decrypt(encryptedBytes, getRijndaelManaged("51n4ps#3@2")));
        }
        private static RijndaelManaged getRijndaelManaged(String secretKey)
        {
            var keyBytes = new byte[16];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }

        // Base64 URL-safe encoding

        private static string Base64UrlEncode(byte[] input)
        {

            return Convert.ToBase64String(input)

                          .Replace("+", "-")

                          .Replace("/", "_")

                          .Replace("=", "");

        }

        // Base64 URL-safe decoding

        private static byte[] Base64UrlDecode(string input)
        {

            string padded = input.Replace("-", "+").Replace("_", "/");

            switch (padded.Length % 4)

            {

                case 2: padded += "=="; break;

                case 3: padded += "="; break;

            }

            return Convert.FromBase64String(padded);

        }


        private static byte[] Encrypt(byte[] plainBytes, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateEncryptor()
                .TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }
        private static byte[] Decrypt(byte[] encryptedData, RijndaelManaged rijndaelManaged)
        {
            return rijndaelManaged.CreateDecryptor()
                .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
        public static byte[] toByte(string hexString)
        {
            int len = hexString.Length / 2;
            byte[] result = new byte[len];
            for (int i = 0; i < len; i++)
                result[i] = Convert.ToByte(hexString.Substring(2 * i, 2 * i + 2),
                        16);
            return result;
        }
    }
}
