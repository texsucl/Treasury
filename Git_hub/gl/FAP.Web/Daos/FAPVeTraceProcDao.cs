
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
/// 功能說明：FAP_VE_TRACE_PROC 逾期未兌領清理記錄歷程檔
/// 初版作者：20190618 Daiyu
/// 修改歷程：20190618 Daiyu
/// 需求單號：
/// 初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTrackProcDao
    {
       

        public void updateForOAP0002(string check_acct_short, string check_no, string paid_id
          , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
  UPDATE FAP_VE_TRACE_PROC
    SET paid_id = @paid_id
    WHERE check_acct_short = @check_acct_short
	  AND check_no = @check_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@paid_id", paid_id);

                cmd.Parameters.AddWithValue("@check_acct_short", check_acct_short);
                cmd.Parameters.AddWithValue("@check_no", check_no);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<OAP0008Model> qryForOAP0008(string paid_id, string[] status) {
            bool bStatus = false;
            if (status == null)
                bStatus = true;


            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE
                                //join d in db.FAP_VE_TRACE_PROC on new { m.check_no, m.check_acct_short } equals new { d.check_no, d.check_acct_short }
                                join cProc in db.FAP_VE_TRACE_PROC on new { m.check_no, m.check_acct_short } equals new { cProc.check_no, cProc.check_acct_short } into psProc
                                from d in psProc.DefaultIfEmpty()


                                where m.paid_id == paid_id
                                   & (bStatus || (!bStatus & status.Contains(m.status)))

                                select new OAP0008Model
                                {
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    paid_name = m.paid_name,
                                    level_1 = m.level_1,
                                    level_2 = m.level_2,
                                    practice = d.practice,
                                    practice_1 = d.practice,
                                    cert_doc = d.cert_doc,
                                    cert_doc_1 = d.cert_doc,
                                    exec_date = d.exec_date == null ? "" : SqlFunctions.DateName("year", d.exec_date) + "/" +
                                                         SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                         SqlFunctions.DateName("day", d.exec_date).Trim(),
                                    exec_date_1 = d.exec_date == null ? "" : SqlFunctions.DateName("year", d.exec_date) + "/" +
                                                         SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                         SqlFunctions.DateName("day", d.exec_date).Trim(),
                                    proc_desc = d.proc_desc,
                                    data_status = d.data_status
                                }).Distinct().OrderBy(x => x.exec_date.Length).ThenBy(x => x.exec_date).ThenBy(x => x.practice).ToList();

                    return rows;

                }
            }

        }

        public List<OAP0010DModel> qryByCheckNo(string check_no, string check_acct_short, dbFGLEntities db)
        {
            var rows = (from d in db.FAP_VE_TRACE_PROC
                        where d.check_no == check_no
                           & d.check_acct_short == check_acct_short

                        select new OAP0010DModel
                        {
                            practice = d.practice,
                            cert_doc = d.cert_doc,
                            proc_desc = d.proc_desc,
                            exec_date = d.exec_date == null ? "" : (SqlFunctions.DatePart("year", d.exec_date) - 1911) + "/" +
                                                 SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                 SqlFunctions.DateName("day", d.exec_date).Trim()
                        }).Distinct().ToList();

            return rows;

        }


        public List<OAP0010DModel> qryByCheckNo(string check_no, string check_acct_short)
        {

            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from d in db.FAP_VE_TRACE_PROC
                                where d.check_no == check_no
                                   & d.check_acct_short == check_acct_short

                                select new OAP0010DModel
                                {
                                    practice = d.practice,
                                    cert_doc = d.cert_doc,
                                    proc_desc = d.proc_desc,
                                    exec_date = d.exec_date == null ? "" : (SqlFunctions.DatePart("year", d.exec_date) - 1911) + "/" +
                                                         SqlFunctions.DatePart("m", d.exec_date) + "/" +
                                                         SqlFunctions.DateName("day", d.exec_date).Trim(),
                                    update_id = d.update_id,
                                    update_datetime = d.update_datetime == null ? "" : (SqlFunctions.DatePart("year", d.update_datetime) - 1911) + "/" +
                                                         SqlFunctions.DatePart("m", d.update_datetime) + "/" +
                                                          SqlFunctions.DateName("day", d.update_datetime).Trim() + " " +
                                                                          SqlFunctions.DateName("hh", d.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("n", d.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("s", d.update_datetime).Trim()
                                }).Distinct().ToList();

                    return rows;

                }
            }

        }

        public List<FAP_VE_TRACE_PROC> qryByCheckNoList(string[] checkArr)
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
                    List<FAP_VE_TRACE_PROC> rows = db.FAP_VE_TRACE_PROC
                        .Where(x => checkArr.Contains(x.check_no)).OrderBy(x => x.practice).ToList();

                    return rows;
                }
            }
        }


        /// <summary>
        /// 查詢特定支票，指定踐行程序的件數
        /// </summary>
        /// <param name="check_acct_short"></param>
        /// <param name="check_no"></param>
        /// <returns></returns>
        public List<OAP0004DModel> qryForSendCnt(List<OAP0004DModel> dataList)
        {
            string[] send_cnt_arr = new string[] { "G1", "G2", "G3", "G4", "G10", "G15" };
            string[] check_list = dataList.Select(x => x.check_no).ToArray();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FAP_VE_TRACE_PROC> rows = db.FAP_VE_TRACE_PROC
                      .Where(x => check_list.Contains(x.check_no)).ToList();
                
                foreach (OAP0004DModel d in dataList) {
                    int cnt = rows.Where(x => x.check_no == d.check_no).Count();

                    d.send_cnt = (d.send_cnt + cnt);
                }
                

                

                return dataList;
            }
        }


        public FAP_VE_TRACE_PROC qryByKey(FAP_VE_TRACE_PROC q)
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
                    FAP_VE_TRACE_PROC d = db.FAP_VE_TRACE_PROC
                        .Where(x => x.paid_id == q.paid_id
                            & x.check_no == q.check_no
                            & x.check_acct_short == q.check_acct_short
                            & x.practice == q.practice
                            & x.cert_doc == q.cert_doc
                            & x.exec_date == q.exec_date)
                        .FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_VE_TRACE_PROC();
                }
            }
        }






        public void insertForOAP0011(string baseCheckNo, string baseCheckAcctShort,string check_no, string check_acct_short
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_TRACE_PROC
           (paid_id
           ,check_no
           ,check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,data_status
           ,update_id
           ,update_datetime)

  SELECT paid_id
           ,@check_no
           ,@check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,'1'
           ,update_id
           ,update_datetime
  FROM FAP_VE_TRACE_PROC
    WHERE check_no = @baseCheckNo
      AND check_acct_short = @baseCheckAcctShort";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(check_acct_short));
                cmd.Parameters.AddWithValue("@baseCheckNo", StringUtil.toString(baseCheckNo));
                cmd.Parameters.AddWithValue("@baseCheckAcctShort", StringUtil.toString(baseCheckAcctShort));


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void insert(DateTime dt, string userId, FAP_VE_TRACE_PROC d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_TRACE_PROC
           (paid_id
           ,check_no
           ,check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,data_status
           ,update_id
           ,update_datetime)
             VALUES
           (@paid_id
           ,@check_no
           ,@check_acct_short
           ,@practice
           ,@cert_doc
           ,@exec_date
           ,@proc_desc
           ,@data_status
           ,@update_id
           ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));
                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void insertForTelProc(string tel_std_type, string tel_proc_no, string practice, string cert_doc, string proc_desc
            , DateTime exec_date, string update_id, DateTime update_datetime, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_TRACE_PROC
           (paid_id
           ,check_no
           ,check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,data_status
           ,update_id
           ,update_datetime)

  SELECT distinct 
         m.paid_id
       , m.check_no
       , m.check_acct_short
       , @practice
       , @cert_doc
       , @exec_date
       , @proc_desc
       ,'1'
       , @update_id
       , @update_datetime
  FROM FAP_TEL_CHECK his join FAP_VE_TRACE m on his.system = m.system and his.check_acct_short = m.check_acct_short and his.check_no = m.check_no
    WHERE his.tel_proc_no = @tel_proc_no
      AND his.tel_std_type = @tel_std_type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));
                cmd.Parameters.AddWithValue("@tel_proc_no", StringUtil.toString(tel_proc_no));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", exec_date);
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(proc_desc));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(update_id));
                cmd.Parameters.AddWithValue("@update_datetime", update_datetime);

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void insertTelCheck(string tel_std_type, string aply_no, string practice, string cert_doc, string proc_desc
            , DateTime exec_date, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_VE_TRACE_PROC
           (paid_id
           ,check_no
           ,check_acct_short
           ,practice
           ,cert_doc
           ,exec_date
           ,proc_desc
           ,data_status
           ,update_id
           ,update_datetime)

  SELECT distinct 
         m.paid_id
       , m.check_no
       , m.check_acct_short
       , @practice
       , @cert_doc
       , @exec_date
       , @proc_desc
       ,'1'
       , his.appr_id
       , his.approve_datetime
  FROM FAP_TEL_CHECK_HIS his join FAP_VE_TRACE m on his.system = m.system and his.check_acct_short = m.check_acct_short and his.check_no = m.check_no
    WHERE his.aply_no = @aply_no
      AND his.tel_std_type = @tel_std_type
      and his.appr_stat = '2'";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@tel_std_type", StringUtil.toString(tel_std_type));
                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", exec_date);
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(proc_desc));

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        public void deleteForOAP0008(FAP_VE_TRACE_PROC_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
DELETE FAP_VE_TRACE_PROC
 WHERE paid_id = @paid_id
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

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updateForOAP0008(DateTime dt, FAP_VE_TRACE_PROC_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_TRACE_PROC
  SET data_status = @data_status
     ,practice = @practice_1
     ,cert_doc = @cert_doc_1
     ,exec_date = @exec_date_1
     ,proc_desc = @proc_desc
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE paid_id = @paid_id
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

                cmd.Parameters.AddWithValue("@data_status", "1");
                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice_1", StringUtil.toString(d.practice_1));
                cmd.Parameters.AddWithValue("@cert_doc_1", StringUtil.toString(d.cert_doc_1));
                cmd.Parameters.AddWithValue("@exec_date_1", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date_1 ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));
                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void update(DateTime dt, string userId, FAP_VE_TRACE_PROC d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_TRACE_PROC
  SET proc_desc = @proc_desc
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE paid_id = @paid_id
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

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", StringUtil.toString(d.proc_desc));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updateStatus(DateTime dt, string userId, FAP_VE_TRACE_PROC d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_TRACE_PROC
  SET data_status = @data_status
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE paid_id = @paid_id
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

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                cmd.Parameters.AddWithValue("@practice", StringUtil.toString(d.practice));
                cmd.Parameters.AddWithValue("@cert_doc", StringUtil.toString(d.cert_doc));
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.Date).Value = (System.Object)d.exec_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@data_status", StringUtil.toString(d.data_status));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}