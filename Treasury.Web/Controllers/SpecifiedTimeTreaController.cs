using Treasury.WebActionFilter;
using System;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.ViewModels;
using System.Linq;
using Treasury.Web.Enum;
using Treasury.WebControllers;

/// <summary>
/// 功能說明：金庫進出管理作業-指定時間開庫作業
/// 初版作者：20180821 李彥賢
/// 修改歷程：20180821 李彥賢
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
	[Authorize]
	[CheckSessionFilterAttribute]
	public class SpecifiedTimeTreaController : CommonController
    {
		private ISpecifiedTimeTreasury SpecifiedTimeTreasury;
		internal List<string> Apply_Status { get; set; }

		public SpecifiedTimeTreaController()
		{
			SpecifiedTimeTreasury = new SpecifiedTimeTreasury();
			Apply_Status = new List<string>()
			{
				((int)Ref.ApplyStatus._1).ToString(),
				((int)Ref.ApplyStatus._2).ToString(),
				((int)Ref.ApplyStatus._3).ToString(),
				((int)Ref.ApplyStatus._4).ToString()
			};
		}

		// GET: specifiedTimeTreasury
		/// <summary>
		/// 畫面初始
		/// </summary>
		/// <returns></returns>	
		public ActionResult Index()
        {
			SpecifiedTimeTreasurySearchViewModel searchModel = new SpecifiedTimeTreasurySearchViewModel();
			var dt = DateTime.Today;
			searchModel.vAPLY_DT_S = dt.ToString("yyyy/MM/dd");
			searchModel.vAPLY_DT_E = dt.ToString("yyyy/MM/dd");
			Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchData);
			Cache.Set(CacheList.SpecifiedTimeTreasurySearchData, searchModel);
			ViewBag.opScope = GetopScope("~/SpecifiedTimeTrea/");
			var data = SpecifiedTimeTreasury.GetTreaItem();
			//作業類型1
			ViewBag.workType1 = Extension.CheckBoxString("WorkType1", data.Item1, null, 1).Replace('\'','\"');
			//作業類型2
			ViewBag.workType2 = Extension.CheckBoxString("WorkType2", data.Item2, null, 1).Replace('\'', '\"');
			//作業類型3
			ViewBag.workType3 = Extension.CheckBoxString("WorkType3", data.Item3, null, 1).Replace('\'', '\"');
			//作業類型4
			ViewBag.workType4 = Extension.CheckBoxString("WorkType4", data.Item4, null, 1).Replace('\'', '\"');
			//內文編號(EmailId)
			ViewBag.emailId = data.Item5;
			//目前使用者
			ViewBag.cUserId = AccountController.CurrentUserId;
			return View();
        }

		/// <summary>
		/// 畫面查詢
		/// </summary>
		/// <param name="searchModel"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult Search(SpecifiedTimeTreasurySearchViewModel searchModel)
		{
			MSGReturnModel<string> result = new MSGReturnModel<string>();
			result.RETURN_FLAG = false;
			result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
			Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchData);
			Cache.Set(CacheList.SpecifiedTimeTreasurySearchData, searchModel);
			var datas = SpecifiedTimeTreasury.GetSearchDetail(searchModel);
			if (datas.Any())
			{
				Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
				Cache.Set(CacheList.SpecifiedTimeTreasurySearchDetailViewData, datas);
				result.RETURN_FLAG = true;
			}
			return Json(result);
		}

		/// <summary>
		/// jqgrid cache data
		/// </summary>
		/// <param name="jdata"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public JsonResult GetCacheData(jqGridParam jdata, string type)
		{
			switch (type)
			{
				case "Search":
					var SearchDatas = (List<SpecifiedTimeTreasurySearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
					return Json(jdata.modelToJqgridResult(SearchDatas.Where(x => x.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._1).ToString() || x.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._3).ToString()).ToList()));
				case "Status":
					var StatusDatas = (List<SpecifiedTimeTreasurySearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
					return Json(jdata.modelToJqgridResult(StatusDatas.Where(x => x.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._2).ToString() || x.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._4).ToString()).ToList()));
			}
			return null;
		}

		/// <summary>
		/// 申請覆核
		/// </summary>
		/// <param name="applyModel"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult InsertApply(SpecifiedTimeTreasuryApplyViewModel applyModel)
		{
			MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
			result.RETURN_FLAG = false;
			result.DESCRIPTION = Ref.MessageType.Apply_Audit_Fail.GetDescription();
			var searchData = (SpecifiedTimeTreasurySearchViewModel)Cache.Get(CacheList.SpecifiedTimeTreasurySearchData);
			//var datas = (List<SpecifiedTimeTreasurySearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
		    result = SpecifiedTimeTreasury.InsertApplyData(applyModel, AccountController.CurrentUserId, searchData);
			if (result.RETURN_FLAG)
			{
				Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
				Cache.Set(CacheList.SpecifiedTimeTreasurySearchDetailViewData, result.Datas);
			}
			return Json(result);
		}

		/// <summary>
		/// 修改申請
		/// </summary>
		/// <param name="UpdateModel"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult UpdateApply(SpecifiedTimeTreasuryUpdateViewModel UpdateModel)
		{
			MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
			result.RETURN_FLAG = false;
			result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
			var searchData = (SpecifiedTimeTreasurySearchViewModel)Cache.Get(CacheList.SpecifiedTimeTreasurySearchData);
			result = result = SpecifiedTimeTreasury.UpdateApplyData(UpdateModel, AccountController.CurrentUserId, searchData);
			if (result.RETURN_FLAG)
			{
				Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
				Cache.Set(CacheList.SpecifiedTimeTreasurySearchDetailViewData, result.Datas);
			}
			return Json(result);
		}

		/// <summary>
		/// 取消申請
		/// </summary>
		/// <param name="CancelApply"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult CancelApply(SpecifiedTimeTreasuryCancelViewModel CancelModel)
		{
			MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
			result.RETURN_FLAG = false;
			result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
			var searchData = (SpecifiedTimeTreasurySearchViewModel)Cache.Get(CacheList.SpecifiedTimeTreasurySearchData);
			result = SpecifiedTimeTreasury.CancelApplyData(CancelModel, AccountController.CurrentUserId, searchData);
			if (result.RETURN_FLAG)
			{
				Cache.Invalidate(CacheList.SpecifiedTimeTreasurySearchDetailViewData);
				Cache.Set(CacheList.SpecifiedTimeTreasurySearchDetailViewData, result.Datas);
			}
			return Json(result);
		}
	}
}