
using SSO.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SSO.Web.Models;
using System.Transactions;
using SSO.Web.Utils;

namespace SSO.Web.Daos
{
    public class CodeRoleFunctionDao
    {
        //角色管理-查詢存取項目權限
        public List<FuncRoleModel> qryForRoleMgr(string roleId)
        {


            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {


                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var rows = (from func in db.CODE_FUNC
                                join role in db.CODE_ROLE_FUNC on func.FUNC_ID equals role.FUNC_ID
                                join sys in db.CODE_SYS_INFO on func.SYS_CD equals sys.SYS_CD


                                where 1 == 1
                                & func.IS_DISABLED == "N"
                                & role.ROLE_ID == roleId
                                orderby (func.FUNC_LEVEL == 1 ? func.FUNC_ID : func.PARENT_FUNC_ID), func.PARENT_FUNC_ID, func.FUNC_ORDER

                                select new FuncRoleModel
                                {
                                    sysCd = func.SYS_CD,
                                    sysCdDesc = sys.SYS_NAME,
                                    cFunctionID = func.FUNC_ID,
                                    cFunctionName = func.FUNC_NAME,
                                    iFunctionLevel = func.FUNC_LEVEL,
                                    cParentFunctionID = func.PARENT_FUNC_ID
                                }).ToList<FuncRoleModel>();
                    return rows;

                }

            }

        }


        public CODE_ROLE_FUNC getFuncRoleByKey(string roleId, string cFunctionID)
        {
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
            {
                    CODE_ROLE_FUNC roleFunc = db.CODE_ROLE_FUNC.Where(x => x.ROLE_ID == roleId
                && x.FUNC_ID == cFunctionID).FirstOrDefault();

                return roleFunc;
            }
        }
        }



        /// <summary>
        /// 角色功能報表清單
        /// </summary>
        /// <returns></returns>
        public List<FuncRoleModel> qryFuncRole()
        {

            List<FuncRoleModel> rows = new List<FuncRoleModel>();
            using (new TransactionScope(
                   TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted
                   }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
            {

                rows = (from r in db.CODE_ROLE
                        join rf in db.CODE_ROLE_FUNC on r.ROLE_ID equals rf.ROLE_ID
                        join f in db.CODE_FUNC on rf.FUNC_ID equals f.FUNC_ID

                        join fParent in db.CODE_FUNC on f.PARENT_FUNC_ID equals fParent.FUNC_ID into psParent
                        from xParent in psParent.DefaultIfEmpty()

                        where 1 == 1
                            & r.IS_DISABLED == "N"
                            & f.IS_DISABLED == "N"
                        select new FuncRoleModel()
                        {
                            cRoleId = r.ROLE_ID.Trim(),
                            cRoleName = r.ROLE_NAME.Trim(),
                            vMemo = r.MEMO.Trim(),
                            cFunctionID = f.FUNC_ID.Trim(),
                            cFunctionName = f.FUNC_NAME.Trim(),
                            iFunctionLevel = f.FUNC_LEVEL,
                            cParentFunctionID = f.PARENT_FUNC_ID.Trim(),
                            cParentFunctionName = xParent == null ? "" : xParent.FUNC_NAME.Trim(),
                            iParentFunctionLevel = xParent.FUNC_LEVEL
                        }).Distinct()
                        .OrderBy(d => d.cRoleId)
                        .ToList<FuncRoleModel>();



                return rows;
            }
        }
        }





        /**
       刪除角色功能資料檔
           **/
        public int Delete(CODE_ROLE_FUNC roleFunc, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete CODE_ROLE_FUNC
        where 1=1
        and ROLE_ID = @ROLE_ID and FUNC_ID = @FUNC_ID
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleFunc.ROLE_ID));
                command.Parameters.AddWithValue("@FUNC_ID", StringUtil.toString(roleFunc.FUNC_ID));

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /**
       新增角色功能資料檔
           **/
        public int Insert(CODE_ROLE_FUNC roleFunc, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"insert into CODE_ROLE_FUNC
        (ROLE_ID, FUNC_ID, LAST_UPDATE_UID, LAST_UPDATE_DT)
        values (@ROLE_ID, @FUNC_ID, @LAST_UPDATE_UID, @LAST_UPDATE_DT)
        ";
            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(roleFunc.ROLE_ID));
                command.Parameters.AddWithValue("@FUNC_ID", StringUtil.toString(roleFunc.FUNC_ID));
                command.Parameters.AddWithValue("@LAST_UPDATE_UID", StringUtil.toString(roleFunc.LAST_UPDATE_UID));
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
        將角色功能資料檔的各欄位組成一字串，for Log
            **/
        public String logContent(CODE_ROLE_FUNC roleFun)
        {
            String content = "";

            content += StringUtil.toString(roleFun.ROLE_ID) + '|';
            content += StringUtil.toString(roleFun.FUNC_ID) + '|';
            content += StringUtil.toString(roleFun.LAST_UPDATE_UID) + '|';
            content += roleFun.LAST_UPDATE_DT;

            return content;
        }
    }
}