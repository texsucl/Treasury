using FGL.Web.ActionFilter;
using FGL.Web.AS400PGM;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：科目設定維護作業
/// 初版作者：20190318 Daiyu
/// 修改歷程：20190318 Daiyu
///           需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191129 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：1.新增"商品簡稱"欄位
///           2.將畫面上的險種科目，與相對應的【會科取號檔】中同一規則的下一個科目編號相比，若畫面號碼小於取號檔，不回寫【會科取號檔】
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00007Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00007/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            return View();
        }


        /// <summary>
        /// 查詢待輸入的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string isQryTmp, string updDateB, string updDateE, string item)
        {
            List<OGL00003Model> rows = new List<OGL00003Model>();


            if ("1".Equals(isQryTmp))   //查詢"會計商品資訊檔暫存檔"
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                rows = fGLItemInfoHisDao.qryForOGL00007(updDateB, updDateE, item);
            }
            else
            {  //查詢"會計商品資訊"
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                rows = fGLItemInfoDao.qryByDateItem(updDateB, updDateE, item, "6");
            }
                


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OGL00003Model d in rows)
                {
                    d.aplyDt = d.createDt;
                    d.aplyUId = d.createId ?? "";


                    createUid = d.aplyUId;

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }


                        d.aplyUId = createUid + " " + userNameMap[createUid];
                    }
                }
            }

            
            var jsonData = new { success = true, dataList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 查詢FGLSMPB取得SA、GA項目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySMPB(OGL00003Model model)
        {
            List<smpNumModel> prodGaList = new List<smpNumModel>();
            List<smpNumModel> prodSaList = new List<smpNumModel>();


            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();

            string[] acctType = new string[] {"B"};

           

            List<OGL10181Model> rows = fGLSMPBDao.qryForItem(model.productType, model.fuMk, model.itemCon, model.discPartFeat
                , acctType);

            foreach (OGL10181Model d in rows)
            {
                smpNumModel smpNum = new smpNumModel();
                smpNum.tempId = d.tempId;
                smpNum.smpNum = d.smpNum;
                smpNum.smpName = d.smpName;
                smpNum.productType = d.productType;
                smpNum.acctType = d.acctType;

                switch (d.acctType.Substring(1, 1))
                {
                    case "1":
                    case "3":
                            prodGaList.Add(smpNum);
                        break;
                    case "2":
                            prodSaList.Add(smpNum);
                        break;
                }
            }

            List<OGL00003DModel> prodSumpD = new List<OGL00003DModel>();

            if ("1".Equals(model.isQryTmp))
            {
                FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                prodSumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "P");
            }
            else
            {
                FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                prodSumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "P");
            }



            return Json(new { success = true
                , prodGaList = prodGaList
                , prodSaList = prodSaList
                , prodSumpD = prodSumpD }, JsonRequestBehavior.AllowGet);
        }


        

        public class smpNumModel
        {
            public string tempId { get; set; }
            public string smpNum { get; set; }
            public string smpName { get; set; }
            public string productType { get; set; }
            public string acctType { get; set; }

            public string acctItem { get; set; }
            public string acctItemVal { get; set; }


        }

        /// <summary>
        /// 開啟明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns
        public ActionResult detail(string tempId, string isQryTmp)
        {

            SysCodeDao sysCodeDao = new SysCodeDao();

            //險種類別
            var productTypeList = sysCodeDao.loadSelectList("GL", "PRODUCT_TYPE", true);
            ViewBag.productTypeList = productTypeList;

            //Y/N
            var ynList = sysCodeDao.loadSelectList("GL", "YN_FLAG", true);
            ViewBag.ynList = ynList;

            //合約分類
            var itemConList = sysCodeDao.loadSelectList("GL", "ITEM_CON", true);
            ViewBag.itemConList = itemConList;

            //保險商品編號版本
            var prodNoVerList = sysCodeDao.loadSelectList("GL", "PROD_NO_VER", true);
            ViewBag.prodNoVerList = prodNoVerList;

            //進件系統別
            var sysTypeList = sysCodeDao.loadSelectList("GL", "SYS_TYPE", true);
            ViewBag.sysTypeList = sysTypeList;

            //險種主險別
            var itemMainTypeList = sysCodeDao.loadSelectList("GL", "ITEM_MAIN_TYPE", true);
            ViewBag.itemMainTypeList = itemMainTypeList;

            //投資型商品類型
            var investTypeList = sysCodeDao.loadSelectList("GL", "INVEST_TYPE", true);
            ViewBag.investTypeList = investTypeList;

            //保險期間
            var insTermList = sysCodeDao.loadSelectList("GL", "INS_TERM", true);
            ViewBag.insTermList = insTermList;

            //業務性質
            var busiTypeList = sysCodeDao.loadSelectList("GL", "BUSI_TYPE", true);
            ViewBag.busiTypeList = busiTypeList;

            //佣金/承攬費
            var comTypeList = sysCodeDao.loadSelectList("GL", "COM_TYPE", true);
            ViewBag.comTypeList = comTypeList;

            //保費費用
            var lodprmTypeList = sysCodeDao.loadSelectList("GL", "LODPRM_TYPE", true);
            ViewBag.lodprmTypeList = lodprmTypeList;

            //覆核註記
            var apprMkList = sysCodeDao.loadSelectList("GL", "ITEM_APPR_MK", true);
            ViewBag.apprMkList = apprMkList;

            //建立來源
            var flagList = sysCodeDao.loadSelectList("GL", "ITEM_Flag", true);
            ViewBag.flagList = flagList;

            OGL00003Model model = new OGL00003Model();
            try
            {
                var temp = StringUtil.toString(tempId).Split('|');
                string item = temp[0];
                string productType = temp[1];
                string fuMk = temp[2];
                string itemCon = temp[3];
                string discPartFeat = temp[4];

                //查詢【FGL_ITEM_INFO_HIS 會計商品資訊暫存檔】
                OGL00003Model modelHis = new OGL00003Model();
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                modelHis = fGLItemInfoHisDao.qryItem(item);
                //modelHis = fGLItemInfoHisDao.qryItem(item, productType, fuMk, itemCon, discPartFeat);


                ViewBag.aplyNo = modelHis == null ? "" : modelHis.aplyNo ?? "";
                ViewBag.apprMkHis = modelHis == null ? "" : modelHis.apprMk ?? "";
                ViewBag.status = modelHis == null ? "" : modelHis.status ?? "";
                ViewBag.execAction = modelHis == null ? "" : modelHis.execAction ?? "";

                if ("1".Equals(isQryTmp))
                {
                    ViewBag.aplyNo = modelHis.aplyNo;
                    model = modelHis;
                }
                else
                {
                    //查詢【FGL_ITEM_INFO 會計商品資訊檔】
                    FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                    model = fGLItemInfoDao.qryItem(item, "6");
                    //model = fGLItemInfoDao.qryPK(item, productType, fuMk, itemCon, discPartFeat, "6");
                }


                ViewBag.isQryTmp = isQryTmp;
                ViewBag.bHaveData = "Y";
                ViewBag.itemNameSrc = "";


                //查詢"商品科目設定檔"

                FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                List<OGL00001Model> itemAcct = fGLItemAcctDao.qryHeadForItem(model.productType, model.fuMk, model.itemCon, model.discPartFeat);

                if (itemAcct.Count == 0)
                    return Json(new { success = false, err = "查無「商品科目設定」資料!!" }, JsonRequestBehavior.AllowGet);

                //ViewBag.itemHealthMgrMk = itemAcct[0].healthMgrMk;
                ViewBag.itemInvestTradMk = itemAcct[0].investTradMk;
                ViewBag.itemInvestTypeMk = itemAcct[0].investTypeMk;
                ViewBag.itemLodprmMk = itemAcct[0].lodprmMk;
                ViewBag.itemPakindDmopMk = itemAcct[0].pakindDmopMk;
                ViewBag.itemCoiType = itemAcct[0].coiType;


                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        model.prodUpId = model.prodUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.prodUpId, dbIntra).EMP_NAME);
                        model.prodApprId = model.prodApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.prodApprId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }

                if (!"O".Equals(model.flag)) {
                    //生效日-->要做民國年、西元年的轉換
                    model.effectYY = model.effectYY == "" ? "" : (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString();

                    //險種中文名稱，需自上游預帶值
                    NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
                    if ("A".Equals(model.execAction)) {
                       
                        string itemName = newAcctItemUtil.qryItemName(model.item);

                        if (!"".Equals(StringUtil.toString(itemName).TrimEnd()))
                        {
                            ViewBag.itemNameSrc = itemName;
                            //model.itemName = itemName;
                        }


                        //商品簡稱預設為商品名稱   add by daiyu 20191129
                        if ("".Equals(StringUtil.toString(model.itemNameShrt)))
                            model.itemNameShrt = model.itemName.Length > 10 ? model.itemName.Substring(0, 10) : model.itemName;

                      
                    }

                    ViewBag.itemHealthMgrMk = StringUtil.toString(model.healthMgrType);

                    OGL00001Model oGL00001Model = newAcctItemUtil.qryItemAcct(modelHis.productType, modelHis.fuMk, modelHis.itemCon, modelHis.discPartFeat);

                    //ViewBag.itemHealthMgrMk = oGL00001Model.healthMgrMk;
                    ViewBag.itemInvestTypeMk = oGL00001Model.investTypeMk;
                    ViewBag.itemLodprmMk = oGL00001Model.lodprmMk;
                    ViewBag.itemPakindDmopMk = oGL00001Model.pakindDmopMk;
                    ViewBag.itemCoiType = oGL00001Model.coiType;
                }


                return View(model);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(model);
            }
        }


        


        private Dictionary<string, OGL00003DModel> chkInput(OGL00003Model model)
        {
            NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
            Dictionary<string, OGL00003DModel> smpMap = newAcctItemUtil.getSmpList("U", model);

            //檢查科目是否重複
            FGLAACTDao fGLAACTDao = new FGLAACTDao();
            smpMap = fGLAACTDao.qryForOGL00005(smpMap);

            return smpMap;
        }


        private FGL_ITEM_INFO_HIS formalToHis(FGL_ITEM_INFO_HIS his, OGL00003Model model)
        {
            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            OGL00003Model formal = fGLItemInfoDao.qryItem(model.item, "6");
            //OGL00003Model formal = fGLItemInfoDao.qryPK(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "6");

            his.prod_no_ver = formal.prodNoVer;
            his.prod_no = formal.prodNo;
            his.sys_type = formal.sysType;
            his.item_main_type = formal.itemMainType;
            his.invest_type = formal.investType;
            his.ins_term = formal.insTerm;
            his.busi_type = formal.busiType;
            his.com_type = formal.comType;
            his.ext_schedul_type = formal.extSchedulType;
            his.pakind_dmop_type = formal.pakindDmopType;
            his.lodprm_type = formal.lodprmType;
            his.health_mgr_type = formal.healthMgrType;
            his.coi_type = formal.coiType;

            his.eff_mk = formal.effMk;
            his.flag = formal.flag;

            return his;
        }

        /// <summary>
        /// 檢查險種科目是否已存在
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemAcct"></param>
        /// <returns></returns>
        private string chkDupItemAcct(string item, string itemAcct) {
            bool bErr = false;
            string itemExist = "";

            //檢查險種科目是否已存在
            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            itemExist = fGLItemInfoDao.chkItemAcct(item, itemAcct);

            if (!"".Equals(itemExist))
                bErr = true;

            if (!bErr)
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                itemExist = fGLItemInfoHisDao.chkItemAcct(item, itemAcct);
            }

            return itemExist;
        }


        [HttpPost]
        public JsonResult chkSmpNum(OGL00003Model model)
        {
            
            string item_acct = "";
            string separat_acct = "";
            string coi_acct = "";
            bool bWarn = false;


            try
            {
                //檢查險種科目是否已存在
                string itemExist = chkDupItemAcct(model.item, model.itemAcct);
                if (!"".Equals(itemExist)) 
                    return Json(new { success = false, err = "取號重複，請確認!!(險種代號：" + itemExist + ")" }, JsonRequestBehavior.AllowGet);



                //依"險種資料"取得該商品對應的取號規則
                smpRuleModel smpRule = qryRuleModel(model);

                //組合"險種科目 item_acct"
                if (smpRule.findSmpNum)
                {
                    if (!model.itemAcct.Substring(0, 1).Equals(smpRule.rule56.prod_no_5))
                        bWarn = true;

                    if (!model.itemAcct.Substring(1, 1).Equals(smpRule.rule56.prod_no_6))
                        bWarn = true;

                    if (!model.itemAcct.Substring(2, 1).Equals(smpRule.rule7.item_acct_7))
                        bWarn = true;

                    item_acct = model.itemAcct;
                }


                //組合"COI科目 coi_acct"
                if (smpRule.ruleCOI != null & "Y".Equals(model.coiType))
                    coi_acct = getOtherAcct((Convert.ToInt32(item_acct) + smpRule.ruleCOI.takenum_interval).ToString().PadLeft(6, '0').Substring(0, 6), smpRule.ruleCOI);


                //組合"分離帳科目 separat_acct"
                if (smpRule.ruleSeparat != null)
                    separat_acct = getOtherAcct(item_acct, smpRule.ruleSeparat);



                var jsonData = new
                {
                    success = true,
                    bWarn = bWarn,
                    item_acct = item_acct,
                    separat_acct = separat_acct,
                    coi_acct = coi_acct
                };


                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 自動取號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySmpNum(OGL00003Model model)
        {     

            string item_acct = "";
            string separat_acct = "";
            string coi_acct = "";

            try
            {
                //依"險種資料"取得該商品對應的取號規則
                smpRuleModel smpRule = qryRuleModel(model);

                //組合"險種科目 item_acct"
                if (smpRule.findSmpNum)
                {
                    FGLSmpNumSeqDao fGLSmpNumSeqDao = new FGLSmpNumSeqDao();
                    FGL_SMP_NUM_SEQ smpSeq = fGLSmpNumSeqDao.qryForOGL00007(smpRule.rule56.seq, smpRule.rule7.seq, smpRule.rule810.seq);

                    if (smpSeq == null)
                        item_acct = smpRule.rule56.item_acct_5 + smpRule.rule56.item_acct_6 + smpRule.rule7.item_acct_7 + "000";
                    else
                        item_acct = smpRule.rule56.item_acct_5 + smpRule.rule56.item_acct_6 + smpRule.rule7.item_acct_7 + smpSeq.next_seq.ToString("000");
                }
                else
                    return Json(new { success = false, err = "自動取號規則無法判斷，請人工取號" }, JsonRequestBehavior.AllowGet);


                //組合"COI科目 coi_acct"
                if (smpRule.ruleCOI != null & "Y".Equals(model.coiType)) 
                    coi_acct = getOtherAcct((Convert.ToInt32(item_acct) + smpRule.ruleCOI.takenum_interval).ToString().PadLeft(6, '0').Substring(0, 6), smpRule.ruleCOI);


                //組合"分離帳科目 separat_acct"
                if (smpRule.ruleSeparat != null)
                    separat_acct = getOtherAcct(item_acct, smpRule.ruleSeparat);



                var jsonData = new { success = true,
                    item_acct = item_acct, separat_acct = separat_acct, coi_acct = coi_acct
                };


                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }

        private FGL_SMP_NUM_RULE compareRule(OGL00003Model model, List<FGL_SMP_NUM_RULE> ruleList) {
            FGL_SMP_NUM_RULE rule = null;
            FGL_SMP_NUM_RULE ruleDefault = null;

            bool bFind = false;

            foreach (FGL_SMP_NUM_RULE d in ruleList)
            {
                if ("".Equals(StringUtil.toString(d.smp_rule_product_type)) & "".Equals(StringUtil.toString(d.prod_no_4))
                    & "".Equals(StringUtil.toString(d.prod_no_5)) & "".Equals(StringUtil.toString(d.prod_no_6))
                    & "".Equals(StringUtil.toString(d.prod_no_7)) & "".Equals(StringUtil.toString(d.prod_no_8))
                    )
                {
                    ruleDefault = d;

                }
                else {
                    //比對"險種類別"
                    if (!"".Equals(StringUtil.toString(d.smp_rule_product_type)))
                    {
                        if (d.smp_rule_product_type.IndexOf(model.productType) < 0)
                            continue;
                    }

                    //比對"保險商品編號_第4碼"
                    if (!"".Equals(StringUtil.toString(d.prod_no_4)))
                    {
                        if (d.prod_no_4.IndexOf(model.prodNo.Substring(3, 1)) < 0)
                            continue;
                    }

                    //比對"保險商品編號_第5碼"
                    if (!"".Equals(StringUtil.toString(d.prod_no_5)))
                    {
                        if (d.prod_no_5.IndexOf(model.prodNo.Substring(4, 1)) < 0)
                            continue;
                    }

                    //比對"保險商品編號_第6碼"
                    if (!"".Equals(StringUtil.toString(d.prod_no_6)))
                    {
                        if (d.prod_no_6.IndexOf(model.prodNo.Substring(5, 1)) < 0)
                            continue;
                    }

                    //比對"保險商品編號_第7碼"
                    if (!"".Equals(StringUtil.toString(d.prod_no_7)))
                    {
                        if (d.prod_no_7.IndexOf(model.prodNo.Substring(6, 1)) < 0)
                            continue;
                    }

                    //比對"保險商品編號_第8碼"
                    if (!"".Equals(StringUtil.toString(d.prod_no_8)))
                    {
                        if (d.prod_no_8.IndexOf(model.prodNo.Substring(7, 1)) < 0)
                            continue;
                    }

                    
                }
                bFind = true;
                rule = d;

                break;
            }


            if (bFind)
                return rule;
            else
                return ruleDefault;

        }



        /// <summary>
        /// 修改作業
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(bool bNewItem, string execAction, string oItemAcct, OGL00003Model model)
        {
            if (!"".Equals(StringUtil.toString(model.itemAcct))) {
                string itemExist = chkDupItemAcct(model.item, model.itemAcct);
                if (!"".Equals(itemExist))
                    return Json(new { success = false, err = "取號重複，請確認!!(險種代號：" + itemExist + ")" }, JsonRequestBehavior.AllowGet);
            }


            //若屬"新增"商品，且「進件系統別=2.F或3.A&F」時，需做「會計科目」檢查
            if (bNewItem) {
            //if (bNewItem & ("2".Equals(model.sysType) || "3".Equals(model.sysType))) {
                Dictionary<string, OGL00003DModel> smpMap = chkInput(model);
                List<string> errSmpList = new List<string>();

                foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                {
                    if (item.Value.bExist)
                        errSmpList.Add(item.Key);
                }

                if (errSmpList.Count > 0)
                    return Json(new { success = false, err = "以下會計科目重覆，請確認", errSmp = errSmpList }, JsonRequestBehavior.AllowGet);

            }


            try
            {
                FGL_ITEM_INFO_HIS his = new FGL_ITEM_INFO_HIS();

                his.item = model.item;
                his.product_type = model.productType;
                his.fu_mk = model.fuMk;
                his.item_con = model.itemCon;
                his.disc_part_feat = model.discPartFeat;
                his.item_acct = model.itemAcct;
                his.separat_acct = model.separatAcct;
                his.coi_acct = model.coiAcct;
                his.item_name = model.itemName;
                his.item_name_shrt = model.itemNameShrt;  // add by daiyu 20191129

                if (!"O".Equals(model.flag) & !"".Equals(StringUtil.toString(his.item_acct)))
                    his.effect_date = DateUtil.stringToDatetime(DateUtil.formatDateTimeDbToSc(
                        (Convert.ToInt16(model.effectYY) + 1911).ToString().PadLeft(4, '0') + model.effectMM.PadLeft(2, '0') + model.effectDD.PadLeft(2, '0'), "D"));

                his.acct_upd_id = Session["UserID"].ToString();
                his.acct_upd_dt = DateTime.Now;
                his.aply_no = StringUtil.toString(model.aplyNo);
                his.exec_action = execAction;
                his.appr_mk = model.apprMk;

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                        FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();

                        if ("".Equals(his.aply_no))
                        {
                            //取得流水號
                            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = curDateTime[0];
                            var cId = sysSeqDao.qrySeqNo("FGL", "E", qPreCode).ToString();
                            cId = "E" + qPreCode + cId.ToString().PadLeft(4, '0');
                            his.aply_no = cId;
                            his.create_id = Session["UserID"].ToString();
                            his.create_dt = DateTime.Now;
                            his.exec_action = "U";
                            his.status = "14";
                            his.appr_mk = "14";

                            //將本支未維護的欄位從正式檔對搬至暫存檔
                            his = formalToHis(his, model);


                            //新增"會計商品資訊暫存檔"
                            fGLItemInfoHisDao.insert(his, conn, transaction);


                            //將正式檔的給付項目一併寫入暫存檔
                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                , model.itemCon, model.discPartFeat, "P", conn, transaction);

                            //若此險種有投資交易的項目，將正式檔的投資交易項目一併寫入暫存檔
                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                , model.itemCon, model.discPartFeat, "I", conn, transaction);

                            model.aplyNo = his.aply_no;

                        }
                        else {
                            //異動"FGL_ITEM_INFO_HIS 會計商品資訊暫存檔"
                            fGLItemInfoHisDao.updateForAcct(his, conn, transaction);
                        }




                        /*-----回寫"FGL_SMP_NUM_SEQ 會科編碼流水號資料檔"   begin-----*/
                        if (!StringUtil.toString(oItemAcct).Equals(his.item_acct) & !"".Equals(his.item_acct)) {
                            //依"險種資料"取得該商品對應的取號規則
                            smpRuleModel smpRule = qryRuleModel(model);

                            if (smpRule.findSmpNum)
                            {
                                FGLSmpNumSeqDao fGLSmpNumSeqDao = new FGLSmpNumSeqDao();
                                FGL_SMP_NUM_SEQ smpSeq = fGLSmpNumSeqDao.qryForOGL00007(smpRule.rule56.seq, smpRule.rule7.seq, smpRule.rule810.seq);

                                if (smpSeq == null)
                                {
                                    smpSeq = new FGL_SMP_NUM_SEQ();
                                    smpSeq.seq_item_acct_5_6 = smpRule.rule56.seq;
                                    smpSeq.seq_item_acct_7 = smpRule.rule7.seq;
                                    smpSeq.seq_item_acct_8_10 = smpRule.rule810.seq;
                                    smpSeq.next_seq = (Convert.ToInt32(his.item_acct) + (decimal)smpRule.rule810.takenum_interval) % 1000;
                                    smpSeq.item = his.item;
                                    smpSeq.item_acct = his.item_acct;
                                    smpSeq.update_id = Session["UserID"].ToString();
                                    smpSeq.update_datetime = DateTime.Now;

                                    fGLSmpNumSeqDao.insert(smpSeq, conn, transaction);
                                }
                                else
                                {
                                    smpSeq.next_seq = (Convert.ToInt32(his.item_acct) + (decimal)smpRule.rule810.takenum_interval) % 1000;
                                    smpSeq.item = his.item;
                                    smpSeq.item_acct = his.item_acct;
                                    smpSeq.update_id = Session["UserID"].ToString();
                                    smpSeq.update_datetime = DateTime.Now;

                                    fGLSmpNumSeqDao.update(smpSeq, conn, transaction);
                                }
                            }
                        }
                        /*-----回寫"FGL_SMP_NUM_SEQ 會科編碼流水號資料檔"   end-----*/

                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        logger.Error("其它錯誤：" + e.ToString());

                        return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                    }

                }


                var jsonData = new { success = true, model = model };


                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// 依"險種資料"取得該商品對應的取號規則
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private smpRuleModel qryRuleModel(OGL00003Model model) {
            smpRuleModel smpRule = new smpRuleModel();
            smpRule.findSmpNum = true;

            FGLSmpNumRuleDao fGLSmpNumRuleDao = new FGLSmpNumRuleDao();
            List<FGL_SMP_NUM_RULE> ruleList = fGLSmpNumRuleDao.qryForOGL00007(StringUtil.toString(model.fuMk));

            if (ruleList == null)
                smpRule.findSmpNum = false;

            /*---   組合"險種科目 item_acct"  begin   ---*/
            if (smpRule.findSmpNum)
            {
                //5、6碼
                smpRule.rule56 = compareRule(model, ruleList.Where(x => x.smp_rule_type == "1").ToList());

                if (smpRule.rule56 == null)
                    smpRule.findSmpNum = false;
                //else
                //    seqItemAcct56 = rule56.seq;
            }

            if (smpRule.findSmpNum)
            {
                //第7碼
                smpRule.rule7 = compareRule(model, ruleList.Where(x => x.smp_rule_type == "2").ToList());

                if (smpRule.rule7 == null)
                    smpRule.findSmpNum = false;
                //else
                //    seqItemAcct7 = rule7.seq;
            }

            if (smpRule.findSmpNum)
            {
                //第8~10碼
                smpRule.rule810 = compareRule(model, ruleList.Where(x => x.smp_rule_type == "3").ToList());

                if (smpRule.rule810 == null)
                    smpRule.findSmpNum = false;
                //else
                //    seqItemAcct810 = rule810.seq;
            }


            if (smpRule.findSmpNum) {
                smpRule.ruleCOI = compareRule(model, ruleList.Where(x => x.smp_rule_type == "4").ToList());
            }

            if (smpRule.findSmpNum)
            {
                smpRule.ruleSeparat = compareRule(model, ruleList.Where(x => x.smp_rule_type == "5").ToList());
            }

            

            return smpRule;
        }


        private string getOtherAcct(string itemAcct, FGL_SMP_NUM_RULE ruleModel) {
            string otherAcct = itemAcct;

            if (!"*".Equals(ruleModel.item_acct_5))
                otherAcct = ruleModel.item_acct_5 + otherAcct.Substring(1, 5);

            if (!"*".Equals(ruleModel.item_acct_6))
                otherAcct = otherAcct.Substring(0, 1) + ruleModel.item_acct_6 + otherAcct.Substring(2, 4);

            if (!"*".Equals(ruleModel.item_acct_7))
                otherAcct = otherAcct.Substring(0, 2) + ruleModel.item_acct_7 + otherAcct.Substring(3, 3);

            if (!"*".Equals(ruleModel.item_acct_8_10.Substring(0, 1)))
                otherAcct = otherAcct.Substring(0, 3) + ruleModel.item_acct_8_10.Substring(0, 1) + otherAcct.Substring(4, 2);

            if (!"*".Equals(ruleModel.item_acct_8_10.Substring(1, 1)))
                otherAcct = otherAcct.Substring(0, 4) + ruleModel.item_acct_8_10.Substring(1, 1) + otherAcct.Substring(5, 1);

            if (!"*".Equals(ruleModel.item_acct_8_10.Substring(2, 1)))
                otherAcct = otherAcct.Substring(0, 5) + ruleModel.item_acct_8_10.Substring(2, 1);


            return otherAcct;
        }


        public class smpModel
        {
            public string dataType { get; set; }

            public string showType { get; set; }

            public string smpName { get; set; }
        }


        public class smpRuleModel
        {
            public bool findSmpNum { get; set; }
            public FGL_SMP_NUM_RULE rule56 { get; set; }

            public FGL_SMP_NUM_RULE rule7 { get; set; }

            public FGL_SMP_NUM_RULE rule810 { get; set; }

            public FGL_SMP_NUM_RULE ruleCOI { get; set; }

            public FGL_SMP_NUM_RULE ruleSeparat { get; set; }
        }
    }
}
 
 