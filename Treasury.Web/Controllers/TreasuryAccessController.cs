using Treasury.WebActionFilter;
using System;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;
using System.Linq;
using static Treasury.Web.Enum.Ref;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 初始畫面
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class TreasuryAccessController : CommonController
    {
        private ITreasuryAccess TreasuryAccess;

        public TreasuryAccessController()
        {
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/TreasuryAccess/");
            var data = TreasuryAccess.TreasuryAccessDetail(
                 AccountController.CurrentUserId, AccountController.CustodianFlag
                );
            //        var data = TreasuryAccess.TreasuryAccessDetail(
            // AccountController.CurrentUserId, true
            //);
            var _aProjectAll = data.Item1.ModelConvert<SelectOption, SelectOption>();
            var _aUnitAll = data.Item2.ModelConvert<SelectOption, SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };
            _aProjectAll.Insert(0, All);
            _aUnitAll.Insert(0, All);
            ViewBag.aProject = new SelectList(data.Item1, "Value", "Text");
            ViewBag.aUnit = new SelectList(data.Item2, "Value", "Text");
            ViewBag.applicant = new SelectList(data.Item3, "Value", "Text");
            ViewBag.aProjectAll = new SelectList(_aProjectAll, "Value", "Text");
            ViewBag.aUnitAll = new SelectList(_aUnitAll, "Value", "Text");
            ViewBag.lCREATE_User = data.Item4;
            ViewBag.lCREATE_Dep = data.Item5;
            return View();
        }

        /// <summary>
        /// 改變申請單位時,變動申請人
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChangeUnit(string DPT_CD)
        {
            return Json(TreasuryAccess.ChangeUnit(DPT_CD));
        }

        [HttpPost]
        public JsonResult Search(TreasuryAccessSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryAccessSearchData);
            Cache.Set(CacheList.TreasuryAccessSearchData, searchModel);
            searchModel.vCreateUid = AccountController.CurrentUserId;
            var datas = TreasuryAccess.GetSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryAccessSearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }

            return Json(result);
        }

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Cancel(string AplyNo)
        {
            MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.already_Change.GetDescription();
            var searchData = (TreasuryAccessSearchViewModel)Cache.Get(CacheList.TreasuryAccessSearchData);
            var datas = (List<TreasuryAccessSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessSearchDetailViewData);
            var data = datas.FirstOrDefault(x => x.vAPLY_NO == AplyNo);
            if (data != null)
            {
                result = TreasuryAccess.Cancel(searchData, data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessSearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Invalidate(string AplyNo)
        {
            MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.already_Change.GetDescription();
            var searchData = (TreasuryAccessSearchViewModel)Cache.Get(CacheList.TreasuryAccessSearchData);
            var datas = (List<TreasuryAccessSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessSearchDetailViewData);
            var data = datas.FirstOrDefault(x => x.vAPLY_NO == AplyNo);
            if (data != null)
            {
                result = TreasuryAccess.Invalidate(searchData, data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessSearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 使用單號抓取基本資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetByAplyNo(string AplyNo)
        {
            MSGReturnModel<TreasuryAccessViewModel> result = new MSGReturnModel<TreasuryAccessViewModel>();
            result.RETURN_FLAG = false;
            if (!AplyNo.IsNullOrWhiteSpace())
            {
                result.RETURN_FLAG = true;
                result.Datas  = TreasuryAccess.GetByAplyNo(AplyNo);
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
            if (Cache.IsSet(CacheList.TreasuryAccessSearchDetailViewData))
            {
                var datas = (List<TreasuryAccessSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessSearchDetailViewData);
                var AccessStatus = new List<string>() {
                    AccessProjectFormStatus.A01.ToString(),
                    AccessProjectFormStatus.A03.ToString(),
                    AccessProjectFormStatus.A04.ToString()
                 };
                switch (type)
                {
                    case "Access":
                            return Json(jdata.modelToJqgridResult(datas.Where(x=> AccessStatus.Contains(x.vAPLY_STATUS)).ToList()));
                    case "Report":
                            return Json(jdata.modelToJqgridResult(datas.Where(x => !AccessStatus.Contains(x.vAPLY_STATUS)).ToList()));
                }
            }
            return null;
        }
    }
}