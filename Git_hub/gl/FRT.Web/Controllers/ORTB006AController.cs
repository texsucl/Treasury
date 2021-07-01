using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Web.Mvc;
using System.Linq;
using FRT.Web.AS400Models;
using FRT.Web.Models;

/// <summary>
/// 功能說明：人工修改匯款失敗原因覆核作業
/// 初版作者：20180807 Daiyu
/// 修改歷程：20180807 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB006AController : BaseController
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

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB006A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            FPMCODEDao fPMCODEDao = new FPMCODEDao();

            //匯款失敗原因
            ViewBag.failCodejqList = fPMCODEDao.jqGridList("FAIL-CODE", "", "", true);

            return View();

        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="fastNo"></param>
        /// <param name="policyNo"></param>
        /// <param name="policySeq"></param>
        /// <param name="idDup"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData(string fastNo, string policyNo, string policySeq, string idDup)
        {
            FRTBARHDao fRTBARHDao = new FRTBARHDao();

            try {
                List<ORTB006Model> rows = fRTBARHDao.qryForORTB006(fastNo, policyNo, policySeq, idDup);

                if (rows.Any())
                {
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "ORTB006AController";
                    piaLogMain.EXECUTION_CONTENT = $@"fastNo:{fastNo}";
                    piaLogMain.AFFECT_ROWS = rows.Count;
                    piaLogMain.PIA_TYPE = "0000000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "LRTBARM";
                    piaLogMainDao.Insert(piaLogMain);
                }

                double totAmt = rows.Sum(item => Convert.ToInt64(item.remitAmt));

                var jsonData = new { success = true, rows , cnt = String.Format("{0:N0}", rows.Count), totAmt = String.Format("{0:N0}", totAmt) };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString()};
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }


        



        /// <summary>
        /// 核可/退回
        /// </summary>
        /// <param name="recData"></param>
        /// <param name="rtnData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB006Model> recData, List<ORTB006Model> rtnData)
        {
            List<FRTBARMModel> sendList = new List<FRTBARMModel>();
            List<FRTBARHModel> rtnDataList = new List<FRTBARHModel>();
            List<FRTBARHModel> recDataList = new List<FRTBARHModel>();

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn())) {
                conn.Open();

                EacTransaction transaction = conn.BeginTransaction();
                //EacTransaction transaction = null;

                try
                {
                    FRTBARHDao fRTBARHDao = new FRTBARHDao();

                    //處理駁回資料
                    if (rtnData.Count > 0) {
                        foreach (ORTB006Model d in rtnData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);

                            FRTBARHModel fRTBARHModel = new FRTBARHModel();
                            fRTBARHModel.applyNo = d.applyNo;
                            fRTBARHModel.fastNo = d.fastNo;

                            rtnDataList.Add(fRTBARHModel);
                        }



                        fRTBARHDao.updateFRTBARH0(Session["UserID"].ToString(), "3", rtnDataList, conn, transaction);
                    }
                        


                    //處理核可資料
                    if (recData.Count > 0) {
                        FRTBARMDao fRTBARMDao = new FRTBARMDao();
                        foreach (ORTB006Model d in recData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                            else {
                                FRTBARMModel mainData = fRTBARMDao.qryByFastNo(d.fastNo, conn);
                                mainData.failCode = d.failCode;
                                mainData.filler_20 = d.filler_20;
                                sendList.Add(mainData);

                                FRTBARHModel fRTBARHModel = new FRTBARHModel();
                                fRTBARHModel.applyNo = d.applyNo;
                                fRTBARHModel.fastNo = d.fastNo;

                                recDataList.Add(fRTBARHModel);

                                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();
                                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                                piaLogMain.TRACKING_TYPE = "A";
                                piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                                piaLogMain.ACCOUNT_NAME = "";
                                piaLogMain.PROGFUN_NAME = "ORTB006AController";
                                piaLogMain.EXECUTION_CONTENT = $@"fastNo:{d.fastNo}";
                                piaLogMain.AFFECT_ROWS = 1;
                                piaLogMain.PIA_TYPE = "0000000000";
                                piaLogMain.EXECUTION_TYPE = "E";
                                piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                                piaLogMainDao.Insert(piaLogMain);
                            }
                        }
                        fRTBARHDao.updateFRTBARH0(Session["UserID"].ToString(), "2", recDataList, conn, transaction);
                        fRTBARMDao.apprFRTBARM0(Session["UserID"].ToString(), recData, conn, transaction);
                    }

                    transaction.Commit();

                    //寄送MAIL
                    Dictionary<string, string> errMap = procSendMail(sendList);

                    return Json(new { success = true, errMap = errMap, errCnt = errMap.Count });
                }
                catch (Exception e) {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        /// <summary>
        /// 啟動匯款失敗通知表
        /// </summary>
        /// <param name="recData"></param>
        /// <returns></returns>
        private Dictionary<string, string> procSendMail(List<FRTBARMModel> recData) {

            FastErrMailUtil fastErrMailUtil = new FastErrMailUtil();

            Dictionary<string, string> errMap = fastErrMailUtil.procSendMail("M", recData, "");
            

            return errMap;
        }

   
    }
}