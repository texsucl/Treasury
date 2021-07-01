
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
    public class FGLItemAcctHisDao
    {

        public int qryByProductCnt(string apprStat, string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4)
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
                               join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no
                               where m.product_type == productType
                               & m.fu_mk == fuMk
                               & m.item_con == itemCon
                               & m.disc_part_feat == discPartFeat
                               & m.is_ifrs4 == isIfrs4
                               & appr.appr_stat == apprStat
                                 & (appr.appr_stat == "0" || appr.appr_stat == "1")
                               select new OGL00001Model
                               {
                                   acctType = m.acct_type.Trim(),
                                   acctItem = m.smp_num.Trim(),
                                   corpNo = m.corp_no.Trim()

                               }).Count();

                    return his;
                }
            }

        }



        public List<OGL00001Model> qryHead(string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4)
        {
            bool bproductType = StringUtil.isEmpty(productType);
            bool bfuMk = StringUtil.isEmpty(fuMk);
            bool bitemCon = StringUtil.isEmpty(itemCon);
            bool bdiscPartFeat = StringUtil.isEmpty(discPartFeat);
            bool bisIfrs4 = StringUtil.isEmpty(isIfrs4);

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
                               //join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no
                               join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no into psHis
                               from xHis in psHis.DefaultIfEmpty()

                               where (appr.appr_stat == "0" || appr.appr_stat == "1")
                               & appr.aply_type == "C"
                               & (bproductType || appr.appr_mapping_key.Substring(0, 1) == productType)
                               & (bfuMk || appr.appr_mapping_key.Substring(2, 1) == fuMk)
                               & (bitemCon || appr.appr_mapping_key.Substring(4, 1) == itemCon)
                               & (bdiscPartFeat || appr.appr_mapping_key.Substring(6, 1) == discPartFeat)
                               & (bisIfrs4 || appr.appr_mapping_key.Substring(8, 1) == isIfrs4)

                               //& (bproductType || m.product_type == productType)
                               //& (bfuMk || m.fu_mk == fuMk)
                               //& (bitemCon || m.item_con == itemCon)
                               //& (bdiscPartFeat || m.disc_part_feat == discPartFeat)
                               //& (bisIfrs4 || m.is_ifrs4 == isIfrs4)
                               select new OGL00001Model
                               {
                                   tempId = appr.appr_mapping_key,
                                   productType = appr.appr_mapping_key.Substring(0, 1),
                                   fuMk = appr.appr_mapping_key.Substring(2, 1),
                                   itemCon = appr.appr_mapping_key.Substring(4, 1),
                                   discPartFeat = appr.appr_mapping_key.Substring(6, 1),
                                   isIfrs4 = appr.appr_mapping_key.Substring(8, 1),

                                   //tempId = xHis.product_type.Trim() + "|"
                                   //         + xHis.fu_mk.Trim() + "|"
                                   //         + xHis.item_con.Trim() + "|"
                                   //         + xHis.disc_part_feat.Trim() + "|"
                                   //         + xHis.is_ifrs4.Trim(),
                                   //productType = xHis.product_type.Trim(),
                                   //fuMk = xHis.fu_mk.Trim(),
                                   //itemCon = xHis.item_con.Trim(),
                                   //discPartFeat = xHis.disc_part_feat.Trim(),
                                   //isIfrs4 = xHis.is_ifrs4.Trim(),
                                   investTypeMk = xHis.invest_type_mk.Trim(),
                                   investTradMk = xHis.invest_trad_mk.Trim(),
                                   lodprmMk = xHis.lodprm_mk.Trim(),
                                   //extSchedulMk = m.ext_schedul_mk.Trim(),
                                   pakindDmopMk = xHis.pakind_dmop_mk.Trim(),
                                   coiType = xHis.coi_type.Trim(),
                                   dataStatus = appr.appr_stat == "0" ? "1" : "2",
                                   dataStatusDesc = appr.appr_stat == "0" ? "可異動" : "凍結中"

                               }).Distinct().OrderBy(x => x.tempId).ToList<OGL00001Model>();

                    return his;
                }
            }
        }

        public List<OGL00001Model> qryByAplyNo(string aplyNo)
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
                               join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no

                               join formal in db.FGL_ITEM_ACCT on 
                               new { m.product_type, m.fu_mk, m.item_con, m.disc_part_feat, m.is_ifrs4, m.acct_type, m.smp_num, m.corp_no, m.dmop_mk} 
                               equals new { formal.product_type, formal.fu_mk, formal.item_con, formal.disc_part_feat, formal.is_ifrs4, formal.acct_type, formal.smp_num, formal.corp_no, formal.dmop_mk}
                    into psFormal
                               from xFormal in psFormal.DefaultIfEmpty()

                               where appr.aply_no == aplyNo
                               select new OGL00001Model
                               {
                                   tempId = m.acct_type.Trim() + "|"
                                            + m.smp_num.Trim() + "|"
                                            + m.corp_no.Trim(),
                                   aplyNo = m.aply_no,
                                   productType = m.product_type.Trim(),
                                   fuMk = m.fu_mk.Trim(),
                                   itemCon = m.item_con.Trim(),
                                   discPartFeat = m.disc_part_feat.Trim(),
                                   isIfrs4 = m.is_ifrs4.Trim(),
                                   investTypeMk = m.invest_type_mk.Trim(),
                                   investTradMk = m.invest_trad_mk.Trim(),
                                   lodprmMk = m.lodprm_mk.Trim(),
                                   //extSchedulMk = m.ext_schedul_mk.Trim(),
                                   pakindDmopMk = m.pakind_dmop_mk.Trim(),
                                   coiType = m.coi_type.Trim(),
                                   acctType = m.acct_type.Trim(),
                                   acctItem = m.smp_num.Trim(),
                                   corpNo = m.corp_no.Trim(),
                                   dmopMk = m.dmop_mk.Trim(),
                                   updateMk = xFormal.smp_num == null ? "Y" : "N"

                               }).ToList<OGL00001Model>();

                    return his;
                }
            }

        }


        public List<OGL00001Model> qryByProduct(string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4 )
        {
            string mappingKey = StringUtil.toString(productType) + "|"
                + StringUtil.toString(fuMk) + "|"
                + StringUtil.toString(itemCon) + "|"
                + StringUtil.toString(discPartFeat) + "|"
                + StringUtil.toString(isIfrs4);

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
                                   join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no into psHis
                                  from xHis in psHis.DefaultIfEmpty()


                               where appr.appr_mapping_key == mappingKey
                               //m.product_type == productType
                               //    & m.fu_mk == fuMk
                               //    & m.item_con== itemCon
                               //    & m.disc_part_feat == discPartFeat
                               //    & m.is_ifrs4 == isIfrs4
                                   & (appr.appr_stat == "0" || appr.appr_stat == "1")
                               select new OGL00001Model
                               {
                                   tempId = appr.appr_mapping_key,
                                   productType = appr.appr_mapping_key.Substring(0, 1),
                                   fuMk = appr.appr_mapping_key.Substring(2, 1),
                                   itemCon = appr.appr_mapping_key.Substring(4, 1),
                                   discPartFeat = appr.appr_mapping_key.Substring(6, 1),
                                   isIfrs4 = appr.appr_mapping_key.Substring(8, 1),
                                   //tempId = xHis.acct_type.Trim() + "|" 
                                   //         + xHis.smp_num.Trim() + "|" 
                                   //         + xHis.corp_no.Trim(),
                                   aplyNo = appr.aply_no,
                                   apprStat = appr.appr_stat,
                                   //productType = xHis.product_type.Trim(),
                                   //fuMk = xHis.fu_mk.Trim(),
                                   //itemCon = xHis.item_con.Trim(),
                                   //discPartFeat = xHis.disc_part_feat.Trim(),
                                   //isIfrs4 = xHis.is_ifrs4.Trim(),
                                   investTypeMk = xHis.invest_type_mk.Trim(),
                                   investTradMk = xHis.invest_trad_mk.Trim(),
                                   lodprmMk = xHis.lodprm_mk.Trim(),
                                   //extSchedulMk = m.ext_schedul_mk.Trim(),
                                   pakindDmopMk = xHis.pakind_dmop_mk.Trim(),
                                   coiType = xHis.coi_type.Trim(),
                                   acctType = xHis.acct_type.Trim(),
                                   acctItem = xHis.smp_num.Trim(),
                                   corpNo = xHis.corp_no.Trim(),
                                   dmopMk = xHis.dmop_mk.Trim()

                               }).ToList<OGL00001Model>();

                    return his;
                }
            }
                    
        }


        public OGL00001Model qryByKey(FGL_ITEM_ACCT_HIS d)
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
                               join m in db.FGL_ITEM_ACCT_HIS on appr.aply_no equals m.aply_no

                               where appr.aply_no == d.aply_no
                               & m.product_type == d.product_type
                               & m.fu_mk == d.fu_mk
                               & m.item_con == d.item_con
                               & m.disc_part_feat == d.disc_part_feat
                               & m.is_ifrs4 == d.is_ifrs4
                               & m.acct_type == d.acct_type
                               & m.smp_num == d.smp_num
                               & m.corp_no == d.corp_no
                               select new OGL00001Model
                               {
                                   tempId = m.acct_type.Trim() + "|"
                                            + m.smp_num.Trim() + "|"
                                            + m.corp_no.Trim(),
                                   productType = m.product_type.Trim(),
                                   fuMk = m.fu_mk.Trim(),
                                   itemCon = m.item_con.Trim(),
                                   discPartFeat = m.disc_part_feat.Trim(),
                                   isIfrs4 = m.is_ifrs4.Trim(),
                                   investTypeMk = m.invest_type_mk.Trim(),
                                   investTradMk = m.invest_trad_mk.Trim(),
                                   lodprmMk = m.lodprm_mk.Trim(),
                                   //extSchedulMk = m.ext_schedul_mk.Trim(),
                                   pakindDmopMk = m.pakind_dmop_mk.Trim(),
                                   coiType = m.coi_type.Trim(),
                                   acctType = m.acct_type.Trim(),
                                   acctItem = m.smp_num.Trim(),
                                   corpNo = m.corp_no.Trim(),
                                   dmopMk = m.dmop_mk.Trim()

                               }).FirstOrDefault();

                    if (his == null)
                        his = new OGL00001Model();


                    return his;
                }
            }

        }



        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void insert(FGL_ITEM_ACCT_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_ACCT_HIS
                   ([APLY_NO]
      ,[EXEC_ACTION]
      ,[PRODUCT_TYPE]
      ,[FU_MK]
      ,[ITEM_CON]
      ,[DISC_PART_FEAT]
      ,[IS_IFRS4]
      ,[INVEST_TYPE_MK]
      ,[INVEST_TRAD_MK]
      ,[LODPRM_MK]
      ,[EXT_SCHEDUL_MK]
      ,[PAKIND_DMOP_MK]
      ,[COI_TYPE]
      ,[ACCT_TYPE]
      ,[SMP_NUM]
      ,[CORP_NO]
      ,[DMOP_MK])

             VALUES
(@APLY_NO
      ,@EXEC_ACTION
      ,@PRODUCT_TYPE
      ,@FU_MK
      ,@ITEM_CON
      ,@DISC_PART_FEAT
      ,@IS_IFRS4
      ,@INVEST_TYPE_MK
      ,@INVEST_TRAD_MK
      ,@LODPRM_MK
      ,''
      ,@PAKIND_DMOP_MK
      ,@COI_TYPE
      ,@ACCT_TYPE
      ,@SMP_NUM
      ,@CORP_NO
      ,@DMOP_MK)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(his.exec_action));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.is_ifrs4));
                cmd.Parameters.AddWithValue("@INVEST_TYPE_MK", StringUtil.toString(his.invest_type_mk));
                cmd.Parameters.AddWithValue("@INVEST_TRAD_MK", StringUtil.toString(his.invest_trad_mk));
                cmd.Parameters.AddWithValue("@LODPRM_MK", StringUtil.toString(his.lodprm_mk));
                //cmd.Parameters.AddWithValue("@EXT_SCHEDUL_MK", StringUtil.toString(his.ext_schedul_mk));
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_MK", StringUtil.toString(his.pakind_dmop_mk));
                cmd.Parameters.AddWithValue("@COI_TYPE", StringUtil.toString(his.coi_type));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(his.acct_type));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smp_num));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(his.corp_no));
                cmd.Parameters.AddWithValue("@DMOP_MK", StringUtil.toString(his.dmop_mk));

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public void insertFromFormal(OGL00001Model his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_ACCT_HIS
                   ([APLY_NO]
      ,[PRODUCT_TYPE]
      ,[FU_MK]
      ,[ITEM_CON]
      ,[DISC_PART_FEAT]
      ,[IS_IFRS4]
      ,[INVEST_TYPE_MK]
      ,[INVEST_TRAD_MK]
      ,[LODPRM_MK]
      ,[EXT_SCHEDUL_MK]
      ,[PAKIND_DMOP_MK]
      ,[COI_TYPE]
      ,[ACCT_TYPE]
      ,[SMP_NUM]
      ,[CORP_NO]
      ,[DMOP_MK])
(SELECT @APLY_NO
      ,[PRODUCT_TYPE]
      ,[FU_MK]
      ,[ITEM_CON]
      ,[DISC_PART_FEAT]
      ,[IS_IFRS4]
      ,[INVEST_TYPE_MK]
      ,[INVEST_TRAD_MK]
      ,[LODPRM_MK]
      ,[EXT_SCHEDUL_MK]
      ,[PAKIND_DMOP_MK]
      ,[COI_TYPE]
      ,[ACCT_TYPE]
      ,[SMP_NUM]
      ,[CORP_NO]
      ,[DMOP_MK]
FROM FGL_ITEM_ACCT
WHERE PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aplyNo));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.productType));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fuMk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.itemCon));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.discPartFeat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.isIfrs4));

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }

        public void updateByKey(FGL_ITEM_ACCT_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"UPDATE FGL_ITEM_ACCT_HIS
SET INVEST_TYPE_MK = @INVEST_TYPE_MK
   ,INVEST_TRAD_MK = @INVEST_TRAD_MK
   ,LODPRM_MK = @LODPRM_MK
   ,PAKIND_DMOP_MK = @PAKIND_DMOP_MK
   ,COI_TYPE = @COI_TYPE
   ,DMOP_MK = @DMOP_MK
WHERE APLY_NO = @APLY_NO
  AND PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4
  AND ACCT_TYPE = @ACCT_TYPE
  AND SMP_NUM = @SMP_NUM
  AND CORP_NO = @CORP_NO
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@INVEST_TYPE_MK", StringUtil.toString(his.invest_type_mk));
                cmd.Parameters.AddWithValue("@INVEST_TRAD_MK", StringUtil.toString(his.invest_trad_mk));
                cmd.Parameters.AddWithValue("@LODPRM_MK", StringUtil.toString(his.lodprm_mk));
                //cmd.Parameters.AddWithValue("@EXT_SCHEDUL_MK", StringUtil.toString(his.ext_schedul_mk));
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_MK", StringUtil.toString(his.pakind_dmop_mk));
                cmd.Parameters.AddWithValue("@COI_TYPE", StringUtil.toString(his.coi_type));
                cmd.Parameters.AddWithValue("@DMOP_MK", StringUtil.toString(his.dmop_mk));

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.is_ifrs4));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(his.acct_type));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smp_num));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(his.corp_no));
                

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public void updateProdAppr(FGL_ITEM_ACCT_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"UPDATE FGL_ITEM_ACCT_HIS
SET INVEST_TYPE_MK = @INVEST_TYPE_MK
   ,INVEST_TRAD_MK = @INVEST_TRAD_MK
   ,LODPRM_MK = @LODPRM_MK
   ,PAKIND_DMOP_MK = @PAKIND_DMOP_MK
   ,COI_TYPE = @COI_TYPE
WHERE APLY_NO = @APLY_NO
  AND PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@INVEST_TYPE_MK", StringUtil.toString(his.invest_type_mk));
                cmd.Parameters.AddWithValue("@INVEST_TRAD_MK", StringUtil.toString(his.invest_trad_mk));
                cmd.Parameters.AddWithValue("@LODPRM_MK", StringUtil.toString(his.lodprm_mk));
                //cmd.Parameters.AddWithValue("@EXT_SCHEDUL_MK", StringUtil.toString(his.ext_schedul_mk));
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_MK", StringUtil.toString(his.pakind_dmop_mk));
                cmd.Parameters.AddWithValue("@COI_TYPE", StringUtil.toString(his.coi_type));

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.is_ifrs4));


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public void delete(FGL_ITEM_ACCT_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"DELETE FGL_ITEM_ACCT_HIS
WHERE APLY_NO = @APLY_NO
  AND PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4
  AND ACCT_TYPE = @ACCT_TYPE
  AND SMP_NUM = @SMP_NUM
  AND CORP_NO = @CORP_NO
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.is_ifrs4));
                cmd.Parameters.AddWithValue("@ACCT_TYPE", StringUtil.toString(his.acct_type));
                cmd.Parameters.AddWithValue("@SMP_NUM", StringUtil.toString(his.smp_num));
                cmd.Parameters.AddWithValue("@CORP_NO", StringUtil.toString(his.corp_no));


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public int delByAplyNo(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {
                string sql = @"DELETE FGL_ITEM_ACCT_HIS WHERE APLY_NO = @APLY_NO";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", aplyNo);


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