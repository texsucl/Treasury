
using Dapper;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace SSO.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            logger.Info("Application_Start");
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            //以資料表保存集合物件JSON
            //SqlMapper.AddTypeHandler<List<HistoryRecord>>(new JsonConvertHandler<List<HistoryRecord>>());
           // SqlMapper.AddTypeHandler(new DateTimeHandler());

            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

           // JobScheduler.Start();


        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {


            ////Check If it is a new session or not, if not then do the further checks
            //if (Request.Cookies["ASP.NET_SessionId"] != null && Request.Cookies["ASP.NET_SessionId"].Value != null)
            //{
            //    string newSessionID = Request.Cookies["ASP.NET_SessionId"].Value;


            //    //Genrate Hash key for this User,Browser and machine and match with the Entered NewSessionID
            //    if (!(newSessionID.Equals(Session["ASP.NET_SessionId"])))
            //    {
            //        //Log the attack details here
            //        Response.Cookies["TriedTohack"].Value = "True";
            //        throw new HttpException("Invalid Request");
            //    }
                

            //}

        }

        protected void Application_EndRequest()
        {
            ////Pass the custom Session ID to the browser.
            //if (Response.Cookies["ASP.NET_SessionId"] != null)
            //{
            //    string guid = Guid.NewGuid().ToString();
            //    Session["ASP.NET_SessionId"] = guid;
            //    // now create a new cookie with this guid value
            //    Response.Cookies["ASP.NET_SessionId"].Value = guid;
            //}
        }

        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();
            var code = (ex is HttpException) ? (ex as HttpException).GetHttpCode() : 500;
            //Log.AddSysLogError(ex.Message, statusCode: code, xml: ex.StackTrace);
        }
    }
}
