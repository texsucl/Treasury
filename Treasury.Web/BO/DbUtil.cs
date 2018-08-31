using Fubon.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Treasury.WebUtils
{
    public static class DbUtil
    {
        public static string GetDBTreasuryConnStr()
        {
			var connString = ConfigurationManager.ConnectionStrings["dbTreasury"].ConnectionString;

			return connString;
        }
    }
}