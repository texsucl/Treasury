
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;
using System.Web.Mvc;

/// <summary>
/// 功能說明：FAP_TEL_POLI 保單聯絡電話檔
/// 初版作者：20200827 Daiyu
/// 修改歷程：20200827 Daiyu
///           需求單號：202008120153-00
///           初版
/// --------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelPoliDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<FAP_TEL_POLI> qryByPolicyNo(FAP_TEL_POLI d)
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
                    List<FAP_TEL_POLI> rows = db.FAP_TEL_POLI
                        .Where(x => x.system == d.system & x.policy_no == d.policy_no & x.policy_seq == d.policy_seq & x.id_dup == d.id_dup)
                        .OrderBy(x => x.tel_type).ThenByDescending(x => x.update_datetime).ToList();

                    return rows;
                }
            }
        }



        public void insert(FAP_TEL_POLI d, SqlConnection conn, SqlTransaction transaction)
        {
            FAP_TEL_POLI proc = new FAP_TEL_POLI();
            
            try
            {
                string sql = @"
 INSERT INTO FAP_TEL_POLI (
  system
 ,policy_no 
 ,policy_seq
 ,id_dup
 ,cust_tel
 ,tel_type
 ,update_datetime
)  VALUES (
  @system
 ,@policy_no 
 ,@policy_seq
 ,@id_dup
 ,@cust_tel
 ,@tel_type
 ,@update_datetime
)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();


                cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                cmd.Parameters.AddWithValue("@policy_no", StringUtil.toString(d.policy_no));
                cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.Int).Value = (Object)d.policy_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@id_dup", StringUtil.toString(d.id_dup));
                cmd.Parameters.AddWithValue("@cust_tel", StringUtil.toString(d.cust_tel));
                cmd.Parameters.AddWithValue("@tel_type", StringUtil.toString(d.tel_type));

                if (d.update_datetime.Value.Year.CompareTo(1911) < 0)
                    d.update_datetime = d.update_datetime.Value.AddYears(1911);

                cmd.Parameters.AddWithValue("@update_datetime", System.Data.SqlDbType.DateTime).Value = (Object)d.update_datetime ?? DBNull.Value;


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                //throw e;
            }
        }

    }
}