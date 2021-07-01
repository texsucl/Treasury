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
/// 功能說明：比對報表勾稽_查詢(OPEN跨系統勾稽)
/// 初版作者：20210505 Mark
/// 修改歷程：20210505 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0105QController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        private IORT0105 ORT0105 { get; set; }

        public ORT0105QController()
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0105Q/");
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
                new Service.Actual.Common().GetSysCode("RT", "GOJ_TYPE", true, false), "Value", "Text");

            ViewBag.MAIL_GROUP = new SelectList(
                new Service.Actual.Common().GetSysCode("RT", "MAIL_GROUP", false, true), "Value", "Text");

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
        public JsonResult qryORT0105Q(string type, string kind)
        {
            logger.Info("qryORT0105Q Query!!");
            MSGReturnModel result = new MSGReturnModel();
            result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            try
            {
                return Json(searchORT0105Q(type, kind));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }

        public JsonResult GetGOJ_TYPE_GROUP(string type)
        {
            if (type.IsNullOrWhiteSpace() || (type == "All"))
                return Json(new List<SelectOption>() { new SelectOption() { Text = "All", Value = "All" } });
            else
                return Json(new Service.Actual.Common().GetSysCode("RT", $@"GOJ_TYPE_{type}_GROUP", true, false));
        }

        private MSGReturnModel searchORT0105Q(string type, string kind)
        {
            MSGReturnModel result = new MSGReturnModel();
            var _result = ORT0105.GetSearchData(type, kind);
            if (_result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ORT0105QViewData);
                Cache.Set(CacheList.ORT0105QViewData, _result.Datas);
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
                case "ORT0105QViewData":
                    return Json(jdata.modelToJqgridResult(((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105QViewData))
                        ?.OrderBy(x => x.type).ThenBy(x => x.kind).ToList()));
                case "ORT0105QViewSubData":
                    var ORT0105AViewSubData = (List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105QViewData);
                    return Json(jdata.modelToJqgridResult(((List<ORT0105ViewModel>)Cache.Get(CacheList.ORT0105QViewData))
                        .First(x => x.check_id == check_id).subDatas
                        ?.OrderBy(x => x.platform).ThenBy(x => x.file_code).ToList()));
            }
            return null;
        }
    }
}