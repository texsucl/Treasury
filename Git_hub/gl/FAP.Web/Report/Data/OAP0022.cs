using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FAP.Web.Report.Data
{
    public class OAP0022 : ReportData
    {
        public override DataSet GetData(List<reportParm> parms)
        {
            var resultsTable = new DataSet();

            var _Parameters = new List<SqlParameter>();
            var date_s = parms.Where(x => x.key == "APPR_s").FirstOrDefault()?.value?.Trim()?.DPformateTWdate() ?? string.Empty; //覆核日期起
            var date_e = parms.Where(x => x.key == "APPR_e").FirstOrDefault()?.value?.Trim()?.DPformateTWdate() ?? string.Empty; //覆核日期迄
            var rece_id = parms.Where(x => x.key == "RECE_id").FirstOrDefault()?.value?.Trim(); //接收人員
            var report_no = parms.Where(x => x.key == "REPORT_NO").FirstOrDefault()?.value?.Trim(); //表單號碼
            var check_no = parms.Where(x => x.key == "CHECK_NO").FirstOrDefault()?.value?.Trim(); //支票號碼
            string result = parms.Where(x => x.key == "results").FirstOrDefault()?.value ?? string.Empty;
            List<string> _datas = JsonConvert.DeserializeObject<List<string>>(result);
            List<OAP0021DetailSubModel> data_1 = new List<OAP0021DetailSubModel>();
            List<OAP0022ReportModel2> data_2 = new List<OAP0022ReportModel2>();
            List<FAP_FAPPYCD0_REPORT> data_3 = new List<FAP_FAPPYCD0_REPORT>();
            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn.Open();
                string sql = string.Empty;
                using (EacCommand com = new EacCommand(conn))
                {
                    if (_datas.Any())
                    {
                        sql = $@"
                    select APPLY_NO,CHECK_NO,APPLY_SEQ,NEW_HEAD,MARK_TYPE2
                    from FAPPYCD0
                    where APPLY_NO in (
                    ";
                        string c = string.Empty;
                        int i = 0;
                        //foreach (var item in _datas)
                        //{
                        //    //sql += $@" {c}  ( CHECK_NO = :check_no_{i} and  APPLY_NO = :apply_no_{i} ) ";
                        //    sql += $@" {c}  (  APPLY_NO = :apply_no_{i} ) ";
                        //    //com.Parameters.Add($@"check_no_{i}", item.check_no);
                        //    com.Parameters.Add($@"apply_no_{i}", item.apply_no);
                        //    c = " OR ";
                        //    i += 1;
                        //}
                        foreach (var item in _datas)
                        {
                            sql += $@" {c} :apply_no_{i} ";
                            com.Parameters.Add($@"apply_no_{i}", item);
                            c = ",";
                            i += 1;
                        }
                        sql += " ) ;";
                        //if (!check_no.IsNullOrWhiteSpace())
                        //{
                        //    sql += @" and CHECK_NO = :check_no ";
                        //    com.Parameters.Add($@"check_no", check_no);
                        //}
                        //sql += " ;";
                        com.CommandText = sql;
                        com.Prepare();
                        DbDataReader dbresult = com.ExecuteReader();
                        bool _check_no_Flog = check_no.IsNullOrWhiteSpace();
                        while (dbresult.Read())
                        {
                            var _apply_no = dbresult["APPLY_NO"]?.ToString()?.Trim(); //申請單號
                            var _check_no = dbresult["CHECK_NO"]?.ToString()?.Trim(); //支票號碼
                            var _apply_seq = dbresult["APPLY_SEQ"]?.ToString()?.Trim(); //申請序號
                            var _new_head = dbresult["NEW_HEAD"]?.ToString()?.Trim(); //新支票抬頭
                            var _mark_type2 = dbresult["MARK_TYPE2"]?.ToString()?.Trim(); //註記
                            if (_check_no_Flog || check_no == _check_no)
                                data_1.Add(new OAP0021DetailSubModel()
                                {
                                    check_no = _check_no, //支票號碼
                                    apply_no = _apply_no, //申請編號
                                    aply_seq = _apply_seq, //付款申請序號
                                    new_head = _new_head, //新支票抬頭
                                    mark_type2 = _mark_type2, //註記
                                });
                        }
                    }
                    com.Dispose();
                }
                if (data_1.Any())
                {
                    new Service.Actual.OAP0021().getSubData(data_1);
                    using (EacCommand com = new EacCommand(conn))
                    {
                        sql = $@"
                    select APPLY_NO,APPR_DATE2,APPR_TIME2
                    from FAPPYCH0
                    where STATUS in  ('8','4') ";
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
                        if (!rece_id.IsNullOrWhiteSpace())
                        {
                            sql += " and  RECE_ID = :RECE_ID ";
                            com.Parameters.Add("RECE_ID", rece_id);
                        }
                        sql += $@"and APPLY_NO in ( ";
                        string c = string.Empty;
                        int i = 0;
                        foreach (var item in data_1.Select(x => x.apply_no).Distinct())
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
                            data_2.Add(new OAP0022ReportModel2()
                            {
                                APPLY_NO = dbresult["APPLY_NO"]?.ToString()?.Trim(), //申請單號
                                APPR_DATE2 = dbresult["APPR_DATE2"]?.ToString()?.Trim(), //覆核日期
                                APPR_TIME2 = dbresult["APPR_TIME2"]?.ToString()?.Trim(), //覆核時間
                            });
                        }
                    }
                }
                conn.Dispose();
                conn.Close();
            }
            var _mark_types = new SysCodeDao().qryByType("AP", "MARK_TYPE");
            List<OAP0022ReportModel> reportModels = new List<OAP0022ReportModel>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                var _apply_no = data_1.Select(x => x.apply_no).ToList();
                data_3 = db.FAP_FAPPYCD0_REPORT.AsNoTracking()
                    .Where(x => _apply_no.Contains(x.apply_no)).ToList();
            }

            foreach (var item in data_2)
            {
                reportModels.AddRange(data_1.Where(x => x.apply_no == item.APPLY_NO)
                    .Select(x => new OAP0022ReportModel()
                    {
                        APPRDT = item.APPR_DATE2,
                        APPRTM = item.APPR_TIME2,
                        MARK_TYPE = x.mark_type2,
                        CHECK_NO = x.check_no,
                        CHECK_AMOUNT = x.amount,
                        CHECK_HEAD = x.receiver,
                        NEW_HEAD = x.new_head,
                        APPLYSEQ = x.aply_seq,
                        REPORT_NO = data_3.FirstOrDefault(y=>y.apply_no == x.apply_no)?.report_no
                    }));
            }
                                              
            List<OAP0022ReportModel> fixedModel = new List<OAP0022ReportModel>();
            foreach (var item in reportModels.GroupBy(x => new {x.APPRDT,x.MARK_TYPE,x.REPORT_NO })
                .OrderBy(x => x.Key.REPORT_NO).ThenByDescending(x=>x.Key.APPRDT).ThenBy(x => x.Key.MARK_TYPE))
            {
                fixedModel.Add(new OAP0022ReportModel()
                {
                    APPRDT = item.Key.APPRDT.stringTWDateFormate(),
                    CHECK_NO = $@"【{_mark_types.FirstOrDefault(x=> x.CODE == item.Key.MARK_TYPE)?.CODE_VALUE}】",
                    REPORT_NO = item.Key.REPORT_NO,
                    GroupId = "0",
                });
                var _MARK_TYPE = 0;
                Int32.TryParse(item.Key.MARK_TYPE, out _MARK_TYPE);
                fixedModel.Add(new OAP0022ReportModel()
                {
                    APPRDT = item.Key.APPRDT.stringTWDateFormate(),
                    CHECK_NO = "支票號碼",
                    CHECK_AMOUNT = "支票面額",
                    CHECK_HEAD = "支票抬頭",
                    NEW_HEAD = _MARK_TYPE >= 3 ? "新抬頭" : string.Empty,
                    GroupId = "1",
                    REPORT_NO = item.Key.REPORT_NO,
                });
                foreach (var i in item.OrderByDescending(x=>x.APPRTM).ThenBy(x=>x.APPLYSEQ))
                {
                    fixedModel.Add(new OAP0022ReportModel()
                    {
                        APPRDT = item.Key.APPRDT.stringTWDateFormate(),
                        CHECK_NO = i.CHECK_NO,
                        CHECK_AMOUNT = i.CHECK_AMOUNT,
                        CHECK_HEAD = i.CHECK_HEAD,
                        NEW_HEAD = i.NEW_HEAD,
                        GroupId = "2",
                        REPORT_NO = item.Key.REPORT_NO,
                    });
                }
                fixedModel.Add(new OAP0022ReportModel()
                {
                    APPRDT = item.Key.APPRDT.stringTWDateFormate(),
                    CHECK_NO = $@"共計 : {item.Count()} 張",
                    GroupId = "3",
                    REPORT_NO = item.Key.REPORT_NO,
                });
            }
            resultsTable.Tables.Add(fixedModel.ToDataTable());
            return resultsTable;
        }

        public class OAP0022ReportModel {

            /// <summary>
            /// 表單號碼
            /// </summary>
            public string REPORT_NO { get; set; }

            /// <summary>
            /// 覆核日期
            /// </summary>
            public string APPRDT { get; set; }

            /// <summary>
            /// 覆核時間
            /// </summary>
            public string APPRTM { get; set; }

            /// <summary>
            /// 申請序號
            /// </summary>
            public string APPLYSEQ { get; set; }

            /// <summary>
            /// 註記
            /// </summary>
            public string MARK_TYPE { get; set; }

            /// <summary>
            /// 支票號碼
            /// </summary>
            public string CHECK_NO { get; set; }

            /// <summary>
            /// 支票面額
            /// </summary>
            public string CHECK_AMOUNT { get; set; }

            /// <summary>
            /// 支票抬頭
            /// </summary>
            public string CHECK_HEAD { get; set; }

            /// <summary>
            /// 新抬頭
            /// </summary>
            public string NEW_HEAD { get; set; }

            public string GroupId { get; set; }
        }

        public class OAP0022ReportModel2 {

            public string APPLY_NO { get; set; }

            public string APPR_DATE2 { get; set; }

            public string APPR_TIME2 { get; set; }

            public string MARK_TYPE2 { get; set; }

            public string NEW_HEAD { get; set; }
        }
    }
}