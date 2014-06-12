using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Web;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                string original = "mrn=123456789&source=Tufts&tstamp=20120201101149&userid=William";
                string key = "SL94lATiyuaCo4sWZg1ECz5xMTD78yR9";
                string encrypted = EncryptECW256AES(original, key);
                string decrypted = DecryptECW256AES(encrypted, key);
                Console.WriteLine("Plain Text: {0}", original);
                Console.WriteLine("Encrypted:  {0}", encrypted);
                Console.WriteLine("Round Trip: {0}", decrypted);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }

        // Decrypt wrapper function for ASP.NET ECW Interop
        // qs is the unparsed query string recieved from the calling application with the args1, args2, and args3 key/value pairs
        // key is the secret key value, which is converted to a 256bit SHA hash (to ensure a 256 bit encryption key for AES)
        static string DecryptECW256AES(string qs, string key)
        {
            if (qs == null) return "error: no query string";

            NameValueCollection qsc = HttpUtility.ParseQueryString(qs);
            string cipherText = qsc.Get("args1");
            string IV = qsc.Get("args2");
            string md5 = qsc.Get("args3");
            string md5test = HashMD5_64(cipherText + IV);
            if (md5 != md5test) return "error: intregity check failed.";

            return DecryptAES(Convert.FromBase64String(cipherText), HashSHA256(key), Convert.FromBase64String(IV));
        }

        // Encrypt wrapper function for ASP.NET ECW Interop
        // qa is the plaintext unparsed query string that is expected to be passed to the receiving application
        // key is the secret key value, which is converted to a 256bit SHA hash (to ensure a 256 bit encryption key for AES)
        static string EncryptECW256AES(string qs, string key)
        {
            if (qs == null) return "error: no query string";

            AesManaged aesAlg = new AesManaged();
            aesAlg.Key = HashSHA256(key);
            string cipherText = Convert.ToBase64String(EncryptAES(qs, aesAlg.Key, aesAlg.IV));
            string IV = Convert.ToBase64String(aesAlg.IV);
            string md5 = HashMD5_64(cipherText + IV);

            return "args1=" + HttpUtility.UrlEncode(cipherText) + "&args2=" + HttpUtility.UrlEncode(IV) + "&args3=" + HttpUtility.UrlEncode(md5);
        }

        static string HashMD5_64(string plainText)
        {
            return Convert.ToBase64String(HashMD5(plainText));
        }
        static byte[] HashMD5(string plainText)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        }
        static byte[] HashSHA256(string plainText)
        {
            SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
        }
        static byte[] EncryptAES(string plainText, byte[] Key, byte[] IV)
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

        static string DecryptAES(byte[] cipherText, byte[] Key, byte[] IV)
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
