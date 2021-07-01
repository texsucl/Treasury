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
using FAP.Web.Models;
using System.Configuration;

/// <summary>
/// 功能說明：應付票據變更接收作業
/// 初版作者：20191114 Mark
/// 修改歷程：20191114 Mark
///           需求單號：201910030636
///           初版    
/// </summary>
/// 

namespace FAP.Web.Controllers
{
    public class OAP0021Controller : CommonController
    {
        private IOAP0021 OAP0021;
        public OAP0021Controller()
        {
            OAP0021 = new OAP0021();
        }
        // GET: OAP0021
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0021/");
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
        /// 變更檔明細
        /// </summary>
        /// <param name="apply_no"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public ActionResult Detail(string apply_no , string action)
        {
            var detail = OAP0021.GetDetailData(apply_no, AccountController.CurrentUserId);
            var data = detail.Datas;
            try
            {
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                piaLogMain.TRACKING_TYPE = "A";
                piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                piaLogMain.ACCOUNT_NAME = "";
                piaLogMain.PROGFUN_NAME = "OAP0021Controller";
                piaLogMain.EXECUTION_CONTENT = $@"apply_no:{apply_no}|action:{action}";
                piaLogMain.AFFECT_ROWS = 1;
                piaLogMain.PIA_TYPE = "0100000000";
                piaLogMain.EXECUTION_TYPE = "Q";
                piaLogMain.ACCESSOBJ_NAME = "FAPPYCH";
                piaLogMainDao.Insert(piaLogMain);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }

            Cache.Invalidate(CacheList.OAP0021DetailData);
            Cache.Set(CacheList.OAP0021DetailData, data);
            Cache.Invalidate(CacheList.OAP0021SubData);
            Cache.Set(CacheList.OAP0021SubData, data.check_nos);
            SysCodeDao sysCodeDao = new SysCodeDao();
            ViewBag.MARK_TYPE = sysCodeDao.loadSelectList("AP", "MARK_TYPE", true); //註記
            ViewBag.MARK_RSN = sysCodeDao.loadSelectList("AP", "MARK_RSN", true, data.mark_rsn2); //變更原因
            ViewBag.MARK_MTH = sysCodeDao.loadSelectList("AP", "MARK_MTH", true, data.mark_mth2); //申辦方式
            ViewBag.YN = new SelectList(
                items: new List<SelectOption>() {
                    new SelectOption() { Value = " ",Text = "請選擇"},
                    new SelectOption() { Value = "Y",Text = "Y"},
                    new SelectOption() { Value = "N",Text = "N"}
                },
                dataValueField: "Value",
                dataTextField: "Text",
                selectedValue: (object)data.reg_yn
            );

            var _Q33 = string.Empty;
            try
            {
                _Q33 = GetLRTCODE("BKMSG_CKQ", "Q33");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            ViewBag.Q33 = _Q33;
            ViewBag.self = (data.rece_id == AccountController.CurrentUserId);
            ViewBag.action = action;

            return PartialView(data);
        }

        /// <summary>
        /// 查詢 應付票據變更接收作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0021(OAP0021SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0021SearchData);
            Cache.Set(CacheList.OAP0021SearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0021(searchModel);
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
        /// 查詢業務人員資料 id , 姓名 , unit_id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult changeSend_Id(string id)
        {
            MSGReturnModel<string> _result = new MSGReturnModel<string>();
            try
            {
                var result = OAP0021.callEBXGXFK(id);
                if (!result.Item3.IsNullOrWhiteSpace())
                {
                    var OAP0021DetailData = (OAP0021DetailModel)Cache.Get(CacheList.OAP0021DetailData);
                    OAP0021DetailData.send_unit = result.Item3;
                    Cache.Invalidate(CacheList.OAP0021DetailData);
                    Cache.Set(CacheList.OAP0021DetailData, OAP0021DetailData);
                }
                _result.Datas = result.Item2;
                _result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                _result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(_result);
        }

        /// <summary>
        /// 檢查付款對象
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult checkName()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var models = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                if (models.Select(x => x.receiver).Distinct().Count() > 1)
                {
                    result.DESCRIPTION = GetLRTCODE("BKMSG_CKQ", "Q35");
                }
                else
                {
                    result.RETURN_FLAG = true;
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = string.Empty;
                //訊息由前端提供
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 接收變更檔
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OAP0021RECE(OAP0021DetailModel detailModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var OAP0021DetailData = (OAP0021DetailModel)Cache.Get(CacheList.OAP0021DetailData);
                OAP0021DetailData.rcv_name = detailModel.rcv_name;
                OAP0021DetailData.zip_code = detailModel.zip_code;
                OAP0021DetailData.addr = detailModel.addr;
                OAP0021DetailData.tel = detailModel.tel;
                OAP0021DetailData.mark_rsn2 = detailModel.mark_rsn2;
                OAP0021DetailData.mark_mth2 = detailModel.mark_mth2;
                OAP0021DetailData.send_id = detailModel.send_id;
                OAP0021DetailData.reg_yn = detailModel.reg_yn;
                OAP0021DetailData.check_nos = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                if (!OAP0021DetailData.check_nos.Any())
                {
                    return Json(new MSGReturnModel()
                    {
                        RETURN_FLAG = false,
                        DESCRIPTION = "至少要登錄一筆支票明細!"
                    });
                }
                result = OAP0021.RECEFAPPYCH0(OAP0021DetailData, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                    searchOAP0021();
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 退回變更檔
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OAP0021REJ(OAP0021DetailModel detailModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var OAP0021DetailData = (OAP0021DetailModel)Cache.Get(CacheList.OAP0021DetailData);
                OAP0021DetailData.rej_rsn = detailModel.rej_rsn;
                OAP0021DetailData.check_flag = detailModel.check_flag;
                OAP0021DetailData.check_nos = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                result = OAP0021.REJFAPPYCH0(OAP0021DetailData, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    try
                    {
                        var remindList = new FAP.Web.Service.Actual.Common().GetMemoByUserId(new List<string>() { OAP0021DetailData.apply_id });
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
                           "應付票據變更退件通知",
                           $@"申請單號：{OAP0021DetailData.apply_no}，不符合申請要件故予以退件。",
                           false,
                           null
                           );
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    searchOAP0021();
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
        /// 變更檔 補件中案件
        /// </summary>
        /// <param name="detailModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OAP0021AD(OAP0021DetailModel detailModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var OAP0021DetailData = (OAP0021DetailModel)Cache.Get(CacheList.OAP0021DetailData);
                OAP0021DetailData.add_rsn = detailModel.add_rsn;
                OAP0021DetailData.check_nos = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                result = OAP0021.ADFAPPYCH0(OAP0021DetailData, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    try
                    {
                        var remindList = new FAP.Web.Service.Actual.Common().GetMemoByUserId(new List<string>() { OAP0021DetailData.apply_id });
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
                           "應付票據變更補件通知",
                           $@"申請單號：{OAP0021DetailData.apply_no}，文件不齊備照會，請速補件辦理。",
                           false,
                           null
                           );
                    }
                    catch (Exception ex)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                    }
                    searchOAP0021();
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
        /// 新增支票
        /// </summary>
        /// <param name="check_No"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult addSubData(string check_No)
        {
            MSGReturnModel result = new MSGReturnModel();
            if (check_No.IsNullOrWhiteSpace())
            {
                result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
                return Json(result);
            }
            check_No = check_No?.Trim();
            var models = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
            if (models.Any(x => x.check_no == check_No))
            {
                result.DESCRIPTION = MessageType.Already_Same_Data.GetDescription();
            }
            else
            {
                try
                {
                    var model = new OAP0021DetailSubModel() { check_no = check_No };
                    result = OAP0021.getSubData(new List<OAP0021DetailSubModel>() { model });
                    if (model.system.IsNullOrWhiteSpace())
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = GetLRTCODE("BKMSG_CKX", "X33");
                    }
                    else if (result.RETURN_FLAG)
                    {
                        if (model.check_stat != "2")
                        {
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = GetLRTCODE("BKMSG_CKX", "X34");
                        }
                        else
                        {
                            model.apply_no = ((OAP0021DetailModel)Cache.Get(CacheList.OAP0021DetailData)).apply_no;
                            models.Add(model);
                            Cache.Invalidate(CacheList.OAP0021SubData);
                            Cache.Set(CacheList.OAP0021SubData, models);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 修改資料內容
        /// </summary>
        /// <param name="detailModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateSubData(OAP0021DetailSubModel detailModel)
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var models = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                var _model = models.FirstOrDefault(x => x.check_no == detailModel.check_no);
                if (_model == null)
                {
                    result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                }
                else
                {
                    _model.new_head = detailModel.new_head?.Trim();
                    _model.mark_type2 = detailModel.mark_type2;
                    var codes = new SysCodeDao().qryByType("AP", "MARK_TYPE");
                    _model.mark_type2_D = codes.Where(x => x.CODE == _model.mark_type2).Select(x => x.CODE + ":" + x.CODE_VALUE).FirstOrDefault();
                    result.RETURN_FLAG = true;
                    Cache.Invalidate(CacheList.OAP0021SubData);
                    Cache.Set(CacheList.OAP0021SubData, models);
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
        /// 刪除支票
        /// </summary>
        /// <param name="check_No"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult removeSubData(string check_No)
        {
            MSGReturnModel result = new MSGReturnModel();
            var models = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
            var model = models.FirstOrDefault(x => x.check_no == check_No);
            if (model != null)
            {
                models.Remove(model);
                result.RETURN_FLAG = true;
                Cache.Invalidate(CacheList.OAP0021SubData);
                Cache.Set(CacheList.OAP0021SubData, models);
            }
            else
            {
                result.DESCRIPTION = MessageType.time_Out.GetDescription();
            }
            return Json(result);
        }

        private bool searchOAP0021(OAP0021SearchModel searchModel = null)
        {
            List<OAP0021Model> result = new List<OAP0021Model>();
            if (searchModel == null)
                searchModel = (OAP0021SearchModel)Cache.Get(CacheList.OAP0021SearchData);
            if (searchModel != null)
            {
                result = OAP0021.Search_OAP0021(searchModel);
                Cache.Invalidate(CacheList.OAP0021ViewData);
                Cache.Set(CacheList.OAP0021ViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0021ViewData":
                    var OAP0021ViewData = (List<OAP0021Model>)Cache.Get(CacheList.OAP0021ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0021ViewData));
                case "OAP0021SubData":
                    var OAP0021SubData = (List<OAP0021DetailSubModel>)Cache.Get(CacheList.OAP0021SubData);
                    return Json(jdata.modelToJqgridResult(OAP0021SubData));
            }
            return null;
        }
    }
}