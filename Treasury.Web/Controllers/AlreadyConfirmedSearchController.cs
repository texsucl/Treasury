using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Enum;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;

namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class AlreadyConfirmedSearchController : CommonController
    {
        private IAlreadyConfirmedSearch AlreadyConfirmedSearch;
        public AlreadyConfirmedSearchController()
        {
            AlreadyConfirmedSearch = new AlreadyConfirmedSearch();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/AfterOpenTreasury/");
            ViewBag.IsCustodian = AccountController.CustodianFlag;
            var data = AlreadyConfirmedSearch.GetFirstTimeData();
            var All = new SelectOption() { Text = "全選", Value = " " };
            data.Item3.Insert(0, All);
            data.Item2.Insert(0, All);
            ViewBag.IsConfirmed = new SelectList(data.Item1, "Value", "Text");
            ViewBag.OPEN_TREA_TYPE = new SelectList(data.Item2, "Value", "Text");
            ViewBag.Confirm_Id = new SelectList(data.Item3, "Value", "Text");
            return View();
        }

        public JsonResult SearchData(AlreadyConfirmedSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.AlreadyConfirmedSearchData);
            Cache.Set(CacheList.AlreadyConfirmedSearchData, searchModel);
            var datas = AlreadyConfirmedSearch.GetSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.AlreadyConfirmedSearchDetailViewData);
                Cache.Set(CacheList.AlreadyConfirmedSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
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
                    var SearchDatas = (List<AlreadyConfirmedSearchDetailViewModel>)Cache.Get(CacheList.AlreadyConfirmedSearchDetailViewData);
                    return Json(jdta.modelToJqgridResult(SearchDatas.OrderBy(x => x.vTREA_REGISTER_ID).ToList()));
            }
            return null;
        }
    }
}