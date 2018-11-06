using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Enum;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫登記簿覆核作業
/// 初版作者：20181002 李彥賢
/// 修改歷程：20181002 李彥賢
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Controllers
{
    public class TREAReviewWorkController : CommonController
    {
        private ITREAReviewWork TREAReviewWork;

        public TREAReviewWorkController()
        {
            TREAReviewWork = new TREAReviewWork();
        }
        // GET: TREAReviewWork
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TREAReviewWork/");
            ViewBag.vUser_Id = AccountController.CurrentUserId;
            return View();
        }

        /// <summary>
        /// 取得初始資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetFirstDatas()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = TREAReviewWork.GetSearchDatas(AccountController.CurrentUserId);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TREAReviewWorkDetailViewData);
                Cache.Set(CacheList.TREAReviewWorkDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }
        /// <summary>
        /// 單號查Details
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDetailsDatas(string registerId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = TREAReviewWork.GetDetailsDatas(registerId);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TREAReviewWorkSearchDetailViewData);
                Cache.Set(CacheList.TREAReviewWorkSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
                result.Datas = registerId;
            }
            return Json(result);
        }
        /// <summary>
        /// 核准
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Approved()
        {
            MSGReturnModel<List<TREAReviewWorkDetailViewModel>> result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TREAReviewWorkDetailViewData))
            {
                var datas = (List<TREAReviewWorkDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkDetailViewData);
                if(datas.Any(x => x.Ischecked))
                {
                    result = TREAReviewWork.InsertApplyData(datas, AccountController.CurrentUserId);
                }    
                else
                {
                    result.DESCRIPTION = "無勾選覆核項目";
                    return Json(result);
                }
                    
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TREAReviewWorkDetailViewData);
                    Cache.Set(CacheList.TREAReviewWorkDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 駁回
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(string rejectReason)
        {
            MSGReturnModel<List<TREAReviewWorkDetailViewModel>> result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TREAReviewWorkDetailViewData))
            {
                var datas = (List<TREAReviewWorkDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkDetailViewData);
                result = TREAReviewWork.RejectData(rejectReason, datas, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TREAReviewWorkDetailViewData);
                    Cache.Set(CacheList.TREAReviewWorkDetailViewData, result.Datas);
                }
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
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TREAReviewWorkDetailViewData))
            {
                var tempData = (List<TREAReviewWorkDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkDetailViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.hvTREA_REGISTER_ID == checkedmodel);
                if (updateTempData != null)
                {
                    updateTempData.Ischecked = checkedFlag;
                    Cache.Invalidate(CacheList.TREAReviewWorkDetailViewData);
                    Cache.Set(CacheList.TREAReviewWorkDetailViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    //檢查已勾選的項目是否有最後修改者為目前登入者
                    var viewModel = (List<TREAReviewWorkDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkDetailViewData);
                    bool canDo = false;
                    viewModel.ForEach(x => {
                        if (x.Ischecked && x.vLAST_UPDATE_UID == AccountController.CurrentUserId)
                        {
                            canDo = false;
                        }
                        else
                        {
                            canDo = true;
                        }
                    });
                    if(viewModel.All(x => x.Ischecked == false))
                        canDo = true;

                    result.Datas = canDo;

                    //if (canDo == true)
                    //{
                    //    result.Datas = true;   //可以執行覆核、駁回
                    //}
                    //else
                    //{
                    //    result.Datas = false;
                    //}
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }
        /// <summary>
        /// 取得CacheData
        /// </summary>
        /// <param name="jdta"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdta, string type)
        {
            switch (type)
            {
                case "Search":
                    var SearchDatas = (List<TREAReviewWorkDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkDetailViewData);
                    return Json(jdta.modelToJqgridResult(SearchDatas.OrderBy(x => x.vTREA_REGISTER_ID).ToList()));
                case "DetilsData":
                    var DetailDatas = (List<TREAReviewWorkSearchDetailViewModel>)Cache.Get(CacheList.TREAReviewWorkSearchDetailViewData);
                    return Json(jdta.modelToJqgridResult(DetailDatas.ToList()));
            }
            return null;
        }
    }
}