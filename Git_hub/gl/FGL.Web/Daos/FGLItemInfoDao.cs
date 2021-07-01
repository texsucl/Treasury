
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
/// 功能說明：FGL_ITEM_INFO 會計商品資訊檔
/// 初版作者：20190104 Daiyu
/// 修改歷程：20190104 Daiyu
/// 需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191129 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：新增"商品簡稱"欄位
/// </summary>


namespace FGL.Web.Daos
{
    public class FGLItemInfoDao
    {

        /// <summary>
        /// 檢查險種科目是否已存在
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemAcct"></param>
        /// <returns></returns>
        public string chkItemAcct(string item, string itemAcct) {
            string itemExist = "";
            using (dbFGLEntities db = new dbFGLEntities())
            {
                FGL_ITEM_INFO d = db.FGL_ITEM_INFO
                    .Where(x => (x.item_acct == itemAcct || x.coi_acct == itemAcct) & x.item != item
                    ).FirstOrDefault();


                if (d != null)
                    itemExist = StringUtil.toString(d.item);
            }
            

            return itemExist;
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
                    var his = (from m in db.FGL_ITEM_INFO
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
                               & ((m.fu_mk == "N" && m.item_acct.Substring(0,2) == codeSmpRule56.RESERVE1) || (m.fu_mk == "Y" && m.item_acct.Substring(0, 2) == codeSmpRule56.RESERVE2))
                               & (bEffectDateB || (!bEffectDateB & m.effect_date >= sEffB))
                               & (bEffectDateE || (!bEffectDateE & m.effect_date < sEffE))
                               select new OGL00008Model
                               {
                                   tempId = m.item + "|" + m.product_type + "|" + m.fu_mk + "|" + m.item_con + "|" + m.disc_part_feat,
                                   srceFrom = "正式檔",
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
                    var his = (from m in db.FGL_ITEM_INFO
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
                                   tempId = m.item + "|" + m.product_type + "|" + m.fu_mk + "|" + m.item_con + "|" + m.disc_part_feat,
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
                                   itemNameShrt = m.item_name_shrt.Trim(),  //add by daiyu 20191129
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

        public List<OGL00003Model> qryByDateItem(string updDateB, string updDateE, string item, string apprMk)
        {
            bool bUpdDateB = StringUtil.isEmpty(updDateB);
            bool bUpdDateE = StringUtil.isEmpty(updDateE);
            bool bItem = StringUtil.isEmpty(item);
            bool bApprMk = StringUtil.isEmpty(apprMk);

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
                    var his = (from m in db.FGL_ITEM_INFO
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
                               & (bItem || (!bItem & m.item == item))
                               & (bApprMk || (!bApprMk & m.appr_mk == apprMk))
                                & (bUpdDateB || (!bUpdDateB & ((m.prod_upd_dt >= sB & m.prod_upd_dt < sE) || (m.invest_upd_dt >= sB & m.invest_upd_dt < sE) || (m.acct_upd_dt >= sB & m.acct_upd_dt < sE))))

                               select new OGL00003Model
                               {
                                   tempId = m.item + "|" + m.product_type + "|" + m.fu_mk + "|" + m.item_con + "|" + m.disc_part_feat,
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
                                   itemNameShrt = m.item_name_shrt.Trim(),  //add by daiyu 20191129
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



        public OGL00003Model qryItem(string item, string apprMk)
            //public OGL00003Model qryPK(string item, string productType, string fuMk, string itemCon, string discPartFeat, string apprMk)
        {
            bool bApprMk = StringUtil.isEmpty(apprMk);

            //productType = StringUtil.toString(productType);
            //fuMk = StringUtil.toString(fuMk);
            //itemCon = StringUtil.toString(itemCon);
            //discPartFeat = StringUtil.toString(discPartFeat);

            //bool bProductType = StringUtil.isEmpty(productType);
            //bool bFuMk = StringUtil.isEmpty(fuMk);
            //bool bItemCon = StringUtil.isEmpty(itemCon);
            //bool bDiscPartFeat = StringUtil.isEmpty(discPartFeat);

            using (new TransactionScope(
              TransactionScopeOption.Required,
              new TransactionOptions
              {
                  IsolationLevel = IsolationLevel.ReadUncommitted
              }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    var his = (from  m in db.FGL_ITEM_INFO
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
                               & (bApprMk || (!bApprMk & m.appr_mk == apprMk))
                               select new OGL00003Model
                               {
                                   item = m.item,
                                   flag = m.flag,

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
                                   itemMainType = m.item_main_type == null ? "" : m.item_main_type,
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
                                   healthMgrType = m.health_mgr_type == null ? "" : m.health_mgr_type,
                                   coiType = m.coi_type == null ? "" : m.coi_type,
                                   itemName = m.item_name  == null ? "" : m.item_name.Trim(),
                                   itemNameShrt = m.item_name_shrt == null ? "" : m.item_name_shrt.Trim(),  //modify by daiyu 20191129
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



        public void insertFormHis(string aplyNo, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO FGL_ITEM_INFO
                   (ITEM
           ,PRODUCT_TYPE
           ,FU_MK
           ,ITEM_CON
           ,DISC_PART_FEAT
           ,PROD_NO_VER
           ,PROD_NO
           ,SYS_TYPE
           ,ITEM_MAIN_TYPE
           ,INVEST_TYPE
           ,INS_TERM
           ,BUSI_TYPE
           ,COM_TYPE
           ,EXT_SCHEDUL_TYPE
           ,PAKIND_DMOP_TYPE
           ,LODPRM_TYPE
           ,HEALTH_MGR_TYPE
           ,COI_TYPE
           ,ITEM_NAME
           ,ITEM_NAME_SHRT
           ,ITEM_ACCT
           ,SEPARAT_ACCT
           ,COI_ACCT
           ,EFFECT_DATE
           ,EFF_MK
           ,APPR_MK
           ,PROD_UPD_ID
           ,PROD_UPD_DT
           ,PROD_APPR_ID
           ,PROD_APPR_DT
           ,INVEST_UPD_ID
           ,INVEST_UPD_DT
           ,INVEST_APPR_ID
           ,INVEST_APPR_DT
           ,ACCT_UPD_ID
           ,ACCT_UPD_DT
           ,ACCT_APPR_ID
           ,ACCT_APPR_DT
           ,CREATE_ID
           ,CREATE_DT
           ,FLAG) 
SELECT 
ITEM
           ,PRODUCT_TYPE
           ,FU_MK
           ,ITEM_CON
           ,DISC_PART_FEAT
           ,PROD_NO_VER
           ,PROD_NO
           ,SYS_TYPE
           ,ITEM_MAIN_TYPE
           ,INVEST_TYPE
           ,INS_TERM
           ,BUSI_TYPE
           ,COM_TYPE
           ,EXT_SCHEDUL_TYPE
           ,PAKIND_DMOP_TYPE
           ,LODPRM_TYPE
           ,HEALTH_MGR_TYPE
           ,COI_TYPE
           ,ITEM_NAME
           ,ITEM_NAME_SHRT
           ,ITEM_ACCT
           ,SEPARAT_ACCT
           ,COI_ACCT
           ,EFFECT_DATE
           ,'Y'
           ,APPR_MK
           ,PROD_UPD_ID
           ,PROD_UPD_DT
           ,PROD_APPR_ID
           ,PROD_APPR_DT
           ,INVEST_UPD_ID
           ,INVEST_UPD_DT
           ,INVEST_APPR_ID
           ,INVEST_APPR_DT
           ,ACCT_UPD_ID
           ,ACCT_UPD_DT
           ,ACCT_APPR_ID
           ,ACCT_APPR_DT
           ,CREATE_ID
           ,CREATE_DT
           ,FLAG
 FROM FGL_ITEM_INFO_HIS
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

        public void updateApprMk(OGL00003Model his, OGL00003Model formal, SqlConnection conn, SqlTransaction transaction)
        {



            try
            {

                string sql = @"
UPDATE FGL_ITEM_INFO
 SET APPR_MK = @APPR_MK
    ,PROD_UPD_ID = @PROD_UPD_ID
    ,PROD_UPD_DT = @PROD_UPD_DT
    ,PROD_APPR_ID = @PROD_APPR_ID
    ,PROD_APPR_DT = @PROD_APPR_DT
    ,INVEST_UPD_ID = @INVEST_UPD_ID
    ,INVEST_UPD_DT = @INVEST_UPD_DT
    ,INVEST_APPR_ID = @INVEST_APPR_ID
    ,INVEST_APPR_DT = @INVEST_APPR_DT
    ,ACCT_UPD_ID = @ACCT_UPD_ID
    ,ACCT_UPD_DT = @ACCT_UPD_DT
    ,ACCT_APPR_ID = @ACCT_APPR_ID
    ,ACCT_APPR_DT = @ACCT_APPR_DT
  WHERE ITEM = @ITEM
    AND PRODUCT_TYPE = @PRODUCT_TYPE
    AND FU_MK = @FU_MK
    AND ITEM_CON = @ITEM_CON
    AND DISC_PART_FEAT = @DISC_PART_FEAT";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@ITEM", his.item);
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", his.productType);
                cmd.Parameters.AddWithValue("@FU_MK", his.fuMk);
                cmd.Parameters.AddWithValue("@ITEM_CON", his.itemCon);
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", his.discPartFeat);

                cmd.Parameters.AddWithValue("@APPR_MK", his.execAction == "D" ? "7" : "6");
                cmd.Parameters.AddWithValue("@PROD_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.prodUpId ?? formal.prodUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.prodUpDt) == "" ? DBNull.Value : (Object)his.prodUpDt; 
                cmd.Parameters.AddWithValue("@PROD_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.prodApprId ?? formal.prodApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.prodApprDt) == "" ? DBNull.Value : (Object)his.prodApprDt; 
                cmd.Parameters.AddWithValue("@INVEST_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.investUpId ?? formal.investUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.investUpDt) == "" ? DBNull.Value : (Object)his.investUpDt; 
                cmd.Parameters.AddWithValue("@INVEST_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.investApprId ?? formal.investApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.investApprDt) == "" ? DBNull.Value : (Object)his.investApprDt;
                cmd.Parameters.AddWithValue("@ACCT_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.acctUpId ?? formal.acctUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.acctUpDt) == "" ? DBNull.Value : (Object)his.acctUpDt; 
                cmd.Parameters.AddWithValue("@ACCT_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.acctApprId ?? formal.acctApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.acctApprDt) == "" ? DBNull.Value : (Object)his.acctApprDt;

                int cnt = cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {

                throw e;
            }

        }


        public void update(OGL00003Model his, OGL00003Model formal, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"
UPDATE FGL_ITEM_INFO
 SET PROD_NO_VER = @PROD_NO_VER
    ,PROD_NO = @PROD_NO
    ,SYS_TYPE = @SYS_TYPE
    ,ITEM_MAIN_TYPE = @ITEM_MAIN_TYPE
    ,INVEST_TYPE = @INVEST_TYPE
    ,INS_TERM = @INS_TERM
    ,BUSI_TYPE = @BUSI_TYPE
    ,COM_TYPE = @COM_TYPE
    ,EXT_SCHEDUL_TYPE = @EXT_SCHEDUL_TYPE
    ,PAKIND_DMOP_TYPE = @PAKIND_DMOP_TYPE
    ,LODPRM_TYPE = @LODPRM_TYPE
    ,HEALTH_MGR_TYPE = @HEALTH_MGR_TYPE
    ,COI_TYPE = @COI_TYPE
    ,ITEM_NAME = @ITEM_NAME
    ,ITEM_NAME_SHRT = @ITEM_NAME_SHRT
    ,ITEM_ACCT = @ITEM_ACCT
    ,SEPARAT_ACCT = @SEPARAT_ACCT
    ,COI_ACCT = @COI_ACCT
    ,EFFECT_DATE = @EFFECT_DATE
    ,EFF_MK = @EFF_MK
    ,APPR_MK = @APPR_MK
    ,PROD_UPD_ID = @PROD_UPD_ID
    ,PROD_UPD_DT = @PROD_UPD_DT
    ,PROD_APPR_ID = @PROD_APPR_ID
    ,PROD_APPR_DT = @PROD_APPR_DT
    ,INVEST_UPD_ID = @INVEST_UPD_ID
    ,INVEST_UPD_DT = @INVEST_UPD_DT
    ,INVEST_APPR_ID = @INVEST_APPR_ID
    ,INVEST_APPR_DT = @INVEST_APPR_DT
    ,ACCT_UPD_ID = @ACCT_UPD_ID
    ,ACCT_UPD_DT = @ACCT_UPD_DT
    ,ACCT_APPR_ID = @ACCT_APPR_ID
    ,ACCT_APPR_DT = @ACCT_APPR_DT
  WHERE ITEM = @ITEM
    AND PRODUCT_TYPE = @PRODUCT_TYPE
    AND FU_MK = @FU_MK
    AND ITEM_CON = @ITEM_CON
    AND DISC_PART_FEAT = @DISC_PART_FEAT";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@ITEM", his.item);
                cmd.Parameters.AddWithValue("@PRODUCT_TYPE", his.productType);
                cmd.Parameters.AddWithValue("@FU_MK", his.fuMk);
                cmd.Parameters.AddWithValue("@ITEM_CON", his.itemCon);
                cmd.Parameters.AddWithValue("@DISC_PART_FEAT", his.discPartFeat);

                cmd.Parameters.AddWithValue("@PROD_NO_VER", System.Data.SqlDbType.VarChar).Value = (Object)his.prodNoVer ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_NO", System.Data.SqlDbType.VarChar).Value = (Object)his.prodNo ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@SYS_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.sysType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_MAIN_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.itemMainType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.investType ?? DBNull.Value; 
                cmd.Parameters.AddWithValue("@INS_TERM", System.Data.SqlDbType.VarChar).Value = (Object)his.insTerm ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@BUSI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.busiType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.comType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EXT_SCHEDUL_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.extSchedulType ?? DBNull.Value; 
                cmd.Parameters.AddWithValue("@PAKIND_DMOP_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.pakindDmopType ?? DBNull.Value; 
                cmd.Parameters.AddWithValue("@LODPRM_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.lodprmType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@HEALTH_MGR_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.healthMgrType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)his.coiType ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_NAME", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.itemName) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ITEM_NAME_SHRT", System.Data.SqlDbType.VarChar).Value = (Object)StringUtil.halfToFull(his.itemNameShrt) ?? DBNull.Value;  //ADD BY DAIYU 20191129
                cmd.Parameters.AddWithValue("@ITEM_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.itemAcct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@SEPARAT_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.separatAcct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@COI_ACCT", System.Data.SqlDbType.VarChar).Value = (Object)his.coiAcct ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@EFFECT_DATE", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.effectDate) == "" ? DBNull.Value : (Object)his.effectDate;
                cmd.Parameters.AddWithValue("@EFF_MK", System.Data.SqlDbType.VarChar).Value = (Object)his.effMk ?? DBNull.Value; 
                cmd.Parameters.AddWithValue("@APPR_MK", his.execAction == "D" ? "7" : "6");
                cmd.Parameters.AddWithValue("@PROD_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.prodUpId ?? formal.prodUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.prodUpDt) == "" ? (StringUtil.toString(formal.prodUpDt) == "" ? DBNull.Value : (Object)formal.prodUpDt) : (Object)his.prodUpDt; 
                cmd.Parameters.AddWithValue("@PROD_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.prodApprId ?? formal.prodApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@PROD_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.prodUpDt) == "" ? (StringUtil.toString(formal.prodApprDt) == "" ? DBNull.Value : (Object)formal.prodApprDt) : (Object)his.prodApprDt;
                cmd.Parameters.AddWithValue("@INVEST_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.investUpId ?? formal.investUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.investUpDt) == "" ? (StringUtil.toString(formal.investUpDt) == "" ? DBNull.Value : (Object)formal.investUpDt) : (Object)his.investUpDt;
                cmd.Parameters.AddWithValue("@INVEST_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.investApprId ?? formal.investApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@INVEST_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.investApprDt) == "" ? (StringUtil.toString(formal.investApprDt) == "" ? DBNull.Value : (Object)formal.investApprDt) : (Object)his.investApprDt;
                cmd.Parameters.AddWithValue("@ACCT_UPD_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.acctUpId ?? formal.acctUpId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_UPD_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.acctUpDt) == "" ? DBNull.Value : (Object)his.acctUpDt; 
                cmd.Parameters.AddWithValue("@ACCT_APPR_ID", System.Data.SqlDbType.VarChar).Value = (Object)(his.acctApprId ?? formal.acctApprId) ?? DBNull.Value;
                cmd.Parameters.AddWithValue("@ACCT_APPR_DT", System.Data.SqlDbType.DateTime).Value = StringUtil.toString(his.acctApprDt) == "" ? DBNull.Value : (Object)his.acctApprDt;

                int cnt = cmd.ExecuteNonQuery();

                

            }
            catch (Exception e)
            {

                throw e;
            }

        }



    }
}