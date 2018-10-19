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
/// 功能說明：金庫鑰匙密碼保管清單列印作業
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
    public class TreasuryKeyCheckController : CommonController
    {
        private ITreasuryKeyCheck TreasuryKeyCheck;
        public TreasuryKeyCheckController()
        {
            TreasuryKeyCheck = new TreasuryKeyCheck();
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
            ViewBag.opScope = GetopScope("~/TreasuryKeyCheck/");
            var viewModel = TreasuryKeyCheck.GetItemId();
            return View(viewModel);
        }

    }
}