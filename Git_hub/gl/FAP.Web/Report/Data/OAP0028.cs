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
    public class OAP0028 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();



            string applyNo_S = parms.Where(x => x.key == "applyNo_S").FirstOrDefault()?.value?.Trim() ?? string.Empty; //申請單號(起)
            string applyNo_E = parms.Where(x => x.key == "applyNo_E").FirstOrDefault()?.value?.Trim() ?? string.Empty; //申請單號(迄)


            List<OAP0028ReportModel> reportModels = new List<OAP0028ReportModel>();
            List<OAP0021DetailSubModel> subdatas = new List<OAP0021DetailSubModel>();
            List<SYS_CODE> SYS_CODEs = new List<SYS_CODE>();
            List<FAP_CODE> FAP_CODEs = new List<FAP_CODE>();
            List<string> CODE_TYPEs = new List<string>() {
                "SEND_STYLE"
            };
            using (dbFGLEntities db = new dbFGLEntities())
            {
                SYS_CODEs = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.SYS_CD == "AP" && CODE_TYPEs.Contains(x.CODE_TYPE)).ToList();
                FAP_CODEs = db.FAP_CODE.AsNoTracking().ToList();                  
            }

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;

                using (EacCommand com = new EacCommand(conn))
                {
                    sql = $@"
select SEND_STYLE, CHECK_NO, APPLY_NO, APPLY_UNIT, APPLY_ID, CE_RSN , APLY_NO , APLY_SEQ  from FAPPYEH0 
where 1 = 1 
and STATUS = '3' ";

                    if (!applyNo_S.IsNullOrWhiteSpace())
                    {
                        sql += $@" and APPLY_NO >= :APPLY_NO_S ";
                        com.Parameters.Add($@"APPLY_NO_S", applyNo_S);
                    }
                    if (!applyNo_E.IsNullOrWhiteSpace())
                    {
                        sql += $@" and APPLY_NO <= :APPLY_NO_E ";
                        com.Parameters.Add($@"APPLY_NO_E", applyNo_E);
                    }
                    com.CommandText = sql;
                    com.Prepare();
                    DbDataReader dbresult = com.ExecuteReader();

                    while (dbresult.Read())
                    {
                        var model = new OAP0028ReportModel();
                        //model.SEND_STYLE = dbresult["SEND_STYLE"]?.ToString()?.Trim(); //寄送方式
                        model.CHECK_NO = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼(抽)
                        model.APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                        model.APPLY_UNIT = dbresult["APPLY_UNIT"]?.ToString()?.Trim(); //申請單位
                        model.APPLY_ID = dbresult["APPLY_ID"]?.ToString()?.Trim(); //申請人
                        model.CE_RSN = dbresult["CE_RSN"]?.ToString()?.Trim(); //抽票原因
                        model.APLY_NO = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                        model.APLY_SEQ = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                        reportModels.Add(model);
                        if (!model.CHECK_NO.IsNullOrWhiteSpace())
                            subdatas.Add(new OAP0021DetailSubModel() { check_no = model.CHECK_NO });
                    }
                    com.Dispose();
                }
                if (reportModels.Any())
                {
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                    select APLY_NO , APLY_SEQ , SEND_STYLE  from FGLGPCK0
                    where APLY_NO in (
";
                        int i = 0;
                        string c = string.Empty;
                        foreach (var item in reportModels.Select(x => x.APLY_NO).Distinct())
                        {
                            sql += $@" {c} :APLY_NO_{i} ";
                            com.Parameters.Add($@"APLY_NO_{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += @" ) and APLY_SEQ in ( ";
                        i = 0;
                        c = string.Empty;
                        foreach (var item in reportModels.Select(x => x.APLY_SEQ).Distinct())
                        {
                            sql += $@" {c} :APLY_SEQ_{i} ";
                            com.Parameters.Add($@"APLY_SEQ_{i}", item);
                            c = " , ";
                            i += 1;
                        }
                        sql += " ); ";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();

                        while (dbresult.Read())
                        {
                            var _APLY_NO = dbresult["APLY_NO"]?.ToString()?.Trim(); //付款申請編號
                            var _APLY_SEQ = dbresult["APLY_SEQ"]?.ToString()?.Trim(); //付款申請序號
                            var _SEND_STYLE = dbresult["SEND_STYLE"]?.ToString()?.Trim(); //寄送方式
                            foreach (var item in reportModels.Where(x => x.APLY_NO == _APLY_NO && x.APLY_SEQ == _APLY_SEQ))
                            {
                                item.SEND_STYLE = _SEND_STYLE;
                            }
                        }
                        com.Dispose();
                    }

                    if (subdatas.Any())
                    {
                        new OAP0021().getSubData(subdatas);
                    }

                    var _OAP0029 = new OAP0029();
                    var common = new Service.Actual.Common();
                    var userMemo = common.GetMemoByUserId(reportModels.Where(x => !x.APPLY_ID.IsNullOrWhiteSpace()).Select(x => x.APPLY_ID).Distinct());
                    var _fullDepName = common.getFullDepName(reportModels.Where(x => !x.APPLY_UNIT.IsNullOrWhiteSpace()).Select(x => x.APPLY_UNIT).Distinct());
                    foreach (var item in reportModels)
                    {
                        var _apply_id = userMemo.FirstOrDefault(x => x.Item1 == item.APPLY_ID)?.Item2;
                        item.SEND_STYLE = getCodeValue(SYS_CODEs, "SEND_STYLE", item.SEND_STYLE); //寄送方式 (中文)
                        item.APPLY_ID = _apply_id.IsNullOrWhiteSpace() ? item.APPLY_ID : _apply_id; //申請人
                        item.SRCE_FROM = _OAP0029.getScre_FormInApplyNo(item.APPLY_NO);//來源
                        item.APPLY_UNIT = _fullDepName.First(x => x.Item1 == item.APPLY_UNIT).Item2;
                        var _subdatas = subdatas.FirstOrDefault(x => x.check_no == item.CHECK_NO);
                        item.AMOUNT = _subdatas?.amount; //支票面額
                        item.RECEIVER = _subdatas?.receiver; //支票抬頭
                        item.CE_RSN = FAP_CODEs.FirstOrDefault(x => x.reason_code == item.CE_RSN)?.reason ?? item.CE_RSN; //抽票原因
                    }
                }
                conn.Dispose();
                conn.Close();
            }
            resultsTable.Tables.Add(reportModels.ToDataTable());
            return resultsTable;
        }

        public string getCodeValue(List<SYS_CODE> SYS_CODEs,string code_type, string value)
        {
            return (SYS_CODEs.FirstOrDefault(x => x.CODE_TYPE == code_type && x.CODE == value)?.CODE_VALUE) ?? value;
        }

        public class OAP0028ReportModel {

            /// <summary>
            /// 寄送方式
            /// </summary>
            public string SEND_STYLE { get; set; }

            /// <summary>
            /// 來源
            /// </summary>
            public string SRCE_FROM { get; set; }

            /// <summary>
            /// 支票號碼(抽)
            /// </summary>
            public string CHECK_NO { get; set; }

            /// <summary>
            /// 支票面額
            /// </summary>
            public string AMOUNT { get; set; }

            /// <summary>
            /// 支票抬頭
            /// </summary>
            public string RECEIVER { get; set; }

            /// <summary>
            /// 申請單號
            /// </summary>
            public string APPLY_NO { get; set; }

            /// <summary>
            /// 申請單位
            /// </summary>
            public string APPLY_UNIT { get; set; }

            /// <summary>
            /// 申請人
            /// </summary>
            public string APPLY_ID { get; set; }

            /// <summary>
            /// 抽票原因
            /// </summary>
            public string CE_RSN { get; set; }

            /// <summary>
            /// 付款申請編號
            /// </summary>
            public string APLY_NO { get; set; }

            /// <summary>
            /// 付款申請序號
            /// </summary>
            public string APLY_SEQ { get; set; }
        }
    }
}