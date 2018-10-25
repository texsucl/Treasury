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

            Dictionary<string, object> attr =
            new Dictionary<string, object>();
            attr.Add("class", "forclick");
            //作業類型1
            ViewBag.workType1 = Extension.CheckBoxString("WorkType1", data.Item1, attr, 1).Replace('\'','\"');
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
        /// 覆核起始畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Appr()
        {
            ViewBag.opScope = GetopScope("~/SpecifiedTimeTrea/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            var data = SpecifiedTimeTreasury.GetTreaItem();
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            //內文編號
            ViewBag.emailId = data.Item5;
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
                case "ApprSearch":
                    var ApprSearchDatas = (List<SpecifiedTimeTreasuryApprSearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(ApprSearchDatas));
                case "ApprReason":
                    var ApprReasonDatas = (List<SpecifiedTimeTreasuryApprReasonDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasuryApprReasonDetailViewData);
                    return Json(jdata.modelToJqgridResult(ApprReasonDatas));
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

        /// <summary>
        /// 覆核畫面查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchAppr(SpecifiedTimeTreasuryApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.SpecifiedTimeTreasuryApprSearchData);
            Cache.Set(CacheList.SpecifiedTimeTreasuryApprSearchData, searchModel);
            var datas = SpecifiedTimeTreasury.GetApprSearchData(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                Cache.Set(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="ApprModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Approve(List<string> ApprModel)
        {
            MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> result = new MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (ApprModel.Any() && Cache.IsSet(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData))
            {
                var datas = (List<SpecifiedTimeTreasuryApprSearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                var searchData = (SpecifiedTimeTreasuryApprSearchViewModel)Cache.Get(CacheList.SpecifiedTimeTreasuryApprSearchData);
                result = SpecifiedTimeTreasury.ApproveData(ApprModel, datas, searchData);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                    Cache.Set(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="RejectModel"></param>
        /// <param name="ApprDesc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(List<string> RejectModel, string ApprDesc)
        {
            MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> result = new MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if(RejectModel.Any() && Cache.IsSet(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData))
            {
                var datas = (List<SpecifiedTimeTreasuryApprSearchDetailViewModel>)Cache.Get(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                var searchData = (SpecifiedTimeTreasuryApprSearchViewModel)Cache.Get(CacheList.SpecifiedTimeTreasuryApprSearchData);
                result = SpecifiedTimeTreasury.RejectData(RejectModel, ApprDesc, datas, searchData);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData);
                    Cache.Set(CacheList.SpecifiedTimeTreasuryApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 檢查開庫紀錄檔是否有狀態不為E01的單號
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckRegisterId()
        {
            MSGReturnModel<List<string>> result = new MSGReturnModel<List<string>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = SpecifiedTimeTreasury.CheckRegisterId();
            if(datas.Any())
            {
                //有單號 不繼續作業
                result.RETURN_FLAG = false;
                result.DESCRIPTION = string.Format("尚有金庫登記簿未申請覆核，請儘速完成。\r\n金庫登記簿單號:{0}", string.Join(",", datas));
            }
            else
            {
                //無單號 繼續作業
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢工作項目
        /// </summary>
        /// <param name="ReasonModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReason(List<string> ReasonModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = SpecifiedTimeTreasury.GetReasonDetail(ReasonModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.SpecifiedTimeTreasuryApprReasonDetailViewData);
                Cache.Set(CacheList.SpecifiedTimeTreasuryApprReasonDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }
    }
}