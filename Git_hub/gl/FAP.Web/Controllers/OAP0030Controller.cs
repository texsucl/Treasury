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
using System.Configuration;

/// <summary>
/// 功能說明：OAP0030 用印檢視確認功能
/// 初版作者：20200320 張家華
/// 修改歷程：20200320 張家華 
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
    public class OAP0030Controller : CommonController
    {
        private IOAP0030 OAP0030;

        public OAP0030Controller()
        {
            OAP0030 = new OAP0030();
        }

        // GET: OAP0030
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0030/");
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
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0030(OAP0030SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0030SearchData);
            Cache.Set(CacheList.OAP0030SearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0030(searchModel);
                result.RETURN_FLAG = model;
                if (!model)
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }


        /// <summary>
        /// 勾選觸發事件
        /// </summary>
        /// <param name="apply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string apply_no, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0030ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == apply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                result.Datas = ViewData.Any(x => x.checkFlag);
                Cache.Invalidate(CacheList.OAP0030ViewData);
                Cache.Set(CacheList.OAP0030ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }


        /// <summary>
        /// 更新支票
        /// </summary>
        /// <param name="applyNo"></param>
        /// <param name="checkNo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateOAP0030()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0030ViewData);
                result = OAP0030.SetStatus(ViewData.Where(x => x.checkFlag), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    try
                    {
                        foreach (var item in ViewData.GroupBy(x => x.apply_id))
                        {
                            var remindList = new FAP.Web.Service.Actual.Common().GetMemoByUserId(new List<string>() { item.Key });
                            var _UATmailAccount = ConfigurationManager.AppSettings["UATmailAccount"] ?? string.Empty;
                            List<Tuple<string, string>> _mailTo = new List<Tuple<string, string>>() { };
                            if (_UATmailAccount == "Y")
                            {
                                _mailTo.Add(new Tuple<string, string>("Ex2016ap@fbt.com", "測試帳號-Ex2016ap"));
                            }
                            _mailTo.AddRange(remindList.Select(x => new Tuple<string, string>(x.Item5, x.Item2)));
                            var sms = new SendMail.SendMailSelf();
                            sms.smtpPort = 25;
                            sms.smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                            sms.mailAccount = ConfigurationManager.AppSettings["mailAccount"];
                            sms.mailPwd = ConfigurationManager.AppSettings["mailPwd"];
                            var msg = sms.Mail_Send(
                               new Tuple<string, string>(sms.mailAccount, "應付票據 MAIL 通知"),
                               _mailTo,
                               null,
                               "應付票據變更完成通知",
                               $@"申請單號：{string.Join(",", item.Select(y => y.apply_no).OrderBy(x => x))}，應付票據已完成變更。",
                               false,
                               null
                               );
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    searchOAP0030();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }


        /// <summary>
        /// 查詢OAP0030
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private bool searchOAP0030(OAP0030SearchModel searchModel = null)
        {
            List<OAP0022Model> result = new List<OAP0022Model>();
            if (searchModel == null)
                searchModel = (OAP0030SearchModel)Cache.Get(CacheList.OAP0030SearchData);
            if (searchModel != null)
            {
                result = OAP0030.GetSearchData(searchModel);
                Cache.Invalidate(CacheList.OAP0030ViewData);
                Cache.Set(CacheList.OAP0030ViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0030ViewData":
                    var OAP0030ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0030ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0030ViewData));
            }
            return null;
        }
    }
}