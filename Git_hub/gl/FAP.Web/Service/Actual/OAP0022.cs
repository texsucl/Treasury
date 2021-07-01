using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using System.Data;
using FAP.Web.Daos;
using FAP.Web.Models;

/// <summary>
/// 功能說明：應付票據變更用印清冊
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Service.Actual
{
    public class OAP0022 : Common, IOAP0022
    {
        /// <summary>
        /// 查詢 應付票據變更用印清冊
        /// </summary>
        /// <param name="app_s">覆核日(起)</param>
        /// <param name="sppr_e">覆核日(迄)</param>
        /// <param name="rece_id">接收人</param>
        /// <param name="report_no">表單號碼</param>
        /// <returns></returns>
        public List<OAP0022Model> Search_OAP0022(OAP0022SearchModel searchModel)
        {
            List<OAP0022Model> result = new List<OAP0022Model>();
            List<OAP0022Model> result2 = new List<OAP0022Model>();

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;

                using (EacCommand com = new EacCommand(conn))
                {
                    var date_s = searchModel.appr_s.DPformateTWdate(); //日期起
                    var date_e = searchModel.appr_e.DPformateTWdate(); //日期迄
               
                    sql = $@"
select APPLY_NO , RECE_DATE , RECE_TIME , RECE_ID from FAPPYCH0
where STATUS in ('7','8','4')
";
                    if (!date_s.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPR_DATE2 >= :APPR_DATE_S ";
                        com.Parameters.Add("APPR_DATE_S", date_s);
                    }
                    if (!date_e.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPR_DATE2 <= :APPR_DATE_E ";
                        com.Parameters.Add("APPR_DATE_E", date_e);
                    }
                    if (!searchModel.rece_id.IsNullOrWhiteSpace())
                    {
                        sql += " and  RECE_ID = :RECE_ID ";
                        com.Parameters.Add("RECE_ID", searchModel.rece_id);
                    }
                    sql += "  ;";
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();
                    while (dbresult.Read())
                    {
                        var data = new OAP0022Model()
                        {
                            apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                            rece_date = dbresult["RECE_DATE"]?.ToString()?.Trim()?.stringTWDateFormate(), //接收日期
                            rece_time = dbresult["RECE_TIME"]?.ToString()?.Trim()?.stringTimeFormate(), //接收時間
                            rece_id = dbresult["RECE_ID"]?.ToString()?.Trim(), //接收人
                        };
                        result.Add(data);
                    }
                    com.Dispose();
                }
                if (result.Any())
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        var date_s = searchModel.appr_s.DPformateTWdate(); //日期起
                        var date_e = searchModel.appr_e.DPformateTWdate(); //日期迄

                        sql = $@"
select APPLY_NO,CHECK_NO from FAPPYCD0
where APPLY_NO in (
";
                        string c = string.Empty;
                        int i = 0;
                        foreach (var item in result.Select(x=> x.apply_no))
                        {
                            sql += $@" {c} :apply_no{i} ";
                            com.Parameters.Add($@"apply_no{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += " ) ; ";
                        //if (!searchModel.check_no.IsNullOrWhiteSpace())
                        //{
                        //    sql += @" and CHECK_NO = :check_no ";
                        //    com.Parameters.Add($@"check_no", searchModel.check_no.Trim());
                        //}
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var _apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                            //var _data = result.Single(x => x.apply_no == _apply_no);
                            var data = new OAP0022Model()
                            {
                                apply_no = _apply_no,
                                check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                            };
                            result2.Add(data);
                        }
                        com.Dispose();
                    }
                    var userMemo = GetMemoByUserId(result.Where(x => !x.rece_id.IsNullOrWhiteSpace()).Select(x => x.rece_id).Distinct());
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        if (!searchModel.report_no.IsNullOrWhiteSpace())
                        {
                            var _report_no = searchModel.report_no.Trim();
                            var _checks = new List<OAP0022Model>();

                            var _FAP_FAPPYCD0_REPORTs = db.FAP_FAPPYCD0_REPORT.AsNoTracking().
                                 Where(x => x.report_no == _report_no).ToList();
                            foreach (var item in result)
                            {
                                var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id)?.Item2;
                                item.rece_id = _rece_id.IsNullOrWhiteSpace() ? item.rece_id : _rece_id; //接收人

                                item.check_no = string.Join(",", result2
                                    .Where(x => x.apply_no == item.apply_no)
                                    .Select(x => x.check_no));
                                var _FAP_FAPPYCD0_REPORT = _FAP_FAPPYCD0_REPORTs.FirstOrDefault(x => x.apply_no == item.apply_no);
                                if (_FAP_FAPPYCD0_REPORT != null)
                                {
                                    item.report_no = _FAP_FAPPYCD0_REPORT.report_no;
                                    _checks.Add(item);
                                }
                            }
                            result = _checks;
                        }
                        else
                        {
                            var _apply_no = result.Select(x => x.apply_no).ToList(); 
                            var _FAP_FAPPYCD0_REPORTs = db.FAP_FAPPYCD0_REPORT.AsNoTracking()
                                .Where(x=> _apply_no.Contains(x.apply_no)).ToList();
                            foreach (var item in result)
                            {
                                var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id)?.Item2;
                                item.rece_id = _rece_id.IsNullOrWhiteSpace() ? item.rece_id : _rece_id; //接收人

                                item.check_no = string.Join(",", result2
                                .Where(x => x.apply_no == item.apply_no)
                                .Select(x => x.check_no));
                                var _FAP_FAPPYCD0_REPORT = _FAP_FAPPYCD0_REPORTs.FirstOrDefault(x => x.apply_no == item.apply_no);
                                if (_FAP_FAPPYCD0_REPORT != null)
                                {
                                    item.report_no = _FAP_FAPPYCD0_REPORT.report_no;
                                }
                            }
                        }
                    }
                }
                conn.Dispose();
                conn.Close();
            }

            return result
                .Where(x=> !x.check_no.IsNullOrWhiteSpace()
                && x.check_no.Split(',').Contains(searchModel.check_no.Trim())
                , !searchModel.check_no.IsNullOrWhiteSpace())
                .OrderBy(x=>x.rece_date).ThenBy(x=>x.rece_time).ToList();
        }

        /// <summary>
        /// 設定 表單號碼
        /// </summary>
        /// <param name="datas">應付票據</param>
        /// <param name="userId">使用者</param>
        /// <returns></returns>
        public MSGReturnModel<string> Set_OAP0022(IEnumerable<OAP0022Model> datas, string userId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            if (!datas.Any(x => x.checkFlag))
            {
                result.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
                return result;
            }
            if (datas.Where(x => x.checkFlag).Any(x => !string.IsNullOrWhiteSpace(x.report_no)))
            {
                result.DESCRIPTION = "所選資料中已有表單號碼!";
                return result;
            }
            DateTime dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            SysSeqDao sysSeqDao = new SysSeqDao();
            string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("AP", "R1", qPreCode).ToString();
            string reportNo = $@"{qPreCode}{cId.ToString().PadLeft(3, '0')}";
            result.Datas = reportNo;
            string sql = string.Empty;
            try
            {
                using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                {
                    conn.Open();
                    EacTransaction transaction = conn.BeginTransaction();
                    using (EacCommand com = new EacCommand(conn))
                    {
//                        sql = $@"
//update LAPPYCH1
//set 
//STATUS = :STATUS
//,
//RECE_DATE = :RECE_DATE,
//RECE_TIME = :RECE_TIME
//where STATUS = '7'
//and APPLY_NO in ( 
//";

                        sql = $@"
update LAPPYCH1
set 
STATUS = :STATUS,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where STATUS = '7'
and APPLY_NO in ( 
";
                        com.Parameters.Add("STATUS", "8"); //8:用印中
                        com.Parameters.Add("UPD_DATE", updatedt); //更新日期
                        com.Parameters.Add("UPD_TIME", updatetm); //更新時間

                        string c = string.Empty;
                        int i = 0;
                        foreach (var item in datas.Select(x => x.apply_no))
                        {
                            sql += $@" {c} :APPLY_NO_{i} ";
                            com.Parameters.Add($@"APPLY_NO_{i}", item);
                            c = ",";
                            i += 1;
                        }
                        sql += " ) ;";
                        com.Transaction = transaction;
                        com.CommandText = sql;
                        com.Prepare();
                        var updateNum = com.ExecuteNonQuery();
                        com.Dispose();
                        transaction.Commit();
                        try
                        {
                            using (dbFGLEntities db = new dbFGLEntities())
                            {
                                db.FAP_FAPPYCD0_REPORT.AddRange(datas.Select(x => new FAP_FAPPYCD0_REPORT()
                                {
                                    apply_no = x.apply_no,
                                    report_no = reportNo,
                                    report_datetime = dtn,
                                    create_id = userId
                                }));
                                db.SaveChanges();
                            }
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = MessageType.Exec_Success.GetDescription();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                result.DESCRIPTION = MessageType.Exec_Fail.GetDescription(null, "系統發生錯誤，請洽系統管理員!!");
            }
            return result;
        }
    }
}