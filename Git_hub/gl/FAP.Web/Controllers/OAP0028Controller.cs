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
using FAP.Web.ActionFilter;

/// <summary>
/// 功能說明：OAP0028 應付票據票明細表(For財務部抽票使用)
/// 初版作者：20200117 張家華
/// 修改歷程：20200117 張家華 
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
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0028Controller : CommonController
    {
        private IOAP0028 OAP0028;

        public OAP0028Controller()
        {
            OAP0028 = new OAP0028();
        }

        // GET: OAP0028
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0028/");
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

        /// <summary>
        /// 執行明細表產生
        /// </summary>
        /// <param name="entry_date"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execDetail(string entry_date = null)
        {
            MSGReturnModel result = new MSGReturnModel();
            var _result = OAP0028.changStatus(AccountController.CurrentUserId, entry_date);
            result.RETURN_FLAG = _result.Item1;
            result.DESCRIPTION = _result.Item2;
            return Json(result);
        }
    }
}