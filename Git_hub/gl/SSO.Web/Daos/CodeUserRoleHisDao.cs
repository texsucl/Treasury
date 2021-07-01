
using SSO.Web.Models;
using SSO.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Data.Entity.SqlServer;
using SSO.Web.Utils;

namespace SSO.Web.Daos
{
    public class CodeUserRoleHisDao
    {
        /// <summary>
        /// 查歷史異動
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="apprStatus"></param>
        /// <param name="updDateB"></param>
        /// <param name="updDateE"></param>
        /// <returns></returns>
        public List<UserRoleHisModel> qryForUserMgrHis(dbFGLEntities db, string userId, string apprStatus, string updDateB, string updDateE)
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


            var userRoleHis = (from m in db.CODE_USER_ROLE_HIS

                           join appr in db.SSO_APLY_REC on m.APLY_NO equals appr.APLY_NO into psAppr
                           from xAppr in psAppr.DefaultIfEmpty()

                           join role in db.CODE_ROLE on m.ROLE_ID equals role.ROLE_ID 

                           where m.USER_ID == userId
                             & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                             & (bDateB || xAppr.CREATE_DT >= sB)
                             & (bDateE || xAppr.CREATE_DT <= sE)
                           select new UserRoleHisModel
                           {
                               aplyNo = m.APLY_NO.Trim(),
                               updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                        SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                        SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                               updateUid = xAppr.CREATE_UID.Trim(),
                               execAction = m.EXEC_ACTION.Trim(),
                               cRoleID = m.ROLE_ID.Trim(),
                               cRoleName = role.ROLE_NAME.Trim(),
                               apprStatus = xAppr.APPR_STATUS.Trim()
                               

                           }).ToList<UserRoleHisModel>();

            return userRoleHis;

        }


        public List<CodeUserRoleModel> qryByAplyNo(string aplyNo)
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


                    List<CodeUserRoleModel> rows = (from main in db.CODE_USER_ROLE_HIS
                                                    join role in db.CODE_ROLE on main.ROLE_ID equals role.ROLE_ID

                                                    join cType in db.SYS_CODE.Where(x => x.CODE_TYPE == "EXEC_ACTION" & x.SYS_CD == "SSO") on main.EXEC_ACTION equals cType.CODE into psCType
                                                    from xType in psCType.DefaultIfEmpty()


                                                    where main.APLY_NO == aplyNo

                                                    select new CodeUserRoleModel
                                                    {
                                                        roleId = main.ROLE_ID,
                                                        roleName = role.ROLE_NAME.Trim(),
                                                        roleAuthUnit = role.AUTH_UNIT,
                                                        execAction = main.EXEC_ACTION.Trim(),
                                                        execActionDesc = xType == null ? "" : xType.CODE_VALUE.Trim()
                                                    }).ToList();

                    return rows;
                }
            }
        }

        




        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="codeUserRoleHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aplyNo, CodeUserRoleModel codeUserRoleModel, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"INSERT INTO CODE_USER_ROLE_HIS
                   ([APLY_NO]
                   ,[USER_ID]
                   ,[ROLE_ID]
                   ,[EXEC_ACTION])

             VALUES
                  (@APLY_NO
                   ,@USER_ID
                   ,@ROLE_ID
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);
                cmd.Parameters.AddWithValue("@USER_ID", StringUtil.toString(codeUserRoleModel.userId));
                cmd.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(codeUserRoleModel.roleId));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(codeUserRoleModel.execAction));



                int cnt = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}