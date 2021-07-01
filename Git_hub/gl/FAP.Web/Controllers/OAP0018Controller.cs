using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.BO.Utility;
using static FAP.Web.Enum.Ref;

namespace FAP.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class OAP0018Controller : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOAP0018 OAP0018;
        public OAP0018Controller()
        {
            OAP0018 = new OAP0018();
        }
        // GET: CrossDept
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0018/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;
            ViewBag.funId = new SelectList(new Service.Actual.Common().GetSysCode("AP", "FUN_ID", false), "Value", "Text");

            return View();
        }

        /// <summary>
        /// 表單查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(OAP0018SearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                result.DESCRIPTION = Search(searchModel);
                DateTime now = DateTime.Now;
                //searchModel.searchDt = now;
                //if (result.DESCRIPTION.IsNullOrWhiteSpace())
                result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        private string Search(OAP0018SearchViewModel searchData = null)
        {
            var result = string.Empty;
            if (searchData == null)
                searchData = (OAP0018SearchViewModel)Cache.Get(CacheList.OAP0018SearchData);
            searchData.searchDt = DateTime.Now;
            Cache.Invalidate(CacheList.OAP0018SearchData);
            Cache.Set(CacheList.OAP0018SearchData, searchData);
            var datas = OAP0018.GetSearchData(searchData);
            Cache.Invalidate(CacheList.OAP0018ViewData);
            Cache.Set(CacheList.OAP0018ViewData, datas);
            if (!datas.Any())
                result = MessageType.query_Not_Find.GetDescription();
            return result;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertData(OAP0018InsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.insert_Fail.GetDescription();
            try
            {
                var hasSameData = OAP0018.CheckSameData(model, "A");
                if (hasSameData)
                {
                    result.DESCRIPTION = "資料已存在請確認";
                    return Json(result);
                }
                else
                {
                    List<OAP0018ViewModel> tempData = new List<OAP0018ViewModel>();
                    if (Cache.IsSet(CacheList.OAP0018ViewData))
                    {
                        tempData = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);

                        tempData.ForEach(x =>
                        {
                            //if (x.fun_id == model.fun_id && x.appr_unit == model.appr_unit && x.user_unit == model.user_unit)
                            if (x.fun_id == model.fun_id && x.user_unit == model.user_unit)
                            {
                                hasSameData = true;
                            }
                        });
                        if (hasSameData)
                        {
                            result.DESCRIPTION = MessageType.Already_Same_Data.GetDescription();
                            return Json(result);
                        }
                    }

                    var _action = new Service.Actual.Common().GetSysCode("AP", "EXEC_ACTION", false);
                    var _status = new Service.Actual.Common().GetSysCode("AP", "DATA_STATUS", false);
                    var _fun = new Service.Actual.Common().GetSysCode("AP", "FUN_ID", false);
                    var emps = new Service.Actual.Common().GetEmps();
                    var depts = new Service.Actual.Common().GetDepts();
                    DateTime now = DateTime.Now;

                    OAP0018ViewModel insertData = new OAP0018ViewModel()
                    {
                        pk_id = model.pk_id,
                        fun_id = model.fun_id,
                        fun_value = _fun.FirstOrDefault(x => x.Value == model.fun_id)?.Text?.Trim(),
                        appr_unit = model.appr_unit,
                        appr_unit_name = new Service.Actual.Common().getFullDepName(depts.Where(z => z.DEP_ID == model.appr_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //appr_unit_name = model.appr_unit_name,
                        user_unit = model.user_unit,
                        user_unit_name = new Service.Actual.Common().getFullDepName(depts.Where(z => z.DEP_ID == model.user_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2,
                        //user_unit_name = model.user_unit_name,
                        memo = model.memo,
                        exec_action = "A",
                        exec_action_value = _action.FirstOrDefault(x => x.Value == "A")?.Text?.Trim(),
                        data_status = "1",
                        data_status_value = _status.FirstOrDefault(x => x.Value == "1")?.Text?.Trim(),
                        update_name = emps.Where(z => z.MEM_MEMO1 == AccountController.CurrentUserId)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault(),
                        update_time = $"{TypeTransfer.dateTimeNToStringNT(now)} {now.ToString("HH:mm:ss")}",
                        update_time_cpmpare = now
                    };


                    if (Cache.IsSet(CacheList.OAP0018ViewData))
                    {
                        tempData = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);
                    }

                    tempData.Add(insertData);
                    Cache.Invalidate(CacheList.OAP0018ViewData);
                    Cache.Set(CacheList.OAP0018ViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.insert_Success.GetDescription();
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
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateData(OAP0018InsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            try
            {
                var hasSameData = OAP0018.CheckSameData(model, "U");
                if (hasSameData)
                {
                    result.DESCRIPTION = MessageType.Already_Same_Data.GetDescription();
                    return Json(result);
                }

                List<OAP0018ViewModel> tempData = new List<OAP0018ViewModel>();
                if (Cache.IsSet(CacheList.OAP0018ViewData))
                {
                    tempData = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);

                    tempData.Where(x => x.pk_id != model.pk_id).ToList().ForEach(x =>
                    {
                        //if (x.fun_id == model.fun_id && x.appr_unit == model.appr_unit && x.user_unit == model.user_unit)
                        if (x.fun_id == model.fun_id && x.user_unit == model.user_unit)
                        {
                            hasSameData = true;
                        }
                    });
                    if (hasSameData)
                    {
                        result.DESCRIPTION = MessageType.Already_Same_Data.GetDescription();
                        return Json(result);
                    }
                }
                var _action = new Service.Actual.Common().GetSysCode("AP", "EXEC_ACTION", false);
                var _status = new Service.Actual.Common().GetSysCode("AP", "DATA_STATUS", false);
                var _fun = new Service.Actual.Common().GetSysCode("AP", "FUN_ID", false);
                var emps = new Service.Actual.Common().GetEmps();
                var depts = new Service.Actual.Common().GetDepts();
                DateTime now = DateTime.Now;

                var rowData = tempData.FirstOrDefault(x => x.pk_id == model.pk_id);

                if (rowData != null)
                {
                    rowData.fun_id = model.fun_id;
                    rowData.fun_value = _fun.FirstOrDefault(x => x.Value == model.fun_id)?.Text?.Trim();
                    rowData.appr_unit = model.appr_unit;
                    rowData.appr_unit_name = new Service.Actual.Common().getFullDepName(depts.Where(z => z.DEP_ID == model.appr_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2;
                    //rowData.appr_unit_name = model.appr_unit_name;
                    rowData.user_unit = model.user_unit;
                    rowData.user_unit_name = new Service.Actual.Common().getFullDepName(depts.Where(z => z.DEP_ID == model.user_unit)?.Select(z => z.DEP_ID)).FirstOrDefault()?.Item2;
                    //rowData.user_unit_name = model.user_unit_name;
                    rowData.memo = model.memo;

                    rowData.exec_action = rowData.exec_action == "A" ? "A" : "U";
                    rowData.exec_action_value = rowData.exec_action == "A" ? _action.FirstOrDefault(x => x.Value == "A")?.Text?.Trim() : _action.FirstOrDefault(x => x.Value == "U")?.Text?.Trim();
                    rowData.update_name = emps.Where(z => z.MEM_MEMO1 == AccountController.CurrentUserId)?.Select(y => { return $@"{y.MEM_NAME}({y.MEM_MEMO1})"; }).FirstOrDefault();
                    rowData.update_time = $"{TypeTransfer.dateTimeNToStringNT(now)} {now.ToString("HH:mm:ss")}";
                    rowData.update_time_cpmpare = now;

                    Cache.Invalidate(CacheList.OAP0018ViewData);
                    Cache.Set(CacheList.OAP0018ViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.update_Fail.GetDescription();
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
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult DeleteData(OAP0018InsertViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            try
            {
                var _action = new Service.Actual.Common().GetSysCode("AP", "EXEC_ACTION", false);

                if (Cache.IsSet(CacheList.OAP0018ViewData))
                {
                    var tempData = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);
                    var rowData = tempData.FirstOrDefault(x => x.pk_id == model.pk_id);

                    if (rowData != null)
                    {
                        //判斷是否新增資料
                        if (rowData.exec_action == "A")
                            tempData.Remove(rowData);
                        else
                        {
                            rowData.exec_action = "D";
                            rowData.exec_action_value = _action.FirstOrDefault(x => x.Value == "D")?.Text?.Trim();
                        }
                        Cache.Invalidate(CacheList.OAP0018ViewData);
                        Cache.Set(CacheList.OAP0018ViewData, tempData);
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = MessageType.delete_Success.GetDescription();
                        result.Datas = tempData.Where(x => x.exec_action != null).Any();
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = MessageType.update_Fail.GetDescription();
                    }
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
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                if (Cache.IsSet(CacheList.OAP0018SearchData) && Cache.IsSet(CacheList.OAP0018ViewData))
                {
                    var searchModel = (OAP0018SearchViewModel)Cache.Get(CacheList.OAP0018SearchData);
                    var _data = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);
                    result = OAP0018.ApplyDeptData(_data.Where(x => x.exec_action != null && x.data_status == "1").ToList());

                    if (result.RETURN_FLAG)
                        Search(searchModel);
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.Apply_Audit_Fail.GetDescription();
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
        /// 取部門名稱
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetDeptNameData(string deptId)
        {
            string deptName = "";
            if (!deptId.IsNullOrWhiteSpace())
            {
                if (deptId.Length == 5)
                {
                    var deptList = new Service.Actual.Common().GetDepts();
                    var Dept = deptList.FirstOrDefault(x => x.DEP_ID == deptId);
                    if (Dept != null)
                    {
                        deptName = Dept.DEP_NAME;
                    }
                }
            }
            return Json(deptName);
        }

        /// <summary>
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                var searchModel = (OAP0018SearchViewModel)Cache.Get(CacheList.OAP0018SearchData);
                result.DESCRIPTION = Search(searchModel);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            result.RETURN_FLAG = true;
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Search":
                    var Datas = (List<OAP0018ViewModel>)Cache.Get(CacheList.OAP0018ViewData);
                    return Json(jdata.modelToJqgridResult(Datas.OrderBy(x => x.pk_id).ToList()));
            }
            return null;
        }
    }
}