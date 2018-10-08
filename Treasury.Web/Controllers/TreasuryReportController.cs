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
/// 功能說明：保管物品庫存表列印作業
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
    public class TreasuryReportController : CommonController
    {
        
        private ITreasuryReport TreasuryReport;
        private IDeposit Deposit;
        public TreasuryReportController()
        {
            Deposit = new Deposit();
            TreasuryReport = new TreasuryReport();
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
            ViewBag.opScope = GetopScope("~/TreasuryReport/");
            var viewModel = TreasuryReport.GetItemId();
            return View(viewModel);
        }

        /// <summary>
        /// 抓取權責部門,科別名稱
        /// </summary>
        /// <param name="type"></param>
        /// <param name="DEPT_ITEM">部門名稱</param>
        /// <returns></returns>
        public JsonResult GetCharge(Ref.TreaItemType  type , string DEPT_ITEM)
        {
            var result = new List<SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };       
            if (DEPT_ITEM.IsNullOrWhiteSpace())
            {
                result = TreasuryReport.getDEPT(type);
            }
            else
            {
               result =  TreasuryReport.getSECT(DEPT_ITEM,type);
            }
            result.Insert(0, All);
            return Json(result);
        }
        public JsonResult GetNAME( Ref.TreaItemType type,string DEPT_ITEM = null)
        {
             var result = new List<SelectOption>();
             var All = new SelectOption() { Text = "All", Value = "All" };         
             var deps = new Treasury.Web.Service.Actual.Common().GetDepts();
             if (!DEPT_ITEM.IsNullOrWhiteSpace())
             {
                 result = TreasuryReport.getSECT(DEPT_ITEM, type);
             }
             else
             {
                 result = TreasuryReport.getDEPT(type);
             }
             result.Insert(0, All);
            return Json(result);
        }

    }
}