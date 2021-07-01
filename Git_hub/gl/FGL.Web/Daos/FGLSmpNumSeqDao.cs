
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;

/// <summary>
/// 功能說明：FGL_SMP_NUM_SEQ 會科編碼流水號資料檔
/// 初版作者： Daiyu
/// 修改歷程： Daiyu
/// 需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191202 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：將畫面上的險種科目，與相對應的【會科取號檔】中同一規則的下一個科目編號相比，若畫面號碼小於取號檔，不回寫【會科取號檔】
/// </summary>
/// 

namespace FGL.Web.Daos
{
    public class FGLSmpNumSeqDao
    {


        public FGL_SMP_NUM_SEQ qryForOGL00007(decimal seqItemAcct56, decimal seqItemAcct7, decimal seqItemAcct810)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FGL_SMP_NUM_SEQ d = db.FGL_SMP_NUM_SEQ
                    .Where(x => x.seq_item_acct_5_6 == seqItemAcct56 
                        & x.seq_item_acct_7 == seqItemAcct7
                        & x.seq_item_acct_8_10 == seqItemAcct810
                    ).FirstOrDefault();

                return d;
            }
        }


        public void insert(FGL_SMP_NUM_SEQ seq, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMP_NUM_SEQ
                   (seq_item_acct_5_6
           ,seq_item_acct_7
           ,seq_item_acct_8_10
           ,next_seq
           ,item
           ,item_acct
           ,update_id
           ,update_datetime)
             VALUES
                  (@seq_item_acct_5_6
                   ,@seq_item_acct_7
                   ,@seq_item_acct_8_10
                   ,@next_seq
                   ,@item
                   ,@item_acct
                   ,@update_id
                   ,@update_datetime)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@seq_item_acct_5_6", seq.seq_item_acct_5_6);
                cmd.Parameters.AddWithValue("@seq_item_acct_7", seq.seq_item_acct_7);
                cmd.Parameters.AddWithValue("@seq_item_acct_8_10", seq.seq_item_acct_8_10);
                cmd.Parameters.AddWithValue("@next_seq", seq.next_seq);
                cmd.Parameters.AddWithValue("@item", StringUtil.toString(seq.item));
                cmd.Parameters.AddWithValue("@item_acct", StringUtil.toString(seq.item_acct));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(seq.update_id));
                cmd.Parameters.Add("@update_datetime", System.Data.SqlDbType.DateTime).Value = (System.Object)seq.update_datetime ?? System.DBNull.Value;
               
                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void update(FGL_SMP_NUM_SEQ seq, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE FGL_SMP_NUM_SEQ
        set next_seq = @next_seq
           ,item = @item
           ,item_acct = @item_acct
           ,update_id = @update_id
           ,update_datetime = @update_datetime
        where seq_item_acct_5_6 = @seq_item_acct_5_6
          and seq_item_acct_7 = @seq_item_acct_7
          and seq_item_acct_8_10 = @seq_item_acct_8_10
          and next_seq < @next_seq";    //modify by daiyu 20191202

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@seq_item_acct_5_6", seq.seq_item_acct_5_6);
                cmd.Parameters.AddWithValue("@seq_item_acct_7", seq.seq_item_acct_7);
                cmd.Parameters.AddWithValue("@seq_item_acct_8_10", seq.seq_item_acct_8_10);
                cmd.Parameters.AddWithValue("@next_seq", seq.next_seq);
                cmd.Parameters.AddWithValue("@item", StringUtil.toString(seq.item));
                cmd.Parameters.AddWithValue("@item_acct", StringUtil.toString(seq.item_acct));
                cmd.Parameters.AddWithValue("@update_id", StringUtil.toString(seq.update_id));
                cmd.Parameters.Add("@update_datetime", System.Data.SqlDbType.DateTime).Value = (System.Object)seq.update_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}