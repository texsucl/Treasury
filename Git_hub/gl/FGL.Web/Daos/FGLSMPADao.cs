using FGL.Web.BO;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace FGL.Web.Daos
{
    public class FGLSMPADao
    {

        public OGL00003DModel qryForOGL00005(OGL00003DModel model)
        {
            OGL00003DModel d = new OGL00003DModel();

            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    d = (from m in db.FGL_SMPA
                         where 1 == 1
                             & m.smp_num == model.smpNum
                             & m.product_type == model.productType
                             & m.acct_type == model.acctType
                             & m.corp_no == model.corpNo

                         select new OGL00003DModel()
                         {
                             tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type + "|" + m.corp_no,
                             smpNum = m.smp_num.Trim(),
                             productType = m.product_type.Trim(),
                             acctType = m.acct_type.Trim(),
                             corpNo = m.corp_no.Trim(),
                             sqlSmpNum = m.sql_actnum.Trim(),
                             sqlSmpName = m.sql_actnm.Trim()
                         }).FirstOrDefault();

                }
            }

            if (d != null) {
                model.sqlSmpName = d.sqlSmpName;
                model.sqlSmpNum = d.sqlSmpNum;
            }


            return model;
        }



        public List<OGL10182Model> qryForOGL10182A(string aplyNo)
        {

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

                    rows = (from appr in db.FGL_APLY_REC
                            join his in db.FGL_SMPA_HIS on appr.aply_no equals his.aply_no
                            join m in db.FGL_SMPA on new { his.smp_num, his.product_type, his.acct_type, his.corp_no }
                                    equals new { m.smp_num, m.product_type, m.acct_type, m.corp_no }
                            join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                            from xItem in psItem.DefaultIfEmpty()

                            join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                            from xAcct in psAcct.DefaultIfEmpty()

                            join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                            from xStatus in psStatus.DefaultIfEmpty()

                            where 1 == 1
                                & appr.aply_no == aplyNo

                            select new OGL10182Model()
                            {
                                tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type + "|" + m.corp_no,
                                smpNum = m.smp_num.Trim(),
                                productType = m.product_type.Trim(),
                                productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                acctType = m.acct_type.Trim(),
                                acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                corpNo = m.corp_no.Trim(),
                                sqlActNum = m.sql_actnum.Trim(),
                                sqlActNm = m.sql_actnm.Trim()
                            }).Distinct().OrderBy(d => d.tempId).ToList<OGL10182Model>();

                }
            }

            return rows;
        }


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
                            join codeItem in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                            from xItem in psItem.DefaultIfEmpty()

                            join codeAcct in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                            from xAcct in psAcct.DefaultIfEmpty()

                            join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                            from xStatus in psStatus.DefaultIfEmpty()

                            where 1 == 1
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


                                dataStatus = m.data_status.Trim(),
                    dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE),
                    updateId = m.update_id == null ? "" : m.update_id.Trim(),
                    updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.update_datetime).Trim(),
                    
                            }).Distinct().OrderBy(d => d.tempId).ToList<OGL10182Model>();

                }
            }

            return rows;
        }




        /// <summary>
        /// 新增"科目樣本SQL會科對應檔"
        /// </summary>
        /// <param name="smpa"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_SMPA smpa, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMPA
                   ([SMP_NUM]
                   ,[PRODUCT_TYPE]
                   ,[ACCT_TYPE]
                   ,[CORP_NO]
                   ,[SQL_ACTNUM]
                   ,[SQL_ACTNM]
                   ,[DATA_STATUS]
                   ,[UPDATE_ID]
                   ,[UPDATE_DATETIME]
                   ,[APPR_ID]
                   ,[APPROVE_DATETIME])

             VALUES
                  (@SMP_NUM
                   ,@PRODUCT_TYPE
                   ,@ACCT_TYPE
                   ,@CORP_NO
                   ,@SQL_ACTNUM
                   ,@SQL_ACTNM
                   ,@DATA_STATUS
                   ,@UPDATE_ID
                   ,@UPDATE_DATETIME
                   ,@APPR_ID
                   ,@APPROVE_DATETIME)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpa.smp_num));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpa.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpa.acct_type));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpa.corp_no));
                cmd.Parameters.AddWithValue("@SQL_ACTNUM", StringUtil.toString(smpa.sql_actnum));
                cmd.Parameters.AddWithValue("@SQL_ACTNM", StringUtil.halfToFull(StringUtil.toString(smpa.sql_actnm)));

                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(smpa.data_status));
                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpa.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.update_datetime ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpa.appr_id));
                cmd.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.approve_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }





        /// <summary>
        /// 異動"科目樣本SQL會科對應檔"
        /// </summary>
        /// <param name="smpa"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int update(FGL_SMPA smpa, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update FGL_SMPA
        set SQL_ACTNUM = @SQL_ACTNUM
           ,SQL_ACTNM = @SQL_ACTNM
           ,DATA_STATUS = @DATA_STATUS
           ,UPDATE_ID = @UPDATE_ID
           ,UPDATE_DATETIME = @UPDATE_DATETIME
           ,APPR_ID = @APPR_ID
           ,APPROVE_DATETIME = @APPROVE_DATETIME
        where 1=1
        and SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE
        and CORP_NO = @CORP_NO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpa.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpa.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpa.acct_type));
                command.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpa.corp_no));
                command.Parameters.AddWithValue("@SQL_ACTNUM", StringUtil.toString(smpa.sql_actnum));
                command.Parameters.AddWithValue("@SQL_ACTNM", StringUtil.halfToFull(StringUtil.toString(smpa.sql_actnm)));

                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(smpa.data_status));
                
                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpa.update_id));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.update_datetime ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpa.appr_id));
                command.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.approve_datetime ?? System.DBNull.Value;



                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }




        /// <summary>
        /// 異動"科目樣本SQL會科對應檔"資料狀態
        /// </summary>
        /// <param name="smpa"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStatus(string dataStatus, FGL_SMPA smpa, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update FGL_SMPA
        set DATA_STATUS = @DATA_STATUS
           ,UPDATE_ID = @UPDATE_ID
           ,UPDATE_DATETIME = @UPDATE_DATETIME
           ,APPR_ID = @APPR_ID
           ,APPROVE_DATETIME = @APPROVE_DATETIME
        where 1=1
        and SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE
        and CORP_NO = @CORP_NO
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpa.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpa.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpa.acct_type));
                command.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpa.corp_no));

                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(dataStatus));

                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpa.update_id));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.update_datetime ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpa.appr_id));
                command.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpa.approve_datetime ?? System.DBNull.Value;



                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }




        /// <summary>
        /// 刪除"科目樣本SQL會科對應檔"
        /// </summary>
        /// <param name="smpa"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int delete (FGL_SMPA smpa, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete FGL_SMPA
        where SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE
        and CORP_NO = @CORP_NO";

            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpa.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpa.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpa.acct_type));
                command.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(smpa.corp_no));

                int cnt = command.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}