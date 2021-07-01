using FAP.Web.BO;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using FAP.Web.Service.Interface;
using FAP.Web.Service.Actual;
using System.Configuration;

/// <summary>
/// 功能說明：應付票據變更接收作業
/// 初版作者：20191114 Mark
/// 修改歷程：20191114 Mark
///           需求單號：201910030636
///           初版    
/// </summary>
/// 


namespace FAP.Web.Controllers
{
    public class OAP0021AController : CommonController
    {
        private IOAP0021 OAP0021;

        public OAP0021AController()
        {
            OAP0021 = new OAP0021();
        }

        // GET: OAP0021A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0021A/");
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
        /// 勾選觸發事件
        /// </summary>
        /// <param name="apply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string apply_no, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OAP0021Model>)Cache.Get(CacheList.OAP0021AViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == apply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                result.Datas = ViewData.Any(x => x.checkFlag);
                Cache.Invalidate(CacheList.OAP0021AViewData);
                Cache.Set(CacheList.OAP0021AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 執行 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OAP0021APPR(bool flag)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var ViewData = (List<OAP0021Model>)Cache.Get(CacheList.OAP0021AViewData);
                var data = ViewData.Where(x => x.checkFlag).ToList();
                if (!data.Any())
                {
                    result.DESCRIPTION = MessageType.not_Find_Update_Data.GetDescription();
                }
                else
                {
                    result = OAP0021.UpdateOAP0021A(data, AccountController.CurrentUserId, flag);
                    if (result.RETURN_FLAG)
                    {
                        searchOAP0021A();
                    }
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 執行 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="apply_no"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OAP0021APPRByApply_no(bool flag,string apply_no)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0021.UpdateOAP0021A(new List<OAP0021Model>() {
                new OAP0021Model() { apply_no = apply_no}
                }, AccountController.CurrentUserId, flag);
                if (result.RETURN_FLAG)
                {
                    searchOAP0021A();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢 應付票據變更接收覆核作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0021A(OAP0021ASearchModel searchModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            try
            {
                Cache.Invalidate(CacheList.OAP0021ASearchData);
                Cache.Set(CacheList.OAP0021ASearchData, searchModel);
                var model = searchOAP0021A(searchModel);
                result.RETURN_FLAG = model.Any();
                result.Datas = model.Any(x => x.checkFlag);
                if (!result.RETURN_FLAG)
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        private List<OAP0021Model> searchOAP0021A(OAP0021ASearchModel searchModel = null)
        {
            List<OAP0021Model> result = new List<OAP0021Model>();
            if(searchModel == null)
                searchModel = (OAP0021ASearchModel)Cache.Get(CacheList.OAP0021ASearchData);
            if (searchModel != null)
            {
                result = OAP0021.Search_OAP0021A(searchModel,AccountController.CurrentUserId);
                Cache.Invalidate(CacheList.OAP0021AViewData);
                Cache.Set(CacheList.OAP0021AViewData, result);
            }
            return result;
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0021AViewData":
                    var OAP0021AViewData = (List<OAP0021Model>)Cache.Get(CacheList.OAP0021AViewData);
                    return Json(jdata.modelToJqgridResult(OAP0021AViewData));
            }
            return null;
        }
    }
}