
using SSO.Web.Models;
using System;
using System.Data.SqlClient;
using SSO.Web.Utils;
using SSO.Web.BO;

/// <summary>
/// 功能說明：處理"個資稽核軌跡記錄檔"存取
/// 初版作者：20170817 黃黛鈺
/// 修改歷程：20170817 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// ==============================================
/// 修改日期/修改人：20190515 daiyu
/// 需求單號：
/// 修改內容：配合金檢議題，稽核軌跡多加寫HOSTNAME
/// ==============================================

/// </summary>
/// 

namespace SSO.Web.Daos
{
    public class PiaLogMainDao
    {

        public int Insert(PIA_LOG_MAIN piaLogMain)
        {

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {

                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    int cnt = Insert(piaLogMain, conn, transaction);
                    transaction.Commit();
                    return cnt;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        public int Insert(PIA_LOG_MAIN piaLogMain, SqlConnection conn, SqlTransaction transaction)
        {

                string sql = @"INSERT INTO [PIA_LOG_MAIN]
           ([TRACKING_TYPE]
           ,[ACCESS_ACCOUNT]
           ,[ACCOUNT_NAME]
           ,[FROM_IP]
           ,[PROGFUN_NAME]
           ,[ACCESSOBJ_NAME]
,[EXECUTION_TYPE]
,[EXECUTION_CONTENT]
,[AFFECT_ROWS]
,[PIA_OWNER1]
,[PIA_OWNER2]
,[PIA_TYPE])
     VALUES
           (@TRACKING_TYPE
           ,@ACCESS_ACCOUNT
           ,@ACCOUNT_NAME
           ,@FROM_IP
           ,@PROGFUN_NAME
           ,@ACCESSOBJ_NAME
,@EXECUTION_TYPE
,@EXECUTION_CONTENT
,@AFFECT_ROWS
,@PIA_OWNER1
,@PIA_OWNER2
,@PIA_TYPE)
";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            CommonUtil commonUtil = new CommonUtil();

            string ip = "";
            try
            {
                //ip = commonUtil.GetFormIp();
                ip = commonUtil.GetFormIp();    //modify by daiyu 20190516
                
            }
            catch (Exception e) {
                
            }

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@TRACKING_TYPE", piaLogMain.TRACKING_TYPE);
                cmd.Parameters.AddWithValue("@ACCESS_ACCOUNT", piaLogMain.ACCESS_ACCOUNT);
                cmd.Parameters.AddWithValue("@ACCOUNT_NAME", piaLogMain.ACCOUNT_NAME);
                cmd.Parameters.AddWithValue("@FROM_IP", ip);
                cmd.Parameters.AddWithValue("@PROGFUN_NAME", piaLogMain.PROGFUN_NAME);
                cmd.Parameters.AddWithValue("@ACCESSOBJ_NAME", piaLogMain.ACCESSOBJ_NAME);
                cmd.Parameters.AddWithValue("@EXECUTION_TYPE", piaLogMain.EXECUTION_TYPE);
                cmd.Parameters.AddWithValue("@EXECUTION_CONTENT", piaLogMain.EXECUTION_CONTENT);
                cmd.Parameters.AddWithValue("@AFFECT_ROWS", piaLogMain.AFFECT_ROWS);
                cmd.Parameters.AddWithValue("@PIA_OWNER1", StringUtil.toString(piaLogMain.PIA_OWNER1));
                cmd.Parameters.AddWithValue("@PIA_OWNER2", StringUtil.toString(piaLogMain.PIA_OWNER2));
                cmd.Parameters.AddWithValue("@PIA_TYPE", piaLogMain.PIA_TYPE);

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }

          
            }
        }
    
}