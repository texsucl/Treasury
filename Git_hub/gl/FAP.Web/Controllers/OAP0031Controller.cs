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
using FAP.Web.Models;

/// <summary>
/// 功能說明：OAP0031 用印檢視確認功能
/// 初版作者：20200406 張家華
/// 修改歷程：20200406 張家華 
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
    public class OAP0031Controller : CommonController
    {
        private IOAP0031 OAP0031;

        public OAP0031Controller()
        {
            OAP0031 = new OAP0031();
        }

        // GET: OAP0031
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0031/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }
            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            ViewBag.sendStyle_A = new SelectList(new Service.Actual.Common().GetSysCode("AP", "SEND_STYLE", true), "Value", "Text");
            var _REJ_RSN = new Service.Actual.Common().GetSysCode("AP", "REJ_RSN");
            _REJ_RSN.Insert(0, new SelectOption() { Value = " ", Text = "請選擇" });
            ViewBag.REJ_RSN = new SelectList(_REJ_RSN, "Value", "Text");
            string _Q32 = string.Empty;
            try
            {
                _Q32 = GetLRTCODE("BKMSG_CKQ", "Q32"); 
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            ViewBag.Q32 = _Q32;
            ViewBag.YN = new SelectList(
                items: new List<SelectOption>() {
                                new SelectOption() { Value = "All", Text = "All"},
                                new SelectOption() { Value = "Y",Text = "Y"},
                                new SelectOption() { Value = "N",Text = "N"}
                },
                dataValueField: "Value",
                dataTextField: "Text"
            );
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0031(OAP0031SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0031SearchData);
            Cache.Set(CacheList.OAP0031SearchData, searchModel);
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            try
            {
                var model = searchOAP0031(searchModel);
                result.RETURN_FLAG = model.Item1;
                if (result.RETURN_FLAG)
                {
                    result.Datas = new Tuple<bool, bool>(model.Item2, model.Item3);

                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "OAP0031Controller";
                    piaLogMain.EXECUTION_CONTENT = $@"create_date_s:{searchModel.create_date_s}|create_date_e:{searchModel.create_date_e}|label_no_flag:{searchModel.label_no_flag}|rece_id:{searchModel.rece_id}";
                    piaLogMain.AFFECT_ROWS = model.Item4;
                    piaLogMain.PIA_TYPE = "0100000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_MAIL_LABEL";
                    piaLogMainDao.Insert(piaLogMain);
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
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
        /// 產出標籤號碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetLabelNo()
        {
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool,bool>>();
            try
            {
                var models = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0031ViewData);
                if (models.Any(x => x.checkFlag))
                {
                    var _result = OAP0031.SetLabel_No(models.Where(x => x.checkFlag), AccountController.CurrentUserId);
                    result.DESCRIPTION = _result.DESCRIPTION;
                    result.RETURN_FLAG = _result.RETURN_FLAG;
                    if (result.RETURN_FLAG)
                    {
                        var model = searchOAP0031();
                        result.Datas = new Tuple<bool, bool>(model.Item2, model.Item3);
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
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
        /// 標籤檔案 匯出 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DownLoadOAP0031()
        {
            MSGReturnModel result = new MSGReturnModel();
            var models = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0031ViewData);
            if (models.Any(x => x.checkFlag))
            {
                result.RETURN_FLAG = true;
                Cache.Invalidate($@"DL_OAP0031");
                var _data = models.Where(x => x.checkFlag).Cast<IFileDownLoadModel>().ToList();
                Cache.Set($@"DL_OAP0031", (_data));
                try
                {
                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "OAP0031Controller";
                    piaLogMain.EXECUTION_CONTENT = " ";
                    piaLogMain.AFFECT_ROWS = _data.Count;
                    piaLogMain.PIA_TYPE = "0100000000";
                    piaLogMain.EXECUTION_TYPE = "X";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_MAIL_LABEL";
                    piaLogMainDao.Insert(piaLogMain);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            else
            {
                result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 大宗號碼 匯入
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpLoadOAP0031()
        {
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var models = (List<OAP0031ViewModel>)Cache.Get($@"UL_OAP0031");
                if (models.Any())
                {
                    if (!models.Any(x => !x.bulk_no.IsNullOrWhiteSpace()))
                    {
                        result.DESCRIPTION = "匯入資料中大宗掛號均沒有值!";
                    }
                    else
                    {
                        result = OAP0031.Setbulk_no(models, AccountController.CurrentUserId);
                        try
                        {
                            PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                            PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                            piaLogMain.TRACKING_TYPE = "A";
                            piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                            piaLogMain.ACCOUNT_NAME = "";
                            piaLogMain.PROGFUN_NAME = "OAP0031Controller";
                            piaLogMain.EXECUTION_CONTENT = " ";
                            piaLogMain.AFFECT_ROWS = models.Count;
                            piaLogMain.PIA_TYPE = "0100000000";
                            piaLogMain.EXECUTION_TYPE = "I";
                            piaLogMain.ACCESSOBJ_NAME = "FAP_MAIL_LABEL";
                            piaLogMainDao.Insert(piaLogMain);
                        }
                        catch (Exception ex)
                        {
                            NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                        }
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Any.GetDescription();
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
        /// 勾選觸發事件
        /// </summary>
        /// <param name="apply_no"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckChange(string id, bool flag)
        {
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool,bool>>();
            var ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0031ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.pkid == id);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                var _getBtnStatus = getBtnStatus(ViewData);
                result.Datas = new Tuple<bool, bool>(_getBtnStatus.Item1, _getBtnStatus.Item2);
                Cache.Invalidate(CacheList.OAP0031ViewData);
                Cache.Set(CacheList.OAP0031ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// true => 選擇有labelNo的資料, false => 選擇沒有labelNo的資料  
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public JsonResult LabelNoCheckbox(bool flag)
        {
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool,bool>>();
            var ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0031ViewData);
            if (ViewData.Any())
            {
                ViewData.ForEach(x =>
                {
                    x.checkFlag = (flag && !x.label_no.IsNullOrWhiteSpace() && x.bulk_no.IsNullOrWhiteSpace()) || (!flag && x.label_no.IsNullOrWhiteSpace());
                });
                result.RETURN_FLAG = true;
                var _result = getBtnStatus(ViewData);
                result.Datas = new Tuple<bool, bool>(_result.Item1, _result.Item2);
                Cache.Invalidate(CacheList.OAP0031ViewData);
                Cache.Set(CacheList.OAP0031ViewData, ViewData);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetUserName(string userId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var userName = new CommonController().GetUserName(userId);
            if (!userName.IsNullOrWhiteSpace())
            {
                result.RETURN_FLAG = true;
                result.Datas = userName;
            }
            return Json(result);
        }


        /// <summary>
        /// 查詢OAP0031
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private Tuple<bool, bool, bool, int> searchOAP0031(OAP0031SearchModel searchModel = null)
        {
            bool _label_no_flag = false;
            bool _out_flag = false;
            int _count = 0;
            List<OAP0031ViewModel> result = new List<OAP0031ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0031SearchModel)Cache.Get(CacheList.OAP0031SearchData);
            if (searchModel != null)
            {
                result = OAP0031.Search_OAP0031(searchModel);
                _count = result.Count;
                var _getBtnStatus = getBtnStatus(result);
                _label_no_flag = _getBtnStatus.Item1;
                _out_flag = _getBtnStatus.Item2;
                Cache.Invalidate(CacheList.OAP0031ViewData);
                Cache.Set(CacheList.OAP0031ViewData, result);
            }
            return new Tuple<bool,bool,bool,int>(result.Any(), _label_no_flag, _out_flag, _count);
        }

        /// <summary>
        /// 回傳參數 1=> 產出標籤號碼 按鈕的顯示 , 2=> 標籤檔案匯出 按鈕的顯示
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<bool, bool> getBtnStatus(List<OAP0031ViewModel> models)
        {
            bool _label_no_flag = false;
            bool _out_flag = false;
            var _checks = models.Where(x => x.checkFlag).ToList();
            if (_checks.Any())
            {
                _label_no_flag = !_checks.Any(x => !x.label_no.IsNullOrWhiteSpace());
                _out_flag = (!_checks.Any(x=>x.label_no.IsNullOrWhiteSpace())) && (!_checks.Any(x => !x.bulk_no.IsNullOrWhiteSpace()));
            }
            return new Tuple<bool, bool>(_label_no_flag, _out_flag);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0031ViewData":
                    var OAP0031ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0031ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0031ViewData));
                case "ULOAP0031ViewData":
                    var ULOAP0031ViewData = (List<OAP0031ViewModel>)Cache.Get($@"UL_OAP0031");
                    return Json(jdata.modelToJqgridResult(ULOAP0031ViewData));
            }
            return null;
        }
    }
}