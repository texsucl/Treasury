using FAP.Web.ActionFilter;
using FAP.Web.BO;
using FAP.Web.Service.Actual;
using FAP.Web.Service.Interface;
using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using FAP.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static FAP.Web.Enum.Ref;
using static FAP.Web.BO.Utility;

/// <summary>
/// 功能說明：OAP0020-支票簽收窗口維護作業
/// 初版作者：20191115 李彥賢
/// 修改歷程：20191115 李彥賢 
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
    public class OAP0020Controller : CommonController
    {
        private IOAP0020 OAP0020;
        public OAP0020Controller()
        {
            OAP0020 = new OAP0020();
        }
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        // GET: OAP0020
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";

            if (Session["UserID"] != null)
            {
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0020/");
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
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                result.DESCRIPTION = Search();
                if (result.DESCRIPTION.IsNullOrWhiteSpace())
                    result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢明細
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchDetailData(string dept, string dept_Name)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                result.DESCRIPTION = SearchDetail(dept, dept_Name);
                if (result.DESCRIPTION.IsNullOrWhiteSpace())
                    result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = MessageType.sys_Error.GetDescription();
                NLog.LogManager.GetCurrentClassLogger().Error(ex.ToString());
            }
            return Json(result);
        }

        public string Search()
        {
            var datas = OAP0020.GetSearchData();
            Cache.Invalidate(CacheList.OAP0020ViewData);
            Cache.Set(CacheList.OAP0020ViewData, datas);
            return datas.Any() ? string.Empty : MessageType.not_Find_Any.GetDescription();
        }

        public string SearchDetail(string dept, string dept_Name)
        {
            var datas = OAP0020.GetSearchDetail(dept, dept_Name);
            Cache.Invalidate(CacheList.OAP0020DetailViewData);
            Cache.Set(CacheList.OAP0020DetailViewData, datas);
            return datas.Any() ? string.Empty : MessageType.not_Find_Any.GetDescription();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertData(OAP0020InsertModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.insert_Fail.GetDescription();
            try
            {
                var hasSameData = OAP0020.CheckSameData(model, "A");
                if (hasSameData.Item1)
                {
                    result.DESCRIPTION = hasSameData.Item2;
                    return Json(result);
                }
                result = OAP0020.InsertData(model, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    if (model.type == "M")
                        Search();
                    else
                        SearchDetail(model.dep_id, model.dep_name);
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
        public JsonResult UpdateData(OAP0020InsertModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.update_Fail.GetDescription();
            try
            {
                if (model != null)
                {
                    var ViewData = new List<OAP0020ViewModel>();
                    var data = new OAP0020ViewModel();
                    if (model.type == "M")
                    {
                        ViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020ViewData);
                        data = ViewData.FirstOrDefault(x => x.dep_id == model.dep_id);
                    }
                    else
                    {
                        ViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020DetailViewData);
                        data = ViewData.FirstOrDefault(x => x.division == model.division);
                    }

                    if (data != null)
                    {
                        result = OAP0020.UpdateData(model, AccountController.CurrentUserId);
                        if (result.RETURN_FLAG)
                        {
                            if (model.type == "M")
                                Search();
                            else
                                SearchDetail(model.dep_id, model.dep_name);
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = MessageType.data_Not_Compare.GetDescription();
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
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteData(OAP0020InsertModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.delete_Fail.GetDescription();
            try
            {
                if (model != null)
                {
                    var ViewData = new List<OAP0020ViewModel>();
                    var data = new OAP0020ViewModel();
                    if (model.type == "M")
                    {
                        ViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020ViewData);
                        data = ViewData.FirstOrDefault(x => x.dep_id == model.dep_id);
                    }
                    else
                    {
                        ViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020DetailViewData);
                        data = ViewData.FirstOrDefault(x => x.division == model.division);
                    }
                    if (data != null)
                    {
                        result = OAP0020.DeleteData(model, AccountController.CurrentUserId);
                        if (result.RETURN_FLAG)
                        {
                            if (model.type == "M")
                                Search();
                            else
                                SearchDetail(model.dep_id, model.dep_name);
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = MessageType.data_Not_Compare.GetDescription();
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
                if(deptId.Length == 5)
                {
                    var deptList = new Service.Actual.Common().GetDepts();
                    var Dept = deptList.FirstOrDefault(x => x.DEP_ID == deptId);
                    if(Dept != null)
                    {
                        deptName = Dept.DEP_NAME;
                    }
                }
            }
            return Json(deptName);
        }

        /// <summary>
        /// 員工資訊
        /// </summary>
        /// <param name="deptId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetEmpData(string aptId)
        {
            List<string> aptList = new List<string>();
            aptList.Add(aptId);
            var empDetail = new Service.Actual.Common().GetMemoByUserId(aptList);

            return Json(empDetail);
        }
        

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "OAP0020ViewData":
                    var OAP0020ViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020ViewData);
                    return Json(jdata.modelToJqgridResult(OAP0020ViewData));
                case "OAP0020DetailViewData":
                    var OAP0020DetailViewData = (List<OAP0020ViewModel>)Cache.Get(CacheList.OAP0020DetailViewData);
                    return Json(jdata.modelToJqgridResult(OAP0020DetailViewData));
            }
            return null;
        }
    }
}