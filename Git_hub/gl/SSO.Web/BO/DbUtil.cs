using Fubon.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SSO.Web.BO
{
    public static class DbUtil
    {
        public static string GetDBFglConnStr()
        {
            var connString = ConfigurationManager.ConnectionStrings["dbFGL"].ConnectionString;

            return connString;
        }
    }
}