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

/// <summary>
/// 功能說明：金庫進出管理作業-金庫登記簿查詢列印作業
/// 初版作者：20180917 侯蔚鑫
/// 修改歷程：20180917 侯蔚鑫 
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
    public class TreasuryRegisterSearchReportController : CommonController
    {
        // GET: TreasuryRegisterSearchReport
        private ITreasuryRegisterSearchReport TreasuryRegisterSearchReport;

        public TreasuryRegisterSearchReportController()
        {
            TreasuryRegisterSearchReport = new TreasuryRegisterSearchReport();
        }

        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryRegisterSearchReport/");
            ViewBag.vUser_Id = AccountController.CurrentUserId;
            return View();
        }

        /// <summary>
        /// 查詢金庫登記簿
        /// </summary>
        /// <param name="vCreate_Date_From">入庫日期(起)</param>
        /// <param name="vCreate_Date_To">入庫日期(迄)</param>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(string vCreate_Date_From, string vCreate_Date_To,string vTrea_Register_Id)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            Cache.Invalidate(CacheList.TreasuryRegisterSearchReportM);

            Cache.Set(CacheList.TreasuryRegisterSearchReportM, TreasuryRegisterSearchReport.GetSearchList(vCreate_Date_From, vCreate_Date_To, vTrea_Register_Id));

            result.RETURN_FLAG = true;

            return Json(result);
        }

        /// <summary>
        /// 金庫登記簿明細
        /// </summary>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DetailData(string vTrea_Register_Id)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            Cache.Invalidate(CacheList.TreasuryRegisterSearchReportD);

            Cache.Set(CacheList.TreasuryRegisterSearchReportD, TreasuryRegisterSearchReport.GetDetailList(vTrea_Register_Id));

            result.RETURN_FLAG = true;

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
                    if (Cache.IsSet(CacheList.TreasuryRegisterSearchReportM))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryRegisterSearch>)Cache.Get(CacheList.TreasuryRegisterSearchReportM)).OrderByDescending(x => x.vTrea_Register_Id).ToList()));
                    break;
                case "Detail":
                    if (Cache.IsSet(CacheList.TreasuryRegisterSearchReportD))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryRegisterDetail>)Cache.Get(CacheList.TreasuryRegisterSearchReportD)).OrderBy(x => x.vItem_Op_Type).ToList()));
                    break;
            }
            return null;
        }

    }
}