using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Daos;
using FAP.Web.Models;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;

/// <summary>
/// 功能說明：OAP0032 變據變更歸檔檢視報表
/// 初版作者：20200417 張家華
/// 修改歷程：20200417 張家華
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
    public class OAP0032Controller : CommonController
    {
        private IOAP0032 OAP0032;

        public OAP0032Controller()
        {
            OAP0032 = new OAP0032();
        }

        // GET: OAP0032
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            
            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0032/");
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
        public JsonResult qryOAP0032(OAP0032SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0032SearchData);
            Cache.Set(CacheList.OAP0032SearchData, searchModel);
            MSGReturnModel<Tuple<bool, bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            try
            {
                var model = searchOAP0032(searchModel);
                result.RETURN_FLAG = model.Item1;
                if (result.RETURN_FLAG)
                {
                    result.Datas = new Tuple<bool, bool>(model.Item3, model.Item4);

                    PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                    PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                    piaLogMain.TRACKING_TYPE = "A";
                    piaLogMain.ACCESS_ACCOUNT = AccountController.CurrentUserId;
                    piaLogMain.ACCOUNT_NAME = "";
                    piaLogMain.PROGFUN_NAME = "OAP0032Controller";
                    piaLogMain.EXECUTION_CONTENT = $@"label_no:{searchModel.label_no}|update_id:{searchModel.update_id}|update_date:{searchModel.update_date}";
                    piaLogMain.AFFECT_ROWS = model.Item5;
                    piaLogMain.PIA_TYPE = "0100000000";
                    piaLogMain.EXECUTION_TYPE = "Q";
                    piaLogMain.ACCESSOBJ_NAME = "FAP_MAIL_LABEL";
                    piaLogMainDao.Insert(piaLogMain);
                }
                else
                {
                    result.DESCRIPTION = model.Item2;
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
            MSGReturnModel<Tuple<bool, bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            var ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0032ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.pkid == id);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                var _getBtnStatus = getBtnStatus(ViewData);
                result.Datas = new Tuple<bool, bool>(_getBtnStatus.Item1, _getBtnStatus.Item2);
                Cache.Invalidate(CacheList.OAP0032ViewData);
                Cache.Set(CacheList.OAP0032ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 清空標籤號碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ClearLabelNo()
        {
            MSGReturnModel<Tuple<bool, bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            try
            {
                var ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0032ViewData);
                var _result = OAP0032.Clearlabel_no(ViewData.Where(x => x.checkFlag).Select(x => x.label_no),
                    AccountController.CurrentUserId);
                result.DESCRIPTION = _result.DESCRIPTION;
                result.RETURN_FLAG = _result.RETURN_FLAG;
                if (_result.RETURN_FLAG)
                {
                    var _search = searchOAP0032();
                    result.Datas = new Tuple<bool, bool>(_search.Item3, _search.Item4);
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
        /// 清空大宗號碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ClearBulkNo()
        {
            MSGReturnModel<Tuple<bool, bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            try
            {
                var ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0032ViewData);
                var _result = OAP0032.Clearbulk_no(ViewData.Where(x => x.checkFlag).Select(x => x.bulk_no),
                    AccountController.CurrentUserId);
                result.DESCRIPTION = _result.DESCRIPTION;
                result.RETURN_FLAG = _result.RETURN_FLAG;
                if (_result.RETURN_FLAG)
                {
                    var _search = searchOAP0032();
                    result.Datas = new Tuple<bool, bool>(_search.Item3, _search.Item4);
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
        /// 查詢OAP0032
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private Tuple<bool, string, bool, bool, int> searchOAP0032(OAP0032SearchModel searchModel = null)
        {
            bool _label_no_flag = false;
            bool _bulk_no_flag = false;
            int _count = 0;
            MSGReturnModel<List<OAP0031ViewModel>> result = new MSGReturnModel<List<OAP0031ViewModel>>();
            if (searchModel == null)
                searchModel = (OAP0032SearchModel)Cache.Get(CacheList.OAP0032SearchData);
            if (searchModel != null)
            {
                result = OAP0032.Search_OAP0032(searchModel);
                if (result.RETURN_FLAG)
                {
                    var _getBtnStatus = getBtnStatus(result.Datas);
                    _label_no_flag = _getBtnStatus.Item1;
                    _bulk_no_flag = _getBtnStatus.Item2;
                    _count = result.Datas.Count;
                }
                else
                    result.Datas = new List<OAP0031ViewModel>();
                Cache.Invalidate(CacheList.OAP0032ViewData);
                Cache.Set(CacheList.OAP0032ViewData, result.Datas);
            }
            return new Tuple<bool, string, bool, bool, int>(result.RETURN_FLAG, result.DESCRIPTION, _label_no_flag, _bulk_no_flag, _count);
        }

        /// <summary>
        /// 回傳參數 1=> 產出標籤號碼 按鈕的顯示 , 2=> 標籤檔案匯出 按鈕的顯示
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private Tuple<bool, bool> getBtnStatus(List<OAP0031ViewModel> models)
        {
            bool _label_no_flag = false;
            bool _bulk_no_flag = false;
            var _checks = models.Where(x => x.checkFlag).ToList();
            if (_checks.Any())
            {
                _label_no_flag = ((!_checks.Any(x => x.label_no.IsNullOrWhiteSpace())) && !_checks.Any(x => !x.bulk_no.IsNullOrWhiteSpace()));
                _bulk_no_flag = !_checks.Any(x => x.bulk_no.IsNullOrWhiteSpace());
            }
            return new Tuple<bool, bool>(_label_no_flag, _bulk_no_flag);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0032ViewData":
                    var OAP0032ViewData = (List<OAP0031ViewModel>)Cache.Get(CacheList.OAP0032ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0032ViewData));
            }
            return null;
        }
    }
}