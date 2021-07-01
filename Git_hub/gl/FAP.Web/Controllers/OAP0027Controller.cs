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
/// 功能說明：OAP0027 抽票原因維護
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
    public class OAP0027Controller : CommonController
    {
        private IOAP0027 OAP0027;

        public OAP0027Controller()
        {
            OAP0027 = new OAP0027();
        }


        // GET: OAP0027
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0027/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;
            var datas = new Service.Actual.Common().tupleToSelectOption(OAP0027.getData(),true);
            datas.Insert(0, new SelectOption() { Text = "All", Value = "All" });
            ViewBag.REASON_CODE_A = new SelectList(datas, "Value", "Text");
            return View();
        }

        /// <summary>
        /// 查詢原因維護
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOAP0027(OAP0027SearchModel searchModel)
        {
            Cache.Invalidate(CacheList.OAP0027SearchData);
            Cache.Set(CacheList.OAP0027SearchData, searchModel);
            MSGReturnModel result = new MSGReturnModel();
            try
            {
                var model = searchOAP0027(searchModel);
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
        /// 新增,刪除,修改 資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult updateOAP0027(OAP0027ViewModel viewModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var OAP0027ViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027ViewData);
            var _exec_action = new Service.Actual.Common().GetSysCode("AP", "EXEC_ACTION", false);
            //var _data_status = new Service.Actual.Common().GetSysCode("AP", "DATA_STATUS", false);
            if (viewModel.exec_action == "A" && OAP0027.CheckSameData(viewModel))
            {
                result.DESCRIPTION =  $@"有相同資料在申請中 或 現行資料中{MessageType.Already_Same_Data.GetDescription()}";
            }
            else
            {
                try
                {
                    switch (viewModel.exec_action)
                    {
                        case "A":
                            OAP0027ViewData.Add(new OAP0027ViewModel()
                            {
                                pk_id = viewModel.pk_id,
                                exec_action = viewModel.exec_action,
                                exec_action_value = _exec_action.FirstOrDefault(x => x.Value == viewModel.exec_action)?.Text,
                                //data_status = viewModel.data_status,
                                //data_status_value = _data_status.FirstOrDefault(x => x.Value == viewModel.data_status)?.Text,
                                reason_code = viewModel.reason_code,
                                reason = viewModel.reason,
                                referral_dep = viewModel.referral_dep,
                                referral_dep_name = GetDeptName(viewModel.referral_dep),
                            });
                            break;
                        case "D":
                            var _OAP0027ViewData_D = OAP0027ViewData.FirstOrDefault(x => x.pk_id == viewModel.pk_id);
                            if (_OAP0027ViewData_D.exec_action == "A")
                            {
                                OAP0027ViewData.Remove(_OAP0027ViewData_D);
                            }
                            else
                            {
                                _OAP0027ViewData_D.exec_action = "D";
                                _OAP0027ViewData_D.exec_action_value = _exec_action.FirstOrDefault(x => x.Value == _OAP0027ViewData_D.exec_action)?.Text;
                            }
                            break;
                        case "U":
                            var _OAP0027ViewData_U = OAP0027ViewData.FirstOrDefault(x => x.pk_id == viewModel.pk_id);
                            if (_OAP0027ViewData_U.exec_action != "A")
                            {
                                _OAP0027ViewData_U.exec_action = "U";
                                _OAP0027ViewData_U.exec_action_value = _exec_action.FirstOrDefault(x => x.Value == _OAP0027ViewData_U.exec_action)?.Text;
                            }
                            else
                            {
                                _OAP0027ViewData_U.reason_code = viewModel.reason_code;
                                _OAP0027ViewData_U.reason = viewModel.reason;
                            }
                            if (viewModel.referral_dep.IsNullOrWhiteSpace())
                            {
                                _OAP0027ViewData_U.referral_dep = null;
                                _OAP0027ViewData_U.referral_dep_name = null;
                            }
                            else
                            {
                                _OAP0027ViewData_U.referral_dep = viewModel.referral_dep;
                                _OAP0027ViewData_U.referral_dep_name = GetDeptName(viewModel.referral_dep);
                            }
                            break;
                    }
                    Cache.Invalidate(CacheList.OAP0027ViewData);
                    Cache.Set(CacheList.OAP0027ViewData, OAP0027ViewData);
                    result.RETURN_FLAG = true;
                    result.Datas = OAP0027ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
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
        /// 申請覆核
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyData()
        {
            var OAP0027ViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027ViewData);
            if (!OAP0027ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace()))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription() });
            }
            MSGReturnModel _result = new MSGReturnModel();
            try
            {
                _result = OAP0027.ApplyDeptData(OAP0027ViewData.Where(x => !x.exec_action.IsNullOrWhiteSpace()), AccountController.CurrentUserId);
                if (_result.RETURN_FLAG)
                    searchOAP0027();
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
                searchOAP0027();
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

        private bool searchOAP0027(OAP0027SearchModel searchModel = null)
        {
            List<OAP0027ViewModel> result = new List<OAP0027ViewModel>();
            if (searchModel == null)
                searchModel = (OAP0027SearchModel)Cache.Get(CacheList.OAP0027SearchData);
            if (searchModel != null)
            {
                result = OAP0027.GetSearchData(searchModel);
                Cache.Invalidate(CacheList.OAP0027ViewData);
                Cache.Set(CacheList.OAP0027ViewData, result);
            }
            return result.Any();
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0027ViewData":
                    var OAP0027ViewData = (List<OAP0027ViewModel>)Cache.Get(CacheList.OAP0027ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0027ViewData));
            }
            return null;
        }
    }
}