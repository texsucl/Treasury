using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.Service.Actual;
using FRT.Web.Service.Interface;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static FRT.Web.BO.Utility;
using static FRT.Web.Enum.Ref;

/// <summary>
/// 功能說明：比對報表勾稽_批次定義(OPEN跨系統勾稽)
/// 初版作者：20210202 Mark
/// 修改歷程：20210202 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0105Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        private IORT0105 ORT0105 { get; set; }

        public ORT0105Controller()
        {
            Cache = new DefaultCacheProvider();
            ORT0105 = new ORT0105();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0105/");
                if (roleInfo != null && roleInfo.Length == 2)
                {
                    opScope = roleInfo[0];
                    funcName = roleInfo[1];
                }
            }

            ViewBag.opScope = opScope;
            ViewBag.funcName = funcName;

            SysCodeDao sysCodeDao = new SysCodeDao();
            //DATA_STATUS
            var _data_Status = sysCodeDao.qryByTypeDic("RT", "DATA_STATUS");
            ViewBag.dataStatusjqList = _data_Status;

            //資料狀態
            ViewBag.statusjqList = sysCodeDao.qryByTypeDic("RT", "STATUS");

            //跨系統勾稽作業_類別
            ViewBag.GOJ_TYPE = new SelectList(
                new Service.Actual.Common().GetSysCode("RT", "GOJ_TYPE", false, true), "Value", "Text");

            var _mail_group = new Service.Actual.Common().GetSysCodes("RT", new List<string>() { "MAIL_GROUP" })
                .Where(x => x.RESERVE1 == "CRS").OrderBy(x => x.ISORTBY)
                .Select(x => new SelectOption()
                {
                    Value = x.CODE,
                    Text = x.CODE_VALUE
                }).ToList();
            _mail_group.Insert(0, new SelectOption() { Text = " ", Value = " " });

            ViewBag.MAIL_GROUP = new SelectList(_mail_group, "Value", "Text");

            //ViewBag.MAIL_GROUP = new SelectList(
            //    new Service.Actual.Common().GetSysCode("RT", "MAIL_GROUP", false, true) , "Value", "Text");

            ViewBag.PLATFORM = new SelectList(
                new Service.Actual.Common().GetSysCode("RT", "GOJ_PLATFORM_TYPE", false, true), "Value", "Text"); 

            ViewBag.GOJ_START_TYPE = new Service.Actual.Common().GetSysCode("RT", "MAIL_GROUP");

            List<SelectOption> hh = new List<SelectOption>();
            for (var i = 0; i < 24; i++)
            {
                hh.Add(
                    new SelectOption()
                    {
                        Text = i.ToString().PadLeft(2, '0'),
                        Value = i.ToString().PadLeft(2, '0')
                    });
            }

            List<SelectOption> mm = new List<SelectOption>();
            for (var i = 0; i < 60; i++)
            {
                mm.Add(
                    new SelectOption()
                    {
                        Text = i.ToString().PadLeft(2, '0'),
                        Value = i.ToString().PadLeft(2, '0')
                    });
            }

            ViewBag.SCHEDULER_TIME_HH = new SelectList(hh, "Value", "Text");
            ViewBag.SCHEDULER_TIME_MM = new SelectList(mm, "Value", "Text");

            return View();
        }

        /// <summary>
        /// 查詢 資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORT0105()
        {
            logger.Info("qryORT0105 Query!!");
            MSGReturnModel result = new MSGReturnModel();
            result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            try
            {
                return Json(searchORT0105());
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }

        public JsonResult updateORT0105(ORT0105ViewModel viewModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ORT0105ViewData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105ViewData);
            //var _exec_action = new Service.Actual.Common().GetSysCode("RT", "STATUS", false);
            var _sysCodes = new Service.Actual.Common().GetSysCodes("RT", new List<string>() { 
                "STATUS", 
                "MAIL_GROUP",
                //"GOJ_PLATFORM_TYPE",
                "GOJ_START_TYPE",
                "GOJ_TYPE",
                "GOJ_TYPE_AP_GROUP",
                "GOJ_TYPE_BD_GROUP",
                "GOJ_TYPE_NP_GROUP" });
            if (viewModel.exec_action == "A" && 
                (ORT0105ViewData.Any(x => x.kind == viewModel.kind && x.type == viewModel.type) || 
                ORT0105.CheckSameData(viewModel)))
            {
                result.DESCRIPTION = $@"欲新增的資料,已有相同'類別:{viewModel.type_d},性質:{viewModel.kind_d}'存在現行的資料或申請的資料!";
            }
            else
            {
                try
                {
                    switch (viewModel.exec_action)
                    {
                        case "A":
                            ORT0105ViewData.Add(new ORT0105ViewModel()
                            {
                                check_id = viewModel.check_id,
                                exec_action = viewModel.exec_action,
                                exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" &&  x.CODE == viewModel.exec_action)?.CODE_VALUE,
                                type = viewModel.type,
                                type_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == viewModel.type)?.CODE_VALUE,
                                kind = viewModel.kind,
                                kind_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == $@"GOJ_TYPE_{viewModel.type}_GROUP" && x.CODE == viewModel.kind)?.CODE_VALUE,
                                frequency = viewModel.frequency,
                                frequency_value = viewModel.frequency_value,
                                frequency_d = ORT0105.frequency_d(viewModel.frequency,viewModel.frequency_value),
                                scheduler_time_hh = viewModel.scheduler_time_hh,
                                scheduler_time_mm = viewModel.scheduler_time_mm,
                                scheduler_time = new TimeSpan(viewModel.scheduler_time_hh,viewModel.scheduler_time_mm,0),
                                scheduler_time_d = $@"{viewModel.scheduler_time_hh.ToString().PadLeft(2,'0')}:{viewModel.scheduler_time_mm.ToString().PadLeft(2,'0')}",
                                start_date_type = viewModel.start_date_type,
                                start_date_value = viewModel.start_date_value,
                                start_date_d = ORT0105.start_date_d(viewModel.start_date_type,viewModel.start_date_value),
                                mail_group = viewModel.mail_group,
                                mail_group_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "MAIL_GROUP" && x.CODE == viewModel.mail_group)?.CODE_VALUE,
                                mail_key = viewModel.mail_key,
                                subDatas = ((List<ORT0105SubViewModel>)Cache.Get(CacheList.ORT0105ViewSubData)) ?? new List<ORT0105SubViewModel>()
                            });
                            break;
                        case "D":
                            var _ORT0105ViewData_D = ORT0105ViewData.FirstOrDefault(x => x.check_id == viewModel.check_id);
                            if (_ORT0105ViewData_D.exec_action == "A")
                            {
                                ORT0105ViewData.Remove(_ORT0105ViewData_D);
                            }
                            else
                            {
                                _ORT0105ViewData_D.exec_action = "D";
                                _ORT0105ViewData_D.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewModel.exec_action)?.CODE_VALUE;
                            }
                            break;
                        case "U":
                            var _ORT0105ViewData_U = ORT0105ViewData.FirstOrDefault(x => x.check_id == viewModel.check_id);
                            if (_ORT0105ViewData_U.exec_action != "A")
                            {
                                _ORT0105ViewData_U.exec_action = "U";
                                _ORT0105ViewData_U.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewModel.exec_action)?.CODE_VALUE;
                            }
                            _ORT0105ViewData_U.type = viewModel.type;
                            _ORT0105ViewData_U.type_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "GOJ_TYPE" && x.CODE == viewModel.type)?.CODE_VALUE;
                            _ORT0105ViewData_U.kind = viewModel.kind;
                            _ORT0105ViewData_U.kind_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == $@"GOJ_TYPE_{viewModel.type}_GROUP" && x.CODE == viewModel.kind)?.CODE_VALUE;
                            _ORT0105ViewData_U.frequency = viewModel.frequency;
                            _ORT0105ViewData_U.frequency_value = viewModel.frequency_value;
                            _ORT0105ViewData_U.frequency_d = ORT0105.frequency_d(viewModel.frequency, viewModel.frequency_value);
                            _ORT0105ViewData_U.scheduler_time_hh = viewModel.scheduler_time_hh;
                            _ORT0105ViewData_U.scheduler_time_mm = viewModel.scheduler_time_mm;
                            _ORT0105ViewData_U.scheduler_time = new TimeSpan(viewModel.scheduler_time_hh, viewModel.scheduler_time_mm, 0);
                            _ORT0105ViewData_U.scheduler_time_d = $@"{viewModel.scheduler_time_hh.ToString().PadLeft(2, '0')}:{viewModel.scheduler_time_mm.ToString().PadLeft(2, '0')}";
                            _ORT0105ViewData_U.start_date_type = viewModel.start_date_type;
                            _ORT0105ViewData_U.start_date_value = viewModel.start_date_value;
                            _ORT0105ViewData_U.start_date_d = ORT0105.start_date_d(viewModel.start_date_type, viewModel.start_date_value);
                            _ORT0105ViewData_U.mail_group = viewModel.mail_group;
                            _ORT0105ViewData_U.mail_group_d = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "MAIL_GROUP" && x.CODE == viewModel.mail_group)?.CODE_VALUE;
                            _ORT0105ViewData_U.mail_key = viewModel.mail_key;
                            if (Cache.IsSet(CacheList.ORT0105ViewSubData))
                                _ORT0105ViewData_U.subDatas = ((List<ORT0105SubViewModel>)Cache.Get(CacheList.ORT0105ViewSubData));
                            break;
                    }
                    Cache.Invalidate(CacheList.ORT0105ViewData);
                    Cache.Set(CacheList.ORT0105ViewData, ORT0105ViewData);
                    result.RETURN_FLAG = true;
                    result.Datas = ORT0105ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                    Cache.Invalidate(CacheList.ORT0105ViewSubData);
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(result);
        }

        public JsonResult updateORT0105Sub(ORT0105SubViewModel viewSubModel)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            var ORT0105ViewData = ((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105ViewData))
                .FirstOrDefault(x => x.check_id == viewSubModel.check_id);
            var ORT0105ViewSubData = ORT0105ViewData?.subDatas ??
                ((List<ORT0105SubViewModel>)Cache.Get(CacheList.ORT0105ViewSubData)) ??
                new List<ORT0105SubViewModel>();
            var _sysCodes = new Service.Actual.Common().GetSysCodes("RT", new List<string>() {
                "STATUS",
                "GOJ_PLATFORM_TYPE" });
            if (viewSubModel.exec_action != "D" && ORT0105ViewSubData
                .Any(x => x.sub_id != viewSubModel.sub_id
                && x.platform == viewSubModel.platform 
                && x.file_code == viewSubModel.file_code))
            {
                result.DESCRIPTION = $@"欲新增的明細資料,明細檔中已有相同資料!";
            }
            else
            {
                try
                {
                    switch (viewSubModel.exec_action)
                    {
                        case "A":
                            ORT0105ViewSubData.Add(new ORT0105SubViewModel()
                            {
                                check_id = viewSubModel.check_id,
                                sub_id = viewSubModel.sub_id,
                                exec_action = viewSubModel.exec_action,
                                exec_action_value = _sysCodes.FirstOrDefault(x =>
                                x.CODE_TYPE == "STATUS" && 
                                x.CODE == viewSubModel.exec_action)?.CODE_VALUE,
                                platform = viewSubModel.platform,
                                file_code = viewSubModel.file_code,
                                file_code_d = viewSubModel.file_code_d,
                                memo = viewSubModel.memo,

                            });
                            break;
                        case "D":
                            var _ORT0105ViewSubData_D = ORT0105ViewSubData.FirstOrDefault(x => x.sub_id == viewSubModel.sub_id);
                            if (_ORT0105ViewSubData_D.exec_action == "A")
                            {
                                ORT0105ViewSubData.Remove(_ORT0105ViewSubData_D);
                            }
                            else
                            {
                                _ORT0105ViewSubData_D.exec_action = "D";
                                _ORT0105ViewSubData_D.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewSubModel.exec_action)?.CODE_VALUE;
                            }
                            break;
                        case "U":
                            var _ORT0105ViewSubData_U = ORT0105ViewSubData.FirstOrDefault(x => x.sub_id == viewSubModel.sub_id);
                            if (_ORT0105ViewSubData_U.exec_action != "A")
                            {
                                _ORT0105ViewSubData_U.exec_action = "U";
                                _ORT0105ViewSubData_U.exec_action_value = _sysCodes.FirstOrDefault(x => x.CODE_TYPE == "STATUS" && x.CODE == viewSubModel.exec_action)?.CODE_VALUE;
                            }
                             _ORT0105ViewSubData_U.platform = viewSubModel.platform;
                             _ORT0105ViewSubData_U.file_code = viewSubModel.file_code;
                             _ORT0105ViewSubData_U.file_code_d = viewSubModel.file_code_d;
                             _ORT0105ViewSubData_U.memo = viewSubModel.memo;                           
                            break;
                    }
                    Cache.Invalidate(CacheList.ORT0105ViewSubData);
                    Cache.Set(CacheList.ORT0105ViewSubData, ORT0105ViewSubData);
                    result.RETURN_FLAG = true;
                    result.Datas = ORT0105ViewSubData.Any(x => !x.exec_action.IsNullOrWhiteSpace());
                }
                catch (Exception ex)
                {
                    result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
                }
            }
            return Json(result);
        }

        public JsonResult ApplyData()
        {
            var ORT0105ViewData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105ViewData);
            if (!ORT0105ViewData.Any(x => !x.exec_action.IsNullOrWhiteSpace()))
            {
                return Json(new MSGReturnModel() { DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription() });
            }
            MSGReturnModel _result = new MSGReturnModel();
            try
            {
                _result = ORT0105.ApplyDeptData(ORT0105ViewData.Where(x => !x.exec_action.IsNullOrWhiteSpace()), AccountController.CurrentUserId);
                if (_result.RETURN_FLAG)
                    searchORT0105();
            }
            catch (Exception ex)
            {
                _result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(_result);
        }

        public JsonResult GetGOJ_TYPE_GROUP(string type)
        {
            //return Json(new SelectList(
            //    new Service.Actual.Common().GetSysCode("RT", $@"GOJ_TYPE_{type}_GROUP", false, true), "Value", "Text"));
            return Json(new Service.Actual.Common().GetSysCode("RT", $@"GOJ_TYPE_{type}_GROUP", false, true));
        }

        private MSGReturnModel searchORT0105()
        {
            MSGReturnModel result = new MSGReturnModel();
            Cache.Invalidate(CacheList.ORT0105ViewSubData);
            var _result = ORT0105.GetSearchData();
            if (_result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ORT0105ViewData);
                Cache.Set(CacheList.ORT0105ViewData, _result.Datas);
            }
            result.RETURN_FLAG = _result.RETURN_FLAG;
            result.DESCRIPTION = _result.DESCRIPTION;
            return result;
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type, string check_id = null)
        {
            switch (type)
            {
                case "ORT0105Model":
                    return Json(jdata.modelToJqgridResult(((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105ViewData))
                        ?.OrderBy(x => x.type).ThenBy(x => x.kind).ToList()));
                case "ORT0105SubModel":
                    return Json(jdata.modelToJqgridResult(
                        ((List<ORT0105SubViewModel>)Cache.Get(CacheList.ORT0105ViewSubData)) ??
                        ((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105ViewData))
                        .FirstOrDefault(x => x.check_id == check_id)?.subDatas
                        ?.OrderBy(x => x.platform).ThenBy(x => x.file_code).ToList() ??                      
                        new List<ORT0105SubViewModel>()));
            }
            return null;
        }
    }
}