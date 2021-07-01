using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：存摺顯示字樣設定作業的TABLE檔覆核作業
/// 初版作者：20190131 Mark
/// 修改歷程：20190131 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB016AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        FRTWordHisDao fRTWordHisDao = null;
        FRTWordDao fRTWordDao = null;
        public ORTB016AController()
        {
            fRTWordHisDao = new FRTWordHisDao();
            fRTWordDao = new FRTWordDao();
        }

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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB016A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

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
            try
            {
                List<ORTB016Model> rows = fRTWordHisDao.qryForSTAT();

                var jsonData = new { success = true, rows };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString);
                var jsonData = new { success = false, err = e.ToString() };
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
        public JsonResult execSave(List<ORTB016Model> recData, List<ORTB016Model> rtnData)
        {
            try
            {
                FRT.Web.Models.dbFGLEntities db = new Models.dbFGLEntities();
                DateTime dtn = DateTime.Now;
                recData = recData ?? new List<ORTB016Model>();
                rtnData = rtnData ?? new List<ORTB016Model>();
                //處理駁回資料
                if (rtnData.Any())
                {
                    foreach (ORTB016Model d in rtnData)
                    {
                        if (d.updId.Equals(Session["UserID"].ToString()))
                            return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        d.apprId = Session["UserID"].ToString();
                    }
                }
                //處理核可資料
                if (recData.Any())
                {
                    foreach (ORTB016Model d in recData)
                    {
                        if (d.updId.Equals(Session["UserID"].ToString()))
                            return Json(new { success = false, err = "申請人與覆核人員不可相同!!" }, JsonRequestBehavior.AllowGet);
                        d.apprId = Session["UserID"].ToString();
                    }
                }
                var msg = fRTWordDao.reject(rtnData, db);
                if (!string.IsNullOrEmpty(msg))
                    return Json(new { success = false, err = msg });
                var msg1 = fRTWordDao.appr(recData, db, dtn);
                if(!string.IsNullOrEmpty(msg1))
                    return Json(new { success = false, err = msg1});
                var msg2 = fRTWordHisDao.updateStat(recData, rtnData, db, dtn);
                if (!string.IsNullOrEmpty(msg2))
                {
                    logger.Error("[execSave]其它錯誤：" + msg2);
                    return Json(new { success = false, err = "更新失敗，請洽系統管理員!!" });
                }
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                logger.Error("[execSave]其它錯誤：" + e.ToString());

                return Json(new { success = false, err = "其它錯誤，請洽系統管理員!!" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}