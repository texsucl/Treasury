using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWTD.Daos
{
    public class EasyCom
    {
        /// <summary>
        /// 取得easycom連線資訊
        /// </summary>
        /// <returns></returns>
        public static String GetEasycomConn()
        {
            var connString = ConfigurationManager.ConnectionStrings["Easycom"]?.ConnectionString;
            return connString;
        }
    }
}
