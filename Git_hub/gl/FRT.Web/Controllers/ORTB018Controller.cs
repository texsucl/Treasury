using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款改FBO匯款轉檔回復作業
/// 初版作者：20190223 Mark
/// 修改歷程：20190223 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB018Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ORTB018Controller()
        {

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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB018/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //DATA_STATUS
            var _data_Status = sysCodeDao.qryByTypeDic("RT", "DATA_STATUS");
            ViewBag.dataStatusjqList = _data_Status;
            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

            return View();
        }

        /// <summary>
        /// 查詢"FRT_WORD"資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORTB018(string filler_20)
        {
            logger.Info("qryORTB018 Query!!");
            MSGReturnModel<ORTB018Model> result = new MSGReturnModel<ORTB018Model>();
            result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            try
            {
                List<FBOModel> rows = new List<FBOModel>();
                result = new FRTFBODao().qryForORTB018(filler_20?.Trim());
                return Json(result);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }

        /// <summary>
        /// 針對點選的快速付款編號執行動作
        /// </summary>
        /// <param name="fastNos">快速付款編號</param>
        /// <param name="typeFlag">選擇動作 P:報表 T:txt</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ORTB018Event(string filler_20)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = "系統發生錯誤，請洽系統管理員!!";
            logger.Info("ORTB018Event !!");
            try
            {
                result = new FRTFBODao().updForORTB018(filler_20?.Trim());
                return Json(result);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }
    }
}