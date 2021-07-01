using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：投資交易商品資料維護作業
/// 初版作者：20190117 Daiyu
/// 修改歷程：20190117 Daiyu
///           需求單號：201805080167-00
///           初版
/// -----------------------------------------------
/// 修改歷程：20191202 Daiyu
/// 需求單號：201911060431-00
/// 修改內容：若已將商品送到『OGL00004A 投資交易商品資料覆核作業』，但未覆核前，可以再查詢出來，但只能列印。
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00004Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00004/");
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
            List<OGL00003Model> dataList = new List<OGL00003Model>();

            if ("1".Equals(isQryTmp))   //查詢"會計商品資訊檔暫存檔"
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                string[] apprMk = new string[] { "2", "3", "10", "11" };   //modify by daiyu 20191202 增加可查詢覆核狀態=3的資料
                rows = fGLItemInfoHisDao.qryByApprMk(apprMk, "OGL00004", updDateB, updDateE, item, false);
            }
            else
            {  //查詢"會計商品資訊"
                FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                rows = fGLItemInfoDao.qryByDateItem(updDateB, updDateE, item, "6");
            }

            FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
            

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OGL00003Model d in rows)
                {
                    //查詢"商品科目設定檔"-->若"投資型商品交易維護"=Y者，才需要投資維護
                    List<OGL00001Model> itemAcct = fGLItemAcctDao.qryHeadForItem(d.productType, d.fuMk, d.itemCon, d.discPartFeat);
                    if (itemAcct.Count == 0)
                        continue;
                    else {
                        string investTradMk = itemAcct[0].investTradMk;

                        if (!"Y".Equals(investTradMk))
                            continue;
                        else
                        {
                            d.aplyDt = d.createDt;
                            d.aplyUId = d.createId;

                            createUid = StringUtil.toString(d.aplyUId);

                            if (!"".Equals(createUid))
                            {
                                if (!userNameMap.ContainsKey(createUid))
                                {
                                    userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                                }
                                d.aplyUId = createUid + " " + userNameMap[createUid];
                            }

                            dataList.Add(d);
                        }
                    }
                }
            }

              


            var jsonData = new { success = true, dataList = dataList };
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
            List<smpNumModel> gaList = new List<smpNumModel>();
            List<smpNumModel> saList = new List<smpNumModel>();

            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
            string[] acctType = new string[] { "C" };
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
                        gaList.Add(smpNum);
                        break;
                    case "2":
                        saList.Add(smpNum);
                        break;
                }
            }

            List<OGL00003DModel> sumpD = new List<OGL00003DModel>();

            if ("1".Equals(model.isQryTmp))
            {
                FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                sumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "I");
            }
            else {
                FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "I");

            }
            

            return Json(new { success = true, gaList = gaList, saList = saList, sumpD = sumpD }, JsonRequestBehavior.AllowGet);
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

            OGL00003Model model = new OGL00003Model();

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

            //覆核註記
            var apprMkList = sysCodeDao.loadSelectList("GL", "ITEM_APPR_MK", true);
            ViewBag.apprMkList = apprMkList;


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
                model = fGLItemInfoHisDao.qryItem(item);
                //model = fGLItemInfoHisDao.qryItem(item, productType, fuMk, itemCon, discPartFeat);

                ViewBag.aplyNo = "";
                ViewBag.apprMkHis = model == null ? "" : model.apprMk ?? "";
                ViewBag.status = model == null ? "" : model.status ?? "";
                ViewBag.execAction = model == null ? "" : model.execAction ?? "";

                if ("1".Equals(isQryTmp))
                {
                    ViewBag.aplyNo = model.aplyNo;
                }
                else {
                    FGLItemInfoDao fGLItemInfoDao = new FGLItemInfoDao();
                    model = fGLItemInfoDao.qryItem(item, "6");
                   // model = fGLItemInfoDao.qryPK(item, productType, fuMk, itemCon, discPartFeat, "6");
                }

                ViewBag.isQryTmp = isQryTmp;
                ViewBag.bHaveData = "Y";


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


                return View(model);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(model);
            }
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
            smpHis.data_type = "I";
            smpHis.flag = d.acctItemVal;

            return smpHis;
        }

        private FGL_ITEM_INFO_HIS formalToHis(FGL_ITEM_INFO_HIS his, OGL00003Model model) {
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
            his.item_name = formal.itemName;
            his.item_name_shrt = formal.itemNameShrt;
            his.item_acct = formal.itemAcct;
            his.separat_acct = formal.separatAcct;
            his.coi_acct = formal.coiAcct;
            his.effect_date = DateUtil.stringToDatetime(formal.effectDate);
            his.eff_mk = formal.effMk;
            his.flag = formal.flag;

            return his;
        }


        [HttpPost]
        public JsonResult execSave(string execAction, OGL00003Model model, List<smpNumModel> acctList)
        {
            if (!"D".Equals(execAction) & !"C".Equals(execAction))
            {
                //檢查科目代號是否均有輸入
                string msg = "";

                foreach (smpNumModel d in acctList)
                {
                    if ("".Equals(StringUtil.toString(d.acctItemVal)))
                        msg += d.smpName + "　";
                }
                if (!"".Equals(msg))
                    return Json(new { success = false, err = msg }, JsonRequestBehavior.AllowGet);
            }


            try
            {
                FGL_ITEM_INFO_HIS his = new FGL_ITEM_INFO_HIS();

                his.item = model.item;
                his.product_type = model.productType;
                his.fu_mk = model.fuMk;
                his.item_con = model.itemCon;
                his.disc_part_feat = model.discPartFeat;

                his.invest_upd_id = Session["UserID"].ToString();
                his.invest_upd_dt = DateTime.Now;

                switch (execAction)
                {
                    case "C":
                        his.appr_mk = "0";
                        break;
                    default:
                        his.appr_mk = "3";
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
                            his.status = "3";
                            his.create_id = Session["UserID"].ToString();
                            his.create_dt = DateTime.Now;

                            his.exec_action = "U";

                            //將本支未維護的欄位從正式檔對搬至暫存檔
                            his = formalToHis(his, model);


                            //新增"會計商品資訊暫存檔"
                            fGLItemInfoHisDao.insert(his, conn, transaction);

                            //新增"會計商品給付項目明細暫存檔"
                            foreach (smpNumModel d in acctList)
                            {
                                FGL_ITEM_SMPNUM_HIS smpHis = scToSumpnumDb(model, d);
                                smpHis.aply_no = his.aply_no;

                                fGLItemSmpnumHisDao.insert(smpHis, conn, transaction);
                            }


                            //將正式檔的給付項目一併寫入暫存檔
                            fGLItemSmpnumHisDao.insertFromFormal(his.aply_no, model.item, model.productType, model.fuMk
                                , model.itemCon, model.discPartFeat, "P", conn, transaction);

                            model.aplyNo = his.aply_no;
                        }
                        else {
                            switch (execAction)
                            {
                                case "U":
                                case "D":
                                    //異動"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.updateForInvest(his, conn, transaction);


                                    if ("U".Equals(execAction)) {
                                        //刪除"會計商品給付項目明細暫存檔"
                                        fGLItemSmpnumHisDao.deleteByAplyNoDataType(his.aply_no, "I", conn, transaction);

                                        //新增"會計商品給付項目明細暫存檔"
                                        foreach (smpNumModel d in acctList)
                                        {
                                            FGL_ITEM_SMPNUM_HIS smpHis = scToSumpnumDb(model, d);
                                            smpHis.aply_no = his.aply_no;

                                            fGLItemSmpnumHisDao.insert(smpHis, conn, transaction);
                                        }
                                    }

                                    break;

                                case "C":
                                    //異動"會計商品資訊暫存檔"
                                    fGLItemInfoHisDao.updateForInvest(his, conn, transaction);

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

        public class smpModel
        {
            public string dataType { get; set; }
            public string smpName { get; set; }
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



            //查詢保單投資明細
            List<OGL00003DModel> sumpD = new List<OGL00003DModel>();
            if (model != null)
            {
                main.Add(model);

                if (isQryTmp == "0")
                {
                    FGLItemSmpnumDao fGLItemSmpnumDao = new FGLItemSmpnumDao();
                    sumpD = fGLItemSmpnumDao.qryByItem(model.item, model.productType, model.fuMk, model.itemCon, model.discPartFeat, "I");
                }
                else
                {
                    FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
                    sumpD = fGLItemSmpnumHisDao.qryByAplyNo(model.aplyNo, "I");
                }

                if (sumpD != null)
                {
                    foreach (OGL00003DModel d in sumpD)
                    {

                        if ("Y".Equals(d.flag))
                        {
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
                rw.ReportId = "OGL00004P";
                rw.ReportPath = Server.MapPath($"~/Report/Rdlc/OGL00004P.rdlc");

                rw.ReportParameters.Add("investUpDt", model.investUpDt.Split(' ')[0]);
                rw.ReportParameters.Add("investUpId", model.investUpId);
                rw.ReportParameters.Add("investApprId", model.investApprId);


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


    }
}