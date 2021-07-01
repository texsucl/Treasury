using FRT.Web.BO;
using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTXml522657Dao
    {
        public string qryRmtapx(string fastNo)
        {
            string rmtapx = "";

            dbFGLEntities db = new dbFGLEntities();
            var rows = (from r522657 in db.FRT_XML_522657
                             where 1 == 1
                                 & r522657.FAST_NO == fastNo
                             select new  
                             {
                                 rmtApx = r522657.RMT_APX
                             }).FirstOrDefault();

            if (rows != null)
                rmtapx = StringUtil.toString(rows.rmtApx);

            return rmtapx;

        }
    }
}