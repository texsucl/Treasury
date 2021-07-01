using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FAP.Web.Service.Actual;

namespace FAP.Web.Report.Data
{
    public class OAP0025 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var entry_date_s_old = parms.Where(x => x.key == "entry_date_s").FirstOrDefault()?.value?.Trim();
            var entry_date_e_old = parms.Where(x => x.key == "entry_date_e").FirstOrDefault()?.value?.Trim();
            string entry_date_s = entry_date_s_old?.DPformateTWdate() ?? string.Empty; //新增日期(起)
            string entry_date_e = entry_date_e_old?.DPformateTWdate() ?? string.Empty; //新增日期(迄)
            string unit_code = parms.Where(x => x.key == "unit_code").FirstOrDefault()?.value?.Trim() ?? string.Empty; //收件部門
            string srce_from = parms.Where(x => x.key == "srce_from").FirstOrDefault()?.value?.Trim() ?? string.Empty; //資料來源 (All,抽票,票據變更,批次轉入)
            string apply_no = parms.Where(x => x.key == "apply_no").FirstOrDefault()?.value?.Trim() ?? string.Empty; //批次單號
            string rej_flag = parms.Where(x => x.key == "rej_flag").FirstOrDefault()?.value?.Trim() ?? string.Empty; //票據變更退件

            List<OAP0025ReportModel> reportModels = new List<OAP0025ReportModel>();
            List<OAP0021DetailSubModel> subdatas = new List<OAP0021DetailSubModel>();
            List<FAP_CODE> FAP_CODEs = new List<FAP_CODE>();

            List <string> FAPPYCH0_apply_nos = new List<string>(); //應付票據變更申請主檔
            List<string> FAPPYEH0_apply_nos = new List<string>(); //應付票據抽票申請主檔
            extensionParms.Add(new reportParm()
            {
                key = "Data_Date",
                value = $@"{entry_date_s_old}~{entry_date_e_old}"
            });
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;

