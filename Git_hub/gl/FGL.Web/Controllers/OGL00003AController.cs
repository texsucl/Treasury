using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Daos;
using FGL.Web.Models;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;

/// <summary>
/// 功能說明：商品資料維護作業
/// 初版作者：20190114 Daiyu
/// 修改歷程：20190114 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00003AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00003A/");
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
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string updDateB, string updDateE, string item)
        {
            FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
            List<OGL00003Model> rows = new List<OGL00003Model>();
            string[] apprMk = new string[] { "1" };
            rows = fGLItemInfoHisDao.qryByApprMk(apprMk, "OGL00003", updDateB, updDateE, item, false);


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OGL00003Model d in rows)
                {
                    createUid = StringUtil.toString(d.prodUpId);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.prodUpId = createUid + " " + userNameMap[createUid];
                    }
                }
            }

              


            var jsonData = new { success = true, rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 查詢FGLSMPB取得SA、GA項目
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qrySmpNum(string aplyNo, string productType, string fuMk, string itemCon, string discPartFeat)
        {
            List<smpNumModel> gaList = new List<smpNumModel>();
            List<smpNumModel> saList = new List<smpNumModel>();

            FGLSMPBDao fGLSMPBDao = new FGLSMPBDao();
            string[] acctType = new string[] {"B" };
            List<OGL10181Model> rows = fGLSMPBDao.qryForItem(productType, fuMk, itemCon, discPartFeat, acctType);

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

            FGLItemSmpnumHisDao fGLItemSmpnumHisDao = new FGLItemSmpnumHisDao();
            List<OGL00003DModel>  sumpD = fGLItemSmpnumHisDao.qryByAplyNo(aplyNo, "P");

            return Json(new { success = true, gaList = gaList, saList = saList, sumpD = sumpD }, JsonRequestBehavior.AllowGet);
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
            //OGL00003Model formal = fGLItemInfoDao.qryPK(item,productType,fuMk, itemCon, discPartFeat, "6");
            OGL00003Model formal = fGLItemInfoDao.qryItem(item, "6");
            return Json(new { success = true, formal = formal }, JsonRequestBehavior.AllowGet); 
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
        /// 開啟修改明細畫面
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public ActionResult detailAply(string aplyNo)
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

            //建立來源
            var flagList = sysCodeDao.loadSelectList("GL", "ITEM_Flag", true);
            ViewBag.flagList = flagList;

            bool bHaveData = false;
            try
            {
                FGLItemInfoHisDao fGLItemInfoHisDao = new FGLItemInfoHisDao();
                
                //查詢"會計商品資訊暫存檔"
                if (!"".Equals(StringUtil.toString(aplyNo)))
                {
                    modelHis = fGLItemInfoHisDao.qryAplyNo(aplyNo);

                    if (modelHis != null)
                        if (!"".Equals(StringUtil.toString(modelHis.aplyNo)))
                            bHaveData = true;
                }


                if (!bHaveData) {
                    ViewBag.bHaveData = "N";
                    return View(modelHis);
                } else
                {
                    if (!"O".Equals(modelHis.flag))
                    {
                        NewAcctItemUtil newAcctItemUtil = new NewAcctItemUtil();
                        OGL00001Model itemAcct = newAcctItemUtil.qryItemAcct(modelHis.productType, modelHis.fuMk, modelHis.itemCon, modelHis.discPartFeat);

                        ViewBag.itemHealthMgrMk = itemAcct.healthMgrMk;
                        ViewBag.itemInvestTradMk = itemAcct.investTradMk;
                        ViewBag.itemInvestTypeMk = itemAcct.investTypeMk;
                        ViewBag.itemLodprmMk = itemAcct.lodprmMk;
                        ViewBag.itemPakindDmopMk = itemAcct.pakindDmopMk;
                        ViewBag.itemCoiType = itemAcct.coiType;
                    }
                        
                }


                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        modelHis.prodUpId = modelHis.prodUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.prodUpId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }


                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
                ViewBag.execAction = modelHis.execAction;

                

                return View(modelHis);

            }
            catch (Exception e)
            {
                ViewBag.bHaveData = "N";
                return View(modelHis);
            }
        }


        

        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="execType"></param>
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

                   

                    if (StringUtil.toString(modelHis.prodUpId).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                    string investTradMk = "";

                    if (!"O".Equals(modelHis.flag)) {
                        //查詢"商品科目設定檔"
                        FGLItemAcctDao fGLItemAcctDao = new FGLItemAcctDao();
                        List<OGL00001Model> itemAcct = fGLItemAcctDao.qryHeadForItem(modelHis.productType, modelHis.fuMk, modelHis.itemCon, modelHis.discPartFeat);

                        if (itemAcct.Count == 0)
                            return Json(new { success = false, err = "查無「商品科目設定」資料!!" }, JsonRequestBehavior.AllowGet);

                        investTradMk = itemAcct[0].investTradMk;
                    }

                    string apprMk = "";

                    //申請"新增商品"
                    if ("A".Equals(modelHis.execAction))
                    {
                        if ("R".Equals(execType))   //執行"退回"
                            apprMk = "8";
                        else   //執行"核可"
                            apprMk = investTradMk == "Y" ? (StringUtil.toString(modelHis.investApprId) == "" ? "2" : "12") : "12";
                    }
                    else
                    {
                        if ("R".Equals(execType))   //執行"退回"
                            apprMk = "8";
                        else   //執行"核可"
                            apprMk = investTradMk == "N" ? "12" : (modelHis.execAction == "D" ? "2" : "12"); 
                    }


                    fGLItemInfoHisDao.updateApprMk(aplyNo, apprMk, "OGL00003A", Session["UserID"].ToString(), execType, conn, transaction);
                    

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, errors = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }

        }



        

    }
}