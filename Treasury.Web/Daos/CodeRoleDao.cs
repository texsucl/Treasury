
using Treasury.WebUtils;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Treasury.Web.Models;
using System.Data.SqlClient;
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
namespace Treasury.WebDaos
{
    public class CodeRoleDao
    {


        public bool dupRoleName(string roleId, string authType, string roleName) {
            bool bDup = false;

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    CODE_ROLE role = db.CODE_ROLE
                        .Where(x => x.ROLE_ID != roleId && x.ROLE_AUTH_TYPE == authType && x.ROLE_NAME == roleName.Trim())
                        .FirstOrDefault();

                    if (role != null) {
                        if (!"".Equals(StringUtil.toString(role.ROLE_ID)))
                            bDup = true;
                    }


                    if (!bDup) {
                        string roleH = (from roleHis in db.CODE_ROLE_HIS
                                                join aply in db.AUTH_APPR.Where(x => x.AUTH_APLY_TYPE == "R" & x.APPR_STATUS == "1") on roleHis.ROLE_ID equals aply.APPR_MAPPING_KEY
                                                where roleHis.ROLE_ID != roleId
                                                   & roleHis.ROLE_AUTH_TYPE == authType
                                                    & roleHis.ROLE_NAME == roleName.Trim()
                                                select roleHis.ROLE_ID).FirstOrDefault();

                        if (!"".Equals(StringUtil.toString(roleH)))
                            bDup = true;

                    }
                }
            }

            return bDup;
        }


        /// <summary>
        /// 查詢有效的角色
        /// </summary>
        /// <returns></returns>
        public List<CODE_ROLE> qryValidRole(string roleAuthType)
        {

            bool broleAuthType = StringUtil.isEmpty(roleAuthType);

            using (new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    List<CODE_ROLE> roleList = db.CODE_ROLE
                        .Where(x => x.IS_DISABLED == "N" && (broleAuthType || (x.ROLE_AUTH_TYPE == roleAuthType)))
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
        public string jqGridRoleList(string roleAuthType)
        {
            List<CODE_ROLE> roleList = qryValidRole(roleAuthType);

            string controlStr = "";
            foreach (var item in roleList)
            {
                controlStr += StringUtil.toString(item.ROLE_ID) + ":" + StringUtil.toString(item.ROLE_NAME) + ";";
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
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    CODE_ROLE codeRole = db.CODE_ROLE.Where(x => x.ROLE_ID == roleId).FirstOrDefault<CODE_ROLE>();

                    return codeRole;
                }
            }
        }


        public List<RoleMgrModel> roleMgrQry(string codeRole, string roleAuthType , string isDIsabled, string vMemo, string cUpdUserID)
        {
            bool bCodeRole = StringUtil.isEmpty(codeRole);
            bool bRoleAuthType = StringUtil.isEmpty(roleAuthType);
            bool bisDIsabled = StringUtil.isEmpty(isDIsabled);
            bool bcUpdUserID = StringUtil.isEmpty(cUpdUserID);


            using (new TransactionScope (
                 TransactionScopeOption.Required,
                 new TransactionOptions
                 {
                     IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                 }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    List<RoleMgrModel> roleList = (from role in db.CODE_ROLE
                                                   join codeFlag in db.SYS_CODE.Where(x => x.CODE_TYPE == "IS_DISABLED") on role.IS_DISABLED equals codeFlag.CODE into psFlag
                                                   from xFlag in psFlag.DefaultIfEmpty()

                                                   join codeReview in db.SYS_CODE.Where(x => x.CODE_TYPE == "DATA_STATUS") on role.DATA_STATUS equals codeReview.CODE into psReview
                                                   from xReview in psReview.DefaultIfEmpty()

                                                   join codeAuthType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ROLE_AUTH_TYPE") on role.ROLE_AUTH_TYPE equals codeAuthType.CODE into psAuthType
                                                   from xAuthType in psAuthType.DefaultIfEmpty()

                                                   where 1 == 1

                                                       & (bCodeRole || (role.ROLE_ID == codeRole.Trim()))
                                                       & (bRoleAuthType || (role.ROLE_AUTH_TYPE == roleAuthType.Trim()))
                                                       & (bisDIsabled || (role.IS_DISABLED == isDIsabled.Trim()))
                                                       & (bcUpdUserID || (role.LAST_UPDATE_UID == cUpdUserID.Trim()))

                                                   select new RoleMgrModel
                                                   {
                                                       cRoleID = role.ROLE_ID,
                                                       cRoleName = role.ROLE_NAME.Trim(),
                                                       roleAuthType = role.ROLE_AUTH_TYPE,
                                                       roleAuthTypeDesc = (xAuthType == null ? String.Empty : xAuthType.CODE_VALUE),
                                                       isDisabled = (xFlag == null ? String.Empty : xFlag.CODE_VALUE),
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


        /**
        新增角色:
            **/
        public int Create(CODE_ROLE cODEROLE, SqlConnection conn, SqlTransaction transaction)
        {


            // string strConn = DbUtil.GetDBAccountConnStr();

            string sql = @"

INSERT INTO [dbo].[CODE_ROLE]
           ([ROLE_ID]
           ,[ROLE_NAME]
           ,[ROLE_AUTH_TYPE]
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
,@ROLE_AUTH_TYPE
,@IS_DISABLED
,@MEMO
,@DATA_STATUS
,@CREATE_UID
,@CREATE_DT
,@LAST_UPDATE_UID
,@LAST_UPDATE_DT
,@APPR_UID
,@APPR_DT
)";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(cODEROLE.ROLE_ID));
                command.Parameters.AddWithValue("@ROLE_NAME", StringUtil.toString(cODEROLE.ROLE_NAME));
                command.Parameters.AddWithValue("@ROLE_AUTH_TYPE", StringUtil.toString(cODEROLE.ROLE_AUTH_TYPE));
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



        /**
        異動角色:
            1.查詢角色檔資料
            2.異動流水號檔
            3.新增資料至角色檔
            **/
        public int Update(CODE_ROLE cODEROLE, SqlConnection conn, SqlTransaction transaction)
        {


            // string strConn = DbUtil.GetDBAccountConnStr();

            string sql = @"update CODE_ROLE
        set ROLE_NAME = @ROLE_NAME
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


        /**
        將角色檔的各欄位組成一字串，for Log
            **/
        public String roleLogContent(CODE_ROLE codeRole)
        {
            String content = "";

            content += StringUtil.toString(codeRole.ROLE_ID) + "|";
            content += StringUtil.toString(codeRole.ROLE_NAME) + "|";
            content += StringUtil.toString(codeRole.ROLE_AUTH_TYPE) + "|";
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





        /**
        查詢出所有角色資料(for畫面下拉選單使用)
        **/
        public SelectList loadSelectList()
        {
            dbTreasuryEntities context = new dbTreasuryEntities();

            var result = context.CODE_ROLE;
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