using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using System.IO;
using System.Xml;
using System.IdentityModel.Tokens;
using System.Collections.Specialized;
using System.Security.Cryptography.Xml;
using System.Configuration;

namespace SiemensISRM
{

    public class SIBLog
    {
        public static void Write(string t, params string[] p)
        {
            string loglevel = ConfigurationManager.AppSettings["SIBLogfileLevel"].ToLower();
            if (loglevel == "information" && t.ToLower() == "debug") return;
            if (loglevel == "error" && t.ToLower() != "error") return;                
            try
            {
                string fileName = ConfigurationManager.AppSettings["SIBLogfilepath"].ToString();

                using (StreamWriter w = File.AppendText(fileName))
                {
                    w.Write("\"{0}\", \"{1}\"", DateTime.Now.ToString(), t);
                    for (int i = 0; i < p.Length; i++) w.Write(", \"{0}\"", p[i]);
                    w.WriteLine("");
                }
            }
            catch { }
        }
    }

    public class SIBData
    {
        public static NameValueCollection inParams = new NameValueCollection();
        public static NameValueCollection outParams = new NameValueCollection();
        public static string redirectGETRequest;
        public static string redirectPOSTRequest;
    }

    public class SIBTransform
    {
        public static NameValueCollection BuildNVC(string p, char b1, char b2)
        {
            NameValueCollection n = new NameValueCollection();
            if (!String.IsNullOrEmpty(p))
            {
                string[] a = p.Split(b1);
                string[] nv;
                foreach (string s in a)
                {
                    nv = s.Split(b2);
                    n.Add(nv[0], nv[1]);
                }
            }
            return n;
        }
        public static bool IsPostRequest()
        {
            if (ConfigurationManager.AppSettings["SIBTargetRequestType"].ToUpper() == "POST") return true;
            return false;
        }
        public static void BuildResponse()
        {
            string[] encryptionParameters = ConfigurationManager.AppSettings["SIBTargetHashParameters"].Split(';');
            string plaintext = String.Empty;
            string ciphertext = String.Empty;
            string timestamp;

            // Populate timestamp parameter in preparation for use
            if (ConfigurationManager.AppSettings["SIBTargetTimestampUTC"].ToLower() == "true") timestamp = SIBTime.UTCTimeStamp();
            else timestamp = SIBTime.TimeStamp();
            SIBLog.Write("Debug", "Timestamp", timestamp);

            // acquire encryption parameters identified in config file and build a cleartext string
            foreach (string parm in encryptionParameters)
            {
                switch(parm)
                {
                    case "SIBTargetCipherKey":
                        plaintext += ConfigurationManager.AppSettings["SIBTargetCipherKey"];
                        break;
                    case "SIBTargetTimestamp":
                        plaintext += timestamp;
                        break;
                    default:
                        plaintext += SIBData.inParams[parm];
                        break;
                }
            }
            SIBLog.Write("Debug", "Plaintext", plaintext);

            // Encrypt the required parameters with appropirate cipher
            switch (ConfigurationManager.AppSettings["SIBTargetCipher"].ToUpper())
            {
                case "MD5":
                    if (ConfigurationManager.AppSettings["SIBTargetCipherEncoding"].ToUpper() == "HEX") ciphertext = SIBCrypto.HashMD5_HEX(plaintext);
                    else if (ConfigurationManager.AppSettings["SIBTargetCipherEncoding"].ToUpper() == "BASE64") ciphertext = SIBCrypto.HashMD5_64(plaintext);
                    break;
                default:
                    ciphertext = plaintext;
                    break;
            }
            SIBLog.Write("Debug", "Ciphertext", ciphertext);

            // Replace values from source to target in parameter list
            SIBData.outParams.Clear();
            SIBData.outParams.Add(BuildNVC(ConfigurationManager.AppSettings["SIBTargetParameters"], ';', '='));
            foreach (string key in SIBData.outParams.AllKeys)
            {
                switch (SIBData.outParams[key])
                {
                    case "SIBTargetHashParameters":
                        SIBData.outParams[key] = ciphertext;
                        break;
                    case "SIBTargetTimestamp":
                        SIBData.outParams[key] = timestamp;
                        break;
                    default:
                        SIBData.outParams[key] = SIBData.inParams[SIBData.outParams[key]];
                        break;
                }
            }

            //Build the redirect URL
            SIBData.redirectGETRequest = ConfigurationManager.AppSettings["SIBTargetRedirectURL"] + "?";
            for (int i=0; i<SIBData.outParams.Count;i++)
            {
                SIBData.redirectGETRequest += String.Format("{0}={1}", SIBData.outParams.GetKey(i), SIBData.outParams[i]);
                if (i < SIBData.outParams.Count-1) SIBData.redirectGETRequest += "&";
            }
            SIBLog.Write("Debug", "Get Request", SIBData.redirectGETRequest);

            //Build the response/redirect HTLM document
            SIBData.redirectPOSTRequest = "<html>";
            SIBData.redirectPOSTRequest += @"<body onload='document.forms[""form""].submit()'>";
            SIBData.redirectPOSTRequest += String.Format("<form name='form' action='{0}' method='post'>", ConfigurationManager.AppSettings["SIBTargetRedirectURL"]);
            foreach (string key in SIBData.outParams.AllKeys)
            {
                SIBData.redirectPOSTRequest += String.Format("<input type='hidden' name='{0}' value='{1}'>", key, SIBData.outParams[key]);
            }
            SIBData.redirectPOSTRequest += "</form></body></html>";
            SIBLog.Write("Debug", "Post Request", SIBData.redirectPOSTRequest);
        }

