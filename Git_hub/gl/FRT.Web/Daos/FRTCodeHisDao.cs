using FRT.Web.AS400Models;
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
    public class FRTCodeHisDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 查詢在覆核中，且為"新增"的資料，FOR ORTB013查詢時就要看見
        /// </summary>
        /// <param name="srceFrom"></param>
        /// <param name="groupId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<FRT_CODE_HIS> qryByStatus(string srceFrom, string groupId, string status)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                List<FRT_CODE_HIS> his = db.FRT_CODE_HIS
                    .Where(x => x.SRCE_FROM == srceFrom & x.GROUP_ID == groupId & x.STATUS == status & x.APPR_STAT == "1")
                    .ToList<FRT_CODE_HIS>();

                return his;
            }
        }

        public FRT_CODE_HIS chkOnAply(string srceFrom, string groupId, string refNo)
        {


            using (dbFGLEntities db = new dbFGLEntities())
            {
                FRT_CODE_HIS his = db.FRT_CODE_HIS
                    .Where(x => x.SRCE_FROM == srceFrom & x.GROUP_ID == groupId & x.REF_NO == refNo & x.APPR_STAT == "1")
                    .FirstOrDefault<FRT_CODE_HIS>();

                return his;
            }
        }


        public List<FrtCodeHisModel> qryForSTAT(string srceFrom, string groupId, string refNo, string refNoN, string apprStat) {

            bool brefNo = StringUtil.isEmpty(refNo) & StringUtil.isEmpty(refNoN);

            dbFGLEntities db = new dbFGLEntities();

            var rows = (from main in db.FRT_CODE_HIS
                        where 1 == 1
                            & main.SRCE_FROM == srceFrom
                            & main.GROUP_ID == groupId
                            & (brefNo || (main.REF_NO == refNo & main.REF_NO_N == refNoN))
                            & main.APPR_STAT == apprStat
                        select new FrtCodeHisModel
                        {

                            aplyNo = main.APPLY_NO.ToString(),
                            status = main.STATUS,
                            srceFrom = main.SRCE_FROM,
                            groupId = main.GROUP_ID.ToString(),
                            textLen = main.TEXT_LEN.ToString(),
                            refNo = main.REF_NO.ToString(),
                            refNoN = main.REF_NO_N.ToString(),
                            useMark = main.USE_MARK,
                            text = main.TEXT.ToString(),
                            updId = main.UPDATE_ID,
                            updDateTime = SqlFunctions.DateName("year", main.UPDATE_DATETIME) + "/" +
                                            SqlFunctions.DatePart("m", main.UPDATE_DATETIME) + "/" +
                                            SqlFunctions.DateName("day", main.UPDATE_DATETIME).Trim() + " " +
                                            SqlFunctions.DateName("hh", main.UPDATE_DATETIME).Trim() + ":" +
                                            SqlFunctions.DateName("n", main.UPDATE_DATETIME).Trim() + ":" +
                                            SqlFunctions.DateName("s", main.UPDATE_DATETIME).Trim()
                        }).ToList<FrtCodeHisModel>();

            return rows;
        }


        public bool updateFRTCodeHis(string apprId, string apprStat, FrtCodeHisModel procData, SqlConnection conn, SqlTransaction transaction) {
            bool execResult = true;
            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            try
            {
                string sql = @"
UPDATE FRT_CODE_HIS
  SET APPR_STAT = @APPR_STAT
     ,APPR_ID = @APPR_ID 
     ,APPROVE_DATETIME = @APPROVE_DATETIME 
 WHERE  1 = 1
  AND APPLY_NO = @APPLY_NO 
  AND GROUP_ID = @GROUP_ID 
  AND REF_NO = @REF_NO
  AND REF_NO_N = @REF_NO_N ";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@APPR_STAT", apprStat);
                cmd.Parameters.AddWithValue("@APPR_ID", apprId);
                cmd.Parameters.AddWithValue("@APPROVE_DATETIME", DateTime.Now);
                cmd.Parameters.AddWithValue("@APPLY_NO", procData.aplyNo);
                cmd.Parameters.AddWithValue("@GROUP_ID", procData.groupId);
                cmd.Parameters.AddWithValue("@REF_NO", StringUtil.toString(procData.refNo));
                cmd.Parameters.AddWithValue("@REF_NO_N", StringUtil.toString(procData.refNoN));

                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                execResult = false;
                throw e;
            }


            return execResult;
        }


        public void insert(string applyNo, List<FRT_CODE_HIS> data)
        {
            DateTime dtNow = DateTime.Now;

            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string sql = @"INSERT INTO [FRT_CODE_HIS]
           ([APPLY_NO]
           ,[GROUP_ID]
           ,[TEXT_LEN]
           ,[REF_NO]
           ,[TEXT]
           ,[SRCE_FROM]
           ,[USE_MARK]
           ,[UPDATE_ID]
           ,[UPDATE_DATETIME]
           ,[REF_NO_N]
           ,[STATUS]
           ,[APPR_STAT])
     VALUES
            (@APPLY_NO
           ,@GROUP_ID
           ,@TEXT_LEN
           ,@REF_NO
           ,@TEXT
           ,@SRCE_FROM
           ,@USE_MARK
           ,@UPDATE_ID
           ,@UPDATE_DATETIME
           ,@REF_NO_N
           ,@STATUS
           ,@APPR_STAT)";

                    SqlCommand cmd = conn.CreateCommand();

                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.CommandText = sql;

                    foreach (FRT_CODE_HIS d in data)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@APPLY_NO", applyNo);
                        cmd.Parameters.AddWithValue("@GROUP_ID", d.GROUP_ID);
                        cmd.Parameters.AddWithValue("@TEXT_LEN", d.TEXT_LEN);
                        cmd.Parameters.AddWithValue("@REF_NO", d.REF_NO);
                        cmd.Parameters.AddWithValue("@TEXT", d.TEXT == null ? "" : d.TEXT);
                        //cmd.Parameters.AddWithValue("@TEXT", d.TEXT.Replace("\r\n", "").Replace("\r", "").Replace("\n", ""));
                        cmd.Parameters.AddWithValue("@SRCE_FROM", d.SRCE_FROM);
                        cmd.Parameters.AddWithValue("@USE_MARK", d.USE_MARK == null ? "" : d.USE_MARK);
                        cmd.Parameters.AddWithValue("@UPDATE_ID", d.UPDATE_ID);
                        cmd.Parameters.AddWithValue("@UPDATE_DATETIME", dtNow);
                        cmd.Parameters.AddWithValue("@REF_NO_N", d.REF_NO_N == null ? "" : d.REF_NO_N);
                        cmd.Parameters.AddWithValue("@STATUS", d.STATUS);
                        cmd.Parameters.AddWithValue("@APPR_STAT", "1");

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }

            }
        }
    }

    
}