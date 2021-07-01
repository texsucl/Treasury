using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using System.Web;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using System.Configuration;

namespace Obounl.Infrastructure
{
    public class IPAuthAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var _ip = new Common().GetIp(true);
            var _ips = (ConfigurationManager.AppSettings["ipSecurity"] ?? string.Empty).Split(';').ToList();
            NlogSet($@"IPAuthAttribute ip:{_ip}", null, Nlog.Info);
            return _ips.Contains(_ip);
        }
    }
}