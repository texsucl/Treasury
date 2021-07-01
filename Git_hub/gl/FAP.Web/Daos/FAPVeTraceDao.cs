
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;


/// <summary>
/// 功能說明：FAP_VE_TRACE 逾期未兌領清理記錄檔
/// 初版作者：20190614 Daiyu
/// 修改歷程：20190614 Daiyu
///           需求單號：201905310556-01
///           初版
/// ---------------------------------------------------------------------
/// 修改歷程：20191025 daiyu
/// 需求單號：201910240295-00
///           1.新增FAP_VE_TRACE時，若保局範圍沒有指定...以"0"寫入
///           2.【OAP0004 指定保局範圍作業】若查詢條件"保局範圍=0"時，要可以查到
///            2.1 null值
///            2.2 空白
///            2.3 "0"
/// ---------------------------------------------------------------------
/// 修改歷程：20200812 Daiyu
/// 需求單號：
/// 修改內容：1.修改AS400寫清理紀錄檔方式
///           2.【OAP0042 電訪暨簡訊標準設定作業】報表查詢
///           3.【OAP0054 不同清理狀態查詢】
/// ---------------------------------------------------------------------
/// 修改歷程：20210118 Daiyu
/// 需求單號：
/// 修改內容：1.調整OAP0016件數計算邏輯
///           2.OAP0014加用給付日期/清理日期查詢
/// ----------------------------------------------------
/// 修改歷程：20210208 daiyu
/// 需求單號：202101280283-00
/// 修改內容：【OAP0042 電訪暨簡訊標準設定作業】設定項目屬"電話訪問"時，增加可篩選"支票號碼"或"給付對象ID"挑錄特定的支票。
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTraceDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<OAP0054Model> qryForOAP0054(string[] status_arr
         , SqlConnection conn)
        {

            List<OAP0054Model> rows = new List<OAP0054Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
select distinct
  main.paid_id
 ,main.check_no
 ,main.check_acct_short
 ,main.check_date
 ,main.check_amt
 ,poli.o_paid_cd
 ,main.system
 ,main.status
from FAP_VE_TRACE main left join fap_ve_trace_poli poli on main.check_no = poli.check_no and main.check_acct_short = poli.check_acct_short
where main.paid_id in (
select tmp.paid_id from (
SELECT distinct m.paid_id, m.status
  FROM FAP_VE_TRACE m
where (m.paid_id <> '' and m.paid_id is not null)
";
                
                //組合"清理狀態"條件
                sql += " AND m.status IN ( ";
                foreach (string d in status_arr)
                {
                    sql += "'" + d + "',";

                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";

                sql += @" ) tmp";
  //組合"清理狀態"條件
                sql += " where tmp.status IN ( ";
                foreach (string d in status_arr)
                {
                    sql += "'" + d + "',";

                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";
                sql += @" group by tmp.paid_id
having count(*) > 1) ";
//組合"清理狀態"條件
                sql += " and main.status IN ( ";
                foreach (string d in status_arr)
                {
                    sql += "'" + d + "',";

                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";
                sql += @" order by main.paid_id, main.status ";

                cmd.CommandText = sql;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0054Model d = new OAP0054Model();
                        d.paid_id = dr["paid_id"].ToString();
                        d.check_no = dr["check_no"].ToString();
                        d.check_acct_short = dr["check_acct_short"].ToString();
                        d.check_date = DateUtil.ADDateToChtDate(DateUtil.stringToDatetime(dr["check_date"].ToString()), 3, "/");
                        d.check_amt = Convert.ToInt64(dr["check_amt"]);
                        d.system = dr["system"].ToString();
                        d.o_paid_cd = dr["o_paid_cd"].ToString();
                        d.status = dr["status"].ToString();

                        rows.Add(d);
                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }




        public List<TelDispatchRptModel> qryByForOAP0042Fsc(string[] fsc_range, string rpt_cnt_tp)
        {
            List<TelDispatchRptModel> rows = new List<TelDispatchRptModel>();
            if (fsc_range.Length == 0)
                return rows;

            try
            {

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    SqlCommand cmd = conn.CreateCommand();

                    cmd.Connection = conn;
                    cmd.Parameters.Clear();

                    string sql = @"
SELECT fsc_range, count(*) cnt, sum(amt) amt
 from (
 SELECT fsc_range, stat_id, sum(check_amt) amt
 from (
 SELECT fsc_range,
	   check_amt,
	   CASE WHEN @cnt_type = 'P' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID
  FROM [FAP_VE_TRACE]
 WHERE 1 = 1
";

                    //組合"保局範圍"條件
                    sql += " AND fsc_range IN ( ";
                    foreach (string d in fsc_range)
                    {
                        if (!"".Equals(d))
                            sql += "'" + d + "',";
                    }
                    sql = sql.Substring(0, sql.Length - 1);
                    sql += ") ";


                    string strGroup = @"  ) m
  group by fsc_range, stat_id) rpt
group by fsc_range
order by len(fsc_range), fsc_range";

                    sql += strGroup;

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@cnt_type", rpt_cnt_tp);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            TelDispatchRptModel d = new TelDispatchRptModel();
                            d.fsc_range = dr["fsc_range"]?.ToString() ;
                            d.cnt = Convert.ToInt64(dr["cnt"]);
                            d.amt = Convert.ToInt64(dr["amt"]);

                            rows.Add(d);
                        }
                    }
                }
                   

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 『OAP0042 電訪暨簡訊標準設定作業』報表
        /// </summary>
        /// <param name="status"></param>
        /// <param name="o_paid_cd"></param>
        /// <param name="tel_appr_result"></param>
        /// <returns></returns>
        public List<TelDispatchRptModel> qryByForOAP0042(string[] status, string[] o_paid_cd,  string[] tel_appr_result, string[] sms_status
            , string rpt_cnt_tp, string type, string paid_id, string check_no)
        {

            try
            {//清理狀態
                bool bStatus = false;
                if (status == null)
                {
                    bStatus = true;
                    status = new string[] { "" };
                }


                //原給付性質
                bool bOPaidCd = false;
                if (o_paid_cd == null)
                {
                    bOPaidCd = true;
                    o_paid_cd = new string[] { "" };
                }


                //電訪覆核結果
                bool bTelApprResult = false;
                if (tel_appr_result == null)
                {
                    bTelApprResult = true;
                    tel_appr_result = new string[] { "" };
                }

                //簡訊狀態
                bool bSmsStatus = false;
                if (sms_status == null)
                {
                    bSmsStatus = true;
                    sms_status = new string[] { "" };
                }

                //給付對象ID    add by daiyu 20210208
                bool bPaidId = false;
                paid_id = StringUtil.toString(paid_id);
                if ("".Equals(StringUtil.toString(paid_id)))
                    bPaidId = true;

                //支票號碼    add by daiyu 20210208
                bool bCheckNo = false;
                check_no = StringUtil.toString(check_no);
                if ("".Equals(StringUtil.toString(check_no)))
                    bCheckNo = true;

                logger.Info("query begin!!");


                using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
                {
                    using (dbFGLEntities db = new dbFGLEntities())
                    {
                        var rows = (from m in db.FAP_VE_TRACE
                                    join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                    from xPoli in psPoli.DefaultIfEmpty()

                                    join tel in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" & x.tel_std_type == type) on new { m.check_no, m.check_acct_short } equals new { tel.check_no, tel.check_acct_short } into psTel
                                    from xTel in psTel.DefaultIfEmpty()

                                    join telInterview in db.FAP_TEL_INTERVIEW on xTel.tel_proc_no equals telInterview.tel_proc_no into psTelInterview
                                    from xTelInterview in psTelInterview.DefaultIfEmpty()

                                    where 1 == 1
                                       & (bStatus || (!bStatus & !status.Contains(m.status)))
                                       & (bOPaidCd || (!bOPaidCd & !o_paid_cd.Contains(xPoli.o_paid_cd)))

                                       & (bPaidId || (!bPaidId & m.paid_id.Equals(paid_id)))   //add by daiyu 20210208
                                       & (bCheckNo || (!bCheckNo & m.check_no.Equals(check_no)))   //add by daiyu 20210208

                                    //& (bTelApprResult || (!bTelApprResult & tel_appr_result.Contains(xTelInterview.tel_appr_result)))
                                    // & (bSmsStatus || (!bSmsStatus & sms_status.Contains(xTel.sms_status)))

                                    select new TelDispatchRptModel
                                    {
                                        temp_id = rpt_cnt_tp == "P" ? (m.paid_id == "" ? m.check_no : m.paid_id) : m.check_no,
                                        fsc_range = m.fsc_range,
                                        check_no = m.check_no,
                                        check_acct_short = m.check_acct_short,
                                        check_amt = m.check_amt == null ? (Decimal)0 : (Decimal)m.check_amt,
                                        main_amt = xPoli.main_amt == null ? (Decimal)0 : (Decimal)xPoli.main_amt,
                                        check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                      SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                      SqlFunctions.DateName("day", m.check_date).Trim(),
                                        o_paid_cd = xPoli.o_paid_cd,
                                        paid_id = m.paid_id,
                                        paid_name = m.paid_name,
                                        system = m.system,
                                        policy_no = xPoli.policy_no,
                                        policy_seq = xPoli == null ? (int)0 : xPoli.policy_seq,
                                        id_dup = xPoli.id_dup,
                                        change_id = xPoli.change_id == null ? "" : xPoli.change_id,
                                        tel_appr_result = xTelInterview.tel_appr_result,
                                        sms_status = xTel.sms_status,
                                        dispatch_status = xTel == null ? "" : xTel.dispatch_status
                                    }).ToList();

                        logger.Info("query end!!");

                        //rows = rows.Where(x => (bTelApprResult || (!bTelApprResult & !tel_appr_result.Contains(x.tel_appr_result)))
                        //               & (bSmsStatus || (!bSmsStatus & !sms_status.Contains(x.sms_status)))).ToList();

                        //add by daiyu 20201209
                        if ("tel_assign_case".Equals(type))
                            rows = rows.Where(x => x.dispatch_status != "1").ToList();

                        return rows.Where(x => (bTelApprResult || (!bTelApprResult & !tel_appr_result.Contains(x.tel_appr_result)))
                                       & (bSmsStatus || (!bSmsStatus & !sms_status.Contains(x.sms_status)))).ToList();

                    }
                }

            }
            catch (Exception e) {
                logger.Error(e.ToString());
                throw e;
            }

        }



        public int insertForOAP0050(FAP_VE_TRACE d, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE
 (system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,paid_name
 ,check_amt
 ,check_date
 ,re_paid_date
 ,re_paid_type
 ,fsc_range
 ,status
 ,update_id
 ,update_datetime
 ,data_status
 ,re_paid_date_n
 ,as400_send_cnt
 ,stage_status
 ) values (
  @system
 ,@check_no
 ,@check_acct_short
 ,@paid_id
 ,@paid_name
 ,@check_amt
 ,@check_date
 ,@re_paid_date
 ,@re_paid_type
 ,@fsc_range
 ,@status
 ,@update_id
 ,@update_datetime
 ,@data_status
 ,@re_paid_date_n
 ,@as400_send_cnt
 ,@stage_status
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)d.system ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_id", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_name", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_name ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.check_amt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_date", System.Data.SqlDbType.DateTime).Value = (Object)d.check_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@re_paid_type", System.Data.SqlDbType.VarChar).Value = (Object)d.re_paid_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@fsc_range", System.Data.SqlDbType.VarChar).Value = (Object)d.fsc_range ?? "0";
                cmd.Parameters.AddWithValue("@status", System.Data.SqlDbType.VarChar).Value = (Object)d.status ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date_n ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@as400_send_cnt", 0);
                cmd.Parameters.AddWithValue("@stage_status", "1");

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int insert(FAP_VE_TRACE d, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE
 (system
 ,check_no
 ,check_acct_short
 ,paid_id
 ,paid_name
 ,check_amt
 ,check_date
 ,re_paid_date
 ,re_paid_type
 ,fsc_range
 ,status
 ,cert_doc_1
 ,exec_date_1
 ,practice_1
 ,cert_doc_2
 ,exec_date_2
 ,practice_2
 ,cert_doc_3
 ,exec_date_3
 ,practice_3
 ,cert_doc_4
 ,exec_date_4
 ,practice_4
 ,cert_doc_5
 ,exec_date_5
 ,practice_5
 ,proc_desc
 ,update_id
 ,update_datetime
 ,data_status
 ,exec_date
 ,re_paid_date_n
 ,paid_code
 ,as400_send_cnt
 ) values (
  @system
 ,@check_no
 ,@check_acct_short
 ,@paid_id
 ,@paid_name
 ,@check_amt
 ,@check_date
 ,@re_paid_date
 ,@re_paid_type
 ,@fsc_range
 ,@status
 ,@cert_doc_1
 ,@exec_date_1
 ,@practice_1
 ,@cert_doc_2
 ,@exec_date_2
 ,@practice_2
 ,@cert_doc_3
 ,@exec_date_3
 ,@practice_3
 ,@cert_doc_4
 ,@exec_date_4
 ,@practice_4
 ,@cert_doc_5
 ,@exec_date_5
 ,@practice_5
 ,@proc_desc
 ,@update_id
 ,@update_datetime
 ,@data_status
 ,@exec_date
 ,@re_paid_date_n
 ,@paid_code
 ,@as400_send_cnt
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)d.system ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_id", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_name", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_name ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.check_amt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_date", System.Data.SqlDbType.DateTime).Value = (Object)d.check_date  ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date ?? DBNull.Value ;
                cmd.Parameters.AddWithValue("@re_paid_type", System.Data.SqlDbType.VarChar).Value = (Object)d.re_paid_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@fsc_range", System.Data.SqlDbType.VarChar).Value = (Object)d.fsc_range ?? "0";    //add by daiyu 20191025
                cmd.Parameters.AddWithValue("@status", System.Data.SqlDbType.VarChar).Value = (Object)d.status  ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_1", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_1", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_1", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_1 ??  DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_2", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_2 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_2", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_2 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_2", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_2 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_3", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_3 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_3", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_3 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_3", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_3 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_4", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_4 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_4", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_4 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_4", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_4 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_5", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_5 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_5", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_5 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_5", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_5 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@proc_desc", System.Data.SqlDbType.VarChar).Value = (Object)d.proc_desc ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.DateTime).Value = d.exec_date == null ? DBNull.Value : (Object)d.exec_date;

                cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date_n ?? DBNull.Value;    //add by dayiu 20200924
                cmd.Parameters.AddWithValue("@paid_code", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_code ?? DBNull.Value;   //add by dayiu 20200924
                cmd.Parameters.AddWithValue("@as400_send_cnt", System.Data.SqlDbType.Int).Value = (Object)d.as400_send_cnt ?? 0;    //add by daiyu 20201123

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 異動AS400踐行程序的過程說明
        /// </summary>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="proc_desc"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateProcDesc(string check_no, string check_acct_short, string proc_desc
            , SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
UPDATE FAP_VE_TRACE
  SET proc_desc = @proc_desc
WHERE check_no = @check_no
  AND check_acct_short = @check_acct_short
 
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                //cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)system ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)check_acct_short ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@proc_desc", System.Data.SqlDbType.VarChar).Value = (Object)proc_desc ?? DBNull.Value;
             
                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 異動踐行程序一~五
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updaetForOAP0008(FAP_VE_TRACE d, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
UPDATE FAP_VE_TRACE
  SET 
  cert_doc_1 = @cert_doc_1
 ,exec_date_1 = @exec_date_1
 ,practice_1 = @practice_1
 ,cert_doc_2 = @cert_doc_2
 ,exec_date_2 = @exec_date_2
 ,practice_2 = @practice_2
 ,cert_doc_3 = @cert_doc_3
 ,exec_date_3 = @exec_date_3
 ,practice_3 = @practice_3
 ,cert_doc_4 = @cert_doc_4
 ,exec_date_4 = @exec_date_4
 ,practice_4 = @practice_4
 ,cert_doc_5 = @cert_doc_5
 ,exec_date_5 = @exec_date_5
 ,practice_5 = @practice_5
 ,as400_send_cnt = @as400_send_cnt
WHERE system = @system
  AND check_no = @check_no
  AND check_acct_short = @check_acct_short
 
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)d.system ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
               
                cmd.Parameters.AddWithValue("@cert_doc_1", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_1", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_1", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_1 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_2", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_2 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_2", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_2 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_2", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_1 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_3", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_3 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_3", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_3 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_3", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_3 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_4", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_4 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_4", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_4 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_4", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_4 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@cert_doc_5", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc_5 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date_5", System.Data.SqlDbType.DateTime).Value = (Object)d.exec_date_5 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_5", System.Data.SqlDbType.VarChar).Value = (Object)d.practice_5 ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@as400_send_cnt", System.Data.SqlDbType.Int).Value = (Object)d.as400_send_cnt ?? 0;    //add by daiyu 20201123

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        





        public List<OAP0015Model> qryForOAP0015(string[] fsc_range_arr, string date_e, string[] status_arr
            , SqlConnection conn)
        {

            List<OAP0015Model> rows = new List<OAP0015Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT m.system,
       m.check_date,
       m.check_no, 
       m.check_acct_short,
       m.check_amt,
       m.rpt_status,
       d.policy_no,
       d.policy_seq,
       d.id_dup, 
       d.main_amt
FROM (
SELECT system,
       check_date,
	   check_no,
       check_acct_short,
	   check_amt,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN exec_date <= @date_e THEN '3'
			WHEN exec_date > @date_e THEN '4'
			ELSE '4'
	   END AS rpt_status 
  FROM FAP_VE_TRACE
WHERE 1 = 1
  AND (re_paid_date > @date_e OR re_paid_date is null)
";
                //組合"統計起迄區間"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"保局範圍"條件
                sql += " AND fsc_range IN ( ";
                foreach (string d in fsc_range_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";

                sql += " ) m JOIN [FAP_VE_TRACE_POLI] d ON m.check_no = d.check_no and m.check_acct_short = d.check_acct_short";
                sql += " WHERE 1 = 1 ";


                //組合"清理狀態"條件
                sql += " AND rpt_status IN ( ";
                foreach (string d in status_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";

                sql += " ORDER BY m.check_date, m.check_no, d.policy_no, d.policy_seq ";

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@date_e", date_e);



                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0015Model d = new OAP0015Model();
                        d.check_date = dr["check_date"].ToString();
                        d.system = dr["system"].ToString();
                        d.policy_no = dr["policy_no"].ToString();
                        d.policy_seq = dr["policy_seq"].ToString();
                        d.id_dup = dr["id_dup"].ToString();
                        d.check_no = dr["check_no"].ToString();
                        d.check_acct_short = dr["check_acct_short"].ToString();
                        d.check_amt = Convert.ToInt64(dr["check_amt"]).ToString();
                        d.main_amt = Convert.ToInt64(dr["main_amt"]).ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        

                        rows.Add(d);

                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<OAP0014Model> qryForOAP0014FscRange(string[] fsc_range_arr, string date_e, string status
           , string[] level_1_arr, string[] level_2_arr, string cnt_type, bool levelSpace, SqlConnection conn)
        {
        
            bool bStatus = StringUtil.toString(status) == "" ? true : false;



            List<OAP0014Model> rows = new List<OAP0014Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT fsc_range, rpt_status, count(*) cnt, sum(amt) amt
from (
SELECT fsc_range, rpt_status, stat_id, sum(check_amt) amt
from (
SELECT fsc_range,
       re_paid_date,
	   closed_date,
	   exec_date,
	   status,
	   check_no,
       check_acct_short,
	   paid_id,
	   check_amt,
	   CASE WHEN @cnt_type = 'paid_id' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN exec_date <= @date_e THEN '3'
			WHEN exec_date > @date_e THEN '4'
			ELSE '4'
	   END AS rpt_status 
  FROM [FAP_VE_TRACE]
WHERE 1 = 1
";
                //組合"統計截止日"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"保局範圍"條件
                sql += " AND fsc_range IN ( ";
                foreach (string d in fsc_range_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                if (levelSpace)
                {
                    sql += " AND (((level_1 = '' or level_1 is null) and (level_2 = '' or level_2 is null)) OR (";
                }
                else
                    sql += " AND ((1 = 2) OR (";

                //組合"清理大類"條件
                sql += "  level_1 IN ( ";
                foreach (string d in level_1_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                //組合"清理小類"條件
                sql += " AND level_2 IN ( ";
                foreach (string d in level_2_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "))) ";

                string strGroup = @"  ) m
  group by fsc_range, stat_id, rpt_status) rpt
group by fsc_range, rpt_status
order by len(fsc_range), fsc_range";

                sql += strGroup;

                cmd.CommandText = sql;


   
                cmd.Parameters.AddWithValue("@date_e", date_e);
                cmd.Parameters.AddWithValue("@cnt_type", cnt_type);


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0014Model d = new OAP0014Model();
                        d.check_ym = dr["fsc_range"].ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        d.cnt = dr["cnt"].ToString();
                        d.amt = Convert.ToInt64(dr["amt"]).ToString();

                        //組合"清理狀態"條件
                        if (!bStatus)
                        {
                            if (status.Equals(dr["rpt_status"].ToString()))
                                rows.Add(d);
                        }
                        else
                            rows.Add(d);
                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// "OAP0014 應付未付每月分析表"查詢
        /// </summary>
        /// <param name="check_ym_b"></param>
        /// <param name="check_ym_e"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <param name="status"></param>
        /// <param name="level_1_arr"></param>
        /// <param name="level_2_arr"></param>
        /// <param name="cnt_type"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<OAP0014Model> qryForOAP0014RepaidYm(string repaid_ym_b, string repaid_ym_e, string date_b, string date_e, string status
            , string[] level_1_arr, string[] level_2_arr, string cnt_type, bool levelSpace, SqlConnection conn)
        {
            bool bDateB = StringUtil.toString(date_b) == "" ? true : false;
            bool bDateE = StringUtil.toString(date_e) == "" ? true : false;
            bool bStatus = StringUtil.toString(status) == "" ? true : false;

            repaid_ym_b = repaid_ym_b + "-01";
            repaid_ym_e = Convert.ToDateTime(repaid_ym_e + "-01").AddMonths(1).ToString("yyyy-MM-dd");

            List<OAP0014Model> rows = new List<OAP0014Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT check_ym, rpt_status, count(*) cnt, sum(amt) amt
from (
SELECT check_ym, rpt_status, stat_id, sum(check_amt) amt
from (
SELECT case when rpt_status = '1' then repaid_ym else closed_ym end as check_ym, rpt_status, stat_id, check_amt
from (
SELECT convert(varchar(7), check_date, 111) as check_ym,
       convert(varchar(7), re_paid_date, 111) as repaid_ym,
	   convert(varchar(7), closed_date, 111) as closed_ym,
	   exec_date,
	   status,
	   check_no,
       check_acct_short,
	   paid_id,
	   check_amt,
	   CASE WHEN @cnt_type = 'paid_id' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN exec_date <= @date_e THEN '3'
			WHEN exec_date > @date_e THEN '4'
			ELSE '4'
	   END AS rpt_status 
  FROM [FAP_VE_TRACE]
WHERE 1 = 1
";
                //組合"統計截止日"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"給付年月"條件
                sql += " AND ((re_paid_date >= @repaid_ym_b AND re_paid_date < @repaid_ym_e) OR (closed_date >= @repaid_ym_b AND closed_date < @repaid_ym_e))";
                sql += " AND (re_paid_date <= @date_e OR closed_date <= @date_e)";

                if (levelSpace)
                {
                    sql += " AND (((level_1 = '' or level_1 is null) and (level_2 = '' or level_2 is null)) OR (";
                }
                else
                    sql += " AND ((1 = 2) OR (";

                //組合"清理大類"條件
                sql += "  level_1 IN ( ";
                foreach (string d in level_1_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                //組合"清理小類"條件
                sql += " AND level_2 IN ( ";
                foreach (string d in level_2_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "))) ";

                string strGroup = @"  ) tmp ) m
  group by check_ym, stat_id, rpt_status) rpt
group by check_ym, rpt_status";

                sql += strGroup;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@repaid_ym_b", repaid_ym_b);
                cmd.Parameters.AddWithValue("@repaid_ym_e", repaid_ym_e);
                cmd.Parameters.AddWithValue("@date_e", date_e);
                cmd.Parameters.AddWithValue("@cnt_type", cnt_type);


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0014Model d = new OAP0014Model();
                        d.check_ym = dr["check_ym"].ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        d.cnt = dr["cnt"].ToString();
                        d.amt = Convert.ToInt64(dr["amt"]).ToString();

                        //組合"清理狀態"條件
                        if (!bStatus)
                        {
                            if(status.Equals(dr["rpt_status"].ToString()))
                                rows.Add(d);
                        } else
                            rows.Add(d);
                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<OAP0014Model> qryForOAP0014CheckYm(string check_ym_b, string check_ym_e, string date_b, string date_e, string status
            , string[] level_1_arr, string[] level_2_arr, string cnt_type, bool levelSpace, SqlConnection conn)
        {
            bool bDateB = StringUtil.toString(date_b) == "" ? true : false;
            bool bDateE = StringUtil.toString(date_e) == "" ? true : false;
            bool bStatus = StringUtil.toString(status) == "" ? true : false;

            check_ym_b = check_ym_b + "-01";
            check_ym_e = Convert.ToDateTime(check_ym_e + "-01").AddMonths(1).ToString("yyyy-MM-dd");

            List<OAP0014Model> rows = new List<OAP0014Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT check_ym, rpt_status, count(*) cnt, sum(amt) amt
from (
SELECT check_ym, rpt_status, stat_id, sum(check_amt) amt
from (
SELECT convert(varchar(7), check_date, 111) as check_ym,
       re_paid_date,
	   closed_date,
	   exec_date,
	   status,
	   check_no,
       check_acct_short,
	   paid_id,
	   check_amt,
	   CASE WHEN @cnt_type = 'paid_id' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN exec_date <= @date_e THEN '3'
			WHEN exec_date > @date_e THEN '4'
			ELSE '4'
	   END AS rpt_status 
  FROM [FAP_VE_TRACE]
WHERE 1 = 1
";
                //組合"統計截止日"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"指定支票年月"條件
                sql += " AND (check_date >= @check_ym_b AND check_date < @check_ym_e)";


                if (levelSpace)
                {
                    sql += " AND (((level_1 = '' or level_1 is null) and (level_2 = '' or level_2 is null)) OR (";
                }
                else
                    sql += " AND ((1 = 2) OR (";

                //組合"清理大類"條件
                sql += "  level_1 IN ( ";
                foreach (string d in level_1_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                //組合"清理小類"條件
                sql += " AND level_2 IN ( ";
                foreach (string d in level_2_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "))) ";

                string strGroup = @"  ) m
  group by check_ym, stat_id, rpt_status) rpt
group by check_ym, rpt_status";

                sql += strGroup;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@check_ym_b", check_ym_b);
                cmd.Parameters.AddWithValue("@check_ym_e", check_ym_e);
                cmd.Parameters.AddWithValue("@date_e", date_e);
                cmd.Parameters.AddWithValue("@cnt_type", cnt_type);


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0014Model d = new OAP0014Model();
                        d.check_ym = dr["check_ym"].ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        d.cnt = dr["cnt"].ToString();
                        d.amt = Convert.ToInt64(dr["amt"]).ToString();

                        //組合"清理狀態"條件
                        if (!bStatus)
                        {
                            if (status.Equals(dr["rpt_status"].ToString()))
                                rows.Add(d);
                        }
                        else
                            rows.Add(d);
                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// "OAP0013 壽險公司應支付而未能給付保戶款項調查表"查詢
        /// </summary>
        /// <param name="fsc_range_arr"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <param name="status"></param>
        /// <param name="level_1_arr"></param>
        /// <param name="level_2_arr"></param>
        /// <param name="cnt_type"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public List<OAP0013Model> qryForOAP0013(string[] fsc_range_arr, string date_b, string date_e, string status
            , string[] level_1_arr, string[] level_2_arr, string cnt_type, bool levelSpace, SqlConnection conn)
        {
            bool bDateB = StringUtil.toString(date_b) == "" ? true : false;
            bool bDateE = StringUtil.toString(date_e) == "" ? true : false;
            bool bStatus = StringUtil.toString(status) == "" ? true : false;

            List<OAP0013Model> rows = new List<OAP0013Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT fsc_range, rpt_status, count(*) cnt, sum(amt) amt
from (
SELECT fsc_range, rpt_status, stat_id, sum(check_amt) amt
from (
SELECT fsc_range,
       re_paid_date,
	   closed_date,
	   exec_date,
	   status,
	   check_no,
	   paid_id,
	   check_amt,
	   CASE WHEN @cnt_type = 'paid_id' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN exec_date <= @date_e THEN '3'
			WHEN exec_date > @date_e THEN '4'
			ELSE '4'
	   END AS rpt_status 
  FROM [FAP_VE_TRACE]
WHERE 1 = 1
";
                //組合"統計起迄區間"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"保局範圍"條件
                sql += " AND fsc_range IN ( ";
                foreach (string d in fsc_range_arr) {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                if (levelSpace)
                {
                    sql += " AND (((level_1 = '' or level_1 is null) and (level_2 = '' or level_2 is null)) OR (";
                }
                else
                    sql += " AND ((1 = 2) OR (";


                //組合"清理大類"條件
                sql += "  level_1 IN ( ";
                foreach (string d in level_1_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                //組合"清理小類"條件
                sql += " AND level_2 IN ( ";
                foreach (string d in level_2_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "))) ";

                string strGroup = @"  ) m
  group by fsc_range, stat_id, rpt_status) rpt
group by fsc_range, rpt_status";

                sql += strGroup;

                cmd.CommandText = sql;

                

                cmd.Parameters.AddWithValue("@date_e", date_e);
                cmd.Parameters.AddWithValue("@cnt_type", cnt_type);


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0013Model d = new OAP0013Model();
                        d.fsc_range = dr["fsc_range"].ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        d.cnt = dr["cnt"].ToString();
                        d.amt = Convert.ToInt64(dr["amt"]).ToString();

                        //組合"清理狀態"條件
                        if (!bStatus)
                        {
                            if (status.Equals(dr["rpt_status"].ToString()))
                                rows.Add(d);
                        }
                        else
                            rows.Add(d);

                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<OAP0016Model> qryForOAP0016(string[] fsc_range_arr, string date_b, string date_e, string status
           , string[] level_1_arr, string[] level_2_arr, string cnt_type, bool levelSpace, SqlConnection conn)
        {
            bool bDateB = StringUtil.toString(date_b) == "" ? true : false;
            bool bDateE = StringUtil.toString(date_e) == "" ? true : false;
            bool bStatus = StringUtil.toString(status) == "" ? true : false;

            List<OAP0016Model> rows = new List<OAP0016Model>();
            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Parameters.Clear();

                string sql = @"
SELECT fsc_range, rpt_status, count(*) cnt, sum(amt) amt
from (
SELECT fsc_range, rpt_status, stat_id, sum(check_amt) amt
from (
SELECT fsc_range,
       re_paid_date,
	   closed_date,
	   exec_date,
	   status,
	   check_no,
	   paid_id,
	   check_amt,
	   CASE WHEN @cnt_type = 'paid_id' THEN (CASE WHEN LEN(ISNULL(paid_id,'')) = 0 THEN check_no + check_acct_short ELSE paid_id END) ELSE  check_no + check_acct_short END AS STAT_ID,
	   CASE WHEN re_paid_date <= @date_e THEN '1'
	        WHEN closed_date <= @date_e THEN '2'
			WHEN (status = 4 or status = '') THEN '4'
			ELSE '3'
	   END AS rpt_status 
  FROM [FAP_VE_TRACE]
WHERE 1 = 1
";
                //組合"統計起迄區間"條件
                //sql += " AND (exec_date_1 <= @date_e OR exec_date_1 is null)";    delete by daiyu 20190909 取消判斷未來件


                //組合"保局範圍"條件    modify by daiyu 20210208 修改若全選時...不看保局範圍
                bool bFscRange = false;
                foreach (string d in fsc_range_arr)
                {
                    if ("on".Equals(d.ToLower()))
                        bFscRange = true;
                }

                if (!bFscRange) {
                    sql += " AND fsc_range IN ( ";
                    foreach (string d in fsc_range_arr)
                    {
                        if (!"".Equals(d))
                            sql += "'" + d + "',";
                    }
                    sql = sql.Substring(0, sql.Length - 1);
                    sql += ") ";
                }
                


                if (levelSpace)
                {
                    sql += " AND (((level_1 = '' or level_1 is null) and (level_2 = '' or level_2 is null)) OR (";
                }
                else
                    sql += " AND ((1 = 2) OR (";


                //組合"清理大類"條件
                sql += "  level_1 IN ( ";
                foreach (string d in level_1_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ") ";


                //組合"清理小類"條件
                sql += " AND level_2 IN ( ";
                foreach (string d in level_2_arr)
                {
                    if (!"".Equals(d))
                        sql += "'" + d + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += "))) ";

                string strGroup = @"  ) m
  group by fsc_range, stat_id, rpt_status) rpt
group by fsc_range, rpt_status";

                sql += strGroup;

                cmd.CommandText = sql;



                cmd.Parameters.AddWithValue("@date_e", date_e);
                cmd.Parameters.AddWithValue("@cnt_type", cnt_type);


                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        OAP0016Model d = new OAP0016Model();
                        d.fsc_range = dr["fsc_range"].ToString();
                        d.rpt_status = dr["rpt_status"].ToString();
                        d.cnt = dr["cnt"].ToString();
                        d.amt = Convert.ToInt64(dr["amt"]).ToString();

                        //組合"清理狀態"條件
                        if (!bStatus)
                        {
                            if (status.Equals(dr["rpt_status"].ToString()))
                                rows.Add(d);
                        }
                        else
                            rows.Add(d);

                    }
                }

                return rows;

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// "OAP0012 態樣統計表"查詢
        /// </summary>
        /// <param name="fsc_range_arr"></param>
        /// <param name="date_b"></param>
        /// <param name="date_e"></param>
        /// <param name="level_1_arr"></param>
        /// <param name="level_2_arr"></param>
        /// <param name="cnt_type"></param>
        /// <returns></returns>
        public List<OAP0012Model> qryForOAP0012(string[] fsc_range_arr, string date_b, string date_e
            , string[] level_1_arr, string[] level_2_arr, string cnt_type)
        {
            bool bDateB = StringUtil.toString(date_b) == "" ? true : false;
            bool bDateE = StringUtil.toString(date_e) == "" ? true : false;

            DateTime? _DateB = !bDateB ? Convert.ToDateTime(date_b) : (DateTime?)null;
            DateTime? _DateE = !bDateE ? Convert.ToDateTime(date_e) : (DateTime?)null;


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in 
                                    (from d in db.FAP_VE_TRACE
                                     where 1 == 1
                                     & d.status == "2"
                                     & (bDateB || (!bDateB & d.closed_date >= _DateB))
                                     & (bDateE || (!bDateE & d.closed_date <= _DateE))
                                     & fsc_range_arr.Contains(d.fsc_range)
                                     & level_1_arr.Contains(d.level_1)
                                     & level_2_arr.Contains(d.level_2)
                                     group d by new {
                                         level_1 = d.level_1,
                                         level_2 = d.level_2,
                                         stat_id = (cnt_type == "check_no" ? d.check_no : ((d.paid_id == "" || d.paid_id == null) ? d.check_no : d.paid_id))
                                     } into grp
                                     select new OAP0012Model
                                     {
                                         level_1 = grp.Key.level_1,
                                         level_2 = grp.Key.level_2,
                                         stat_id = grp.Key.stat_id,
                                         amt = (decimal)grp.Sum(x => x.check_amt)
                                     }
                                     ).ToList<OAP0012Model>()
                                group m by new {
                                         level_1 = m.level_1,
                                         level_2 = m.level_2
                                     } into grpM
                                select new OAP0012Model
                                {
                                    level_1 = grpM.Key.level_1,
                                    level_2 = grpM.Key.level_2,
                                    cnt = grpM.Count().ToString(),
                                    amt = (decimal)grpM.Sum(x => x.amt)
                                }).OrderBy(x => x.level_1).ThenBy(x => x.level_2).ToList<OAP0012Model>();


                    return rows;
                }
            }
        }

        public List<FAP_VE_TRACE> qryForOAP0017(string status, dbFGLEntities db, string date_b, string date_e)
        {
            bool bDateB = "".Equals(StringUtil.toString(date_b)) ? false : true;
            bool bDateE = "".Equals(StringUtil.toString(date_e)) ? false : true;

            DateTime dtB = DateUtil.stringToDatetime(date_b);
            DateTime dtE = DateUtil.stringToDatetime(date_e);

            List<FAP_VE_TRACE> rows = db.FAP_VE_TRACE
                        .Where(x => x.status == status
                        & (

                        ((!bDateB || bDateB & (x.exec_date_1.HasValue & x.exec_date_1 >= dtB)) & (!bDateE || bDateE & (x.exec_date_1.HasValue & x.exec_date_1 <= dtE)))
                       || ((!bDateB || bDateB & (x.exec_date_2.HasValue & x.exec_date_2 >= dtB)) & (!bDateE || bDateE & (x.exec_date_2.HasValue & x.exec_date_2 <= dtE)))
                       || ((!bDateB || bDateB & (x.exec_date_3.HasValue & x.exec_date_3 >= dtB)) & (!bDateE || bDateE & (x.exec_date_3.HasValue & x.exec_date_3 <= dtE)))
                       || ((!bDateB || bDateB & (x.exec_date_4.HasValue & x.exec_date_4 >= dtB)) & (!bDateE || bDateE & (x.exec_date_4.HasValue & x.exec_date_4 <= dtE)))
                       || ((!bDateB || bDateB & (x.exec_date_5.HasValue & x.exec_date_5 >= dtB)) & (!bDateE || bDateE & (x.exec_date_5.HasValue & x.exec_date_5 <= dtE)))

                        //(!x.exec_date_1.HasValue || dtB.CompareTo(x.exec_date_1) <= 0 & dtE.CompareTo(x.exec_date_1) >= 0) ||
                        //(dtB.CompareTo(x.exec_date_2) <= 0 & dtE.CompareTo(x.exec_date_2) >= 0) ||
                        //(dtB.CompareTo(x.exec_date_3) <= 0 & dtE.CompareTo(x.exec_date_3) >= 0) ||
                        //(dtB.CompareTo(x.exec_date_4) <= 0 & dtE.CompareTo(x.exec_date_4) >= 0) ||
                        //(dtB.CompareTo(x.exec_date_5) <= 0 & dtE.CompareTo(x.exec_date_5) >= 0) 
                        ) 
                        ).ToList();

            return rows;
        }

        public List<OAP0017Model> qryForOAP0017Proc(string status, dbFGLEntities db, string date_b, string date_e)
        {
            //List<FAP_VE_TRACE> rows = db.FAP_VE_TRACE
            //            .Where(x => x.status == status).ToList();

            bool bDateB = "".Equals(StringUtil.toString(date_b)) ? false : true;
            bool bDateE = "".Equals(StringUtil.toString(date_e)) ? false : true;

            DateTime dtB = DateUtil.stringToDatetime(date_b);
            DateTime dtE = DateUtil.stringToDatetime(date_e);


            List<OAP0017Model> rows = new List<OAP0017Model>();
            rows = (from m in db.FAP_VE_TRACE
                    where m.status == status
                      & m.exec_date_1.HasValue
                      & ((!bDateB || bDateB & (m.exec_date_1.HasValue & m.exec_date_1 >= dtB)) & (!bDateE || bDateE & (m.exec_date_1.HasValue & m.exec_date_1 <= dtE)))
                    select new OAP0017Model
                    {
                        check_no = m.check_no,
                        check_acct_short = m.check_acct_short,
                        level_1 = m.level_1,
                        level_2 = m.level_2,
                        paid_id = m.paid_id,
                        seq = "1",
                        practice = m.practice_1,
                        cert_doc = m.cert_doc_1,
                        proc_desc = m.proc_desc,
                        exec_date = m.exec_date_1 == null ? "" : (SqlFunctions.DatePart("year", m.exec_date_1) - 1911) + "/" +
                                                 SqlFunctions.DatePart("m", m.exec_date_1) + "/" +
                                                 SqlFunctions.DateName("day", m.exec_date_1).Trim()
                    }).Distinct().ToList().Union

                    (from m in db.FAP_VE_TRACE
                     where m.status == status
                       & m.exec_date_2.HasValue
                       & ((!bDateB || bDateB & (m.exec_date_2.HasValue & m.exec_date_2 >= dtB)) & (!bDateE || bDateE & (m.exec_date_2.HasValue & m.exec_date_2 <= dtE)))
                     select new OAP0017Model
                     {
                         check_no = m.check_no,
                         check_acct_short = m.check_acct_short,
                         level_1 = m.level_1,
                         level_2 = m.level_2,
                         paid_id = m.paid_id,
                         seq = "2",
                         practice = m.practice_2,
                         cert_doc = m.cert_doc_2,
                         proc_desc = m.proc_desc,
                         exec_date = m.exec_date_2 == null ? "" : (SqlFunctions.DatePart("year", m.exec_date_2) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.exec_date_2) + "/" +
                                                  SqlFunctions.DateName("day", m.exec_date_2).Trim()
                     }).Distinct().ToList().Union

                     (from m in db.FAP_VE_TRACE
                      where m.status == status
                        & m.exec_date_3.HasValue
                        & ((!bDateB || bDateB & (m.exec_date_3.HasValue & m.exec_date_3 >= dtB)) & (!bDateE || bDateE & (m.exec_date_3.HasValue & m.exec_date_3 <= dtE)))
                      select new OAP0017Model
                      {
                          check_no = m.check_no,
                          check_acct_short = m.check_acct_short,
                          level_1 = m.level_1,
                          level_2 = m.level_2,
                          paid_id = m.paid_id,
                          seq = "3",
                          practice = m.practice_3,
                          cert_doc = m.cert_doc_3,
                          proc_desc = m.proc_desc,
                          exec_date = m.exec_date_3 == null ? "" : (SqlFunctions.DatePart("year", m.exec_date_3) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", m.exec_date_3) + "/" +
                                                   SqlFunctions.DateName("day", m.exec_date_3).Trim()
                      }).Distinct().ToList().Union

                      (from m in db.FAP_VE_TRACE
                       where m.status == status
                         & m.exec_date_4.HasValue
                         & ((!bDateB || bDateB & (m.exec_date_4.HasValue & m.exec_date_4 >= dtB)) & (!bDateE || bDateE & (m.exec_date_4.HasValue & m.exec_date_4 <= dtE)))
                       select new OAP0017Model
                       {
                           check_no = m.check_no,
                           check_acct_short = m.check_acct_short,
                           level_1 = m.level_1,
                           level_2 = m.level_2,
                           paid_id = m.paid_id,
                           seq = "4",
                           practice = m.practice_4,
                           cert_doc = m.cert_doc_4,
                           proc_desc = m.proc_desc,
                           exec_date = m.exec_date_4 == null ? "" : (SqlFunctions.DatePart("year", m.exec_date_4) - 1911) + "/" +
                                                    SqlFunctions.DatePart("m", m.exec_date_4) + "/" +
                                                    SqlFunctions.DateName("day", m.exec_date_4).Trim()
                       }).Distinct().ToList().Union

                       (from m in db.FAP_VE_TRACE
                        where m.status == status
                          & m.exec_date_5.HasValue
                          & ((!bDateB || bDateB & (m.exec_date_5.HasValue & m.exec_date_5 >= dtB)) & (!bDateE || bDateE & (m.exec_date_5.HasValue & m.exec_date_5 <= dtE)))
                        select new OAP0017Model
                        {
                            check_no = m.check_no,
                            check_acct_short = m.check_acct_short,
                            level_1 = m.level_1,
                            level_2 = m.level_2,
                            paid_id = m.paid_id,
                            seq = "5",
                            practice = m.practice_5,
                            cert_doc = m.cert_doc_5,
                            proc_desc = m.proc_desc,
                            exec_date = m.exec_date_5 == null ? "" : (SqlFunctions.DatePart("year", m.exec_date_5) - 1911) + "/" +
                                                     SqlFunctions.DatePart("m", m.exec_date_5) + "/" +
                                                     SqlFunctions.DateName("day", m.exec_date_5).Trim()
                        }).Distinct().ToList().Union

            (from m in db.FAP_VE_TRACE
                    join d in db.FAP_VE_TRACE_PROC.Where(x => ((!bDateB || bDateB & (dtB.CompareTo(x.exec_date) <= 0)) & (!bDateE || bDateE & (dtE.CompareTo(x.exec_date) >= 0)))) 
                    on new { m.check_acct_short, m.check_no} equals new { d.check_acct_short, d.check_no }
                    where m.status == status
                    select new OAP0017Model
                    {
                        check_no = m.check_no,
                        check_acct_short = m.check_acct_short,
                        level_1 = m.level_1,
                        level_2 = m.level_2,
                        paid_id = m.paid_id,
                        seq = "6",
                        practice = d.practice,
                        cert_doc = d.cert_doc,
                        proc_desc = d.proc_desc,
                        exec_date = d.exec_date == null ? "" : (SqlFunctions.DatePart("year", d.exec_date) - 1911) + "/" +
                                                 SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                 SqlFunctions.DateName("day", d.exec_date).Trim()
                    }).Distinct().ToList();


            return rows;
        }


        public List<FAP_VE_TRACE> qryForOAP0011Rpt(string[] checkList)
        {
            using (new TransactionScope(
               TransactionScopeOption.Required,
               new TransactionOptions
               {
                   IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
               }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<FAP_VE_TRACE> rows = db.FAP_VE_TRACE
                        .Where(x => checkList.Contains(x.check_no)).ToList();

                    return rows;
                }
            }
        }


        public List<OAP0011Model> qryByClosedNo(string closed_no)
        {
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                where 1 == 1
                                & m.closed_no == closed_no
                                select new OAP0011Model
                                {
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    // o_paid_cd = m.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    level_1 = m.level_1,
                                    level_2 = m.level_2,
                                    check_amt = m.check_amt.ToString(),
                                    data_status = m.data_status,
                                    proc_desc = m.proc_desc,
                                    closed_no = m.closed_no,
                                    closed_date = m.closed_date == null ? "" : (SqlFunctions.DatePart("year", m.closed_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.closed_date) + "/" +
                                                  SqlFunctions.DateName("day", m.closed_date).Trim(),
                                    status = m.status
                                }).Distinct().ToList<OAP0011Model>();

                    return rows;
                }
            }
        }


        /// <summary>
        /// OAP0011 清理結案申請作業(查詢)
        /// </summary>
        /// <param name="qType"></param>
        /// <param name="paid_id"></param>
        /// <param name="check_no"></param>
        /// <param name="closed_no"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<OAP0011Model> qryForOAP0011(string qType, string paid_id, string check_no, string closed_no, string[] status)
        {
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                //join cAply in db.FAP_APLY_REC.Where(x => x.aply_type == "C" & x.appr_mapping_key != "") on m.closed_no equals cAply.appr_mapping_key into psAply
                                //from xAply in psAply.DefaultIfEmpty()
                                where 1 == 1
                                & status.Contains(m.status)
                                & ((qType == "q_paid_id" & m.paid_id == paid_id)
                                || (qType == "q_check_no" & m.check_no == check_no)
                                || (qType == "q_closed_no" & m.closed_no == closed_no))
                                select new OAP0011Model
                                {
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) -1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                   // o_paid_cd = m.o_paid_cd,
                                   paid_id = m.paid_id,
                                   paid_name = m.paid_name,
                                    level_1 = m.level_1,
                                    level_2 = m.level_2,
                                    check_amt = m.check_amt.ToString(),
                                    data_status = m.data_status,
                                    proc_desc = m.proc_desc,
                                    closed_no = m.closed_no,
                                    closed_date = m.closed_date == null ? "" : (SqlFunctions.DatePart("year", m.closed_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.closed_date) + "/" +
                                                  SqlFunctions.DateName("day", m.closed_date).Trim(),
                                    //memo = xAply.memo,
                                    status = m.status
                                    
                                    
                                }).Distinct().ToList<OAP0011Model>();

                    if (rows.Count > 0) {
                        if (!"".Equals(StringUtil.toString(rows[0].closed_no))) {
                            FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();  //add by daiyu 20201209
                            List<APAplyRecModel> fapAplyRecList = fAPAplyRecDao.qryAplyType("C", "", StringUtil.toString(rows[0].closed_no), db).OrderByDescending(x => x.create_dt).ToList();
                            if (fapAplyRecList.Count > 0) {
                                APAplyRecModel fapAplyRec = fapAplyRecList[0];
                                string _memo = "";
                                if (fapAplyRec != null)
                                {
                                    _memo = StringUtil.toString(fapAplyRec.memo);
                                    rows.Select(x => { x.memo = _memo; return x; }).ToList();
                                }
                            }
                            
                        }
                       
                    }


                    return rows;
                }
            }
        }



        /// <summary>
        /// OAP0010 清理歷程查詢
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="paid_name"></param>
        /// <param name="policy_no"></param>
        /// <param name="policy_seq"></param>
        /// <param name="id_dup"></param>
        /// <param name="check_no"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<OAP0010Model> qryForOAP0010(string paid_id, string paid_name
            , string policy_no, string policy_seq, string id_dup, string check_no, string status)
        {
            bool bPaidId = StringUtil.toString(paid_id) == "" ? true : false;
            bool bPaidName = StringUtil.toString(paid_name) == "" ? true : false;
            bool bPolicyNo = StringUtil.toString(policy_no) == "" ? true : false;
            bool bPolicySeq = StringUtil.toString(policy_seq) == "" ? true : false;
            bool bIdDup = StringUtil.toString(id_dup) == "" ? true : false;
            bool bCheckNo = StringUtil.toString(check_no) == "" ? true : false;
            bool bStatus = StringUtil.toString(status) == "" ? true : false;


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join poli in db.FAP_VE_TRACE_POLI on new { m.check_no, m.check_acct_short } equals new { poli.check_no, poli.check_acct_short } into psPoli
                                from xPoli in psPoli.DefaultIfEmpty()
                                where 1 == 1
                                & (bPaidId || (!bPaidId & m.paid_id == paid_id))
                                & (bPaidName || (!bPaidName & m.paid_name == paid_name))
                                & (bPolicyNo || (!bPolicyNo & xPoli.policy_no == policy_no))
                                & (bPolicySeq || (!bPolicySeq & xPoli.policy_seq.ToString() == policy_seq))
                                & (bIdDup || (!bIdDup & xPoli.id_dup == id_dup))
                                & (bCheckNo || (!bCheckNo & m.check_no == check_no))
                                & (bStatus || (!bStatus & m.status == status))
                                select new OAP0010Model
                                {
                                    temp_id = m.check_no + "_" + m.check_acct_short,
                                    fsc_range = m.fsc_range,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = m.check_amt == null ? "" : m.check_amt.ToString(),
                                    re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                    re_paid_type = m.re_paid_type,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    status = m.status,
                                    level_1 = m.level_1,
                                    level_2 = m.level_2,
                                    closed_no = m.closed_no,
                                    update_id = m.update_id,
                                    update_datetime= m.update_datetime == null ? "" : (SqlFunctions.DatePart("year", m.update_datetime) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                  SqlFunctions.DateName("day", m.update_datetime).Trim()
                                }).Distinct().ToList<OAP0010Model>();

                    return rows;
                }
            }
        }





        /// <summary>
        /// "OAP0004 指定保局範圍作業"查詢
        /// </summary>
        /// <param name="check_date_b"></param>
        /// <param name="check_date_e"></param>
        /// <param name="check_no"></param>
        /// <param name="fsc_range"></param>
        /// <returns></returns>
        public List<OAP0004Model> qryForOAP0004(string check_date_b, string check_date_e, string check_no, string fsc_range)
        {
            bool bCheckDateB = StringUtil.toString(check_date_b) == "" ? true : false;
            bool bCheckDateE = StringUtil.toString(check_date_e) == "" ? true : false;
            bool bCheckNo = StringUtil.toString(check_no) == "" ? true : false;
            bool bFscRange = StringUtil.toString(fsc_range) == "" ? true : false;

            DateTime? checkDateB = !bCheckDateB ? Convert.ToDateTime(check_date_b) : (DateTime?)null;
            DateTime? checkDateE = !bCheckDateE ? Convert.ToDateTime(check_date_e) : (DateTime?)null;



            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                where 1 == 1
                                & (bCheckDateB || (!bCheckDateB & m.check_date >= checkDateB))
                                & (bCheckDateE || (!bCheckDateE & m.check_date <= checkDateE))
                                & (bCheckNo || (!bCheckNo & m.check_no == check_no))
                                & (bFscRange ||
                                 (!bFscRange & (
                                                  (fsc_range != "0" & m.fsc_range == fsc_range)
                                              || (fsc_range == "0" & (m.fsc_range == fsc_range || m.fsc_range == "" || Nullable<int>.Equals(m.fsc_range, null)))
                                               )
                                ))  //modify by daiyu 20191025


                                // & (bFscRange || (!bFscRange & fsc_range_arr.Contains(m.fsc_range))) 
                                group m by new { m.check_acct_short, m.check_no, m.check_date, m.fsc_range } into g

                                select new OAP0004Model
                                {
                                    check_acct_short= g.Key.check_acct_short,
                                    check_no = g.Key.check_no,
                                    //check_date = g.Key.check_date == null ? "" : (SqlFunctions.DatePart("year", g.Key.check_date) - 1911) + "/" +
                                    //              SqlFunctions.DatePart("m", g.Key.check_date) + "/" +
                                    //              SqlFunctions.DateName("day", g.Key.check_date).Trim(),
                                    check_date = g.Key.check_date.ToString(),
                                    fsc_range = g.Key.fsc_range,
                                    check_amt = g.Sum(x => x.check_amt).ToString()

                                }).OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList<OAP0004Model>();


                    foreach (OAP0004Model d in rows) {
                        if (!"".Equals(d.check_date)) 
                            d.check_date = DateUtil.ADDateToChtDate(d.check_date, 3, "/");
                    }
                    
                    return rows;
                }
            }
        }


        public List<OAP0004DModel> qryForOAP0004Export(string check_date_b, string check_date_e, string check_no, string fsc_range)
        {
            bool bCheckDateB = StringUtil.toString(check_date_b) == "" ? true : false;
            bool bCheckDateE = StringUtil.toString(check_date_e) == "" ? true : false;
            bool bCheckNo = StringUtil.toString(check_no) == "" ? true : false;
            bool bFscRange = StringUtil.toString(fsc_range) == "" ? true : false;

            DateTime? checkDateB = !bCheckDateB ? Convert.ToDateTime(check_date_b) : (DateTime?)null;
            DateTime? checkDateE = !bCheckDateE ? Convert.ToDateTime(check_date_e) : (DateTime?)null;

            string[] send_cnt_arr = new string[] { "G1", "G2", "G3", "G4", "G10", "G15" };
            string[] tel_appr_arr = new string[] { "1", "2"};

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join codeFscRange in db.FAP_VE_CODE.Where(x => x.code_type == "FSC_RANGE") on m.fsc_range equals codeFscRange.code_id into psFscRange
                                from xFscRange in psFscRange.DefaultIfEmpty()

                                join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "CLR_STATUS") on m.status equals codeStatus.CODE into psStatus
                                from xStatus in psStatus.DefaultIfEmpty()

                                join codeRePaidTp in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "R_PAID_TP") on m.re_paid_type equals codeRePaidTp.CODE into psRePaidTp
                                from xRePaidTp in psRePaidTp.DefaultIfEmpty()

                                where 1 == 1
                                & (bCheckDateB || (!bCheckDateB & m.check_date >= checkDateB))
                                & (bCheckDateE || (!bCheckDateE & m.check_date <= checkDateE))
                                & (bCheckNo || (!bCheckNo & m.check_no == check_no))
                                   & (bFscRange ||
                                 (!bFscRange & (
                                                  (fsc_range != "0" & m.fsc_range == fsc_range)
                                              || (fsc_range == "0" & (m.fsc_range == fsc_range || m.fsc_range == "" || Nullable<int>.Equals(m.fsc_range, null)))
                                               )
                                ))  //modify by daiyu 20191025

                                //& (bFscRange || (!bFscRange & m.fsc_range == fsc_range))

                                select new OAP0004DModel
                                {
                                    fsc_range = xFscRange.code_value == null ? m.fsc_range : m.fsc_range + "." + xFscRange.code_value,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_amt = (Int64)m.check_amt,
                                    check_date = m.check_date.ToString(),
                                    //check_date = (m.check_date == null) ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                    //              SqlFunctions.DatePart("m", m.check_date) + "/" +
                                    //              SqlFunctions.DateName("day", m.check_date).Trim(),
                                    re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                    re_paid_date_n = m.re_paid_date_n == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date_n) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date_n) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date_n).Trim(),
                                    re_paid_type = xRePaidTp.CODE_VALUE == null ? m.re_paid_type : m.re_paid_type + "." + xRePaidTp.CODE_VALUE,
                                    status = m.status,
                                    closed_date = m.closed_date == null ? "" : (SqlFunctions.DatePart("year", m.closed_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.closed_date) + "/" +
                                                  SqlFunctions.DateName("day", m.closed_date).Trim()

                                }).OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList<OAP0004DModel>();

                    foreach (OAP0004DModel d in rows)
                    {
                        if (!"".Equals(d.check_date))
                            d.check_date = DateUtil.ADDateToChtDate(d.check_date, 3, "/");
                    }
                    return rows;
                }
            }
        }


        public List<OAP0004DModel> qryForOAP0004QQuery(string check_date_b, string check_date_e, string check_no, string fsc_range
            , string dispatch_status, string clean_status)
        {
            bool bCheckDateB = StringUtil.toString(check_date_b) == "" ? true : false;
            bool bCheckDateE = StringUtil.toString(check_date_e) == "" ? true : false;
            bool bCheckNo = StringUtil.toString(check_no) == "" ? true : false;
            bool bFscRange = StringUtil.toString(fsc_range) == "" ? true : false;
            bool bDispatchStatus = StringUtil.toString(dispatch_status) == "" ? true : false;
            bool bCleanStatus = StringUtil.toString(clean_status) == "" ? true : false;

            DateTime? checkDateB = !bCheckDateB ? Convert.ToDateTime(check_date_b) : (DateTime?)null;
            DateTime? checkDateE = !bCheckDateE ? Convert.ToDateTime(check_date_e) : (DateTime?)null;

            string[] send_cnt_arr = new string[] { "G1", "G2", "G3", "G4", "G10", "G15" };
            string[] tel_appr_arr = new string[] { "1", "2" };

            string[] dispatch_status_arr = new string[] { };
            if (!bDispatchStatus)
                dispatch_status_arr = dispatch_status.Split('|').ToArray();


            string[] clean_status_arr = new string[] { };
            if (!bCleanStatus)
                clean_status_arr = clean_status.Split('|').ToArray();


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join codeFscRange in db.FAP_VE_CODE.Where(x => x.code_type == "FSC_RANGE") on m.fsc_range equals codeFscRange.code_id into psFscRange
                                from xFscRange in psFscRange.DefaultIfEmpty()


                                join telM in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" & x.tel_std_type == "tel_assign_case") on new { m.system, m.check_acct_short, m.check_no } equals new { telM.system, telM.check_acct_short, telM.check_no } into psTelM
                                from xTelM in psTelM.DefaultIfEmpty()

                                join telInterview in db.FAP_TEL_INTERVIEW on xTelM.tel_proc_no equals telInterview.tel_proc_no into psInterview
                                from xInterview in psInterview.DefaultIfEmpty()

                                join codeDispatch in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "dispatch_status") on xTelM.dispatch_status equals codeDispatch.CODE into psDispatch
                                from xDispatch in psDispatch.DefaultIfEmpty()


                                where 1 == 1
                                & (bCheckDateB || (!bCheckDateB & m.check_date >= checkDateB))
                                & (bCheckDateE || (!bCheckDateE & m.check_date <= checkDateE))
                                & (bCheckNo || (!bCheckNo & m.check_no == check_no))
                                & (bDispatchStatus || (!bDispatchStatus & dispatch_status_arr.Contains(xTelM.dispatch_status)))
                                & (bCleanStatus || (!bCleanStatus & clean_status_arr.Contains(m.status)))

                                & (bFscRange ||
                                 (!bFscRange & (
                                                  (fsc_range != "0" & m.fsc_range == fsc_range)
                                              || (fsc_range == "0" & (m.fsc_range == fsc_range || m.fsc_range == "" || Nullable<int>.Equals(m.fsc_range, null)))
                                               )
                                ))  //modify by daiyu 20191025

                                //& (bFscRange || (!bFscRange & m.fsc_range == fsc_range))

                                select new OAP0004DModel
                                {
                                    fsc_range = m.fsc_range,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_amt = (Int64)m.check_amt,
                                    check_date = m.check_date.ToString(),
                                    status = m.status,
                                    tel_appr_result = xInterview.tel_appr_result,
                                    clean_status = xInterview.clean_status,
                                    tel_result = xInterview.tel_result

                                }).OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList<OAP0004DModel>();

                    foreach (OAP0004DModel d in rows)
                    {
                        if (!"".Equals(d.check_date))
                            d.check_date = DateUtil.ADDateToChtDate(d.check_date, 3, "/");
                    }
                    return rows;
                }
            }
        }


        public List<OAP0004DModel> qryForOAP0004QExport(string check_date_b, string check_date_e, string check_no, string fsc_range
            ,string dispatch_status, string clean_status)
        {
            bool bCheckDateB = StringUtil.toString(check_date_b) == "" ? true : false;
            bool bCheckDateE = StringUtil.toString(check_date_e) == "" ? true : false;
            bool bCheckNo = StringUtil.toString(check_no) == "" ? true : false;
            bool bFscRange = StringUtil.toString(fsc_range) == "" ? true : false;
            bool bDispatchStatus = StringUtil.toString(dispatch_status) == "" ? true : false;
            bool bCleanStatus = StringUtil.toString(clean_status) == "" ? true : false;

            DateTime? checkDateB = !bCheckDateB ? Convert.ToDateTime(check_date_b) : (DateTime?)null;
            DateTime? checkDateE = !bCheckDateE ? Convert.ToDateTime(check_date_e) : (DateTime?)null;

            string[] send_cnt_arr = new string[] { "G1", "G2", "G3", "G4", "G10", "G15" };
            string[] tel_appr_arr = new string[] { "1", "2" };

            string[] dispatch_status_arr = new string[] { };
            if(!bDispatchStatus)
                dispatch_status_arr = dispatch_status.Split('|').ToArray();


            string[] clean_status_arr = new string[] { };
            if (!bCleanStatus)
                clean_status_arr = clean_status.Split('|').ToArray();


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join codeFscRange in db.FAP_VE_CODE.Where(x => x.code_type == "FSC_RANGE") on m.fsc_range equals codeFscRange.code_id into psFscRange
                                from xFscRange in psFscRange.DefaultIfEmpty()

                                join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "CLR_STATUS") on m.status equals codeStatus.CODE into psStatus
                                from xStatus in psStatus.DefaultIfEmpty()

                                join codeRePaidTp in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "R_PAID_TP") on m.re_paid_type equals codeRePaidTp.CODE into psRePaidTp
                                from xRePaidTp in psRePaidTp.DefaultIfEmpty()

                                join telM in db.FAP_TEL_CHECK.Where(x => x.data_flag == "Y" & x.tel_std_type == "tel_assign_case") on new { m.system, m.check_acct_short, m.check_no } equals new { telM.system, telM.check_acct_short, telM.check_no } into psTelM
                                from xTelM in psTelM.DefaultIfEmpty()

                                join telInterview in db.FAP_TEL_INTERVIEW on xTelM.tel_proc_no equals telInterview.tel_proc_no into psInterview
                                from xInterview in psInterview.DefaultIfEmpty()

                                join codeDispatch in db.SYS_CODE.Where(x => x.SYS_CD == "AP" & x.CODE_TYPE == "dispatch_status") on xTelM.dispatch_status equals codeDispatch.CODE into psDispatch
                                from xDispatch in psDispatch.DefaultIfEmpty()

                                let cCount = (from c in db.FAP_VE_TRACE_PROC
                                              where c.check_acct_short == m.check_acct_short
                                                  & c.check_no == m.check_no
                                                  & send_cnt_arr.Contains(c.practice)
                                              select c).Count()
                                //二次追踨-電訪
                                let _stage_2_date = (from c in db.FAP_TEL_INTERVIEW_HIS
                                                     where c.tel_proc_no == xInterview.tel_proc_no
                                                         & tel_appr_arr.Contains(c.data_type)
                                                         & c.tel_appr_result == "11"
                                                        & c.appr_stat == "2"
                                                     select c).Max(x => x.approve_datetime)
                                //清理階段
                                let _stage_3_date = (from c in db.FAP_TEL_INTERVIEW_HIS
                                                     where c.tel_proc_no == xInterview.tel_proc_no
                                                         & tel_appr_arr.Contains(c.data_type)
                                                         & c.tel_appr_result == "13"
                                                        & c.appr_stat == "2"
                                                     select c).Max(x => x.approve_datetime)
                                //戶政調閱
                                let _stage_4_date = (from c in db.FAP_TEL_INTERVIEW_HIS
                                                     where c.tel_proc_no == xInterview.tel_proc_no
                                                         & c.data_type == "3"
                                                         & c.tel_appr_result == "13"
                                                         & c.clean_status == "7"
                                                         & c.appr_stat == "2"
                                                     select c).Max(x => x.approve_datetime)
                                //清理結案
                                let _stage_5_date = (from c in db.FAP_APLY_REC
                                                     where c.aply_type == "C"
                                                         & c.appr_stat == "2"
                                                         & c.appr_mapping_key.Substring(5, c.appr_mapping_key.Length - 5) == m.check_no
                                                     select c).Max(x => x.approve_datetime)
                                //待處理
                                let _stage_7_date = (from c in db.FAP_TEL_INTERVIEW_HIS
                                                     where c.tel_proc_no == xInterview.tel_proc_no
                                                         & tel_appr_arr.Contains(c.data_type)
                                                         & (c.tel_appr_result == "12" || c.tel_appr_result == "15")
                                                         & c.appr_stat == "2"
                                                     select c).Max(x => x.approve_datetime)
                                where 1 == 1
                                & (bCheckDateB || (!bCheckDateB & m.check_date >= checkDateB))
                                & (bCheckDateE || (!bCheckDateE & m.check_date <= checkDateE))
                                & (bCheckNo || (!bCheckNo & m.check_no == check_no))
                                & (bDispatchStatus || (!bDispatchStatus & dispatch_status_arr.Contains(xTelM.dispatch_status)))
                                & (bCleanStatus || (!bCleanStatus & clean_status_arr.Contains(m.status)))

                                & (bFscRange ||
                                 (!bFscRange & (
                                                  (fsc_range != "0" & m.fsc_range == fsc_range)
                                              || (fsc_range == "0" & (m.fsc_range == fsc_range || m.fsc_range == "" || Nullable<int>.Equals(m.fsc_range, null)))
                                               )
                                ))  //modify by daiyu 20191025

                                //& (bFscRange || (!bFscRange & m.fsc_range == fsc_range))

                                select new OAP0004DModel
                                {
                                    fsc_range =  m.fsc_range,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    check_amt = (Int64)m.check_amt,
                                    check_date = m.check_date.ToString(),
                                    //check_date = (m.check_date == null) ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                    //              SqlFunctions.DatePart("m", m.check_date) + "/" +
                                    //              SqlFunctions.DateName("day", m.check_date).Trim(),
                                    re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                    re_paid_date_n = m.re_paid_date_n == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date_n) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date_n) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date_n).Trim(),
                                    re_paid_type = xRePaidTp.CODE_VALUE == null ? m.re_paid_type : m.re_paid_type + "." + xRePaidTp.CODE_VALUE,
                                    status = m.status,
                                    closed_date = m.closed_date == null ? "" : (SqlFunctions.DatePart("year", m.closed_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.closed_date) + "/" +
                                                  SqlFunctions.DateName("day", m.closed_date).Trim(),
                                    paid_code = m.paid_code,
                                    dispatch_status = xTelM.dispatch_status, //xDispatch.CODE_VALUE == null ? xTelM.dispatch_status : xTelM.dispatch_status + "." + xDispatch.CODE_VALUE
                                    dispatch_date = xTelM.dispatch_date == null ? "" : (SqlFunctions.DatePart("year", xTelM.dispatch_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", xTelM.dispatch_date) + "/" +
                                                  SqlFunctions.DateName("day", xTelM.dispatch_date).Trim(),
                                    tel_interview_id = xTelM.tel_interview_id,

                                    tel_appr_result = xInterview.tel_appr_result,
                                    clean_status = xInterview.clean_status,
                                    tel_result = xInterview.tel_result,

                                    send_cnt = ((int)(m.as400_send_cnt == null ? 0 : m.as400_send_cnt)) + cCount,

                                    //send_cnt = ((int)(m.as400_send_cnt == null ? 0 : m.as400_send_cnt) + cCount),
                                    stage_1_date = xInterview.tel_interview_f_datetime == null ? "" : (SqlFunctions.DatePart("year", xInterview.tel_interview_f_datetime) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", xInterview.tel_interview_f_datetime) + "/" +
                                                   SqlFunctions.DateName("day", xInterview.tel_interview_f_datetime).Trim(),
                                    stage_2_date = _stage_2_date == null ? "" : (SqlFunctions.DatePart("year", _stage_2_date) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", _stage_2_date) + "/" +
                                                   SqlFunctions.DateName("day", _stage_2_date).Trim(),
                                    stage_3_date = _stage_3_date == null ? "" : (SqlFunctions.DatePart("year", _stage_3_date) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", _stage_3_date) + "/" +
                                                   SqlFunctions.DateName("day", _stage_3_date).Trim(),
                                    stage_4_date = _stage_4_date == null ? "" : (SqlFunctions.DatePart("year", _stage_4_date) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", _stage_4_date) + "/" +
                                                   SqlFunctions.DateName("day", _stage_4_date).Trim(),
                                    stage_5_date = _stage_5_date == null ? "" : (SqlFunctions.DatePart("year", _stage_5_date) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", _stage_5_date) + "/" +
                                                   SqlFunctions.DateName("day", _stage_5_date).Trim(),
                                    stage_7_date = _stage_7_date == null ? "" : (SqlFunctions.DatePart("year", _stage_7_date) - 1911) + "/" +
                                                   SqlFunctions.DatePart("m", _stage_7_date) + "/" +
                                                   SqlFunctions.DateName("day", _stage_7_date).Trim()

                                }).OrderBy(x => x.check_date).ThenBy(x => x.check_no).ToList<OAP0004DModel>();

                    foreach (OAP0004DModel d in rows)
                    {
                        if (!"".Equals(d.check_date))
                            d.check_date = DateUtil.ADDateToChtDate(d.check_date, 3, "/");
                    }
                    return rows;
                }
            }
        }

        public List<FAP_VE_TRACE> qryByPaidId(string paid_id, string[] status)
        {
            bool bStatus = false;
            if (status == null)
                bStatus = true;

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<FAP_VE_TRACE> rows = db.FAP_VE_TRACE
                        .Where(x => x.paid_id == paid_id
                        & (bStatus || (!bStatus & status.Contains(x.status)))).ToList();

                    return rows;
                }
            }
        }



        public FAP_VE_TRACE chkPaidIdExist(string paid_id)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_VE_TRACE d = db.FAP_VE_TRACE.Where(x => x.paid_id == paid_id).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_VE_TRACE();
            }
        }



        public FAP_VE_TRACE qryByCheckNo(string check_no, string check_acct_short, dbFGLEntities db)
        {
            
                FAP_VE_TRACE d = db.FAP_VE_TRACE
                    .Where(x => x.check_no == check_no & x.check_acct_short == check_acct_short).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_VE_TRACE();
            
        }


        /// <summary>
        /// 依"支票號碼查詢"
        /// </summary>
        /// <param name="checkNo"></param>
        /// <returns></returns>
        public FAP_VE_TRACE qryByCheckNo(string check_no, string check_acct_short)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FAP_VE_TRACE d = db.FAP_VE_TRACE
                    .Where(x => x.check_no == check_no & x.check_acct_short == check_acct_short).FirstOrDefault();

                if (d != null)
                    return d;
                else
                    return new FAP_VE_TRACE();
            }
        }


        public void updateForOAP0002(string check_acct_short, string check_no, string paid_id, string re_paid_date, string re_paid_date_n
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
  UPDATE FAP_VE_TRACE
    SET paid_id = @paid_id
       ,re_paid_date = @re_paid_date
       ,re_paid_date_n = @re_paid_date_n
  FROM FAP_VE_TRACE
    WHERE check_acct_short = @check_acct_short
	  AND check_no = @check_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@paid_id", paid_id);
                cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.Date).Value = StringUtil.toString(re_paid_date) == "" ? DBNull.Value : (Object)re_paid_date;
                cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.Date).Value = StringUtil.toString(re_paid_date_n) == "" ? DBNull.Value : (Object)re_paid_date_n;

                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);
                cmd.Parameters.AddWithValue("@check_no", check_no);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 異動"保局範圍"
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <param name="fsc_range"></param>
        /// <param name="aply_no"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateFscRange(DateTime dt, string userId, string fsc_range, string aply_no
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
  UPDATE FAP_VE_TRACE
    SET fsc_range = @fsc_range
       ,update_id = @update_id
       ,update_datetime = @update_datetime
  FROM FAP_VE_TRACE_HIS 
    WHERE FAP_VE_TRACE_HIS.check_acct_short = FAP_VE_TRACE.check_acct_short
	  AND FAP_VE_TRACE_HIS.check_no = FAP_VE_TRACE.check_no
	  AND FAP_VE_TRACE_HIS.aply_no = @aply_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@fsc_range", fsc_range);
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updateForVeClean(string type, FAP_VE_TRACE d , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sqlM = @"
UPDATE FAP_VE_TRACE
  SET status = @status
     ,proc_desc = @proc_desc
     ,exec_date = @exec_date
     ,as400_send_cnt = @as400_send_cnt
     ,update_id = @update_id
     ,update_datetime = @update_datetime
";

                string sqlWhere = @"
 WHERE check_no = @check_no
   AND check_acct_short = @check_acct_short
";

                string sqlSet = "";
                switch (type)
                {
                    case "1":
                        sqlSet = @" ,cert_doc_1 = @cert_doc_1 ,exec_date_1 = @exec_date_1 ,practice_1 = @practice_1 ";
                        break;
                    case "2":
                        sqlSet = @" ,cert_doc_2 = @cert_doc_2 ,exec_date_2 = @exec_date_2 ,practice_2 = @practice_2 ";
                        break;
                    case "3":
                        sqlSet = @" ,cert_doc_3 = @cert_doc_3 ,exec_date_3 = @exec_date_3 ,practice_3 = @practice_3 ";
                        break;
                    case "4":
                        sqlSet = @" ,cert_doc_4 = @cert_doc_4 ,exec_date_4 = @exec_date_4 ,practice_4 = @practice_4 ";
                        break;
                    case "5":
                        sqlSet = @" ,cert_doc_5 = @cert_doc_5 ,exec_date_5 = @exec_date_5 ,practice_5 = @practice_5 ";
                        break;
                }


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sqlM + sqlSet + sqlWhere;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@status", d.status);
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));
                cmd.Parameters.AddWithValue("@exec_date", d.exec_date);
                cmd.Parameters.AddWithValue("@as400_send_cnt", d.as400_send_cnt);   //add by daiyu 20201123
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(d.update_id));
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);
                cmd.Parameters.AddWithValue("@check_no", d.check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", d.check_acct_short);

                switch (type)
                {
                    case "1":
                        cmd.Parameters.AddWithValue("@cert_doc_1", StringUtil.toString(d.cert_doc_1));
                        cmd.Parameters.AddWithValue("@exec_date_1", d.exec_date_1);
                        cmd.Parameters.AddWithValue("@practice_1", StringUtil.toString(d.practice_1));
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@cert_doc_2", StringUtil.toString(d.cert_doc_2));
                        cmd.Parameters.AddWithValue("@exec_date_2", d.exec_date_2);
                        cmd.Parameters.AddWithValue("@practice_2", StringUtil.toString(d.practice_2));
                        break;
                    case "3":
                        cmd.Parameters.AddWithValue("@cert_doc_3", StringUtil.toString(d.cert_doc_3));
                        cmd.Parameters.AddWithValue("@exec_date_3", d.exec_date_3);
                        cmd.Parameters.AddWithValue("@practice_3", StringUtil.toString(d.practice_3));
                        break;
                    case "4":
                        cmd.Parameters.AddWithValue("@cert_doc_4", StringUtil.toString(d.cert_doc_4));
                        cmd.Parameters.AddWithValue("@exec_date_4", d.exec_date_4);
                        cmd.Parameters.AddWithValue("@practice_4", StringUtil.toString(d.practice_4));
                        break;
                    case "5":
                        cmd.Parameters.AddWithValue("@cert_doc_5", StringUtil.toString(d.cert_doc_5));
                        cmd.Parameters.AddWithValue("@exec_date_5", d.exec_date_5);
                        cmd.Parameters.AddWithValue("@practice_5", StringUtil.toString(d.practice_5));
                        break;

                }


                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void updateForAs400PaidType(string userId, string check_no, string check_acct_short, string re_paid_date, string re_paid_type
            , string sql_vhrdt, string paid_code, string level_1, string level_2, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
UPDATE FAP_VE_TRACE
  SET status = @status
     ,re_paid_date = @sql_vhrdt
     ,re_paid_date_n = @re_paid_date
     ,re_paid_type = @re_paid_type 
     ,paid_code = @paid_code
     ,level_1 = @level_1
     ,level_2 = @level_2
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE check_no = @check_no
   AND check_acct_short = @check_acct_short
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@status", "1");
                cmd.Parameters.AddWithValue("@sql_vhrdt", System.Data.SqlDbType.Date).Value = StringUtil.toString(sql_vhrdt) == null ? DBNull.Value : (Object)sql_vhrdt;    //add by daiyu 20201005
                cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.Date).Value = StringUtil.toString(re_paid_date) == null ? DBNull.Value : (Object)re_paid_date;
                cmd.Parameters.AddWithValue("@re_paid_type", StringUtil.toString(re_paid_type));
                cmd.Parameters.AddWithValue("@paid_code", StringUtil.toString(paid_code));
                cmd.Parameters.AddWithValue("@level_1", StringUtil.toString(level_1));
                cmd.Parameters.AddWithValue("@level_2", StringUtil.toString(level_2));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", DateTime.Now);
                cmd.Parameters.AddWithValue("@check_no", check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 異動"資料狀態"
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="data_status"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateForOAP0011(DateTime dt, string userId, string check_no, string check_acct_short, string data_status, string closed_no
            , string closed_desc, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
UPDATE FAP_VE_TRACE
  SET data_status = @data_status
     ,closed_no = @closed_no
     ,closed_desc = @closed_desc
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE check_no = @check_no
   AND check_acct_short = @check_acct_short
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", data_status);
                cmd.Parameters.AddWithValue("@closed_no", closed_no);
                cmd.Parameters.AddWithValue("@closed_desc", closed_desc);
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);
                cmd.Parameters.AddWithValue("@check_no", check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// OPEN端的踐行程序有異動
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="exec_date"></param>
        /// <param name="level_1"></param>
        /// <param name="level_2"></param>
        /// <param name="bchgLevel"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateForProc(DateTime dt, string userId, string check_no, string check_acct_short, DateTime? exec_date
            , string level_1, string level_2, bool bchgLevel
            , SqlConnection conn, SqlTransaction transaction)
        {

            string status = "";
            if (exec_date == null)
                status = "4";
            else
                status = "3";

            try
            {
                string sql = "";
                if (bchgLevel)
                {

                    sql = @"
UPDATE FAP_VE_TRACE
  SET level_1 = @level_1
     ,level_2 = @level_2
     ,exec_date = @exec_date
     ,status = @status
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE check_no = @check_no
   AND check_acct_short = @check_acct_short
";
                }
                else {
                    sql = @"
UPDATE FAP_VE_TRACE
  SET exec_date = @exec_date
     ,status = @status
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE check_no = @check_no
   AND check_acct_short = @check_acct_short
";

                }

             

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                if (bchgLevel) {
                    cmd.Parameters.AddWithValue("@level_1", level_1);
                    cmd.Parameters.AddWithValue("@level_2", level_2);
                }

                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);
                cmd.Parameters.AddWithValue("@check_no", check_no);
                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


      

        /// <summary>
        /// 異動"資料狀態"
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <param name="check_no"></param>
        /// <param name="check_acct_short"></param>
        /// <param name="data_status"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public int updateForOAP0011A(string apprStat, DateTime dt, string userId, string aply_no, string level_1, string level_2
            , string closed_date, string closed_no, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";

                if ("2".Equals(apprStat))
                {
                    sql = @"
UPDATE FAP_VE_TRACE
SET data_status = '1'
   ,level_1 = @level_1
   ,level_2 = @level_2
   ,status = '2'
   ,closed_date = @closed_date
   ,closed_no = @closed_no
   ,update_id = @update_id
   ,update_datetime = @update_datetime
FROM FAP_VE_TRACE_HIS 
 WHERE FAP_VE_TRACE_HIS.aply_no = @aply_no
   AND FAP_VE_TRACE_HIS.check_acct_short = FAP_VE_TRACE.check_acct_short
   AND FAP_VE_TRACE_HIS.check_no = FAP_VE_TRACE.check_no
";
                }
                else {
                    //modify by daiyu 20200730 駁回的資料結案編號亦不清....供使用者以結案編號重新查詢修改
                    sql = @"UPDATE FAP_VE_TRACE
SET data_status = '1'
   , update_id = @update_id
   , update_datetime = @update_datetime
FROM FAP_VE_TRACE_HIS
 WHERE FAP_VE_TRACE_HIS.aply_no = @aply_no
   AND FAP_VE_TRACE_HIS.check_acct_short = FAP_VE_TRACE.check_acct_short
   AND FAP_VE_TRACE_HIS.check_no = FAP_VE_TRACE.check_no
";

//                    sql = @"UPDATE FAP_VE_TRACE
//SET data_status = '1'
//   ,closed_no = ''
//   , update_id = @update_id
//   , update_datetime = @update_datetime
//FROM FAP_VE_TRACE_HIS
// WHERE FAP_VE_TRACE_HIS.aply_no = @aply_no
//   AND FAP_VE_TRACE_HIS.check_acct_short = FAP_VE_TRACE.check_acct_short
//   AND FAP_VE_TRACE_HIS.check_no = FAP_VE_TRACE.check_no
//";
                }

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();



                if ("2".Equals(apprStat)) {
                    cmd.Parameters.AddWithValue("@level_1", level_1);
                    cmd.Parameters.AddWithValue("@level_2", level_2);
                    cmd.Parameters.AddWithValue("@closed_date", System.Data.SqlDbType.Date).Value = (System.Object)closed_date ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@closed_no", closed_no);
                }
                    
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);
                cmd.Parameters.AddWithValue("@aply_no", aply_no);


                int cnt =cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int updateForTelCheckProc(string tel_std_type, string tel_proc_no, string proc_desc, DateTime exec_date
           , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";


                sql = @"
UPDATE FAP_VE_TRACE
SET  FAP_VE_TRACE.status = case when m.status = '1' then '1'
                                when m.status = '2' then '2'
					            else '3' end
   , FAP_VE_TRACE.exec_date = @exec_date
   , FAP_VE_TRACE.proc_desc = @proc_desc
   , FAP_VE_TRACE.update_id = his.update_id
   , FAP_VE_TRACE.update_datetime = his.update_datetime
FROM FAP_VE_TRACE m join FAP_TEL_CHECK his on his.system = m.system and his.check_acct_short = m.check_acct_short and his.check_no = m.check_no
    WHERE his.tel_proc_no = @tel_proc_no
      AND his.tel_std_type = @tel_std_type
      and m.status <> '1'";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_proc_no", tel_proc_no);
                cmd.Parameters.AddWithValue("@tel_std_type", tel_std_type);
                cmd.Parameters.AddWithValue("@proc_desc", proc_desc);
                cmd.Parameters.AddWithValue("@exec_date", exec_date);

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public int updateForTelCheck(string tel_std_type, string aply_no, string proc_desc, DateTime exec_date
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = "";


                    sql = @"
UPDATE FAP_VE_TRACE
SET  FAP_VE_TRACE.status = case when m.status = '1' then '1'
                                when m.status = '2' then '2'
					            else '3' end
   , FAP_VE_TRACE.exec_date = @exec_date
   , FAP_VE_TRACE.update_id = his.appr_id
   , FAP_VE_TRACE.update_datetime = his.approve_datetime
FROM FAP_VE_TRACE m join FAP_TEL_CHECK_HIS his on his.system = m.system and his.check_acct_short = m.check_acct_short and his.check_no = m.check_no
    WHERE his.aply_no = @aply_no
      AND his.tel_std_type = @tel_std_type
      and his.appr_stat = '2'
      and m.status <> '1'";
                

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", aply_no);
                cmd.Parameters.AddWithValue("@tel_std_type", tel_std_type);
               // cmd.Parameters.AddWithValue("@proc_desc", proc_desc);
                cmd.Parameters.AddWithValue("@exec_date", exec_date);

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 刪除結案編號  add by daiyu 20200803
        /// </summary>
        /// <param name="closed_no"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delCloseNo(string closed_no, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
  UPDATE FAP_VE_TRACE
    SET closed_no = ''
       ,closed_desc = ''
  FROM FAP_VE_TRACE
    WHERE closed_no = @closed_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@closed_no", closed_no);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}