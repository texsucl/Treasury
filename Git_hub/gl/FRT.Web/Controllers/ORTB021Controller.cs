using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Actual;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static FRT.Web.BO.Utility;

/// <summary>
/// 功能說明：快速付款結案日出款立帳資料沖銷查詢及印表
/// 初版作者：20210224 Mark
/// 修改歷程：20210224 Mark
///           需求單號：202101280265-00
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB021Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        private IORTB021 ORTB021 { get; set; }

        public ORTB021Controller()
        {
            Cache = new DefaultCacheProvider();
            ORTB021 = new ORTB021();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB021/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            return View();
        }

        /// <summary>
        /// 查詢快速付款結案日出款立帳資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORTB021(string close_date_S, string close_date_E)
        {
            return Json(searchORTB021(close_date_S, close_date_E));
        }

        public MSGReturnModel<bool> searchORTB021(string close_date_S, string close_date_E)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var _result = ORTB021.GetSearchData(close_date_S, close_date_E);
            Cache.Invalidate(CacheList.ORTB021ViewData);
            if (_result.RETURN_FLAG)
            {
                Cache.Set(CacheList.ORTB021ViewData, _result.Datas);
                result.Datas = _result.Datas.Any() && _result.Datas.Any(x => x.REMAIN_AMT != "0" || x.CANCEL_AMT != "0");
            }
            result.RETURN_FLAG = _result.RETURN_FLAG;
            result.DESCRIPTION = _result.DESCRIPTION;
            return result;
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.ORTB021ViewData))
            {
                return Json(jdata.modelToJqgridResult((List<ORTB021ViewModel>)Cache.Get(CacheList.ORTB021ViewData)));
            }
            return null;
        }
    }
}