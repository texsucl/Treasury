
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.SqlServer;
using System.Transactions;
using FAP.Web.BO;

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
namespace FAP.Web.Daos
{
    public class CodeUserDao 
    {
        

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
                using (dbFGLEntities db = new dbFGLEntities())
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
                using (dbFGLEntities db = new dbFGLEntities())
            {

                rows = (from user in db.CODE_USER
                        join codeFlag in db.SYS_CODE.Where(x => x.CODE_TYPE == "IS_DISABLED" & x.SYS_CD == "SSO") on user.IS_DISABLED equals codeFlag.CODE into psFlag
                        from xFlag in psFlag.DefaultIfEmpty()

                        join codeMail in db.SYS_CODE.Where(x => x.CODE_TYPE == "YN_FLAG" & x.SYS_CD == "SSO") on user.IS_DISABLED equals codeMail.CODE into psMail
                        from xMail in psMail.DefaultIfEmpty()

                        join role in db.CODE_USER_ROLE on user.USER_ID equals role.USER_ID into psRole
                        from xRole in psRole.DefaultIfEmpty()

                        join codeStatus in db.SYS_CODE.Where(x => x.CODE_TYPE == "DATA_STATUS" & x.SYS_CD == "SSO") on user.DATA_STATUS equals codeStatus.CODE into psStatus
                        from xStatus in psStatus.DefaultIfEmpty()

                        where 1 == 1
                            & (bcUserID || (user.USER_ID == userMgrModel.cUserID.Trim()))
                            & (bisDisabled || (user.IS_DISABLED == userMgrModel.isDisabled.Trim()))
                            & (bisMail || (user.IS_MAIL == userMgrModel.isMail.Trim()))
                            & (bcodeRole || (xRole.ROLE_ID == userMgrModel.codeRole.Trim()))
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

        


        ///// <summary>
        ///// 以userId為鍵項，查詢使用者資料
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public CODE_USER qryByKey(String userId) {
        //    using (dbFGLEntities db = new dbFGLEntities())
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
            using (dbFGLEntities db = new dbFGLEntities())
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
