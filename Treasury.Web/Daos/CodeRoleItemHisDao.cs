using Treasury.WebModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebUtils;
using Treasury.WebViewModels;
using Treasury.Web.ViewModels;
using System.Transactions;
using System.Data.Entity.SqlServer;

namespace Treasury.WebDaos
{
    public class CodeRoleItemHisDao
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
        public List<CodeRoleItemModel> qryForRoleMgrHis(dbTreasuryEntities db, string cRoleID, string apprStatus, string updDateB, string updDateE)
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


           
                var roleItemHis = (from m in db.CODE_ROLE_ITEM_HIS

                                   join item in db.TREA_ITEM on m.ITEM_ID equals item.ITEM_ID

                                   join appr in db.AUTH_APPR on m.APLY_NO equals appr.APLY_NO into psAppr
                                   from xAppr in psAppr.DefaultIfEmpty()

                                   join cType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ITEM_OP_TYPE") on item.ITEM_OP_TYPE equals cType.CODE into psCType
                                   from xType in psCType.DefaultIfEmpty()

                                   where m.ROLE_ID == cRoleID
                                     & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                                     & (bDateB || xAppr.CREATE_DT >= sB)
                                     & (bDateE || xAppr.CREATE_DT <= sE)
                                   select new CodeRoleItemModel
                                   {
                                       authType = m.AUTH_TYPE,
                                       aplyNo = m.APLY_NO.Trim(),
                                       apprUid = xAppr.APPR_UID.Trim(),
                                       apprStatus = xAppr.APPR_STATUS.Trim(),
                                       updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                                SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                                SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                                       updateUid = xAppr.CREATE_UID.Trim(),
                                       execAction = m.EXEC_ACTION.Trim(),

                                       itemOpType = xType == null ? item.ITEM_OP_TYPE : xType.CODE_VALUE.Trim(),
                                       itemId = m.ITEM_ID.Trim(),
                                       itemDesc = item.ITEM_DESC.Trim()


                                   }).ToList<CodeRoleItemModel>();

                return roleItemHis;
            
        }


        /// <summary>
        /// 依申請單號查詢角色作業項目資料異動檔
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<CodeRoleItemModel> qryByAplyNo(string aplyNo, string authType)
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


                    List<CodeRoleItemModel> rows = (from main in db.CODE_ROLE_ITEM_HIS
                                                    join item in db.TREA_ITEM on main.ITEM_ID equals item.ITEM_ID
                                                    join cType in db.SYS_CODE.Where(x => x.CODE_TYPE == "EXEC_ACTION") on main.EXEC_ACTION equals cType.CODE into psCType
                                                    from xType in psCType.DefaultIfEmpty()
                                                    join opType in db.SYS_CODE.Where(x => x.CODE_TYPE == "ITEM_OP_TYPE") on item.ITEM_OP_TYPE equals opType.CODE into psOpType
                                                    from xOpType in psOpType.DefaultIfEmpty()
                                                    where main.APLY_NO == aplyNo
                                                      & (bAuthType || main.AUTH_TYPE == authType)
                                                    select new CodeRoleItemModel
                                                    {
                                                        id = main.AUTH_TYPE + main.ITEM_ID,
                                                        itemId = main.ITEM_ID,
                                                        authType = main.AUTH_TYPE,
                                                        itemOpType = xOpType == null ? item.ITEM_OP_TYPE : xOpType.CODE_VALUE.Trim(),
                                                        itemDesc = item.ITEM_DESC.Trim(),
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
        /// <param name="codeRoleItemModel"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(string aplyNo, CodeRoleItemModel codeRoleItemModel, SqlConnection conn, SqlTransaction transaction)
        {
            try
            {
                string sql = @"
        INSERT INTO [CODE_ROLE_ITEM_HIS]
                   ([APLY_NO]
                   ,[ROLE_ID]
                   ,[ITEM_ID]
                   ,[AUTH_TYPE]
                   ,[EXEC_ACTION])
             VALUES
                   (@APLY_NO
                   ,@ROLE_ID
                   ,@ITEM_ID
                   ,@AUTH_TYPE
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(codeRoleItemModel.roleId));
                cmd.Parameters.AddWithValue("@ITEM_ID", StringUtil.toString(codeRoleItemModel.itemId));
                cmd.Parameters.AddWithValue("@AUTH_TYPE", StringUtil.toString(codeRoleItemModel.authType));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(codeRoleItemModel.execAction));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}