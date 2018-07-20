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
        /// 回傳 dept 資料
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
        /// 回傳 DEPT SECT
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

        /// <summary>
        /// 回傳 Emply 資料
        /// </summary>
        /// <param name="USR_ID"></param>
        /// <returns></returns>
        public List<V_EMPLY2> getEmply(string USR_ID=null)
        {
            var result = new List<V_EMPLY2>();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                if(string.IsNullOrEmpty(USR_ID))
                {
                    result = db.V_EMPLY2.ToList();
                }
                else
                {
                    result = db.V_EMPLY2.Where(x => x.USR_ID == USR_ID).ToList();
                }
            }
            return result;
        }
    }
}