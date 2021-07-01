using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Obounl.Utility.Log;
using static Obounl.ObounlEnum.Ref;
using Obounl.Utility;

namespace Obounl.Infrastructure
{
    public class Common
    {
        public string GetIp(bool onlyIp = false)
        {
            string _ip = string.Empty;
            try
            {
                string _ipfrom = System.Configuration.ConfigurationManager.AppSettings.Get("IpFrom");
                if (_ipfrom == "U") //測試
                {
                    _ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]?.ToString();
                }
                else //正式
                {
                    _ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]?.ToString();
                }
                //if (!onlyIp)
                //{
                //    var _HostName = System.Net.Dns.GetHostEntry(_ip)?.HostName;
                //    if (!_HostName.IsNullOrWhiteSpace())
                //        _ip = $@"{_ip},{_HostName}";
                //}
            }
            catch (Exception ex) 
            {
                NlogSet(ex.exceptionMessage(), null, Nlog.Error);
            }
            return _ip;
        }
    }
}