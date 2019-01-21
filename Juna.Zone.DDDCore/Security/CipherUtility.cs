using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Juna.DDDCore.Security
{
    public class CipherUtility
    {
        private static string Encrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            string salt = RandomKey();
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm algo = new T();

            byte[] rgbKey = rgb.GetBytes(algo.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algo.BlockSize >> 3);

            ICryptoTransform transform = algo.CreateEncryptor(rgbKey, rgbIV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream ct = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter st = new StreamWriter(ct, Encoding.Unicode))
                    {
                        st.Write(value);
                    }
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private static string Decrypt<T>(string text, string password) where T : SymmetricAlgorithm, new()
        {
            string salt = RandomKey();
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm algo = new T();

            byte[] rgbKey = rgb.GetBytes(algo.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algo.BlockSize >> 3);

            ICryptoTransform transform = algo.CreateDecryptor(rgbKey, rgbIV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream ct = new CryptoStream(ms, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(ct, Encoding.Unicode))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        private static string RandomKey()
        {
            byte[] key256 = new byte[32];
            for (int i = 0; i < 32; i++)
                key256[i] = Convert.ToByte(i % 256);

            return key256.ToString();
        }

        public static string Encrypt(string text)
        {
            string key = CipherUtility.RandomKey();

            return CipherUtility.Encrypt<AesManaged>(text, key);
        }

        public static string Decrypt(string text)
        {
            string key = CipherUtility.RandomKey();

            return CipherUtility.Decrypt<AesManaged>(text, key);
        }

        public static string Encrypt(string text, string key)
        {
            return CipherUtility.Encrypt<AesManaged>(text, key);
        }

        public static string Decrypt(string text, string key)
        {
            return CipherUtility.Decrypt<AesManaged>(text, key);
        }

        public static string ConvertStringToHex(String input)
        {
            System.Text.Encoding encoding = System.Text.Encoding.Unicode;
            Byte[] stringBytes = encoding.GetBytes(input);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            return sbBytes.ToString();
        }

        public static string ConvertHexToString(String hexInput)
        {
            System.Text.Encoding encoding = System.Text.Encoding.Unicode;
            int numberChars = hexInput.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexInput.Substring(i, 2), 16);
            }
            return encoding.GetString(bytes);
        }

        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
