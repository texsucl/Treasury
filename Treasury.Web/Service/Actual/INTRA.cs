using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;

namespace Treasury.Web.Service.Actual
{
    public class INTRA
    {
        /// <summary>
        /// 回傳 部門 資料
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        public VW_OA_DEPT getDept(string DPT_CD)
        {
            var result = new VW_OA_DEPT();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                result = db.VW_OA_DEPT.FirstOrDefault(x => x.DPT_CD == DPT_CD);
            }
            return result;
        }

        /// <summary>
        /// 回傳 1.部門 2.科別
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        public Tuple<string, string> getDept_Sect(string DPT_CD)
        {
            //var result = 
            var _dept = getDept(DPT_CD);
            if (_dept.Dpt_type != null)
            {
                switch (_dept.Dpt_type.Trim())
                {
                    case "04":
                        return new Tuple<string, string>(_dept.UP_DPT_CD?.Trim(),_dept.DPT_CD?.Trim());
                    case "03":
                        return new Tuple<string, string>(_dept.DPT_CD?.Trim(),null);
                }
            }
            return new Tuple<string, string>(null,null);
        }
    }
}