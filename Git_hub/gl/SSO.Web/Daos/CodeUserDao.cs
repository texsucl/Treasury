
using SSO.Web.Models;
using SSO.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SSO.Web.Utils;
using SSO.Web.BO;
using System.Data.Entity.SqlServer;
using System.Transactions;
using System.Data.SqlClient;
using SSO.Web.Utility;
using MoreLinq;

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
/// 修改日期/修改人：20200131 B0077 黃黛鈺
/// 需求單號：201911280279-00
/// 修改內容：調整報表條件及格式內容
/// </summary>
/// 
namespace SSO.Web.Daos
{
    public class CodeUserDao 
    {
        //200319 Bianco add userRoleList, rptFormat, userUnit, userFrom
        public List<AuthRptModel> qryForAuthRpt(string[] ownerUntList, string[] userList, string[] userRoleList, string rptFormat, string[] userUnit, string[] userFrom) {

            List<AuthRptModel> rows = new List<AuthRptModel>();
            IEnumerable<AuthRptModel> data = null;

            bool bOwnerUnit = false;
            if (ownerUntList == null)
                bOwnerUnit = true;
            else if(ownerUntList.Length == 0)
                bOwnerUnit = true;

            List<string> test = new List<string>();
            test.Add("VL70B");
            test.Add("VE401");


            using (dbFGLEntities db = new dbFGLEntities())
            {
                //200319 Bianco 判斷報表格式
                var dbCODE_FUNC = db.CODE_FUNC.AsNoTracking().ToList();
                switch (rptFormat)
                {
                    case "allRpt": //程式權限授權報表
                        data = (from user in db.CODE_USER.Where(x => x.IS_DISABLED == "N")
                                join userRole in db.CODE_USER_ROLE on user.USER_ID equals userRole.USER_ID
                                join codeRole in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N") on userRole.ROLE_ID equals codeRole.ROLE_ID
                                join roleFunc in db.CODE_ROLE_FUNC on userRole.ROLE_ID equals roleFunc.ROLE_ID
                                join func in db.CODE_FUNC.Where(x => x.IS_DISABLED == "N") on roleFunc.FUNC_ID equals func.FUNC_ID
                                join cFuncG in db.CODE_FUNC.Where(x => x.IS_DISABLED == "N") on func.RPT_BELONG_FUNC equals cFuncG.FUNC_ID into psFuncG
                                from xFuncG in psFuncG.DefaultIfEmpty()
                                where userList.Contains(user.USER_ID.TrimEnd())
                                & (bOwnerUnit || (!bOwnerUnit & ownerUntList.Contains(codeRole.AUTH_UNIT)))
                                select new
                                {
                                    USER_ID = user.USER_ID,
                                    USER_UNIT = user.USER_UNIT,
                                    RPT_BELONG_FUNC = func.RPT_BELONG_FUNC,
                                    FUNC_NAME = xFuncG.FUNC_NAME,
                                    AUTH_UNIT = codeRole.AUTH_UNIT,
                                    ROLE_ID = codeRole.ROLE_ID,
                                    ROLE_NAME = codeRole.ROLE_NAME,
                                    PGM_ID = func.FUNC_ID,
                                    PGM_NAME = func.FUNC_NAME,
                                    PARENT_FUNC_ID = func.PARENT_FUNC_ID,
                                }).AsEnumerable()
                                .Select(x => new AuthRptModel
                                {
                                    user_id = x.USER_ID,
                                    user_unit = x.USER_UNIT,
                                    grp_id = x.RPT_BELONG_FUNC,
                                    grp_name = x.FUNC_NAME,
                                    owner_unit = x.AUTH_UNIT,
                                    role_id = x.ROLE_ID,
                                    role_name = x.ROLE_NAME,
                                    pgm_id = x.PGM_ID,
                                    pgm_name = x.PGM_NAME 
                                }).Distinct();
                        break;
                    case "userRoleRpt": //角色功能報表

                        data = (
                                from codeRole in db.CODE_ROLE.Where(x => x.IS_DISABLED == "N")
                                join roleFunc in db.CODE_ROLE_FUNC on codeRole.ROLE_ID equals roleFunc.ROLE_ID
                                join func in db.CODE_FUNC.Where(x => x.IS_DISABLED == "N") on roleFunc.FUNC_ID equals func.FUNC_ID
                                select new
                                {
                                    //USER_ID = user.USER_ID,
                                    //USER_UNIT = user.USER_UNIT,
                                    RPT_BELONG_FUNC = func.RPT_BELONG_FUNC,
                                    FUNC_NAME = func.FUNC_NAME,
                                    AUTH_UNIT = codeRole.AUTH_UNIT,
                                    ROLE_ID = codeRole.ROLE_ID,
                                    ROLE_NAME = codeRole.ROLE_NAME,
                                    PGM_ID = func.FUNC_ID,
                                    PGM_NAME = func.FUNC_NAME,
                                    PARENT_FUNC_ID = func.PARENT_FUNC_ID,
                                }).AsEnumerable()
                                .Where(x => ownerUntList.Contains(x.AUTH_UNIT), ownerUntList != null || ownerUntList.Length != 0)
                                .Select(x => new AuthRptModel
                                {
                                    user_id = "N/A",
                                    user_unit = "N/A",
                                    grp_id = x.RPT_BELONG_FUNC,
                                    grp_name = $"{dbCODE_FUNC.FirstOrDefault(y => y.FUNC_ID == x.RPT_BELONG_FUNC)?.FUNC_NAME}",
                                    owner_unit = x.AUTH_UNIT,
                                    role_id = x.ROLE_ID,
                                    role_name = x.ROLE_NAME,
                                    pgm_id = x.PGM_ID,
                                    pgm_name = x.PGM_NAME,
                                    parent_func_id = x.PARENT_FUNC_ID,
                                    parent_func_name = string.IsNullOrWhiteSpace(x.PARENT_FUNC_ID) ? "" : dbCODE_FUNC.FirstOrDefault(y => y.FUNC_ID == x.PARENT_FUNC_ID).FUNC_NAME
                                });
                        break;
                    case "userRpt": //使用者角色報表
                        //參考SQL
                        //SELECT DISTINCT CODE_FUNC.RPT_BELONG_FUNC, (select DISTINCT a.FUNC_NAME from CODE_FUNC as a where a.FUNC_ID = CODE_FUNC.RPT_BELONG_FUNC), CODE_ROLE.AUTH_UNIT, CODE_ROLE.ROLE_ID, CODE_USER.USER_ID, CODE_USER.USER_UNIT
                        //FROM   CODE_ROLE
                        //Left JOIN CODE_USER_ROLE ON CODE_ROLE.ROLE_ID = CODE_USER_ROLE.ROLE_ID
                        //Left jOIN CODE_USER on CODE_USER_ROLE.USER_ID = CODE_USER.USER_ID
                        //Left JOIN CODE_ROLE_FUNC on CODE_ROLE.ROLE_ID = CODE_ROLE_FUNC.ROLE_ID
                        //Left JOIN CODE_FUNC on CODE_ROLE_FUNC.FUNC_ID = CODE_FUNC.FUNC_ID
                        //where CODE_ROLE.AUTH_UNIT = 'VL70B' and CODE_ROLE.IS_DISABLED = 'N'
                        //and((CODE_USER_ROLE.USER_ID is not null and CODE_USER.IS_DISABLED = 'N') or CODE_USER_ROLE.USER_ID is null) and CODE_FUNC.IS_DISABLED = 'N'

                        var dbCODE_ROLE_FUNC = db.CODE_ROLE_FUNC.AsNoTracking().ToList();

                        var userData = db.CODE_ROLE.AsNoTracking().Where(x => x.IS_DISABLED == "N")
                             .GroupJoin(db.CODE_USER_ROLE.AsNoTracking(),    //Left JOIN CODE_USER_ROLE ON CODE_ROLE.ROLE_ID = CODE_USER_ROLE.ROLE_ID
                             cROLE => cROLE.ROLE_ID,
                             uRole => uRole.ROLE_ID,
                             (cROLE, uRole) => uRole.Select(x => new { CODE_ROLE = cROLE, CODE_USER_ROLE = x })
                             .DefaultIfEmpty(new { CODE_ROLE = cROLE, CODE_USER_ROLE = (CODE_USER_ROLE)null }))
                             .SelectMany(g => g)
                             .GroupJoin(db.CODE_USER.AsNoTracking(),         //Left jOIN CODE_USER on CODE_USER_ROLE.USER_ID = CODE_USER.USER_ID
                             firstJoin => firstJoin.CODE_USER_ROLE.USER_ID,
                             user => user.USER_ID,
                             (firstJoin, user) => user.Select(y => new { CODE_ROLE = firstJoin.CODE_ROLE, CODE_USER_ROLE = firstJoin.CODE_USER_ROLE, CODE_USER = y })
                             .DefaultIfEmpty(new { CODE_ROLE = firstJoin.CODE_ROLE, CODE_USER_ROLE = firstJoin.CODE_USER_ROLE, CODE_USER = (CODE_USER)null }))
                             .SelectMany(g1 => g1)
                             .GroupJoin(db.CODE_ROLE_FUNC.AsNoTracking(),    //Left JOIN CODE_ROLE_FUNC on CODE_ROLE.ROLE_ID = CODE_ROLE_FUNC.ROLE_ID
                             secendJoin => secendJoin.CODE_ROLE.ROLE_ID,
                             cROLE_FUNC => cROLE_FUNC.ROLE_ID,
                             (secendJoin, cROLE_FUNC) => cROLE_FUNC.Select(z => new { CODE_ROLE = secendJoin.CODE_ROLE, CODE_USER_ROLE = secendJoin.CODE_USER_ROLE, CODE_USER = secendJoin.CODE_USER, CODE_ROLE_FUNC = z })
                             .DefaultIfEmpty(new { CODE_ROLE = secendJoin.CODE_ROLE, CODE_USER_ROLE = secendJoin.CODE_USER_ROLE, CODE_USER = secendJoin.CODE_USER, CODE_ROLE_FUNC = (CODE_ROLE_FUNC)null }))
                             .SelectMany(g2 => g2)
                             .GroupJoin(db.CODE_FUNC.AsNoTracking(),         //Left JOIN CODE_FUNC on CODE_ROLE_FUNC.FUNC_ID = CODE_FUNC.FUNC_ID
                             thirdJoin => thirdJoin.CODE_ROLE_FUNC.FUNC_ID,
                             cFUNC => cFUNC.FUNC_ID,
                             (thirdJoin, cFUNC) => cFUNC.Select(u => new { CODE_ROLE = thirdJoin.CODE_ROLE, CODE_USER_ROLE = thirdJoin.CODE_USER_ROLE, CODE_USER = thirdJoin.CODE_USER, CODE_ROLE_FUNC = thirdJoin.CODE_ROLE_FUNC, CODE_FUNC = u })
                             .DefaultIfEmpty(new { CODE_ROLE = thirdJoin.CODE_ROLE, CODE_USER_ROLE = thirdJoin.CODE_USER_ROLE, CODE_USER = thirdJoin.CODE_USER, CODE_ROLE_FUNC = thirdJoin.CODE_ROLE_FUNC, CODE_FUNC = (CODE_FUNC)null }))
                             .SelectMany(g3 => g3)
                             .AsEnumerable()
                             .Where(x => string.IsNullOrWhiteSpace(x.CODE_USER_ROLE?.USER_ID) || (!string.IsNullOrWhiteSpace(x.CODE_USER_ROLE?.USER_ID) && x.CODE_USER?.IS_DISABLED == "N"))  //and((CODE_USER_ROLE.USER_ID is not null and CODE_USER.IS_DISABLED = 'N') or CODE_USER_ROLE.USER_ID is null)
                             .Where(x => ownerUntList.Contains(x.CODE_ROLE.AUTH_UNIT), ownerUntList != null)
                             .Where(x => string.IsNullOrWhiteSpace(x.CODE_USER?.USER_ID) || userList.Contains(x.CODE_USER?.USER_ID), userList != null) 
                             .Where(x => !string.IsNullOrWhiteSpace(x.CODE_USER?.USER_ID), userUnit != null || userFrom != null) //前端有選使用者單位 || 使用者時，排除使用者及使用單位為NULL 的資料
                             .Where(x => !string.IsNullOrWhiteSpace(x.CODE_FUNC?.RPT_BELONG_FUNC))                           
                             .Select(x => new AuthRptModel {
                                 owner_unit = x.CODE_ROLE.AUTH_UNIT,
                                 role_id = x.CODE_ROLE.ROLE_ID,
                                 role_name = $"{dbCODE_FUNC.FirstOrDefault(y => y.FUNC_ID == x.CODE_FUNC?.RPT_BELONG_FUNC)?.FUNC_NAME}-{x.CODE_ROLE.ROLE_NAME}",
                                 user_id = string.IsNullOrWhiteSpace(x.CODE_USER?.USER_ID) ? "N/A" : x.CODE_USER?.USER_ID,
                                 user_unit = string.IsNullOrWhiteSpace(x.CODE_USER?.USER_UNIT) ? "N/A" : x.CODE_USER?.USER_UNIT,
                                 grp_id = x.CODE_FUNC.RPT_BELONG_FUNC
                             });

                        data = userData.DistinctBy(x => new { x.grp_id, x.role_id, x.owner_unit, x.user_id, x.user_unit}) //DISTINCT CODE_FUNC.RPT_BELONG_FUNC, CODE_ROLE.AUTH_UNIT, CODE_ROLE.ROLE_ID, CODE_USER.USER_ID, CODE_USER.USER_UNIT
                            .OrderBy(x => x.owner_unit)
                            .ThenBy(x => x.grp_id);
                        break;
                    
                }

                rows = data.Where(x => userRoleList.Contains(x.role_id), userRoleList != null).ToList();   //依角色選擇
            }

            return rows;
        }



