using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Treasury.WebActionFilter
{
    public class CheckSessionFilterAttribute : ActionFilterAttribute
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            var user = session["UserID"];

            logger.Info("[OnActionExecuting]user:" + user);
            logger.Info("[OnActionExecuting]session.IsNewSession:" + session.IsNewSession);

            if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession))
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
    }
}