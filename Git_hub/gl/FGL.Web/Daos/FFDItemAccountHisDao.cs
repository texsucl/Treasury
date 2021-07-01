
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;


/// <summary>
/// 功能說明：FFD_ITEM_ACCOUNT_HIS
/// -----------------------------------------------
/// 修改歷程：20200110 Daiyu
/// 需求單號：201906120463-
/// 修改內容：初版
/// </summary>
/// 

namespace FGL.Web.Daos
{
    public class FFDItemAccountHisDao
    {




        public void insert(FFD_ITEM_ACCOUNT_HIS d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FFD_ITEM_ACCOUNT_HIS
( aply_no
, exec_action
, item
, item_desc
, item_kind
, fund_ac_corp_no
, item_account
, item_transfer
, item_effective_date
, item_expiration_date)
 VALUES
( @aply_no
, @exec_action
, @item
, @item_desc
, @item_kind
, @fund_ac_corp_no
, @item_account
, @item_transfer
, @item_effective_date
, @item_expiration_date)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@aply_no", d.aply_no);
                cmd.Parameters.AddWithValue("@exec_action", d.exec_action);
                cmd.Parameters.AddWithValue("@item", d.item);
                cmd.Parameters.AddWithValue("@item_desc", d.item_desc);
                cmd.Parameters.AddWithValue("@item_kind", d.item_kind == "5" ? "1" : "2");
                cmd.Parameters.AddWithValue("@fund_ac_corp_no", d.fund_ac_corp_no);
                cmd.Parameters.AddWithValue("@item_account", d.item_account);
                cmd.Parameters.AddWithValue("@item_transfer", d.item_transfer);
                cmd.Parameters.AddWithValue("@item_effective_date", d.item_effective_date);
                cmd.Parameters.AddWithValue("@item_expiration_date", d.item_expiration_date);

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}