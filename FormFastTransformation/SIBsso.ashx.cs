using System;
using System.Web;
using SiemensISRM;
using System.Configuration;

namespace sibsso
{
    /// <summary>
    /// sibsso supports SIB transformation using Web.config parameters
    /// </summary>
    public class sibsso : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Processing Request...");
            SIBConsume.ParseRequest(context.Request);
            context.Response.Redirect(Transformation()); 
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        // Transformation function for SSO/SIB Interop
        private static string Transformation()
        {
            string[] encryptionParameters = ConfigurationManager.AppSettings["SIBTargetHashParameters"].Split(';');
            string plaintext;
            string ciphertext;

            // acquire encryption parameters identified in config file and build a cleartext string
            foreach (string parm in encryptionParameters)
            {
                switch(parm)
                {
                    case "SIBTargetCipherKey":
                        plaintext = plaintext + ConfigurationManager.AppSettings["SIBTargetCipherKey"];
                        break;
                    case "SIBTargetTimestamp":
                        string tempTimestamp;
                        if (ConfigurationManager.AppSettings["SIBTargetTimestampUTC"].ToLower() == "true") tempTimestamp = SIBTime.UTCTimeStamp();
                        else tempTimestamp = SIBTime.TimeStamp();
                        plaintext = plaintext + tempTimestamp;
                        break;
                    default:
                        plaintext = plaintext + SIBData.parameters[parm];
                }
            }

            // Encrypt the required parameters with appropirate cipher
            switch (ConfigurationManager.AppSettings["SIBTargetCipher"].ToUpper())
            {
                case "MD5":
                    if (ConfigurationManager.AppSettings["SIBTargetCipherEncoding"].ToUpper() == "HEX") ciphertext = SIBCrypto.HashMD5_HEX(plaintext);
                    else if (ConfigurationManager.AppSettings["SIBTargetCipherEncoding"].ToUpper() == "BASE64") ciphertext = SIBCrypto.HashMD5_64(plaintext);
                    break;
                default:
                    ciphertext = plaintext;
            }

            // Replace values from source to target in parameter list



            try
            {
                byte[] IV = SIBCrypto.GenerateAESIV();
                byte[] cipherText = SIBCrypto.EncryptAES(qs, SIBCrypto.HashSHA256(key), IV);
                byte[] md5 = SIBCrypto.HashMD5(SIBCrypto.Concat(cipherText, IV));
                return ConfigurationManager.AppSettings["ECWOutboundRedirectURL"].ToString() + "?args1=" + SIBCrypto.URLEncode(Convert.ToBase64String(cipherText)) + "&args2=" + SIBCrypto.URLEncode(Convert.ToBase64String(IV)) + "&args3=" + SIBCrypto.URLEncode(Convert.ToBase64String(md5));
            }
            catch
            {
                SIBLog.Write("Error", "Transformation failure.");
                return "Error: Transformation failure.";
            }
        }
    }
}