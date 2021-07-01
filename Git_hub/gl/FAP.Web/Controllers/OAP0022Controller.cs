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
/// 功能說明：應付票據變更用印清冊
/// 初版作者：20191125 Mark
/// 修改歷程：20191125 Mark
///           需求單號：
///           初版
/// </summary>
/// 

namespace FAP.Web.Controllers
{
    public class OAP0022Controller : CommonController
    {
        private IOAP0022 OAP0022;
        public OAP0022Controller()
        {
            OAP0022 = new OAP0022();
        }
        // GET: OAP0022
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();
            string opScope = "";
            string funcName = "";
            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0022/");
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
        /// 查詢應付票據變更用印清冊
        /// </summary>
        /// <param name="appr_s">覆核日(起)</param>
        /// <param name="appr_e">覆核日(迄)</param>
        /// <param name="rece_id">接收人</param>
        /// <param name="report_no">表單號碼</param>
        /// <returns></returns>
        [HttpPost]     
        public JsonResult qryOAP0022(OAP0022SearchModel searchModel)
        {
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            try
            {
                Cache.Invalidate(CacheList.OAP0022SearchData);
                Cache.Set(CacheList.OAP0022SearchData, searchModel);
                var model = searchOAP0022(searchModel);
                result.RETURN_FLAG = model.Item1;
                if (result.RETURN_FLAG)
                {
                    result.Datas = new Tuple<bool, bool>(model.Item2, model.Item3);
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
        /// 觸發 checkbox 事件
        /// </summary>
        /// <param name="check_no">支票號碼</param>
        /// <param name="apply_no">申請單號碼</param>
        /// <param name="flag">勾選狀態</param>
        /// <returns>回傳參數 : 1.列印按鈕顯示Flag 2.產生表單號碼列印按鈕 </returns>
        [HttpPost]
        public JsonResult CheckChange(string check_no, string apply_no, bool flag)
        {
            MSGReturnModel<Tuple<bool,bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            var ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0022ViewData);
            var _ViewData = ViewData.FirstOrDefault(x => x.check_no == check_no && x.apply_no == apply_no);
            if (_ViewData != null)
            {
                result.RETURN_FLAG = true;
                _ViewData.checkFlag = flag;
                result.Datas = new Tuple<bool, bool>(
                    ViewData.Any(x => x.checkFlag) && !ViewData.Where(x=>x.checkFlag).Any(x => string.IsNullOrWhiteSpace(x.report_no)),
                    ViewData.Any(x => x.checkFlag) && !ViewData.Where(x=>x.checkFlag).Any(x => !string.IsNullOrWhiteSpace(x.report_no))
                    );
                Cache.Invalidate(CacheList.OAP0022ViewData);
                Cache.Set(CacheList.OAP0022ViewData, ViewData);
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 設定表單號碼
        /// </summary>
        /// <returns></returns>
        public JsonResult SetReportNo()
        {
            MSGReturnModel<Tuple<bool, bool>> result = new MSGReturnModel<Tuple<bool, bool>>();
            var ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0022ViewData);
            var checkDatas = ViewData.Where(x => x.checkFlag).ToList();
            if (checkDatas.Any())
            {
                try
                {
                    var _result = OAP0022.Set_OAP0022(checkDatas, AccountController.CurrentUserId);
                    result.RETURN_FLAG = _result.RETURN_FLAG;
                    if (_result.RETURN_FLAG)
                    {
                        result.DESCRIPTION = MessageType.Exec_Success.GetDescription(null, $@"表單編號 : {_result.Datas}");
                        var _search = searchOAP0022();
                        result.Datas = new Tuple<bool, bool>(_search.Item2, _search.Item3);
                    }
                    else
                    {
                        result.DESCRIPTION = _result.DESCRIPTION;
                    }
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            else
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 組合支票號碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReportParm()
        {
            var ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0022ViewData);
            //return Json(string.Join(",", ViewData.Where(x => x.checkFlag).Select(x => x.check_no)));
            return Json(JsonConvert.SerializeObject(ViewData.Where(x => x.checkFlag).Select(x=>x.apply_no).ToList()));
        }

        /// <summary>
        /// 查詢 OAP0022  回傳參數 : 1.有無資料 2.列印按鈕顯示Flag 3.產生表單號碼列印按鈕 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns>回傳參數 : 1.有無資料 2.列印按鈕顯示Flag 3.產生表單號碼列印按鈕 </returns>
        private Tuple<bool,bool,bool> searchOAP0022(OAP0022SearchModel searchModel = null)
        {
            List<OAP0022Model> result = new List<OAP0022Model>();
            var printFlag = false; //列印按鈕顯示Flag
            var setFlag = false; //產生表單號碼列印按鈕
            if (searchModel == null)
                searchModel = (OAP0022SearchModel)Cache.Get(CacheList.OAP0022SearchData);
            if (searchModel != null)
            {
                result = OAP0022.Search_OAP0022(searchModel);
                var _buttonFlag = buttonFlag(result);
                printFlag = _buttonFlag.Item1;
                setFlag = _buttonFlag.Item2;
                Cache.Invalidate(CacheList.OAP0022ViewData);
                Cache.Set(CacheList.OAP0022ViewData, result);
            }
            return new Tuple<bool, bool, bool>(result.Any(), printFlag, setFlag);
        }

        /// <summary>
        /// 判斷 網頁按鈕 顯示
        /// </summary>
        /// <param name="model"></param>
        /// <returns>回傳參數 : 1.列印按鈕顯示Flag 2.產生表單號碼列印按鈕 </returns>
        private Tuple<bool, bool> buttonFlag(List<OAP0022Model> model)
        {
            var printFlag = false; //列印按鈕顯示Flag
            var setFlag = false; //產生表單號碼列印按鈕
            if (model.Any())
            {
                var _report_nos = model.Where(x => string.IsNullOrWhiteSpace(x.report_no)).ToList();
                if (_report_nos.Any())
                {
                    _report_nos.ForEach(x => x.checkFlag = true);
                    setFlag = true;
                }
                else
                {
                    model.ForEach(x => x.checkFlag = true);
                    printFlag = true;
                }
            }
            return new Tuple< bool, bool>(printFlag, setFlag);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0022ViewData":
                    var OAP0022ViewData = (List<OAP0022Model>)Cache.Get(CacheList.OAP0022ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0022ViewData));
            }
            return null;
        }
    }
}