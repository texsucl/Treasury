using FRT.Web.ActionFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// 功能說明：退匯改給付方式原因Table檔-查詢
/// 初版作者：20201209 Bianco
/// 修改歷程：20201209 Bianco
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0103QController : BaseController
    {
        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("OnlyQry", "ORT0103", new { srce_from = "ORT0103Q"});
            //return View();
        }
    }
}