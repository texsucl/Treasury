
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
    public class FGLItemSmpnumHisDao
    {

        public void insertFromFormal(string aplyNo, string item, string productType, string fuMk, string itemCon, string discPartFeat
            , string dataType, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
INSERT INTO FGL_ITEM_SMPNUM_HIS
(  APLY_NO
  ,ITEM
  ,PRODUCT_TYPE
  ,FU_MK
  ,ITEM_CON
  ,DISC_PART_FEAT
  ,SMP_NUM
  ,ACCT_TYPE
  ,DATA_TYPE
  ,FLAG) 
SELECT @APLY_NO
      ,ITEM 
      ,PRODUCT_TYPE
      ,FU_MK
      ,ITEM_CON
      ,DISC_PART_FEAT
      ,SMP_NUM
      ,ACCT_TYPE
      ,DATA_TYPE
      ,FLAG
 FROM FGL_ITEM_SMPNUM
  WHERE ITEM = @ITEM
    AND PRODUCT_TYPE = @PRODUCT_TYPE
    AND FU_MK = @FU_MK
    AND ITEM_CON = @ITEM_CON
    AND DISC_PART_FEAT = @DISC_PART_FEAT
    AND DATA_TYPE = @DATA_TYPE";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);
                cmd.Parameters.AddWithValue("@ITEM", item);
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", productType);
                cmd.Parameters.AddWithValue("@FU_MK", fuMk);
                cmd.Parameters.AddWithValue("@ITEM_CON", itemCon);
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", discPartFeat);
                cmd.Parameters.AddWithValue("@DATA_TYPE", dataType);

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }



        /// <summary>
        /// 以"覆核單號"進行查詢
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<OGL00003DModel> qryByAplyNo(string aplyNo, string dataType)
        {
            bool bDataType = StringUtil.isEmpty(dataType);

            List<OGL00003DModel> rows = new List<OGL00003DModel>();


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    rows = (from m in db.FGL_ITEM_SMPNUM_HIS
                            join sumB in db.FGL_SMPB on new { m.smp_num, m.product_type, m.acct_type } equals new { sumB.smp_num, sumB.product_type, sumB.acct_type } into psSumB
                            from xSumB in psSumB.DefaultIfEmpty()

                            join formal in db.FGL_ITEM_SMPNUM on new { m.item, m.product_type, m.fu_mk, m.item_con, m.disc_part_feat, m.smp_num, m.acct_type, m.flag }
                            equals new { formal.item, formal.product_type, formal.fu_mk, formal.item_con, formal.disc_part_feat, formal.smp_num, formal.acct_type, formal.flag } into psFormal
                            from xFormal in psFormal.DefaultIfEmpty()

                            where 1 == 1
                               & m.aply_no == aplyNo
                               & (bDataType || (!bDataType & m.data_type == dataType))
                               select new OGL00003DModel
                               {
                                   aplyNo = m.aply_no,
                                   item = m.item,
                                   productType = m.product_type,
                                   fuMk = m.fu_mk,
                                   itemCon = m.item_con,
                                   discPartFeat = m.disc_part_feat,

                                   smpNum = m.smp_num,
                                   smpName = xSumB.smp_name,
                                   acctType = m.acct_type,
                                   dataType = m.data_type,
                                   flag = m.flag,
                                   bChg = xFormal.flag == null ? true : false
                               }).ToList<OGL00003DModel>();

                    return rows;
                }
            }

        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_ITEM_SMPNUM_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"INSERT INTO FGL_ITEM_SMPNUM_HIS
                   ([APLY_NO]
           ,[ITEM]
           ,[PRODUCT_TYPE]
           ,[FU_MK]
           ,[ITEM_CON]
           ,[DISC_PART_FEAT]
           ,[SMP_NUM]
           ,[ACCT_TYPE]
           ,[DATA_TYPE]
           ,[FLAG])
VALUES
(@APLY_NO
,@ITEM
,@PRODUCT_TYPE
,@FU_MK
,@ITEM_CON
,@DISC_PART_FEAT
,@SMP_NUM
,@ACCT_TYPE
,@DATA_TYPE
,@FLAG
)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));

                cmd.Parameters.AddWithValue("@ITEM", StringUtil.toString(his.item));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));

                cmd.Parameters.AddWithValue("@SMP_NUM", his.smp_num);
                cmd.Parameters.AddWithValue("@ACCT_TYPE", his.acct_type);
                cmd.Parameters.AddWithValue("@DATA_TYPE", his.data_type);
                cmd.Parameters.AddWithValue("@FLAG", his.flag);


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public int deleteByAplyNoDataType(string aplyNo, string dataType, SqlConnection conn, SqlTransaction transaction)
        {


            SqlCommand cmd = conn.CreateCommand();


            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                string sql = "";
                sql = @"
DELETE FGL_ITEM_SMPNUM_HIS 
WHERE APLY_NO = @APLY_NO 
  AND DATA_TYPE = @DATA_TYPE";


                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);
                cmd.Parameters.AddWithValue("@DATA_TYPE", StringUtil.toString(dataType));

                int cnt = cmd.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }

    }
}