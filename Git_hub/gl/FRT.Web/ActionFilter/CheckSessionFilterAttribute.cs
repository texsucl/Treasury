using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FRT.Web.ActionFilter
{
    public class CheckSessionFilterAttribute : ActionFilterAttribute
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string loginUrl = System.Configuration.ConfigurationManager.AppSettings.Get("loginUrl");

            logger.Info("FRT OnActionExecuting");

            try
            {
                HttpSessionStateBase session1 = filterContext.HttpContext.Session;
                logger.Info("get session test:" + session1.SessionID.ToString());
                //logger.Info("get session test:" + session1["test"].ToString());

                //var auth = filterContext.HttpContext.Request.Cookies["LoginInfo"];
                
                //string auth = filterContext.HttpContext.Request.Cookies["LoginInfo"]?.Value;
                HttpSessionStateBase session = filterContext.HttpContext.Session;

                if (filterContext.HttpContext.Session["UserID"] == null)
                {
                    logger.Info("filterContext.HttpContext.Session[UserID] null");
                    var url = new UrlHelper(filterContext.RequestContext);
                    //var loginUrl = url.Content("~/Account/Login");
                    session.RemoveAll();
                    session.Clear();
                    session.Abandon();
                    filterContext.HttpContext.Response.Redirect(loginUrl, true);

                }
                else {
                    logger.Info("filterContext.HttpContext.Session[UserID] not null");
                }
               
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                var url = new UrlHelper(filterContext.RequestContext);
                //var loginUrl = url.Content("~/Account/Login");

                filterContext.HttpContext.Response.Redirect(loginUrl, true);
            }


            

            //HttpSessionStateBase session = filterContext.HttpContext.Session;
            //var user = session["UserID"];

            //logger.Info("[OnActionExecuting]user:" + user);
            //logger.Info("[OnActionExecuting]session.IsNewSession:" + session.IsNewSession);

            //if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession))
            //{
            //    //send them off to the login page
            //    var url = new UrlHelper(filterContext.RequestContext);
            //    var loginUrl = url.Content("~/Account/Login");
            //    session.RemoveAll();
            //    session.Clear();
            //    session.Abandon();
            //    filterContext.HttpContext.Response.Redirect(loginUrl, true);
            //}
        }
    }
}