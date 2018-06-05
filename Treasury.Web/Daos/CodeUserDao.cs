
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Treasury.WebUtils;
using Treasury.WebDaos;
using Treasury.WebBO;
using Treasury.WebViewModels;
using System.Data.Entity.SqlServer;
using System.Transactions;
using System.Data.SqlClient;

/// <summary>
/// 功能說明：
/// 初版作者：20171023 黃黛鈺
/// 修改歷程：20171023 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// ==============================================
/// 修改日期/修改人：20180221 黃黛鈺 
/// 需求單號：201801230413-00 
/// 修改內容：加入覆核功能
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Daos
{
    public class CodeUserDao 
    {

        /**
        查詢出所有使用者資料(for畫面下拉選單使用)
        **/
        public SelectList loadSelectList()
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            List<UserMgrModel> result1 = (from user in context.CODE_USER
                           select new UserMgrModel
                           {
                               cUserID = user.USER_ID.Trim()
                           }
                          ).ToList();


            OaEmpDao oaEmpDao = new OaEmpDao();
            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                foreach (UserMgrModel user in result1)
                {
                    try
                    {
                        user.cUserName = user.cUserID + " " + StringUtil.toString(oaEmpDao.qryByUsrId(user.cUserID, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
               


            var items = new SelectList
                (
                items: result1,
                dataValueField: "cUserID",
                dataTextField: "cUserName",
                selectedValue: (object)null
                );
            

            return items;
        }


        /// <summary>
        /// 使用者角色權限查詢
        /// </summary>
        /// <returns></returns>
        public List<UserMgrModel> qryUserRole()
        {

            List<UserMgrModel> rows = new List<UserMgrModel>();

            using (dbTreasuryEntities db = new dbTreasuryEntities())
            {

                rows = (from u in db.CODE_USER

                        join ur in db.CODE_USER_ROLE on u.USER_ID equals ur.USER_ID

                        join r in db.CODE_ROLE on ur.ROLE_ID equals r.ROLE_ID
                        where 1 == 1
                            & u.IS_DISABLED == "N"

                        select new UserMgrModel()
                        {
                            cUserID = u.USER_ID.Trim(),
                            roleAuthType = r.ROLE_AUTH_TYPE.Trim(),
                            roleName = r.ROLE_NAME.Trim()
                        }).Distinct()
                        .ToList<UserMgrModel>();



                return rows;
            }

        }


        /// <summary>
        /// 新增使用者
        /// </summary>
        /// <param name="user"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Create(CODE_USER user, SqlConnection conn, SqlTransaction transaction)
        {

            
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                OaEmpDao oaEmpDao = new OaEmpDao();
                V_EMPLY2 emp = new V_EMPLY2();
                try
                {
                    emp = oaEmpDao.qryByUsrId(user.USER_ID, db);
                    if (emp != null)
                        user.USER_UNIT = StringUtil.toString(emp.DPT_CD);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            string sql = @"

INSERT INTO [dbo].[CODE_USER]
           ([USER_ID]
           ,[USER_UNIT]
           ,[IS_DISABLED]
           ,[IS_MAIL]
           ,[DATA_STATUS]
           ,[CREATE_UID]
           ,[CREATE_DT]
           ,[LAST_UPDATE_UID]
           ,[LAST_UPDATE_DT]
           ,[APPR_UID]
           ,[APPR_DT]
)
     VALUES
(
 @USER_ID
,@USER_UNIT
,@IS_DISABLED
,@IS_MAIL
,@DATA_STATUS
,@CREATE_UID
,@CREATE_DT
,@LAST_UPDATE_UID
,@LAST_UPDATE_DT
,@APPR_UID
,@APPR_DT
)
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@USER_ID", StringUtil.toString(user.USER_ID));
                command.Parameters.AddWithValue("@USER_UNIT", StringUtil.toString(user.USER_UNIT));
                command.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(user.IS_DISABLED));
                command.Parameters.AddWithValue("@IS_MAIL", StringUtil.toString(user.IS_MAIL));
                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(user.DATA_STATUS));
                command.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(user.CREATE_UID));

                command.Parameters.Add("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.CREATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(user.LAST_UPDATE_UID));

                command.Parameters.Add("@LAST_UPDATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.LAST_UPDATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_UID", StringUtil.toString(user.APPR_UID));

                command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.APPR_DT ?? System.DBNull.Value;


                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }



        /// <summary>
        /// 以鍵項查詢使用者資料
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CODE_USER qryUserByKey(String userId)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    CODE_USER codeUser = db.CODE_USER.Where(x => x.USER_ID == userId).FirstOrDefault<CODE_USER>();

                    return codeUser;
                }
            }
        }


        /// <summary>
        /// 使用者維護查詢
        /// </summary>
        /// <param name="userMgrModel"></param>
        /// <returns></returns>
        public List<UserMgrModel> qryUserMgr(UserMgrModel userMgrModel)
        {

            bool bcUserID = StringUtil.isEmpty(userMgrModel.cUserID);
            bool bcUserName = StringUtil.isEmpty(userMgrModel.cUserName);
            bool bisDisabled = StringUtil.isEmpty(userMgrModel.isDisabled);
            bool bisMail = StringUtil.isEmpty(userMgrModel.isMail);
            //bool bcBelongUnitCode = StringUtil.isEmpty(userMgrModel.cBelongUnitCode);
            //bool bcBelongUnitSeq = StringUtil.isEmpty(userMgrModel.cBelongUnitSeq);
            bool broleAuthType = StringUtil.isEmpty(userMgrModel.roleAuthType);
            bool bcodeRole = StringUtil.isEmpty(userMgrModel.codeRole);
            bool bcUpdUserID = StringUtil.isEmpty(userMgrModel.cUpdUserID);
            bool bcUpdDateB = StringUtil.isEmpty(userMgrModel.cUpdDateB);
            bool bcUpdDateE = StringUtil.isEmpty(userMgrModel.cUpdDateE);


            DateTime sB = DateTime.Now.AddDays(1);
            if (!bcUpdDateB)
            {
                sB = Convert.ToDateTime(userMgrModel.cUpdDateB);
            }
            DateTime sE = DateTime.Now.AddDays(1);
            if (!bcUpdDateE)
            {
                sE = Convert.ToDateTime(userMgrModel.cUpdDateE);
            }
            sE = sE.AddDays(1);

            List<UserMgrModel> rows = new List<UserMgrModel>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
            {

                rows = (from user in db.CODE_USER
                        join codeFlag in db.SYS_CODE.Where(x => x.CODE_TYPE == "IS_DISABLED") on user.IS_DISABLED equals codeFlag.CODE into psFlag
                        from xFlag in psFlag.DefaultIfEmpty()

                        join codeMail in db.SYS_CODE.Where(x => x.CODE_TYPE == "YN_FLAG") on user.IS_DISABLED equals codeMail.CODE into psMail
                        from xMail in psMail.DefaultIfEmpty()

                        join userRole in db.CODE_USER_ROLE on user.USER_ID equals userRole.USER_ID into psUserRole
                        from xUserRole in psUserRole.DefaultIfEmpty()

                        join role in db.CODE_ROLE on xUserRole.ROLE_ID equals role.ROLE_ID into psRole
                        from xRole in psRole.DefaultIfEmpty()

                        join codeStatus in db.SYS_CODE.Where(x => x.CODE_TYPE == "DATA_STATUS") on user.DATA_STATUS equals codeStatus.CODE into psStatus
                        from xStatus in psStatus.DefaultIfEmpty()

                        where 1 == 1
                            & (bcUserID || (user.USER_ID == userMgrModel.cUserID.Trim()))
                            & (bisDisabled || (user.IS_DISABLED == userMgrModel.isDisabled.Trim()))
                            & (bisMail || (user.IS_MAIL == userMgrModel.isMail.Trim()))
                            & (broleAuthType || (xRole.ROLE_AUTH_TYPE == userMgrModel.roleAuthType.Trim()))
                            & (bcodeRole || (xUserRole.ROLE_ID == userMgrModel.codeRole.Trim()))
                            & (bcUpdUserID || (user.LAST_UPDATE_UID == userMgrModel.cUpdUserID.Trim()))
                            & (bcUpdDateB || user.LAST_UPDATE_DT >= sB)
                            & (bcUpdDateE || user.LAST_UPDATE_DT <= sE)

                        select new UserMgrModel()
                        {
                            isDisabled = user.IS_DISABLED,
                            isDisabledDesc = xFlag.CODE_VALUE.Trim(),
                            isMail = user.IS_MAIL,
                            isMailDesc = xMail.CODE_VALUE.Trim(),
                            cUserID = user.USER_ID.Trim(),
                            cCrtUserID = user.CREATE_UID.Trim(),
                            cCrtDate = user.CREATE_DT == null ? "" : SqlFunctions.DateName("year", user.CREATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", user.CREATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", user.CREATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", user.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", user.CREATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", user.CREATE_DT).Trim(),
                            cUpdUserID = user.LAST_UPDATE_UID.Trim(),
                            cUpdDate = user.LAST_UPDATE_DT == null ? "" : SqlFunctions.DateName("year", user.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", user.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", user.LAST_UPDATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", user.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", user.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", user.LAST_UPDATE_DT).Trim(),
                            apprUid = user.APPR_UID == null ? "" : user.APPR_UID.Trim(),
                            apprDt = user.APPR_DT == null ? "" : SqlFunctions.DateName("year", user.APPR_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", user.APPR_DT) + "/" +
                                                                     SqlFunctions.DateName("day", user.APPR_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", user.APPR_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", user.APPR_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", user.APPR_DT).Trim(),
                            frezzeUid = user.FREEZE_UID == null ? "": user.FREEZE_UID.Trim(),
                            frezzeDt = user.FREEZE_DT == null ? "" : SqlFunctions.DateName("year", user.FREEZE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", user.FREEZE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", user.FREEZE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", user.FREEZE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", user.FREEZE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", user.FREEZE_DT).Trim(),
                            dataStatus = user.DATA_STATUS,
                            dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE)
                        }).Distinct().OrderBy(d => d.cUserID).ToList<UserMgrModel>();

            }
        }

            return rows;
            }


        public int Update(CODE_USER user, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update  [CODE_USER]
                  set USER_UNIT = @USER_UNIT 
                     ,IS_DISABLED = @IS_DISABLED
                     ,IS_MAIL = @IS_MAIL
                     ,DATA_STATUS = @DATA_STATUS
        ,CREATE_UID = @CREATE_UID
        ,CREATE_DT = @CREATE_DT
        ,LAST_UPDATE_UID = @LAST_UPDATE_UID
        ,LAST_UPDATE_DT = @LAST_UPDATE_DT
        ,APPR_UID = @APPR_UID
        ,APPR_DT = @APPR_DT
        ,FREEZE_UID = @FREEZE_UID
        ,FREEZE_DT = @FREEZE_DT
        ,LAST_LOGIN_DT = @LAST_LOGIN_DT
        ,LAST_LOGOUT_DT = @LAST_LOGOUT_DT
             where USER_ID = @USER_ID
        ";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@USER_ID", StringUtil.toString(user.USER_ID));
                cmd.Parameters.AddWithValue("@USER_UNIT", StringUtil.toString(user.USER_UNIT));
                cmd.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(user.IS_DISABLED));
                cmd.Parameters.AddWithValue("@IS_MAIL", StringUtil.toString(user.IS_MAIL));

                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(user.DATA_STATUS));
                cmd.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(user.CREATE_UID));
                cmd.Parameters.Add("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.CREATE_DT ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(user.LAST_UPDATE_UID));
                cmd.Parameters.Add("@LAST_UPDATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.LAST_UPDATE_DT ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_UID", StringUtil.toString(user.APPR_UID));
                cmd.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.APPR_DT ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@FREEZE_UID", StringUtil.toString(user.FREEZE_UID));
                cmd.Parameters.Add("@FREEZE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.FREEZE_DT ?? System.DBNull.Value;
                cmd.Parameters.Add("@LAST_LOGIN_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.LAST_LOGIN_DT ?? System.DBNull.Value;
                cmd.Parameters.Add("@LAST_LOGOUT_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)user.LAST_LOGOUT_DT ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }


        }
        


        ///// <summary>
        ///// 以userId為鍵項，查詢使用者資料
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public CODE_USER qryByKey(String userId) {
        //    using (dbTreasuryEntities db = new dbTreasuryEntities())
        //    {
        //        CODE_USER codeUser = db.CODE_USER.Where(x => x.USER_ID == userId).FirstOrDefault<CODE_USER>();

        //        return codeUser;
        //    }

        //}



        /// <summary>
        /// 異動user的login、logout時間
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        public void updateLogInOut(String userId, String type)
        {
            using (dbTreasuryEntities db = new dbTreasuryEntities())
            {
                CODE_USER codeUser = db.CODE_USER.Where(x => x.USER_ID == userId).FirstOrDefault<CODE_USER>();

         
                if ("I".Equals(type))
                    codeUser.LAST_LOGIN_DT = DateUtil.getCurDateTime();
                else
                    codeUser.LAST_LOGOUT_DT = DateUtil.getCurDateTime();

                int cnt = db.SaveChanges();

            }

        }

        public String userLogContent(CODE_USER codeUser)
        {
            String content = "";

            content += StringUtil.toString(codeUser.USER_ID) + "|";
            content += StringUtil.toString(codeUser.USER_UNIT) + "|";
            content += StringUtil.toString(codeUser.IS_DISABLED) + "|";
            content += StringUtil.toString(codeUser.IS_MAIL) + "|";
            content += StringUtil.toString(codeUser.DATA_STATUS) + "|";


            content += StringUtil.toString(codeUser.CREATE_UID) + "|";
            content += codeUser.CREATE_DT == null ? "|" : codeUser.CREATE_DT + "|";
            content += StringUtil.toString(codeUser.LAST_UPDATE_UID) + "|";
            content += codeUser.LAST_UPDATE_DT == null ? "|" : codeUser.LAST_UPDATE_DT + "|";
            content += StringUtil.toString(codeUser.APPR_UID) + "|";
            content += codeUser.APPR_DT == null ? "|" : codeUser.APPR_DT + "|";
            content += StringUtil.toString(codeUser.FREEZE_UID) + "|";
            content += codeUser.FREEZE_DT == null ? "|" : codeUser.FREEZE_DT + "|";
            content += codeUser.LAST_LOGIN_DT == null ? "|" : codeUser.LAST_LOGIN_DT + "|";
            content += codeUser.LAST_LOGOUT_DT == null ? "|" : codeUser.LAST_LOGOUT_DT + "|";

            return content;
        }


        /// <summary>
        /// (查詢)稽核軌跡的執行細項資訊content
        /// </summary>
        /// <param name="userMgrModel"></param>
        /// <returns></returns>
        public string trackLogContent(UserMgrModel userMgrModel) {
            String content = "";


            if (!StringUtil.isEmpty(userMgrModel.cUserID))
                content += "cUserID = " + userMgrModel.cUserID + "|";


            if (!StringUtil.isEmpty(userMgrModel.cUserName)) {
                String userName = "";
                if (userMgrModel.cUserName.Trim().Length > 1)
                    userName = userMgrModel.cUserName.Substring(0, 1) + "Ｏ" + userMgrModel.cUserName.Substring(2, userMgrModel.cUserName.Length - 2);
                else
                    userName = userMgrModel.cUserName;

                content += "cUserName = " + userName + "|";
            }
                


            if (!StringUtil.isEmpty(userMgrModel.isDisabled))
                content += "isDisabled = " + userMgrModel.isDisabled + "|";


            if (!StringUtil.isEmpty(userMgrModel.isMail))
                content += "isMail = " + userMgrModel.isMail + "|";



            if (!StringUtil.isEmpty(userMgrModel.codeRole))
                content += "codeRole = " + userMgrModel.codeRole + "|";


            if (!StringUtil.isEmpty(userMgrModel.cUpdUserID))
                content += "cUpdUserID = " + userMgrModel.cUpdUserID + "|";


            if (!StringUtil.isEmpty(userMgrModel.cUpdDateB))
                content += "cUpdDateB = " + userMgrModel.cUpdDateB + "|";

            if (!StringUtil.isEmpty(userMgrModel.cUpdDateE))
                content += "cUpdDateE = " + userMgrModel.cUpdDateE + "|";

            return content;
        }
    }
}
