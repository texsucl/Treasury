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
    public class TreaDefinitionApprController : CommonController
    {
        private ITreaDefinitionAppr TreaDefinitionAppr;

        public TreaDefinitionApprController()
        {
            TreaDefinitionAppr = new TreaDefinitionAppr();
        }
        // GET: TreaDefinition_Appr
        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreaDefinitionAppr/");                           
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);      


            List<SelectOption> enums = ((Ref.DefinitionType[])System.Enum.GetValues(typeof(Ref.DefinitionType))).Select(c => new SelectOption() { Value = c.ToString(), Text = c.GetDescription() }).ToList();
            var All = new SelectOption() { Text = "All", Value = "All" };
            enums.Insert(0, All);
            ViewBag.dOption = new SelectList(enums, "Value", "Text");
            ViewBag.hCREATE_User = userInfo.EMP_ID;                                          
            return View();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Search()
        {
            ViewBag.opScope = GetopScope("~/TreaDefinitionAppr/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);

            List<SelectOption> enums = ((Ref.DefinitionType[])System.Enum.GetValues(typeof(Ref.DefinitionType))).Select(c => new SelectOption() { Value = c.ToString(), Text = c.GetDescription() }).ToList();
            var All = new SelectOption() { Text = "All", Value = "All" };
            enums.Insert(0, All);
            ViewBag.dOption = new SelectList(enums, "Value", "Text");
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchAppr(TDAApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TDAApprSearchData);
            Cache.Set(CacheList.TDAApprSearchData, searchModel);
            var datas = TreaDefinitionAppr.GetApprSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TDAApprSearchDetailViewData);
                Cache.Set(CacheList.TDAApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }

            return Json(result);
        }
        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="Insert_Data"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Appraisal(List<string> AplyNos)
        {
            MSGReturnModel<List<TDAApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<TDAApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            if (AplyNos.Any() && Cache.IsSet(CacheList.TDAApprSearchDetailViewData))
            {
                var datas = (List<TDAApprSearchDetailViewModel>)Cache.Get(CacheList.TDAApprSearchDetailViewData);

                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TDAApprSearchViewModel)Cache.Get(CacheList.TDAApprSearchData);
                result = TreaDefinitionAppr.Approved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TDAApprSearchDetailViewData);
                    Cache.Set(CacheList.TDAApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <param name="apprDesc"></param>
        /// <returns></returns>
        public JsonResult Reject(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<TDAApprSearchDetailViewModel>> result =
                 new MSGReturnModel<List<TDAApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.TDAApprSearchDetailViewData))
            {
                var datas = (List<TDAApprSearchDetailViewModel>)Cache.Get(CacheList.TDAApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (TDAApprSearchViewModel)Cache.Get(CacheList.TDAApprSearchData);
                result = TreaDefinitionAppr.Reject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TDAApprSearchDetailViewData);
                    Cache.Set(CacheList.TDAApprSearchDetailViewData, result.Datas);
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
                    var ApprDatas = (List<TDAApprSearchDetailViewModel>)Cache.Get(CacheList.TDAApprSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(ApprDatas));
                case "Search":
                    var Datas = (List<TDAApprSearchDetailViewModel>)Cache.Get(CacheList.TDASearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(Datas));
            }
            return null;
        }

        [HttpPost]
        public JsonResult Search(TDAApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            var datas = TreaDefinitionAppr.GetSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TDASearchDetailViewData);
                Cache.Set(CacheList.TDASearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }
    }
}