using Fubon.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FGL.Web.BO
{
    public static class DbUtil
    {
        public static string GetDBFglConnStr()
        {
            var connString = ConfigurationManager.ConnectionStrings["dbFGL"].ConnectionString;

            return connString;
        }


        /// <summary>
        /// 連結基金DB
        /// </summary>
        /// <returns></returns>
        public static string GetDBFdConnStr()
        {
            var connString = ConfigurationManager.ConnectionStrings["dbFD"].ConnectionString;

            return connString;
        }

        public static string GetDBGlsiExtConnStr()
        {
            var connString = ConfigurationManager.ConnectionStrings["dbGLSIEXT"].ConnectionString;

            return connString;
        }
    }
}