using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;
using Newtonsoft.Json;

/// <summary>
/// 功能說明：OAP0033 變據變更歸檔檢視報表
/// 初版作者：20200408 張家華
/// 修改歷程：20200408 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
namespace FAP.Web.Controllers
{
    public class OAP0033Controller : CommonController
    {
  
        public OAP0033Controller()
        {

        }
        // GET: OAP0033
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0033/");
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