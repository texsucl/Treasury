
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
/// 功能說明：FAP_VE_TRACE_POLI 逾期未兌領清理保單明細檔
/// 初版作者：20190711 Daiyu
/// 修改歷程：20190711 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPVeTracePoliDao
    {

        public FAP_VE_TRACE_POLI qryForOAP0011(OAP0011Model model)
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
                    FAP_VE_TRACE_POLI d = db.FAP_VE_TRACE_POLI
                        .Where(x => x.check_no == model.check_no & x.check_acct_short == model.check_acct_short).FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_VE_TRACE_POLI();
                }
            }
        }


        public FAP_VE_TRACE_POLI qryForOAP0042(string check_no, string check_acct_short, string policy_no, int policy_seq, string id_dup)
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
                    FAP_VE_TRACE_POLI d = db.FAP_VE_TRACE_POLI
                        .Where(x => x.check_no == check_no & x.check_acct_short == check_acct_short
                        & x.policy_no == policy_no & x.policy_seq == policy_seq & x.id_dup == id_dup).FirstOrDefault();

                    if (d != null)
                        return d;
                    else
                        return new FAP_VE_TRACE_POLI();
                }
            }
        }



        public int updForOAP0042(TelDispatchRptModel d, SqlConnection conn, SqlTransaction transaction)
        {


            try
            {
                string sql = @"
update FAP_VE_TRACE_POLI
  set sysmark = @sysmark
     ,send_id = @send_id
     ,send_unit = @send_unit
     ,send_name = @send_name
     ,mobile = @send_tel
     ,appl_id = @appl_id
     ,appl_name = @appl_name
     ,ins_id = @ins_id
     ,ins_name = @ins_name
where check_no = @check_no
  and check_acct_short = @check_acct_short
  and policy_no  = @policy_no 
  and policy_seq = @policy_seq
  and id_dup = @id_dup
  
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_no", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@id_dup", System.Data.SqlDbType.VarChar).Value = (Object)d.id_dup ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@sysmark", System.Data.SqlDbType.VarChar).Value = (Object)d.sysmark ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@send_id", System.Data.SqlDbType.VarChar).Value = (Object)d.send_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@send_unit", System.Data.SqlDbType.VarChar).Value = (Object)d.send_unit ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@send_name", System.Data.SqlDbType.VarChar).Value = (Object)d.send_name ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@send_tel", System.Data.SqlDbType.VarChar).Value = (Object)d.send_tel ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@appl_id", System.Data.SqlDbType.VarChar).Value = (Object)d.appl_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@appl_name", System.Data.SqlDbType.VarChar).Value = (Object)d.appl_name ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ins_id", System.Data.SqlDbType.VarChar).Value = (Object)d.ins_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ins_name", System.Data.SqlDbType.VarChar).Value = (Object)d.ins_name ?? DBNull.Value;




                cmd.Parameters.AddWithValue("@main_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.main_amt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@o_paid_cd", System.Data.SqlDbType.VarChar).Value = (Object)d.o_paid_cd ?? DBNull.Value;

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public int insert(FAP_VE_TRACE_POLI d, SqlConnection conn, SqlTransaction transaction)
        {
            

            try
            {
                string sql = @"
INSERT INTO FAP_VE_TRACE_POLI
 (check_no
 ,check_acct_short
 ,policy_no 
 ,policy_seq
 ,id_dup
 ,member_id
 ,change_id
 ,main_amt  
 ,o_paid_cd
 ) values (
  @check_no
 ,@check_acct_short
 ,@policy_no 
 ,@policy_seq
 ,@id_dup
 ,@member_id
 ,@change_id
 ,@main_amt  
 ,@o_paid_cd
)
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@check_no", System.Data.SqlDbType.VarChar).Value = (Object)d.check_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@check_acct_short", System.Data.SqlDbType.VarChar).Value = (Object)d.check_acct_short ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_no", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_no ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@policy_seq", System.Data.SqlDbType.VarChar).Value = (Object)d.policy_seq ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@id_dup", System.Data.SqlDbType.VarChar).Value = (Object)d.id_dup ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@member_id", System.Data.SqlDbType.VarChar).Value = (Object)d.member_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@change_id", System.Data.SqlDbType.VarChar).Value = (Object)d.change_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@main_amt", System.Data.SqlDbType.Decimal).Value = (Object)d.main_amt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@o_paid_cd", System.Data.SqlDbType.VarChar).Value = (Object)d.o_paid_cd ?? DBNull.Value;

                return cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}