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
/// 功能說明：OAP0026 抽票部門權限關聯維護
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
    public class OAP0026Controller : CommonController
    {
        private IOAP0026 OAP0026;

        public OAP0026Controller()
        {
            OAP0026 = new OAP0026();
        }

        // GET: OAP0026
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0026/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            var datas = new Service.Actual.Common().tupleToSelectOption(OAP0026.getData(true));
            var datas_All = new List<SelectOption>();
            datas_All.Add(new SelectOption() { Text = "All", Value = "All" });
            datas_All.AddRange(datas);
            ViewBag.AP_PAID = new SelectList(datas, "Value", "Text");
            ViewBag.AP_PAID_A = new SelectList(datas_All, "Value", "Text");
            return View();
        }

        /// <summary>
        /// 查詢抽票部門權限關聯維護
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0026(OAP0026SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0026SearchData);
            Cache.Set(CacheList.OAP0026SearchData, searchModel);
            var model = searchOAP0026(searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                //result.RETURN_FLAG = model;
                result.RETURN_FLAG = true;
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
        /// 新增 or 刪除 資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateOAP0026(OAP0026ViewModel viewModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            try
            {
                var OAP0026ViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026ViewData);
                var _EXEC_ACTIONs = new SysCodeDao().qryByType("AP", "EXEC_ACTION");
                //var _DATA_STATUSs = new SysCodeDao().qryByType("AP", "DATA_STATUS");
                var datas = OAP0026.getData(); //查詢給付類型 & 中文
                switch (viewModel.exec_action)
                {
                    case "D":
                        if (viewModel.pk_id.IsNullOrWhiteSpace()) //新增的直接刪除
                        {
                            var _OAP0026ViewData = OAP0026ViewData
                                .FirstOrDefault(x => x.ap_paid == viewModel.ap_paid && x.unit_code == viewModel.unit_code);
                            if (_OAP0026ViewData == null)
                            {
                                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                            }
                            else
                            {
                                OAP0026ViewData.Remove(_OAP0026ViewData);
                                Cache.Invalidate(CacheList.OAP0026ViewData);
                                Cache.Set(CacheList.OAP0026ViewData, OAP0026ViewData);
                                result.RETURN_FLAG = true;
                                result.Datas = OAP0026ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                            }
                        }
                        else //現有資料註記為刪除
                        {
                            var _OAP0026ViewData = OAP0026ViewData
                                .FirstOrDefault(x => x.pk_id == viewModel.pk_id);
                            if (_OAP0026ViewData == null)
                            {
                                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                            }
                            else
                            {
                                _OAP0026ViewData.exec_action = viewModel.exec_action;
                                _OAP0026ViewData.exec_action_value = _EXEC_ACTIONs
                                    .FirstOrDefault(x => x.CODE == _OAP0026ViewData.exec_action)?.CODE_VALUE;
                                Cache.Invalidate(CacheList.OAP0026ViewData);
                                Cache.Set(CacheList.OAP0026ViewData, OAP0026ViewData);
                                result.RETURN_FLAG = true;
                                result.Datas = OAP0026ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                            }
                        }
                        break;
                    case "A":
                        var _checkFlaf = OAP0026.CheckSameData(viewModel);
                        if (OAP0026ViewData.Any(x => x.ap_paid == viewModel.ap_paid && x.unit_code == viewModel.unit_code) || _checkFlaf)
                        {
                            result.DESCRIPTION = $@"有相同資料在申請中 或 現行資料中{MessageType.Already_Same_Data.GetDescription()}";
                        }
                        else
                        {
                            OAP0026ViewData.Add(new OAP0026ViewModel()
                            {
                                ap_paid = viewModel.ap_paid,
                                ap_paid_value = datas.FirstOrDefault(x => x.Item1 == viewModel.ap_paid)?.Item2,
                                exec_action = viewModel.exec_action,
                                exec_action_value = _EXEC_ACTIONs.FirstOrDefault(x => x.CODE == viewModel.exec_action)?.CODE_VALUE,
                                unit_code = viewModel.unit_code,
                                unit_code_value = GetDeptName(viewModel.unit_code)
                            });
                            Cache.Invalidate(CacheList.OAP0026ViewData);
                            Cache.Set(CacheList.OAP0026ViewData, OAP0026ViewData);
                            result.RETURN_FLAG = true;
                            result.Datas = OAP0026ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                        }
                        break;
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
        /// 申請覆核
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyData()
        {
            var OAP0026ViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026ViewData);
            if (!OAP0026ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace()))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription()});
            }
            MSGReturnModel _result = new MSGReturnModel();
            try
            {
                 _result = OAP0026.ApplyDeptData(
                           OAP0026ViewData.Where(x => !x.exec_action.IsNullOrWhiteSpace()),
                           AccountController.CurrentUserId);
                if (_result.RETURN_FLAG)
                    searchOAP0026();
            }
            catch (Exception ex)
            {
                _result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(_result);
        }

        /// <summary>
        /// 取消申請(復原)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetData()
        {
            try
            {
                searchOAP0026();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(string.Empty);
        }

        /// <summary>
        /// 取部門名稱
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDeptNameData(string deptId)
        {
            return Json(GetDeptName(deptId));
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        private bool searchOAP0026(OAP0026SearchModel searchModel = null)
        {
            List<OAP0026ViewModel> result = new List<OAP0026ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0026SearchModel)Cache.Get(CacheList.OAP0026SearchData);
            if (searchModel != null)
            {
                result = OAP0026.GetSearchData(searchModel);
                Cache.Invalidate(CacheList.OAP0026ViewData);
                Cache.Set(CacheList.OAP0026ViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0026ViewData":
                    var OAP0026ViewData = (List<OAP0026ViewModel>)Cache.Get(CacheList.OAP0026ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0026ViewData));
            }
            return null;
        }
    }
}