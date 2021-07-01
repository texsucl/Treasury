using FAP.Web.ActionFilter;
using FAP.Web.AS400Models;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Data.SqlClient;
using System.Transactions;
using System.Web.Mvc;
using System.Linq;

/// <summary>
/// 功能說明：逾期未兌領支票維護作業-明細檔
/// 初版作者：20190419 Daiyu
/// 修改歷程：20190419 Daiyu
///           需求單號：
///           初版
/// ------------------------------------------------------
/// 修改歷程：20191122 Daiyu
/// 需求單號：201910290100-01
/// 修改內容：當此信函編號對應的所有歸戶的支票號碼在PPAA的FILLER_14為03+”*”，不可刪除此信函編號，提示訊息”此筆為制裁凍結案件，不可刪除!”
/// </summary>
///

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0003Controller : BaseController
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
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0003/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();
            //原給付方式(給付項目)
            ViewBag.oPaidCdjqList = fPMCODEDao.jqGridList("PAID_CDTXT", "AP", true);

            //處理階段
            ViewBag.rStatusList = fPMCODEDao.qryGrpList("PPADSTATUS", "AP");


            return View();
        }


        /// <summary>
        /// 畫面執行查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryPPAD(OAP0003Model model)
        {
            if("".Equals(StringUtil.toString(model.report_no)))
                Json(new { success = false, err = "請輸入「信函編號」!!" });

            string hisApprStat = "";

            APAplyRecModel aply = new APAplyRecModel();
            //依「信函編號」查是否有覆核中的資料
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
            }))
            {
                using (dbFGLEntities db = new dbFGLEntities())
                {
                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    List<APAplyRecModel> aplyList = new List<APAplyRecModel>();
                    aplyList = fAPAplyRecDao.qryAplyType("B", "1", model.report_no, db);

                    if (aplyList.Count > 0) {
                        ObjectUtil.CopyPropertiesTo(aplyList[0], aply);
                        hisApprStat = aply.appr_stat;
                    }
                }
            }


            int cnt = 0;
            using (EacConnection conn400 = new EacConnection(CommonUtil.GetEasycomConn()))
            {
                conn400.Open();

                try
                {
                    FMNPPADDao fMNPPADDao = new FMNPPADDao();
                    List<OAP0003PoliModel> ppadList = fMNPPADDao.qryForOAP0003(conn400, model.report_no);

                    int amlCnt = 0;
                    if (ppadList.Count > 0) {
                        ObjectUtil.CopyPropertiesTo(ppadList[0], model);
                        cnt = ppadList.Count;
                        amlCnt = ppadList.Where(x => x.filler_14.Equals("03*")).Count();   //add by daiyu 20191122
                    }


                    writePiaLog(model.report_no, ppadList.Count, model.paid_id, "Q");
                    var jsonData = new { success = true, cnt = cnt, dataSum = model, policyList = ppadList, hisApprStat = hisApprStat, amlCnt = amlCnt };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {

                    logger.Error(e.ToString());
                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" });
                }
            }
        }


        /// <summary>
        /// 稽核軌跡
        /// </summary>
        /// <param name="checkNo"></param>
        /// <param name="affectRows"></param>
        /// <param name="piaOwner"></param>
        /// <param name="executionType"></param>
        private void writePiaLog(string checkNo, int affectRows, string piaOwner, string executionType) {
            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
            piaLogMain.TRACKING_TYPE = "A";
            piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
            piaLogMain.ACCOUNT_NAME = Session["UserName"].ToString();
            piaLogMain.PROGFUN_NAME = "OAP0003Controller";
            piaLogMain.EXECUTION_CONTENT = checkNo;
            piaLogMain.AFFECT_ROWS = affectRows;
            piaLogMain.PIA_TYPE = "0100100000";
            piaLogMain.EXECUTION_TYPE = executionType;
            piaLogMain.ACCESSOBJ_NAME = "FMNPPAD0";
            piaLogMain.PIA_OWNER1 = piaOwner;
            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
            piaLogMainDao.Insert(piaLogMain);

        }


        /// <summary>
        /// 畫面執行申請覆核
        /// </summary>
        /// <param name="model"></param>
        /// <param name="policyNoData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execAply(string report_no, string execAction)
        {
            string aply_no = "";
            string strConn = DbUtil.GetDBFglConnStr();
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlTransaction transaction = conn.BeginTransaction("Transaction");
                try
                {
                    string[] curDateTime = DateUtil.getCurChtDateTime(4).Split(' ');

                    FAPAplyRecDao fAPAplyRecDao = new FAPAplyRecDao();
                    FAP_APLY_REC aplyRec = new FAP_APLY_REC();
                    aplyRec.aply_type = "B";
                    aplyRec.appr_stat = "1";
                    aplyRec.appr_mapping_key = report_no;
                    aplyRec.create_id = Session["UserID"].ToString();

                    //新增"覆核資料檔"
                    aply_no = fAPAplyRecDao.insert("", aplyRec, conn, transaction);

                    transaction.Commit();

                    return Json(new { success = true, aplyNo = aply_no });
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    logger.Error("其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }
            }

            
        }
    }
}