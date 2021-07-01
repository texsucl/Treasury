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
/// 功能說明：跨系統資料庫勾稽銀存銷帳不比對帳號查詢
/// 初版作者：20210518 Mark
/// 修改歷程：20210518 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORT0109QController : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        private IORT0109 ORT0109 { get; set; }

        public ORT0109QController()
        {
            Cache = new DefaultCacheProvider();
            ORT0109 = new ORT0109();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORT0109Q/");
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

            return View();
        }

        /// <summary>
        /// 查詢 資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORT0109Q(string bank_acct_no = null)
        {
            logger.Info("qryORT0109 Query!!");
            MSGReturnModel result = new MSGReturnModel();
            result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            try
            {
                return Json(searchORT0109Q(bank_acct_no));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }


        private MSGReturnModel searchORT0109Q(string bank_acct_no = null)
        {
            MSGReturnModel result = new MSGReturnModel();
            var _result = ORT0109.GetSearchData(bank_acct_no);
            if (_result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ORT0109ViewData);
                Cache.Set(CacheList.ORT0109ViewData, _result.Datas);
            }
            result.RETURN_FLAG = _result.RETURN_FLAG;
            result.DESCRIPTION = _result.DESCRIPTION;
            return result;
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "ORT0109QModel":
                    return Json(jdata.modelToJqgridResult(((List<ORT0109ViewModel>)Cache.Get(CacheList.ORT0109ViewData))
                        ?.OrderBy(x => x.bank_acct_no).ToList()));
            }
            return null;
        }
    }
}