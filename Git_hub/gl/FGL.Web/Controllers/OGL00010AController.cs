using FGL.Web.ActionFilter;
using FGL.Web.BO;
using FGL.Web.Service.Actual;
using FGL.Web.Service.Interface;
using FGL.Web.Utilitys;
using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static FGL.Web.BO.Utility;
using static FGL.Web.Enum.Ref;


/// <summary>
/// 功能說明：退費類別覆核
/// 初版作者：20200712 Mark
/// 修改歷程：20200712 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FGL.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OGL00010AController : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOGL00010A OGL00010A;

        public OGL00010AController()
        {
            OGL00010A = new OGL00010A();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OGL00010A/");
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
        public JsonResult qryOGL00010A(string payclass)
        {
            Cache.Invalidate(CacheList.OGL00010ASearchData);
            Cache.Set(CacheList.OGL00010ASearchData, payclass);
            MSGReturnModel result = new MSGReturnModel();
            result = searchOGL00010A(payclass);
            return Json(result);
        }

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="pk_id"></param>
        /// <returns></returns>
        public JsonResult ApprovedData(string pk_id = null)
        {
            var OGL00010AViewDatas = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010AViewData);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (pk_id.IsNullOrWhiteSpace())
                {
                    if (!OGL00010AViewDatas.Any(x => x.Ischecked))
                    {
                        return Json(new MSGReturnModel() { DESCRIPTION = "無選擇核可資料!" });
                    }
                    result = OGL00010A.ApprovedData(OGL00010AViewDatas.Where(x => x.Ischecked), AccountController.CurrentUserId);
                }
                else
                {
                    result = OGL00010A.ApprovedData(OGL00010AViewDatas.Where(x => x.pk_id == pk_id), AccountController.CurrentUserId);
                }            
                if (result.RETURN_FLAG)
                    searchOGL00010A(null,true);
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
        /// <param name="pk_id"></param>
        /// <returns></returns>
        public JsonResult RejectedData(string pk_id = null)
        {
            var OGL00010AViewDatas = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010AViewData);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                if (pk_id.IsNullOrWhiteSpace())
                {
                    if (!OGL00010AViewDatas.Any(x => x.Ischecked))
                    {
                        return Json(new MSGReturnModel() { DESCRIPTION = "無選擇退件資料!" });
                    }
                    result = OGL00010A.RejectedData(OGL00010AViewDatas.Where(x => x.Ischecked), AccountController.CurrentUserId);
                }
                else
                {
                    result = OGL00010A.RejectedData(OGL00010AViewDatas.Where(x => x.pk_id == pk_id), AccountController.CurrentUserId);
                }
                if (result.RETURN_FLAG)
                    searchOGL00010A(null, true);
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
        public JsonResult CheckChange(string pk_id, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010AViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.pk_id == pk_id);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.OGL00010AViewData);
                Cache.Set(CacheList.OGL00010AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        } 

        private MSGReturnModel searchOGL00010A(string payclass = null, bool saveFlag = false)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (payclass == null)
                payclass = (string)Cache.Get(CacheList.OGL00010ASearchData) ?? string.Empty;
            if (payclass != null)
            {
                var _result = OGL00010A.GetSearchData(payclass, AccountController.CurrentUserId);
                if (_result.RETURN_FLAG || saveFlag)
                {
                    Cache.Invalidate(CacheList.OGL00010AViewData);
                    Cache.Set(CacheList.OGL00010AViewData, _result.Datas);
                }
                result.RETURN_FLAG = _result.RETURN_FLAG;
                result.DESCRIPTION = _result.DESCRIPTION;
            }
            return result;
        }


        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OGL00010AViewData":
                    var OGL00010AViewData = (List<OGL00010ViewModel>)Cache.Get(CacheList.OGL00010AViewData);
                    return Json(jdata.modelToJqgridResult(OGL00010AViewData));
            }
            return null;
        }
    }
}