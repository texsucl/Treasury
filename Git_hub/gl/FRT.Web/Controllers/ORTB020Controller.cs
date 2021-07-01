using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款銀行出款匯總表補印功能
/// 初版作者：20210421 Mark
/// 修改歷程：20210421 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB020Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        public ORTB020Controller()
        {
            Cache = new DefaultCacheProvider();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB020/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            return View();
        }
    }
}