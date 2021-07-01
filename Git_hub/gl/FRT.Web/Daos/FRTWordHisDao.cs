using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTWordHisDao
    {

        public List<ORTB016Model> qryForSTAT()
        {
            List<ORTB016Model> results = new List<ORTB016Model>();

            var emplys = new List<V_EMPLY2>();

            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                emplys = db.V_EMPLY2.AsNoTracking().Where(x => x.USR_ID != null).ToList();
            }

            using (dbFGLEntities db = new dbFGLEntities())
            {
                results.AddRange(db.FRT_WORD_HIS.AsNoTracking()
                    .Where(x => x.appr_stat == "1").AsEnumerable()
                    .Select(x => new ORTB016Model()
                    {
                        frt_word_Id = x.frt_word_id,
                        aplyNo = x.aply_no,
                        frt_sys_type = x.frt_sys_type,
                        frt_srce_from = x.frt_srce_from,
                        frt_srce_kind = x.frt_srce_kind,
                        frt_memo_apx = x.frt_memo_apx,
                        frt_achcode = x.frt_achcode,
                        status = x.exec_action,
                        updId = x.update_id,
                        updateUName = emplys.FirstOrDefault(y => y.USR_ID == x.update_id)?.EMP_NAME?.Trim() ?? x.update_id,
                        updDatetime = x.update_datetime?.ToString("yyyy/MM/dd HH:mm:ss"),
                    }));
            }

            return results;
        }

        /// <summary>
        /// 更新覆核結果
        /// </summary>
        /// <param name="recData">核可資料</param>
        /// <param name="rtnData">駁回資料</param>
        /// <param name="db"></param>
        /// <param name="dtn">更新時間</param>
        /// <returns></returns>
        public string updateStat(List<ORTB016Model> recData, List<ORTB016Model> rtnData, dbFGLEntities db ,DateTime dtn)
        {
            string msg = string.Empty;
            try
            {
                var his = db.FRT_WORD_HIS.Where(x => x.appr_stat == "1").ToList();
                foreach (var item in recData) //核可
                {
                    var _his = his.FirstOrDefault(x => x.aply_no == item.aplyNo && x.frt_word_id == item.frt_word_Id);
                    if (_his != null)
                    {
                        _his.appr_id = item.apprId;
                        _his.appr_stat = "2";
                        _his.approve_datetime = dtn;
                    }
                    else if(msg == string.Empty)
                    {
                        msg = "資料已異動!!";
                    }
                }
                foreach (var item in rtnData)
                {
                    var _his = his.FirstOrDefault(x => x.aply_no == item.aplyNo && x.frt_word_id == item.frt_word_Id);
                    if (_his != null)
                    {
                        _his.appr_id = item.apprId;
                        _his.appr_stat = "3";
                        _his.approve_datetime = dtn;
                    }
                    else if (msg == string.Empty)
                    {
                        msg = "資料已異動!!";
                    }
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                msg = e.Message;
            }
            return msg;

        }


    }
}