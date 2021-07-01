using FGL.Web.ActionFilter;
using FGL.Web.AS400Models;
using FGL.Web.AS400PGM;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;

/// <summary>
/// 功能說明：商品資料會計接收覆核作業
/// 初版作者：20190122 Daiyu
/// 修改歷程：20190122 Daiyu
///           需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191202 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：1.畫面增加"商品簡稱"欄位
///           2.下送資料時，”商品簡稱”取代原本的商品名稱下送到AS400的相關欄位，僅保留的”商品名稱”下送到Wanpie總帳欄位。
///           3.回寫AS400【FGLAACT0會計科目主檔】、【FGLGACT0 會計科目轉換檔】時，"ACT_NAME"改以"商品簡稱"組成。
/// -----------------------------------------------
/// 修改歷程：20200110 Daiyu
/// 需求單號：201906120463-00
/// 修改內容：「險種類別=5,6,7」時，需回寫基金險種接收【FFD_ITEM_ACCOUNT_HIS】
/// -----------------------------------------------
/// 修改歷程：20200730   daiyu
/// 需求單號：202007230106-02
/// 修改內容：繳款單帳務優化，覆核時，將會計科目加寫至【FPMCODE0】
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00005AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00005A/");
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

            FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            string[] apprMk = new string[] { "5" };
            rows = fGLItemInfoHisDao.qryByApprMk(apprMk, "OGL00005A", updDateB, updDateE, item, false);


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OGL00003Model d in rows)
                {
                    d.aplyDt = d.createDt;
                    d.aplyUId = d.createId;

                    createUid = d.aplyUId;

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);

                        d.aplyUId = createUid + " " + userNameMap[createUid];
                    }
                }
            }

            var jsonData = new { success = true, dataList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 查詢正式檔資料(for比對資料是否異動)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFormal(string item, string productType, string fuMk, string itemCon, string discPartFeat)
        {
            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            OGL00003Model formal = fGLItemInfoDao.qryItem(item, "6");
            //OGL00003Model formal = fGLItemInfoDao.qryPK(item, productType, fuMk, itemCon, discPartFeat, "6");

            if (formal != null)
                formal.effectYY = (Convert.ToInt16(formal.effectYY.Substring(0, 4)) - 1911).ToString();

            return Json(new { success = true, formal = formal }, JsonRequestBehavior.AllowGet);
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

            string[] acctType = new string[] {"B", "C" };
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

            FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
            prodSumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "P");
            investSumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "I");


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

            OGL00003Model modelHis = new OGL00003Model();

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


            bool bHaveData = false;
            try
            {
                var temp = StringUtil.toString(tempId).Split('|');
                string item = temp[0];
                string productType = temp[1];
                string fuMk = temp[2];
                string itemCon = temp[3];
                string discPartFeat = temp[4];

                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                modelHis = fGLItemInfoHisDao.qryItem(item);
                //modelHis = fGLItemInfoHisDao.qryItem(item, productType, fuMk, itemCon, discPartFeat);

                ViewBag.apprMkHis = StringUtil.toString(modelHis.apprMk);
                ViewBag.status = StringUtil.toString(modelHis.status);
                ViewBag.execAction = StringUtil.toString(modelHis.execAction);
                ViewBag.aplyNo = modelHis.aplyNo;

                ViewBag.isQryTmp = isQryTmp;
                ViewBag.bHaveData = "Y";


                if (!"O".Equals(modelHis.flag))
                {
                    //查詢"商品科目設定檔"

                    FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                    List<OGL00001Model> itemAcct = fGLItemAcctDao.qryHeadForItem(modelHis.productType, modelHis.fuMk, modelHis.itemCon, modelHis.discPartFeat);

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
                        modelHis.prodUpId = modelHis.prodUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.prodUpId, dbIntra).EMP_NAME);
                        modelHis.prodApprId = modelHis.prodApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.prodApprId, dbIntra).EMP_NAME);

                        modelHis.investUpId = modelHis.investUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.investUpId, dbIntra).EMP_NAME);
                        modelHis.investApprId = modelHis.investApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.investApprId, dbIntra).EMP_NAME);

                        modelHis.acctUpId = modelHis.acctUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.acctUpId, dbIntra).EMP_NAME);
                        modelHis.acctApprId = modelHis.acctApprId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.acctApprId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }

                ViewBag.itemHealthMgrMk = StringUtil.toString(modelHis.healthMgrType);
                modelHis.effectYY = (Convert.ToInt16(modelHis.effectYY.Substring(0, 4)) - 1911).ToString();

                return View(modelHis);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(modelHis);
            }
        }



        //private Dictionary<string, OGL00003DModel> insertSmpChkMap(Dictionary<string, OGL00003DModel> smpMap, string acctNum, OGL00003DModel d)
        //{
        //    if (!smpMap.ContainsKey(acctNum))
        //    {
        //        d.acctNum = acctNum;
        //        smpMap.Add(acctNum, d);
        //    }


        //    return smpMap;
        //}


        //private Dictionary<string, OGL00003DModel> getSmpList(OGL00003Model model)
        //{
        //    Dictionary<string, OGL00003DModel> smpMap = new Dictionary<string, OGL00003DModel>();

        //    //取得"商品科目設定"中的科目
        //    FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
        //    List<OGL00003DModel> flgItemList = fGLItemAcctDao.qryForOGL00005(
        //        model.productType, model.fuMk, model.itemCon, model.discPartFeat,
        //        model.comType, model.extSchedulType, model.lodprmType, model.pakindDmopType);

        //    foreach (OGL00003DModel d in flgItemList)
        //    {
        //        d.effectDate = (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString() + "/" + model.effectMM + "/" + model.effectDD;
        //        d.item = model.item;
        //        d.smpNumFrom = "itemAcct";

        //        //險種科目
        //        if (!"".Equals(model.itemAcct) &
        //            ("A".Equals(d.tempId) || ("S".Equals(d.tempId) & "1".Equals(d.acctType.Substring(1, 1)))))
        //        {
        //            OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.itemAcct, itemSmp);
        //        }



        //        //分離科目
        //        if (!"".Equals(model.separatAcct) &
        //            ("A".Equals(d.tempId) || ("S".Equals(d.tempId) & "2".Equals(d.acctType.Substring(1, 1)))))
        //        {
        //            OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.separatAcct, itemSmp);
        //        }


        //        //COI科目
        //        if (!"".Equals(model.coiAcct) &
        //            ("A".Equals(d.tempId) || ("S".Equals(d.tempId) & "3".Equals(d.acctType.Substring(1, 1)))))
        //        {
        //            OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.coiAcct, itemSmp);
        //        }
        //    }

        //    //取得"給付項目/費用"、"保單投資"中的科目
        //    List<OGL00003DModel> itemSmpList = new List<OGL00003DModel>();
        //    if (!"".Equals(model.aplyNo))
        //    {
        //        FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
        //        itemSmpList = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "");
        //    }
        //    else
        //    {
        //        FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
        //        itemSmpList = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "");
        //    }

        //    FGLSMPADao FGLSMPADao = new FGLSMPADao();

        //    foreach (OGL00003DModel d in itemSmpList)
        //    {
        //        if ("Y".Equals(d.flag)) {
        //            d.item = model.item;
        //            d.effectDate = (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString() + "/" + model.effectMM + "/" + model.effectDD;
        //            d.smpNumFrom = "itemSmp";

        //            if ("Y".Equals(model.fuMk))
        //                d.corpNo = d.acctType.Substring(1, 1) == "2" ? "7" : "3";
        //            else
        //                d.corpNo = d.acctType.Substring(1, 1) == "2" ? "9" : "1";


        //            FGLSMPADao.qryForOGL00005(d);

        //            //險種科目
        //            OGL00003DModel itemSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, itemSmp.smpNum + model.itemAcct, itemSmp);
        //            //分離科目
        //            OGL00003DModel separatSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, separatSmp.smpNum + model.separatAcct, separatSmp);
        //            //COI科目
        //            OGL00003DModel coiSmp = (OGL00003DModel)d.Clone();
        //            smpMap = insertSmpChkMap(smpMap, coiSmp.smpNum + model.coiAcct, coiSmp);
        //        }
        //    }


        //    //回AS400取得AS400科目中文及簡稱(LGLSMPL1)
        //    FGLSMPLDao fGLSMPLDao = new FGLSMPLDao();
        //    smpMap = fGLSMPLDao.qryForOGL00005(smpMap);

        //    //至"FGL_SMPB 科目樣本險種類別檔"取得科目名稱
        //    FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
        //    smpMap = fGLSMPBDao.qryForOGL00005(smpMap, model.itemName);


        //    return smpMap;
        //}


        private Dictionary<string, OGL00003DModel> chkInput(OGL00003Model model)
        {
            NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
            Dictionary<string, OGL00003DModel> smpMap = newAcctItemUtil.getSmpList("P", model);
            //Dictionary<string, OGL00003DModel> smpMap = getSmpList(model);

            //若屬"新增"商品時，需做「會計科目」檢查
            if ("A".Equals(model.execAction))
               // if ("A".Equals(model.execAction) & ("2".Equals(model.sysType) || "3".Equals(model.sysType)))
            {
                FGLAACTDao fGLAACTDao = new FGLAACTDao();
                smpMap = fGLAACTDao.qryForOGL00005(smpMap);
            }
            

            return smpMap;
        }


        /// <summary>
        /// 修改作業
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(string aplyNo, string execType)
        {
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction("Transaction");

                try
                {
                    FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();

                    OGL00003Model modelHis = new OGL00003Model();
                    modelHis = fGLItemInfoHisDao.qryAplyNo(aplyNo);

                    if (StringUtil.toString(modelHis.acctUpId).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    FGL_ITEM_CODE_TRAN tran = new FGL_ITEM_CODE_TRAN();
                    Dictionary<string, OGL00003DModel> smpMap = new Dictionary<string, OGL00003DModel>();

                    //由OGL0003 新增的商品 需做下列檢核
                    if (!"O".Equals(modelHis.flag)) {
                        smpMap = chkInput(modelHis);

                        //檢查是否有"商品編號代碼轉換"資料
                        if ("C".Equals(execType))
                        {
                            FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();
                            tran = fGLItemCodeTranDao.qryForWanpie(modelHis.prodNo);
                            if (tran == null)
                                return Json(new { success = false, errors = "無商品編號代碼轉換資料!!" }, JsonRequestBehavior.AllowGet);



                            //若屬"新增"商品時，需做「會計科目」檢查
                            if ("A".Equals(modelHis.execAction))
                                //if ("A".Equals(modelHis.execAction) & ("2".Equals(modelHis.sysType) || "3".Equals(modelHis.sysType)))
                                {
                                List<string> errSmpList = new List<string>();
                                foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                                {
                                    if (item.Value.bExist)
                                        errSmpList.Add(item.Key);
                                }

                                if (errSmpList.Count > 0)
                                    return Json(new { success = false, errors = "以下會計科目重覆，請確認", errSmp = errSmpList }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }

                    string apprMk = execType == "R" ? "13" : "6";

                    fGLItemInfoHisDao.updateApprMk(aplyNo, apprMk, "OGL00005A", Session["UserID"].ToString(), execType, conn, transaction);


                    //處理"核可"的資料
                    if("C".Equals(execType))
                        procFormal(aplyNo, modelHis, smpMap, tran, conn, transaction);


                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    string errString = e.ToString();
                    if (errString.ToUpper().IndexOf("PRIMARY KEY") > 0 & errString.ToUpper().IndexOf("FFD_ITEM_ACCOUNT_HIS") > 0)
                        return Json(new { success = false, errors = "FD基金系統尚未接收，請向承辦確認" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        //處理"核可"的資料
        private void procFormal(string aplyNo, OGL00003Model main, Dictionary<string, OGL00003DModel> smpMap, FGL_ITEM_CODE_TRAN tran
            , SqlConnection conn, SqlTransaction transaction) {

            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            OGL00003Model formal = new OGL00003Model();

            if (!"A".Equals(main.execAction)) {
                formal = fGLItemInfoDao.qryItem(main.item, "6");
                //formal = fGLItemInfoDao.qryPK(main.item, main.productType, main.fuMk, main.itemCon, main.discPartFeat, "6");
            }
              
            Dictionary<string, OGL00003DModel> smpMapFormal = new Dictionary<string, OGL00003DModel>();

            //由OGL0003 新增的商品
            if (!"O".Equals(main.flag)) {
                NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
                smpMapFormal = newAcctItemUtil.getSmpList("P", formal);
            }
            //smpMapFormal = getSmpList(formal);


            /*---判斷AS400或總帳資料是否需同步異動 begin---*/
            bool bAs400Acct = false;
            bool bAs400item = false;
            bool bWanpie = false;
            bool fFd = false;


            List<FGLAACT0Model> aactInvalidList = new List<FGLAACT0Model>();


            //add by daiyu 20200110
            Dictionary<string, string> corpNoMap = new Dictionary<string, string>();
            Dictionary<string, string> corpNoFormalMap = new Dictionary<string, string>();
            if ("5".Equals(main.productType) || "6".Equals(main.productType) || "7".Equals(main.productType)) {
                
                //暫存檔的保單投資項目
                foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                {
                    if (item.Value.acctType.StartsWith("C")) {  //取屬保單的GA、SA項目
                        if (!corpNoMap.ContainsKey(item.Value.corpNo)) {
                            if("2".Equals(item.Value.acctType.Substring(1, 1)))
                                corpNoMap.Add(item.Value.corpNo, main.separatAcct); //SA項目-->以"分離帳科目"
                            else
                                corpNoMap.Add(item.Value.corpNo, main.itemAcct);    //GA項目-->以"險種科目"
                        }
                    }
                }

                //正式檔的保單投資項目
                foreach (KeyValuePair<string, OGL00003DModel> item in smpMapFormal)
                {
                    if (item.Value.acctType.StartsWith("C"))
                    {  //取屬保單的GA、SA項目
                        if (!corpNoFormalMap.ContainsKey(item.Value.corpNo))
                        {
                            if ("2".Equals(item.Value.acctType.Substring(1, 1)))
                                corpNoFormalMap.Add(item.Value.corpNo, main.separatAcct); //SA項目-->以"分離帳科目"
                            else
                                corpNoFormalMap.Add(item.Value.corpNo, main.itemAcct);    //GA項目-->以"險種科目"
                        }
                    }
                }



                if ("A".Equals(main.execAction) || "D".Equals(main.execAction))
                    fFd = true;
                else {
                    if (!"O".Equals(main.flag))
                        fFd = true;
                }
            }
            //end add 20200110


            if ("A".Equals(main.execAction) || "D".Equals(main.execAction))
            {
                bAs400Acct = true;
                bAs400item = true;
                bWanpie = true;
            }
            else {

                if ("O".Equals(main.flag))
                {
                    //由AS400 新增的商品
                    bAs400Acct = false;
                    bAs400item = true;
                    bWanpie = false;
                }
                else
                {
                    //由OGL0003 新增的商品
                    if (!main.sysType.Equals(formal.sysType) || !main.prodNoVer.Equals(formal.prodNoVer) || !main.prodNo.Equals(formal.prodNo))
                        bAs400item = true;

                    if (!main.prodNo.Equals(formal.prodNo) || !main.sysType.Equals(formal.sysType) || !main.itemMainType.Equals(formal.itemMainType)
                        || !main.investType.Equals(formal.investType) || !main.insTerm.Equals(formal.insTerm) || !main.busiType.Equals(formal.busiType)
                        || !main.itemAcct.Equals(formal.itemAcct) || !main.separatAcct.Equals(formal.separatAcct) || !main.coiAcct.Equals(formal.coiAcct)
                        || !main.itemName.Equals(formal.itemName) || !main.itemCon.Equals(formal.itemCon)
                        )
                        bWanpie = true;

                    if (!main.comType.Equals(formal.comType) || !main.extSchedulType.Equals(formal.extSchedulType) || !main.pakindDmopType.Equals(formal.pakindDmopType)
                        || !main.lodprmType.Equals(formal.lodprmType) || !main.itemName.Equals(formal.itemName) || !main.itemNameShrt.Equals(formal.itemNameShrt) || !main.effectDate.Equals(formal.effectDate)
                        || !main.itemAcct.Equals(formal.itemAcct) || !main.separatAcct.Equals(formal.separatAcct) || !main.coiAcct.Equals(formal.coiAcct)
                        )
                        bAs400Acct = true;


                    string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
                    string nowYy = nowStr[0].Substring(0, 4);
                    string nowMm = nowStr[0].Substring(4, 2);
                    string nowDd = nowStr[0].Substring(6, 2);

                    foreach (KeyValuePair<string, OGL00003DModel> item in smpMapFormal)
                    {
                        if (!smpMap.ContainsKey(item.Key))
                        {
                            FGLAACT0Model fGLAACT0Model = new FGLAACT0Model();
                            fGLAACT0Model.corpNo = item.Value.corpNo;
                            fGLAACT0Model.actNum = item.Value.acctNum;
                            fGLAACT0Model.updId = Session["UserID"].ToString();
                            fGLAACT0Model.updYy = nowYy;
                            fGLAACT0Model.updMm = nowMm;
                            fGLAACT0Model.updDd = nowDd;
                            aactInvalidList.Add(fGLAACT0Model);
                        }
                    }

                    if (aactInvalidList.Count > 0)
                    {
                        bAs400Acct = true;
                    }


                    if (!bAs400Acct)
                    {
                        foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                        {
                            if (!smpMapFormal.ContainsKey(item.Key))
                            {
                                bAs400Acct = true;
                                break;
                            }
                        }
                    }
                }
            }

                   


            /*---判斷AS400或總帳資料是否需同步異動 end---*/



            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                EacTransaction transaction400 = conn400.BeginTransaction();

                try
                {
                    /*---處理AS400相關檔案---*/
                    //1.FGLAACT0 會計科目主檔
                    //2.FGLGACT0 會計科目轉換檔
                    //3.FGLITEM0 險種會計科目定義檔
                    //4.FGLGITE0 會計商品資訊檔
                    //5.FGLGRAT0 佣薪獎稅務別 TABLE 檔
                    if (bAs400Acct || bAs400item)
                        procAs400File(main, formal, smpMap, smpMapFormal, aactInvalidList, tran, bAs400Acct, bAs400item, conn400, transaction400);


                    /*---處理OPEN端相關檔案---*/
                    //1.FGL_ITEM_INFO 會計商品資訊檔
                    //2.FGL_ITEM_SMPNUM 會計商品給付項目明細檔
                    procOpenDb(aplyNo, main, formal, conn, transaction);


                    #region 處理總帳相關檔案
                    string strConn = DbUtil.GetDBGlsiExtConnStr();
                    using (SqlConnection connGlsi = new SqlConnection(strConn))
                    {
                        connGlsi.Open();
                        SqlTransaction transactionGlsi = connGlsi.BeginTransaction("Transaction");

                        try
                        {
                            if (bWanpie)
                                procWanpie(main, formal, smpMap, tran, connGlsi, transactionGlsi);


                            /*---處理基金相關檔案 add by daiyu 20200110---*/
                            if (fFd)
                            {
                                string strConnFD = DbUtil.GetDBFdConnStr();
                                using (SqlConnection connFd = new SqlConnection(strConnFD))
                                {
                                    connFd.Open();
                                    SqlTransaction transactionFd = connFd.BeginTransaction("Transaction");
                                    try
                                    {
                                        /*---處理基金相關檔案---*/
                                        procOpenFDDb(main, formal, corpNoMap, corpNoFormalMap, connFd, transactionFd);

                                        transactionFd.Commit();
                                    }
                                    catch (Exception e)
                                    {
                                        throw e;
                                    }

                                }
                            }


                            transactionGlsi.Commit();

                        }
                        catch (Exception e) {
                            throw e;
                        }
                    }
                    #endregion

                    transaction400.Commit();

                }
                catch (Exception e) {
                    transaction400.Rollback();
                    throw e;
                }
            }
        }


        /// <summary>
        /// 處理基金相關檔案
        /// </summary>
        /// <param name="main"></param>
        /// <param name="smpMap"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procOpenFDDb(OGL00003Model main, OGL00003Model formal, Dictionary<string, string> corpNoMap, Dictionary<string, string> corpNoFormalMap, SqlConnection conn, SqlTransaction transaction) {
            List<string> corpNoList = new List<string>();

            Dictionary<string, string> corpNoAddMap = new Dictionary<string, string>();
            Dictionary<string, string> corpNoDelMap = new Dictionary<string, string>();
            Dictionary<string, string> corpNoUpdMap = new Dictionary<string, string>();

            
            if ("U".Equals(main.execAction))
            {
                foreach (KeyValuePair<string, string> item in corpNoMap)    //查本次新增、不變的帳本
                {
                    if (corpNoFormalMap.ContainsKey(item.Key))
                        corpNoUpdMap.Add(item.Key, item.Value);
                    else
                        corpNoAddMap.Add(item.Key, item.Value);
                }


                foreach (KeyValuePair<string, string> item in corpNoFormalMap)  //查本次刪除的帳本
                {
                    if (!corpNoMap.ContainsKey(item.Key))
                        corpNoDelMap.Add(item.Key, item.Value);
                }
            }
          


            //檢查是否需銜接基金
            bool bCallFd = false;

            if ("U".Equals(main.execAction)) {
                if (!main.itemAcct.Equals(formal.itemAcct) || !main.separatAcct.Equals(formal.separatAcct)
                    || !main.effectDate.Equals(formal.effectDate) || !main.itemName.Equals(formal.itemName))
                    bCallFd = true;

                if(corpNoAddMap.Count > 0 || corpNoDelMap.Count > 0)
                    bCallFd = true;
            } else
                bCallFd = true;

            if (!bCallFd)
                return;


            FFDItemAccountHisDao ffdDao = new FFDItemAccountHisDao();

            switch (main.execAction) {
                case "A":
                    foreach (KeyValuePair<string, string> item in corpNoMap)
                    {
                        FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                        d = genFFDData(d, "A", main, item, DateUtil.stringToDatetime("9999/12/31"));
                        ffdDao.insert(d, conn, transaction);
                    }

                    break;
                case "D":
                    foreach (KeyValuePair<string, string> item in corpNoFormalMap)
                    {
                        FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                        //d = genFFDData(d, "U", main, item, DateTime.Now);
                        d = genFFDData(d, "U", main, item, DateUtil.stringToDatetime(main.effectDate));
                        ffdDao.insert(d, conn, transaction);
                    }

                    break;
                case "U":
                    //本次新增的帳本
                    foreach (KeyValuePair<string, string> item in corpNoAddMap)
                    {
                        FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                        d = genFFDData(d, "A", main, item, DateUtil.stringToDatetime("9999/12/31"));
                        ffdDao.insert(d, conn, transaction);
                    }

                    //本次刪除的帳本
                    foreach (KeyValuePair<string, string> item in corpNoDelMap)
                    {
                        FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                        d = genFFDData(d, "U", main, item, DateTime.Now);
                        ffdDao.insert(d, conn, transaction);
                    }

                    //本次異動的資料
                    foreach (KeyValuePair<string, string> item in corpNoUpdMap)
                    {
                        FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                        d = genFFDData(d, "U", main, item, DateUtil.stringToDatetime("9999/12/31"));
                        ffdDao.insert(d, conn, transaction);
                    }

                    break;
            }


            foreach (string cropNo in corpNoList.Distinct()) {

                FFD_ITEM_ACCOUNT_HIS d = new FFD_ITEM_ACCOUNT_HIS();
                d.aply_no = "B99999999999";
                d.exec_action = "";
                d.item = main.item;
                d.item_desc = main.itemName;
                d.item_kind = main.productType;
                d.fund_ac_corp_no = cropNo;
                d.item_account = main.itemAcct;
                d.item_transfer = "N";
                d.item_effective_date = DateUtil.stringToDatetime(main.effectDate);
                d.item_expiration_date = DateUtil.stringToDatetime("9999/12/31");

                ffdDao.insert(d, conn, transaction);
            }

        }



        private FFD_ITEM_ACCOUNT_HIS genFFDData(FFD_ITEM_ACCOUNT_HIS d, string execAction, OGL00003Model main
            , KeyValuePair<string, string> item, DateTime item_expiration_date) {
            d.aply_no = "B99999999999";
            d.exec_action = execAction;
            d.item = main.item;
            d.item_desc = main.itemName;
            d.item_kind = main.productType;
            d.fund_ac_corp_no = item.Key;
            d.item_account = item.Value;
            d.item_transfer = "N";
            d.item_effective_date = DateUtil.stringToDatetime(main.effectDate);
            d.item_expiration_date = item_expiration_date;

            return d;
        }



        /// <summary>
        /// 處理下送總帳的資料
        /// </summary>
        /// <param name="main"></param>
        /// <param name="formal"></param>
        /// <param name="smpMap"></param>
        /// <param name="tran"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procWanpie(OGL00003Model main, OGL00003Model formal, Dictionary<string, OGL00003DModel> smpMap, FGL_ITEM_CODE_TRAN tran
            , SqlConnection conn, SqlTransaction transaction) {
            //int updateCnt = 0;
            //string effDate = ""; 

            //if (!"A".Equals(main.execAction)) {
            //    FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            //    updateCnt = fGLItemInfoHisDao.qryUpdateCnt(main.item, main.productType, main.fuMk, main.itemCon, main.discPartFeat);

            //    if (!main.effectDate.Equals(formal.effectDate)) {
            //        if (updateCnt == 0)
            //            effDate = formal.effectDate;
            //        else {
            //            effDate = fGLItemInfoHisDao.qryOEffDate(main.item, main.productType, main.fuMk, main.itemCon, main.discPartFeat);
            //        }
            //    }
            //}
            //if("".Equals(effDate))
            //    effDate = DateUtil.DatetimeToString(Convert.ToDateTime(formal.effectDate).AddDays(updateCnt), "yyyy/MM/dd");


            List<ItemWanpieModel> wanpieList = new List<ItemWanpieModel>();

            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            //取得表單編號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("FGL", "XAC" + StringUtil.toString(tran.tran_k), qPreCode).ToString();
            cId = "XAC" + qPreCode + cId.ToString().PadLeft(5, '0');


            switch (main.sysType) {
                case "1":
                    if (!"".Equals(main.itemAcct))
                        wanpieList = insertWanpieList(cId, "A", main.itemAcct, smpMap, wanpieList, main, formal, tran);

                    if (!"".Equals(main.separatAcct))
                        wanpieList = insertWanpieList(cId, "A", main.separatAcct, smpMap, wanpieList, main, formal, tran);

                    if (!"".Equals(main.coiAcct))
                        wanpieList = insertWanpieList(cId, "A", main.coiAcct, smpMap, wanpieList, main, formal, tran);

                    break;

                case "2":
                    if (!"".Equals(main.itemAcct))
                        wanpieList = insertWanpieList(cId, "F", main.itemAcct, smpMap, wanpieList, main, formal, tran);

                    if (!"".Equals(main.separatAcct))
                        wanpieList = insertWanpieList(cId, "F", main.separatAcct, smpMap, wanpieList, main, formal, tran);

                    if (!"".Equals(main.coiAcct))
                        wanpieList = insertWanpieList(cId, "F", main.coiAcct, smpMap, wanpieList, main, formal, tran);

                    break;

                case "3":
                    if (!"".Equals(main.itemAcct)) {
                        wanpieList = insertWanpieList(cId, "A", main.itemAcct, smpMap, wanpieList, main, formal, tran);
                        wanpieList = insertWanpieList(cId, "F", main.itemAcct, smpMap, wanpieList, main, formal, tran);
                    }

                    if (!"".Equals(main.separatAcct)) {
                        wanpieList = insertWanpieList(cId, "A", main.separatAcct, smpMap, wanpieList, main, formal, tran);
                        wanpieList = insertWanpieList(cId, "F", main.separatAcct, smpMap, wanpieList, main, formal, tran);
                    }

                    if (!"".Equals(main.coiAcct)) {
                        wanpieList = insertWanpieList(cId, "A", main.coiAcct, smpMap, wanpieList, main, formal, tran);
                        wanpieList = insertWanpieList(cId, "F", main.coiAcct, smpMap, wanpieList, main, formal, tran);
                    }

                    break;
            }

            if (wanpieList.Count > 0) {
                XAC279350738548Dao xAC279350738548Dao = new XAC279350738548Dao();
                xAC279350738548Dao.insert(wanpieList, conn, transaction);
            }
                

        }



        /// <summary>
        /// 準備要寫入總帳的資料
        /// </summary>
        /// <param name="sysType"></param>
        /// <param name="actnumLastSix"></param>
        /// <param name="smpMap"></param>
        /// <param name="wanpieList"></param>
        /// <param name="main"></param>
        /// <param name="formal"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        private List<ItemWanpieModel> insertWanpieList(string voucherNo, string sysType, string actnumLastSix, Dictionary<string, OGL00003DModel> smpMap
            , List<ItemWanpieModel> wanpieList, OGL00003Model main, OGL00003Model formal, FGL_ITEM_CODE_TRAN tran)
        {

            ItemWanpieModel d = new ItemWanpieModel();

            //覆核單號
            d.aplyNo = main.aplyNo;
            //系統別
            d.aSysType = sysType;
            //會科後六碼
            d.aActnumLastsix = actnumLastSix;
            //險種
            d.aInsurPolicyItem = main.item;

            if (!"D".Equals(main.execAction))
            {
                //險種中文名稱
                d.aInsurPolicyObjectName = main.itemName;
                //保發險別大分類
                d.aInsurPolicyMajorCategories = StringUtil.toString(tran.tran_a).PadRight(2, ' ').Substring(0, 1).Trim();
                //險種細分類
                d.aInsurPolicySubdivisions = StringUtil.toString(tran.tran_a);
                //保發個人/團體別
                d.aInsurPolicyPersonGroup = StringUtil.toString(tran.tran_b);
                //保發契約別
                d.aContractType = StringUtil.toString(tran.tran_c);
                //保發傳統/非傳統
                d.aInsurPolicyTradition = StringUtil.toString(tran.tran_d);
                //保發業務性質
                d.aInsurPolicyBusinessObject = main.busiType;
                //保發主/附約
                d.aInsurPolicyMainOrRider = StringUtil.toString(tran.tran_e);
                //保發保險商品類別1
                d.aInsurPolicyObjectType1 = StringUtil.toString(tran.tran_f).PadRight(2, ' ').Substring(0, 1).Trim();

                //險種商品類型2代碼
                //20190708 健康管理保險商品=Y時，險種類型2需改為06
                if ("Y".Equals(StringUtil.toString(main.healthMgrType)))
                {
                    d.aInsurPolicyObjectType2 = "06";
                }
                else {
                    if (StringUtil.toString(tran.tran_f).Trim().Length == 1)
                        d.aInsurPolicyObjectType2 = StringUtil.toString(tran.tran_f) + main.investType;
                    else
                        d.aInsurPolicyObjectType2 = StringUtil.toString(tran.tran_f);
                }
                

                //險種主險別
                d.aInsurPolicyMainInsurType = main.itemMainType;
                //保發危險分類
                d.aInsurPolicyDangerType = StringUtil.toString(tran.tran_g);
                //產品編號
                d.aInsurPolicyObjectNo = main.prodNo;
                //分紅商品
                d.aInsurPolicyIsPar = "N";
                //速報類別1
                d.aInsurPolicyRapidReportCategory1 = main.productType == "2" ? "E" : StringUtil.toString(tran.tran_j);


                //投資合約/保險合約
                //20190806 GL00003的合約分類新增：3.投資&保險合約，下送到Wanpie(需求確認P36)的第19項統一帶入1.投資合約 (莉婷)
                d.aInvestmentInsuranceContract = main.itemCon == "3" ? "1" : main.itemCon;

                //生存滿期註記
                if ("4".Equals(d.aInsurPolicyMajorCategories))
                    d.fieldCha1 = "0";
                else
                {
                    foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                    {
                        if (item.Key.StartsWith("5510") || item.Key.StartsWith("2621") || item.Key.StartsWith("5514") || item.Key.StartsWith("2624"))
                        {
                            d.fieldCha1 = "1";
                            break;
                        }
                        else
                            d.fieldCha1 = "0";
                    }
                }

                //險種年滿期/歲滿期代號
                if (!"".Equals(StringUtil.toString(tran.tran_h)))
                    d.aYmiCode = StringUtil.toString(tran.tran_h);
                else
                {
                    switch (main.insTerm)
                    {
                        case "A":
                            d.aYmiCode = "A00";
                            break;
                        case "B":
                            d.aYmiCode = "B01";
                            break;
                        case "G":
                            d.aYmiCode = "G01";
                            break;
                        default:
                            d.aYmiCode = main.insTerm;
                            break;
                    }
                }

                //審查/特殊商品/給付別
                d.aExamineSpecialPayment = StringUtil.toString(tran.tran_i);

                //公司代碼
                if ("O".Equals(StringUtil.toString(tran.tran_k)))
                    d.corpNo = "OIU";
                else
                    d.corpNo = "FUBONLIFE";

                //表單日期
                d.voucherDate = DateUtil.getCurDate("yyyy/MM/dd");

                //生效日
                d.startDate = main.effectDate;

                //表單編號  
                d.voucherNo = voucherNo;

                //截止日期
                d.endDate = null;
            }
            else {
                //表單日期
                d.voucherDate = null;
                //生效日
                d.startDate = null;
                //生效日
                d.endDate = null;
            }

            

            wanpieList.Add(d);
            return wanpieList;

        }



        /// <summary>
        /// 處理OPEN端相關檔案
        /// 1.FGL_ITEM_INFO 會計商品資訊檔
        /// 2.FGL_ITEM_SMPNUM 會計商品給付項目明細檔
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="main"></param>
        /// <param name="formal"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procOpenDb(string aplyNo, OGL00003Model main, OGL00003Model formal, SqlConnection conn, SqlTransaction transaction) {
            FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
            FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();

            switch (main.execAction) {
                case "A":
                    //FGL_ITEM_INFO 會計商品資訊檔
                    fGLItemInfoDao.insertFormHis(aplyNo, conn, transaction);

                    //FGL_ITEM_SMPNUM 會計商品給付項目明細檔
                    fGLItemSmpnumDao.insertFromHis(aplyNo, conn, transaction);

                    break;
                case "U":
                    main.acctApprId = Session["UserID"].ToString();
                    main.acctApprDt = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");

                    //FGL_ITEM_INFO 會計商品資訊檔
                    fGLItemInfoDao.update(main, formal, conn, transaction);

                    //FGL_ITEM_SMPNUM 會計商品給付項目明細檔
                    if (!"O".Equals(main.flag)) {
                        fGLItemSmpnumDao.deleteByItem(main.item, main.productType, main.fuMk, main.itemCon, main.discPartFeat, conn, transaction);
                        fGLItemSmpnumDao.insertFromHis(aplyNo, conn, transaction);
                    }
                    

                    break;

                case "D":
                    main.acctApprId = Session["UserID"].ToString();
                    main.acctApprDt = DateUtil.getCurDateTime("yyyy/MM/dd HH:mm:ss");
                    main.apprMk = "7";

                    //FGL_ITEM_INFO 會計商品資訊檔
                    fGLItemInfoDao.updateApprMk(main, formal, conn, transaction);

                    break;
            }
            

        }




        /// <summary>
        /// 處理AS400相關檔案
        /// 1.FGLAACT0 會計科目主檔
        /// 2.FGLGACT0 會計科目轉換檔
        /// 3.FGLITEM0 險種會計科目定義檔
        /// </summary>
        /// <param name="main"></param>
        /// <param name="formal"></param>
        /// <param name="smpMap"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procAs400File(OGL00003Model main, OGL00003Model formal, Dictionary<string, OGL00003DModel> smpMap
            , Dictionary<string, OGL00003DModel> smpMapFormal, List<FGLAACT0Model> aactInvalidList, FGL_ITEM_CODE_TRAN tran
            , bool bAs400Acct, bool bAs400item
            , EacConnection conn, EacTransaction transaction)
        {
            try
            {
                if (bAs400Acct)
                {

                    ////「進件系統別 = 2.F 或 3.A&F」時，才需處理【FGLAACT0 會計科目主檔】、【FGLITEM0 險種會計科目定義檔】
                    //if (("2".Equals(main.sysType) || "3".Equals(main.sysType))) {
                    //    //FGLAACT0 會計科目主檔
                    //    procFGLAACT0(main, formal, smpMap, smpMapFormal, aactInvalidList, conn, transaction);

                    //    //FGLITEM0 險種會計科目定義檔
                    //    procFGLITEM0(main, smpMap, conn, transaction);
                    //}

                    //FGLAACT0 會計科目主檔
                    procFGLAACT0(main, formal, smpMap, smpMapFormal, aactInvalidList, conn, transaction);

                    //FGLITEM0 險種會計科目定義檔
                    procFGLITEM0(main, smpMap, conn, transaction);

                    //FGLGACT0 會計科目轉換檔
                    procFGLGACT0(main.execAction, smpMap, conn, transaction);

                    //FGLGRAT0 佣薪獎稅務別 TABLE 檔
                    procFGLGRAT0(main.execAction, smpMap, conn, transaction);

                    //FPMCODE0 各類相關代碼檔  add by daiyu 20200730
                    //「進件系統別 = 2.F 或 3.A&F」時，才需處理【FPMCODE0 各類相關代碼檔】
                    if (("2".Equals(main.sysType) || "3".Equals(main.sysType)))
                        procFPMCODE0(main, tran, conn, transaction);


                }

                if (bAs400item) {
                    //FGLGITE0 會計商品資訊檔
                    procFGLGITE0(main, conn, transaction);
                }


            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw e;
            }
        }


        /// <summary>
        /// AS400 FGLGRAT0 佣薪獎稅務別 TABLE 檔
        /// </summary>
        /// <param name="main"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFGLGRAT0(string execAction, Dictionary<string, OGL00003DModel>  smpMap, EacConnection conn, EacTransaction transaction)
        {
            FGLGRATDao fGLGRATDao = new FGLGRATDao();

            List<FGLGRAT0Model> dataList = new List<FGLGRAT0Model>();

            string[] nowDateTime = DateUtil.getCurChtDateTime().Split(' ');

            foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
            {
                if (item.Value.acctNum.StartsWith("5503") || item.Value.acctNum.StartsWith("5504")) {
                    FGLGRAT0Model fGLGRAT0Model = new FGLGRAT0Model();
                    fGLGRAT0Model.actNum = item.Value.acctNum;
                    fGLGRAT0Model.taxType = "500AA";
                    fGLGRAT0Model.entryId = Session["UserID"].ToString();
                    fGLGRAT0Model.entryDate = nowDateTime[0];
                    fGLGRAT0Model.entryTime = nowDateTime[1];

                    dataList.Add(fGLGRAT0Model);
                }
                
            }

            if(dataList.Count > 0)
                fGLGRATDao.proc(execAction, dataList, conn, transaction);
        }




        /// <summary>
        /// AS400 FGLGITE0 會計商品資訊檔
        /// </summary>
        /// <param name="main"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFGLGITE0(OGL00003Model main, EacConnection conn, EacTransaction transaction)
        {
            FGLGITEDao fGLGITEDao = new FGLGITEDao();

            List<FGLGITE0Model> dataList = new List<FGLGITE0Model>();

            string[] nowDateTime = DateUtil.getCurChtDateTime().Split(' ');

            //A系統
            if ("1".Equals(main.sysType) || "3".Equals(main.sysType)) {
                FGLGITE0Model fGLGITE0Model = new FGLGITE0Model();
                fGLGITE0Model.sysType = "A";
                fGLGITE0Model.item = main.item;
                fGLGITE0Model.numVrsn = main.prodNoVer;
                fGLGITE0Model.num = main.prodNo;
                fGLGITE0Model.updId = Session["UserID"].ToString();
                fGLGITE0Model.updDate = nowDateTime[0];
                fGLGITE0Model.updTime = nowDateTime[1];

                dataList.Add(fGLGITE0Model);
            }


            //F系統
            if ("2".Equals(main.sysType) || "3".Equals(main.sysType))
            {
                FGLGITE0Model fGLGITE0Model = new FGLGITE0Model();
                fGLGITE0Model.sysType = "F";
                fGLGITE0Model.item = main.item;
                fGLGITE0Model.numVrsn = main.prodNoVer;
                fGLGITE0Model.num = main.prodNo;
                fGLGITE0Model.updId = Session["UserID"].ToString();
                fGLGITE0Model.updDate = nowDateTime[0];
                fGLGITE0Model.updTime = nowDateTime[1];

                dataList.Add(fGLGITE0Model);
            }


            fGLGITEDao.proc(main.execAction, dataList, conn, transaction);


        }


        /// <summary>
        /// FPMCODE0 各類相關代碼檔  add by daiyu 20200730
        /// </summary>
        /// <param name="main"></param>
        /// <param name="tran"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFPMCODE0(OGL00003Model main, FGL_ITEM_CODE_TRAN tran, EacConnection conn, EacTransaction transaction) {
            FPMCODEDao fPMCODEDao = new FPMCODEDao();

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            string nowYy = nowStr[0].Substring(0, 4);
            string nowMm = nowStr[0].Substring(4, 2);
            string nowDd = nowStr[0].Substring(6, 2);
            
            //合約別
            var aInvestmentInsuranceContract = main.itemCon == "3" ? "1" : main.itemCon;

            //商品別
            var aInsurPolicyTradition = StringUtil.toString(tran.tran_d);

            FPMCODEModel fPMCODEModel = new FPMCODEModel();
            fPMCODEModel.groupId = "RM-CODE1";
            fPMCODEModel.srce_from = "RT";
            fPMCODEModel.textLen = "20";
            fPMCODEModel.refNo = main.item;
            fPMCODEModel.text = StringUtil.toString(aInvestmentInsuranceContract).PadLeft(1, ' ')
                + StringUtil.toString(aInsurPolicyTradition).PadLeft(1, ' ')
                + StringUtil.toString(main.itemAcct).PadLeft(6, ' ')
                + StringUtil.toString(main.separatAcct).PadLeft(6, ' ')
                + StringUtil.toString(main.coiAcct).PadLeft(6, ' ');


            switch (main.execAction) {
                case "A":
                    fPMCODEModel.entryId = Session["UserID"].ToString();
                    fPMCODEModel.entryYy = nowYy;
                    fPMCODEModel.entryMm = nowMm;
                    fPMCODEModel.entryDd = nowDd;
                    fPMCODEModel.entryTime = nowStr[1];
                    fPMCODEDao.insert(fPMCODEModel, conn, transaction);
                    break;

                case "D":
                    fPMCODEDao.delete(fPMCODEModel, conn, transaction);
                    break;

                default:
                    fPMCODEModel.updId = Session["UserID"].ToString();
                    fPMCODEModel.updYy = nowYy;
                    fPMCODEModel.updMm = nowMm;
                    fPMCODEModel.updDd = nowDd;
                    fPMCODEDao.update(fPMCODEModel, conn, transaction);
                    break;
            }
        }



        /// <summary>
        /// AS400 FGLAACT0 會計科目主檔
        /// </summary>
        /// <param name="main"></param>
        /// <param name="formal"></param>
        /// <param name="smpMap"></param>
        /// <param name="smpMapFormal"></param>
        /// <param name="aactInvalidList"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFGLAACT0(OGL00003Model main, OGL00003Model formal, Dictionary<string, OGL00003DModel> smpMap
            , Dictionary<string, OGL00003DModel> smpMapFormal, List<FGLAACT0Model> aactInvalidList
            , EacConnection conn, EacTransaction transaction)
        {
            FGLAACTDao fGLAACTDao = new FGLAACTDao();
            List<FGLAACT0Model> aactList = new List<FGLAACT0Model>();

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            string nowYy = nowStr[0].Substring(0, 4);
            string nowMm = nowStr[0].Substring(4, 2);
            string nowDd = nowStr[0].Substring(6, 2);

            foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
            {
                FGLAACT0Model fGLAACT0Model = new FGLAACT0Model();
                fGLAACT0Model.corpNo = item.Value.corpNo;
                fGLAACT0Model.actNum = item.Value.acctNum;
                fGLAACT0Model.actType = "1";
                fGLAACT0Model.actAtrb = item.Key.Substring(0, 1);

                switch (fGLAACT0Model.actAtrb) {
                    case "2":
                    case "3":
                    case "4":
                        fGLAACT0Model.dbSign = "C";
                        break;
                    case "1":
                    case "5":
                    case "6":
                        fGLAACT0Model.dbSign = "D";
                        break;
                }

                fGLAACT0Model.actName = item.Value.smpName;
                fGLAACT0Model.actShort = item.Value.smpNameShort;
                fGLAACT0Model.actEngl = fGLAACT0Model.corpNo == "1" ? "" : fGLAACT0Model.corpNo.PadLeft(60, ' ');
                fGLAACT0Model.preRemark = "N";
                fGLAACT0Model.rmRemark = "N";
                fGLAACT0Model.unitRemrk = "N";
                fGLAACT0Model.itemRemrk = "N";


                if ("E11".Equals(item.Value.acctType) || "F11".Equals(item.Value.acctType))
                    fGLAACT0Model.famtRemrk = "N";
                else
                    fGLAACT0Model.famtRemrk = main.fuMk;


                fGLAACT0Model.fnoRemrk = "N";
                fGLAACT0Model.bonuRemrk = "N";
                fGLAACT0Model.effYy = (Convert.ToInt16(main.effectYY) - 1911).ToString();
                fGLAACT0Model.effMm = main.effectMM;
                fGLAACT0Model.effDd = main.effectDD;
                fGLAACT0Model.updId = Session["UserID"].ToString();
                fGLAACT0Model.updYy = nowYy;
                fGLAACT0Model.updMm = nowMm;
                fGLAACT0Model.updDd = nowDd;

                aactList.Add(fGLAACT0Model);
            }


            if("D".Equals(main.execAction))
                fGLAACTDao.procInvalid("D",aactList, conn, transaction); 
            else
                fGLAACTDao.proc(aactList, conn, transaction);


            //處理失效的科目
            if ("U".Equals(main.execAction) & aactInvalidList.Count > 0)
                fGLAACTDao.procInvalid("U", aactInvalidList, conn, transaction);


        }


        /// <summary>
        /// AS400 FGLGACT0 會計科目轉換檔
        /// </summary>
        /// <param name="smpMap"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFGLGACT0(string execAction, Dictionary<string, OGL00003DModel> smpMap
            , EacConnection conn, EacTransaction transaction)
        {
            FGLGACTDao fGLGACTDao = new FGLGACTDao();
            List<FGLGACT0Model> gactList = new List<FGLGACT0Model>();

            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');


            foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
            {
                if (!"".Equals(StringUtil.toString(item.Value.sqlSmpNum))) {
                    FGLGACT0Model fGLGACT0Model = new FGLGACT0Model();
                    fGLGACT0Model.actNum = item.Value.acctNum;
                    fGLGACT0Model.actName = item.Value.smpName;
                    fGLGACT0Model.sqlActnum = item.Value.sqlSmpNum;
                    fGLGACT0Model.sqlActnm = item.Value.sqlSmpName;
                    fGLGACT0Model.entryId = Session["UserID"].ToString();
                    fGLGACT0Model.entryDate = nowStr[0];
                    fGLGACT0Model.entryTime = nowStr[1];

                    gactList.Add(fGLGACT0Model);
                }
            }


            fGLGACTDao.proc(execAction, gactList, conn, transaction);
        }



        /// <summary>
        /// AS400 FGLITEM0 險種會計科目定義檔
        /// </summary>
        /// <param name="main"></param>
        /// <param name="smpMap"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void procFGLITEM0(OGL00003Model main, Dictionary<string, OGL00003DModel> smpMap
            , EacConnection conn, EacTransaction transaction)
        {
            string[] nowStr = DateUtil.getCurChtDateTime(4).Split(' ');
            string nowYy = nowStr[0].Substring(0, 4);
            string nowMm = nowStr[0].Substring(4, 2);
            string nowDd = nowStr[0].Substring(6, 2);

            FGLGACTDao fGLGACTDao = new FGLGACTDao();

            string itemCon = "";

            Dictionary<string, Dictionary<string, string>> itemDic = new Dictionary<string, Dictionary<string, string>>();

            foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
            {
                if ("itemAcct".Equals(item.Value.smpNumFrom)) {
                    if (!itemDic.ContainsKey(item.Value.corpNo)) {

                        Dictionary<string, string> sglgtba = new Dictionary<string, string>();

                        
                        itemCon = item.Value.isIfrs4;

                        if (!"D".Equals(main.execAction))
                        {
                            //取得險種會科定義因子
                            if (sglgtba.Count == 0)
                            {
                                SGLGTBAUtil sGLGTBAUtil = new SGLGTBAUtil();
                                sglgtba = sGLGTBAUtil.callSGLGTBAUtil(conn, main.productType, itemCon, item.Value.corpNo);
                            }
                        }

                        itemDic.Add(item.Value.corpNo, sglgtba);


                    }
                }
            }


            if (!"D".Equals(main.execAction))
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> d in itemDic)
                {
                    Dictionary<string, string> sglgtba = d.Value;
                    FGLITEM0Model fGLITEM0Model = new FGLITEM0Model();
                    fGLITEM0Model.corpNo = d.Key;

                    foreach (KeyValuePair<string, string> codeItem in sglgtba)
                    {
                        foreach (KeyValuePair<string, OGL00003DModel> item in smpMap)
                        {
                            //帳務類別第2碼等於1或3排除『商品設控檔』帳本7及9的資料，帳務類別第2碼等於2排除帳本1及3的資料。
                            if (("1".Equals(item.Value.acctType.Substring(1, 1)) || "3".Equals(item.Value.acctType.Substring(1, 1)))
                                && ("itemAcct".Equals(item.Value.smpNumFrom) && ("7".Equals(item.Value.corpNo) || "9".Equals(item.Value.corpNo)))
                                )
                                continue;

                            if ("2".Equals(item.Value.acctType.Substring(1, 1))
                                && ("1".Equals(item.Value.corpNo) || "3".Equals(item.Value.corpNo))
                                )
                                continue;


                            if (codeItem.Value.Equals(item.Value.smpNum) & fGLITEM0Model.corpNo.Equals(item.Value.corpNo))
                            {
                                string corpAcct = "";

                                switch (item.Value.corpNo)
                                {
                                    case "1":
                                    case "3":
                                        corpAcct = main.itemAcct;
                                        break;
                                    case "7":
                                    case "9":
                                        corpAcct = main.separatAcct;
                                        break;
                                }


                                switch (codeItem.Key)
                                {
                                    case "acctCode":
                                        fGLITEM0Model.acctCode = codeItem.Value + corpAcct;
                                        break;
                                    case "acctCodef":
                                        fGLITEM0Model.acctCodef = codeItem.Value + corpAcct;
                                        break;
                                    case "acctCoder":
                                        fGLITEM0Model.acctCoder = codeItem.Value + corpAcct;
                                        break;
                                    case "acctCodes":
                                        fGLITEM0Model.acctCodes = codeItem.Value + corpAcct;
                                        break;
                                    case "acctCodeg":
                                        fGLITEM0Model.acctCodeg = codeItem.Value + corpAcct;
                                        break;
                                    case "acctCodet":
                                        fGLITEM0Model.acctCodet = codeItem.Value + corpAcct;
                                        break;
                                    case "coiCode":
                                        fGLITEM0Model.coiCode = main.coiAcct == "" ? "" : codeItem.Value + main.coiAcct;
                                        break;
                                    case "coiCodef":
                                        fGLITEM0Model.coiCodef = main.coiAcct == "" ? "" : codeItem.Value + main.coiAcct;
                                        break;
                                    case "coiCoder":
                                        fGLITEM0Model.coiCoder = main.coiAcct == "" ? "" : codeItem.Value + main.coiAcct;
                                        break;
                                    case "acctCodei":
                                        fGLITEM0Model.acctCodei = codeItem.Value + main.itemAcct;
                                        break;
                                    case "comuCodef":
                                        fGLITEM0Model.comuCodef = codeItem.Value + main.itemAcct;
                                        break;
                                    case "comuCoder":
                                        fGLITEM0Model.comuCoder = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acctCodeo":
                                        fGLITEM0Model.acctCodeo = codeItem.Value + main.itemAcct;
                                        break;
                                    case "comuPayf":
                                        fGLITEM0Model.comuPayf = codeItem.Value + main.itemAcct;
                                        break;
                                    case "comuPayr":
                                        fGLITEM0Model.comuPayr = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4570":
                                        fGLITEM0Model.acct4570 = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4571":
                                        fGLITEM0Model.acct4571 = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4572":
                                        fGLITEM0Model.acct4572 = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4573":
                                        fGLITEM0Model.acct4573 = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4574":
                                        fGLITEM0Model.acct4574 = codeItem.Value + main.itemAcct;
                                        break;
                                    case "acct4575":
                                        fGLITEM0Model.acct4575 = codeItem.Value + main.itemAcct;
                                        break;
                                }
                            }
                        }
                    }


                    fGLITEM0Model.item = main.item;
                    fGLITEM0Model.premYear = "0";
                    fGLITEM0Model.updId = Session["UserID"].ToString();
                    fGLITEM0Model.updYy = nowYy;
                    fGLITEM0Model.updMm = nowMm;
                    fGLITEM0Model.updDd = nowDd;

                    FGLITEMDao fGLITEMDao = new FGLITEMDao();
                    fGLITEMDao.proc(main.execAction, fGLITEM0Model, conn, transaction);
                }

            }
            else {
                //20190806 刪除商品作業時，此檔的資料一併刪除
                FGLITEMDao fGLITEMDao = new FGLITEMDao();
                FGLITEM0Model fGLITEM0Model = new FGLITEM0Model();
                fGLITEM0Model.item = main.item;
                fGLITEMDao.proc(main.execAction, fGLITEM0Model, conn, transaction);

            }

                

           

        }

    }
}