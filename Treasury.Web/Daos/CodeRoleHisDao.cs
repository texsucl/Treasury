using Treasury.WebBO;
using Treasury.WebModels;
using Treasury.WebViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.WebUtils;
using System.Transactions;
using Treasury.Web.ViewModels;
using System.Data.Entity.SqlServer;

namespace Treasury.WebDaos
{
    public class CodeRoleHisDao
    {


        /// <summary>
        /// 查歷史異動
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cRoleID"></param>
        /// <param name="apprStatus"></param>
        /// <param name="updDateB"></param>
        /// <param name="updDateE"></param>
        /// <returns></returns>
        public List<CodeRoleModel> qryForRoleMgrHis(dbTreasuryEntities db, string cRoleID, string apprStatus, string updDateB, string updDateE)
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


           
                //AuthReviewRoleModel roleHis = new AuthReviewRoleModel();
                var roleHis = (from m in db.CODE_ROLE_HIS

                               join appr in db.AUTH_APPR on m.APLY_NO equals appr.APLY_NO into psAppr
                               from xAppr in psAppr.DefaultIfEmpty()

                               where m.ROLE_ID == cRoleID
                                 & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                                 & (bDateB || xAppr.CREATE_DT >= sB)
                                 & (bDateE || xAppr.CREATE_DT <= sE)
                               select new CodeRoleModel
                               {
                                   aplyNo = m.APLY_NO.Trim(),
                                   updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                            SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                            SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                                   updateUid = xAppr.CREATE_UID.Trim(),
                                   execAction = m.EXEC_ACTION.Trim(),
                                   roleName = m.ROLE_NAME.Trim(),
                                   roleNameB = m.ROLE_NAME_B.Trim(),
                                   isDisabled = m.IS_DISABLED.Trim(),
                                   isDisabledB = m.IS_DISABLED_B.Trim(),
                                   memo = m.MEMO.Trim(),
                                   memoB = m.MEMO_B.Trim(),
                                   apprUid = xAppr.APPR_UID,
                                   apprStatus = xAppr.APPR_STATUS.Trim()

                               }).ToList<CodeRoleModel>();

                return roleHis;
            
        }


        public CODE_ROLE_HIS qryByAplyNo(String aplyNo)
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
                    CODE_ROLE_HIS codeRole = db.CODE_ROLE_HIS.Where(x => x.APLY_NO == aplyNo).FirstOrDefault<CODE_ROLE_HIS>();

                    return codeRole;
                }
            }
        }


        


        //        /// <summary>
        //        /// 以"覆核單號"為鍵項查詢
        //        /// </summary>
        //        /// <param name="cReviewSeq"></param>
        //        /// <returns></returns>
        //        public CodeRoleHis qryByKey(String cReviewSeq)
        //        {

        //            using (DbAccountEntities db = new DbAccountEntities())
        //            {
        //                CodeRoleHis codeRoleHis = db.CodeRoleHis.Where(x => x.cReviewSeq == cReviewSeq).FirstOrDefault<CodeRoleHis>();

        //                return codeRoleHis;
        //            }
        //        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="codeRoleHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(CODE_ROLE_HIS codeRoleHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
        INSERT INTO [CODE_ROLE_HIS]
                   ([APLY_NO]
                   ,[ROLE_ID]
                   ,[ROLE_NAME]
                   ,[ROLE_AUTH_TYPE]
                   ,[IS_DISABLED]
                   ,[MEMO]
                   ,[ROLE_NAME_B]
                   ,[IS_DISABLED_B]
                   ,[MEMO_B]
                   ,[EXEC_ACTION])
             VALUES
                   (@APLY_NO
                   ,@ROLE_ID
                   ,@ROLE_NAME
                   ,@ROLE_AUTH_TYPE
                   ,@IS_DISABLED
                   ,@MEMO
                   ,@ROLE_NAME_B
                   ,@IS_DISABLED_B
                   ,@MEMO_B
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(codeRoleHis.APLY_NO));
                cmd.Parameters.AddWithValue("@ROLE_ID", StringUtil.toString(codeRoleHis.ROLE_ID));
                cmd.Parameters.AddWithValue("@ROLE_NAME", StringUtil.toString(codeRoleHis.ROLE_NAME));
                cmd.Parameters.AddWithValue("@ROLE_AUTH_TYPE", StringUtil.toString(codeRoleHis.ROLE_AUTH_TYPE));
                cmd.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(codeRoleHis.IS_DISABLED));
                cmd.Parameters.AddWithValue("@MEMO", StringUtil.toString(codeRoleHis.MEMO));
                cmd.Parameters.AddWithValue("@ROLE_NAME_B", StringUtil.toString(codeRoleHis.ROLE_NAME_B));
                cmd.Parameters.AddWithValue("@IS_DISABLED_B", StringUtil.toString(codeRoleHis.IS_DISABLED_B));
                cmd.Parameters.AddWithValue("@MEMO_B", StringUtil.toString(codeRoleHis.MEMO_B));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(codeRoleHis.EXEC_ACTION));

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }


        }
    }
}