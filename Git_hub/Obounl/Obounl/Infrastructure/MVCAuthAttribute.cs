using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Obounl.Controllers;
using Obounl.Utility;
//using Obounl.Models;
//using Obounl.Utility;

namespace Obounl.Infrastructure
{
    public class MVCAuthAttribute : AuthorizeAttribute
    {
        string Token = ConfigurationManager.ConnectionStrings["TokenKey"]?.ConnectionString;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //bool hasuser = httpContext.User != null;
            //httpContext.Request.Headers.
            //驗證要替換

            //bool isAuthenticated = hasuser && httpContext.User.Identity.IsAuthenticated;
            var _token = (httpContext.Request.Headers.GetValues("token")?.FirstOrDefault() ?? string.Empty).AESDecrypt().Item2;
            var _Auth = false;
            DateTime _token_dtm = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(_token))
            {
                DateTime.TryParseExact(_token, "yyyy/MM/dd HH:mm:ss", null,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                out _token_dtm);
                if (_token_dtm >= DateTime.Now)
                    _Auth = true;
            }
            return _Auth;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.Result = new ContentResult();
                filterContext.HttpContext.Response.StatusCode = 401;
            }
            else
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }          
        }
    }
}