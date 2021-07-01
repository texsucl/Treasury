using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Obounl.Controllers;
using Obounl.Utility;
//using Obounl.Models;
//using Obounl.Utility;

namespace Obounl.Infrastructure
{
    public class APIAuthAttribute : AuthorizeAttribute
    {
        string Token = ConfigurationManager.ConnectionStrings["TokenKey"]?.ConnectionString;

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var _token = (actionContext.Request.Headers.GetValues("token")?.FirstOrDefault() ?? string.Empty).AESDecrypt().Item2;
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
    }
}