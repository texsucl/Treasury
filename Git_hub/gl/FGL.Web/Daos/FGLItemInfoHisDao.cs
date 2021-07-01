
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FGL.Web.Models;
using System.Transactions;
using FGL.Web.ViewModels;
using System.Data.Entity.SqlServer;
using FGL.Web.BO;


/// <summary>
/// 功能說明：FGL_ITEM_INFO_his 會計商品資訊暫存檔
/// 初版作者：20190104 Daiyu
/// 修改歷程：20190104 Daiyu
/// 需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191129 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：新增"商品簡稱"欄位
/// </summary>
/// 

namespace FGL.Web.Daos
{
    public class FGLItemInfoHisDao
    {


        /// <summary>
        /// 檢查"保險商品編號"是否重複
        /// </summary>
        /// <param name="prodNo"></param>
        /// <returns></returns>
        public string qryByProdNo(string prodNo, string item)
        {
            string dupItem = "";
            string[] apprMk = new string[] { "0", "6" };

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FGL_ITEM_INFO_HIS d = db.FGL_ITEM_INFO_HIS
                        .Where(x => x.prod_no == prodNo
                        & x.item != item
                        & !apprMk.Contains(x.appr_mk)
                        ).FirstOrDefault();

                    if (d != null)
                        dupItem = d.item;

                    return dupItem;
                }
            }
        }



        /// <summary>
        /// 檢查險種科目是否已存在
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemAcct"></param>
        /// <returns></returns>
        public string chkItemAcct(string item, string itemAcct)
        {
            string[] apprMk = new string[] { "0", "6" };
            string itemExist = "";
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FGL_ITEM_INFO_HIS d = db.FGL_ITEM_INFO_HIS
                    .Where(x => (x.item_acct == itemAcct || x.coi_acct == itemAcct)
                        & x.item != item
                        & !apprMk.Contains(x.appr_mk)
                    ).FirstOrDefault();


                if (d != null)
                    itemExist = StringUtil.toString(d.item);
            }


            return itemExist;
        }


        public string qryOEffDate(string item, string productType, string fuMk, string itemCon, string discPartFeat)
        {
            string[] apprMk = new string[] { "6" };

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FGL_ITEM_INFO_HIS d = db.FGL_ITEM_INFO_HIS
                        .Where(x => x.item == item
                        & x.product_type == productType
                        & x.fu_mk == fuMk
                        & x.item_con == itemCon
                        & x.disc_part_feat == discPartFeat
                        & x.exec_action == "A"
                        & x.appr_mk == "6"
                        ).FirstOrDefault();

                    if (d != null)
                        return DateUtil.DatetimeToString(d.effect_date, "yyyy/MM/dd");
                    else
                        return "";

                }
            }
        }


        public int qryUpdateCnt(string item, string productType, string fuMk, string itemCon, string discPartFeat)
        {
            string[] apprMk = new string[] { "6" };

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    int cnt = db.FGL_ITEM_INFO_HIS
                        .Where(x => x.item == item
                        & x.product_type == productType
                        & x.fu_mk == fuMk
                        & x.item_con == itemCon
                        & x.disc_part_feat == discPartFeat
                        & apprMk.Contains(x.appr_mk)
                        ).Count();


                    return cnt;
                }
            }
        }

        public List<OGL00008Model> qryForOGL00008(string productType, string rule56, string fuMk, string effectDateB, string effectDateE)
        {
            bool bProductType = "".Equals(StringUtil.toString(productType)) ? true : false;
            bool bEffectDateB = StringUtil.isEmpty(effectDateB);
            bool bEffectDateE = StringUtil.isEmpty(effectDateE);

            if (bEffectDateB)
                effectDateB = "1900/01/01";

            if (bEffectDateE)
                effectDateE = DateUtil.getCurDate("yyyy/MM/dd");

            DateTime sEffB = Convert.ToDateTime(effectDateB);
            DateTime sEffE = Convert.ToDateTime(effectDateE);
            sEffE = sEffE.AddDays(1);


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from m in db.FGL_ITEM_INFO_HIS.Where(x => x.appr_mk != "6" && (x.item_acct != "" || x.item_acct != null))

                               from codeSmpRule56 in db.SYS_PARA.Where(x => x.SYS_CD == "GL" & x.GRP_ID == "SmpRule56")

                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeApprMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_APPR_MK") on m.appr_mk equals codeApprMk.CODE into psApprMk
                               from xApprMk in psApprMk.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               & (bProductType || (!bProductType & m.product_type == productType))
                               & codeSmpRule56.PARA_ID == rule56
                               & m.fu_mk == fuMk
                               & ((m.fu_mk == "N" && m.item_acct.Substring(0, 2) == codeSmpRule56.RESERVE1) || (m.fu_mk == "Y" && m.item_acct.Substring(0, 2) == codeSmpRule56.RESERVE2))
                               & (bEffectDateB || (!bEffectDateB & m.effect_date >= sEffB))
                               & (bEffectDateE || (!bEffectDateE & m.effect_date < sEffE))
                               select new OGL00008Model
                               {
                                   tempId = m.aply_no,
                                   srceFrom = "暫存檔",
                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType == null ? m.product_type : m.product_type + "." + xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType == null ? m.sys_type : m.sys_type + "." + xSysType.CODE_VALUE,
                                   itemName = m.item_name.Trim(),
                                   itemAcct = m.item_acct,
                                   separatAcct = m.separat_acct,
                                   coiAcct = m.coi_acct,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),

                                   apprMk = m.appr_mk,
                                   apprMkDesc = xApprMk == null ? m.appr_mk : m.appr_mk + "." + xApprMk.CODE_VALUE,
                                   rule56Desc = codeSmpRule56.PARA_VALUE

                               }).ToList();

                    return his;
                }
            }
        }

        public List<OGL00003Model> qryForOGL00006(string apprMk, string updDateB, string updDateE, string item, string effectDateB, string effectDateE)
        {
            bool bUpdDateB = StringUtil.isEmpty(updDateB);
            bool bUpdDateE = StringUtil.isEmpty(updDateE);
            bool bItem = StringUtil.isEmpty(item);
            bool bApprMk = StringUtil.isEmpty(apprMk);
            bool bEffectDateB = StringUtil.isEmpty(effectDateB);
            bool bEffectDateE = StringUtil.isEmpty(effectDateE);

            if (bUpdDateB)
                updDateB = "1900/01/01";

            if (bUpdDateE)
                updDateE = DateUtil.getCurDate("yyyy/MM/dd");

            DateTime sB = Convert.ToDateTime(updDateB);
            DateTime sE = Convert.ToDateTime(updDateE);
            sE = sE.AddDays(1);


            if (bEffectDateB)
                effectDateB = "1900/01/01";

            if (bEffectDateE)
                effectDateE = DateUtil.getCurDate("yyyy/MM/dd");

            DateTime sEffB = Convert.ToDateTime(effectDateB);
            DateTime sEffE = Convert.ToDateTime(effectDateE);
            sEffE = sEffE.AddDays(1);


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from m in db.FGL_ITEM_INFO_HIS
                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeFuMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "FU_MK") on m.fu_mk equals codeFuMk.CODE into psFuMk
                               from xFuMk in psFuMk.DefaultIfEmpty()

                               join codeItemCon in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_CON") on m.item_con equals codeItemCon.CODE into psItemCon
                               from xItemCon in psItemCon.DefaultIfEmpty()

                               join codeProdNoVer in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PROD_NO_VER") on m.prod_no_ver equals codeProdNoVer.CODE into psProdNoVer
                               from xProdNoVer in psProdNoVer.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeItemMainType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_MAIN_TYPE") on m.item_main_type equals codeItemMainType.CODE into psItemMainType
                               from xItemMainType in psItemMainType.DefaultIfEmpty()

                               join codeInvestType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INVEST_TYPE") on m.invest_type equals codeInvestType.CODE into psInvestType
                               from xInvestType in psInvestType.DefaultIfEmpty()

                               join codeInsTerm in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INS_TERM") on m.ins_term equals codeInsTerm.CODE into psInsTerm
                               from xInsTerm in psInsTerm.DefaultIfEmpty()

                               join codeBusiType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "BUSI_TYPE") on m.busi_type equals codeBusiType.CODE into psBusiType
                               from xBusiType in psBusiType.DefaultIfEmpty()

                               join codeComType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "COM_TYPE") on m.com_type equals codeComType.CODE into psComType
                               from xComType in psComType.DefaultIfEmpty()

                               join codeApprMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_APPR_MK") on m.appr_mk equals codeApprMk.CODE into psApprMk
                               from xApprMk in psApprMk.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               & (bApprMk || (!bApprMk && apprMk.Contains(m.appr_mk)))
                               & (bItem || (!bItem & m.item == item))
                               & (bUpdDateB || (!bUpdDateB & ((m.prod_upd_dt >= sB & m.prod_upd_dt < sE) || (m.invest_upd_dt >= sB & m.invest_upd_dt < sE) || (m.acct_upd_dt >= sB & m.acct_upd_dt < sE))))
                               & (bEffectDateB || (!bEffectDateB & m.effect_date >= sEffB))
                               & (bEffectDateE || (!bEffectDateE & m.effect_date < sEffE))
                               select new OGL00003Model
                               {
                                   tempId = m.aply_no,
                                   aplyNo = m.aply_no,
                                   execAction = m.exec_action,
                                   status = m.status,
                                   flag = m.flag,

                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   fuMkDesc = xFuMk.CODE_VALUE,
                                   itemCon = m.item_con,
                                   itemConDesc = xItemCon.CODE_VALUE,
                                   discPartFeat = m.disc_part_feat,

                                   prodNoVer = m.prod_no_ver,
                                   prodNoVerDesc = xProdNoVer.CODE_VALUE,
                                   prodNo = m.prod_no,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType.CODE_VALUE,
                                   itemMainType = m.item_con,
                                   itemMainTypeDesc = xItemMainType.CODE_VALUE,
                                   investType = m.invest_type,
                                   investTypeDesc = xInvestType.CODE_VALUE,
                                   insTerm = m.ins_term,
                                   insTermDesc = xInsTerm.CODE_VALUE,
                                   busiType = m.busi_type,
                                   busiTypeDesc = xBusiType.CODE_VALUE,
                                   comType = m.com_type,
                                   comTypeDesc = xComType.CODE_VALUE,
                                   extSchedulType = m.ext_schedul_type,
                                   pakindDmopType = m.pakind_dmop_type,
                                   lodprmType = m.lodprm_type,
                                   lodprmTypeDesc = xLodprmType.CODE_VALUE,
                                   healthMgrType = m.health_mgr_type,
                                   coiType = m.coi_type,
                                   itemName = m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt.Trim(),
                                   itemAcct = m.item_acct,
                                   separatAcct = m.separat_acct,
                                   coiAcct = m.coi_acct,
                                   effMk = m.eff_mk,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),
                                   effectYY = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date),
                                   effectMM = m.effect_date == null ? "" : SqlFunctions.DatePart("m", m.effect_date).ToString(),
                                   effectDD = m.effect_date == null ? "" : SqlFunctions.DatePart("day", m.effect_date).ToString(),
                                   apprMk = m.appr_mk,
                                   apprMkDesc = m.appr_mk + "." + xApprMk.CODE_VALUE,
                                   prodUpId = m.prod_upd_id,
                                   prodUpDt = m.prod_upd_dt == null ? "" : SqlFunctions.DateName("year", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_upd_dt).Trim(),
                                   prodApprId = m.prod_appr_id,
                                   prodApprDt = m.prod_appr_dt == null ? "" : SqlFunctions.DateName("year", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_appr_dt).Trim(),
                                   investUpId = m.invest_upd_id,
                                   investUpDt = m.invest_upd_dt == null ? "" : SqlFunctions.DateName("year", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_upd_dt).Trim(),
                                   investApprId = m.invest_appr_id,
                                   investApprDt = m.invest_appr_dt == null ? "" : SqlFunctions.DateName("year", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_appr_dt).Trim(),
                                   acctUpId = m.acct_upd_id,
                                   acctUpDt = m.acct_upd_dt == null ? "" : SqlFunctions.DateName("year", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_upd_dt).Trim(),
                                   acctApprId = m.acct_appr_id,
                                   acctApprDt = m.acct_appr_dt == null ? "" : SqlFunctions.DateName("year", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_appr_dt).Trim(),
                                   createId = m.create_id,
                                   createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.create_dt).Trim()

                               }).ToList();

                    return his;
                }
            }
        }

        public List<OGL00003Model> qryForOGL00007(string updDateB, string updDateE, string item)
        {
            bool bUpdDateB = StringUtil.isEmpty(updDateB);
            bool bUpdDateE = StringUtil.isEmpty(updDateE);
            bool bItem = StringUtil.isEmpty(item);

            string[] apprMkF = new string[] { "0", "1", "6" };

            if (bUpdDateB)
                updDateB = "1900/01/01";

            if (bUpdDateE)
                updDateE = DateUtil.getCurDate("yyyy/MM/dd");

            DateTime sB = Convert.ToDateTime(updDateB);
            DateTime sE = Convert.ToDateTime(updDateE);
            sE = sE.AddDays(1);


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from m in db.FGL_ITEM_INFO_HIS
                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeFuMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "FU_MK") on m.fu_mk equals codeFuMk.CODE into psFuMk
                               from xFuMk in psFuMk.DefaultIfEmpty()

                               join codeItemCon in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_CON") on m.item_con equals codeItemCon.CODE into psItemCon
                               from xItemCon in psItemCon.DefaultIfEmpty()

                               join codeProdNoVer in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PROD_NO_VER") on m.prod_no_ver equals codeProdNoVer.CODE into psProdNoVer
                               from xProdNoVer in psProdNoVer.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeItemMainType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_MAIN_TYPE") on m.item_main_type equals codeItemMainType.CODE into psItemMainType
                               from xItemMainType in psItemMainType.DefaultIfEmpty()

                               join codeInvestType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INVEST_TYPE") on m.invest_type equals codeInvestType.CODE into psInvestType
                               from xInvestType in psInvestType.DefaultIfEmpty()

                               join codeInsTerm in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INS_TERM") on m.ins_term equals codeInsTerm.CODE into psInsTerm
                               from xInsTerm in psInsTerm.DefaultIfEmpty()

                               join codeBusiType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "BUSI_TYPE") on m.busi_type equals codeBusiType.CODE into psBusiType
                               from xBusiType in psBusiType.DefaultIfEmpty()

                               join codeComType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "COM_TYPE") on m.com_type equals codeComType.CODE into psComType
                               from xComType in psComType.DefaultIfEmpty()

                               join codeApprMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_APPR_MK") on m.appr_mk equals codeApprMk.CODE into psApprMk
                               from xApprMk in psApprMk.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               //& m.prod_appr_id != null
                               & m.flag == "N"
                              // & (m.eff_mk == "N" || m.eff_mk == null)
                               & !apprMkF.Contains(m.appr_mk)
                               & (bItem || (!bItem & m.item == item))
                               & (bUpdDateB || (!bUpdDateB & ((m.prod_upd_dt >= sB & m.prod_upd_dt < sE) || (m.invest_upd_dt >= sB & m.invest_upd_dt < sE) || (m.acct_upd_dt >= sB & m.acct_upd_dt < sE))))

                               select new OGL00003Model
                               {
                                   tempId = m.item + "|" + m.product_type + "|" + m.fu_mk + "|" + m.item_con + "|" + m.disc_part_feat,
                                   aplyNo = m.aply_no,
                                   execAction = m.exec_action,
                                   status = m.status,
                                   flag = m.flag,

                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   fuMkDesc = xFuMk.CODE_VALUE,
                                   itemCon = m.item_con,
                                   itemConDesc = xItemCon.CODE_VALUE,
                                   discPartFeat = m.disc_part_feat,

                                   prodNoVer = m.prod_no_ver,
                                   prodNoVerDesc = xProdNoVer.CODE_VALUE,
                                   prodNo = m.prod_no,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType.CODE_VALUE,
                                   itemMainType = m.item_con,
                                   itemMainTypeDesc = xItemMainType.CODE_VALUE,
                                   investType = m.invest_type,
                                   investTypeDesc = xInvestType.CODE_VALUE,
                                   insTerm = m.ins_term,
                                   insTermDesc = xInsTerm.CODE_VALUE,
                                   busiType = m.busi_type,
                                   busiTypeDesc = xBusiType.CODE_VALUE,
                                   comType = m.com_type,
                                   comTypeDesc = xComType.CODE_VALUE,
                                   extSchedulType = m.ext_schedul_type,
                                   pakindDmopType = m.pakind_dmop_type,
                                   lodprmType = m.lodprm_type,
                                   lodprmTypeDesc = xLodprmType.CODE_VALUE,
                                   healthMgrType = m.health_mgr_type,
                                   coiType = m.coi_type,
                                   itemName = m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt.Trim(),
                                   itemAcct = m.item_acct,
                                   separatAcct = m.separat_acct,
                                   coiAcct = m.coi_acct,
                                   effMk = m.eff_mk,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),
                                   effectYY = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date),
                                   effectMM = m.effect_date == null ? "" : SqlFunctions.DatePart("m", m.effect_date).ToString(),
                                   effectDD = m.effect_date == null ? "" : SqlFunctions.DatePart("day", m.effect_date).ToString(),
                                   apprMk = m.appr_mk,
                                   apprMkDesc = m.appr_mk + "." + xApprMk.CODE_VALUE,
                                   prodUpId = m.prod_upd_id,
                                   prodUpDt = m.prod_upd_dt == null ? "" : SqlFunctions.DateName("year", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_upd_dt).Trim(),
                                   prodApprId = m.prod_appr_id,
                                   prodApprDt = m.prod_appr_dt == null ? "" : SqlFunctions.DateName("year", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_appr_dt).Trim(),
                                   investUpId = m.invest_upd_id,
                                   investUpDt = m.invest_upd_dt == null ? "" : SqlFunctions.DateName("year", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_upd_dt).Trim(),
                                   investApprId = m.invest_appr_id,
                                   investApprDt = m.invest_appr_dt == null ? "" : SqlFunctions.DateName("year", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_appr_dt).Trim(),
                                   acctUpId = m.acct_upd_id,
                                   acctUpDt = m.acct_upd_dt == null ? "" : SqlFunctions.DateName("year", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_upd_dt).Trim(),
                                   acctApprId = m.acct_appr_id,
                                   acctApprDt = m.acct_appr_dt == null ? "" : SqlFunctions.DateName("year", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_appr_dt).Trim(),
                                   createId = m.create_id,
                                   createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.create_dt).Trim()

                               }).ToList();

                    return his;
                }
            }
        }


        public List<OGL00003Model> qryByApprMk(string[] apprMk, string qryPgm, string updDateB, string updDateE, string item, bool bContainFinish)
        {
            bool bUpdDateB = StringUtil.isEmpty(updDateB);
            bool bUpdDateE = StringUtil.isEmpty(updDateE);
            bool bItem = StringUtil.isEmpty(item);
            bool bApprMk = apprMk == null ? true : false;

            string[] apprMkF = new string[] { "0", "6" };

            if (bUpdDateB)
                updDateB = "1900/01/01";

            if (bUpdDateE)
                updDateE = DateUtil.getCurDate("yyyy/MM/dd");

            DateTime sB = Convert.ToDateTime(updDateB);
            DateTime sE = Convert.ToDateTime(updDateE);
            sE = sE.AddDays(1);


            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from m in db.FGL_ITEM_INFO_HIS
                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeFuMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "FU_MK") on m.fu_mk equals codeFuMk.CODE into psFuMk
                               from xFuMk in psFuMk.DefaultIfEmpty()

                               join codeItemCon in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_CON") on m.item_con equals codeItemCon.CODE into psItemCon
                               from xItemCon in psItemCon.DefaultIfEmpty()

                               join codeProdNoVer in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PROD_NO_VER") on m.prod_no_ver equals codeProdNoVer.CODE into psProdNoVer
                               from xProdNoVer in psProdNoVer.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeItemMainType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_MAIN_TYPE") on m.item_main_type equals codeItemMainType.CODE into psItemMainType
                               from xItemMainType in psItemMainType.DefaultIfEmpty()

                               join codeInvestType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INVEST_TYPE") on m.invest_type equals codeInvestType.CODE into psInvestType
                               from xInvestType in psInvestType.DefaultIfEmpty()

                               join codeInsTerm in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INS_TERM") on m.ins_term equals codeInsTerm.CODE into psInsTerm
                               from xInsTerm in psInsTerm.DefaultIfEmpty()

                               join codeBusiType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "BUSI_TYPE") on m.busi_type equals codeBusiType.CODE into psBusiType
                               from xBusiType in psBusiType.DefaultIfEmpty()

                               join codeComType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "COM_TYPE") on m.com_type equals codeComType.CODE into psComType
                               from xComType in psComType.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               & (bApprMk || (!bApprMk && apprMk.Contains(m.appr_mk)))
                               & (bItem || (!bItem & m.item == item))
                               & (bUpdDateB || (!bUpdDateB & m.create_dt >= sB))
                               & (bUpdDateE || (!bUpdDateE & m.create_dt < sE))
                               & (bContainFinish || (!bContainFinish && !apprMkF.Contains(m.appr_mk)))
                               select new OGL00003Model
                               {
                                   tempId = m.item + "|" + m.product_type + "|" + m.fu_mk + "|" + m.item_con + "|" + m.disc_part_feat,
                                   aplyNo = m.aply_no,
                                   execAction = m.exec_action,
                                   status = m.status,
                                   flag = m.flag,

                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   fuMkDesc = xFuMk.CODE_VALUE,
                                   itemCon = m.item_con,
                                   itemConDesc = xItemCon.CODE_VALUE,
                                   discPartFeat = m.disc_part_feat,

                                   prodNoVer = m.prod_no_ver,
                                   prodNoVerDesc = xProdNoVer.CODE_VALUE,
                                   prodNo = m.prod_no,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType.CODE_VALUE,
                                   itemMainType = m.item_main_type,
                                   itemMainTypeDesc = xItemMainType.CODE_VALUE,
                                   investType = m.invest_type,
                                   investTypeDesc = xInvestType.CODE_VALUE,
                                   insTerm = m.ins_term,
                                   insTermDesc = xInsTerm.CODE_VALUE,
                                   busiType = m.busi_type,
                                   busiTypeDesc = xBusiType.CODE_VALUE,
                                   comType = m.com_type,
                                   comTypeDesc = xComType.CODE_VALUE,
                                   extSchedulType = m.ext_schedul_type,
                                   pakindDmopType = m.pakind_dmop_type,
                                   lodprmType = m.lodprm_type,
                                   lodprmTypeDesc = xLodprmType.CODE_VALUE,
                                   healthMgrType = m.health_mgr_type,
                                   coiType = m.coi_type,
                                   itemName = m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt.Trim(),
                                   itemAcct = m.item_acct,
                                   separatAcct = m.separat_acct,
                                   coiAcct = m.coi_acct,
                                   effMk = m.eff_mk,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),
                                   effectYY = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date),
                                   effectMM = m.effect_date == null ? "" : SqlFunctions.DatePart("m", m.effect_date).ToString(),
                                   effectDD = m.effect_date == null ? "" : SqlFunctions.DatePart("day", m.effect_date).ToString(),
                                   apprMk = m.appr_mk,
                                   prodUpId = m.prod_upd_id,
                                   prodUpDt = m.prod_upd_dt == null ? "" : SqlFunctions.DateName("year", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_upd_dt).Trim(),
                                   prodApprId = m.prod_appr_id,
                                   prodApprDt = m.prod_appr_dt == null ? "" : SqlFunctions.DateName("year", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_appr_dt).Trim(),
                                   investUpId = m.invest_upd_id,
                                   investUpDt = m.invest_upd_dt == null ? "" : SqlFunctions.DateName("year", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_upd_dt).Trim(),
                                   investApprId = m.invest_appr_id,
                                   investApprDt = m.invest_appr_dt == null ? "" : SqlFunctions.DateName("year", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_appr_dt).Trim(),
                                   acctUpId = m.acct_upd_id,
                                   acctUpDt = m.acct_upd_dt == null ? "" : SqlFunctions.DateName("year", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_upd_dt).Trim(),
                                   acctApprId = m.acct_appr_id,
                                   acctApprDt = m.acct_appr_dt == null ? "" : SqlFunctions.DateName("year", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_appr_dt).Trim(),
                                   createId = m.create_id,
                                   createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.create_dt).Trim()

                               }).ToList();

                    return his;
                }
            }
        }


        /// <summary>
        /// 以"覆核單號"進行查詢
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public OGL00003Model qryAplyNo(string aplyNo)
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
                    var his = (from m in db.FGL_ITEM_INFO_HIS
                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeFuMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "FU_MK") on m.fu_mk equals codeFuMk.CODE into psFuMk
                               from xFuMk in psFuMk.DefaultIfEmpty()

                               join codeItemCon in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_CON") on m.item_con equals codeItemCon.CODE into psItemCon
                               from xItemCon in psItemCon.DefaultIfEmpty()

                               join codeProdNoVer in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PROD_NO_VER") on m.prod_no_ver equals codeProdNoVer.CODE into psProdNoVer
                               from xProdNoVer in psProdNoVer.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeItemMainType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_MAIN_TYPE") on m.item_main_type equals codeItemMainType.CODE into psItemMainType
                               from xItemMainType in psItemMainType.DefaultIfEmpty()

                               join codeInvestType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INVEST_TYPE") on m.invest_type equals codeInvestType.CODE into psInvestType
                               from xInvestType in psInvestType.DefaultIfEmpty()

                               join codeInsTerm in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INS_TERM") on m.ins_term equals codeInsTerm.CODE into psInsTerm
                               from xInsTerm in psInsTerm.DefaultIfEmpty()

                               join codeBusiType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "BUSI_TYPE") on m.busi_type equals codeBusiType.CODE into psBusiType
                               from xBusiType in psBusiType.DefaultIfEmpty()

                               join codeComType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "COM_TYPE") on m.com_type equals codeComType.CODE into psComType
                               from xComType in psComType.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               & m.aply_no == aplyNo
                               select new OGL00003Model
                               {
                                   aplyNo = m.aply_no,
                                   execAction = m.exec_action,
                                   status = m.status,
                                   flag = m.flag,

                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   fuMkDesc = xFuMk.CODE_VALUE,
                                   itemCon = m.item_con,
                                   itemConDesc = xItemCon.CODE_VALUE,
                                   discPartFeat = m.disc_part_feat,

                                   prodNoVer = m.prod_no_ver,
                                   prodNoVerDesc = xProdNoVer.CODE_VALUE,
                                   prodNo = m.prod_no,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType.CODE_VALUE,
                                   itemMainType = m.item_main_type,
                                   itemMainTypeDesc = xItemMainType.CODE_VALUE,
                                   investType = m.invest_type == null ? "" : m.invest_type,
                                   investTypeDesc = xInvestType.CODE_VALUE,
                                   insTerm = m.ins_term == null ? "" : m.ins_term,
                                   insTermDesc = xInsTerm.CODE_VALUE,
                                   busiType = m.busi_type == null ? "" : m.busi_type,
                                   busiTypeDesc = xBusiType.CODE_VALUE,
                                   comType = m.com_type == null ? "" : m.com_type,
                                   comTypeDesc = xComType.CODE_VALUE,
                                   extSchedulType = m.ext_schedul_type == null ? "" : m.ext_schedul_type,
                                   pakindDmopType = m.pakind_dmop_type == null ? "" : m.pakind_dmop_type,
                                   lodprmType = m.lodprm_type == null ? "" : m.lodprm_type,
                                   lodprmTypeDesc = xLodprmType.CODE_VALUE,
                                   healthMgrType = m.health_mgr_type,
                                   coiType = m.coi_type,
                                   itemName = m.item_name == null ? "" : m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt == null ? "" : m.item_name_shrt.Trim(),
                                   itemAcct = m.item_acct == null ? "" : m.item_acct,
                                   separatAcct = m.separat_acct == null ? "" : m.separat_acct,
                                   coiAcct = m.coi_acct == null ? "" : m.coi_acct,
                                   effMk = m.eff_mk == null ? "" : m.eff_mk,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),
                                   effectYY = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date),
                                   effectMM = m.effect_date == null ? "" : SqlFunctions.DatePart("m", m.effect_date).ToString(),
                                   effectDD = m.effect_date == null ? "" : SqlFunctions.DatePart("day", m.effect_date).ToString(),
                                   apprMk = m.appr_mk,
                                   prodUpId = m.prod_upd_id,
                                   prodUpDt = m.prod_upd_dt == null ? "" : SqlFunctions.DateName("year", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_upd_dt).Trim(),
                                   prodApprId = m.prod_appr_id,
                                   prodApprDt = m.prod_appr_dt == null ? "" : SqlFunctions.DateName("year", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_appr_dt).Trim(),
                                   investUpId = m.invest_upd_id,
                                   investUpDt = m.invest_upd_dt == null ? "" : SqlFunctions.DateName("year", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_upd_dt).Trim(),
                                   investApprId = m.invest_appr_id,
                                   investApprDt = m.invest_appr_dt == null ? "" : SqlFunctions.DateName("year", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_appr_dt).Trim(),
                                   acctUpId = m.acct_upd_id,
                                   acctUpDt = m.acct_upd_dt == null ? "" : SqlFunctions.DateName("year", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_upd_dt).Trim(),
                                   acctApprId = m.acct_appr_id,
                                   acctApprDt = m.acct_appr_dt == null ? "" : SqlFunctions.DateName("year", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_appr_dt).Trim(),
                                   createId = m.create_id,
                                   createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.create_dt).Trim()

                               }).FirstOrDefault();

                    return his;
                }
            }

        }


        /// <summary>
        /// 以"險種代號"+"險種類別"+"外幣註記"+"合約分類"+"裁量參與特性"進行查詢
        /// </summary>
        /// <param name="item"></param>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <returns></returns>
        public OGL00003Model qryItem(string item)
           // public OGL00003Model qryItem(string item, string productType, string fuMk, string itemCon, string discPartFeat)
        {
            //productType = StringUtil.toString(productType);
            //fuMk = StringUtil.toString(fuMk);
            //itemCon = StringUtil.toString(itemCon);
            //discPartFeat = StringUtil.toString(discPartFeat);

            string[] apprMk = new string[] { "0", "6" };
            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from  m in db.FGL_ITEM_INFO_HIS
                               join codeProductType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PRODUCT_TYPE") on m.product_type equals codeProductType.CODE into psProductType
                               from xProductType in psProductType.DefaultIfEmpty()

                               join codeFuMk in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "FU_MK") on m.fu_mk equals codeFuMk.CODE into psFuMk
                               from xFuMk in psFuMk.DefaultIfEmpty()

                               join codeItemCon in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_CON") on m.item_con equals codeItemCon.CODE into psItemCon
                               from xItemCon in psItemCon.DefaultIfEmpty()

                               join codeProdNoVer in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "PROD_NO_VER") on m.prod_no_ver equals codeProdNoVer.CODE into psProdNoVer
                               from xProdNoVer in psProdNoVer.DefaultIfEmpty()

                               join codeSysType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "SYS_TYPE") on m.sys_type equals codeSysType.CODE into psSysType
                               from xSysType in psSysType.DefaultIfEmpty()

                               join codeItemMainType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "ITEM_MAIN_TYPE") on m.item_main_type equals codeItemMainType.CODE into psItemMainType
                               from xItemMainType in psItemMainType.DefaultIfEmpty()

                               join codeInvestType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INVEST_TYPE") on m.invest_type equals codeInvestType.CODE into psInvestType
                               from xInvestType in psInvestType.DefaultIfEmpty()

                               join codeInsTerm in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "INS_TERM") on m.ins_term equals codeInsTerm.CODE into psInsTerm
                               from xInsTerm in psInsTerm.DefaultIfEmpty()

                               join codeBusiType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "BUSI_TYPE") on m.busi_type equals codeBusiType.CODE into psBusiType
                               from xBusiType in psBusiType.DefaultIfEmpty()

                               join codeComType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "COM_TYPE") on m.com_type equals codeComType.CODE into psComType
                               from xComType in psComType.DefaultIfEmpty()

                               join codeLodprmType in db.SYS_CODE.Where(x => x.SYS_CD == "GL" & x.CODE_TYPE == "LODPRM_TYPE") on m.lodprm_type equals codeLodprmType.CODE into psLodprmType
                               from xLodprmType in psLodprmType.DefaultIfEmpty()

                               where 1 == 1
                               & m.item == item
                               //& m.product_type == productType
                               //& m.fu_mk == fuMk
                               //& m.item_con == itemCon
                               //& m.disc_part_feat == discPartFeat
                               & !apprMk.Contains(m.appr_mk)
                                
                               select new OGL00003Model
                               {
                                   aplyNo = m.aply_no,
                                   execAction = m.exec_action,
                                   status = m.status,
                                   flag = m.flag,

                                   item = m.item,
                                   productType = m.product_type,
                                   productTypeDesc = xProductType.CODE_VALUE,
                                   fuMk = m.fu_mk,
                                   fuMkDesc = xFuMk.CODE_VALUE,
                                   itemCon = m.item_con,
                                   itemConDesc = xItemCon.CODE_VALUE,
                                   discPartFeat = m.disc_part_feat,

                                   prodNoVer = m.prod_no_ver,
                                   prodNoVerDesc = xProdNoVer.CODE_VALUE,
                                   prodNo = m.prod_no,
                                   sysType = m.sys_type,
                                   sysTypeDesc = xSysType.CODE_VALUE,
                                   itemMainType = m.item_main_type,
                                   itemMainTypeDesc = xItemMainType.CODE_VALUE,
                                   investType = m.invest_type,
                                   investTypeDesc = xInvestType.CODE_VALUE,
                                   insTerm = m.ins_term,
                                   insTermDesc = xInsTerm.CODE_VALUE,
                                   busiType = m.busi_type,
                                   busiTypeDesc = xBusiType.CODE_VALUE,
                                   comType = m.com_type,
                                   comTypeDesc = xComType.CODE_VALUE,
                                   extSchedulType = m.ext_schedul_type,
                                   pakindDmopType = m.pakind_dmop_type,
                                   lodprmType = m.lodprm_type,
                                   lodprmTypeDesc = xLodprmType.CODE_VALUE,
                                   healthMgrType = m.health_mgr_type,
                                   coiType = m.coi_type,
                                   itemName = m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt.Trim(),
                                   itemAcct = m.item_acct,
                                   separatAcct = m.separat_acct,
                                   coiAcct = m.coi_acct,
                                   effMk = m.eff_mk,
                                   effectDate = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date) + "/" +
                                                            SqlFunctions.DatePart("m", m.effect_date) + "/" +
                                                            SqlFunctions.DateName("day", m.effect_date).Trim(),
                                   effectYY = m.effect_date == null ? "" : SqlFunctions.DateName("year", m.effect_date),
                                   effectMM = m.effect_date == null ? "" : SqlFunctions.DatePart("m", m.effect_date).ToString(),
                                   effectDD = m.effect_date == null ? "" : SqlFunctions.DatePart("day", m.effect_date).ToString(),
                                   apprMk = m.appr_mk,
                                   prodUpId = m.prod_upd_id,
                                   prodUpDt = m.prod_upd_dt == null ? "" : SqlFunctions.DateName("year", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_upd_dt).Trim(),
                                   prodApprId = m.prod_appr_id,
                                   prodApprDt = m.prod_appr_dt == null ? "" : SqlFunctions.DateName("year", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.prod_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.prod_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.prod_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.prod_appr_dt).Trim(),
                                   investUpId = m.invest_upd_id,
                                   investUpDt = m.invest_upd_dt == null ? "" : SqlFunctions.DateName("year", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_upd_dt).Trim(),
                                   investApprId = m.invest_appr_id,
                                   investApprDt = m.invest_appr_dt == null ? "" : SqlFunctions.DateName("year", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.invest_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.invest_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.invest_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.invest_appr_dt).Trim(),
                                   acctUpId = m.acct_upd_id,
                                   acctUpDt = m.acct_upd_dt == null ? "" : SqlFunctions.DateName("year", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_upd_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_upd_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_upd_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_upd_dt).Trim(),
                                   acctApprId = m.acct_appr_id,
                                   acctApprDt = m.acct_appr_dt == null ? "" : SqlFunctions.DateName("year", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.acct_appr_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.acct_appr_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.acct_appr_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.acct_appr_dt).Trim(),
                                   createId = m.create_id,
                                   createDt = m.create_dt == null ? "" : SqlFunctions.DateName("year", m.create_dt) + "/" +
                                                            SqlFunctions.DatePart("m", m.create_dt) + "/" +
                                                            SqlFunctions.DateName("day", m.create_dt).Trim() + " " +
                                                                     SqlFunctions.DateName("hh", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("n", m.create_dt).Trim() + ":" +
                                                                     SqlFunctions.DateName("s", m.create_dt).Trim()

                               }).FirstOrDefault();

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
        public void insert(FGL_ITEM_INFO_HIS his, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_INFO_HIS
                   ([APLY_NO]
           ,[ITEM]
           ,[PRODUCT_TYPE]
           ,[FU_MK]
           ,[ITEM_CON]
           ,[DISC_PART_FEAT]
           ,[EXEC_ACTION]
           ,[PROD_NO_VER]
           ,[PROD_NO]
           ,[SYS_TYPE]
           ,[ITEM_MAIN_TYPE]
           ,[INVEST_TYPE]
           ,[INS_TERM]
           ,[BUSI_TYPE]
           ,[COM_TYPE]
           ,[EXT_SCHEDUL_TYPE]
           ,[PAKIND_DMOP_TYPE]
           ,[LODPRM_TYPE]
           ,HEALTH_MGR_TYPE
           ,COI_TYPE
           ,[ITEM_NAME]
           ,[ITEM_NAME_SHRT]
           ,[ITEM_ACCT]
           ,[SEPARAT_ACCT]
           ,[COI_ACCT]
           ,[EFFECT_DATE]
           ,[EFF_MK]
           ,[APPR_MK]
           ,[PROD_UPD_ID]
           ,[PROD_UPD_DT]
           ,[PROD_APPR_ID]
           ,[PROD_APPR_DT]
           ,[INVEST_UPD_ID]
           ,[INVEST_UPD_DT]
           ,[INVEST_APPR_ID]
           ,[INVEST_APPR_DT]
           ,[ACCT_UPD_ID]
           ,[ACCT_UPD_DT]
           ,[ACCT_APPR_ID]
           ,[ACCT_APPR_DT]
           ,[CREATE_ID]
           ,[CREATE_DT]
           ,[STATUS]
           ,[FLAG])
VALUES
(@APLY_NO
,@ITEM
,@PRODUCT_TYPE
,@FU_MK
,@ITEM_CON
,@DISC_PART_FEAT
,@EXEC_ACTION
,@PROD_NO_VER
,@PROD_NO
,@SYS_TYPE
,@ITEM_MAIN_TYPE
,@INVEST_TYPE
,@INS_TERM
,@BUSI_TYPE
,@COM_TYPE
,@EXT_SCHEDUL_TYPE
,@PAKIND_DMOP_TYPE
,@LODPRM_TYPE
,@HEALTH_MGR_TYPE
,@COI_TYPE
,@ITEM_NAME
,@ITEM_NAME_SHRT
,@ITEM_ACCT
,@SEPARAT_ACCT
,@COI_ACCT
,@EFFECT_DATE
,@EFF_MK
,@APPR_MK
,@PROD_UPD_ID
,@PROD_UPD_DT
,@PROD_APPR_ID
,@PROD_APPR_DT
,@INVEST_UPD_ID
,@INVEST_UPD_DT
,@INVEST_APPR_ID
,@INVEST_APPR_DT
,@ACCT_UPD_ID
,@ACCT_UPD_DT
,@ACCT_APPR_ID
,@ACCT_APPR_DT
,@CREATE_ID
,@CREATE_DT
,@STATUS
,@FLAG)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@APLY_NO", StringUtil.toString(his.aply_no));
                cmd.Parameters.AddWithValue("@EXEC_ACTION", StringUtil.toString(his.exec_action));

                cmd.Parameters.AddWithValue("@ITEM", StringUtil.toString(his.item));
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", StringUtil.toString(his.product_type));
                cmd.Parameters.AddWithValue("@FU_MK", StringUtil.toString(his.fu_mk));
                cmd.Parameters.AddWithValue("@ITEM_CON", StringUtil.toString(his.item_con));
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", StringUtil.toString(his.disc_part_feat));

                cmd.Parameters.AddWithValue("@PROD_NO_VER", his.prod_no_ver);
                cmd.Parameters.AddWithValue("@PROD_NO", his.prod_no);
                cmd.Parameters.AddWithValue("@SYS_TYPE", his.sys_type);
                cmd.Parameters.AddWithValue("@ITEM_MAIN_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.item_main_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.invest_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INS_TERM", System.Data.SqlDbType.VarChar).Value = (Object)his.ins_term ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@BUSI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.busi_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.com_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EXT_SCHEDUL_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.ext_schedul_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.pakind_dmop_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@LODPRM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.lodprm_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@HEALTH_MGR_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.health_mgr_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.coi_type ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@ITEM_NAME", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.item_name) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_NAME_SHRT", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.item_name_shrt) ?? DBNull.Value;    //add by daiyu 20191129

                cmd.Parameters.AddWithValue("@ITEM_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.item_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@SEPARAT_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.separat_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.coi_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EFFECT_DATE", System.Data.SqlDbType.DateTime).Value = (Object)his.effect_date ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EFF_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.eff_mk ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@APPR_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.appr_mk ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.prod_upd_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.prod_upd_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.prod_appr_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_APPR_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.prod_appr_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.invest_upd_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.invest_upd_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.invest_appr_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_APPR_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.invest_appr_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.acct_upd_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)his.acct_upd_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.acct_appr_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_APPR_DT" , System.Data.SqlDbType.DateTime).Value = (System.Object)his.acct_appr_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@CREATE_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.create_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@CREATE_DT", System.Data.SqlDbType.DateTime).Value = (System.Object)his.create_dt ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@STATUS", System.Data.SqlDbType.VarChar).Value = (System.Object)his.status ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@FLAG", System.Data.SqlDbType.VarChar).Value = (System.Object)his.flag ?? DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }

        




        /// <summary>
        /// 異動"覆核註記"
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="apprMk"></param>
        /// <param name="execPgm"></param>
        /// <param name="uId"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateApprMk(string aplyNo, string apprMk, string execPgm, string uId, string execType,
            SqlConnection conn, SqlTransaction transaction)
        {
            

            SqlCommand command = conn.CreateCommand();


            command.Connection = conn;
            command.Transaction = transaction;

            try
            {
                string sql = "";
                sql = @"update FGL_ITEM_INFO_HIS set APPR_MK = @APPR_MK ";

                switch (execPgm)
                {
                    case "OGL00003":
                        sql += " ,PROD_UPD_DT = @PROD_UPD_DT, PROD_UPD_ID = @PROD_UPD_ID ";

                        command.Parameters.AddWithValue("@PROD_UPD_DT", DateTime.Now);
                        command.Parameters.AddWithValue("@PROD_UPD_ID", uId);
                        break;

                    case "OGL00003A":
                        sql += " ,PROD_APPR_DT = @PROD_APPR_DT, PROD_APPR_ID = @PROD_APPR_ID ";

                        if ("R".Equals(execType))
                        {
                            command.Parameters.AddWithValue("@PROD_APPR_DT", DBNull.Value);
                            command.Parameters.AddWithValue("@PROD_APPR_ID", "");
                        }
                        else {
                            command.Parameters.AddWithValue("@PROD_APPR_DT", DateTime.Now);
                            command.Parameters.AddWithValue("@PROD_APPR_ID", uId);
                        }
                        
                        break;
                    case "OGL00004A":
                        sql += " ,INVEST_APPR_DT = @INVEST_APPR_DT, INVEST_APPR_ID = @INVEST_APPR_ID ";

                        if ("R".Equals(execType))
                        {
                            command.Parameters.AddWithValue("@INVEST_APPR_DT", DBNull.Value);
                            command.Parameters.AddWithValue("@INVEST_APPR_ID", "");
                        }
                        else {
                            command.Parameters.AddWithValue("@INVEST_APPR_DT", DateTime.Now);
                            command.Parameters.AddWithValue("@INVEST_APPR_ID", uId);
                        }

                            
                        break;
                    case "OGL00005":
                        sql += " ,ACCT_UPD_DT = @ACCT_UPD_DT, ACCT_UPD_ID = @ACCT_UPD_ID ";

                        command.Parameters.AddWithValue("@ACCT_UPD_DT", DateTime.Now);
                        command.Parameters.AddWithValue("@ACCT_UPD_ID", uId);
                        break;
                    case "OGL00005A":
                        sql += " ,ACCT_APPR_DT = @ACCT_APPR_DT, ACCT_APPR_ID = @ACCT_APPR_ID ";

                        if ("R".Equals(execType))
                        {
                            command.Parameters.AddWithValue("@ACCT_APPR_DT", DBNull.Value);
                            command.Parameters.AddWithValue("@ACCT_APPR_ID", "");
                        }
                        else {
                            command.Parameters.AddWithValue("@ACCT_APPR_DT", DateTime.Now);
                            command.Parameters.AddWithValue("@ACCT_APPR_ID", uId);
                        }
                            
                        break;
                }

                sql += " WHERE APLY_NO = @APLY_NO ";


                command.CommandText = sql;
                command.Parameters.AddWithValue("@APLY_NO", aplyNo);
                command.Parameters.AddWithValue("@APPR_MK", apprMk);

                int cnt = command.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }


        /// <summary>
        /// "商品資料維護"的異動
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateForProd(FGL_ITEM_INFO_HIS his, SqlConnection conn, SqlTransaction transaction)
        {


            SqlCommand cmd = conn.CreateCommand();


            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                string sql = "";
                sql = @"
UPDATE FGL_ITEM_INFO_HIS 
  SET PROD_NO_VER = @PROD_NO_VER,
      PROD_NO = @PROD_NO,
      SYS_TYPE = @SYS_TYPE,
      ITEM_MAIN_TYPE = @ITEM_MAIN_TYPE,
      INVEST_TYPE = @INVEST_TYPE,
      INS_TERM = @INS_TERM,
      BUSI_TYPE = @BUSI_TYPE,
      COM_TYPE = @COM_TYPE,
      EXT_SCHEDUL_TYPE = @EXT_SCHEDUL_TYPE,
      PAKIND_DMOP_TYPE = @PAKIND_DMOP_TYPE,
      LODPRM_TYPE = @LODPRM_TYPE,
      HEALTH_MGR_TYPE = @HEALTH_MGR_TYPE,
      COI_TYPE = @COI_TYPE,
      ITEM_NAME = @ITEM_NAME,
      APPR_MK = @APPR_MK,
      PROD_UPD_ID = @PROD_UPD_ID,
      PROD_UPD_DT = @PROD_UPD_DT
WHERE APLY_NO = @APLY_NO";



                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", his.aply_no);

                cmd.Parameters.AddWithValue("@PROD_NO_VER", his.prod_no_ver);
                cmd.Parameters.AddWithValue("@PROD_NO", his.prod_no);
                cmd.Parameters.AddWithValue("@SYS_TYPE", his.sys_type);

                cmd.Parameters.AddWithValue("@ITEM_MAIN_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.item_main_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.invest_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INS_TERM", System.Data.SqlDbType.VarChar).Value = (Object)his.ins_term ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@BUSI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.busi_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.com_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EXT_SCHEDUL_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.ext_schedul_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.pakind_dmop_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@LODPRM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.lodprm_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@HEALTH_MGR_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.health_mgr_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.coi_type ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_NAME", System.Data.SqlDbType.VarChar).Value = (Object)his.item_name ?? DBNull.Value;

                cmd.Parameters.AddWithValue("@APPR_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.appr_mk ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.prod_upd_id  ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.prod_upd_dt ?? DBNull.Value;


                int cnt = cmd.ExecuteNonQuery();


                return cnt;
            }
            catch (Exception e)
            {

                throw e;
            }

        }



        /// <summary>
        /// "投資交易商品資料維護"的異動
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateForInvest(FGL_ITEM_INFO_HIS his, SqlConnection conn, SqlTransaction transaction)
        {
            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                string sql = "";
                sql = @"
UPDATE FGL_ITEM_INFO_HIS 
  SET APPR_MK = @APPR_MK,
      INVEST_UPD_ID = @INVEST_UPD_ID,
      INVEST_UPD_DT = @INVEST_UPD_DT
WHERE APLY_NO = @APLY_NO";


                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", his.aply_no);

                cmd.Parameters.AddWithValue("@APPR_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.appr_mk ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.invest_upd_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.invest_upd_dt ?? DBNull.Value;

                int cnt = cmd.ExecuteNonQuery();

                return cnt;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// "商品資料會計接收"的異動
        /// </summary>
        /// <param name="his"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int updateForAcct(FGL_ITEM_INFO_HIS his, SqlConnection conn, SqlTransaction transaction)
        {
            SqlCommand cmd = conn.CreateCommand();

            cmd.Connection = conn;
            cmd.Transaction = transaction;

            try
            {
                string sql = "";
                sql = @"
UPDATE FGL_ITEM_INFO_HIS 
  SET ITEM_NAME = @ITEM_NAME,
      ITEM_NAME_SHRT = @ITEM_NAME_SHRT,
      ITEM_ACCT = @ITEM_ACCT,
      SEPARAT_ACCT = @SEPARAT_ACCT,
      COI_ACCT = @COI_ACCT,
      EFFECT_DATE = @EFFECT_DATE,
      APPR_MK = @APPR_MK,
      ACCT_UPD_ID = @ACCT_UPD_ID,
      ACCT_UPD_DT = @ACCT_UPD_DT
WHERE APLY_NO = @APLY_NO";


                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@APLY_NO", his.aply_no);

                cmd.Parameters.AddWithValue("@ITEM_NAME", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.item_name) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_NAME_SHRT", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.item_name_shrt) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.item_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@SEPARAT_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.separat_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.coi_acct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EFFECT_DATE", System.Data.SqlDbType.DateTime).Value = (Object)his.effect_date ?? DBNull.Value;
                
                cmd.Parameters.AddWithValue("@APPR_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.appr_mk ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)his.acct_upd_id ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_DT", System.Data.SqlDbType.DateTime).Value = (Object)his.acct_upd_dt ?? DBNull.Value;

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