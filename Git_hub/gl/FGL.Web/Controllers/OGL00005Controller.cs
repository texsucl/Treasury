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
/// 功能說明：商品資料會計接收作業
/// 初版作者：20190118 Daiyu
/// 修改歷程：20190118 Daiyu
///           需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191202 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：1.若已將商品送到『OGL00005A 商品資料會計接收覆核作業』，但未覆核前，可以再查詢出來，但只能列印。
///           2.畫面增加"商品簡稱"欄位
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00005Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00005/");
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
                string[] apprMk = new string[] { "4","5", "12", "13", "14" };   //modify by daiyu 20191202 增加可查詢覆核狀態=5的資料
                rows = fGLItemInfoHisDao.qryByApprMk(apprMk, "OGL00005", updDateB, updDateE, item, false);
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

                ViewBag.aplyNo = "";
                ViewBag.apprMkHis = modelHis == null ? "" : modelHis.apprMk ?? "";
                ViewBag.status = modelHis == null ? "" : modelHis.status ?? "";
                ViewBag.execAction = modelHis == null ? "" : modelHis.execAction ?? "";



                if ("1".Equals(isQryTmp))
                {
                    ViewBag.aplyNo = modelHis.aplyNo;
                    model = modelHis;
                }
                else {
                    //查詢【FGL_ITEM_INFO 會計商品資訊檔】
                    FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                    model = fGLItemInfoDao.qryItem(item, "6");
                    //model = fGLItemInfoDao.qryPK(item, productType, fuMk, itemCon, discPartFeat, "6");
                }

                ViewBag.isQryTmp = isQryTmp;
                ViewBag.bHaveData = "Y";

                if (!"O".Equals(model.flag))
                {
                    //查詢"商品科目設定檔"

                    FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                    List<OGL00001Model> itemAcct = fGLItemAcctDao.qryHeadForItem(model.productType, model.fuMk, model.itemCon, model.discPartFeat);

                    if (itemAcct.Count == 0)
                        return Json(new { success = false, err = "查無「商品科目設定」資料!!" }, JsonRequestBehavior.AllowGet);

                    
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

                if (!"O".Equals(model.flag)) {
                    ViewBag.itemHealthMgrMk = StringUtil.toString(model.healthMgrType);

                    //生效日-->要做民國年、西元年的轉換
                    model.effectYY = model.effectYY == "" ? "" : (Convert.ToInt16(model.effectYY.Substring(0, 4)) - 1911).ToString();

                    ////險種中文名稱，需自上游預帶值
                    //if ("A".Equals(model.execAction)) {
                    //    string itemName = qryItemName(model.item);

                    //    if (!"".Equals(StringUtil.toString(itemName).TrimEnd()))
                    //        model.itemName = itemName;
                    //}
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
        /// 修改作業
        /// </summary>
        /// <param name="execAction"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(bool bNewItem, string execAction, OGL00003Model model)
        {
            //由OGL0003 新增的商品 需做下列檢核
            if (!"O".Equals(model.flag))
            {
                FGL_ITEM_CODE_TRAN tran = new FGL_ITEM_CODE_TRAN();
                FGLItemCodeTranDao fGLItemCodeTranDao = new FGLItemCodeTranDao();
                tran = fGLItemCodeTranDao.qryForWanpie(model.prodNo);
                if (tran == null)
                    return Json(new { success = false, err = "無商品編號代碼轉換資料!!" }, JsonRequestBehavior.AllowGet);



                //若屬"新增"商品時，需做「會計科目」檢查
                if (bNewItem)
                    //if (bNewItem & ("2".Equals(model.sysType) || "3".Equals(model.sysType)))
                {
                    //if ("A".Equals(execAction) & ("2".Equals(model.sysType) || "3".Equals(model.sysType))) {
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
                his.item_name_shrt = model.itemNameShrt;

                if (!"O".Equals(model.flag))
                    his.effect_date = DateUtil.stringToDatetime(DateUtil.formatDateTimeDbToSc(
                        (Convert.ToInt16(model.effectYY) + 1911).ToString().PadLeft(4, '0') + model.effectMM.PadLeft(2, '0') + model.effectDD.PadLeft(2, '0'), "D"));

                his.acct_upd_id = Session["UserID"].ToString();
                his.acct_upd_dt = DateTime.Now;

                switch (execAction)
                {
                    case "C":
                        his.appr_mk = "0";
                        break;
                    default:
                        his.appr_mk = "5";
                        break;
                }


                his.aply_no = StringUtil.toString(model.aplyNo);
                his.exec_action = execAction;

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
                            his.status = "5";


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
                            switch (execAction)
                            {
                                case "D":
                                case "U":
                                    //異動"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.updateForAcct(his, conn, transaction);
                                    break;

                                case "C":
                                    //異動"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.updateForAcct(his, conn, transaction);
                                    break;
                            }
                        }

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



        [HttpPost]
        public JsonResult execRtn(string rtnType, OGL00003Model model)
        {


            try
            {
                FGL_ITEM_INFO_HIS his = new FGL_ITEM_INFO_HIS();


                his.aply_no = StringUtil.toString(model.aplyNo);

                his.appr_mk = rtnType == "P" ? "9" : "11";

                string strConn = DbUtil.GetDBFglConnStr();
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction("Transaction");
                    try
                    {
                        FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();

                        fGLItemInfoHisDao.updateApprMk(his.aply_no, his.appr_mk, "OGL00005", Session["UserID"].ToString(), "", conn, transaction);



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



        [HttpPost]
        public ActionResult Print(OGL00003Model model)
        {
            string isQryTmp = model.isQryTmp;


            //查"會計商品資訊檔"
            if (isQryTmp == "0")
            {
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                model = fGLItemInfoDao.qryItem(model.item, "6");
                //model = fGLItemInfoDao.qryPK(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "6");
            }
            else
            {  //查詢"會計商品資訊暫存檔"
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                model = fGLItemInfoHisDao.qryAplyNo(model.aplyNo);
            }

            List<OGL00003Model> main = new List<OGL00003Model>();
            //List<OGL00003DModel> smpList = new List<OGL00003DModel>();

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

            List<smpModel> smpList = new List<smpModel>();

            if (model != null)
            {
                main.Add(model);

                NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
                Dictionary<string, OGL00003DModel> smpMap = newAcctItemUtil.getSmpList("P", model);

                //查詢給付項目明細
                List<OGL00003DModel> sumpD = new List<OGL00003DModel>();
                if (isQryTmp == "0")
                {
                    FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                    sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "");
                }
                else
                {
                    FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                    sumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "");
                }

                if (sumpD != null)
                {
                    foreach (OGL00003DModel d in sumpD)
                    {

                        if ("Y".Equals(d.flag))
                        {
                            smpModel smpModel = new smpModel();

                            smpModel.smpName = d.smpName;
                            smpModel.dataType = d.dataType;

                            switch (d.acctType.Substring(1, 1))
                            {
                                case "1":
                                case "3":
                                    smpModel.showType = "GA";
                                    break;
                                case "2":
                                    smpModel.showType = "SA";
                                    break;
                            }

                            smpList.Add(smpModel);
                        }

                    }
                }

                //商品資料接收報表
                ReportWrapper rw = new ReportWrapper();
                rw.ReportId = "OGL00005P";
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/OGL00005P.rdlc");

                rw.ReportParameters.Add("acctUpDt", model.acctUpDt.Split(' ')[0]);
                rw.ReportParameters.Add("acctUpId", model.acctUpId);
                rw.ReportParameters.Add("acctApprId", model.acctApprId);
             

                CommonUtil commonUtil = new CommonUtil();
                DataTable dtMain = commonUtil.ConvertToDataTable<OGL00003Model>(main);
                rw.ReportDataSources = dtMain;

                DataTable dtDetail = commonUtil.ConvertToDataTable<smpModel>(smpList);
                rw.ReportSubDataSources = dtDetail;

                String guid = Guid.NewGuid().ToString();

                Session[guid] = rw;


                //商品資料接收報表-會科異動紀錄
                ReportWrapper rw2 = new ReportWrapper();
                rw2.ReportId = "OGL00005P2";
                rw2.ReportPath = Server.MapPath($"~/Report/Rdlc/OGL00005P2.rdlc");

                rw2.ReportParameters.Add("acctUpDt", model.acctUpDt.Split(' ')[0]);
                rw2.ReportParameters.Add("acctUpId", model.acctUpId);
                rw2.ReportParameters.Add("acctApprId", model.acctApprId);
                rw2.ReportParameters.Add("item", model.item);
                rw2.ReportParameters.Add("execAction", model.execAction);

                DataTable dtDetailP2 = commonUtil.ConvertToDataTable<OGL00003DModel>(smpMap.Values.ToList());
                rw2.ReportDataSources = dtDetailP2;

                String guid2 = Guid.NewGuid().ToString();

                Session[guid2] = rw2;

                var jsonData = new { success = true, guid = guid, guid2 = guid2 };
                return Json(jsonData, JsonRequestBehavior.AllowGet);

            }
            else
                return Json(new { success = false, err = "查無資料!!" }, JsonRequestBehavior.AllowGet);
        }



        public class smpModel
        {
            public string dataType { get; set; }

            public string showType { get; set; }

            public string smpName { get; set; }
        }
    }
}