
using SSO.Web.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using SSO.Web.ViewModels;
using System.Data.SqlClient;
using SSO.Web.Models;

/// <summary>
/// 功能說明：CodeRoleDao角色資料檔
/// 初版作者：20170817 黃黛鈺
/// 修改歷程：20170817 黃黛鈺 
///           需求單號：201707240447-01 
///           初版
/// ==============================================
/// 修改日期/修改人：20180214 黃黛鈺 
/// 需求單號：201801230413-00 
/// 修改內容：加入覆核功能
/// ==============================================
/// /// </summary>
namespace SSO.Web.Daos
{
    public class CodeRoleDao
    {


        /// <summary>
        /// 查詢有效的角色
        /// </summary>
        /// <returns></returns>
        public List<CODE_ROLE> qryValidRole(string[] authUnit, bool bContainFreeAuth, string freeAuth)
        {
            bool bAuthUnit = false;
            try
            {
                if (authUnit.Count() > 0)
                    bAuthUnit = true;
            }
            catch (Exception e) {
            }
          

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<CODE_ROLE> roleList = db.CODE_ROLE
                        .Where(x => ((!bAuthUnit || (bAuthUnit & authUnit.Contains(x.AUTH_UNIT)))
            || (bContainFreeAuth & !authUnit.Contains(x.AUTH_UNIT) & x.FREE_AUTH == freeAuth))
            & x.IS_DISABLED == "N")
                        .ToList<CODE_ROLE>();

                    return roleList;
                }
            }
        }



        /// <summary>
        /// for GRID下拉選單
        /// </summary>
        /// <param name="cType"></param>
        /// <returns></returns>
        public string jqGridRoleList(string[] authUnit, bool bContainFreeAuth, string freeAuth)
        {
            List<CODE_ROLE> roleList = qryValidRole(authUnit, bContainFreeAuth, freeAuth);

            string controlStr = "";
            foreach (var item in roleList)
            {
                controlStr += StringUtil.toString(item.ROLE_ID) + ":" + StringUtil.toString(item.ROLE_NAME) + ";";
            }
            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }

        public string jqGridRoleAuthUnitList(string[] roleList)
        {
            string controlStr = "";
            List<RoleMgrModel> rows = new List<RoleMgrModel>();

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    rows = (from roleFunc in db.CODE_ROLE_FUNC
                                       join role in db.CODE_ROLE on roleFunc.ROLE_ID equals role.ROLE_ID             
                                                   where 1 == 1
                                                   & roleList.Contains(role.ROLE_ID)
                                                   select new RoleMgrModel
                                                   {
                                                       cRoleID = role.ROLE_ID,
                                                       authUnit = role.AUTH_UNIT
                                                   }).Distinct().ToList();
                }
            }

            Dictionary<string, string> unitMap = new Dictionary<string, string>();
            OaDeptDao oaDeptDao = new OaDeptDao();


            foreach (var item in rows)
            {
                string roleId = StringUtil.toString(item.cRoleID);
                string authUnitNm = "";
                if (unitMap.ContainsKey(roleId))
                    authUnitNm = unitMap[roleId];
                else {
                    VW_OA_DEPT dept = oaDeptDao.qryByDptCd(StringUtil.toString(item.authUnit));
                    if (dept != null)
                    {
                        authUnitNm = StringUtil.toString(dept.DPT_NAME);
                        unitMap.Add(roleId, authUnitNm);

                    }
                    else
                        authUnitNm = StringUtil.toString(item.authUnit);
                }

                controlStr += roleId + ":" + item.authUnit + "-" + authUnitNm + ";";
            }

            controlStr = controlStr.Substring(0, controlStr.Length - 1) + "";
            return controlStr;
        }
        


        /**
        查詢角色檔(以"角色編號"為鍵項)
            **/
        public CODE_ROLE qryRoleByKey(String roleId)
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
                    CODE_ROLE codeRole = db.CODE_ROLE.Where(x => x.ROLE_ID == roleId).FirstOrDefault<CODE_ROLE>();

                    return codeRole;
                }
            }
        }


        public List<RoleMgrModel> roleMgrQry(string[] authUnit, string codeRole, string isDIsabled, string vMemo, string cUpdUserID)
        {
            bool bAuthUnit = false;
            if (authUnit.Count() > 0)
                bAuthUnit = true;


            //bool bAuthUnit = StringUtil.isEmpty(authUnit);
            bool bCodeRole = StringUtil.isEmpty(codeRole);
            bool bisDIsabled = StringUtil.isEmpty(isDIsabled);
            bool bcUpdUserID = StringUtil.isEmpty(cUpdUserID);


            using (new TransactionScope (
                 TransactionScopeOption.Required,
                 new TransactionOptions
                 {
                     IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                 }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    List<RoleMgrModel> roleList = (from role in db.CODE_ROLE
                                                   join codeFlag in db.SYS_CODE.Where(x => x.CODE_TYPE == "IS_DISABLED" & x.SYS_CD == "SSO") on role.IS_DISABLED equals codeFlag.CODE into psFlag
                                                   from xFlag in psFlag.DefaultIfEmpty()

                                                   join codeFlag in db.SYS_CODE.Where(x => x.CODE_TYPE == "YN_FLAG" & x.SYS_CD == "SSO") on role.FREE_AUTH equals codeFlag.CODE into psYN
                                                   from xYN in psYN.DefaultIfEmpty()

                                                   join codeReview in db.SYS_CODE.Where(x => x.CODE_TYPE == "DATA_STATUS" & x.SYS_CD == "SSO") on role.DATA_STATUS equals codeReview.CODE into psReview
                                                   from xReview in psReview.DefaultIfEmpty()

                                                   where 1 == 1
                                                   & (!bAuthUnit || (bAuthUnit & authUnit.Contains(role.AUTH_UNIT)))
                                                       & (bCodeRole || (role.ROLE_ID == codeRole.Trim()))
                                                       & (bisDIsabled || (role.IS_DISABLED == isDIsabled.Trim()))
                                                       & (bcUpdUserID || (role.LAST_UPDATE_UID == cUpdUserID.Trim()))

                                                   select new RoleMgrModel
                                                   {
                                                       cRoleID = role.ROLE_ID,
                                                       cRoleName = role.ROLE_NAME,
                                                       isDisabled = (xFlag == null ? String.Empty : xFlag.CODE_VALUE),
                                                       freeAuth = (xYN == null ? String.Empty : xYN.CODE_VALUE),
                                                       vMemo = role.MEMO,
                                                       freezeUid = role.FREEZE_UID == null ? "" : role.FREEZE_UID,
                                                       cUpdUserID = role.LAST_UPDATE_UID == null ? "" : role.LAST_UPDATE_UID,
                                                       cUpdDateTime = role.LAST_UPDATE_DT == null ? "" : SqlFunctions.DateName("year", role.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DatePart("m", role.LAST_UPDATE_DT) + "/" +
                                                                     SqlFunctions.DateName("day", role.LAST_UPDATE_DT).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", role.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", role.LAST_UPDATE_DT).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", role.LAST_UPDATE_DT).Trim(),
                                                       dataStatus = (xReview == null ? String.Empty : xReview.CODE_VALUE) 
                                                   }).ToList();

                    return roleList;

                }
            }
                    

        }


        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="cODEROLE"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Create(CODE_ROLE cODEROLE, SqlConnection conn, SqlTransaction transaction)
        {


            // string strConn = DbUtil.GetDBAccountConnStr();

            string sql = @"

INSERT INTO [dbo].[CODE_ROLE]
           ([ROLE_ID]
           ,[ROLE_NAME]
           ,[AUTH_UNIT]
           ,[FREE_AUTH]
           ,[IS_DISABLED]
           ,[MEMO]
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
 @ROLE_ID
,@ROLE_NAME
,@AUTH_UNIT
,@FREE_AUTH
,@IS_DISABLED
,@MEMO
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
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(cODEROLE.ROLE_ID));
                command.Parameters.AddWithValue("@ROLE_NAME", StringUtil.toString(cODEROLE.ROLE_NAME));
                command.Parameters.AddWithValue("@AUTH_UNIT", StringUtil.toString(cODEROLE.AUTH_UNIT));
                command.Parameters.AddWithValue("@FREE_AUTH", StringUtil.toString(cODEROLE.FREE_AUTH));
                command.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(cODEROLE.IS_DISABLED));
                command.Parameters.AddWithValue("@MEMO", StringUtil.toString(cODEROLE.MEMO));
                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(cODEROLE.DATA_STATUS));
                command.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(cODEROLE.CREATE_UID));

                command.Parameters.Add("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.CREATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(cODEROLE.LAST_UPDATE_UID));

                command.Parameters.Add("@LAST_UPDATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.LAST_UPDATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_UID", StringUtil.toString(cODEROLE.APPR_UID));

                command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.APPR_DT ?? System.DBNull.Value;


                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }



        /// <summary>
        ///  異動角色:
        ///     1.查詢角色檔資料
        ///     2.異動流水號檔
        ///     3.新增資料至角色檔
        /// </summary>
        /// <param name="cODEROLE"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Update(CODE_ROLE cODEROLE, SqlConnection conn, SqlTransaction transaction)
        {


            // string strConn = DbUtil.GetDBAccountConnStr();

            string sql = @"update CODE_ROLE
        set ROLE_NAME = @ROLE_NAME
           ,FREE_AUTH = @FREE_AUTH
           ,IS_DISABLED = @IS_DISABLED
           ,MEMO = @MEMO
           ,DATA_STATUS = @DATA_STATUS
           ,CREATE_UID = @CREATE_UID
           ,CREATE_DT = @CREATE_DT
           ,LAST_UPDATE_UID = @LAST_UPDATE_UID
           ,LAST_UPDATE_DT = @LAST_UPDATE_DT
           ,APPR_UID = @APPR_UID
           ,APPR_DT = @APPR_DT
           ,FREEZE_UID = @FREEZE_UID
           ,FREEZE_DT = @FREEZE_DT
        where 1=1
        and ROLE_ID = @ROLE_ID
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(cODEROLE.ROLE_ID));
                command.Parameters.AddWithValue("@ROLE_NAME", StringUtil.toString(cODEROLE.ROLE_NAME));
                command.Parameters.AddWithValue("@FREE_AUTH", StringUtil.toString(cODEROLE.FREE_AUTH));
                command.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(cODEROLE.IS_DISABLED));
                command.Parameters.AddWithValue("@MEMO", StringUtil.toString(cODEROLE.MEMO));
                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(cODEROLE.DATA_STATUS));
                command.Parameters.AddWithValue("@CREATE_UID", StringUtil.toString(cODEROLE.CREATE_UID));

                command.Parameters.Add("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.CREATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(cODEROLE.LAST_UPDATE_UID));

                command.Parameters.Add("@LAST_UPDATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.LAST_UPDATE_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_UID", StringUtil.toString(cODEROLE.APPR_UID));

                command.Parameters.Add("@APPR_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.APPR_DT ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@FREEZE_UID", StringUtil.toString(cODEROLE.FREEZE_UID));

                command.Parameters.Add("@FREEZE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)cODEROLE.FREEZE_DT ?? System.DBNull.Value;




                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }


        /// <summary>
        /// 將角色檔的各欄位組成一字串，for Log
        /// </summary>
        /// <param name="codeRole"></param>
        /// <returns></returns>
        public String roleLogContent(CODE_ROLE codeRole)
        {
            String content = "";

            content += StringUtil.toString(codeRole.ROLE_ID) + "|";
            content += StringUtil.toString(codeRole.ROLE_NAME) + "|";
            content += StringUtil.toString(codeRole.AUTH_UNIT) + "|";
            content += StringUtil.toString(codeRole.FREE_AUTH) + "|";
            content += StringUtil.toString(codeRole.IS_DISABLED) + "|";
            content += StringUtil.toString(codeRole.MEMO) + "|";
            content += StringUtil.toString(codeRole.DATA_STATUS) + "|";
            content += StringUtil.toString(codeRole.CREATE_UID) + "|";
            content += codeRole.CREATE_DT == null ? "|" : codeRole.CREATE_DT + "|";
            content += StringUtil.toString(codeRole.LAST_UPDATE_UID) + "|";
            content += codeRole.LAST_UPDATE_DT == null ? "|" : codeRole.LAST_UPDATE_DT + "|";
            content += StringUtil.toString(codeRole.APPR_UID) + "|";
            content += codeRole.APPR_DT == null ? "|" : codeRole.APPR_DT + "|";
            content += StringUtil.toString(codeRole.FREEZE_UID) + "|";
            content += codeRole.FREEZE_DT == null ? "|" : codeRole.FREEZE_DT + "|";

            return content;
        }





        /// <summary>
        /// 查詢出所有角色資料(for畫面下拉選單使用)
        /// </summary>
        /// <param name="authUnit"></param>
        /// <param name="bContainFreeAuth"></param>
        /// <param name="freeAuth"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string authUnit, bool bContainFreeAuth, string freeAuth)
        {
            bool bAuthUnit = StringUtil.isEmpty(authUnit);
            bool bFreeAuth = StringUtil.isEmpty(freeAuth);

            dbFGLEntities context = new dbFGLEntities();

            var result = context.CODE_ROLE.Where(x => (bAuthUnit || x.AUTH_UNIT == authUnit)
            || (x.AUTH_UNIT != authUnit && (bContainFreeAuth == false || (bContainFreeAuth == true && x.FREE_AUTH == freeAuth))));
            var items = new SelectList
                (
                items: result,
                dataValueField: "ROLE_ID",
                dataTextField: "ROLE_NAME",
                selectedValue: (object)null
                );

            return items;
        }



        /// <summary>
        /// 查詢出指定OWNER單位的角色資料(for畫面下拉選單使用)
        /// </summary>
        /// <param name="authUnit"></param>
        /// <param name="bContainFreeAuth"></param>
        /// <param name="freeAuth"></param>
        /// <returns></returns>
        public SelectList loadSelectList(string[] authUnit, bool bContainFreeAuth, string freeAuth)
        {
            bool bAuthUnit = false;
            bool bFreeAuth = StringUtil.isEmpty(freeAuth);

            if (authUnit.Count() > 0)
                bAuthUnit = true;

            dbFGLEntities context = new dbFGLEntities();

            var result = context.CODE_ROLE.Where(x => ((!bAuthUnit || (bAuthUnit & authUnit.Contains(x.AUTH_UNIT)))
            || (bContainFreeAuth & !authUnit.Contains(x.AUTH_UNIT) & x.FREE_AUTH == freeAuth))
            & x.IS_DISABLED == "N");

           // var result = context.CODE_ROLE.Where(x => (!bAuthUnit || (bAuthUnit & authUnit.Contains(x.AUTH_UNIT))));

            var items = new SelectList
                (
                items: result,
                dataValueField: "ROLE_ID",
                dataTextField: "ROLE_NAME",
                selectedValue: (object)null
                );

            return items;
        }


    }
}