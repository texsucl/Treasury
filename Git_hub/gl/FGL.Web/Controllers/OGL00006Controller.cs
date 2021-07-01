using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

/// <summary>
/// 功能說明：商品資料查詢作業
/// 初版作者：20190129 Daiyu
/// 修改歷程：20190129 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00006Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00006/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            SysCodeDao sysCodeDao = new SysCodeDao();

            //覆核註記
            var apprMkList = sysCodeDao.loadSelectList("GL", "ITEM_APPR_MK", true);
            ViewBag.apprMkList = apprMkList;

            return View();
        }


        /// <summary>
        /// 查詢待輸入的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string isQryTmp, string updDateB, string updDateE, string item, string apprMk
            , string effectDateB, string effectDateE)
        {
            List<OGL00003Model> rows = new List<OGL00003Model>();

            if ("1".Equals(isQryTmp))   //查詢"會計商品資訊檔暫存檔"
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                rows = fGLItemInfoHisDao.qryForOGL00006(apprMk, updDateB, updDateE, item, effectDateB, effectDateE);
            }
            else
            {  //查詢"會計商品資訊"
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                rows = fGLItemInfoDao.qryForOGL00006(apprMk, updDateB, updDateE, item, effectDateB, effectDateE);
            }
            


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();


                foreach (OGL00003Model d in rows)
                {
                    if (!"".Equals(StringUtil.toString(d.prodUpId))) {
                        if(!userNameMap.ContainsKey(d.prodUpId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, d.prodUpId, dbIntra);

                        d.prodUpId = userNameMap[d.prodUpId];
                    }

                    if (!"".Equals(StringUtil.toString(d.prodApprId)))
                    {
                        if(!userNameMap.ContainsKey(d.prodApprId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, d.prodApprId, dbIntra);

                        d.prodApprId = userNameMap[d.prodApprId];
                    }

                    if (!"".Equals(StringUtil.toString(d.investUpId)))
                    {
                        if(!userNameMap.ContainsKey(d.investUpId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, d.investUpId, dbIntra);

                        d.investUpId = userNameMap[d.investUpId];
                    }

                    if (!"".Equals(StringUtil.toString(d.investApprId)))
                    {
                        if(!userNameMap.ContainsKey(d.investApprId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, d.investApprId, dbIntra);

                        d.investApprId = userNameMap[d.investApprId];
                    }

                    if (!"".Equals(StringUtil.toString(d.acctUpId)))
                    {
                        if(!userNameMap.ContainsKey(d.acctUpId))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, d.acctUpId, dbIntra);

                        d.acctUpId = userNameMap[d.acctUpId];
                    }

                    if (!"".Equals(StringUtil.toString(d.acctApprId)))
                    {
                        if(!userNameMap.ContainsKey(d.acctApprId))
                        userNameMap = oaEmpDao.qryUsrName(userNameMap, d.acctApprId, dbIntra);

                        d.acctApprId = userNameMap[d.acctApprId];
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

            List<smpNumModel> investGaList = new List<smpNumModel>();
            List<smpNumModel> investSaList = new List<smpNumModel>();

            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();

            string[] acctType = new string[] {"B", "C"};
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
                        if(d.acctType.StartsWith("C"))
                            investGaList.Add(smpNum);
                        else
                            prodGaList.Add(smpNum);
                        break;
                    case "2":
                        if (d.acctType.StartsWith("C"))
                            investSaList.Add(smpNum);
                        else
                            prodSaList.Add(smpNum);
                        break;
                }
            }

            List<OGL00003DModel> prodSumpD = new List<OGL00003DModel>();
            List<OGL00003DModel> investSumpD = new List<OGL00003DModel>();

            if ("1".Equals(model.isQryTmp))
            {
                FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                prodSumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "P");
                investSumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "I");
            }
            else {
                FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                prodSumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "P");
                investSumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "I");

            }
            

            return Json(new { success = true
                , prodGaList = prodGaList, investGaList = investGaList
                , prodSaList = prodSaList, investSaList= investSaList
                , prodSumpD = prodSumpD, investSumpD = investSumpD }, JsonRequestBehavior.AllowGet);
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
        /// <returns></returns>
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
                if ("1".Equals(isQryTmp))
                {
                    OGL00003Model modelHis = new OGL00003Model();
                    FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                    modelHis = fGLItemInfoHisDao.qryAplyNo(tempId);

                    ViewBag.aplyNo = modelHis.aplyNo;
                    ViewBag.execAction = modelHis == null ? "" : modelHis.execAction ?? "";
                    model = modelHis;
                }
                else {
                    var temp = StringUtil.toString(tempId).Split('|');
                    string item = temp[0];
                    string productType = temp[1];
                    string fuMk = temp[2];
                    string itemCon = temp[3];
                    string discPartFeat = temp[4];

                    FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                    //model = fGLItemInfoDao.qryPK(item, productType, fuMk, itemCon, discPartFeat, "");
                    model = fGLItemInfoDao.qryItem(item, "");
                }

                ViewBag.isQryTmp = isQryTmp;
                ViewBag.bHaveData = "Y";


                if (!"O".Equals(model.flag)) {
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
                }



                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        model.prodUpId = model.prodUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.prodUpId, dbIntra).EMP_NAME);
                        model.prodApprId = model.prodApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.prodApprId, dbIntra).EMP_NAME);

                        model.investUpId = model.investUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.investUpId, dbIntra).EMP_NAME);
                        model.investApprId = model.investApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.investApprId, dbIntra).EMP_NAME);

                        model.acctUpId = model.acctUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.acctUpId, dbIntra).EMP_NAME);
                        model.acctApprId = model.acctApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(model.acctApprId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }


                ViewBag.itemHealthMgrMk = StringUtil.toString(model.healthMgrType);

                if (!"O".Equals(model.flag))
                    model.effectYY = (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString();

                return View(model);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(model);
            }
        }


        
    }
}