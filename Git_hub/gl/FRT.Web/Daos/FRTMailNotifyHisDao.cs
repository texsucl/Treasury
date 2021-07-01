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
    public class FRTMailNotifyHisDao
    {

        public List<ORTB015Model> qryForSTAT(string groupCode, string receiverEmpno, string apprStatus)
        {
            bool bgroupCode = StringUtil.isEmpty(groupCode);
            bool breceiverEmpno = StringUtil.isEmpty(receiverEmpno);

            dbFGLEntities db = new dbFGLEntities();

            var rows = (from main in db.FRT_MAIL_NOTIFY_HIS
                        where 1 == 1
                            & (bgroupCode || (main.GROUP_CODE == groupCode))
                            & (breceiverEmpno || (main.RECEIVER_EMPNO == receiverEmpno))
                            & main.APPR_STATUS == apprStatus
                        select new ORTB015Model
                        {
                            tempId = main.GROUP_CODE.ToString() + "|" + main.RECEIVER_EMPNO.ToString(),
                            aplyNo = main.APLY_NO.ToString(),
                            status = main.EXEC_ACTION,
                            groupCode = main.GROUP_CODE.ToString(),
                            receiverEmpno = main.RECEIVER_EMPNO.ToString(),
                            empType = main.EMP_TYPE.ToString(),
                            isNotifyMgr = main.IS_NOTIFY_MGR.ToString(),
                            isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR,
                            updId = main.UPDATE_ID,
                            updDatetime = SqlFunctions.DateName("year", main.UPDATE_DATETIME) + "/" +
                                            SqlFunctions.DatePart("m", main.UPDATE_DATETIME) + "/" +
                                            SqlFunctions.DateName("day", main.UPDATE_DATETIME).Trim() + " " +
                                            SqlFunctions.DateName("hh", main.UPDATE_DATETIME).Trim() + ":" +
                                            SqlFunctions.DateName("n", main.UPDATE_DATETIME).Trim() + ":" +
                                            SqlFunctions.DateName("s", main.UPDATE_DATETIME).Trim()
                        }).ToList<ORTB015Model>();

            return rows;
        }





        public int insert(string aplyNo, List<FRT_MAIL_NOTIFY_HIS> data, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
INSERT INTO [FRT_MAIL_NOTIFY_HIS]
           ([APLY_NO]
           ,[EXEC_ACTION]
           ,[GROUP_CODE]
           ,[RECEIVER_EMPNO]
           ,[EMP_TYPE]
           ,[IS_NOTIFY_MGR]
           ,[IS_NOTIFY_DEPT_MGR]
           ,[MEMO]
           ,[RESERVE1]
           ,[RESERVE2]
           ,[RESERVE3]
           ,[UPDATE_ID]
           ,[UPDATE_DATETIME]
           ,[APPR_STATUS])
     VALUES
           (@APLY_NO
           ,@EXEC_ACTION
           ,@GROUP_CODE
           ,@RECEIVER_EMPNO
           ,@EMP_TYPE
           ,@IS_NOTIFY_MGR
           ,@IS_NOTIFY_DEPT_MGR
           ,@MEMO
           ,@RESERVE1
           ,@RESERVE2
           ,@RESERVE3
           ,@UPDATE_ID
           ,@UPDATE_DATETIME
           ,@APPR_STATUS)
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = sql;
            int cnt = 0;

            try
            {
                foreach (FRT_MAIL_NOTIFY_HIS d in data)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@APLY_NO", aplyNo);
                    command.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(d.EXEC_ACTION));
                    command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.GROUP_CODE));
                    command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.RECEIVER_EMPNO));
                    command.Parameters.AddWithValue("@EMP_TYPE", StringUtil.toString(d.EMP_TYPE));
                    command.Parameters.AddWithValue("@IS_NOTIFY_MGR", StringUtil.toString(d.IS_NOTIFY_MGR));
                    command.Parameters.AddWithValue("@IS_NOTIFY_DEPT_MGR", StringUtil.toString(d.IS_NOTIFY_DEPT_MGR));
                    command.Parameters.AddWithValue("@MEMO", StringUtil.toString(d.MEMO));
                    command.Parameters.AddWithValue("@RESERVE1", StringUtil.toString(d.RESERVE1));
                    command.Parameters.AddWithValue("@RESERVE2", StringUtil.toString(d.RESERVE2));
                    command.Parameters.AddWithValue("@RESERVE3", StringUtil.toString(d.RESERVE3));

                    command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.UPDATE_ID));
                    command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.UPDATE_DATETIME ?? System.DBNull.Value;
                    command.Parameters.AddWithValue("@APPR_STATUS", StringUtil.toString(d.APPR_STATUS));
 
                    cnt += command.ExecuteNonQuery();
                }


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }



        /// <summary>
        /// 更新覆核結果
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="apprStatus"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStat(string apprId, string apprStatus, List<ORTB015Model> procData, SqlConnection conn, SqlTransaction transaction)
        {
            string strSQL = "";
            strSQL += "UPDATE FRT_MAIL_NOTIFY_HIS " +
                      "  SET APPR_STATUS = @APPR_STATUS " +
                          " ,APPR_ID = @APPR_ID  " +
                    " ,APPR_DT = @APPR_DT  " +
                    " WHERE  1 = 1 " +
                    " AND APLY_NO = @APLY_NO " +
                    " AND GROUP_CODE = @GROUP_CODE " +
                    " AND RECEIVER_EMPNO = @RECEIVER_EMPNO ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = strSQL;
            int cnt = 0;

            try
            {
                foreach (ORTB015Model d in procData)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@APPR_STATUS", apprStatus);
                    command.Parameters.AddWithValue("@APPR_ID", apprId);
                    command.Parameters.AddWithValue("@APPR_DT", DateTime.Now);
                    command.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(d.aplyNo));
                    command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.groupCode));
                    command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.receiverEmpno));
                  

                    cnt += command.ExecuteNonQuery();
                }


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }


    }
}