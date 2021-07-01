using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0018AController : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOAP0018A OAP0018A;
        public OAP0018AController()
        {
            OAP0018A = new OAP0018A();
        }

        // GET: OAP0018A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0018A/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(OAP0018ASearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.query_Not_Find.GetDescription();
            try
            {
                searchModel.current_uid = AccountController.CurrentUserId;

                Cache.Invalidate(CacheList.OAP0018ASearchData);
                Cache.Set(CacheList.OAP0018ASearchData, searchModel);

                var datas = OAP0018A.GetReviewSearchDetail(searchModel);
                if (datas.Any())
                {
                    Cache.Invalidate(CacheList.OAP0018AViewData);
                    Cache.Set(CacheList.OAP0018AViewData, datas);
                    result.RETURN_FLAG = true;
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
        /// Detail 核准
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <returns></returns>
        public JsonResult ApprovedDetailData(string AplyNo)
        {
            MSGReturnModel<List<OAP0018AViewModel>> result =
               new MSGReturnModel<List<OAP0018AViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            try
            {
                if (Cache.IsSet(CacheList.OAP0018AViewData))
                {
                    var datas = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);
                    var _datas = datas.Where(x => x.aply_no == AplyNo).ToList();

                    var searchData = (OAP0018ASearchViewModel)Cache.Get(CacheList.OAP0018ASearchData);
                    result = OAP0018A.ApprovedData(searchData, _datas);
                    if (result.RETURN_FLAG)
                    {
                        Cache.Invalidate(CacheList.OAP0018AViewData);
                        Cache.Set(CacheList.OAP0018AViewData, result.Datas);
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
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
        /// 核准
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApprovedData(List<string> AplyNos)
        {
            MSGReturnModel<List<OAP0018AViewModel>> result =
                new MSGReturnModel<List<OAP0018AViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            try
            {
                //if (AplyNos.Any() && Cache.IsSet(CacheList.ReviewRelationViewData))
                if (Cache.IsSet(CacheList.OAP0018AViewData))
                {
                    var datas = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);

                    var _datas = datas.Where(x => x.Ischecked).ToList();

                    var searchData = (OAP0018ASearchViewModel)Cache.Get(CacheList.OAP0018ASearchData);
                    result = OAP0018A.ApprovedData(searchData, _datas);
                    if (result.RETURN_FLAG)
                    {
                        Cache.Invalidate(CacheList.OAP0018AViewData);
                        Cache.Set(CacheList.OAP0018AViewData, result.Datas);
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
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
        /// 駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RejectedData(string AplyNo)
        {
            string apprDesc = string.Empty;
            MSGReturnModel<List<OAP0018AViewModel>> result = new MSGReturnModel<List<OAP0018AViewModel>>();
            List<OAP0018AViewModel> _datas = new List<OAP0018AViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            try
            {
                //if (AplyNos.Any() && Cache.IsSet(CacheList.ReviewRelationViewData))
                if (Cache.IsSet(CacheList.OAP0018AViewData))
                {
                    var datas = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);
                    if (AplyNo.IsNullOrWhiteSpace())
                        _datas = datas.Where(x => x.Ischecked).ToList();
                    else
                        _datas = datas.Where(x => x.aply_no == AplyNo).ToList();

                    var searchData = (OAP0018ASearchViewModel)Cache.Get(CacheList.OAP0018ASearchData);

                    result = OAP0018A.RejectedData(searchData, _datas, apprDesc);
                    if (result.RETURN_FLAG)
                    {
                        Cache.Invalidate(CacheList.OAP0018AViewData);
                        Cache.Set(CacheList.OAP0018AViewData, result.Datas);
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
        /// Checkbox 選取事件
        /// </summary>
        /// <param name="checkedmodel"></param>
        /// <param name="takeoutFlag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckedData(string checkedmodel, bool checkedFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.OAP0018AViewData))
            {
                var tempData = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.aply_no == checkedmodel);
                if (updateTempData != null)
                {
                    updateTempData.Ischecked = checkedFlag;

                    Cache.Invalidate(CacheList.OAP0018AViewData);
                    Cache.Set(CacheList.OAP0018AViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.update_Success.GetDescription();
                    //檢查已勾選的項目是否有申請者為目前登入者
                    var viewModel = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);
                    bool canDo = false;
                    //viewModel.ForEach(x =>
                    //{
                    //    if (x.Ischecked && x.aply_id == AccountController.CurrentUserId)
                    //    {
                    //        canDo = false;
                    //    }
                    //    else
                    //    {
                    //        canDo = true;
                    //    }
                    //});

                    if(viewModel.Any(x => x.Ischecked && x.aply_id == AccountController.CurrentUserId))
                        canDo = false;
                    else
                        canDo = true;

                    if (viewModel.All(x => x.Ischecked == false))
                        canDo = false;

                    result.Datas = canDo;
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult ChangeRecordView(string AplyNo)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.query_Not_Find.GetDescription();
            try
            {
                var datas = OAP0018A.GetHisData(AplyNo);
                if (datas.Any())
                {
                    Cache.Invalidate(CacheList.OAP0018AHisViewData);
                    Cache.Set(CacheList.OAP0018AHisViewData, datas);
                    result.RETURN_FLAG = true;
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
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Search":
                    var Datas = (List<OAP0018AViewModel>)Cache.Get(CacheList.OAP0018AViewData);
                    return Json(jdata.modelToJqgridResult(Datas));
                case "His":
                    var His_Datas = (List<OAP0018AHisViewModel>)Cache.Get(CacheList.OAP0018AHisViewData);
                    return Json(jdata.modelToJqgridResult(His_Datas));
            }
            return null;
        }
    }
}