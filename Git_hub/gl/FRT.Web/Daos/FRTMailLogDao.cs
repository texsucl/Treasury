using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTMailLogDao
    {


        /// <summary>
        /// ORTB008 MAIL寄送紀錄查詢
        /// </summary>
        /// <param name="qDateB"></param>
        /// <param name="qDateE"></param>
        /// <param name="receiverId"></param>
        /// <param name="mailResult"></param>
        /// <returns></returns>
        public List<ORTB008Model> qryForORTB008(string qDateB, string qDateE, string receiverEmpno, string mailResult)
        {
            DateTime sB = Convert.ToDateTime(qDateB);
            DateTime sE = Convert.ToDateTime(qDateE);
            sE = sE.AddDays(1);

            bool breceiverEmpno = StringUtil.isEmpty(receiverEmpno);
            bool bmailResult = StringUtil.isEmpty(mailResult);

            dbFGLEntities db = new dbFGLEntities();

            var rows = (from main in db.FRT_MAIL_LOG
                        where 1 == 1
                            & main.MAIL_DATE >= sB & main.MAIL_DATE < sE
                            & (breceiverEmpno || main.RECEIVER_EMPNO == receiverEmpno)
                            & (bmailResult || main.MAIL_RESULT == mailResult)
                        select new ORTB008Model
                        {
                            seq = main.SEQ.ToString(),
                            mailDate = SqlFunctions.DateName("year", main.MAIL_DATE) + "/" +
                                                            SqlFunctions.DatePart("m", main.MAIL_DATE) + "/" +
                                                            SqlFunctions.DateName("day", main.MAIL_DATE).Trim(),
                            mailTime = SqlFunctions.DateName("hh", main.MAIL_TIME).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", main.MAIL_TIME).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", main.MAIL_TIME).Trim(),
                            receiverEmpno = main.RECEIVER_EMPNO,
                            eMail = main.EMAIL,
                            mailResult = main.MAIL_RESULT,
                            resultDesc = main.RESULT_DESC.ToString(),
                            mailSub = main.MAIL_SUB
                        }).ToList<ORTB008Model>();

            return rows;

        }



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