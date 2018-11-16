using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Enum;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-mail發送時間設定檔維護作業
/// 初版作者：20181109 張家華
/// 修改歷程：20181109 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class TreasuryMailTimeController : CommonController
    {
        // GET: TreasuryMaintain
        private ITreasuryMailTime TreasuryMailTime;

        public TreasuryMailTimeController()
        {
            TreasuryMailTime = new TreasuryMailTime();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>	
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryMailTime/");
            ViewBag.dMAIL_ID = new SelectList(new TreasuryMailContent().Get_MAIL_ID(false,true), "Value", "Text");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", false), "Value", "Text");
            return View();
        }

        /// <summary>
        /// mail發送時間設定異動紀錄查詢畫面
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">異動紀錄查詢ViewModel</param>
        /// <param name="type">呼叫原始書面</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecord()
        {
            ViewBag.dAppr_Status = new SelectList(new Service.Actual.Common().GetSysCode("APPR_STATUS", true), "Value", "Text");
            return PartialView();
        }

        /// <summary>
        /// mail發送時間設定異動紀錄
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">查詢ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecordView(string AplyNo, TreasuryMailTimeSearchViewModel data)
        {
            var _data = TreasuryMailTime.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.TreasuryMailTimeChangeRecordData);
            Cache.Set(CacheList.TreasuryMailTimeChangeRecordData, _data);
            return PartialView();
        }

        /// <summary>
        /// mail發送時間設定查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            var datas = TreasuryMailTime.GetSearchData(new TreasuryMailTimeSearchViewModel());
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryMailTimeData);
                Cache.Set(CacheList.TreasuryMailTimeData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITinItem>> result = new MSGReturnModel<IEnumerable<ITinItem>>();
            if (Cache.IsSet(CacheList.TreasuryMailTimeData))
            {
                var _data = (List<TreasuryMailTimeViewModel>)Cache.Get(CacheList.TreasuryMailTimeData);
                result = TreasuryMailTime.TinApplyAudit(_data, new TreasuryMailTimeSearchViewModel() {userId = AccountController.CurrentUserId });
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 取消申請(復原tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CancelTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();           
            var datas = TreasuryMailTime.GetSearchData(new TreasuryMailTimeSearchViewModel());
            if (datas.Any())
            {
                result.RETURN_FLAG = true;
                Cache.Invalidate(CacheList.TreasuryMailTimeData);
                Cache.Set(CacheList.TreasuryMailTimeData, datas);         
            }
            return Json(result);
        }

        /// <summary>
        /// 修改mail發送時間資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(TreasuryMailTimeViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMailTimeData))
            {
                var tempData = (List<TreasuryMailTimeViewModel>)Cache.Get(CacheList.TreasuryMailTimeData);
                var updateTempData = tempData.FirstOrDefault(x => x.vMAIL_TIME_ID == model.vMAIL_TIME_ID);
                if (updateTempData != null)
                {
                    updateTempData.updateFlag = true;
                    updateTempData.vAction =  "修改";
                    updateTempData.vDATA_STATUS = "3";
                    updateTempData.vDATA_STATUS_NAME = "修改中";
                    updateTempData.vEXEC_TIME_B = model.vEXEC_TIME_B; //系統時間(起)
                    updateTempData.vEXEC_TIME_E = model.vEXEC_TIME_E; //系統時間(迄)
                    updateTempData.vSEND_TIME = model.vSEND_TIME; //發送時間
                    updateTempData.vINTERVAL_MIN = model.vINTERVAL_MIN; //間隔時間
                    updateTempData.vTREA_OPEN_TIME = model.vTREA_OPEN_TIME; //開庫時間
                    updateTempData.vFUNC_ID = model.vFUNC_ID; //程式編號(註解)
                    updateTempData.vMAIL_CONTENT_ID = model.vMAIL_CONTENT_ID; //內文編號
                    updateTempData.vMEMO = model.vMEMO; //備註
                    updateTempData.vIS_DISABLED = model.vIS_DISABLED;//停用註記
                    var _DISABLED = new Service.Actual.Common().GetSysCode("IS_DISABLED", false);
                    updateTempData.vIS_DISABLED_NAME = _DISABLED.FirstOrDefault(x => x.Value == model.vIS_DISABLED)?.Text;
                    Cache.Invalidate(CacheList.TreasuryMailTimeData);
                    Cache.Set(CacheList.TreasuryMailTimeData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = true;
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
        /// 重設mail發送時間資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult ReSetTempData(TreasuryMailTimeViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            var datas = (List<TreasuryMailTimeViewModel>)(TreasuryMailTime.GetSearchData(new TreasuryMailTimeSearchViewModel()));
            if (Cache.IsSet(CacheList.TreasuryMailTimeData))
            {
                var tempData = (List<TreasuryMailTimeViewModel>)Cache.Get(CacheList.TreasuryMailTimeData);
                var resetTempData = tempData.FirstOrDefault(x => x.vMAIL_TIME_ID == model.vMAIL_TIME_ID);
                var addTempData = datas.FirstOrDefault(x => x.vMAIL_TIME_ID == model.vMAIL_TIME_ID);
                if (resetTempData != null)
                {
                    tempData.Remove(resetTempData);
                    tempData.Add(addTempData);
                    tempData = tempData.OrderBy(x => x.vMAIL_TIME_ID).ToList();
                    Cache.Invalidate(CacheList.TreasuryMailTimeData);
                    Cache.Set(CacheList.TreasuryMailTimeData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.updateFlag);
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
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Index":
                    if (Cache.IsSet(CacheList.TreasuryMailTimeData))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMailTimeViewModel>)Cache.Get(CacheList.TreasuryMailTimeData))));
                    break;
                case "ChangeRecord":
                    if (Cache.IsSet(CacheList.TreasuryMailTimeChangeRecordData))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMailTimeHistoryViewModel>)Cache.Get(CacheList.TreasuryMailTimeChangeRecordData))));
                    break;
            }
            return null;
        }

    }
}