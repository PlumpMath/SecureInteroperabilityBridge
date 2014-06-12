using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.IdentityModel.Tokens;


namespace SAMLTest
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string sSAMLRawResponse = "PHNhbWxwOlJlc3BvbnNlIHhtbG5zOnNhbWxwPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6cHJvdG9jb2wiIElzc3VlSW5zdGFudD0iMjAxNC0wNS0wNlQxMzozNzoxNC41MDVaIiBJRD0iXzM5Yzc1NDI5LTZlYzAtNDc2MS04OTVmLTg2ZDFmMDFhOWY2MyIgRGVzdGluYXRpb249Imh0dHA6Ly93d3cuZ29vZ2xlLmNvbT9Vc2VyTmFtZT13Ym9nZ3MmYW1wO1Zpc2l0SUQ9MSZhbXA7RXh0ZXJuYWxQYXRpZW50SUQ9ODAwNDY4IiBWZXJzaW9uPSIyLjAiPjxzYW1scDpTdGF0dXM%2BPHNhbWxwOlN0YXR1c0NvZGUgVmFsdWU9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDpzdGF0dXM6U3VjY2VzcyI%2BPC9zYW1scDpTdGF0dXNDb2RlPjwvc2FtbHA6U3RhdHVzPjxzYW1sMjpBc3NlcnRpb24geG1sbnM6c2FtbDI9InVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphc3NlcnRpb24iIFZlcnNpb249IjIuMCIgSUQ9Il9CNEYwMTc1NDk3NkZGNDVFOTUxMzk5MzgzNDQ0Mzg0IiBJc3N1ZUluc3RhbnQ9IjIwMTQtMDUtMDZUMTM6Mzc6MTQuNTUyWiI%2BPHNhbWwyOklzc3Vlcj5DTj1zb2FyaWFuc3lzdGVtY2VydGlmaWNhdGVnMG13PC9zYW1sMjpJc3N1ZXI%2BPGRzOlNpZ25hdHVyZSB4bWxuczpkcz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC8wOS94bWxkc2lnIyI%2BPGRzOlNpZ25lZEluZm8%2BPGRzOkNhbm9uaWNhbGl6YXRpb25NZXRob2QgQWxnb3JpdGhtPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzEwL3htbC1leGMtYzE0biMiPjwvZHM6Q2Fub25pY2FsaXphdGlvbk1ldGhvZD48ZHM6U2lnbmF0dXJlTWV0aG9kIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMC8wOS94bWxkc2lnI3JzYS1zaGExIj48L2RzOlNpZ25hdHVyZU1ldGhvZD48ZHM6UmVmZXJlbmNlIFVSST0iI19CNEYwMTc1NDk3NkZGNDVFOTUxMzk5MzgzNDQ0Mzg0Ij48ZHM6VHJhbnNmb3Jtcz48ZHM6VHJhbnNmb3JtIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMC8wOS94bWxkc2lnI2VudmVsb3BlZC1zaWduYXR1cmUiPjwvZHM6VHJhbnNmb3JtPjxkczpUcmFuc2Zvcm0gQWxnb3JpdGhtPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzEwL3htbC1leGMtYzE0biMiPjwvZHM6VHJhbnNmb3JtPjwvZHM6VHJhbnNmb3Jtcz48ZHM6RGlnZXN0TWV0aG9kIEFsZ29yaXRobT0iaHR0cDovL3d3dy53My5vcmcvMjAwMC8wOS94bWxkc2lnI3NoYTEiPjwvZHM6RGlnZXN0TWV0aG9kPjxkczpEaWdlc3RWYWx1ZT4zYlNKeVhOTXNHSTV1VlgxRlVnNzFveFdlUTg9PC9kczpEaWdlc3RWYWx1ZT48L2RzOlJlZmVyZW5jZT48L2RzOlNpZ25lZEluZm8%2BPGRzOlNpZ25hdHVyZVZhbHVlPm1xbUhucHUwWk5nTEdHSWdPUGNnc29xODBJdDNxREdSeDdnMUlDTDYyUmVqWVc4Y25JbTZTVnpmRmlBWnYyc3JaOVd6ZFpOVTVQNm9MV3FicjdkWExnZkp5T0g4WlRVbGZzZFNzMzJ0eXBKclpmQndjVUFYL3hGNmxxWitBTDQvMDgxcE5xenZVZEt6T1l2KzNOditCU3MvK2l0NXVGNnpIL1RtWU0vVkEvMD08L2RzOlNpZ25hdHVyZVZhbHVlPjxkczpLZXlJbmZvPjxkczpYNTA5RGF0YT48ZHM6WDUwOUNlcnRpZmljYXRlPk1JSUJ4VENDQVM2Z0F3SUJBZ0lFVWw2d2lUQU5CZ2txaGtpRzl3MEJBUVVGQURBbk1TVXdJd1lEVlFRREV4eFRiMkZ5YVdGdVUzbHpkR1Z0UTJWeWRHbG1hV05oZEdWSE1FMVhNQjRYRFRFek1UQXhOakUxTWpnd09Wb1hEVEl6TVRBeE5ERTFNamd3T1Zvd0p6RWxNQ01HQTFVRUF4TWNVMjloY21saGJsTjVjM1JsYlVObGNuUnBabWxqWVhSbFJ6Qk5WekNCbnpBTkJna3Foa2lHOXcwQkFRRUZBQU9CalFBd2dZa0NnWUVBMElycjllbDJNeFplR0VRRUxPMzBEdEtUTi9nQ21RUFEzSDJBdmRmcHhqaEdJbGVNODF3Zyszc3Y5bWJlVER3VjZNRlNpVitRakROK0ZpelNNVXZtQzhtY29NOGhwMGdLd0x5ejZPM3VuTm5DR1NLeEx2Zk5HM2dDbS96TjRZU1NSTDdLSlc3bm5KYnZiZ1VqdXpMaHdMaEFXNXlWd3ZsUlViSHlRaUhhUVVrQ0F3RUFBVEFOQmdrcWhraUc5dzBCQVFVRkFBT0JnUUF1M1c5b3hHNGdYYjNkaFNnMjJqdjcwK3NNR1BSRDBjOGlkVkhSbldPM05sblBQM1JWMVM4akdzYldMUXFvSEVFM0FEUFZVMUR6RE1sT1RtMHN6cVdubnJQMzQxclBDUnZzNmx3dUZscDBVaE1wTmM3ZUIvU09IenFZUDU1VkJBVHhPSlBCanNUZWM4ZnJDNHJyS291MHUxVzJiaitTeWJRK2YzZFd5Q2R5bVE9PTwvZHM6WDUwOUNlcnRpZmljYXRlPjwvZHM6WDUwOURhdGE%2BPC9kczpLZXlJbmZvPjwvZHM6U2lnbmF0dXJlPjxzYW1sMjpTdWJqZWN0PjxzYW1sMjpOYW1lSUQgRm9ybWF0PSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoxLjE6bmFtZWlkLWZvcm1hdDp1bnNwZWNpZmllZCI%2Bd2JvZ2dzPC9zYW1sMjpOYW1lSUQ%2BPHNhbWwyOlN1YmplY3RDb25maXJtYXRpb24gTWV0aG9kPSJ1cm46b2FzaXM6bmFtZXM6dGM6U0FNTDoyLjA6Y206YmVhcmVyIj48c2FtbDI6U3ViamVjdENvbmZpcm1hdGlvbkRhdGEgTm90T25PckFmdGVyPSIyMDE0LTA1LTA2VDEzOjM4OjE0LjU2N1oiIFJlY2lwaWVudD0iaHR0cDovL3d3dy5nb29nbGUuY29tP1VzZXJOYW1lPXdib2dncyZhbXA7VmlzaXRJRD0xJmFtcDtFeHRlcm5hbFBhdGllbnRJRD04MDA0NjgiPjwvc2FtbDI6U3ViamVjdENvbmZpcm1hdGlvbkRhdGE%2BPC9zYW1sMjpTdWJqZWN0Q29uZmlybWF0aW9uPjwvc2FtbDI6U3ViamVjdD48c2FtbDI6Q29uZGl0aW9ucyBOb3RCZWZvcmU9IjIwMTQtMDUtMDZUMTM6Mzc6MTQuNTUyWiIgTm90T25PckFmdGVyPSIyMDE0LTA1LTA2VDEzOjM4OjE0LjU1MloiPjxzYW1sMjpBdWRpZW5jZVJlc3RyaWN0aW9uPjxzYW1sMjpBdWRpZW5jZT5odHRwOi8vd3d3Lmdvb2dsZS5jb20%2FVXNlck5hbWU9d2JvZ2dzJmFtcDtWaXNpdElEPTEmYW1wO0V4dGVybmFsUGF0aWVudElEPTgwMDQ2ODwvc2FtbDI6QXVkaWVuY2U%2BPC9zYW1sMjpBdWRpZW5jZVJlc3RyaWN0aW9uPjwvc2FtbDI6Q29uZGl0aW9ucz48c2FtbDI6QXV0aG5TdGF0ZW1lbnQgQXV0aG5JbnN0YW50PSIyMDE0LTA1LTA2VDEzOjM3OjE0LjU2N1oiPjxzYW1sMjpBdXRobkNvbnRleHQ%2BPHNhbWwyOkF1dGhuQ29udGV4dENsYXNzUmVmPnVybjpvYXNpczpuYW1lczp0YzpTQU1MOjIuMDphYzpjbGFzc2VzOlBhc3N3b3JkPC9zYW1sMjpBdXRobkNvbnRleHRDbGFzc1JlZj48L3NhbWwyOkF1dGhuQ29udGV4dD48L3NhbWwyOkF1dGhuU3RhdGVtZW50PjxzYW1sMjpBdHRyaWJ1dGVTdGF0ZW1lbnQ%2BPHNhbWwyOkF0dHJpYnV0ZSBOYW1lPSJzZXNzaW9uLmlkLmlkZW50aWZpZXIuZ3NtIj48c2FtbDI6QXR0cmlidXRlVmFsdWU%2BZnVndVFGQnVJQlFFRmlFaUpRQUJBQUFlbFFDZWdxblA8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iRXh0ZXJuYWxQYXRpZW50SUQiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZT44MDA0Njg8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iVmlzaXRJRCI%2BPHNhbWwyOkF0dHJpYnV0ZVZhbHVlPjE8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjxzYW1sMjpBdHRyaWJ1dGUgTmFtZT0iVXNlck5hbWUiPjxzYW1sMjpBdHRyaWJ1dGVWYWx1ZT53Ym9nZ3M8L3NhbWwyOkF0dHJpYnV0ZVZhbHVlPjwvc2FtbDI6QXR0cmlidXRlPjwvc2FtbDI6QXR0cmlidXRlU3RhdGVtZW50Pjwvc2FtbDI6QXNzZXJ0aW9uPjwvc2FtbHA6UmVzcG9uc2U%2B";
                //string sSAMLRawResponse = Request["SAMLResponse"];
                WriteLog("SAMLRaw", sSAMLRawResponse); 
                
                string sSAMLResponse = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(System.Web.HttpUtility.UrlDecode(sSAMLRawResponse)));
                //string sSAMLResponse = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(sSAMLRawResponse));
                TextBox1.Text = sSAMLResponse;
                WriteLog("SAMLDecoded", sSAMLResponse);

                //Saml2SecurityToken token = null;
                //using (StringReader sr = new StringReader(sSAMLResponse))
                //{
                //    using (XmlReader xr = XmlReader.Create(sr))
                //    {
                //        xr.ReadToFollowing("Assertion", "urn:oasis:names:tc:SAML:2.0:assertion");

                //        SecurityTokenHandlerCollection coll = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
                //        token = (Saml2SecurityToken)coll.ReadToken(xr.ReadSubtree());
                //        TextBox2.Text = token.Assertion.Subject.NameId.Value;
                //        //TextBox2.Text = token.Assertion.Statements.Count();
                //    }
                //}
                
                XmlDocument xmlSAML = new XmlDocument();
                xmlSAML.LoadXml(sSAMLResponse);
                if (IsValidSignature(xmlSAML)) TextBox2.Text = "Signature is valid;";
                else TextBox2.Text = "Signature is invalid";
                XmlNodeList nodeList = xmlSAML.GetElementsByTagName("saml2:Attribute");
                for (int i = 0; i < nodeList.Count; i++) TextBox2.Text = TextBox2.Text + nodeList.Item(i).Attributes.Item(0).Value + "=" + nodeList.Item(i).InnerText + ";";
            }
            catch 
            { 
                WriteLog("Error", "No or improperly formated SAMLResponse value presented");
            }
        }

        private bool IsValidSignature(XmlDocument xmlDoc)
        {
            SignedXml signedXml = new SignedXml(xmlDoc);
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("ds:Signature");

            if (nodeList != null && nodeList.Count > 0)
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
                return signedXml.CheckSignature();
            }
            return false;
        }

        protected void WriteLog(string t, params string[] p)
        {
            try
            {
                string fileName = ConfigurationManager.AppSettings["Logfilepath"].ToString();

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
}