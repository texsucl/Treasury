using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;



/// <summary>
/// 功能說明：商品資料維護作業
/// 初版作者：20190104 Daiyu
/// 修改歷程：20190104 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00003Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            logger.Info("Index begin");

            UserAuthUtil authUtil = new UserAuthUtil();


            string opScope = "";
            string funcName = "";


            if (Session["UserID"] != null)
            {
                string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00003/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }


            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;


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

            //覆核註記
            var apprMkList = sysCodeDao.loadSelectList("GL", "ITEM_APPR_MK", true);
            ViewBag.apprMkList = apprMkList;

            //建立來源
            var flagList = sysCodeDao.loadSelectList("GL", "ITEM_Flag", true);
            ViewBag.flagList = flagList;

            //保費費用
            var lodprmTypeList = sysCodeDao.loadSelectList("GL", "LODPRM_TYPE", true);
            ViewBag.lodprmTypeList = lodprmTypeList;

            logger.Info("Index end");
            return View();
        }



        /// <summary>
        /// 查詢FGLSMPB取得SA、GA項目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySMPB(OGL00003Model model)
        {
            List<smpNumModel> gaList = new List<smpNumModel>();
            List<smpNumModel> saList = new List<smpNumModel>();

            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
            string[] acctType = new string[] {"B"};
            List<OGL10181Model> rows = fGLSMPBDao.qryForItem(model.productType, model.fuMk, model.itemCon, model.discPartFeat
                , acctType);

            foreach (OGL10181Model d in rows) {
                smpNumModel smpNum = new smpNumModel();
                smpNum.tempId = d.tempId;
                smpNum.smpNum = d.smpNum;
                smpNum.smpName = d.smpName;
                smpNum.productType = d.productType;
                smpNum.acctType = d.acctType;

                switch (d.acctType.Substring(1, 1)) {
                    case "1":
                    case "3":
                        gaList.Add(smpNum);
                        break;
                    case "2":
                        saList.Add(smpNum);
                        break;
                }
            }

            return Json(new { success = true, gaList = gaList, saList = saList }, JsonRequestBehavior.AllowGet);
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
        /// 畫面執行"查詢"
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryItemInfo(OGL00003Model model)
        {
            string productType = model.productType;
            string fuMk = model.fuMk;
            string itemCon = model.itemCon;
            string discPartFeat = model.discPartFeat;


            string isQryTmp = model.isQryTmp;

            FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            OGL00003Model modelHis = new OGL00003Model();
            //查詢"會計商品資訊暫存檔"
            //modelHis = fGLItemInfoHisDao.qryItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat);
            modelHis = fGLItemInfoHisDao.qryItem(model.item);

            //查"會計商品資訊檔"
            if (isQryTmp == "0")
            {
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                model = fGLItemInfoDao.qryItem(model.item, "6");
                //model = fGLItemInfoDao.qryPK(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "6");
            }
            else
                model = modelHis;



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



            string hisMk = "";
            string hisExecAction = "";
            if (modelHis != null)
            {
                hisMk = StringUtil.toString(modelHis.apprMk);
                hisExecAction = StringUtil.toString(modelHis.execAction);
            }


            List<OGL00003DModel> sumpD = new List<OGL00003DModel>();
            OGL00001Model itemAcct = new OGL00001Model();

            //查詢給付項目明細
            if (model != null)
            {
                if (!"O".Equals(StringUtil.toString(model.flag)))
                {
                    //查詢"商品科目設定檔"
                    itemAcct = qryItemAcct(model.productType, model.fuMk, model.itemCon, model.discPartFeat);

                    if (isQryTmp == "0")
                    {
                        FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                        sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "P");
                    }
                    else
                    {
                        FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                        sumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "P");
                    }
                }
            }
            else {

                //查詢"商品科目設定檔"
                //itemAcct = qryItemAcct(productType, fuMk, itemCon, discPartFeat);
            }


            var jsonData = new { success = true, model = model, hisMk = hisMk, hisExecAction = hisExecAction, sumpD = sumpD, itemAcct = itemAcct };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// //查詢"商品科目設定檔"
        /// </summary>
        /// <param name="productType"></param>
        /// <param name="fuMk"></param>
        /// <param name="itemCon"></param>
        /// <param name="discPartFeat"></param>
        /// <returns></returns>
        private OGL00001Model qryItemAcct(string productType, string fuMk, string itemCon, string discPartFeat) {
            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            List<OGL00001Model> itemAcctList = fGLItemAcctDao.qryHeadForItem(productType, fuMk, itemCon, discPartFeat);

            OGL00001Model itemAcct = new OGL00001Model();

            if (itemAcctList.Count > 0)
                itemAcct = itemAcctList[0];

            return itemAcct;

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
            List<smpModel> smpList = new List<smpModel>();

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



            //查詢給付項目明細
            List<OGL00003DModel> sumpD = new List<OGL00003DModel>();
            if (model != null)
            {
                main.Add(model);

                if (isQryTmp == "0")
                {
                    FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                    sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "P");
                }
                else
                {
                    FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                    sumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "P");
                }

                if (sumpD != null) {
                    foreach (OGL00003DModel d in sumpD) {

                        if ("Y".Equals(d.flag)) {
                            smpModel smpModel = new smpModel();

                            smpModel.smpName = d.smpName;

                            switch (d.acctType.Substring(1, 1))
                            {
                                case "1":
                                case "3":
                                    smpModel.dataType = "GA";
                                    break;
                                case "2":
                                    smpModel.dataType = "SA";
                                    break;
                            }

                            smpList.Add(smpModel);
                        }
                        
                    }
                }
                


                ReportWrapper rw = new ReportWrapper();
                rw.ReportId = "OGL00003P";
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/OGL00003P.rdlc");

                rw.ReportParameters.Add("prodUpDt", model.prodUpDt.Split(' ')[0]);
                rw.ReportParameters.Add("prodUpId", model.prodUpId);
                rw.ReportParameters.Add("prodApprId", model.prodApprId);
                

                CommonUtil commonUtil = new CommonUtil();
                DataTable dtMain = commonUtil.ConvertToDataTable<OGL00003Model>(main);
                rw.ReportDataSources = dtMain;

                DataTable dtDetail = commonUtil.ConvertToDataTable<smpModel>(smpList);
                rw.ReportSubDataSources = dtDetail;

                String guid = Guid.NewGuid().ToString();

                Session[guid] = rw;

                var jsonData = new { success = true, guid = guid };
                return Json(jsonData, JsonRequestBehavior.AllowGet);

            }
            else
                return Json(new { success = false, err = "查無資料!!" }, JsonRequestBehavior.AllowGet);


        }


        public class smpModel
        {
            public string dataType { get; set; }
            public string smpName { get; set; }
        }


        private FGL_ITEM_SMPNUM_HIS scToSumpnumDb(OGL00003Model model, smpNumModel d)
        {
            FGL_ITEM_SMPNUM_HIS smpHis = new FGL_ITEM_SMPNUM_HIS();

            smpHis.item = model.item;
            smpHis.product_type = model.productType;
            smpHis.fu_mk = model.fuMk;
            smpHis.item_con = model.itemCon;
            smpHis.disc_part_feat = model.discPartFeat;

            var temp = d.acctItem.Split('|');
            smpHis.smp_num = temp[0];
            smpHis.product_type = temp[1];
            smpHis.acct_type = temp[2];
            smpHis.data_type = "P";
            smpHis.flag = d.acctItemVal;

            return smpHis;
        }

        private FGL_ITEM_INFO_HIS scToMainDb(string execAction, OGL00003Model model, OGL00001Model item) {

            FGL_ITEM_INFO_HIS his = new FGL_ITEM_INFO_HIS();

            his.item = model.item;
            his.product_type = model.productType;
            his.fu_mk = model.fuMk;
            his.item_con = model.itemCon;
            his.disc_part_feat = model.discPartFeat;

            his.prod_no_ver = model.prodNoVer;
            his.prod_no = model.prodNo;
            his.sys_type = model.sysType;
            his.item_main_type = model.itemMainType;
            his.invest_type = StringUtil.toString(item.investTypeMk) == "" ? "" : model.investType;
            his.ins_term = model.insTerm;
            his.busi_type = model.busiType;
            his.com_type = model.comType;
            his.ext_schedul_type = StringUtil.toString(item.extSchedulMk) == "" ? "" : model.extSchedulType;
            his.pakind_dmop_type = StringUtil.toString(item.pakindDmopMk) == "" ? "" : model.pakindDmopType;
            his.lodprm_type = StringUtil.toString(item.lodprmMk) == "" ? "" : model.lodprmType;
            his.health_mgr_type =  model.healthMgrType;
            his.coi_type = model.coiType;
            his.item_name = model.itemName;
            his.flag = model.flag;


            if (!"A".Equals(execAction)) {
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();

                OGL00003Model formal = fGLItemInfoDao.qryItem(model.item, "6");
               // OGL00003Model formal = fGLItemInfoDao.qryPK(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "6");

                if (formal != null) {
                    //his.item_name = formal.itemName;
                    his.item_name_shrt = formal.itemNameShrt;
                    his.item_acct = formal.itemAcct;
                    his.separat_acct = formal.separatAcct;
                    his.coi_acct = formal.coiAcct;

                    if (!"".Equals(StringUtil.toString(formal.effectDate)))
                        his.effect_date = DateUtil.stringToDatetime(formal.effectDate);

                    his.eff_mk = formal.effMk;
                }
                

            }

            switch (execAction) {
                case "C":
                    his.appr_mk = "0";
                    break;
                default:
                    his.appr_mk = "1";
                    break;
            }


            his.prod_upd_id = Session["UserID"].ToString();
            his.prod_upd_dt = DateTime.Now;

            return his;
        }


        /// <summary>
        /// 檢查畫面輸入的值
        /// </summary>
        /// <param name="model"></param>
        /// <param name="acctList"></param>
        /// <returns></returns>
        private errModel chkInput(string execAction, OGL00003Model model, List<smpNumModel> acctList, OGL00001Model item) {
            errModel errModel = new errModel();
            errModel.success = true;
            errModel.err = "";

            string msg = "";

            //檢查畫面欄位是否都有輸入
            if ("".Equals(StringUtil.toString(model.item)))
                msg += "「險種代號」　";

            //非AS400建立的商品
            if (!"O".Equals(model.flag)) {
                if ("".Equals(StringUtil.toString(model.productType)))
                    msg += "「險種類別」　";

                if ("".Equals(StringUtil.toString(model.fuMk)))
                    msg += "「外幣註記」　";

                if ("".Equals(StringUtil.toString(model.itemCon)))
                    msg += "「合約分類」　";

                if ("".Equals(StringUtil.toString(model.discPartFeat)))
                    msg += "「裁量參與特性」　";
            }
                


            if (!"D".Equals(execAction) & !"C".Equals(execAction))
            { 
                if ("".Equals(StringUtil.toString(model.prodNoVer)))
                    msg += "「保險商品編號版本」　";

                if ("".Equals(StringUtil.toString(model.prodNo)))
                    msg += "「保險商品編號」　";

                if ("".Equals(StringUtil.toString(model.sysType)))
                    msg += "「進件系統別」　";


                //非AS400建立的商品
                if (!"O".Equals(model.flag))
                {

                    //險種中文名稱
                    if ("".Equals(StringUtil.toString(model.itemName)))
                        msg += "「險種中文名稱」　";

                    //健康管理保險商品
                    switch (model.productType) {
                        case "1":
                        case "2":
                        case "3":
                        case "4":
                            if("".Equals(StringUtil.toString(model.healthMgrType)))
                                msg += "「健康管理保險商品」　";
                            break;
                    }
                      
                    
                    if ("".Equals(StringUtil.toString(model.itemMainType)))
                        msg += "「險種主險別」　";

                    if ("Y".Equals(item.investTypeMk) && "".Equals(StringUtil.toString(model.investType)))
                        msg += "「投資型商品類型」　";

                    if ("".Equals(StringUtil.toString(model.insTerm)))
                        msg += "「保險期間」　";

                    if ("".Equals(StringUtil.toString(model.busiType)))
                        msg += "「業務性質」　";

                    if ("".Equals(StringUtil.toString(model.comType)))
                        msg += "「佣金/承攬費」　";

                    if ("Y".Equals(item.extSchedulMk) && "".Equals(StringUtil.toString(model.extSchedulType)))
                        msg += "「可展期定期保險」　";

                    if ("Y".Equals(item.pakindDmopMk) && "".Equals(StringUtil.toString(model.pakindDmopType)))
                        msg += "「繳費方式僅限躉繳」　";

                    if ("Y".Equals(item.lodprmMk) && "".Equals(StringUtil.toString(model.lodprmType)))
                        msg += "「保費費用」　";

                    if ("Y".Equals(item.coiType) && "".Equals(StringUtil.toString(model.coiType)))
                        msg += "「COI」　";
                }
            }

            if (!"".Equals(msg))
                errModel.err = "以下資訊未輸入：" + msg + "；";



            if (!"O".Equals(model.flag) & !"D".Equals(execAction) & !"C".Equals(execAction))
            {
                //檢查科目代號是否均有輸入
                msg = "";

                foreach (smpNumModel d in acctList)
                {
                    if ("".Equals(StringUtil.toString(d.acctItemVal)))
                        msg += d.smpName + "　";
                }
                if (!"".Equals(msg))
                    errModel.err += "以下科目未設定：" + msg + "；";


                //string dupItem = chkDupProdNo(model.prodNo, model.item);
                //if(!"".Equals(dupItem))
                //    errModel.err += "輸入的保發編碼與其他商品重複，請重新檢視正確性(險種代號：" + dupItem + ")；";


            }



            if (!"".Equals(errModel.err))
                errModel.success = false;
  
            //errModel.err = msg;


            return errModel;
        }

        /// <summary>
        /// 檢查"保險商品編號"是否重複
        /// </summary>
        /// <param name="prodNo"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        //private string chkDupProdNo(string prodNo, string item) {
        //    string dupItem = "";

        //    //查詢"FGLGITE0 會計商品資訊檔"
        //    FGLGITEDao fGLGITEDao = new FGLGITEDao();
        //    dupItem = fGLGITEDao.qryByProdNo(prodNo, item);

        //    if ("".Equals(dupItem)) {
        //        FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
        //        dupItem = fGLItemInfoHisDao.qryByProdNo(prodNo, item);
        //    }


        //    return dupItem;
        //}


        public JsonResult chkDupProdNo(string prodNo, string item)
        {
            string dupItem = "";

            //查詢"FGLGITE0 會計商品資訊檔"
            FGLGITEDao fGLGITEDao = new FGLGITEDao();
            dupItem = fGLGITEDao.qryByProdNo(prodNo, item);

            if ("".Equals(dupItem))
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                dupItem = fGLItemInfoHisDao.qryByProdNo(prodNo, item);
            }

            var jsonData = new { success = true, dupItem = dupItem };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult chkItemAcct(OGL00003Model model)
        {
            OGL00001Model itemAcct = new OGL00001Model();
            List<OGL00003DModel> sumpD = new List<OGL00003DModel>();

            //查詢"商品科目設定檔"
            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            itemAcct = qryItemAcct(model.productType, model.fuMk, model.itemCon, model.discPartFeat);
            if (!"".Equals(itemAcct.productType)) {
                FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "P");
            }

            var jsonData = new { success = true, itemAcct = itemAcct, sumpD = sumpD };

            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }




        [HttpPost]
        public JsonResult execSave(string execAction, OGL00003Model model, List<smpNumModel> acctList)
        {
            errModel errModel = new errModel();
            List<OGL00001Model> itemAcct = new List<OGL00001Model>();

            if (!"O".Equals(model.flag))
            {
                //查詢"商品科目設定檔"
                FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                itemAcct = fGLItemAcctDao.qryHeadForItem(model.productType, model.fuMk, model.itemCon, model.discPartFeat);

                if (itemAcct.Count == 0)
                    return Json(new { success = false, err = "查無「商品科目設定」資料!!" }, JsonRequestBehavior.AllowGet);

                //檢查畫面欄位
                errModel = chkInput(execAction, model, acctList, itemAcct[0]);
            }
            else {
                //檢查畫面欄位
                errModel = chkInput(execAction, model, acctList, null);
            }

            model.item = model.item.ToUpper();

            if (!errModel.success)
                return Json(new { success = false, err = errModel.err }, JsonRequestBehavior.AllowGet);

            try
            {

                FGL_ITEM_INFO_HIS his = scToMainDb(execAction, model, itemAcct.Count.ToString() == "0" ? new OGL00001Model() : itemAcct[0]);
                his.aply_no = model.aplyNo ?? "";
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

                        switch (execAction)
                        {
                            case "A":
                                //取得流水號
                                getAplyNo(his);
                                model.aplyNo = his.aply_no;


                                //新增"會計商品資訊暫存檔"
                                fGLItemInfoHisDao.insert(his, conn, transaction);

                                //非AS400建立的商品
                                if (!"O".Equals(model.flag))
                                {
                                    //新增"會計商品給付項目明細暫存檔"
                                    foreach (smpNumModel d in acctList)
                                    {
                                        FGL_ITEM_SMPNUM_HIS smpHis = scToSumpnumDb(model, d);
                                        smpHis.aply_no = his.aply_no;

                                        fGLItemSmpnumHisDao.insert(smpHis, conn, transaction);
                                    }
                                }
                                    
                                break;

                            case "U":
                            case "D":
                                if ("".Equals(his.aply_no))
                                {
                                    //取得流水號
                                    getAplyNo(his);
                                    model.aplyNo = his.aply_no;

                                    //新增"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.insert(his, conn, transaction);

                                    //非AS400建立的商品
                                    if (!"O".Equals(model.flag))
                                    {
                                        if ("U".Equals(execAction))
                                        {
                                            //新增"會計商品給付項目明細暫存檔"
                                            foreach (smpNumModel d in acctList)
                                            {
                                                FGL_ITEM_SMPNUM_HIS smpHis = scToSumpnumDb(model, d);
                                                smpHis.aply_no = his.aply_no;

                                                fGLItemSmpnumHisDao.insert(smpHis, conn, transaction);
                                            }


                                            //若此險種有投資交易的項目，將正式檔的投資交易項目一併寫入暫存檔
                                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                                , model.itemCon, model.discPartFeat, "I", conn, transaction);
                                        }
                                        else
                                        {
                                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                                , model.itemCon, model.discPartFeat, "P", conn, transaction);

                                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                                , model.itemCon, model.discPartFeat, "I", conn, transaction);
                                        }

                                    }


                                }
                                else {
                                    //異動"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.updateForProd(his, conn, transaction);

                                    //非AS400建立的商品
                                    if (!"O".Equals(model.flag) & "U".Equals(execAction)) {
                                        //刪除"會計商品給付項目明細暫存檔"
                                        fGLItemSmpnumHisDao.deleteByAplyNoDataType(his.aply_no, "P", conn, transaction);

                                        //新增"會計商品給付項目明細暫存檔"
                                        foreach (smpNumModel d in acctList)
                                        {
                                            FGL_ITEM_SMPNUM_HIS smpHis = scToSumpnumDb(model, d);
                                            smpHis.aply_no = his.aply_no;

                                            fGLItemSmpnumHisDao.insert(smpHis, conn, transaction);
                                        }
                                    }
                                      
                                }

                                break;

                            case "C":
                                OGL00003Model hisO = fGLItemInfoHisDao.qryAplyNo(his.aply_no);
                                if (!"Y".Equals(StringUtil.toString(hisO.effMk)) && !"".Equals(hisO.itemAcct)) {
                                    transaction.Rollback();
                                    return Json(new { success = false, err = "請通知保單會計部承辦，清除科目設定維護作業中的「險種科目」代碼!!" }, JsonRequestBehavior.AllowGet);
                                }

                                //異動"會計商品資訊暫存檔"
                                fGLItemInfoHisDao.updateApprMk(his.aply_no, "0", "OGL00003", Session["UserID"].ToString(), "", conn, transaction);

                                break;
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
            catch (Exception e) {

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }


        private FGL_ITEM_INFO_HIS getAplyNo(FGL_ITEM_INFO_HIS his) {
            string[] curDateTime = DateUtil.getCurChtDateTime().Split(' ');

            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = curDateTime[0];
            var cId = sysSeqDao.qrySeqNo("FGL", "E", qPreCode).ToString();
            cId = "E" + qPreCode + cId.ToString().PadLeft(4, '0');
            his.aply_no = cId;
            his.status = "1";
            his.create_id = Session["UserID"].ToString();
            his.create_dt = DateTime.Now;

            return his;
        }


        public class errModel
        {
            public bool success { get; set; }
            public string err { get; set; }
        }

        

        
    }
}