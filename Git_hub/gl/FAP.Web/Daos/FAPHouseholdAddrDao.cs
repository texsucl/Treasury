
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
/// 功能說明：FAP_HOUSEHOLD_ADDR 戶政查詢地址檔
/// 初版作者：20190617 Daiyu
/// 修改歷程：20190617 Daiyu
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Daos
{
    public class FAPHouseholdAddrDao
    {

        public FAP_HOUSEHOLD_ADDR qryByKey(string paid_id, string addr_type)
        {
            bool bAddrType = true;
            if (!"".Equals(addr_type))
                bAddrType = false;

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAP_HOUSEHOLD_ADDR row = db.FAP_HOUSEHOLD_ADDR
                        .Where(x => x.paid_id == paid_id
                        & (bAddrType || (!bAddrType &x.addr_type == addr_type))
                        ).FirstOrDefault();

                    if (row != null)
                        return row;
                    else
                        return new FAP_HOUSEHOLD_ADDR();
                }
            }
        }


        public List<FAP_HOUSEHOLD_ADDR> qryByPaidId(string paid_id, string addr_type)
        {
            bool bAddrType = true;
            if (!"".Equals(addr_type))
                bAddrType = false;

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<FAP_HOUSEHOLD_ADDR> row = db.FAP_HOUSEHOLD_ADDR
                        .Where(x => x.paid_id == paid_id
                        & (bAddrType || (!bAddrType & x.addr_type == addr_type))).ToList();

              
                        return row;
                  
                }
            }
        }

        public void insert(DateTime dt, string userId, OAP0009Model d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO FAP_HOUSEHOLD_ADDR
           (paid_id
           ,addr_type
           ,paid_name
           ,zip_code
           ,address
           ,update_id
           ,update_datetime)
             VALUES
           (@paid_id
           ,@addr_type
           ,@paid_name
           ,@zip_code
           ,@address
           ,@update_id
           ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@addr_type", StringUtil.toString(d.addr_type));
                cmd.Parameters.AddWithValue("@paid_name", StringUtil.toString(d.paid_name));
                cmd.Parameters.AddWithValue("@zip_code", StringUtil.toString(d.zip_code));
                cmd.Parameters.AddWithValue("@address", StringUtil.toString(d.address));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(userId));
                cmd.Parameters.AddWithValue("@update_datetime", dt);


                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        public void update(DateTime dt, string userId, OAP0009Model d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FAP_HOUSEHOLD_ADDR
  SET paid_name = @paid_name
     ,zip_code = @zip_code
     ,address = @address
     ,update_id = @update_id
     ,update_datetime = @update_datetime
 WHERE paid_id = @paid_id
  and addr_type = @addr_type";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("@paid_id", StringUtil.toString(d.paid_id));
                cmd.Parameters.AddWithValue("@addr_type", StringUtil.toString(d.addr_type));
                cmd.Parameters.AddWithValue("@paid_name", StringUtil.toString(d.paid_name));
                cmd.Parameters.AddWithValue("@zip_code", StringUtil.toString(d.zip_code));
                cmd.Parameters.AddWithValue("@address", StringUtil.toString(d.address));
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