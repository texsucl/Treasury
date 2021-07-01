
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
    public class FGLSMPAHisDao
    {

        /// <summary>
        /// 查詢"科目樣本SQL會科對應檔"
        /// </summary>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        public List<OGL10182Model> qryForFGL10182(string smpNum, string productType, string corpNo, string acctType)
        {
            bool bsmpNum = StringUtil.isEmpty(smpNum);
            bool bproductType = StringUtil.isEmpty(productType);
            bool bcorpNo = StringUtil.isEmpty(corpNo);
            bool bacctType = StringUtil.isEmpty(acctType);


            List<OGL10182Model> rows = new List<OGL10182Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows = (from m in db.FGL_SMPA

                            join his in db.FGL_SMPA_HIS.Where(x => x.exec_action != "A" & (x.appr_stat == "1" || x.appr_stat == "0")) on new { m.smp_num, m.product_type, m.acct_type, m.corp_no }
                                    equals new { his.smp_num, his.product_type, his.acct_type, his.corp_no } into psHis
                            from xHis in psHis.DefaultIfEmpty()

                            //join aply in db.FGL_APLY_REC on xHis.APLY_NO equals aply.APLY_NO into psAply
                            //from xAply in psAply.DefaultIfEmpty()

                            //join aply in db.FGL_APLY_REC.Where(x => x.APPR_STAT == "1" || x.APPR_STAT == "") on xHis.APLY_NO equals aply.APLY_NO into psAply
                            //from xAply in psAply.DefaultIfEmpty()

                            join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                            from xItem in psItem.DefaultIfEmpty()

                            join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                            from xAcct in psAcct.DefaultIfEmpty()

                            join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                            from xStatus in psStatus.DefaultIfEmpty()

                            join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on xHis.exec_action equals codeAction.CODE into psAction
                            from xAction in psAction.DefaultIfEmpty()

                            where 1 == 1
                                & (xHis == null || (xHis != null && (xHis.appr_stat == "1" || (xHis.appr_stat == "0"))))
                               // &  ((xAply == null  && (xHis.APLY_NO == "" & xHis.EXEC_ACTION != "A")) || (xHis.APLY_NO != "" & xAply.APPR_STAT == "1"))
                                & (bsmpNum || m.smp_num == smpNum.Trim())
                                & (bproductType || m.product_type == productType.Trim())
                                & (bcorpNo || m.corp_no == corpNo.Trim())
                                & (bacctType || m.acct_type == acctType.Trim())

                            select new OGL10182Model()
                            {
                                tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type + "|" + m.corp_no,
                                smpNum = m.smp_num.Trim(),
                                productType = m.product_type.Trim(),
                                productTypeDesc = (xItem == null ? String.Empty : xItem.CODE_VALUE),
                                acctType = m.acct_type.Trim(),
                                acctTypeDesc = (xAcct == null ? String.Empty : xAcct.CODE_VALUE),
                                corpNo = m.corp_no.Trim(),

                                sqlActNum = (xHis == null ? m.sql_actnum: xHis.sql_actnum),
                                sqlActNm = (xHis == null ? m.sql_actnm : xHis.sql_actnm),

                                execAction = (xHis == null ? String.Empty : xHis.exec_action),
                                execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                dataStatus = m.data_status.Trim(),
                                dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE),
                                updateId = (xHis == null ? m.update_id : xHis.update_id),
                                updateDatetime = (xHis == null ? (m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.update_datetime).Trim())
                                                                         
                                                                         : (xHis.update_datetime == null ? "" : SqlFunctions.DateName("year", xHis.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", xHis.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", xHis.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", xHis.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", xHis.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", xHis.update_datetime).Trim())),


                            }).Distinct().OrderBy(d => d.tempId).ToList<OGL10182Model>()
                            
                            .Union
                            (from m in db.FGL_SMPA_HIS
                             join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                             from xItem in psItem.DefaultIfEmpty()

                             join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                             from xAcct in psAcct.DefaultIfEmpty()

                             join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                             from xAction in psAction.DefaultIfEmpty()

                             where 1 == 1
                             //   & m.APLY_NO == ""
                                & m.exec_action == "A"
                                & (m.appr_stat == "0" || m.appr_stat == "1")
                                 & (bsmpNum || m.smp_num == smpNum.Trim())
                                 & (bproductType || m.product_type == productType.Trim())
                                 & (bcorpNo || m.corp_no == corpNo.Trim())
                                 & (bacctType || m.acct_type == acctType.Trim())

                             select new OGL10182Model()
                             {
                                 tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type + "|" + m.corp_no,
                                 smpNum = m.smp_num.Trim(),
                                 productType = m.product_type.Trim(),
                                 productTypeDesc = (xItem == null ? String.Empty : xItem.CODE_VALUE),
                                 acctType = m.acct_type.Trim(),
                                 acctTypeDesc = (xAcct == null ? String.Empty : xAcct.CODE_VALUE),
                                 corpNo = m.corp_no.Trim(),
                                 sqlActNum = m.sql_actnum.Trim(),
                                 sqlActNm = m.sql_actnm.Trim(),
                                 execAction = m.exec_action,
                                 execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),

                                 dataStatus = m.aply_no == "" ? "1" : "2",
                                 dataStatusDesc = m.aply_no == "" ? "可異動" : "凍結中",
                                 updateId = m.update_id == null ? "" : m.update_id.Trim(),
                                 updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                          SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                          SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                          SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("s", m.update_datetime).Trim(),

                             }).Distinct().OrderBy(d => d.tempId).ToList<OGL10182Model>()

                            ;

                }
            }

            return rows;
        }

        public OGL10182Model qryByKey(string aplyNo, string smpNum, string productType, string corpNo, string acctType)
        {


            OGL10182Model d = new OGL10182Model();
            List<OGL10182Model> rows = new List<OGL10182Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows =
                            (from m in db.FGL_SMPA_HIS
                             join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                             from xItem in psItem.DefaultIfEmpty()

                             join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                             from xAcct in psAcct.DefaultIfEmpty()

                             join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                             from xAction in psAction.DefaultIfEmpty()

                             where 1 == 1
                                & m.aply_no == aplyNo
                                 & m.smp_num == smpNum.Trim()
                                 & m.product_type == productType.Trim()
                                 & m.corp_no == corpNo.Trim()
                                 & m.acct_type == acctType.Trim()

                             select new OGL10182Model()
                             {
                                 tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type + "|" + m.corp_no,
                                 smpNum = m.smp_num.Trim(),
                                 productType = m.product_type.Trim(),
                                 productTypeDesc = (xItem == null ? String.Empty : xItem.CODE_VALUE),
                                 acctType = m.acct_type.Trim(),
                                 acctTypeDesc = (xAcct == null ? String.Empty : xAcct.CODE_VALUE),
                                 corpNo = m.corp_no.Trim(),
                                 sqlActNum = m.sql_actnum.Trim(),
                                 sqlActNm = m.sql_actnm.Trim(),
                                 execAction = m.exec_action,
                                 execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                 dataStatus = "1",
                                 dataStatusDesc = "可異動",
                                 updateId = m.update_id == null ? "" : m.update_id.Trim(),
                                 updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                          SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                          SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                          SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                          SqlFunctions.DateName("s", m.update_datetime).Trim(),

                             }).ToList<OGL10182Model>();
                }
            }

            if (rows.Count > 0)
                d = rows[0];

            return d;
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
        public List<OGL10182Model> qryApprHis(string smpNum, string productType, string corpNo, string acctType, string apprDateB, string apprDateE, string apprStat)
        {
            bool bsmpNum = StringUtil.isEmpty(smpNum);
            bool bproductType = StringUtil.isEmpty(productType);
            bool bcorpNo = StringUtil.isEmpty(corpNo);
            bool bacctType = StringUtil.isEmpty(acctType);
            bool bapprStat = StringUtil.isEmpty(apprStat);
            bool bapprDateB = StringUtil.isEmpty(apprDateB);
            bool bapprDateE = StringUtil.isEmpty(apprDateE);
            DateTime sB = DateTime.Now;
            DateTime sE = DateTime.Now;

            if (!bapprDateB)
                sB = Convert.ToDateTime(apprDateB);

            if (!bapprDateE)
            {
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
                    var smpaHis = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_SMPA_HIS on appr.aply_no equals m.aply_no

                                   join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                                   from xItem in psItem.DefaultIfEmpty()

                                   join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                                   from xAcct in psAcct.DefaultIfEmpty()

                                   join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on appr.appr_stat equals codeStatus.CODE into psStatus
                                   from xStatus in psStatus.DefaultIfEmpty()

                                   join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where (appr.appr_stat == "1" || appr.appr_stat == "2" || appr.appr_stat == "3")
                                     & (bsmpNum || m.smp_num == smpNum)
                                   & (bproductType || m.product_type == productType)
                                   & (bcorpNo || m.corp_no == corpNo)
                                    & (bacctType || m.acct_type == acctType)
                                    & (bapprStat || appr.appr_stat == apprStat)
                                     & (bapprDateB || appr.approve_datetime >= sB)
                                     & (bapprDateE || appr.approve_datetime < sE)
                                   select new OGL10182Model
                                   {
                                       aplyNo = m.aply_no,
                                       smpNum = m.smp_num.Trim(),
                                       productType = m.product_type.Trim(),
                                       productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                       corpNo = m.corp_no.Trim(),
                                       acctType = m.acct_type.Trim(),
                                       acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                       sqlActNum = m.sql_actnum,
                                       sqlActNm = m.sql_actnm,
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
                                   }).OrderByDescending(x => x.aplyNo).ToList<OGL10182Model>();

                    return smpaHis;
                }
            }

        }

        /// <summary>
        /// 依覆核單號查歷史異動
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<OGL10182Model> qryByAplyNo(string aplyNo )
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
                    var smpaHis = (from appr in db.FGL_APLY_REC
                                   join m in db.FGL_SMPA_HIS on appr.aply_no equals m.aply_no

                                   join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                                   from xItem in psItem.DefaultIfEmpty()

                                   join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                                   from xAcct in psAcct.DefaultIfEmpty()

                                   //join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "APPR_STAT") on appr.APPR_STAT equals codeStatus.CODE into psStatus
                                   //from xStatus in psStatus.DefaultIfEmpty()

                                   join codeAction in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "EXEC_ACTION") on m.exec_action equals codeAction.CODE into psAction
                                   from xAction in psAction.DefaultIfEmpty()

                                   where appr.aply_no == aplyNo

                                   select new OGL10182Model
                                   {
                                       smpNum = m.smp_num.Trim(),
                                       productType = m.product_type.Trim(),
                                       productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                       acctType = m.acct_type.Trim(),
                                       acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                       corpNo = m.corp_no.Trim(),
                                       sqlActNum = m.sql_actnum.Trim(),
                                       sqlActNm = m.sql_actnm.Trim(),
                                       execAction = m.exec_action.Trim(),
                                       execActionDesc = (xAction == null ? String.Empty : xAction.CODE_VALUE),
                                       updateId = appr.create_id,
                                       updateDatetime = appr.create_dt == null ? "" : SqlFunctions.DateName("year", appr.create_dt) + "/" +
                                                                         SqlFunctions.DatePart("m", appr.create_dt) + "/" +
                                                                         SqlFunctions.DateName("day", appr.create_dt).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", appr.create_dt).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", appr.create_dt).Trim(),

                                   }).ToList<OGL10182Model>();

                    return smpaHis;
                }
            }
                    
        }


        /// <summary>
        /// 承辦人執行"申請覆核"
        /// </summary>
        /// <param name="smpaHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateAplyNo(FGL_SMPA_HIS smpaHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql =
