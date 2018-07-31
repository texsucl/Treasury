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

        public TreasuryAccessController()
        {

        }

        /// <summary>
        /// 申請作業 畫面初始
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
            var userInfo = data.Item4;
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.lCREATE_User = userInfo.EMP_Name;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            ViewBag.lCREATE_Dep = userInfo.DPT_Name;
            return View();
        }

        /// <summary>
        /// 覆核作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Appr()
        {
            ViewBag.opScope = GetopScope("~/TreasuryAccess/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
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

        /// <summary>
        /// 查詢畫面查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(TreasuryAccessSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryAccessSearchData);
            Cache.Set(CacheList.TreasuryAccessSearchData, searchModel);
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
        /// 覆核作業查詢畫面
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public JsonResult SearchAppr(TreasuryAccessApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryAccessApprSearchData);
            Cache.Set(CacheList.TreasuryAccessApprSearchData, searchModel);
            var datas = TreasuryAccess.GetApprSearchDetail(searchModel);
            if (datas.Any())
            {               
                Cache.Invalidate(CacheList.TreasuryAccessApprSearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessApprSearchDetailViewData, datas);
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
        /// 覆核
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Appraisal(List<string> AplyNos)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(List<string> AplyNos)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

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
                
                switch (type)
                {
                    case "Access":
                        var AccessDatas = (List<TreasuryAccessSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessSearchDetailViewData);
                        return Json(jdata.modelToJqgridResult(AccessDatas.Where(x=> Aply_Appr_Type.Contains(x.vAPLY_STATUS)).ToList()));
                    case "Report":
                        var ReportDatas = (List<TreasuryAccessSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessSearchDetailViewData);
                        return Json(jdata.modelToJqgridResult(ReportDatas.Where(x => !Aply_Appr_Type.Contains(x.vAPLY_STATUS)).ToList()));
                    case "Appr":
                        var ApprDatas = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessApprSearchDetailViewData);
                        return Json(jdata.modelToJqgridResult(ApprDatas));
                }
            }
            return null;
        }

        public void ResetSearchData()
        {
            var searchData = (TreasuryAccessSearchViewModel)Cache.Get(CacheList.TreasuryAccessSearchData);
            Cache.Set(CacheList.TreasuryAccessSearchData, searchData);
            var datas = TreasuryAccess.GetSearchDetail(searchData);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryAccessSearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessSearchDetailViewData, datas);
            }
        }
    }
}