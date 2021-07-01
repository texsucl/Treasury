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
    public class OAP0019Controller : CommonController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IOAP0019 OAP0019;
        public OAP0019Controller()
        {
            OAP0019 = new OAP0019();
        }
        // GET: OAP0019
        public ActionResult Index()
        {
            UserAuthUtil authUtil = new UserAuthUtil();

            string opScope = "";
            string funcName = "";
            string[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/OAP0019/");
            if (roleInfo != null && roleInfo.Length == 2)
            {
                opScope = roleInfo[0];
                funcName = roleInfo[1];
            }

            ViewBag.funcName = funcName;
            ViewBag.opScope = opScope;

            return View();
        }

        /// <summary>
        /// 表單查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(OAP0019SearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                result.DESCRIPTION = Search(searchModel);
                DateTime now = DateTime.Now;

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

        private string Search(OAP0019SearchViewModel searchData = null)
        {
            var result = string.Empty;
            if (searchData == null)
                searchData = (OAP0019SearchViewModel)Cache.Get(CacheList.OAP0019SearchData);

            Cache.Invalidate(CacheList.OAP0019SearchData);
            Cache.Set(CacheList.OAP0019SearchData, searchData);
            var datas = OAP0019.GetSearchData(searchData);
            Cache.Invalidate(CacheList.OAP0019ViewData);
            Cache.Set(CacheList.OAP0019ViewData, datas);
            if (!datas.Any())
                result = MessageType.query_Not_Find.GetDescription();
            return result;
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

        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Search":
                    var SearchHisViewData = (List<OAP0019ViewModel>)Cache.Get(CacheList.OAP0019ViewData);
                    return Json(jdata.modelToJqgridResult(SearchHisViewData.OrderBy(x => x.apply_time).ToList(), false));
            }
            return null;
        }
    }
}