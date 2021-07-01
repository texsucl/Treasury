
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
/// 功能說明：FAP_TEL_CODE 電訪標準設定檔
/// 初版作者：20200827 Daiyu
/// 修改歷程：20200827 Daiyu
///           需求單號：202008120153-00
///           初版
/// --------------------------------------------------
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPTelCodeDao
    {
        public void updateStatus(string appr_mk, DateTime dt, FAP_TEL_CODE d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_TEL_CODE
  SET data_status = @data_status
     ,update_id = @update_id
     ,update_datetime = @update_datetime";
                sql += appr_mk == "2" ? ", proc_id = @proc_id, std_1 = @std_1, std_2 = @std_2, std_3 = @std_3, appr_id = @appr_id ,approve_datetime = @approve_datetime" : "";
                sql += @" WHERE code_type = @code_type
                   AND code_id = @code_id";


                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@data_status", appr_mk == "1" ? "2" : "1");
                cmd.Parameters.AddWithValue("@update_id", d.update_id);
                cmd.Parameters.AddWithValue("@update_datetime", dt);

                if ("2".Equals(appr_mk))
                {
                    cmd.Parameters.AddWithValue("@proc_id", d.proc_id);
                    cmd.Parameters.AddWithValue("@std_1", System.Data.SqlDbType.Int).Value = (Object)d.std_1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@std_2", System.Data.SqlDbType.Int).Value = (Object)d.std_2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@std_3", System.Data.SqlDbType.Int).Value = (Object)d.std_3 ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@appr_id", d.appr_id);
                    cmd.Parameters.AddWithValue("@approve_datetime", dt);
                }

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(d.code_type));
                cmd.Parameters.AddWithValue("@code_id", StringUtil.toString(d.code_id));



                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<FAP_TEL_CODE> qryByGrp(string code_type)
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
                    List<FAP_TEL_CODE> rows = db.FAP_TEL_CODE.Where(x => x.code_type == code_type)
                        .OrderBy(x => x.code_id.Length).ThenBy(x => x.code_id).ToList();

                    return rows;
                }
            }
        }


        public FAP_TEL_CODE qryByKey(string code_type, string code_id)
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
                    FAP_TEL_CODE d = db.FAP_TEL_CODE
                        .Where(x => x.code_type == code_type & x.code_id == code_id)
                        .FirstOrDefault();

                    return d;
                }
            }
        }


        public void insertForOAP0043A(string aply_no, string code_type, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
INSERT INTO FAP_TEL_CODE
           (code_type
           ,code_id
           ,proc_id
           ,std_1
           ,std_2
           ,std_3
           ,fsc_range
           ,amt_range
           ,data_status
           ,remark
           ,update_id
           ,update_datetime
           ,appr_id
           ,approve_datetime) 
select code_type
      ,code_id
      ,proc_id
      ,std_1
      ,std_2
      ,std_3
      ,fsc_range
      ,amt_range
      ,'1'
      ,remark
      ,update_id
      ,update_datetime
      ,appr_id
      ,approve_datetime
  from FAP_TEL_CODE_HIS
    where aply_no = @aply_no
      and code_type = @code_type
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(aply_no));
                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(code_type));



                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void delForOAP0043(string code_type, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {

                string sql = @"
DELETE FAP_TEL_CODE
 WHERE code_type = @code_type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@code_type", StringUtil.toString(code_type));
                cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }



    }
}