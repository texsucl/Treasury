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
    public class FGLSMPBDao
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public FGL_SMPB qryByKey(string smpNum, string productType, string acctType) {
            FGL_SMPB smpb = new FGL_SMPB();

            using (dbFGLEntities db = new dbFGLEntities())
            {
                try
                {
                    smpb = db.FGL_SMPB.Where(
                           x => x.smp_num == smpNum
                           & x.product_type == productType
                           & x.acct_type == acctType).FirstOrDefault<FGL_SMPB>();
                }
                catch (Exception e)
                {
                    logger.Error(e.ToString());
                }
            }
            return smpb;
        }


        public Dictionary<string, OGL00003DModel> qryForOGL00005(Dictionary<string, OGL00003DModel> smpMap, string itemName)
        {
            using (dbFGLEntities db = new dbFGLEntities())
            {
                try
                {
                    foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                    {
                        FGL_SMPB smpb = db.FGL_SMPB.Where(
                            x => x.smp_num == item.Value.smpNum
                            & x.product_type == item.Value.productType
                            & x.acct_type == item.Value.acctType).FirstOrDefault<FGL_SMPB>();

                        if (smpb != null) {
                            int smpNameCnt = Convert.ToInt16(item.Value.smpNameCnt);
                            item.Value.smpName = StringUtil.toString(smpb.smp_name).PadRight(smpNameCnt, ' ').Substring(0, smpNameCnt).Trim()
                                + "－" + itemName.PadRight(20, ' ').Substring(0, 20).Trim();
                        }
                    }
                }
                catch (Exception e) {
                    logger.Error(e.ToString());
                }

                return smpMap;
            }
        }




        /// <summary>
        /// 查詢"商品精算"、"保單投資"可維護的會計科目
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <param name="acctTypeKind"></param>
        /// <returns></returns>
        public List<OGL10181Model> qryForItem(string productType, string fuMk, string itemCon, string discPartFeat
            , string[] acctTypeKind)
        {


            List<OGL10181Model> rows = new List<OGL10181Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows = (from m in db.FGL_SMPB
                            join item in db.FGL_ITEM_ACCT on 
                                new { m.product_type, m.acct_type, m.smp_num }  equals new { item.product_type, item.acct_type, item.smp_num} into psItem
                            from xItem in psItem

                            where 1 == 1
                                & m.product_type == productType
                                & acctTypeKind.Contains(m.acct_type.Substring(0, 1))
                                & xItem.product_type == productType
                                & xItem.fu_mk == fuMk
                                & xItem.item_con == itemCon
                                & xItem.disc_part_feat == discPartFeat

                            select new OGL10181Model()
                            {
                                tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type,
                                smpNum = m.smp_num.Trim(),
                                smpName = m.smp_name.Trim(),
                                productType = m.product_type.Trim(),
                                acctType = m.acct_type.Trim()
                            }).Distinct().OrderBy(d => d.tempId).ToList<OGL10181Model>();

                }
            }

            return rows;
        }


        /// <summary>
        /// 以科目代號查詢科目樣本險種類別資料
        /// </summary>
        /// <param name="smpNum"></param>
        /// <returns></returns>
        public List<OGL10181Model> qryBySmpNum(String smpNum)
        {
            List<OGL10181Model> rows = new List<OGL10181Model>();
            using (new TransactionScope(
                  TransactionScopeOption.Required,
                  new TransactionOptions
                  {
                      IsolationLevel = IsolationLevel.ReadUncommitted
                  }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {

                    rows = (from m in db.FGL_SMPB

                            join codeItem in db.SYS_CODE.Where(x => x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeItem.CODE into psItem
                            from xItem in psItem.DefaultIfEmpty()

                            join codeAcct in db.SYS_CODE.Where(x => x.CODE_TYPE == "ACCT_TYPE") on m.acct_type equals codeAcct.CODE into psAcct
                            from xAcct in psAcct.DefaultIfEmpty()

                            join codeStatus in db.SYS_CODE.Where(x => x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                            from xStatus in psStatus.DefaultIfEmpty()

                            where 1 == 1
                                & m.smp_num == smpNum

                            select new OGL10181Model()
                            {
                                tempId = m.smp_num + "|" + m.product_type + "|" + m.acct_type,
                                smpNum = m.smp_num.Trim(),
                                smpName = m.smp_name.Trim(),
                                productType = m.product_type.Trim(),
                                productTypeDesc = (xItem == null ? String.Empty : m.product_type.Trim() + "." + xItem.CODE_VALUE),
                                acctType = m.acct_type.Trim(),
                                acctTypeDesc = (xAcct == null ? String.Empty : m.acct_type.Trim() + "." + xAcct.CODE_VALUE),
                                dataStatus = m.data_status.Trim(),
                    dataStatusDesc = (xStatus == null ? String.Empty : xStatus.CODE_VALUE),
                    updateId = m.update_id == null ? "" : m.update_id.Trim(),
                    updateDatetime = m.update_datetime == null ? "" : SqlFunctions.DateName("year", m.update_datetime) + "/" +
                                                                         SqlFunctions.DatePart("m", m.update_datetime) + "/" +
                                                                         SqlFunctions.DateName("day", m.update_datetime).Trim() + " " +
                                                                         SqlFunctions.DateName("hh", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("n", m.update_datetime).Trim() + ":" +
                                                                         SqlFunctions.DateName("s", m.update_datetime).Trim(),
                    
                            }).Distinct().OrderBy(d => d.tempId).ToList<OGL10181Model>();

                }
            }

            return rows;
        }




        /// <summary>
        /// 新增"科目權本險種類別檔"
        /// </summary>
        /// <param name="smpb"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_SMPB smpb, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_SMPB
                   ([SMP_NUM]
                   ,[SMP_NAME]
                   ,[PRODUCT_TYPE]
                   ,[ACCT_TYPE]
                   ,[DATA_STATUS]
                   ,[UPDATE_ID]
                   ,[UPDATE_DATETIME]
                   ,[APPR_ID]
                   ,[APPROVE_DATETIME])

             VALUES
                  (@SMP_NUM
                   ,@SMP_NAME
                   ,@PRODUCT_TYPE
                   ,@ACCT_TYPE
                   ,@DATA_STATUS
                   ,@UPDATE_ID
                   ,@UPDATE_DATETIME
                   ,@APPR_ID
                   ,@APPROVE_DATETIME)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpb.smp_num));
                cmd.Parameters.AddWithValue("@SMP_NAME", StringUtil.halfToFull(StringUtil.toString(smpb.smp_name)));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpb.product_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpb.acct_type));
                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(smpb.data_status));

                cmd.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpb.update_id));
                cmd.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.update_datetime ?? System.DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpb.appr_id));
                cmd.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.approve_datetime ?? System.DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 異動"科目權本險種類別檔"
        /// </summary>
        /// <param name="smpb"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int update(FGL_SMPB smpb, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update FGL_SMPB
        set SMP_NAME = @SMP_NAME
           ,DATA_STATUS = @DATA_STATUS
           ,UPDATE_ID = @UPDATE_ID
           ,UPDATE_DATETIME = @UPDATE_DATETIME
           ,APPR_ID = @APPR_ID
           ,APPROVE_DATETIME = @APPROVE_DATETIME
        where 1=1
        and SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpb.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpb.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpb.acct_type));
                command.Parameters.AddWithValue("@SMP_NAME", StringUtil.halfToFull(StringUtil.toString(smpb.smp_name)));
                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(smpb.data_status));
                
                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpb.update_id));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.update_datetime ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpb.appr_id));
                command.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.approve_datetime ?? System.DBNull.Value;



                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }




        /// <summary>
        /// 異動"科目權本險種類別檔"資料狀態
        /// </summary>
        /// <param name="smpb"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateStatus(string dataStatus, FGL_SMPB smpb, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"update FGL_SMPB
        set DATA_STATUS = @DATA_STATUS
           ,UPDATE_ID = @UPDATE_ID
           ,UPDATE_DATETIME = @UPDATE_DATETIME
           ,APPR_ID = @APPR_ID
           ,APPROVE_DATETIME = @APPROVE_DATETIME
        where 1=1
        and SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE
        ";


            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpb.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpb.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpb.acct_type));
                command.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(dataStatus));

                command.Parameters.AddWithValue("@UPDATE_ID", StringUtil.toString(smpb.update_id));
                command.Parameters.Add("@UPDATE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.update_datetime ?? System.DBNull.Value;

                command.Parameters.AddWithValue("@APPR_ID", StringUtil.toString(smpb.appr_id));
                command.Parameters.Add("@APPROVE_DATETIME", System.Data.SqlDbType.DateTime).Value = (System.Object)smpb.approve_datetime ?? System.DBNull.Value;



                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }




        /// <summary>
        /// 刪除"科目權本險種類別檔"
        /// </summary>
        /// <param name="smpb"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int delete (FGL_SMPB smpb, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"delete FGL_SMPB
        where SMP_NUM = @SMP_NUM
        and PRODUCT_TYPE = @PRODUCT_TYPE
        and ACCT_TYPE = @ACCT_TYPE";

            SqlCommand command = conn.CreateCommand();

            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(smpb.smp_num));
                command.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(smpb.product_type));
                command.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(smpb.acct_type));
             
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