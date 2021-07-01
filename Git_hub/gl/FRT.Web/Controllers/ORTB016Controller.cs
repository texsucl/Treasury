using FRT.Web.ActionFilter;
using FRT.Web.BO;
using FRT.Web.Daos;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using FRT.Web.CacheProvider;
using System.Web.Mvc;
using FRT.Web.Enum;

/// <summary>
/// 功能說明：存摺顯示字樣設定作業
/// 初版作者：20190130 Mark
/// 修改歷程：20190130 Mark
///           需求單號：
///           初版
/// </summary>
///

namespace FRT.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ORTB016Controller : BaseController
    {
        internal ICacheProvider Cache { get; set; }
        static private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        FRTWordDao fRTWordDao = null;
        public ORTB016Controller() {
            fRTWordDao = new FRTWordDao();
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
                String[] roleInfo = authUtil.chkUserFuncAuth(Session["UserID"].ToString(), "~/ORTB016/");
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

            return View();
        }

        /// <summary>
        /// 查詢"FRT_WORD"資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryFRTWord()
        {
            logger.Info("qryFRTWord begin!!");
            MSGReturnModel<List<ORTB016Model>> result = new MSGReturnModel<List<ORTB016Model>>();
            try
            {
                result = fRTWordDao.qryForORTB016();
                Cache.Invalidate(CacheList.ORTB016ViewData);
                Cache.Set(CacheList.ORTB016ViewData, result.Datas);
            }
            catch (Exception e)
            {
                logger.Error($@"qryFRTWord Error : {e.ToString()}");
                result.DESCRIPTION = Ref.MessageType.sys_Error.GetDescription();
                result.RETURN_FLAG = false;
            }
            return Json(result);
        }

        /// <summary>
        /// 畫面執行"申請覆核"
        /// </summary>
        /// <param name="code"></param>
        /// <param name="gridData"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult execSave(List<ORTB016Model> gridData)
        {
            logger.Info("ORTB016 execSave begin");
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            try
            {
                /*------------------ DB處理   begin------------------*/
             
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
                if (Cache.IsSet(CacheList.ORTB016ViewData))
                {
                    var tempData = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                    tempData = tempData.Where(x => x.status != null).ToList();
                    tempData.ForEach(x =>
                    {
                        x.frt_sys_type = x.frt_sys_type?.Trim() ?? string.Empty;
                        x.frt_srce_from = x.frt_srce_from?.Trim() ?? string.Empty;
                        x.frt_srce_kind = x.frt_srce_kind?.Trim() ?? string.Empty;
                        x.frt_memo_apx = x.frt_memo_apx?.Trim() ?? string.Empty;
                        x.frt_achcode = x.frt_achcode?.Trim() ?? string.Empty;
                        x.updId = Session["UserID"]?.ToString();
                    });
                    result = fRTWordDao.updateORTB016(tempData);

                }
            }
            catch (Exception e)
            {
                logger.Error($@"ORTB016 execSave Error : {e.ToString()} ");
                result.DESCRIPTION = Ref.MessageType.sys_Error.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(ORTB016Model model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORTB016ViewData))
            {
                var tempData = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                model.status = "A";
                tempData.Add(model);
                Cache.Invalidate(CacheList.ORTB016ViewData);
                Cache.Set(CacheList.ORTB016ViewData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(ORTB016Model model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORTB016ViewData))
            {
                var tempData = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.frt_word_Id == model.frt_word_Id);
                if (updateTempData != null)
                {
                    if(updateTempData.status.IsNullOrWhiteSpace() || (updateTempData.status == "D"))
                        updateTempData.status = "U";
                    updateTempData.frt_sys_type = model.frt_sys_type;
                    updateTempData.frt_srce_from = model.frt_srce_from;
                    updateTempData.frt_srce_kind = model.frt_srce_kind;
                    updateTempData.frt_memo_apx = model.frt_memo_apx;
                    updateTempData.frt_achcode = model.frt_achcode;
                    Cache.Invalidate(CacheList.ORTB016ViewData);
                    Cache.Set(CacheList.ORTB016ViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 刪除明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(ORTB016Model model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ORTB016ViewData))
            {
                var tempData = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                var deleteTempData = tempData.FirstOrDefault(x => x.frt_word_Id == model.frt_word_Id);
                if (deleteTempData != null)
                {
                    if (deleteTempData.status == "A")
                        tempData.Remove(deleteTempData);
                    else
                        deleteTempData.status = "D";
                    Cache.Invalidate(CacheList.ORTB016ViewData);
                    Cache.Set(CacheList.ORTB016ViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                    result.Datas = tempData.Any();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 清除明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            if (Cache.IsSet(CacheList.ORTB016ViewData))
            {
                var tempData = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                tempData = new List<ORTB016Model>();
                Cache.Invalidate(CacheList.ORTB016ViewData);
                Cache.Set(CacheList.ORTB016ViewData, tempData);
            }
            return Json(result);
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.ORTB016ViewData))
            {
                var data = (List<ORTB016Model>)Cache.Get(CacheList.ORTB016ViewData);
                return Json(jdata.modelToJqgridResult(data));
            }
            return null;
        }
    }
}