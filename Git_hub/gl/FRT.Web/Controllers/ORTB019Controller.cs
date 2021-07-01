using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.CacheProvider;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// 功能說明：快速付款人工比對作業
/// 初版作者：20190220 Mark
/// 修改歷程：20190220 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB019Controller : BaseController
    {
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal ICacheProvider Cache { get; set; }

        public ORTB019Controller()
        {
            Cache = new DefaultCacheProvider();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB019/");
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
        /// 查詢"FRT_WORD"資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryORTB019(string fastNo_S, string fastNo_E)
        {
            logger.Info("qryORTB019 Query!!");
            MSGReturnModel<List<FBOModel>> result = new MSGReturnModel<List<FBOModel>>();
            result = new FRTFBODao().qryForORTB019(fastNo_S, fastNo_E);
            if (result.RETURN_FLAG)
            {
                PIA_LOG_MAIN piaLogMain = new PIA_LOG_MAIN();
                PiaLogMainDao piaLogMainDao = new PiaLogMainDao();

                piaLogMain = new PIA_LOG_MAIN();
                piaLogMain.TRACKING_TYPE = "A";
                piaLogMain.ACCESS_ACCOUNT = Session["UserID"].ToString();
                piaLogMain.ACCOUNT_NAME = "";
                piaLogMain.PROGFUN_NAME = "ORTB019Controller";
                piaLogMain.EXECUTION_CONTENT = $@"fastNo_S:{fastNo_S}|fastNo_E:{fastNo_E}";
                piaLogMain.AFFECT_ROWS = result.Datas.Count;
                piaLogMain.PIA_TYPE = "0000000000";
                piaLogMain.EXECUTION_TYPE = "Q";
                piaLogMain.ACCESSOBJ_NAME = "FRTBARM";
                piaLogMainDao.Insert(piaLogMain);

                Cache.Invalidate("qryORTB019");
                Cache.Set("qryORTB019", result.Datas);
                result.DESCRIPTION = $@"{result.Datas.Count().ToString().formateThousand()}/{result.Datas.Sum(x => x.REMIT_AMT.stringToDecimal()).ToString().formateThousand()}";
            }
            if (!result.RETURN_FLAG)
            {
                logger.Error(result.DESCRIPTION);
            }
            return Json(result);
        }

        /// <summary>
        /// 變更前端勾選狀態
        /// </summary>
        /// <param name="fastNo">快速付款編號</param>
        /// <param name="flag">勾選狀態</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ORTB019Check(string fastNo, bool flag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.DESCRIPTION = "閒置太久請重新登入!";
            if (Cache.IsSet("qryORTB019"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB019");
                var _data = data.FirstOrDefault(x => x.FAST_NO == fastNo);
                if (_data != null)
                {
                    _data.checkFlag = flag;
                    Cache.Invalidate("qryORTB019");
                    Cache.Set("qryORTB019", data);
                    var _checked = data.Where(x => x.checkFlag).ToList();
                    result.Datas = _checked.Any();
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = $@"{_checked.Count().ToString().formateThousand()}/{_checked.Sum(x => x.REMIT_AMT.stringToDecimal()).ToString().formateThousand()}";
                }
                else
                    result.DESCRIPTION = "其它錯誤，請洽系統管理員!!";
            }
            return Json(result);
        }

        /// <summary>
        /// 針對點選的快速付款編號執行動作 (CHK_MARK1)上”Y”
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ORTB019Update()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.DESCRIPTION = "閒置太久請重新登入!";
            try
            {
                if (Cache.IsSet("qryORTB019"))
                {
                    var data = ((List<FBOModel>)Cache.Get("qryORTB019")).Where(x => x.checkFlag).Select(x => x.FAST_NO).ToList();
                    if (!data.Any())
                    {
                        result.DESCRIPTION = "無修改資料";
                        return Json(result);
                    }
                    result = new FRTFBODao().updForORTB019(data);
                }
                return Json(result);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return Json(result);
            }
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet("qryORTB019"))
            {
                var data = (List<FBOModel>)Cache.Get("qryORTB019");
                return Json(jdata.modelToJqgridResult(data, true, new List<string>() { "BANK_CODE_SUB_BANK", "BANK_ACT", "EXPORT_NO", "FILLER_20" }));
            }
            return null;
        }
    }
}