                using (EacCommand com = new EacCommand(conn))
                {
                    //應付票據簽收檔
                    sql = $@"
select UNIT_CODE,APPLY_ID,SRCE_FROM ,CHECK_NO,APPLY_NO,ADM_UNIT,ENTRY_ID  from FAPPYSN0
where entry_date between :entry_date_s and :entry_date_e ";

                    com.Parameters.Add($@"entry_date_s", entry_date_s);
                    com.Parameters.Add($@"entry_date_e", entry_date_e);
                    if (!unit_code.IsNullOrWhiteSpace())
                    {
                        sql += $@" and unit_code = :unit_code ";
                        com.Parameters.Add($@"unit_code", unit_code);
                    }
                    if (srce_from != "All")
                    {
                        sql += $@" and srce_from = :srce_from ";
                        com.Parameters.Add($@"srce_from", srce_from?? string.Empty);
                    }
                    if (!apply_no.IsNullOrWhiteSpace())
                    {
                        sql += $@" and apply_no = :apply_no ";
                        com.Parameters.Add($@"apply_no", apply_no);
                    }
                    if (!rej_flag.IsNullOrWhiteSpace() && (rej_flag == "Y" || rej_flag == "N"))
                    {
                        sql += $@" and srce_from = '2' ";
                    }
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();

                    while (dbresult.Read())
                    {
                        var model = new OAP0025ReportModel();
                        model.UNIT_CODE = dbresult["UNIT_CODE"]?.ToString()?.Trim(); //收件單位
                        model.SEND_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //收件人員
                        model.SRCE_FROM = dbresult["SRCE_FROM"]?.ToString()?.Trim(); //來源
                        model.CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                        model.APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人
                        model.APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請(批次)單號
                        model.ADM_UNIT = dbresult["ADM_UNIT"]?.ToString()?.Trim(); //申請原因/行政單位
                        model.ENTRY_ID = dbresult["ENTRY_ID"]?.ToString()?.Trim(); //處理人員

                        reportModels.Add(model);
                        subdatas.Add(new OAP0021DetailSubModel() { check_no = model.CHECK_NO });
                        if (model.SRCE_FROM == "1") //應付票據抽票申請主檔
                        {
                            FAPPYEH0_apply_nos.Add(model.APPLY_NO);
                        }
                        else if(model.SRCE_FROM == "2") //應付票據變更申請主檔
                            FAPPYCH0_apply_nos.Add(model.APPLY_NO);
                    }
                    com.Dispose();
                }
                if (reportModels.Any())
                {
                    if (!rej_flag.IsNullOrWhiteSpace() && (rej_flag == "Y" || rej_flag == "N")) //票據變更退件有值
                    {
                        reportModels = reportModels.Where(x => x.SRCE_FROM == "2").ToList(); //只抓取來源為票據變更
                        FAPPYEH0_apply_nos = new List<string>(); //抽票資料清空
                    }
                    var common = new Service.Actual.Common();
                    var oap0021 = new OAP0021();
                    var depts = common.GetDepts();
                    //應付票據簽收窗口部門檔
                    List<FAP_NOTES_PAYABLE_RECEIVED> FNPR = new List<FAP_NOTES_PAYABLE_RECEIVED>();
                    //應付票據簽收窗口科別檔
                    List<FAP_NOTES_PAYABLE_RECEIVED_D> FNPRD = new List<FAP_NOTES_PAYABLE_RECEIVED_D>();
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        FNPR = db.FAP_NOTES_PAYABLE_RECEIVED.AsNoTracking().ToList();
                        FNPRD = db.FAP_NOTES_PAYABLE_RECEIVED_D.AsNoTracking().ToList();
                        FAP_CODEs = db.FAP_CODE.AsNoTracking().ToList();
                    }
                    var SRCE_FROMs = new SysCodeDao().qryByType("AP", "SRCE_FROM");
                    oap0021.getSubData(subdatas);
                    if (FAPPYEH0_apply_nos.Any())
                    {
                        using (EacCommand com = new EacCommand(conn))
                        {
                            //應付票據抽票申請主檔
                            sql = $@"
select APPLY_NO , APPLY_ID , CE_RSN , CE_RPLY_ID , STATUS from FAPPYEH0 
where APPLY_NO in ( " ;
                            string c = string.Empty;
                            int i = 0;
                            foreach (var item in FAPPYEH0_apply_nos)
                            {
                                sql += $@" {c} :APPLY_NO_{i} ";
                                com.Parameters.Add($@"APPLY_NO_{i}", item);
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
                                var _APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                                var _CE_RSN = dbresult["CE_RSN"]?.ToString()?.Trim(); //抽件原因
                                var _CE_RPLY_ID = dbresult["CE_RPLY_ID"]?.ToString()?.Trim(); //抽票回覆人員
                                var _STATUS = dbresult["STATUS"]?.ToString()?.Trim(); //應付票據抽票主檔 狀態
                                var _apply_memo = oap0021.GetMemoByUserId(new List<string>() { _APPLY_ID });
                                var _UNIT_CODE_memo = _apply_memo.First().Item3; //收件單位
                                var _SEND_ID_memo = _APPLY_ID; //收件人
                                //應付票據簽收窗口部門檔 有找到簽收窗口部門的資料
                                var _FNPRD = FNPRD.FirstOrDefault(x => x.division == _UNIT_CODE_memo);
                                if (_FNPRD != null)
                                {
                                    var _FNPR_memo = FNPR.FirstOrDefault(x => x.dep_id == _FNPRD.dep_id);
                                    _UNIT_CODE_memo = _FNPR_memo.dep_id; //部門檔
                                    _SEND_ID_memo = _FNPR_memo.apt_id; //窗口人員
                                }
                                foreach (var item in reportModels.Where(x => x.APPLY_NO == _APPLY_NO))
                                {
                                    item.UNIT_CODE = _UNIT_CODE_memo; //收件單位
                                    item.SEND_ID = _SEND_ID_memo; //收件人員
                                    item.APPLY_ID = _APPLY_ID; //申請人
                                    item.ADM_UNIT = FAP_CODEs.FirstOrDefault(x => x.reason_code == _CE_RSN)?.reason ?? _CE_RSN; //抽票原因
                                    item.ENTRY_ID = _CE_RPLY_ID; //處理人員
                                    item.STATUS = _STATUS; // 狀態
                                }
                            }
                            com.Dispose();
                        }
                    }
                    if (FAPPYCH0_apply_nos.Any())
                    {
                        var MARK_TYPEs = new SysCodeDao().qryByType("AP", "MARK_TYPE");
                        using (EacCommand com = new EacCommand(conn))
                        {
                            sql = $@"
select APPLY_NO,SEND_UNIT,SEND_ID,APPLY_ID,REJ_RSN,STATUS,RECE_ID from FAPPYCH0
where APPLY_NO in (
";
                            string c = string.Empty;
                            int i = 0;
                            foreach (var item in FAPPYCH0_apply_nos)
                            {
                                sql += $@" {c} :APPLY_NO_{i} ";
                                com.Parameters.Add($@"APPLY_NO_{i}", item);
                                c = " , ";
                                i += 1;
                            }
                            sql += " ) ; ";
                            com.CommandText = sql;
                            com.Prepare();
                            DbDataReader dbresult = com.ExecuteReader();
                            while (dbresult.Read())
                            { 
                                var _APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請(批次)單號
                                var _SEND_UNIT = dbresult["SEND_UNIT"]?.ToString()?.Trim(); //送件單位
                                var _SEND_ID = dbresult["SEND_ID"]?.ToString()?.Trim(); //送件人員
                                var _APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人員
                                //var _MARK_TYPE2 = dbresult["MARK_TYPE2"]?.ToString()?.Trim(); //目前註記
                                var _REJ_RSN = dbresult["REJ_RSN"]?.ToString()?.Trim(); //退件原因
                                var _STATUS = dbresult["STATUS"]?.ToString()?.Trim(); //狀態
                                var _RECE_ID = dbresult["RECE_ID"]?.ToString()?.Trim(); //接收人員
                                //var _ADM_UNIT = (_STATUS == "5") ? _REJ_RSN :
                                //    (MARK_TYPEs.FirstOrDefault(x => x.CODE == _MARK_TYPE2)?.CODE_VALUE ?? _MARK_TYPE2); //申請原因/行政單位
                                var _ADM_UNIT = _REJ_RSN; //申請原因/行政單位
                                //var _apply_memo = oap0021.GetMemoByUserId(new List<string>() { _APPLY_ID });
                                //var _UNIT_CODE_memo = _apply_memo.First().Item3 ; //收件單位
                                //var _SEND_ID_memo = _APPLY_ID; //收件人
                                //應付票據簽收窗口部門檔 有找到簽收窗口部門的資料
                                //var _FNPRD = FNPRD.FirstOrDefault(x => x.division == _UNIT_CODE_memo);
                                //if (_FNPRD != null)
                                //{
                                //    var _FNPR_memo = FNPR.FirstOrDefault(x => x.dep_id == _FNPRD.dep_id);
                                //    _UNIT_CODE_memo = _FNPR_memo.dep_id; //部門檔
                                //    _SEND_ID_memo = _FNPR_memo.apt_id; //窗口人員
                                //}
                                foreach (var item in reportModels.Where(x => x.APPLY_NO == _APPLY_NO))
                                {
                                    //item.UNIT_CODE = _UNIT_CODE_memo; //收件單位
                                    //item.SEND_ID = _SEND_ID_memo; //收件人員
                                    item.APPLY_ID = _APPLY_ID; //申請人
                                    item.ADM_UNIT = _ADM_UNIT; //申請原因/行政單位
                                    item.ENTRY_ID = _RECE_ID; //處理人員
                                    item.STATUS = _STATUS; //應付票據變更申請主檔 狀態
                                }
                            }
                            var _apply_memos = oap0021.GetMemoByUserId(
                                reportModels.Where(x=> FAPPYCH0_apply_nos.Contains(x.APPLY_NO))
                                .Select(x=>x.APPLY_ID).Distinct());
                            foreach (var item in reportModels.Where(x => FAPPYCH0_apply_nos.Contains(x.APPLY_NO)))
                            {
                                var _UNIT_CODE_memo = _apply_memos.First(x=>x.Item1 == item.APPLY_ID).Item3; //收件單位
                                var _SEND_ID_memo = item.APPLY_ID; //收件人
                                //應付票據簽收窗口部門檔 有找到簽收窗口部門的資料
                                var _FNPRD = FNPRD.FirstOrDefault(x => x.division == _UNIT_CODE_memo);
                                if (_FNPRD != null)
                                {
                                    var _FNPR_memo = FNPR.FirstOrDefault(x => x.dep_id == _FNPRD.dep_id);
                                    _UNIT_CODE_memo = _FNPR_memo.dep_id; //部門檔
                                    _SEND_ID_memo = _FNPR_memo.apt_id; //窗口人員
                                }
                                item.UNIT_CODE = _UNIT_CODE_memo; //收件單位
                                item.SEND_ID = _SEND_ID_memo; //收件人員
                            }
                            com.Dispose();
                        }
                        if (!rej_flag.IsNullOrWhiteSpace() && rej_flag == "Y" ) //票據變更退件 條件
                        {
                            reportModels = reportModels.Where(x => x.STATUS == "5").ToList();
                        }
                        else if (!rej_flag.IsNullOrWhiteSpace() && rej_flag == "N") //票據變更非退件 條件
                        {
                            reportModels = reportModels.Where(x => x.STATUS == "4").ToList();
                        }
                        //來源為應付票據變更申請主檔 且不為退件
                        var _FAPPYCD0s = reportModels.Where(x => x.SRCE_FROM == "2" && x.STATUS != "5").ToList();
                        if (_FAPPYCD0s.Any())
                        {
                            using (EacCommand com = new EacCommand(conn))
                            {
                                sql = $@"
select APPLY_NO, CHECK_NO , MARK_TYPE2 from FAPPYCD0
where APPLY_NO in (
";
                                string c = string.Empty;
                                int i = 0;
                                foreach (var item in _FAPPYCD0s.Select(x=>x.APPLY_NO).Distinct())
                                {
                                    sql += $@" {c} :APPLY_NO_{i} ";
                                    com.Parameters.Add($@"APPLY_NO_{i}", item);
                                    c = " , ";
                                    i += 1;
                                }
                                sql += @" ) and  CHECK_NO in ( ";
                                c = string.Empty;
                                i = 0;
                                foreach (var item in _FAPPYCD0s.Select(x => x.CHECK_NO).Distinct())
                                {
                                    sql += $@" {c} :CHECK_NO_{i} ";
                                    com.Parameters.Add($@"CHECK_NO_{i}", item);
                                    c = " , ";
                                    i += 1;
                                }
                                sql += @" ) ;";
                                com.CommandText = sql;
                                com.Prepare();
                                DbDataReader dbresult = com.ExecuteReader();
                                while (dbresult.Read())
                                {
                                    var _APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請(批次)單號
                                    var _CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //送件單位
                                    var _MARK_TYPE2 = dbresult["MARK_TYPE2"]?.ToString()?.Trim(); //目前註記
                                    var _data = reportModels
                                        .FirstOrDefault(x => x.APPLY_NO == _APPLY_NO &&
                                        x.CHECK_NO == _CHECK_NO &&
                                        x.SRCE_FROM == "2" && x.STATUS != "5");
                                    if (_data != null)
                                        _data.ADM_UNIT = (MARK_TYPEs.FirstOrDefault(x => x.CODE == _MARK_TYPE2)?.CODE_VALUE ?? _MARK_TYPE2);
                                }
                                com.Dispose();
                            }
                        }
                    }
                    var _memo = new List<string>();
                    _memo.AddRange(reportModels.Select(x => x.APPLY_ID));
                    _memo.AddRange(reportModels.Select(x => x.ENTRY_ID));
                    _memo.AddRange(reportModels.Select(x => x.SEND_ID));

                    var _UNIT_CODE = oap0021.GetMemoByUserId(_memo.Distinct());
                    var _fullDepName = common.getFullDepName(reportModels.Select(x => x.UNIT_CODE).Distinct());
                    foreach (var item in reportModels)
                    {
                        var _SRCE_FROM = SRCE_FROMs.FirstOrDefault(x => x.CODE == item.SRCE_FROM)?.CODE_VALUE ?? item.SRCE_FROM; //來源(中文)
                        if (_SRCE_FROM.Length > 2)
                        {
                            _SRCE_FROM = _SRCE_FROM.Substring(_SRCE_FROM.Length - 2, 2);
                        }
                        item.SRCE_FROM = _SRCE_FROM;
                        var _APPLY_ID = (_UNIT_CODE.FirstOrDefault(x => x.Item1 == item.APPLY_ID)?.Item2);
                        item.APPLY_ID = (((!_APPLY_ID.IsNullOrWhiteSpace() ? _APPLY_ID : 
                            (FNPR.FirstOrDefault(x => x.apt_id == item.APPLY_ID)?.apt_name)) ?? item.APPLY_ID)); //申請人員(中文)
                        item.ENTRY_ID = _UNIT_CODE.FirstOrDefault(x => x.Item1 == item.ENTRY_ID)?.Item2 ?? item.ENTRY_ID; //處理人員(中文)
                        var _FNPR = FNPR.FirstOrDefault(x => x.dep_id == item.UNIT_CODE);
                        if (_FNPR != null)
                        {
                            var _SEND_ID = (_UNIT_CODE.FirstOrDefault(x => x.Item1 == item.SEND_ID)?.Item2);
                            item.SEND_ID = (!_SEND_ID.IsNullOrWhiteSpace() ? _SEND_ID :
                                _FNPR.apt_name ) ?? _FNPR.apt_id; //收件人員(中文)
                        }
                        else   
                        {
                            item.SEND_ID = (_UNIT_CODE.FirstOrDefault(x => x.Item1 == item.SEND_ID)?.Item2) ?? item.SEND_ID; //收件人員(中文)
                        }
                        var _subdata = subdatas.FirstOrDefault(x => x.check_no == item.CHECK_NO);
                        item.AMOUNT = _subdata?.amount; //票面金額
                        item.RECEIVER = _subdata?.receiver; //付款對象    
                        item.UNIT_CODE = _fullDepName.FirstOrDefault(x=>x.Item1 == item.UNIT_CODE)?.Item2; //收件單位(中文)
                    }
                }
                conn.Dispose();
                conn.Close();
            }
            resultsTable.Tables.Add(reportModels.ToDataTable());
            return resultsTable;
        }

        public class OAP0025ReportModel {

            /// <summary>
            /// 收件單位
            /// </summary>
            public string UNIT_CODE { get; set; }

            /// <summary>
            /// 收件人員
            /// </summary>
            public string SEND_ID { get; set; }

            /// <summary>
            /// 來源
            /// </summary>
            public string SRCE_FROM { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string CHECK_NO { get; set; }

            /// <summary>
            /// 票面金額
            /// </summary>
            public string AMOUNT { get; set; }

            /// <summary>
            /// 支票抬頭
            /// </summary>
            public string RECEIVER { get; set; }

            /// <summary>
            /// 申請(批次)單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 申請人
            /// </summary>
            public string APPLY_ID { get; set; }

            /// <summary>
            /// 申請原因/行政單位
            /// </summary>
            public string ADM_UNIT { get; set; }

            /// <summary>
            /// 處理人員
            /// </summary>
            public string ENTRY_ID { get; set; }

            /// <summary>
            /// 應付票據變更申請主檔 狀態
            /// </summary>
            public string STATUS { get; set; }
        }
    }
}