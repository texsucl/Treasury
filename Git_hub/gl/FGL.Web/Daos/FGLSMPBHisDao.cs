
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;

namespace FGL.Web.Daos
{
    public class FGLSMPBHisDao
    {
        public void insertFromFormal(string aplyNo, OGL10181Model his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMPB_HIS
                   ([APLY_NO]
      ,[EXEC_ACTION]
      ,[SMP_NUM]
      ,[SMP_NAME]
      ,[PRODUCT_TYPE]
      ,[ACCT_TYPE])
(SELECT @APLY_NO
      ,''
      ,[SMP_NUM]
      ,[SMP_NAME]
      ,[PRODUCT_TYPE]
      ,[ACCT_TYPE]
FROM FGL_SMPB
WHERE SMP_NUM = @SMP_NUM)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smpNum));

                int cnt = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public OGL10181Model qryByKey(string aplyNo, OGL10181Model d)
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
                    var his = (from appr in db.FGL_APLY_REC
                               join m in db.FGL_SMPB_HIS on appr.aply_no equals m.aply_no

                               where appr.aply_no == aplyNo
                               & m.smp_num == d.smpNum
                               & m.product_type == d.productType
                               & m.acct_type == d.acctType
                               select new OGL10181Model
                               {
                                   tempId = m.smp_num.Trim() + "|"
                                            + m.product_type.Trim() + "|"
                                            + m.acct_type.Trim(),
                                   smpNum = m.smp_num.Trim(),
                                   productType = m.product_type.Trim(),
                                   acctType = m.acct_type.Trim(),
                                   execAction = m.exec_action

                               }).FirstOrDefault();

