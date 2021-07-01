
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
/// 功能說明：FAP_VE_CLOSED_PROC_HIS 結案報表踐行程序暫存檔
/// 初版作者：20200804 Daiyu
/// 修改歷程：20200804 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeClosedProcHisDao
    {
        /// <summary>
        /// 依"結案編號"查該結案報表的踐行程序
        /// </summary>
        /// <param name="closed_no"></param>
        /// <param name="apprStat"></param>
        /// <returns></returns>
        public List<FAP_VE_CLOSED_PROC_HIS> qryByClosedNo(string closed_no, string[] apprStat)
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
                    List<FAP_VE_CLOSED_PROC_HIS> rows = db.FAP_VE_CLOSED_PROC_HIS
                        .Where(x => x.closed_no.Equals(closed_no) & apprStat.Contains(x.appr_stat)).ToList();

                    return rows;
                }
            }
        }



        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int insert(FAP_VE_CLOSED_PROC_HIS d, SqlConnection conn, SqlTransaction transaction)
        {
            

            try
            {
                string sql = @"
INSERT INTO FAP_VE_CLOSED_PROC_HIS
 (aply_no
 ,closed_no
 ,practice
 ,cert_doc
 ,exec_date
 ,proc_desc
 ,appr_stat
 ,update_id
 ,update_datetime

 ) values (
  @aply_no
 ,@closed_no
 ,@practice
 ,@cert_doc
 ,@exec_date
 ,@proc_desc
 ,@appr_stat
 ,@update_id
 ,@update_datetime
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", System.Data.SqlDbType.VarChar).Value = (Object)d.aply_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@closed_no", System.Data.SqlDbType.VarChar).Value = (Object)d.closed_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@practice", System.Data.SqlDbType.VarChar).Value = (Object)d.practice ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@cert_doc", System.Data.SqlDbType.VarChar).Value = (Object)d.cert_doc ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@proc_desc", System.Data.SqlDbType.NVarChar).Value = (Object)d.proc_desc ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@exec_date", System.Data.SqlDbType.DateTime).Value = d.exec_date == null ? DBNull.Value : (Object)d.exec_date;
                cmd.Parameters.AddWithValue("@appr_stat", System.Data.SqlDbType.VarChar).Value = (Object)d.appr_stat ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@update_id", System.Data.SqlDbType.VarChar).Value = (Object)d.update_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = d.update_datetime == null ? DBNull.Value : (Object)d.update_datetime;


                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 異動覆核狀態(核可/駁回)
        /// </summary>
        /// <param name="appr_stat"></param>
        /// <param name="dt"></param>
        /// <param name="userId"></param>
        /// <param name="closed_no"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateApprStat(string appr_stat, DateTime dt, string userId, string aply_no
           , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_VE_CLOSED_PROC_HIS
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

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
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