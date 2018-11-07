using Treasury.WebActionFilter;
using System;
using Treasury.Web.Properties;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;
using System.Linq;
using Treasury.Web.Enum;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 初始畫面
/// 初版作者：20181101 張家華
/// 修改歷程：20181101 張家華 
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
    public class TreasuryAccessCustodyController : CommonController
    {
        protected ITreasuryAccessCustody TreasuryAccessCustody;

        public TreasuryAccessCustodyController()
        {
            TreasuryAccessCustody = new TreasuryAccessCustody();
        }

        /// <summary>
        /// 申請作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryAccessCustody/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            return View();
        }

        /// <summary>
        /// 覆核作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Appr()
        {
            ViewBag.opScope = GetopScope("~/TreasuryAccessCustody/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;                                                   
            return View();
        }

        /// <summary>
        /// 查詢畫面查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(TreasuryAccessApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryAccessCustodySearchData);
            Cache.Set(CacheList.TreasuryAccessCustodySearchData, searchModel);
            var datas = TreasuryAccessCustody.GetCustodySearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryAccessCustodySearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessCustodySearchDetailViewData, datas);
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
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryAccessCustodyApprSearchData);
            Cache.Set(CacheList.TreasuryAccessCustodyApprSearchData, searchModel);
            var datas = TreasuryAccessCustody.GetCustodyApprSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessCustodyApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 保管單位承辦作業-覆核
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <param name="apprDesc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CustodyAppraisal(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.TreasuryAccessCustodySearchDetailViewData))
            {
                var datas = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodySearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAPLY_NO)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TreasuryAccessApprSearchViewModel)Cache.Get(CacheList.TreasuryAccessCustodySearchData);
                result = TreasuryAccessCustody.CustodyApproved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessCustodySearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessCustodySearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }


        /// <summary>
        /// 保管單位承辦作業-駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <param name="apprDesc"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CustodyReject(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.TreasuryAccessCustodySearchDetailViewData))
            {
                var datas = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodySearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAPLY_NO)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TreasuryAccessApprSearchViewModel)Cache.Get(CacheList.TreasuryAccessCustodySearchData);
                result = TreasuryAccessCustody.CustodyReject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessCustodySearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessCustodySearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        ///  保管單位覆核作業-覆核
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Appraisal(List<string> AplyNos)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.TreasuryAccessCustodyApprSearchDetailViewData))
            {
                var datas =  (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAPLY_NO)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TreasuryAccessApprSearchViewModel)Cache.Get(CacheList.TreasuryAccessCustodyApprSearchData);
                result = TreasuryAccessCustody.Approved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessCustodyApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 保管單位覆核作業-駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(List<string> AplyNos,string apprDesc)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.TreasuryAccessCustodyApprSearchDetailViewData))
            {
                var datas = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAPLY_NO)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TreasuryAccessApprSearchViewModel)Cache.Get(CacheList.TreasuryAccessCustodyApprSearchData);
                result = TreasuryAccessCustody.Reject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                    Cache.Set(CacheList.TreasuryAccessCustodyApprSearchDetailViewData, result.Datas);
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
            MSGReturnModel<Tuple<TreasuryAccessViewModel, bool, List<SelectOption>, List<SelectOption> , bool>> result =
                new MSGReturnModel<Tuple<TreasuryAccessViewModel, bool, List<SelectOption>, List<SelectOption> , bool>>();
            result.RETURN_FLAG = false;
            if (!AplyNo.IsNullOrWhiteSpace())
            {
                result.RETURN_FLAG = true;
                var _dActType = GetActType(Ref.OpenPartialViewType.CustodyAppr, AplyNo);
                var data = TreasuryAccess.GetByAplyNo(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessSearchUpdateViewData);
                Cache.Set(CacheList.TreasuryAccessSearchUpdateViewData, data);
                var depts = ((TreasuryAccessCustody)TreasuryAccessCustody).GetDepts();
                List<SelectOption> selectOptionsUnit = new List<SelectOption>() { new SelectOption() { Value = data.vAplyUnit, Text = depts.FirstOrDefault(x => x.DPT_CD?.Trim() == data.vAplyUnit)?.DPT_NAME?.Trim() } };
                List<SelectOption> selectOptionsUid = TreasuryAccess.ChangeUnit(data.vAplyUnit);
                result.Datas  = new Tuple<TreasuryAccessViewModel,bool, List<SelectOption>, List<SelectOption> , bool>(data, AccountController.CustodianFlag, selectOptionsUnit, selectOptionsUid, _dActType);
            }
            return Json(result);
        }

        /// <summary>
        /// 更新申請單記錄檔
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateAplyNo(TreasuryAccessViewModel data)
        {
            var cdata = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessSearchUpdateViewData);
            cdata.vAccessReason = data.vAccessReason;
            cdata.vExpectedAccessDate = data.vExpectedAccessDate;
            var searchData = (TreasuryAccessApprSearchViewModel)Cache.Get(CacheList.TreasuryAccessCustodySearchData);
            var result = TreasuryAccessCustody.updateAplyNo(cdata,AccountController.CustodianFlag, searchData,AccountController.CurrentUserId);
            if (result.RETURN_FLAG)
            {
                var data1 = TreasuryAccess.GetByAplyNo(cdata.vAplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessSearchUpdateViewData);
                Cache.Set(CacheList.TreasuryAccessSearchUpdateViewData, data1);
                var data2 = TreasuryAccess.GetTreasuryAccessViewModel(cdata.vAplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data2);
                Cache.Invalidate(CacheList.TreasuryAccessCustodySearchDetailViewData);
                Cache.Set(CacheList.TreasuryAccessCustodySearchDetailViewData, result.Datas);                
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
               case "search":
                   var searchCache = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodySearchDetailViewData);
                   return Json(jdata.modelToJqgridResult(searchCache));
               case "Appr":
                   var apprCache = (List<TreasuryAccessApprSearchDetailViewModel>)Cache.Get(CacheList.TreasuryAccessCustodyApprSearchDetailViewData);
                   return Json(jdata.modelToJqgridResult(apprCache));
           }           
           return null;
        }
    }
}