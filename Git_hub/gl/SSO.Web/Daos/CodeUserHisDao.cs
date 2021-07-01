
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using SSO.Web.Models;
using SSO.Web.Utils;
using System.Transactions;

using System.Data.Entity.SqlServer;
using SSO.Web.ViewModels;

namespace SSO.Web.Daos
{
    public class CodeUserHisDao
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
        public List<CodeUserHisModel> qryForUserMgrHis(dbFGLEntities db, string userId, string apprStatus, string updDateB, string updDateE)
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


            var userHis = (from m in db.CODE_USER_HIS

                           join appr in db.SSO_APLY_REC on m.APLY_NO equals appr.APLY_NO into psAppr
                           from xAppr in psAppr.DefaultIfEmpty()

                           where m.USER_ID == userId
                             & (bapprStatus || xAppr.APPR_STATUS == apprStatus.Trim())
                             & (bDateB || xAppr.CREATE_DT >= sB)
                             & (bDateE || xAppr.CREATE_DT <= sE)
                           select new CodeUserHisModel
                           {
                               aplyNo = m.APLY_NO.Trim(),
                               updateDT = SqlFunctions.DateName("year", xAppr.CREATE_DT) + "/" +
                                                        SqlFunctions.DatePart("m", xAppr.CREATE_DT) + "/" +
                                                        SqlFunctions.DateName("day", xAppr.CREATE_DT).Trim(),
                               updateUid = xAppr.CREATE_UID.Trim(),
                               execAction = m.EXEC_ACTION.Trim(),

                               isDisabled = m.IS_DISABLED.Trim(),
                               isDisabledB = m.IS_DISABLED_B.Trim(),
                               isMail = m.IS_MAIL.Trim(),
                               isMailB = m.IS_MAIL_B.Trim(),
                               apprStatus = xAppr.APPR_STATUS.Trim()

                           }).ToList<CodeUserHisModel>();

            return userHis;

        }


        /// <summary>
        /// 以申請單號查詢使用者異動資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public CODE_USER_HIS qryByAplyNo(String aplyNo)
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
                    CODE_USER_HIS user = db.CODE_USER_HIS.Where(x => x.APLY_NO == aplyNo).FirstOrDefault<CODE_USER_HIS>();

                    return user;
                }
            }
        }


        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="codeUserHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(CODE_USER_HIS codeUserHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO CODE_USER_HIS
                   ([APLY_NO]
                   ,[USER_ID]
                   ,[IS_DISABLED]
                   ,[IS_DISABLED_B]
                   ,[IS_MAIL]
                   ,[IS_MAIL_B]
                   ,[EXEC_ACTION])

             VALUES
                  (@APLY_NO
                   ,@USER_ID
                   ,@IS_DISABLED
                   ,@IS_DISABLED_B
                   ,@IS_MAIL
                   ,@IS_MAIL_B
                   ,@EXEC_ACTION)
        ";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(codeUserHis.APLY_NO));
                cmd.Parameters.AddWithValue("@USER_ID", StringUtil.toString(codeUserHis.USER_ID));
                cmd.Parameters.AddWithValue("@IS_DISABLED", StringUtil.toString(codeUserHis.IS_DISABLED));
                cmd.Parameters.AddWithValue("@IS_DISABLED_B", StringUtil.toString(codeUserHis.IS_DISABLED_B));
                cmd.Parameters.AddWithValue("@IS_MAIL", StringUtil.toString(codeUserHis.IS_MAIL));
                cmd.Parameters.AddWithValue("@IS_MAIL_B", StringUtil.toString(codeUserHis.IS_MAIL_B));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(codeUserHis.EXEC_ACTION));


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }
    }
}