
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
/// 功能說明：FAP_VE_TRACE_IMP 逾期未兌領清理記錄批次匯入檔
/// 初版作者：20200922 Daiyu
/// 修改歷程：20200922 Daiyu
/// 需求單號：202008120153-00
/// 初版
/// ---------------------------------------------------------------------
/// 修改歷程：
/// 需求單號：
///          
/// ---------------------------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTraceImpDao
    {


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int insert(OAP0050Model d, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE_IMP
 (aply_no
 ,system    
 ,check_no
 ,check_acct_short
 ,policy_no 
 ,policy_seq
 ,id_dup
 ,member_id
 ,change_id
 ,paid_id
 ,paid_name
 ,main_amt  
 ,check_amt
 ,check_date
 ,o_paid_cd
 ,re_paid_date
 ,re_paid_type
 ,fsc_range
 ,imp_desc
 ,update_id
 ,update_datetime
 ,appr_stat
 ,re_paid_date_n
 ) values (
  @aply_no
 ,@system    
 ,@check_no
 ,@check_acct_short
 ,@policy_no 
 ,@policy_seq
 ,@id_dup
 ,@member_id
 ,@change_id
 ,@paid_id
 ,@paid_name
 ,@main_amt  
 ,@check_amt
 ,@check_date
 ,@o_paid_cd
 ,@re_paid_date
 ,@re_paid_type
 ,@fsc_range
 ,@imp_desc
 ,@update_id
 ,@update_datetime
 ,@appr_stat
 ,@re_paid_date_n
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", System.Data.SqlDbType.VarChar).Value = (Object)d.aply_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@system", System.Data.SqlDbType.VarChar).Value = (Object)d.system ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_no", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.Int).Value = (Object)d.policy_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@id_dup", System.Data.SqlDbType.VarChar).Value = (Object)d.id_dup ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@member_id", System.Data.SqlDbType.VarChar).Value = (Object)d.member_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@change_id", System.Data.SqlDbType.VarChar).Value = (Object)d.change_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_id", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@paid_name", System.Data.SqlDbType.VarChar).Value = (Object)d.paid_name ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@main_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.main_amt ?? 0;
                cmd.Parameters.AddWithValue("@check_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.check_amt ?? 0;
                cmd.Parameters.AddWithValue("@check_date", System.Data.SqlDbType.DateTime).Value = (Object)d.check_date  ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@o_paid_cd", System.Data.SqlDbType.VarChar).Value = (Object)d.o_paid_cd ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@re_paid_date", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date == "" ? DBNull.Value: (Object)d.re_paid_date;
                cmd.Parameters.AddWithValue("@re_paid_type", System.Data.SqlDbType.VarChar).Value = (Object)d.re_paid_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@fsc_range", System.Data.SqlDbType.VarChar).Value = (Object)d.fsc_range ?? "0";
                cmd.Parameters.AddWithValue("@imp_desc", System.Data.SqlDbType.NVarChar).Value = (Object)d.imp_desc ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", d.update_datetime);
                cmd.Parameters.AddWithValue("@appr_stat", "1");

                cmd.Parameters.AddWithValue("@re_paid_date_n", System.Data.SqlDbType.DateTime).Value = (Object)d.re_paid_date_n == "" ? DBNull.Value : (Object)d.re_paid_date_n;



                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// OAP0050A EXCEL上傳清理記錄檔覆核作業 --待覆核清單查詢
        /// </summary>
        /// <param name="appr_stat"></param>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        public List<OAP0050Model> qryForOAP0050AS(string appr_stat, string aply_no)
        {
            bool bApprStat = StringUtil.toString(appr_stat) == "" ? true : false;
            bool bAplyNo = StringUtil.toString(aply_no) == "" ? true : false;


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE_IMP
                                where 1 == 1
                                & (bApprStat || (!bApprStat & m.appr_stat == appr_stat))
                                & (bAplyNo || (!bAplyNo & m.aply_no == aply_no))

                                group m by new { m.aply_no, m.update_id, m.update_datetime } into g

                                select new OAP0050Model
                                {
                                    aply_no = g.Key.aply_no,
                                    update_id = g.Key.update_id,
                                    update_datetime = g.Key.update_datetime == null ? "" : SqlFunctions.DateName("year", g.Key.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", g.Key.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", g.Key.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", g.Key.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", g.Key.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", g.Key.update_datetime).Trim(),
                                    cnt = g.Count()
                                }).ToList<OAP0050Model>();

                    return rows;
                }
            }
        }


        /// <summary>
        /// OAP0050A EXCEL上傳清理記錄檔覆核作業 --覆核單明細查詢
        /// </summary>
        /// <param name="appr_stat"></param>
        /// <param name="aply_no"></param>
        /// <returns></returns>
        public List<OAP0050Model> qryForOAP0050AD(string appr_stat, string aply_no)
        {
            bool bApprStat = StringUtil.toString(appr_stat) == "" ? true : false;
            bool bAplyNo = StringUtil.toString(aply_no) == "" ? true : false;


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from m in db.FAP_VE_TRACE_IMP
                                where 1 == 1
                                & (bApprStat || (!bApprStat & m.appr_stat == appr_stat))
                                & (bAplyNo || (!bAplyNo & m.aply_no == aply_no))

                                select new OAP0050Model
                                {
                                    aply_no = m.aply_no,
                                    system = m.system,
                                    check_no = m.check_no,
                                    check_acct_short = m.check_acct_short,
                                    policy_no = m.policy_no,
                                    policy_seq = m.policy_seq.ToString(),
                                    id_dup = m.id_dup,
                                    member_id = m.member_id,
                                    change_id = m.change_id,
                                    paid_id = m.paid_id,
                                    paid_name = m.paid_name,
                                    main_amt = m.main_amt.ToString(),
                                    check_amt = m.check_amt.ToString(),
                                    check_date = m.check_date == null ? "" : (SqlFunctions.DatePart("year", m.check_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.check_date) + "/" +
                                                  SqlFunctions.DateName("day", m.check_date).Trim(),
                                    o_paid_cd = m.o_paid_cd,
                                    re_paid_date = m.re_paid_date == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date).Trim(),
                                    re_paid_date_n = m.re_paid_date_n == null ? "" : (SqlFunctions.DatePart("year", m.re_paid_date_n) - 1911) + "/" +
                                                  SqlFunctions.DatePart("m", m.re_paid_date_n) + "/" +
                                                  SqlFunctions.DateName("day", m.re_paid_date_n).Trim(),
                                    re_paid_type = m.re_paid_type,
                                    fsc_range = m.fsc_range,
                                    imp_desc = m.imp_desc,
                                    update_id = m.update_id,
                                    update_datetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                 SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                 SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                 SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                 SqlFunctions.DateName("s", m.update_datetime).Trim()
                                }).ToList<OAP0050Model>();

                    return rows;
                }
            }
        }



        public void updateApprStat(string appr_stat, DateTime dt, string userId, FAP_VE_TRACE_IMP d
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_TRACE_IMP
  SET appr_stat = @appr_stat
     ,appr_id = @appr_id
     ,approve_datetime = @approve_datetime
 WHERE aply_no = @aply_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
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