        /// <summary>
        /// 查詢符合特定單位，但不存在人事資料的使用者
        /// </summary>
        /// <param name="userUnit"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<CODE_USER> qryForAuthRptOthUser(string[] userUnit, string[] user) {
            List<CODE_USER> rows = new List<CODE_USER>();

            using (dbFGLEntities db = new dbFGLEntities())
            {
           //     List<CODE_USER> rows = db.CODE_USER
           //.Where(x => userUnit.Contains(x.USER_UNIT)).ToList();
                rows = db.CODE_USER
           .Where(x => x.IS_DISABLED == "N" & userUnit.Contains(x.USER_UNIT) & !user.Contains(x.USER_ID)).ToList();
            }

            return rows;
        }




        /**
        查詢出所有使用者資料(for畫面下拉選單使用)
        **/
        public SelectList loadSelectList()
        {
            dbFGLEntities context = new dbFGLEntities();

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

            using (dbFGLEntities db = new dbFGLEntities())
            {

                rows = (from u in db.CODE_USER

                        join ur in db.CODE_USER_ROLE on u.USER_ID equals ur.USER_ID

                        join r in db.CODE_ROLE on ur.ROLE_ID equals r.ROLE_ID
                        where 1 == 1
                            & u.IS_DISABLED == "N"
                            & r.IS_DISABLED == "N"

                        select new UserMgrModel()
                        {
                            cUserID = u.USER_ID.Trim(),
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

            string sql = @"

INSERT INTO [dbo].[CODE_USER]
           ([USER_ID]
           ,[USER_UNIT]
           ,[IS_DISABLED]
           ,[IS_MAIL]
           ,[DATA_STATUS]
           ,[CREATE_UID]
           ,[CREATE_DT]
           ,[CREATE_UNIT]
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
,@CREATE_UNIT
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
                command.Parameters.AddWithValue("@CREATE_UNIT", StringUtil.toString(user.CREATE_UNIT));
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
            bool bAuthUnit = StringUtil.isEmpty(userMgrModel.authUnit);
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
                            cWorkUnitCode = user.USER_UNIT.Trim(),
                            cCrtUserID = user.CREATE_UID.Trim(),
                            cCrtUnit = user.CREATE_UNIT.Trim(),
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



        public int Delete(CODE_USER user, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete  [CODE_USER]
             where USER_ID = @USER_ID";

            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@USER_ID", StringUtil.toString(user.USER_ID));
 
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
