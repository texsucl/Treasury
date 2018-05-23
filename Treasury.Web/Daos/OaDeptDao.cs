using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;

namespace Treasury.Web.Daos
{
    public class OaDeptDao
    {
        /// <summary>
        /// 輸入單位代碼，查詢相關資訊
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        public VW_OA_DEPT qryByDptCd(String DPT_CD)
        {
            DB_INTRAEntities context = new DB_INTRAEntities();

            VW_OA_DEPT oaDept = context.VW_OA_DEPT
                .Where(x => x.DPT_CD == DPT_CD).FirstOrDefault();

            return oaDept;
        }
    }
}