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
/// 功能說明：OAP0026A 抽票部門權限關聯覆核
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
    public class OAP0026AController : CommonController
    {
        private IOAP0026A OAP0026A;

        public OAP0026AController()
        {
            OAP0026A = new OAP0026A();
        }

        // GET: OAP0026A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0026A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            var datas = new Service.Actual.Common().tupleToSelectOption(new OAP0026().getData(true));
            datas.Insert(0, new SelectOption() { Text = "All", Value = "All" });
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.AP_PAID_A = new SelectList(datas, "Value", "Text");
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public JsonResult SearchData(OAP0026SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0022SearchData);
            Cache.Set(CacheList.OAP0022SearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0026A(searchModel);
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
            var OAP0026AViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026AViewData);
            if (!OAP0026AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇核可資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0026A.ApprovedData(OAP0026AViewData.Where(x => x.Ischecked), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchOAP0026A();
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
            var OAP0026AViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026AViewData);
            if (!OAP0026AViewData.Any(x => x.Ischecked))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = "無選擇退件資料!" });
            }
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0026A.RejectedData(OAP0026AViewData.Where(x => x.Ischecked), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchOAP0026A();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 勾選觸發事件
        /// </summary>
        /// <param name="aply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string aply_no, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026AViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.aply_no == aply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.OAP0026AViewData);
                Cache.Set(CacheList.OAP0026AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        private bool searchOAP0026A(OAP0026SearchModel searchModel = null)
        {
            List<OAP0026ViewModel> result = new List<OAP0026ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0026SearchModel)Cache.Get(CacheList.OAP0026ASearchData);
            if (searchModel != null)
            {
                result = OAP0026A.GetSearchData(searchModel,AccountController.CurrentUserId);
                Cache.Invalidate(CacheList.OAP0026AViewData);
                Cache.Set(CacheList.OAP0026AViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0026AViewData":
                    var OAP0026AViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026AViewData);
                    return Json(jdata.modelToJqgridResult(OAP0026AViewData));
            }
            return null;
        }
    }
}