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

/// <summary>
/// 功能說明：支票寄送清冊產出
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Controllers
{
    public class OAP0025Controller : CommonController
    {
        // GET: OAP0025
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0025/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.SRCE_FROM = new SysCodeDao().loadSelectList("AP", "SRCE_FROM", true, null, true);//資料來源        
            ViewBag.YN = new SelectList(
               items: new List<SelectOption>() {
                                       new SelectOption() { Value = "All",Text = "All"},
                                       new SelectOption() { Value = "Y",Text = "Y"},
                                       new SelectOption() { Value = "N",Text = "N"}
               },
               dataValueField: "Value",
               dataTextField: "Text"
            );
            return View();
        }
    }
}