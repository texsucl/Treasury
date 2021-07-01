using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SSO.Web.Models;
using SSO.WebViewModels;
using SSO.Web.Utils;

namespace SSO.Web.Daos
{
    public class OaDeptDao
    {

        public List<VW_OA_DEPT> qryByDptCdArr(string[] DPT_CD)
        {
            DB_INTRAEntities context = new DB_INTRAEntities();

            List<VW_OA_DEPT> oaDept = context.VW_OA_DEPT
                .Where(x => DPT_CD.Contains(x.DPT_CD)).ToList();

            return oaDept;
        }


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


        /// <summary>
        /// 依傳入的單位查出所屬部及其部下所有的科
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        public List<UnitInfoModel> qryDept(String DPT_CD)
        {
            VW_OA_DEPT unit = qryByDptCd(DPT_CD);

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                List<UnitInfoModel> rows = new List<UnitInfoModel>();

                if ("03".Equals(StringUtil.toString(unit.Dpt_type)))
                {
                    rows =
                (from m in db.VW_OA_DEPT
                 where m.DPT_CD == DPT_CD
                 select new UnitInfoModel()
                 {
                     unitName = m.DPT_NAME.Trim(),
                     unitCode = m.DPT_CD.Trim(),
                     levelCode = m.Dpt_type.Trim()

                 }).ToList()
                        .Union(from m in db.VW_OA_DEPT
                               where m.UP_DPT_CD == DPT_CD
                               select new UnitInfoModel()
                               {
                                   unitName = m.DPT_NAME.Trim(),
                                   unitCode = m.DPT_CD.Trim(),
                                   levelCode = m.Dpt_type.Trim()

                               }).ToList();

                }
                else if ("04".Equals(StringUtil.toString(unit.Dpt_type)))
                {
                    rows =
                (from m in db.VW_OA_DEPT
                 join dept in db.VW_OA_DEPT.Where(x => x.Dpt_type == "03") on m.UP_DPT_CD equals dept.DPT_CD
                 where m.DPT_CD == DPT_CD
                 select new UnitInfoModel()
                 {
                     unitName = dept.DPT_NAME.Trim(),
                     unitCode = dept.DPT_CD.Trim(),
                     levelCode = dept.Dpt_type.Trim()

                 }).ToList()
                        .Union(from m in db.VW_OA_DEPT
                               join dept in db.VW_OA_DEPT on m.UP_DPT_CD equals dept.UP_DPT_CD
                               where m.DPT_CD == DPT_CD
                               select new UnitInfoModel()
                               {
                                   unitName = dept.DPT_NAME.Trim(),
                                   unitCode = dept.DPT_CD.Trim(),
                                   levelCode = dept.Dpt_type.Trim()

                               }).ToList();

                }
                return rows;
            }
               
            }

        

    }
}