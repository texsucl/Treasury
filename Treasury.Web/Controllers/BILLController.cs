using Treasury.WebActionFilter;
using System;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;

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
    public class BILLController : CommonController
    {


        public BILLController()
        {
      
        }

        /// <summary>
        /// 空白票據 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[ChildActionOnly]
        public ActionResult InsertView()
        {
            return PartialView();
        }
    }
}