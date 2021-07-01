using FRT.Web.BO;
using FRT.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FRT.Web.Daos
{
    public class FRTXmlT622685Dao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 將本次的呼叫紀錄至 FRT_XML_T_622685
        /// </summary>
        /// <param name="bSucess"></param>
        /// <param name="reqBodyModel"></param>
        public string writeT622685(string usrId, bool bSucess, FRT_XML_T_622685 T622685)
        {
            string errorMsg = string.Empty;
            try
            {
                string strConn = DbUtil.GetDBFglConnStr();

                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();

                    string sql = @"

INSERT INTO [FRT_XML_T_622685]
 (
HEADER
,ACT_DATE
,BRH_TYPE_01
,ITEM_NO
,SQL_STS
,CRT_TIME
,EXEC_TYPE
,CRT_UID
)
     VALUES
(
 @HEADER
,@ACT_DATE
,@BRH_TYPE_01
,@ITEM_NO
,@SQL_STS
,@CRT_TIME
,@EXEC_TYPE
,@CRT_UID
)
        ";
                    SqlCommand command = conn.CreateCommand();
                    command.Connection = conn;

                    command.CommandText = sql;

                    command.Parameters.AddWithValue("@HEADER", StringUtil.toString(T622685.HEADER));
                    command.Parameters.AddWithValue("@ACT_DATE", StringUtil.toString(T622685.ACT_DATE));
                    command.Parameters.AddWithValue("@BRH_TYPE_01", StringUtil.toString(T622685.BRH_TYPE_01));
                    command.Parameters.AddWithValue("@ITEM_NO", StringUtil.toString(T622685.ITEM_NO));
                    command.Parameters.AddWithValue("@SQL_STS", bSucess == true ? "0" : "");
                    command.Parameters.AddWithValue("@CRT_TIME", DateTime.Now);
                    command.Parameters.AddWithValue("@EXEC_TYPE", "W");
                    command.Parameters.AddWithValue("@CRT_UID", usrId);

                    command.ExecuteNonQuery();


                }

            }
            catch (Exception e)
            {
                errorMsg = e.ToString();
                logger.Info(errorMsg);
            }
            return errorMsg;
        }

        public string writeT622685New(FRT_XML_T_622685_NEW T622685)
        {
            string errorMsg = string.Empty;
            using (dbFGLEntities db = new dbFGLEntities())
            {
                db.FRT_XML_T_622685_NEW.Add(T622685);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    errorMsg = ex.ToString();
                }
            }
            return errorMsg;
        }
    }
}