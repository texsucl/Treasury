
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
    public class FGLItemAcctDao
    {
        /// <summary>
        /// "商品資料會計接收作業"會計科目的取得
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <param name="comType"></param>
        /// <param name="extSchedulType"></param>
        /// <param name="lodprmType"></param>
        /// <param name="pakindDmopType"></param>
        /// <param name="coiType"></param>
        /// <returns></returns>
        public List<OGL00003DModel> qryForOGL00005(string productType, string fuMk, string itemCon, string discPartFeat, 
            string comType, string extSchedulType, string lodprmType, string pakindDmopType, string coiType)
        {
            //10.2.4.2    佣金 / 承攬費 = 1佣金:
            //            取商品科目設定中帳務類別第1碼 = E
            //10.2.4.3    佣金 / 承攬費 = 2承攬費:
            //            取商品科目設定中取帳務類別第1碼 = F
            //10.2.4.4    佣金 / 承攬費 = 3取商品科目設定中帳務類別第1碼 = E,及F
            //10.2.4.5    佣金 / 承攬費 = 4不需處理佣金及承攬費的科目
            //10.2.4.6    可展期定期保險 = Y取,商品科目設定中帳務類別第1碼 = G,-->20190416取消此判斷
            //10.2.4.7    保費費用 = Y取帳務類別 = A13 -->20190416 保費費用=外收，取帳務類別=G14
            //10.2.4.8    繳費方式僅限躉繳 = Y則排除商品科目設定中限躉繳 = N
            //10.2.4.9    險種科目 + 帳務類別第2碼 = 1
            //10.2.4.10   分離科目 + 帳務類別第2碼 = 2
            //10.2.4.11   COI科目 + 帳務類別第2碼 = 3
            //10.2.4.13 10.2.4.13	帳務類別第1碼英文字母A類、G11~13的樣本科目代號-->0190416
            //10.2.4.14 若COI=Y，要組帳務類別是D31-->0190416

            List<OGL00003DModel> rtnList = new List<OGL00003DModel>(); 
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from m in db.FGL_ITEM_ACCT
                               join sumA in db.FGL_SMPA on new { smpNum = m.smp_num, m.product_type, m.acct_type, m.corp_no } equals new { smpNum = sumA.smp_num, sumA.product_type, sumA.acct_type, sumA.corp_no } into psSumA
                               from xSumA in psSumA.DefaultIfEmpty()

                               where 1 == 1
                               & m.product_type == productType
                               & m.fu_mk == fuMk
                               & m.item_con == itemCon
                               & m.disc_part_feat == discPartFeat
                               & (pakindDmopType != "Y" || (pakindDmopType == "Y" && m.dmop_mk != "N"))
                               select new OGL00003DModel
                               {
                                   tempId = "A",
                                   productType = m.product_type.Trim(),
                                   fuMk = m.fu_mk,
                                   itemCon = m.item_con,
                                   discPartFeat = m.disc_part_feat,
                                   isIfrs4 = m.is_ifrs4,
                                   acctType = m.acct_type,
                                   smpNum = m.smp_num,
                                   corpNo = m.corp_no,
                                   sqlSmpNum = xSumA.sql_actnum,
                                   sqlSmpName = xSumA.sql_actnm,
                                   
                               }).Distinct().ToList<OGL00003DModel>();

                    //var his = (from m in db.FGL_ITEM_ACCT
                    //           join sumA in db.FGL_SMPA on new { smpNum = m.smp_num, m.product_type, m.acct_type, m.corp_no } equals new { smpNum = sumA.smp_num, sumA.product_type, sumA.acct_type, sumA.corp_no } into psSumA
                    //           from xSumA in psSumA.DefaultIfEmpty()

                    //           where 1 == 1
                    //           & m.product_type == productType
                    //           & m.fu_mk == fuMk
                    //           & m.item_con == itemCon
                    //           & m.disc_part_feat == discPartFeat
                    //           &
                    //           (
                    //              (
                    //                    (comType == "1" && m.acct_type.Substring(0, 1) == "E")
                    //                    || (comType == "2" && m.acct_type.Substring(0, 1) == "F")
                    //                    || (comType == "3" && (m.acct_type.Substring(0, 1) == "E" || m.acct_type.Substring(0, 1) == "F"))
                    //                    || comType == "4" && (m.acct_type.Substring(0, 1) != "E" && m.acct_type.Substring(0, 1) != "F")
                    //              )
                    //           || (extSchedulType != "Y" || (extSchedulType == "Y" && m.acct_type.Substring(0, 1) == "G"))
                    //           || (lodprmType != "Y" || (lodprmType == "Y" && m.acct_type == "A13"))
                    //           || (pakindDmopType != "Y" || (pakindDmopType == "Y" && m.pakind_dmop_mk != "N"))
                    //           )
                    //           select new OGL00003DModel
                    //           {
                    //               tempId = "A",
                    //               productType = m.product_type.Trim(),
                    //               fuMk = m.fu_mk,
                    //               itemCon = m.item_con,
                    //               discPartFeat = m.disc_part_feat,
                    //               isIfrs4 = m.is_ifrs4,
                    //               acctType = m.acct_type,
                    //               smpNum = m.smp_num,
                    //               corpNo = m.corp_no,
                    //               sqlSmpNum = xSumA.sql_actnum,
                    //               sqlSmpName = xSumA.sql_actnm
                    //           }).Distinct().ToList<OGL00003DModel>();
                    //.Union
                    //(from m in db.FGL_ITEM_ACCT
                    // join sumA in db.FGL_SMPA on new { smpNum = m.smp_num, m.product_type, m.acct_type, m.corp_no } equals new { smpNum = sumA.smp_num, sumA.product_type, sumA.acct_type, sumA.corp_no } into psSumA
                    // from xSumA in psSumA.DefaultIfEmpty()

                    // where 1 == 1
                    // & m.product_type == productType
                    // & m.fu_mk == fuMk
                    // & m.item_con == itemCon
                    // & m.disc_part_feat == discPartFeat
                    // & (m.acct_type.Substring(1, 1) == "1" || m.acct_type.Substring(1, 1) == "2" || m.acct_type.Substring(1, 1) == "3")
                    // select new OGL00003DModel
                    // {
                    //     tempId = "S",
                    //     productType = m.product_type.Trim(),
                    //     fuMk = m.fu_mk,
                    //     itemCon = m.item_con,
                    //     discPartFeat = m.disc_part_feat,
                    //     isIfrs4 = m.is_ifrs4,
                    //     acctType = m.acct_type,
                    //     smpNum = m.smp_num,
                    //     corpNo = m.corp_no,
                    //     sqlSmpNum = xSumA.sql_actnum,
                    //     sqlSmpName = xSumA.sql_actnm

                    // }).Distinct().ToList<OGL00003DModel>();

                    //10.2.4.13	帳務類別第1碼英文字母A類、G11~13的樣本科目代號
                    foreach (OGL00003DModel d in his.Where(x => x.acctType.StartsWith("A") 
                    || x.acctType.Equals("G11") || x.acctType.Equals("G12") || x.acctType.Equals("G13"))) {
                        rtnList.Add(d);
                    }

                    //佣金/承攬費
                    switch (comType) {
                        case "1":
                            foreach (OGL00003DModel d in his.Where(x => x.acctType.StartsWith("E")))
                            {
                                rtnList.Add(d);
                            }
                            break;
                        case "2":
                            foreach (OGL00003DModel d in his.Where(x => x.acctType.StartsWith("F")))
                            {
                                rtnList.Add(d);
                            }
                            break;
                        case "3":
                            foreach (OGL00003DModel d in his.Where(x => (x.acctType.StartsWith("E") || x.acctType.StartsWith("F"))))
                            {
                                rtnList.Add(d);
                            }
                            break;
                    }


                    ////可展期定期保險
                    //if ("Y".Equals(extSchedulType)) {
                    //    foreach (OGL00003DModel d in his.Where(x => x.acctType.StartsWith("G") ))
                    //    {
                    //        rtnList.Add(d);
                    //    }
                    //}

                    //保費費用 
                    if ("Y".Equals(lodprmType))
                    {
                        foreach (OGL00003DModel d in his.Where(x => x.acctType.Equals("G14")))
                        {
                            rtnList.Add(d);
                        }
                    }

                    //COI
                    if ("Y".Equals(coiType))
                    {
                        foreach (OGL00003DModel d in his.Where(x => x.acctType.Equals("D31")))
                        {
                            rtnList.Add(d);
                        }
                    }


                    return rtnList;
                }
            }

        }


        public List<OGL00001Model> qryHeadForItem(string productType, string fuMk, string itemCon, string discPartFeat)
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
                    var his = (from m in db.FGL_ITEM_ACCT
                               join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                               from xStatus in psStatus.DefaultIfEmpty()
                               where 1 == 1
                               & m.product_type == productType
                               & m.fu_mk == fuMk
                               & m.item_con == itemCon
                               & m.disc_part_feat == discPartFeat

                               select new OGL00001Model
                               {
                                   tempId = m.product_type.Trim() + "|"
                                            + m.fu_mk.Trim() + "|"
                                            + m.item_con.Trim() + "|"
                                            + m.disc_part_feat.Trim() + "|"
                                            + m.is_ifrs4.Trim(),
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
                                   dataStatus = m.data_status,
                                   dataStatusDesc = xStatus.CODE_VALUE

                               }).Distinct().ToList<OGL00001Model>();

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
                    var his = (from  m in db.FGL_ITEM_ACCT
                               join codeStatus in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "DATA_STATUS") on m.data_status equals codeStatus.CODE into psStatus
                               from xStatus in psStatus.DefaultIfEmpty()
                               where 1 == 1
                               & (bproductType || m.product_type == productType)
                               & (bfuMk || m.fu_mk == fuMk)
                               & (bitemCon || m.item_con == itemCon)
                               & (bdiscPartFeat || m.disc_part_feat == discPartFeat)
                               & (bisIfrs4 || m.is_ifrs4 == isIfrs4)
                               select new OGL00001Model
                               {
                                   tempId = m.product_type.Trim() + "|"
                                            + m.fu_mk.Trim() + "|"
                                            + m.item_con.Trim() + "|"
                                            + m.disc_part_feat.Trim() + "|"
                                            + m.is_ifrs4.Trim(),
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
                                   dataStatus = m.data_status,
                                   dataStatusDesc = xStatus.CODE_VALUE

                               }).Distinct().OrderBy(x => x.tempId).ToList<OGL00001Model>();

                    return his;
                }
            }

        }


        public int qryByProductCnt(string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4)
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
                    var his = (from m in db.FGL_ITEM_ACCT 
                               where m.product_type == productType
                               & m.fu_mk == fuMk
                               & m.item_con == itemCon
                               & m.disc_part_feat == discPartFeat
                               & m.is_ifrs4 == isIfrs4

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

        


        public List<OGL00001Model> qryByProduct(string productType, string fuMk, string itemCon, string discPartFeat, string isIfrs4 )
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
                    var his = (from m in db.FGL_ITEM_ACCT 
                                   where m.product_type == productType
                                   & m.fu_mk == fuMk
                                   & m.item_con== itemCon
                                   & m.disc_part_feat == discPartFeat
                                   & m.is_ifrs4 == isIfrs4
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

                    return his;
                }
            }

        }



        
             public void insertFromHis(OGL00001Model his, FGL_APLY_REC fglAplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_ACCT
                   ([PRODUCT_TYPE]
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
      ,[DATA_STATUS]
      ,[UPDATE_ID]
      ,[UPDATE_DATETIME]
      ,[APPR_ID]
      ,[APPROVE_DATETIME])
(SELECT [PRODUCT_TYPE]
      ,[FU_MK]
      ,[ITEM_CON]
      ,[DISC_PART_FEAT]
      ,[IS_IFRS4]
      ,[INVEST_TYPE_MK]
      ,[INVEST_TRAD_MK]
      ,[LODPRM_MK]
      ,''
      ,[PAKIND_DMOP_MK]
      ,[COI_TYPE]
      ,[ACCT_TYPE]
      ,[SMP_NUM]
      ,[CORP_NO]
      ,[DMOP_MK]
      ,'1'
      ,@UPDATE_ID
      ,@UPDATE_DATETIME
      ,@APPR_ID
      ,@APPROVE_DATETIME
FROM FGL_ITEM_ACCT_HIS
WHERE APLY_NO = @APLY_NO
  AND PRODUCT_TYPE = @PRODUCT_TYPE
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
                cmd.Parameters.AddWithValue("@UPDATE_ID", fglAplyRec.create_id);
                cmd.Parameters.AddWithValue("@UPDATE_DATETIME", fglAplyRec.approve_datetime);

                cmd.Parameters.AddWithValue("@APPR_ID", fglAplyRec.appr_id);
                cmd.Parameters.AddWithValue("@APPROVE_DATETIME", fglAplyRec.approve_datetime);

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }


        /// <summary>
        /// 異動"資料狀態"
        /// </summary>
        /// <param name="his"></param>
        /// <param name="fglAplyRec"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        public void updateDataStatus(OGL00001Model his, FGL_APLY_REC fglAplyRec, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FGL_ITEM_ACCT
  SET  [DATA_STATUS] = @DATA_STATUS
      ,[UPDATE_ID] = @UPDATE_ID
      ,[UPDATE_DATETIME] = @UPDATE_DATETIME
WHERE PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@DATA_STATUS", StringUtil.toString(his.dataStatus));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.productType));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fuMk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.itemCon));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.discPartFeat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(his.isIfrs4));
                cmd.Parameters.AddWithValue("@UPDATE_ID", fglAplyRec.create_id);
                cmd.Parameters.AddWithValue("@UPDATE_DATETIME", fglAplyRec.create_dt);

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }



        public void delByProd(OGL00001Model model, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"DELETE FGL_ITEM_ACCT
WHERE PRODUCT_TYPE = @PRODUCT_TYPE
  AND FU_MK = @FU_MK
  AND ITEM_CON = @ITEM_CON
  AND DISC_PART_FEAT = @DISC_PART_FEAT
  AND IS_IFRS4 = @IS_IFRS4
";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;


                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(model.productType));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(model.fuMk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(model.itemCon));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(model.discPartFeat));
                cmd.Parameters.AddWithValue("@IS_IFRS4", StringUtil.toString(model.isIfrs4));


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }

    }
}