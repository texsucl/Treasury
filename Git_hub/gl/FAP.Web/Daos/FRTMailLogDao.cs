using FAP.Web.BO;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FAP.Web.Daos
{
    public class FRTMailLogDao
    {

        public int Insert(FRT_MAIL_LOG frtMailLog, string[] strDT, SqlConnection conn)
        {
            //string[] strDT = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Split(' ');

            string sql = @"INSERT INTO [FRT_MAIL_LOG]
           ([SEQ]
           ,[MAIL_DATE]
           ,[MAIL_TIME]
           ,[RECEIVER_EMPNO]
           ,[EMAIL]
           ,[MAIL_RESULT]
           ,[RESULT_DESC]
           ,[MAIL_SUB])
     VALUES
           (@SEQ
           ,@MAIL_DATE
           ,@MAIL_TIME
           ,@RECEIVER_EMPNO
           ,@EMAIL
           ,@MAIL_RESULT
           ,@RESULT_DESC
           ,@MAIL_SUB)";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            //cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@SEQ", frtMailLog.SEQ);
                cmd.Parameters.AddWithValue("@MAIL_DATE", strDT[0]);
                cmd.Parameters.AddWithValue("@MAIL_TIME", strDT[1]);
                cmd.Parameters.AddWithValue("@RECEIVER_EMPNO", frtMailLog.RECEIVER_EMPNO);
                cmd.Parameters.AddWithValue("@EMAIL", frtMailLog.EMAIL);
                cmd.Parameters.AddWithValue("@MAIL_RESULT", frtMailLog.MAIL_RESULT);
                cmd.Parameters.AddWithValue("@RESULT_DESC", frtMailLog.RESULT_DESC);
                cmd.Parameters.AddWithValue("@MAIL_SUB", frtMailLog.MAIL_SUB);

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