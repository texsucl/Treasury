using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SSO.Web.Models;
using SSO.Web.Utils;
using SSO.Web.ViewModels;
using System.Transactions;
using System.Data.Entity.SqlServer;

namespace SSO.Web.Daos
{
    public class CodeRoleFuncHisDao
    {
        /// <summary>
        /// 查詢修改前後的資料
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cRoleID"></param>
        /// <param name="apprStatus"></param>
        /// <param name="updDateB"></param>
        /// <param name="updDateE"></param>
        /// <returns></returns>
        public List<RoleFuncHisModel> qryForRoleMgrHis(dbFGLEntities db, string cRoleID, string apprStatus, string updDateB, string updDateE)
        {
            bool bapprStatus = StringUtil.isEmpty(apprStatus);
            bool bDateB = StringUtil.isEmpty(updDateB);
            bool bDateE = StringUtil.isEmpty(updDateE);

            DateTime sB = DateTime.Now.AddDays(1);
            if (!bDateB)
            {
                sB = Convert.ToDateTime(updDateB);
            }
            DateTime sE = DateTime.Now.AddDays(1);
            if (!bDateE)
            {
                sE = Convert.ToDateTime(updDateE);
            }
            sE = sE.AddDays(1);


         
                var roleFuncHis = (from m in db.CODE_ROLE_FUNC_HIS

                               join appr in db.SSO_APLY_REC on m.APLY_NO equals appr.APLY_NO into psAppr
                               from xAppr in psAppr.DefaultIfEmpty()

                               join func in db.CODE_FUNC on m.FUNC_ID equals func.FUNC_ID into psFunc
                               from xFunc in psFunc.DefaultIfEmpty()

                               where m.ROLE_ID == cRoleID
                                 & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                                 & (bDateB || xAppr.CREATE_DT >= sB)
                                 & (bDateE || xAppr.CREATE_DT <= sE)
                               select new RoleFuncHisModel
                               {
                                   aplyNo = m.APLY_NO.Trim(),
                                   apprStatus = xAppr.APPR_STATUS.Trim(),
                                   updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                            SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                            SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                                   updateUid = xAppr.CREATE_UID.Trim(),
                                   execAction = m.EXEC_ACTION.Trim(),
                                   cFunctionID = m.FUNC_ID.Trim(),
                                   cFunctionName = xFunc.FUNC_NAME.Trim()
                                   

                               }).ToList<RoleFuncHisModel>();

                return roleFuncHis;
         
        }



        /// <summary>
        /// 依申請單號查詢角色功能資料異動檔
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<RoleFuncHisModel> qryByAplyNo(String aplyNo)
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
                    List<RoleFuncHisModel> rows = (from main in db.CODE_ROLE_FUNC_HIS
                                                        join func in db.CODE_FUNC on main.FUNC_ID equals func.FUNC_ID
                                                        join cType in db.SYS_CODE.Where(x => x.CODE_TYPE == "EXEC_ACTION" & x.SYS_CD == "SSO") on main.EXEC_ACTION equals cType.CODE into psCType
                                                        from xType in psCType.DefaultIfEmpty()

                                                        where main.APLY_NO == aplyNo
                                                   select new RoleFuncHisModel
                                                   {
                                                            aplyNo = main.APLY_NO.Trim(),
                                                            cFunctionID = main.FUNC_ID.Trim(),
                                                            cFunctionName = func.FUNC_NAME.Trim(),
                                                            execAction = main.EXEC_ACTION,
                                                            execActionDesc = xType == null ? "" : xType.CODE_VALUE.Trim()
                                                        }).ToList();

                    return rows;
                }
            }
        }





        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="codeRoleFuncHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aplyNo, FuncRoleModel funcRoleModel, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                string sql = @"
        INSERT INTO [CODE_ROLE_FUNC_HIS]
                   ([APLY_NO]
                   ,[ROLE_ID]
                   ,[FUNC_ID]
                   ,[EXEC_ACTION])
             VALUES
                   (@APLY_NO
                   ,@ROLE_ID
                   ,@FUNC_ID
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(funcRoleModel.cRoleId));
                cmd.Parameters.AddWithValue("@FUNC_ID", StringUtil.toString(funcRoleModel.cFunctionID));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(funcRoleModel.execAction));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}