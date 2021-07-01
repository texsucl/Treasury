
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FAP.Web.Models;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;
using FAP.Web.ViewModels;

namespace FAP.Web.Daos
{
    public class FAPPpaaDHisDao
    {
        public List<OAP0002PoliModel> qryAplyNo(string aplyNo)
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
                    var his = (from m in db.FAP_PPAA_D_HIS 
                               where 1 == 1
                               & m.aply_no == aplyNo


                               select new OAP0002PoliModel
                               {
                                   temp_id = m.system + "|"
                                    + m.source_op + "|"
                                    + m.policy_no + "|"
                                    + m.policy_seq + "|"
                                    + m.id_dup + "|"
                                    + m.member_id + "|"
                                    + m.change_id + "|"
                                    + m.paid_id + "|"
                                    + m.check_no,
                                   policy_no = m.policy_no_aft,
                                   policy_seq = m.policy_seq_aft.ToString(),
                                   id_dup = m.id_dup_aft,
                                   change_id = m.change_id_aft,
                                   member_id = m.member_id_aft,
                                   main_amt = m.main_amt.ToString()

                               }).Distinct().ToList<OAP0002PoliModel>();

                    return his;
                }
            }

        }

        /// <summary>
        /// 新增覆核資料
        /// </summary>
        /// <param name="d"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(List<FAP_PPAA_D_HIS> dataList, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO [FAP_PPAA_D_HIS]
           (aply_no
           ,system
           ,source_op
           ,policy_no
           ,policy_seq
           ,id_dup
           ,member_id
           ,change_id
           ,paid_id
           ,main_amt
           ,check_no
           ,check_acct_short
           ,policy_no_aft
           ,policy_seq_aft
           ,id_dup_aft
           ,member_id_aft
           ,change_id_aft)
             VALUES
           (@aply_no
           ,@system
           ,@source_op
           ,@policy_no
           ,@policy_seq
           ,@id_dup
           ,@member_id
           ,@change_id
           ,@paid_id
           ,@main_amt
           ,@check_no
           ,@check_acct_short
           ,@policy_no_aft
           ,@policy_seq_aft
           ,@id_dup_aft
           ,@member_id_aft
           ,@change_id_aft)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                foreach (FAP_PPAA_D_HIS d in dataList) {
                    cmd.Parameters.Clear();

                    cmd.Parameters.AddWithValue("@aply_no", StringUtil.toString(d.aply_no));
                    cmd.Parameters.AddWithValue("@system", StringUtil.toString(d.system));
                    cmd.Parameters.AddWithValue("@source_op", StringUtil.toString(d.source_op));
                    cmd.Parameters.AddWithValue("@policy_no", StringUtil.toString(d.policy_no));
                    cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.Int).Value = (Object)d.policy_seq ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@id_dup", StringUtil.toString(d.id_dup));
                    cmd.Parameters.AddWithValue("@member_id", StringUtil.toString(d.member_id));
                    cmd.Parameters.AddWithValue("@change_id", StringUtil.toString(d.change_id));
                    cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                    cmd.Parameters.AddWithValue("@main_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.main_amt ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@check_no", StringUtil.toString(d.check_no));
                    cmd.Parameters.AddWithValue("@check_acct_short", StringUtil.toString(d.check_acct_short));
                    cmd.Parameters.AddWithValue("@policy_no_aft", StringUtil.toString(d.policy_no_aft));
                    cmd.Parameters.AddWithValue("@policy_seq_aft", System.Data.SqlDbType.Int).Value = (Object)d.policy_seq_aft ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@id_dup_aft", StringUtil.toString(d.id_dup_aft));
                    cmd.Parameters.AddWithValue("@member_id_aft", StringUtil.toString(d.member_id_aft));
                    cmd.Parameters.AddWithValue("@change_id_aft", StringUtil.toString(d.change_id_aft));


                    cmd.ExecuteNonQuery();
                }
                

            }
            catch (Exception e)
            {

                throw e;
            }


        }

        


        
    }
}