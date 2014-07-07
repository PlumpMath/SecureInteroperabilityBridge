using System;
using System.Web;
using SiemensISRM;


namespace sibsso
{
    /// <summary>
    /// sibsso supports SIB transformation using Web.config parameters
    /// </summary>
    public class sibsso : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            SIBLog.Write("Information", "Parsing request.");
            SIBTransform.ParseRequest(context.Request);
            SIBLog.Write("Information", "Building response and redirect.");
            SIBTransform.BuildResponse();
            if (SIBTransform.IsPostRequest() == true)
            {
                SIBLog.Write("Information", "Posting response.");
                context.Response.Write(SIBData.redirectPOSTRequest);
                context.Response.End();
            }
            else
            {
                SIBLog.Write("Information", "Performing redirect.");
                context.Response.Redirect(SIBData.redirectGETRequest);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}