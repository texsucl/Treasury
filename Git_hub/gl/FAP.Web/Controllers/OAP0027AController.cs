using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.ActionFilter;

/// <summary>
/// 功能說明：OAP0027A 抽票原因覆核
/// 初版作者：20200117 張家華
/// 修改歷程：20200117 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0027AController : CommonController
    {
        private IOAP0027A OAP0027A;

        public OAP0027AController()
        {
            OAP0027A = new OAP0027A();
        }

        // GET: OAP0027A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0027A/");
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
        public JsonResult SearchData(OAP0027SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0027ASearchData);
            Cache.Set(CacheList.OAP0027ASearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0027A(searchModel);
                result.RETURN_FLAG = model;
                if (!model)
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
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
        public JsonResult ApprovedData()
        {
            var OAP0027AViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027AViewData);
            if (!OAP0027AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇核可資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0027A.ApprovedData(OAP0027AViewData.Where(x => x.Ischecked), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchOAP0027A();
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
        public JsonResult RejectedData()
        {
            var OAP0027AViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027AViewData);
            if (!OAP0027AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇退件資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0027A.RejectedData(OAP0027AViewData.Where(x => x.Ischecked), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchOAP0027A();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult CheckChange(string aply_no, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027AViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.aply_no == aply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.OAP0027AViewData);
                Cache.Set(CacheList.OAP0027AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        private bool searchOAP0027A(OAP0027SearchModel searchModel = null)
        {
            List<OAP0027ViewModel> result = new List<OAP0027ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0027SearchModel)Cache.Get(CacheList.OAP0027ASearchData);
            if (searchModel != null)
            {
                result = OAP0027A.GetSearchData(searchModel,AccountController.CurrentUserId);
                Cache.Invalidate(CacheList.OAP0027AViewData);
                Cache.Set(CacheList.OAP0027AViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0027AViewData":
                    var OAP0027AViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027AViewData);
                    return Json(jdata.modelToJqgridResult(OAP0027AViewData));
            }
            return null;
        }
    }
}