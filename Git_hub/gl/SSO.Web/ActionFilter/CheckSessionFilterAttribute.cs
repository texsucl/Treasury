using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SSO.Web.ActionFilter
{
    public class CheckSessionFilterAttribute : ActionFilterAttribute
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            try
            {
                HttpSessionStateBase session = filterContext.HttpContext.Session;
                if (filterContext.HttpContext.Session["UserID"] == null)
                {
                    //send them off to the login page
                    var url = new UrlHelper(filterContext.RequestContext);
                    var loginUrl = url.Content("~/Account/Login");
                    session.RemoveAll();
                    session.Clear();
                    session.Abandon();
                    filterContext.HttpContext.Response.Redirect(loginUrl, true);
                }
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
            }
        }
    }
}