@"UPDATE FGL_SMPA_HIS
   SET  APLY_NO = @APLY_NO
      , APPR_STAT = @APPR_STAT
      , UPDATE_ID = @UPDATE_ID
      , UPDATE_DATETIME = @UPDATE_DATETIME
 WHERE APLY_NO = ''
   AND SMP_NUM = @SMP_NUM
   AND PRODUCT_TYPE = @PRODUCT_TYPE
   AND CORP_NO = @CORP_NO
   AND ACCT_TYPE = @ACCT_TYPE";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(smpaHis.aply_no));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(smpaHis.appr_stat));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpaHis.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpaHis.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpaHis.acct_type));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpaHis.corp_no));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpaHis.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpaHis.update_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updateApprStat(string aplyNo, string apprStat, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql =
@"UPDATE FGL_SMPA_HIS
   SET  APPR_STAT = @APPR_STAT
 WHERE APLY_NO = @APLY_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(aplyNo));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(apprStat));

                int cnt = cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void updateByKey(FGL_SMPA_HIS smpaHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql =
@"UPDATE FGL_SMPA_HIS
   SET  EXEC_ACTION = @EXEC_ACTION
      , APPR_STAT = @APPR_STAT
      , SQL_ACTNUM = @SQL_ACTNUM
      , SQL_ACTNM = @SQL_ACTNM
      , UPDATE_ID = @UPDATE_ID
      , UPDATE_DATETIME = @UPDATE_DATETIME
 WHERE APLY_NO = @APLY_NO
   AND SMP_NUM = @SMP_NUM
   AND PRODUCT_TYPE = @PRODUCT_TYPE
   AND CORP_NO = @CORP_NO
   AND ACCT_TYPE = @ACCT_TYPE";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(smpaHis.aply_no));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(smpaHis.appr_stat));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(smpaHis.exec_action));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpaHis.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpaHis.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpaHis.acct_type));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpaHis.corp_no));
                cmd.Parameters.AddWithValue("@SQL_ACTNUM", StringUtil.toString(smpaHis.sql_actnum));
                cmd.Parameters.AddWithValue("@SQL_ACTNM", StringUtil.halfToFull(StringUtil.toString(smpaHis.sql_actnm)));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpaHis.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpaHis.update_datetime ?? System.DBNull.Value;




                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        /// <summary>
        /// 刪除畫面上的暫存資料
        /// </summary>
        /// <param name="smpNum"></param>
        /// <param name="productType"></param>
        /// <param name="corpNo"></param>
        /// <param name="acctType"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void delTmpFOr10182(string smpNum, string productType, string corpNo, string acctType
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                string sql =
@"DELETE FGL_SMPA_HIS
 WHERE APLY_NO = ''";

                if (!string.IsNullOrWhiteSpace(smpNum)) {
                    sql += " AND SMP_NUM = @SMP_NUM";
                    cmd.Parameters.AddWithValue("@SMP_NUM", smpNum);
                }

                if (!string.IsNullOrWhiteSpace(productType))
                {
                    sql += " AND PRODUCT_TYPE = @PRODUCT_TYPE";
                    cmd.Parameters.AddWithValue("@PRODUCT_TYPE", productType);
                }

                if (!string.IsNullOrWhiteSpace(corpNo))
                {
                    sql += " AND CORP_NO = @CORP_NO";
                    cmd.Parameters.AddWithValue("@CORP_NO", corpNo);
                }

                if (!string.IsNullOrWhiteSpace(acctType))
                {
                    sql += " AND @CCT_TYPE = @ACCT_TYPE";
                    cmd.Parameters.AddWithValue("@ACCT_TYPE", acctType);
                }

                cmd.CommandText = sql;

                int cnt = cmd.ExecuteNonQuery();

            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public void deleteByKey(FGL_SMPA_HIS smpaHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql =
@"DELETE FGL_SMPA_HIS
 WHERE APLY_NO = @APLY_NO
   AND SMP_NUM = @SMP_NUM
   AND PRODUCT_TYPE = @PRODUCT_TYPE
   AND CORP_NO = @CORP_NO
   AND ACCT_TYPE = @ACCT_TYPE";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(smpaHis.aply_no));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpaHis.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpaHis.product_type));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpaHis.corp_no));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpaHis.acct_type));

                

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
        /// <param name="smpaHis"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_SMPA_HIS smpaHis, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMPA_HIS
                   ([APLY_NO]
                   ,[EXEC_ACTION]
                   ,[APPR_STAT]
                   ,[SMP_NUM]
                   ,[PRODUCT_TYPE]
                   ,[ACCT_TYPE]
                   ,[CORP_NO]
                   ,[SQL_ACTNUM]
                   ,[SQL_ACTNM]
                   ,[UPDATE_ID]
                   ,[UPDATE_DATETIME])

             VALUES
                  (@APLY_NO
                   ,@EXEC_ACTION
                   ,@APPR_STAT
                   ,@SMP_NUM
                   ,@PRODUCT_TYPE
                   ,@ACCT_TYPE
                   ,@CORP_NO
                   ,@SQL_ACTNUM
                   ,@SQL_ACTNM
                   ,@UPDATE_ID
                   ,@UPDATE_DATETIME)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(smpaHis.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(smpaHis.exec_action));
                cmd.Parameters.AddWithValue("@APPR_STAT", StringUtil.toString(smpaHis.appr_stat));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpaHis.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpaHis.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpaHis.acct_type));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpaHis.corp_no));
                cmd.Parameters.AddWithValue("@SQL_ACTNUM", StringUtil.toString(smpaHis.sql_actnum));
                cmd.Parameters.AddWithValue("@SQL_ACTNM", StringUtil.halfToFull(StringUtil.toString(smpaHis.sql_actnm)));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpaHis.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpaHis.update_datetime ?? System.DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }
    }
}