                    return his;
                }
            }

        }


        /// <summary>
        /// 查詢暫存檔特定狀態的筆數
        /// </summary>
        /// <param name="apprStat"></param>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        public int qryByProductCnt(string apprStat, string smpNum)
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
                    var his = (from appr in db.FGL_APLY_REC
                               join m in db.FGL_SMPB_HIS on appr.aply_no equals m.aply_no
                               where m.smp_num == smpNum
                               & appr.appr_stat == apprStat
                                 & (appr.appr_stat == "0")
                               select new OGL10181Model
                               {
                                   smpNum = m.smp_num.Trim(),
                                   smpName = m.smp_name.Trim(),
                                   productType = m.product_type.Trim(),
                                   acctType = m.acct_type.Trim()
                               }).Count();

                    return his;
                }
            }
        }

        public List<OGL10181Model> qryBySmpNum(string smpNum)
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
                    var his = (from appr in db.FGL_APLY_REC
                               join m in db.FGL_SMPB_HIS on appr.aply_no equals m.aply_no

                               join formal in db.FGL_SMPB on new { m.smp_num, m.product_type, m.acct_type } 
                                    equals new { formal.smp_num, formal.product_type, formal.acct_type }
                                into psFormal
                               from xFormal in psFormal.DefaultIfEmpty()

                               join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on xFormal.data_status equals codeStatus.CODE into psStatus
                               from xStatus in psStatus.DefaultIfEmpty()


                               where m.smp_num == smpNum
                               & (appr.appr_stat == "0" || appr.appr_stat == "1")
                               select new OGL10181Model
                               {
                                   tempId = m.smp_num.Trim() + "|"
                                            + m.product_type.Trim() + "|"
                                            + m.acct_type.Trim(),
                                   aplyNo = m.aply_no,
                                   smpNum = m.smp_num,
                                   smpName = m.smp_name,
                                   productType = m.product_type,
                                   acctType = m.acct_type,
                                   execAction = m.exec_action,
                                   dataStatus = appr.appr_stat == "1" ? "2" : (xFormal == null ? "1" : xFormal.data_status),
                                   dataStatusDesc = appr.appr_stat == "1" ? "凍結中" : (xStatus == null ? "可異動" : xStatus.CODE_VALUE),
                                   updateId = m.exec_action == "" ? xFormal.update_id : appr.create_id,
                                   updateDatetime = m.exec_action == "" ? (xFormal.update_datetime == null ? "" :
                                                                        (SqlFunctions.DateName("year", xFormal.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", xFormal.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", xFormal.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", xFormal.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", xFormal.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", xFormal.update_datetime).Trim())) :

                                                                         (appr.create_dt == null ? "" :(SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim()))
                               }).Distinct().ToList<OGL10181Model>();

                    return his;
                }
            }

        }


        /// <summary>
        /// 查歷史異動
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<OGL10181Model> qryByAplyNo(string aplyNo )
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
                    var smpbHis = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_SMPB_HIS on appr.aply_no equals m.aply_no

                                   join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                                   from xItem in psItem.DefaultIfEmpty()

                                   join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                                   from xAcct in psAcct.DefaultIfEmpty()


                                   join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where appr.aply_no == aplyNo

                                   select new OGL10181Model
                                   {
                                       smpNum = m.smp_num.Trim(),
                                       smpName = m.smp_name.Trim(),
                                       productType = m.product_type.Trim(),
                                       productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                       acctType = m.acct_type.Trim(),
                                       acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                       execAction = m.exec_action.Trim(),
                                       execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                       updateId = appr.create_id,
                                       updateDatetime = appr.create_dt == null ? "" : SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim(),

                                   }).ToList<OGL10181Model>();

                    return smpbHis;
                }
            }
                    
        }



        /// <summary>
        /// 查詢歷史紀錄
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="productType"></param>
        /// <param name="acctType"></param>
        /// <param name="apprDateB"></param>
        /// <param name="apprDateE"></param>
        /// <returns></returns>
        public List<OGL10181Model> qryApprHis(string smpNum, string productType, string acctType, string apprDateB, string apprDateE, string apprStat)
        {
            bool bproductType = StringUtil.isEmpty(productType);
            bool bacctType = StringUtil.isEmpty(acctType);
            bool bapprStat = StringUtil.isEmpty(apprStat);
            bool bapprDateB = StringUtil.isEmpty(apprDateB);
            bool bapprDateE = StringUtil.isEmpty(apprDateE);
            DateTime sB = DateTime.Now;
            DateTime sE = DateTime.Now;

            if(!bapprDateB)
                sB = Convert.ToDateTime(apprDateB);

            if (!bapprDateE) {
                sE = Convert.ToDateTime(apprDateE);
                sE = sE.AddDays(1);
            }
 

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var smpbHis = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_SMPB_HIS on appr.aply_no equals m.aply_no

                                   join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                                   from xItem in psItem.DefaultIfEmpty()

                                   join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                                   from xAcct in psAcct.DefaultIfEmpty()

                                   join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on appr.appr_stat equals codeStatus.CODE into psStatus
                                   from xStatus in psStatus.DefaultIfEmpty()

                                   join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where (appr.appr_stat == "1" || appr.appr_stat == "2" || appr.appr_stat == "3")
                                    & m.smp_num == smpNum
                                   & (bproductType || m.product_type == productType)
                                    & (bacctType || m.acct_type == acctType)
                                    & (bapprStat || appr.appr_stat == apprStat)
                                     & (bapprDateB || appr.approve_datetime >= sB)
                                     & (bapprDateE || appr.approve_datetime < sE)
                                   select new OGL10181Model
                                   {
                                       aplyNo = m.aply_no,
                                       smpNum = m.smp_num.Trim(),
                                       smpName = m.smp_name.Trim(),
                                       productType = m.product_type.Trim(),
                                       productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                       acctType = m.acct_type.Trim(),
                                       acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                       apprStat = (xStatus == null ? appr.appr_stat : xStatus.CODE_VALUE),
                                       execAction = m.exec_action.Trim(),
                                       execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                       updateId = appr.create_id,
                                       updateDatetime = appr.create_dt == null ? "" : SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim(),
                                       apprId = appr.appr_id,
                                       apprDt = appr.approve_datetime == null ? "" : SqlFunctions.DateName("year", appr.approve_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.approve_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", appr.approve_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.approve_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.approve_datetime).Trim(),
                                   }).OrderByDescending(x => x.aplyNo).ToList<OGL10181Model>();

                    return smpbHis;
                }
            }

        }




        public void updateByKey(FGL_SMPB_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"UPDATE FGL_SMPB_HIS
SET EXEC_ACTION = @EXEC_ACTION
   ,SMP_NAME = @SMP_NAME
WHERE APLY_NO = @APLY_NO
  AND SMP_NUM = @SMP_NUM
  AND PRODUCT_TYPE = @PRODUCT_TYPE
  AND ACCT_TYPE = @ACCT_TYPE
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(his.exec_action));
                cmd.Parameters.AddWithValue("@SMP_NAME", StringUtil.halfToFull(StringUtil.toString(his.smp_name)));

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(his.acct_type));


                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }

        }



        /// <summary>
        /// 申請覆核時，將暫存檔內未異動的資料清空，僅留下有異動的資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delNoChangeByAply(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"DELETE FGL_SMPB_HIS
WHERE APLY_NO = @APLY_NO
  AND EXEC_ACTION = ''
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        /// <summary>
        /// 依覆核單號刪除暫存資料(畫面執行"刪除暫存資料")
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delByAplyNo(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"DELETE FGL_SMPB_HIS
WHERE APLY_NO = @APLY_NO
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }

        }


        public void deleteByKey(FGL_SMPB_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"DELETE FGL_SMPB_HIS
WHERE APLY_NO = @APLY_NO
  AND SMP_NUM = @SMP_NUM
  AND PRODUCT_TYPE = @PRODUCT_TYPE
  AND ACCT_TYPE = @ACCT_TYPE
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(his.acct_type));


                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="smpbHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_SMPB_HIS smpbHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMPB_HIS
                   ([APLY_NO]
                   ,[EXEC_ACTION]
                   ,[SMP_NUM]
                   ,[SMP_NAME]
                   ,[PRODUCT_TYPE]
                   ,[ACCT_TYPE])

             VALUES
                  (@APLY_NO
                   ,@EXEC_ACTION
                   ,@SMP_NUM
                   ,@SMP_NAME
                   ,@PRODUCT_TYPE
                   ,@ACCT_TYPE)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(smpbHis.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(smpbHis.exec_action));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpbHis.smp_num));
                cmd.Parameters.AddWithValue("@SMP_NAME", StringUtil.halfToFull(StringUtil.toString(smpbHis.smp_name)));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpbHis.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpbHis.acct_type));


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }
    }
}