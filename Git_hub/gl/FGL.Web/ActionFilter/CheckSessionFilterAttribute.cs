using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FGL.Web.ActionFilter
{
    public class CheckSessionFilterAttribute : ActionFilterAttribute
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string loginUrl = System.Configuration.ConfigurationManager.AppSettings.Get("loginUrl");

            logger.Info("FGL OnActionExecuting");

            try
            {
                HttpSessionStateBase session1 = filterContext.HttpContext.Session;
                logger.Info("get session test:" + session1.SessionID.ToString());

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
                else
                {
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


        }
    }
}