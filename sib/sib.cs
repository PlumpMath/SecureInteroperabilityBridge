using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using System.IO;


namespace SiemensISRM
{
    public class SIBTime
    {
        public static string UTCTimeStamp()
        {
            return TimeStamp(true);
        }
        public static string TimeStamp()
        {
            return TimeStamp(false);
        }
        public static string TimeStamp(bool utc)
        {
            DateTime t = DateTime.Now;
            if (utc) t = t.ToUniversalTime();
            return TimeStamp(t);
        }
        public static string TimeStamp(string t)
        {
            return TimeStamp(DateTime.Parse(t));
        }
        public static string TimeStamp(DateTime t)
        {
            return t.Year.ToString("D4") + t.Month.ToString("D2") + t.Day.ToString("D2") + t.Hour.ToString("D2") + t.Minute.ToString("D2") + t.Second.ToString("D2");
        }
    }
    
    public class SIBCrypto
    {
        public static byte [] Concat(byte [] a, byte [] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);
            return c;
        }

        public static string URLEncode(string u)
        {
            return HttpUtility.UrlEncode(u);
        }
        public static string HashMD5_64(byte [] plainText)
        {
            return Convert.ToBase64String(HashMD5(plainText));
        }
        public static byte[] HashMD5(byte [] plainText)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(plainText);
        }
        public static byte[] HashSHA256(string plainText)
        {
            SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        }

        public static byte[] GenerateAESIV()
        {
            AesManaged a = new AesManaged();
            return a.IV;
        }

        public static byte[] EncryptAES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object 
            // with the specified key and IV. 
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream. 
            return encrypted;
        }

        public static string DecryptAES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold 
            // the decrypted text. 
            string plainText = null;

            // Create an AesManaged object 
            // with the specified key and IV. 
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plainText;
        }
    }
}
