
using SSO.Web.BO;
using SSO.Web.Models;
using System;
using System.Data.SqlClient;

namespace SSO.Web.Daos
{
    public class LogDao
    {
        public static void Insert(Log log, String userId)
        {
            //DateUtil dateUtil = new DateUtil();
            CommonUtil commonUtil = new CommonUtil();
            string[] curDateTime = DateUtil.getCurDateTime("yyyyMMdd HHmmss").Split(' ');
            log.CDATE = curDateTime[0];
            log.CTIME = curDateTime[1];
            //log.CCONTENT = content;
            log.CUSERID = userId;

            string strConn = DbUtil.GetDBFglConnStr();

            using (SqlConnection conn = new SqlConnection(strConn))
            {

                string sql = @"INSERT INTO [Log]
           ([CDATE]
           ,[CTIME]
           ,[CFUNCTION]
           ,[CUSERID]
           ,[CACTION]
           ,[CCONTENT])
     VALUES
           (@CDATE
           ,@CTIME
           ,@CFUNCTION
           ,@CUSERID
           ,@CACTION
           ,@CCONTENT)
";


                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CDATE", log.CDATE);
                cmd.Parameters.AddWithValue("@CTIME", log.CTIME);
                cmd.Parameters.AddWithValue("@CFUNCTION", log.CFUNCTION);
                cmd.Parameters.AddWithValue("@CUSERID", log.CUSERID);
                cmd.Parameters.AddWithValue("@CACTION", log.CACTION);
                cmd.Parameters.AddWithValue("@CCONTENT", log.CCONTENT);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }
    }
}