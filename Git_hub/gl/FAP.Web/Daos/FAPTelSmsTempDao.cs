
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
/// 功能說明：FAP_TEL_SMS_TEMP一年以下簡訊通知暫存檔
/// 初版作者：20200930 Daiyu
/// 修改歷程：20200930 Daiyu
/// 需求單號：202008120153-01
///           初版
/// ---------------------------------------------------------------------
/// 修改歷程：
/// 需求單號：
/// ---------------------------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelSmsTempDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void Insert(List<FAP_TEL_SMS_TEMP> dataList, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                string sql = @"
INSERT INTO FAP_TEL_SMS_TEMP
 (tel_std_aply_no
 ,check_no
 ,check_acct_short
 ,check_date
 ,check_amt
 ,o_paid_cd
 ,paid_id
 ,paid_name
 ,system
 ,policy_no
 ,policy_seq
 ,id_dup
 ,appl_id
 ,appl_name
 ,ins_id
 ,ins_name
 ,sec_stat
 ,mobile
 ) values (
  @tel_std_aply_no
 ,@check_no
 ,@check_acct_short
 ,@check_date
 ,@check_amt
 ,@o_paid_cd
 ,@paid_id
 ,@paid_name
 ,@system
 ,@policy_no
 ,@policy_seq
 ,@id_dup
 ,@appl_id
 ,@appl_name
 ,@ins_id
 ,@ins_name
 ,@sec_stat
 ,@mobile
)
";
                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                foreach (FAP_TEL_SMS_TEMP d in dataList)
                {
                    try
                    {
                        cmd.Parameters.Clear();
                        //cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@tel_std_aply_no", System.Data.SqlDbType.VarChar).Value = (Object)d.tel_std_aply_no;
                        cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no;
                        cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@check_date", System.Data.SqlDbType.Date).Value = (Object)d.check_date ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@check_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.check_amt ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@o_paid_cd", System.Data.SqlDbType.VarChar).Value = (Object)d.o_paid_cd ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@paid_id", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_id ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@paid_name", System.Data.SqlDbType.NVarChar).Value = (Object)d.paid_name ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)d.system ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@policy_no", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_no ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_seq ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@id_dup", System.Data.SqlDbType.VarChar).Value = (Object)d.id_dup ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@appl_id", System.Data.SqlDbType.VarChar).Value = (Object)d.appl_id ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@appl_name", System.Data.SqlDbType.NVarChar).Value = (Object)d.appl_name ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@ins_id", System.Data.SqlDbType.VarChar).Value = (Object)d.ins_id ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@ins_name", System.Data.SqlDbType.NVarChar).Value = (Object)d.ins_name ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@sec_stat", System.Data.SqlDbType.VarChar).Value = (Object)d.sec_stat ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@mobile", System.Data.SqlDbType.VarChar).Value = (Object)d.mobile ?? DBNull.Value;
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e) {
                        logger.Error(e.ToString());
                    }
                    
                }
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                //throw e;
            }

        }



        public void delete(SqlConnection conn)
        {
            try
            {

                string sql = @"
DELETE FAP_TEL_SMS_TEMP
 WHERE tel_std_aply_no = ''";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void updateAplyNo( string aply_no, string check_no, string check_acct_short, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
UPDATE FAP_TEL_SMS_TEMP
  SET tel_std_aply_no = @aply_no
 WHERE tel_std_aply_no = ''
 and check_no = @check_no
 and check_acct_short = @check_acct_short";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(check_no));
                cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(check_acct_short));

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public List<TelDispatchRptModel> qrySmsNotifyRpt(string rpt_cnt_tp, string aply_no)
        {
    
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_TEL_SMS_TEMP
                                where 1 == 1
                                  & m.tel_std_aply_no == aply_no
                                select new TelDispatchRptModel
                                {
                                    temp_id = rpt_cnt_tp == "P" ? (m.paid_id == "" ? m.check_no : m.paid_id)  : m.check_no,
                                    //fsc_range = m.fsc_range,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    check_date = m.check_date == null ? "" : SqlFunctions.DatePart("year", m.check_date) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    check_amt = m.check_amt == null ? (Decimal)0 :(Decimal)m.check_amt,
                                    o_paid_cd = m.o_paid_cd,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    system = m.system,
                                    policy_no = m.policy_no,
                                    policy_seq = m.policy_seq,
                                    id_dup = m.id_dup,
                                    appl_id = m.appl_id,
                                    appl_name = m.appl_name,
                                    ins_id = m.ins_id,
                                    ins_name = m.ins_name,
                                    sec_stat = m.sec_stat,
                                    policy_mobile = m.mobile
                                }).ToList();


                    return rows;

                }
            }

        }



    }
}