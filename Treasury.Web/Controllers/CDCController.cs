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
/// 功能說明：資料查詢異動作業
/// 初版作者：20180828 卓建毅
/// 修改歷程：2018 
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
    public class CDCController : CommonController
    {
        private ICDC CDC;
        private IEstate Estate;
        public CDCController()
        {
            CDC = new CDC();
            Estate = new Estate();
        }

        /// <summary>
        /// 資料查詢異動作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var All = new SelectOption() { Text = "All", Value = "All" };
            var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/CDC/");
            var viewModel = CDC.GetItemId();
            return View(viewModel);
        }

        /// <summary>
        /// 資料庫異動覆核作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Appr()
        {
            ViewBag.opScope = GetopScope("~/CDC/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchAppr(CDCApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.CDCApprSearchData);
            Cache.Set(CacheList.CDCApprSearchData, searchModel);
            var datas = CDC.GetApprSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                Cache.Set(CacheList.CDCApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
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
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCApprSearchData);
                result = CDC.Approved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCApprSearchData);
                result = CDC.Reject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCApprSearchDetailViewData, result.Datas);
                }
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
                case "Appr":
                    var ApprDatas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(ApprDatas));
            }
            return null;
        }

    }
}