using Treasury.WebActionFilter;
using System;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;

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
        private ITreasuryAccess TreasuryAccess;

        public TreasuryAccessController()
        {
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 畫面初始
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
            //var data = TreasuryAccess.TreasuryAccessDetail(
            //      AccountController.CurrentUserId, true
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

            return View();
        }

        [HttpPost]
        public JsonResult ChangeUnit(string DPT_CD)
        {
            return Json(TreasuryAccess.ChangeUnit(DPT_CD));
        }
    }
}