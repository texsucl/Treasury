
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
/// 功能說明：FAP_VE_CLOSED_PROC 結案報表踐行程序檔
/// 初版作者：20200804 Daiyu
/// 修改歷程：20200804 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeClosedProcDao
    {
        /// <summary>
        /// 把暫存檔的資料搬到正式檔
        /// </summary>
        /// <param name="closed_no"></param>
        /// <param name="aply_no"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insertFromHis(string closed_no, string aply_no
          , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
  INSERT INTO FAP_VE_CLOSED_PROC
( closed_no
 ,practice
 ,cert_doc
 ,exec_date
 ,proc_desc
 ,update_id
 ,update_datetime)
     
SELECT closed_no
      ,practice
      ,cert_doc
      ,exec_date
      ,proc_desc
      ,update_id
      ,update_datetime
  FROM FAP_VE_CLOSED_PROC_HIS
WHERE closed_no = @closed_no
  and aply_no = @aply_no
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@closed_no", closed_no);
                cmd.Parameters.AddWithValue("@aply_no", aply_no);

                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public List<FAP_VE_CLOSED_PROC> qryByClosedNo(string closed_no)
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
                    List<FAP_VE_CLOSED_PROC> rows = db.FAP_VE_CLOSED_PROC
                        .Where(x => x.closed_no.Equals(closed_no)).ToList();

                    return rows;
                }
            }
        }





    }
}