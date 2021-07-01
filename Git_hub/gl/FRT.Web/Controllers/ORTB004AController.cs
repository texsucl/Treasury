using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.EasycomClient;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款匯款失敗原因對照TABLE覆核作業
/// 初版作者：20180706 Daiyu
/// 修改歷程：20180706 Daiyu
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB004AController : BaseController
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB004A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //錯誤歸屬
            ViewBag.errBelongjqList = sysCodeDao.jqGridList("RT", "ERR_BELONG", true);


            return View();
        }


        /// <summary>
        /// 查詢待覆核的資料
        /// </summary>
        /// <param name="cReviewType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadData()
        {
            FRTBERHDao fRTBERHDao = new FRTBERHDao();

            try {
                List<ORTB004Model> rows = fRTBERHDao.qryForORTB004A();
                var jsonData = new { success = true, rows };
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
        public JsonResult execSave(List<ORTB004Model> recData, List<ORTB004Model> rtnData)
        {

            using (EacConnection conn = new EacConnection(CommonUtil.GetEasycomConn())) {
                conn.Open();

                EacTransaction transaction = conn.BeginTransaction();
                //EacTransaction transaction = null;

                try
                {
                    FRTBERHDao fRTBERHDao = new FRTBERHDao();

                    //處理駁回資料
                    if (rtnData.Count > 0) {
                        foreach (ORTB004Model d in rtnData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        }
                        fRTBERHDao.updateFRTBERH0(Session["UserID"].ToString(), "3", rtnData, conn, transaction);
                    }
                        


                    //處理核可資料
                    if (recData.Count > 0) {
                        FRTBERMDao fRTBERMDao = new FRTBERMDao();
                        foreach (ORTB004Model d in recData)
                        {
                            if (d.updId.Equals(Session["UserID"].ToString()))
                                return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        }
                        fRTBERHDao.updateFRTBERH0(Session["UserID"].ToString(), "2", recData, conn, transaction);
                        fRTBERMDao.apprFRTBERM0(Session["UserID"].ToString(), recData, conn, transaction);
                    }

                    transaction.Commit();
                    return Json(new { success = true });
                }
                catch (Exception e) {
                    transaction.Rollback();

                    logger.Error("[execSave]其它錯誤：" + e.ToString());

                    return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
                }

            }
        }
    }
}