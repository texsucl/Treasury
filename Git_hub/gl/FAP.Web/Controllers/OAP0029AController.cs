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
/// 功能說明：OAP0029A 應付票據抽票結果確認功能
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
    public class OAP0029AController : CommonController
    {
        private IOAP0029A OAP0029A;

        public OAP0029AController()
        {
            OAP0029A = new OAP0029A();
        }

        // GET: OAP0029A
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0029A/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope; 
            ViewBag.funcName = funcName;
            ViewBag.sendStyle_A = new SelectList(new Service.Actual.Common().GetSysCode("AP", "SEND_STYLE", true), "Value", "Text");
            ViewBag.resultStatus_A = new SelectList(new Service.Actual.Common().GetSysCode("AP", "CE_RESULT", true), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0029A(OAP0029SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0029ASearchData);
            Cache.Set(CacheList.OAP0029ASearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0029A(searchModel);
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
        /// 核可
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApprovedData()
        {
            var OAP0029AViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029AViewData);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                result = OAP0029A.ApprovedData(OAP0029AViewData.Where(x => x.Ischecked), AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    try
                    {
                        foreach (var item in OAP0029AViewData.Where(x => x.Ischecked && x.ce_result == "Y").GroupBy(x => x.apply_id))
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
                               "應付票據抽票完成通知 (成功)",
                               $@"申請單號：{string.Join(",", item.Select(y => y.apply_no).OrderBy(x => x))}，應付票據已完成抽票。",
                               false,
                               null
                               );
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    try
                    {
                        foreach (var item in OAP0029AViewData.Where(x => x.Ischecked && x.ce_result == "R").GroupBy(x => x.apply_id))
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
                               "應付票據抽票失敗通知 (失敗)",
                               $@"申請單號：{string.Join(",", item.Select(y => y.apply_no).OrderBy(x => x))}，抽票作業失敗，原因請至AS400系統PAP8304應付票據抽票進度查詢。",
                               false,
                               null
                               );
                        }
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    searchOAP0029A();
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
        /// 駁回
        /// </summary>
        /// <returns></returns>
        public JsonResult RejectedData()
        {
            var OAP0029AViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029AViewData);
            var result = OAP0029A.RejectedData(OAP0029AViewData.Where(x => x.Ischecked && x.review_flag), AccountController.CurrentUserId);
            if (result.RETURN_FLAG)
            {
                searchOAP0029A();
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
            var ViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029AViewData);
            if (apply_no == "")
            {
                result.RETURN_FLAG = true;
                result.Datas = ViewData.Any(x => x.Ischecked);
                return Json(result);
            }
            var _ViewData = ViewData.FirstOrDefault(x => x.apply_no == apply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.Ischecked = flag;
                result.Datas = ViewData.Any(x => x.Ischecked);
                Cache.Invalidate(CacheList.OAP0029AViewData);
                Cache.Set(CacheList.OAP0029AViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        private bool searchOAP0029A(OAP0029SearchModel searchModel = null)
        {
            List<OAP0029ViewModel> result = new List<OAP0029ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0029SearchModel)Cache.Get(CacheList.OAP0029ASearchData);
            if (searchModel != null)
            {
                result = OAP0029A.GetSearchData(searchModel, AccountController.CurrentUserId);
                Cache.Invalidate(CacheList.OAP0029AViewData);
                Cache.Set(CacheList.OAP0029AViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0029AViewData":
                    var OAP0029AViewData = (List<OAP0029ViewModel>)Cache.Get(CacheList.OAP0029AViewData);
                    return Json(jdata.modelToJqgridResult(OAP0029AViewData));
            }
            return null;
        }
    }
}