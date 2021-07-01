using FAP.Web.BO;
using FAP.Web.Service.Interface;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Linq;
using System.Web;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Utilitys;
using FAP.Web.Daos;
using FAP.Web.Models;
using System.Web.Mvc;

/// <summary>
/// 功能說明：用印檢視確認功能
/// 初版作者：20200302 Mark
/// 修改歷程：20200302 Mark
///           需求單號：
///           初版
/// </summary>
/// 


namespace FAP.Web.Service.Actual
{
    public class OAP0030 : Common, IOAP0030
    {

        /// <summary>
        /// 查詢 用印檢視確認
        /// </summary>
        /// <param name="applyDt_s">用印申請日(起)</param>
        /// <param name="applyDt_e">用印申請日(起)</param>
        /// <param name="report_no">表單號碼</param>
        /// <returns></returns>
        public List<OAP0022Model> GetSearchData(OAP0030SearchModel searchModel)
        {
            List<OAP0022Model> results = new List<OAP0022Model>();
            List<OAP0022Model> results2 = new List<OAP0022Model>();
            string sql = string.Empty;
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                using (EacCommand com = new EacCommand(conn))
                {
                    var _applyDt_s = searchModel.applyDt_s.DPformateTWdate(); //日期起
                    var _applyDt_e = searchModel.applyDt_e.DPformateTWdate(); //日期迄
                    //找尋狀態等於 8.用印中
                    sql = $@"
                    select APPLY_NO,RECE_DATE,RECE_TIME,RECE_ID,APPLY_ID from FAPPYCH0
                    where STATUS = '8' ";
                    if (!_applyDt_s.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPR_DATE2 >= :APPR_DATE_S ";
                        com.Parameters.Add("APPR_DATE_S", _applyDt_s);
                    }
                    if (!_applyDt_e.IsNullOrWhiteSpace())
                    {
                        sql += " and  APPR_DATE2 <= :APPR_DATE_E ";
                        com.Parameters.Add("APPR_DATE_E", _applyDt_e);
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
                            apply_id = dbresult["APPLY_ID"]?.ToString()?.Trim(), //申請人
                        };
                        results.Add(data);
                    }
                    com.Dispose();
                }
                if (results.Any())
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
select APPLY_NO,CHECK_NO from FAPPYCD0
where APPLY_NO in (
";
                        string c = string.Empty;
                        int i = 0;
                        foreach (var item in results.Select(x => x.apply_no))
                        {
                            sql += $@" {c} :apply_no{i} ";
                            com.Parameters.Add($@"apply_no{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += " ) ; ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        while (dbresult.Read())
                        {
                            var data = new OAP0022Model()
                            {
                                apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                                check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(), //支票號碼
                            };
                            results2.Add(data);
                        }
                        com.Dispose();
                    }
                }
                var userMemo = GetMemoByUserId(results.Where(x => !x.rece_id.IsNullOrWhiteSpace()).Select(x => x.rece_id).Distinct());
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    if (!searchModel.report_no.IsNullOrWhiteSpace())
                    {
                        var _report_no = searchModel.report_no.Trim();
                        var _checks = new List<OAP0022Model>();

                        var _FAP_FAPPYCD0_REPORTs = db.FAP_FAPPYCD0_REPORT.AsNoTracking().
                             Where(x => x.report_no == _report_no).ToList();
                        foreach (var item in results)
                        {
                            var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id)?.Item2;
                            item.rece_id = _rece_id.IsNullOrWhiteSpace() ? item.rece_id : _rece_id; //接收人
                            item.checkFlag = true;
                            item.check_no = string.Join(",", results2
                                .Where(x => x.apply_no == item.apply_no)
                                .Select(x => x.check_no));
                            var _FAP_FAPPYCD0_REPORT = _FAP_FAPPYCD0_REPORTs.FirstOrDefault(x => x.apply_no == item.apply_no);
                            if (_FAP_FAPPYCD0_REPORT != null)
                            {
                                item.report_no = _FAP_FAPPYCD0_REPORT.report_no;                              
                                _checks.Add(item);
                            }
                        }
                        results = _checks;
                    }
                    else
                    {
                        var _apply_no = results.Select(x => x.apply_no).ToList();
                        var _FAP_FAPPYCD0_REPORTs = db.FAP_FAPPYCD0_REPORT.AsNoTracking()
                            .Where(x => _apply_no.Contains(x.apply_no)).ToList();
                        foreach (var item in results)
                        {
                            var _rece_id = userMemo.FirstOrDefault(x => x.Item1 == item.rece_id)?.Item2;
                            item.rece_id = _rece_id.IsNullOrWhiteSpace() ? item.rece_id : _rece_id; //接收人
                            item.checkFlag = true;
                            item.check_no = string.Join(",", results2
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
            return results;
        }

        /// <summary>
        /// 執行 用印檢視確認
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MSGReturnModel SetStatus(IEnumerable<OAP0022Model> updateDatas, string userId)
        {
            MSGReturnModel resultModel = new MSGReturnModel();
            DateTime dtn = DateTime.Now;
            var updatedt = $@"{dtn.Year - 1911}{dtn.ToString("MMdd")}";
            var updatetm = $@"{dtn.ToString("HHmmssff")}";
            var _updateDatas = updateDatas.ToList();
            SysSeqDao sysSeqDao = new SysSeqDao();
            string[] curDateTime = DateUtil.getCurChtDateTime(3).Split(' ');
            String qPreCode = curDateTime[0];
            List<OAP0021DetailModel> OAP0021Models = new List<OAP0021DetailModel>();
            List<FAP_MAIL_LABEL_TEMP> FMLTs = new List<FAP_MAIL_LABEL_TEMP>();
            List<FAP_MAIL_LABEL> FMLs = new List<FAP_MAIL_LABEL>();
            List<FAP_MAIL_LABEL_D> FMLDs = new List<FAP_MAIL_LABEL_D>();
            if (_updateDatas.Any())
            {
                try
                {
                    using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
                    {
                        conn.Open();
                        EacTransaction transaction = conn.BeginTransaction();

                        string sql = string.Empty;

                        #region 檢核資料有無異動 並查詢應付票據變更申請主檔            
                        using (EacCommand com = new EacCommand(conn))
                        {
                            sql = $@"
select 
STATUS,REG_YN,ZIP_CODE,ADDR,RCV_NAME, APPLY_NO,APPLY_UNIT,APPLY_ID,RECE_ID
from FAPPYCH0
where APPLY_NO in (
";
                            string c = string.Empty;
                            int i = 0;
                            foreach (var item in updateDatas.Select(x => x.apply_no))
                            {
                                sql += $@" {c} :apply_no{i} ";
                                com.Parameters.Add($@"apply_no{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ; ";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            List<string> status = new List<string>();
                            while (dbresult.Read())
                            {
                                var _STATUS = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                                var _REG_YN = dbresult["REG_YN"]?.ToString()?.Trim(); //雙掛號回執
                                var _ZIP_CODE = dbresult["ZIP_CODE"]?.ToString()?.Trim(); //郵遞區號
                                var _ADDR = dbresult["ADDR"]?.ToString()?.Trim(); //地址
                                var _RCV_NAME = dbresult["RCV_NAME"]?.ToString()?.Trim(); //收件人
                                var _APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                                var _APPLY_UNIT = dbresult["APPLY_UNIT"]?.ToString()?.Trim(); //申請人員部門
                                var _APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                                var _RECE_ID = dbresult["RECE_ID"]?.ToString()?.Trim(); //接收人員
                                OAP0021Models.Add(new OAP0021DetailModel()
                                {
                                    reg_yn = _REG_YN,
                                    zip_code = _ZIP_CODE,
                                    addr = _ADDR,
                                    rcv_name = _RCV_NAME,
                                    apply_no = _APPLY_NO,
                                    apply_unit = _APPLY_UNIT,
                                    apply_id = _APPLY_ID,
                                    rece_id = _RECE_ID
                                });
                                status.Add(_STATUS); //狀態
                            }
                            if (status.Any(x => x != "8")) //如果狀態不等於 用印中
                            {
                                resultModel.DESCRIPTION = MessageType.already_Change.GetDescription();
                                return resultModel;
                            }
                            com.Dispose();
                        }
                        #endregion

                        #region 查詢應付票據變更申請明細檔
                        using (EacCommand com = new EacCommand(conn))
                        {
                            sql = $@"
select 
APPLY_NO,CHECK_NO
from FAPPYCD0
where APPLY_NO in (
";
                            string c = string.Empty;
                            int i = 0;
                            foreach (var item in updateDatas.Select(x => x.apply_no))
                            {
                                sql += $@" {c} :apply_no{i} ";
                                com.Parameters.Add($@"apply_no{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ; ";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            {
                                var _APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                                var _CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票
                                var item = OAP0021Models.FirstOrDefault(x => x.apply_no == _APPLY_NO);
                                FMLTs.Add(new FAP_MAIL_LABEL_TEMP()
                                {
                                    send_style = item.reg_yn == "Y" ? "1" : (item.reg_yn == "N" ? "2" : item.reg_yn), //寄送方式
                                    zip_code = item.zip_code, //郵遞區號
                                    addr = item.addr, //地址
                                    rcv_id = item.rcv_name, //收件人員
                                    apply_no = item.apply_no, //申請單號
                                    check_no = _CHECK_NO, //支票號碼
                                    apply_name = item.apply_unit , //申請人單位
                                    apply_id = item.apply_id, //申請人
                                    rece_id = item.rece_id //接收者
                                });
                            }
                        }
                        #endregion

                        #region 更新應付票據變更申請主檔 狀態
                        using (EacCommand com = new EacCommand(conn))
                        {
                            //                        sql = $@"
                            //update LAPPYCH1 set
                            //STATUS = :STATUS,
                            //APPR_DATE2 = :APPR_DATE2,
                            //APPR_TIME2 = :APPR_TIME2
                            //where STATUS = '8'
                            //and APPLY_NO in (
                            //";
                            sql = $@"
update LAPPYCH1 set
STATUS = :STATUS,
UPD_DATE = :UPD_DATE,
UPD_TIME = :UPD_TIME
where STATUS = '8'
and APPLY_NO in (
";
                            var c = string.Empty;
                            var i = 0;
                            com.Parameters.Add($@"STATUS", "4"); //完成抽票
                            com.Parameters.Add($@"UPD_DATE", updatedt);
                            com.Parameters.Add($@"UPD_TIME", updatetm);
                            foreach (var item in _updateDatas.Select(x => x.apply_no))
                            {
                                sql += $@" {c} :APPLY_NO{i} ";
                                com.Parameters.Add($@"APPLY_NO{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ";
                            com.Transaction = transaction;
                            com.CommandText = sql;
                            com.Prepare();
                            var updateNum = com.ExecuteNonQuery();
                            com.Dispose();
                        }
                        #endregion

                        using (dbFGLEntities db = new dbFGLEntities())
                        {
                            var FAP_FAPPYCD0_REPORTs = db.FAP_FAPPYCD0_REPORT.Where(x => x.flag != "Y").ToList();
                            foreach (var item in _updateDatas)
                            {
                                var _datas = FAP_FAPPYCD0_REPORTs.
                                    First(x =>
                                    x.apply_no == item.apply_no &&
                                    x.report_no == item.report_no);
                                _datas.update_datetime = dtn;
                                _datas.flag = "Y";
                            }
                            foreach (var items in FMLTs.GroupBy(x => new
                            {
                                x.send_style,
                                x.zip_code,
                                x.addr,
                                x.rcv_id
                            }))
                            {
                                var cId = sysSeqDao.qrySeqNo("AP", "S4", qPreCode).ToString();
                                var _id = $@"S4{qPreCode}{cId.ToString().PadLeft(3, '0')}";
                                FMLs.Add(new FAP_MAIL_LABEL()
                                {
                                    id = _id, //pkid
                                    send_style = items.Key.send_style, //寄送方式
                                    zip_code = items.Key.zip_code, //郵遞區號
                                    addr = items.Key.addr, //地址
                                    rcv_id = items.Key.rcv_id, //收件人員
                                    memo = GetCheckmemo(items.Select(x => x.check_no)), //備註
                                    number = items.Count(), //張數
                                    apply_name = string.Join(",", items.Select(x => x.apply_name).Distinct().OrderBy(x => x)), //行政單位
                                    apply_id = string.Join(",", items.Select(x => x.apply_id).Distinct().OrderBy(x => x)), //申請人員
                                    rece_id = string.Join(",", items.Select(x => x.rece_id).Distinct().OrderBy(x => x)), //接收人員
                                    create_datetime = dtn
                                });
                                foreach (var item in items)
                                {
                                    FMLDs.Add(new FAP_MAIL_LABEL_D()
                                    {
                                        id = _id,  //pkid
                                        apply_no = item.apply_no, //應付票據變更主檔申請單號碼
                                        check_no = item.check_no //支票號碼
                                    });
                                }
                            }
                            transaction.Commit();
                            try
                            {
                                db.FAP_MAIL_LABEL.AddRange(FMLs);
                                db.FAP_MAIL_LABEL_D.AddRange(FMLDs);
                                db.SaveChanges();

                                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                                piaLogMain.TRACKING_TYPE = "A";
                                piaLogMain.ACCESS_ACCOUNT = userId;
                                piaLogMain.ACCOUNT_NAME = "";
                                piaLogMain.PROGFUN_NAME = "OAP0030";
                                piaLogMain.EXECUTION_CONTENT = $@"apply_no:{string.Join(",", updateDatas.Select(x => x.apply_no))}";
                                piaLogMain.AFFECT_ROWS = FMLs.Count;
                                piaLogMain.PIA_TYPE = "0100000000";
                                piaLogMain.EXECUTION_TYPE = "A";
                                piaLogMain.ACCESSOBJ_NAME = "FAPPYCH";
                                piaLogMainDao.Insert(piaLogMain);
                                resultModel.RETURN_FLAG = true;
                                resultModel.DESCRIPTION = MessageType.Exec_Success.GetDescription();

                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                resultModel.DESCRIPTION = MessageType.Exec_Fail.GetDescription();
                                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultModel.DESCRIPTION = MessageType.Exec_Fail.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }

            }
            else
            {
                resultModel.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
            }
            return resultModel;
        }
    }
}