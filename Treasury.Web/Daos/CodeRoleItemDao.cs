using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using Treasury.WebUtils;

namespace Treasury.Web.Daos
{
    public class CodeRoleItemDao
    {
        //角色存取項目、表單申請報表
        public List<CodeRoleItemModel> qryRoleForAuthRpt( string authType)
        {


            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {


                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    var rows = (from main in db.CODE_ROLE_ITEM

                                join role in db.CODE_ROLE on main.ROLE_ID equals role.ROLE_ID

                                join d in db.TREA_ITEM on main.ITEM_ID equals d.ITEM_ID into psItem
                                from xItem in psItem.DefaultIfEmpty()

                                join opType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ITEM_OP_TYPE") on xItem.ITEM_OP_TYPE equals opType.CODE into psOpType
                                from xOpType in psOpType.DefaultIfEmpty()

                                where 1 == 1
                                    & main.AUTH_TYPE == authType
                                    & role.IS_DISABLED == "N"
                                    & xItem.IS_DISABLED == "N"
                                select new CodeRoleItemModel
                                {
                                    id = main.AUTH_TYPE + main.ITEM_ID,
                                    roleId = main.ROLE_ID,
                                    roleName = role.ROLE_NAME.Trim(),
                                    itemId = main.ITEM_ID,
                                    authType = main.AUTH_TYPE,
                                    itemOpType = xItem.ITEM_OP_TYPE,
                                    itemOpTypeDesc = xOpType == null ? "" : xOpType.CODE_VALUE,
                                    itemDesc = xItem.ITEM_DESC.Trim()


                                }).ToList<CodeRoleItemModel>();
                    return rows;

                }

            }

        }



        /// <summary>
        /// 依角色查詢
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<CodeRoleItemModel> qryForAppr(string roleId, string authType)
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

                    bool bAuthType = StringUtil.isEmpty(authType);


                    List<CodeRoleItemModel> rows = (from main in db.CODE_ROLE_ITEM
                                                    join item in db.TREA_ITEM on main.ITEM_ID equals item.ITEM_ID

                                                    join opType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ITEM_OP_TYPE") on item.ITEM_OP_TYPE equals opType.CODE into psOpType
                                                    from xOpType in psOpType.DefaultIfEmpty()
                                                    where main.ROLE_ID == roleId
                                                      & (bAuthType || main.AUTH_TYPE == authType)
                                                    select new CodeRoleItemModel
                                                    {
                                                        id = main.AUTH_TYPE + main.ITEM_ID,
                                                        itemId = main.ITEM_ID,
                                                        authType = main.AUTH_TYPE,
                                                        itemOpType = xOpType == null ? item.ITEM_OP_TYPE : xOpType.CODE_VALUE.Trim(),
                                                        itemDesc = item.ITEM_DESC.Trim(),
                                                        execAction = "",
                                                        execActionDesc = ""
                                                    }).ToList();

                    return rows;
                }
            }
        }


        //角色管理-查詢存取項目權限
        public List<CodeRoleItemModel> qryForRoleMgr(string roleId, string authType)
        {


            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {


                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    var rows = (from main in db.CODE_ROLE_ITEM

                                join d in db.TREA_ITEM on main.ITEM_ID equals d.ITEM_ID into psItem
                                from xItem in psItem.DefaultIfEmpty()

                                join opType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ITEM_OP_TYPE") on xItem.ITEM_OP_TYPE equals opType.CODE into psOpType
                                from xOpType in psOpType.DefaultIfEmpty()

                                where 1 == 1
                                    & main.ROLE_ID == roleId
                                    & main.AUTH_TYPE == authType

                                select new CodeRoleItemModel
                                {
                                    id = main.AUTH_TYPE + main.ITEM_ID,
                                    roleId = main.ROLE_ID,
                                    itemId = main.ITEM_ID,
                                    authType = main.AUTH_TYPE,
                                    itemOpType = xOpType.CODE,
                                    itemDesc = xItem.ITEM_DESC.Trim()


                                }).ToList<CodeRoleItemModel>();
                    return rows;

                }

                }

        }

        public CODE_ROLE_ITEM getRoleItemByKey(string roleId, string itemId, string authType)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbTreasuryEntities db = new dbTreasuryEntities())
                {
                    CODE_ROLE_ITEM roleItem = db.CODE_ROLE_ITEM.Where(x => x.ROLE_ID == roleId
                && x.ITEM_ID == itemId && x.AUTH_TYPE == authType).FirstOrDefault();

                    return roleItem;
                }
            }
        }



        /// <summary>
        /// 新增角色存取項目資料
        /// </summary>
        /// <param name="roleFunc"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Insert(CODE_ROLE_ITEM roleItem, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"insert into CODE_ROLE_ITEM
        (ROLE_ID, ITEM_ID, AUTH_TYPE, LAST_UPDATE_UID, LAST_UPDATE_DT)
        values (@ROLE_ID, @ITEM_ID, @AUTH_TYPE, @LAST_UPDATE_UID, @LAST_UPDATE_DT)
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleItem.ROLE_ID));
                command.Parameters.AddWithValue("@ITEM_ID", StringUtil.toString(roleItem.ITEM_ID));
                command.Parameters.AddWithValue("@AUTH_TYPE", StringUtil.toString(roleItem.AUTH_TYPE));
                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(roleItem.LAST_UPDATE_UID));
                command.Parameters.AddWithValue("@LAST_UPDATE_DT", DateTime.Now);

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /**
刪除角色存取項目資料檔
    **/
        public int Delete(CODE_ROLE_ITEM roleItem, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete CODE_ROLE_ITEM
        where 1=1
        and ROLE_ID = @ROLE_ID and ITEM_ID = @ITEM_ID and AUTH_TYPE = @AUTH_TYPE
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleItem.ROLE_ID));
                command.Parameters.AddWithValue("@ITEM_ID", StringUtil.toString(roleItem.ITEM_ID));
                command.Parameters.AddWithValue("@AUTH_TYPE", StringUtil.toString(roleItem.AUTH_TYPE));

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /**
將角色存取項目資料檔的各欄位組成一字串，for Log
    **/
        public String logContent(CODE_ROLE_ITEM roleItem)
        {
            String content = "";

            content += StringUtil.toString(roleItem.ROLE_ID) + '|';
            content += StringUtil.toString(roleItem.ITEM_ID) + '|';
            content += StringUtil.toString(roleItem.AUTH_TYPE) + '|';
            content += StringUtil.toString(roleItem.LAST_UPDATE_UID) + '|';
            content += roleItem.LAST_UPDATE_DT;

            return content;
        }
    }
}