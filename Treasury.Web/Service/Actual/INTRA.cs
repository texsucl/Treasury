using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;

namespace Treasury.Web.Service.Actual
{
    public class INTRA
    {
        public VW_OA_DEPT getDept(string DPT_CD)
        {
            var result = new VW_OA_DEPT();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                result = db.VW_OA_DEPT.FirstOrDefault(x => x.DPT_CD == DPT_CD);
            }
            return result;
        }
    }
}