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
/// 功能說明：金庫進出管理作業-定存檢核表項目設定
/// 初版作者：20181106 侯蔚鑫
/// 修改歷程：20181106 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class DepChkItemController : CommonController
    {
        // GET: DepChkItem
        private IDepChkItem DepChkItem;

        public DepChkItemController()
        {
            DepChkItem = new DepChkItem();
        }
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryMaintain/");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", true), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 定存檢核表項目查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(DepChkItemSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.DepChkItemSearchData);
            Cache.Set(CacheList.DepChkItemSearchData, searchModel);

            var datas = (List<DepChkItemViewModel>)DepChkItem.GetSearchData(searchModel);
            if (datas.Any())
            {                
                Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                Cache.Set(CacheList.DepChkItem_P_SearchDataList, datas.Where(x => x.vAccess_Type == "P").ToList());
                Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                Cache.Set(CacheList.DepChkItem_G_SearchDataList, datas.Where(x => x.vAccess_Type == "G").ToList());
                result.RETURN_FLAG = true;
            }

            return Json(result);
        }

    }
}