        public static void ParseRequest(HttpRequest request)
        {
            SIBLog.Write("Debug", "HTTPRequest GET.", request.QueryString.ToString());
            SIBData.inParams.Clear();
            foreach (string key in request.QueryString.AllKeys)
            {
                SIBData.inParams.Set(key, request.QueryString[key]);
            }

            SIBLog.Write("Debug", "HTTPRequest POST.", request.Form.ToString());
            foreach (string key in request.Form.AllKeys)
            {
                SIBData.inParams.Set(key, request.Form[key]);
            }

            if (!String.IsNullOrEmpty(SIBData.inParams["SAMLResponse"])) 
            {
                SIBLog.Write("Information", "SAMLResponse detected.");
                try
                {
                    XmlDocument xmlSAML = new XmlDocument();
                    string decodedSAML = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(SIBData.inParams["SAMLResponse"]));
                    SIBLog.Write("Debug", "Decoded SAML", decodedSAML);
                    xmlSAML.LoadXml(decodedSAML);
                    if (IsValidSignature(xmlSAML)) 
                    {
                        SIBLog.Write("Information", "SAML Signature is valid.");
                        XmlNodeList nodeList = xmlSAML.GetElementsByTagName(ConfigurationManager.AppSettings["SIBSourceSAMLAttributeElement"].ToString());

                        for (int i = 0; i < nodeList.Count; i++) 
                        {
                            SIBData.inParams.Set(nodeList.Item(i).Attributes.Item(0).Value, nodeList.Item(i).InnerText);
                        }
                    }
                    else SIBLog.Write("Error", "SAML Signature is invalid.");
                }
                catch 
                { 
                    SIBLog.Write("Error", "No or improperly formated SAMLResponse value presented");
                }
            }

            SIBLog.Write("Debug", "Parameters consumed from target post processing...");
            for (int i = 0; i < SIBData.inParams.Count; i++) SIBLog.Write("Debug", SIBData.inParams.GetKey(i), SIBData.inParams[i]);

            // Override static configuration values set by implementation
            NameValueCollection n = new NameValueCollection();
            n.Add(BuildNVC(ConfigurationManager.AppSettings["SIBSourceParametersOverride"], ';', '='));
            foreach (string key in n.AllKeys)
            {
                SIBData.inParams.Set(key, n[key]);
            }

            SIBLog.Write("Debug", "Parameters post override processing...");
            for (int i = 0; i < SIBData.inParams.Count; i++) SIBLog.Write("Debug", SIBData.inParams.GetKey(i), SIBData.inParams[i]);

        }

        private static bool IsValidSignature(XmlDocument xmlDoc)
        {
            SignedXml signedXml = new SignedXml(xmlDoc);
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName(ConfigurationManager.AppSettings["SIBSourceSAMLSignatureElement"].ToString());

            if (nodeList != null && nodeList.Count > 0)
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
                return signedXml.CheckSignature();
            }
            return false;
        }
    }
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
        public static string BytestoHex(byte[] b)
        {
            string h = string.Empty;

            for (int i=0; i<b.Length;i++)
            {
                h += b[i].ToString("X2");
            }
            return h;
        }
        public static string HashMD5_HEX(string plainText)
        {
            return SIBCrypto.BytestoHex(HashMD5(Encoding.UTF8.GetBytes(plainText)));
        }
        public static string HashMD5_64(string plainText)
        {
            return HashMD5_64(Encoding.UTF8.GetBytes(plainText));
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