
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
    public class FGLItemSmpnumDao
    {
        public List<OGL00003DModel> qryByItem(string item, string productType, string fuMk, string itemCon, string discPartFeat, string dataType)
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
                    rows = (from m in db.FGL_ITEM_SMPNUM
                            join sumB in db.FGL_SMPB on new { m.smp_num, m.product_type, m.acct_type } equals new { sumB.smp_num, sumB.product_type, sumB.acct_type } into psSumB
                            from xSumB in psSumB.DefaultIfEmpty()

                            where 1 == 1
                               & m.item == item
                               & m.product_type == productType
                               & m.fu_mk == fuMk
                               & m.item_con == itemCon
                               & m.disc_part_feat == discPartFeat
                               & (bDataType || (!bDataType & m.data_type == dataType))
                               select new OGL00003DModel
                               {
                                   item = m.item,
                                   productType = m.product_type,
                                   fuMk = m.fu_mk,
                                   itemCon = m.item_con,
                                   discPartFeat = m.disc_part_feat,

                                   smpNum = m.smp_num,
                                   smpName = xSumB.smp_name,
                                   acctType = m.acct_type,
                                   dataType = m.data_type,
                                   flag = m.flag
                               }).ToList<OGL00003DModel>();

                    return rows;
                }
            }

        }

        public void insertFromHis(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"INSERT INTO FGL_ITEM_SMPNUM
                   (ITEM
           ,PRODUCT_TYPE
           ,FU_MK
           ,ITEM_CON
           ,DISC_PART_FEAT
           ,SMP_NUM
           ,ACCT_TYPE
           ,DATA_TYPE
           ,FLAG) 
SELECT ITEM 
           ,PRODUCT_TYPE
           ,FU_MK
           ,ITEM_CON
           ,DISC_PART_FEAT
           ,SMP_NUM
           ,ACCT_TYPE
           ,DATA_TYPE
           ,FLAG
 FROM FGL_ITEM_SMPNUM_HIS 
  WHERE APLY_NO = @APLY_NO";

                SqlCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void insert(FGL_ITEM_SMPNUM d, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"INSERT INTO FGL_ITEM_SMPNUM
                   ([ITEM]
           ,[PRODUCT_TYPE]
           ,[FU_MK]
           ,[ITEM_CON]
           ,[DISC_PART_FEAT]
           ,[SMP_NUM]
           ,[ACCT_TYPE]
           ,[DATA_TYPE]
           ,[FLAG])
VALUES
(@ITEM
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

                cmd.Parameters.AddWithValue("@ITEM", StringUtil.toString(d.item));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(d.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(d.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(d.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(d.disc_part_feat));

                cmd.Parameters.AddWithValue("@SMP_NUM", d.smp_num);
                cmd.Parameters.AddWithValue("@ACCT_TYPE", d.acct_type);
                cmd.Parameters.AddWithValue("@DATA_TYPE", d.data_type);
                cmd.Parameters.AddWithValue("@FLAG", d.flag);


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void deleteByItem(string item, string productType, string fuMk, string itemCon, string discPartFeat
            , SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"
DELETE FGL_ITEM_SMPNUM
  WHERE ITEM = @ITEM
    AND PRODUCT_TYPE = @PRODUCT_TYPE
    AND FU_MK = @FU_MK
    AND ITEM_CON = @ITEM_CON
    AND DISC_PART_FEAT = @DISC_PART_FEAT
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@ITEM", item);
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", productType);
                cmd.Parameters.AddWithValue("@FU_MK", fuMk);
                cmd.Parameters.AddWithValue("@ITEM_CON", itemCon);
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", discPartFeat);


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                throw e;
            }
        }


    }
}