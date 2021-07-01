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
    public class FRTMailNotifyDao
    {
        /// <summary>
        /// 依 GROUP_CODE 查詢需MAIL寄送的對象
        /// </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        public List<MailNotifyModel> qryNtyUsr(string groupCode)
        {

            dbFGLEntities db = new dbFGLEntities();

            List<MailNotifyModel> rows = (from main in db.FRT_MAIL_NOTIFY
                        where 1 == 1
                            & main.GROUP_CODE == groupCode
                            & main.EMP_TYPE == "U"

                        select new MailNotifyModel
                        {
                            receiverEmpno = main.RECEIVER_EMPNO,
                            empType = main.EMP_TYPE,
                            isNotifyMgr = main.IS_NOTIFY_MGR,
                            isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                          
                        }).ToList()
                        .Union(from main in db.FRT_MAIL_NOTIFY
                               join role in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on main.RECEIVER_EMPNO equals role.ROLE_ID
                               join rs in db.CODE_USER_ROLE on role.ROLE_ID equals rs.ROLE_ID
                               join usr in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on rs.USER_ID equals usr.USER_ID
                               where 1 == 1
                                   & main.GROUP_CODE == groupCode
                                   & main.EMP_TYPE == "R"

                               select new MailNotifyModel
                               {
                                   receiverEmpno = usr.USER_ID,
                                   empType = "U",
                                   isNotifyMgr = main.IS_NOTIFY_MGR,
                                   isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                               }).ToList();

            return rows;

        }


        /// <summary>
        /// 依 GROUP_CODE + RESERVE1 + RESERVE2 + RESERVE3 查詢需MAIL寄送的對象
        /// </summary>
        /// <param name="groupCode"></param>
        /// <param name="reserve1"></param>
        /// <param name="reserve2"></param>
        /// <param name="reserve3"></param>
        /// <returns></returns>
        public List<MailNotifyModel> qryNtyUsrByReserve(string[] groupCode, string sysType,
            string reserve1, string reserve2, string reserve3)
        {
            List<MailNotifyModel> data = new List<MailNotifyModel>();

            bool bReserve1 = StringUtil.isEmpty(reserve1);
            bool bReserve2 = StringUtil.isEmpty(reserve2);
            bool bReserve3 = StringUtil.isEmpty(reserve3);

            string code = "";
            SysCodeDao sysCodeDao = new SysCodeDao();
            List<SYS_CODE> codeRows = sysCodeDao.qryByType("RT", "MAIL_GROUP");

            foreach (SYS_CODE d in codeRows) {
                string[] reserve1Arr = StringUtil.toString(d.RESERVE1).Split('|');
                string[] reserve2Arr = StringUtil.toString(d.RESERVE2).Split('|');
                string[] reserve3Arr = StringUtil.toString(d.RESERVE3).Split('|');

                if (groupCode.Contains(d.CODE_VALUE)) {
                    
                    if (!bReserve1 & !reserve1Arr.Contains(reserve1))
                        continue;

                    if (!bReserve2 & !reserve1Arr.Contains(reserve2))
                        continue;

                    if (!bReserve3 & !reserve1Arr.Contains(reserve3))
                        continue;

                    code = d.CODE_VALUE;
                }
            }

            //若沒有查到對應SYS_CODE設定的MAIL群組
            if ("".Equals(code))
                return data;


            dbFGLEntities db = new dbFGLEntities();
            

            List <MailNotifyModel> rows = (from main in db.FRT_MAIL_NOTIFY
                                          join sysCode in db.SYS_CODE.Where(x => x.SYS_CD == "RT" && x.CODE_TYPE == "MAIL_GROUP") on main.GROUP_CODE equals sysCode.CODE_VALUE
                                          where 1 == 1
                                              & main.GROUP_CODE == code
                                              & main.EMP_TYPE == "U"
                                          select new MailNotifyModel
                                          {
                                              receiverEmpno = main.RECEIVER_EMPNO,
                                              empType = main.EMP_TYPE,
                                              isNotifyMgr = main.IS_NOTIFY_MGR,
                                              isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                                          }).ToList()
                        .Union(from main in db.FRT_MAIL_NOTIFY
                               join sysCode in db.SYS_CODE.Where(x => x.SYS_CD == "RT" && x.CODE_TYPE == "MAIL_GROUP") on main.GROUP_CODE equals sysCode.CODE_VALUE
                               join role in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on main.RECEIVER_EMPNO equals role.ROLE_ID
                               join rs in db.CODE_USER_ROLE on role.ROLE_ID equals rs.ROLE_ID
                               join usr in db.CODE_USER.Where(x => x.IS_DISABLED == "N") on rs.USER_ID equals usr.USER_ID
                               where 1 == 1
                                   & main.GROUP_CODE == code
                                   & main.EMP_TYPE == "R"

                               select new MailNotifyModel
                               {
                                   receiverEmpno = usr.USER_ID,
                                   empType = "U",
                                   isNotifyMgr = main.IS_NOTIFY_MGR,
                                   isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR
                               }).ToList();


            return data;

        }


        public List<ORTB015Model> qryForORTB015(string groupCode, string receiverEmpno)
        {
            bool breceiverEmpno = StringUtil.isEmpty(receiverEmpno);
            dbFGLEntities db = new dbFGLEntities();

            List<ORTB015Model> rows = (from main in db.FRT_MAIL_NOTIFY
                                          where 1 == 1
                                              & main.GROUP_CODE == groupCode
                                              & main.EMP_TYPE == "U"
                                              & (breceiverEmpno || (main.RECEIVER_EMPNO == receiverEmpno))
                                       select new ORTB015Model
                                          {
                                              tempId =main.RECEIVER_EMPNO,
                                              receiverEmpno = main.RECEIVER_EMPNO,
                                              empType = main.EMP_TYPE,
                                              isNotifyMgr = main.IS_NOTIFY_MGR,
                                              isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR,
                                              dataStatus = main.DATA_STATUS,
                                              updId = main.UPDATE_ID,
                                              updDatetime = main.UPDATE_DATETIME == null ? "" : SqlFunctions.DateName("year", main.UPDATE_DATETIME) + "/" +
                                                                         SqlFunctions.DatePart("m", main.UPDATE_DATETIME) + "/" +
                                                                         SqlFunctions.DateName("day", main.UPDATE_DATETIME).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", main.UPDATE_DATETIME).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", main.UPDATE_DATETIME).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", main.UPDATE_DATETIME).Trim(),
                                              apprId = main.APPR_ID,
                                              apprDt = main.APPR_DT == null ? "" : SqlFunctions.DateName("year", main.APPR_DT) + "/" +
                                                                         SqlFunctions.DatePart("m", main.APPR_DT) + "/" +
                                                                         SqlFunctions.DateName("day", main.APPR_DT).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", main.APPR_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", main.APPR_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", main.APPR_DT).Trim()
                                          }).ToList()
                        .Union(from main in db.FRT_MAIL_NOTIFY
                               join role in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on main.RECEIVER_EMPNO equals role.ROLE_ID
                               where 1 == 1
                                   & main.GROUP_CODE == groupCode
                                   & main.EMP_TYPE == "R"
                                   & (breceiverEmpno || (main.RECEIVER_EMPNO == receiverEmpno))
                               select new ORTB015Model
                               {
                                   tempId = main.RECEIVER_EMPNO,
                                   receiverEmpno = main.RECEIVER_EMPNO,
                                   receiverEmpDesc = role.ROLE_NAME,
                                   empType = "R",
                                   isNotifyMgr = main.IS_NOTIFY_MGR,
                                   isNotifyDeptMgr = main.IS_NOTIFY_DEPT_MGR,
                                   dataStatus = main.DATA_STATUS,
                                   updId = main.UPDATE_ID,
                                   updDatetime = main.UPDATE_DATETIME == null ? "" : SqlFunctions.DateName("year", main.UPDATE_DATETIME) + "/" +
                                                                         SqlFunctions.DatePart("m", main.UPDATE_DATETIME) + "/" +
                                                                         SqlFunctions.DateName("day", main.UPDATE_DATETIME).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", main.UPDATE_DATETIME).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", main.UPDATE_DATETIME).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", main.UPDATE_DATETIME).Trim(),
                                   apprId = main.APPR_ID,
                                   apprDt = main.APPR_DT == null ? "" : SqlFunctions.DateName("year", main.APPR_DT) + "/" +
                                                                         SqlFunctions.DatePart("m", main.APPR_DT) + "/" +
                                                                         SqlFunctions.DateName("day", main.APPR_DT).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", main.APPR_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", main.APPR_DT).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", main.APPR_DT).Trim()
                                   
                               }).ToList();

            return rows;

        }


        /// <summary>
        /// 凍結/不凍結 資料
        /// </summary>
        /// <param name="dataStatus"></param>
        /// <param name="data"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStatus(string dataStatus, List<FRT_MAIL_NOTIFY> data, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = "";

            sql = @"update FRT_MAIL_NOTIFY
        set DATA_STATUS = @DATA_STATUS
        where 1=1
        and GROUP_CODE = @GROUP_CODE
        and RECEIVER_EMPNO = @RECEIVER_EMPNO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = sql;
            int cnt = 0;

            try
            {
                foreach (FRT_MAIL_NOTIFY d in data) {
                    command.Parameters.Clear();

                    command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.GROUP_CODE));
                    command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.RECEIVER_EMPNO));

                    command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(dataStatus));

                    //command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.UPDATE_ID));
                    //command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.UPDATE_DATETIME ?? System.DBNull.Value;

                    //command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.APPR_ID));
                    //command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)d.APPR_DT ?? System.DBNull.Value;

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
        /// 核可資料異動
        /// </summary>
        /// <param name="apprId"></param>
        /// <param name="procData"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void appr(string apprId, List<ORTB015Model> procData, SqlConnection conn, SqlTransaction transaction)
        {
            foreach (ORTB015Model d in procData) {
                d.apprId = apprId;

                FRT_MAIL_NOTIFY notify = new FRT_MAIL_NOTIFY();
                notify.GROUP_CODE = d.groupCode;
                notify.RECEIVER_EMPNO = d.receiverEmpno;
                notify.EMP_TYPE = d.empType;
                notify.IS_NOTIFY_MGR = d.isNotifyMgr;
                notify.IS_NOTIFY_DEPT_MGR = d.isNotifyDeptMgr;
                notify.MEMO = d.memo;
                notify.RESERVE1 = d.reserve1;
                notify.RESERVE2 = d.reserve2;
                notify.RESERVE3 = d.reserve3;
                notify.DATA_STATUS = "1";
                notify.UPDATE_ID = d.updId;
                notify.UPDATE_DATETIME = DateUtil.stringToDatetime(d.updDatetime);
                notify.APPR_ID = apprId;
                notify.APPR_DT = DateTime.Now;

                switch (d.status) {
                    case "A":
                        insert(notify, conn, transaction);
                        break;
                    case "U":
                        update(notify, conn, transaction);
                        break;
                    case "D":
                        delete(notify, conn, transaction);
                        break;
                }
            }
        }

        public void insert(FRT_MAIL_NOTIFY d, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
INSERT INTO [FRT_MAIL_NOTIFY]
           ([GROUP_CODE]
           ,[RECEIVER_EMPNO]
           ,[EMP_TYPE]
           ,[IS_NOTIFY_MGR]
           ,[IS_NOTIFY_DEPT_MGR]
           ,[MEMO]
           ,[RESERVE1]
           ,[RESERVE2]
           ,[RESERVE3]
           ,[DATA_STATUS]
           ,[UPDATE_ID]
           ,[UPDATE_DATETIME]
           ,[APPR_ID]
           ,[APPR_DT])
     VALUES
           (@GROUP_CODE
           ,@RECEIVER_EMPNO
           ,@EMP_TYPE
           ,@IS_NOTIFY_MGR
           ,@IS_NOTIFY_DEPT_MGR
           ,@MEMO
           ,@RESERVE1
           ,@RESERVE2
           ,@RESERVE3
           ,@DATA_STATUS
           ,@UPDATE_ID
           ,@UPDATE_DATETIME
           ,@APPR_ID
           ,@APPR_DT)
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = sql;

            try
            {

                command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.GROUP_CODE));
                command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.RECEIVER_EMPNO));
                command.Parameters.AddWithValue("@EMP_TYPE", StringUtil.toString(d.EMP_TYPE));
                command.Parameters.AddWithValue("@IS_NOTIFY_MGR", StringUtil.toString(d.IS_NOTIFY_MGR));
                command.Parameters.AddWithValue("@IS_NOTIFY_DEPT_MGR", StringUtil.toString(d.IS_NOTIFY_DEPT_MGR));
                command.Parameters.AddWithValue("@MEMO", StringUtil.toString(d.MEMO));
                command.Parameters.AddWithValue("@RESERVE1", StringUtil.toString(d.RESERVE1));
                command.Parameters.AddWithValue("@RESERVE2", StringUtil.toString(d.RESERVE2));
                command.Parameters.AddWithValue("@RESERVE3", StringUtil.toString(d.RESERVE3));

                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(d.DATA_STATUS));

                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.UPDATE_ID));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.UPDATE_DATETIME ?? System.DBNull.Value;
                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.APPR_ID));
                command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)d.APPR_DT ?? System.DBNull.Value;


                int cnt = command.ExecuteNonQuery();
               
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        public void update(FRT_MAIL_NOTIFY d, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
UPDATE [FRT_MAIL_NOTIFY]
  SET IS_NOTIFY_MGR = @IS_NOTIFY_MGR
     ,IS_NOTIFY_DEPT_MGR = @IS_NOTIFY_DEPT_MGR
     ,DATA_STATUS = @DATA_STATUS
     ,UPDATE_ID = @UPDATE_ID
     ,UPDATE_DATETIME = @UPDATE_DATETIME
     ,APPR_ID = @APPR_ID
     ,APPR_DT = @APPR_DT
WHERE GROUP_CODE = @GROUP_CODE
  AND RECEIVER_EMPNO = @RECEIVER_EMPNO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = sql;

            try
            {

                command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.GROUP_CODE));
                command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.RECEIVER_EMPNO));
                command.Parameters.AddWithValue("@IS_NOTIFY_MGR", StringUtil.toString(d.IS_NOTIFY_MGR));
                command.Parameters.AddWithValue("@IS_NOTIFY_DEPT_MGR", StringUtil.toString(d.IS_NOTIFY_DEPT_MGR));


                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(d.DATA_STATUS));

                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(d.UPDATE_ID));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)d.UPDATE_DATETIME ?? System.DBNull.Value;
                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(d.APPR_ID));
                command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)d.APPR_DT ?? System.DBNull.Value;


                int cnt = command.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void delete(FRT_MAIL_NOTIFY d, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
DELETE [FRT_MAIL_NOTIFY]
WHERE GROUP_CODE = @GROUP_CODE
  AND RECEIVER_EMPNO = @RECEIVER_EMPNO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            command.CommandText = sql;

            try
            {

                command.Parameters.AddWithValue("@GROUP_CODE", StringUtil.toString(d.GROUP_CODE));
                command.Parameters.AddWithValue("@RECEIVER_EMPNO", StringUtil.toString(d.RECEIVER_EMPNO));
            

                int cnt = command.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}