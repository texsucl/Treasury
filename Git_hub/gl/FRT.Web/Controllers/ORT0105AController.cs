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
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：比對報表勾稽_批次定義(OPEN跨系統勾稽)
/// 初版作者：20210209 Mark
/// 修改歷程：20210209 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0105AController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IORT0105A ORT0105A;

        internal ICacheProvider Cache { get; set; }

        public ORT0105AController()
        {
            Cache = new DefaultCacheProvider();
            ORT0105A = new ORT0105A();
        }

        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0105A/");
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
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public JsonResult SearchData()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchORT0105A();
                result.RETURN_FLAG = model.RETURN_FLAG;
                result.DESCRIPTION = model.DESCRIPTION;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <returns></returns>
        public JsonResult ApprovedData(string his_check_id = null)
        {
            var ORT0105AViewData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData);
            if ((!his_check_id.IsNullOrWhiteSpace() && !ORT0105AViewData.Any(x => x.his_check_id == his_check_id)) 
                && !ORT0105AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇核可資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = ORT0105A.ApprovedData(his_check_id.IsNullOrWhiteSpace() ?
                    ORT0105AViewData.Where(x => x.Ischecked) :
                    ORT0105AViewData.Where(x => x.his_check_id == his_check_id), 
                    AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchORT0105A();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <returns></returns>
        public JsonResult RejectedData(string his_check_id = null)
        {
            var ORT0105AViewData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData);
            if ((!his_check_id.IsNullOrWhiteSpace() && !ORT0105AViewData.Any(x => x.his_check_id == his_check_id)) 
                && !ORT0105AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇退件資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = ORT0105A.RejectedData(his_check_id.IsNullOrWhiteSpace() ? 
                    ORT0105AViewData.Where(x => x.Ischecked) :
                    ORT0105AViewData.Where(x => x.his_check_id == his_check_id),
                    AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchORT0105A();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        public JsonResult CheckChange(string his_check_id, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.his_check_id == his_check_id);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.ORT0105AViewData);
                Cache.Set(CacheList.ORT0105AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        private MSGReturnModel searchORT0105A()
        {
            MSGReturnModel result = new MSGReturnModel();
            var _result = ORT0105A.GetSearchData(AccountController.CurrentUserId);
            if (_result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ORT0105AViewData);
                Cache.Set(CacheList.ORT0105AViewData, _result.Datas);
            }
            result.RETURN_FLAG = _result.RETURN_FLAG;
            result.DESCRIPTION = _result.DESCRIPTION;
            return result;
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type, string his_check_id = null)
        {
            switch (type)
            {
                case "ORT0105AViewData":
                    return Json(jdata.modelToJqgridResult((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData)));
                case "ORT0105AViewSubData":
                    var ORT0105AViewSubData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData);
                    return Json(jdata.modelToJqgridResult(((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105AViewData))
                        .First(x => x.his_check_id == his_check_id).subDatas));
            }
            return null;
        }
    }
}