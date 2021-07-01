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
/// 功能說明：投資商品資料維護作業
/// 初版作者：20190117 Daiyu
/// 修改歷程：20190117 Daiyu
///           需求單號：201805080167-00
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00004AController : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00004A/");
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
            string[] apprMk = new string[] { "3" };
            rows = fGLItemInfoHisDao.qryByApprMk(apprMk, "OGL00004", updDateB, updDateE, item, false);


            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                Dictionary<string, string> userNameMap = new Dictionary<string, string>();
                OaEmpDao oaEmpDao = new OaEmpDao();
                string createUid = "";

                foreach (OGL00003Model d in rows)
                {
                    createUid = StringUtil.toString(d.investUpId);

                    if (!"".Equals(createUid))
                    {
                        if (!userNameMap.ContainsKey(createUid))
                        {
                            userNameMap = oaEmpDao.qryUsrName(userNameMap, createUid, dbIntra);
                        }
                        d.investUpId = createUid + " " + userNameMap[createUid];
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
            string[] acctType = new string[] { "C" };
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
            List<OGL00003DModel>  sumpD = fGLItemSmpnumHisDao.qryByAplyNo(aplyNo, "I");

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
                }


                OaEmpDao oaEmpDao = new OaEmpDao();
                using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
                {
                    try
                    {
                        modelHis.investUpId = modelHis.investUpId == null ? "" : StringUtil.toString(oaEmpDao.qryByUsrId(modelHis.investUpId, dbIntra).EMP_NAME);
                    }
                    catch (Exception e)
                    {

                    }
                }



                ViewBag.bHaveData = "Y";
                ViewBag.aplyNo = aplyNo;
                ViewBag.execAction = modelHis == null ? "" : modelHis.execAction ?? "";

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

                    if (StringUtil.toString(modelHis.investUpId).Equals(Session["UserID"].ToString()))
                        return Json(new { success = false, errors = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);


                    string apprMk = execType == "R" ? "10" : "4";  

                    fGLItemInfoHisDao.updateApprMk(aplyNo, apprMk, "OGL00004A", Session["UserID"].ToString(), execType, conn, transaction);
                    

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