
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
/// 功能說明：FAP_VE_TRACE_PROC_HIS 逾期未兌領清理記錄歷程暫存檔
/// 初版作者：20190621 Daiyu
/// 修改歷程：20190621 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTrackProcHisDao
    {

        public void insert(string aply_no, DateTime dt, string userId, FAP_VE_TRACE_PROC_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_TRACE_PROC_HIS
           (aply_no
           ,paid_id
           ,check_no
           ,check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,practice_1
           ,cert_doc_1
           ,exec_date_1
           ,exec_action
           ,appr_stat
           ,update_id
           ,update_datetime
           ,srce_from)
             VALUES
           (@aply_no
           ,@paid_id
           ,@check_no
           ,@check_acct_short
           ,@practice
           ,@cert_doc
           ,@exec_date
           ,@proc_desc
           ,@practice_1
           ,@cert_doc_1
           ,@exec_date_1
           ,@exec_action
           ,@appr_stat
           ,@update_id
           ,@update_datetime
           ,@srce_from)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));

                cmd.Parameters.AddWithValue("@practice_1", StringUtil.toString(d.practice_1));
                cmd.Parameters.AddWithValue("@cert_doc_1", StringUtil.toString(d.cert_doc_1));
                cmd.Parameters.AddWithValue("@exec_date_1", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_action", StringUtil.toString(d.exec_action));
                cmd.Parameters.AddWithValue("@appr_stat", "1");
                cmd.Parameters.AddWithValue("@srce_from", StringUtil.toString(d.srce_from));

                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public FAP_VE_TRACE_PROC_HIS qryByKey(FAP_VE_TRACE_PROC_HIS q)
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
                    FAP_VE_TRACE_PROC_HIS d = db.FAP_VE_TRACE_PROC_HIS
                        .Where(x => x.aply_no == q.aply_no
                            & x.paid_id == q.paid_id
                            & x.check_no == q.check_no
                            & x.check_acct_short == q.check_acct_short
                            & x.practice == q.practice
                            & x.cert_doc == q.cert_doc
                            & x.exec_date == q.exec_date)
                        .FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_VE_TRACE_PROC_HIS();
                }
            }
        }



        /// <summary>
        /// 依"給付對象ID + 覆核狀態"查詢
        /// </summary>
        /// <param name="paid_id"></param>
        /// <param name="appr_stat"></param>
        /// <returns></returns>
        public List<OAP0008Model> qryByForOAP0008A(string paid_id, string[] appr_stat) {
            bool bStatus = false;
            if (appr_stat == null)
                bStatus = true;

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                join mHis in db.FAP_VE_TRACE_HIS on new { m.check_no, m.check_acct_short} equals new { mHis.check_no, mHis.check_acct_short}
                                join d in db.FAP_VE_TRACE_PROC_HIS on new { mHis.aply_no, mHis.check_no, mHis.check_acct_short } equals new { d.aply_no, d.check_no, d.check_acct_short}
                                where m.paid_id == paid_id
                                   & (bStatus || (!bStatus & appr_stat.Contains(d.appr_stat)))

                                select new OAP0008Model
                                {
                                    aply_no = d.aply_no,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    status = m.status,
                                    level_1 = m.level_1,
                                    level_1_1 = mHis.level_1,
                                    level_2 = m.level_2,
                                    level_2_1 = mHis.level_2,
                                    practice = d.practice,
                                    practice_1 = d.practice_1,
                                    cert_doc = d.cert_doc,
                                    cert_doc_1 = d.cert_doc_1,
                                    exec_date = d.exec_date == null ? "" : SqlFunctions.DateName("year", d.exec_date) + "/" +
                                                         SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                         SqlFunctions.DateName("day", d.exec_date).Trim(),
                                    exec_date_1 = d.exec_date == null ? "" : SqlFunctions.DateName("year", d.exec_date_1) + "/" +
                                                         SqlFunctions.DatePart("m", d.exec_date_1) + "/" +
                                                         SqlFunctions.DateName("day", d.exec_date_1).Trim(),
                                    proc_desc = d.proc_desc,
                                    exec_action = d.exec_action,
                                    srce_from = d.srce_from,
                                    update_id = d.update_id,
                                    update_datetime = d.update_datetime == null ? "" : SqlFunctions.DateName("year", d.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", d.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", d.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", d.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", d.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", d.update_datetime).Trim()

                                }).Distinct().OrderBy(x => x.exec_date_1.Length).ThenBy(x => x.exec_date_1).ThenBy(x => x.proc_desc).ToList();

                    return rows;

                }
            }

        }




        public void updateApprStat(string appr_stat, DateTime dt, string userId, FAP_VE_TRACE_PROC_HIS d
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_TRACE_PROC_HIS
  SET appr_stat = @appr_stat
     ,appr_id = @appr_id
     ,approve_datetime = @approve_datetime
 WHERE aply_no = @aply_no
   AND paid_id = @paid_id
   AND check_no = @check_no
   AND check_acct_short = @check_acct_short
   AND practice = @practice
   AND cert_doc = @cert_doc
   AND exec_date = @exec_date
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@appr_stat", appr_stat);
                cmd.Parameters.AddWithValue("@appr_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@approve_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        

    }
}