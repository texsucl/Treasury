using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Treasury.Web.Controllers;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 股票
/// 初版作者：20180709 侯蔚鑫
/// 修改歷程：20180709 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
/// 
namespace Treasury.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class StockController : CommonController
    {
        // GET: Stock
        private IStock Stock;
        private ITreasuryAccess TreasuryAccess;

        public StockController()
        {
            Stock = new Stock();
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 股票 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data)
        {
            ViewBag.dStock_Area_Type = new SelectList(Stock.GetAreaType(), "Value", "Text");
            ViewBag.dStock_Type = new SelectList(Stock.GetStockType(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;
            ViewBag.dActType = AplyNo.IsNullOrWhiteSpace();

            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (data.vAccessType == AccessProjectTradeType.P.ToString())
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(), "Value", "Text");
                }
                else if (data.vAccessType == AccessProjectTradeType.G.ToString())
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(data.vAplyUnit), "Value", "Text");
                }

                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetStockViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                //resetBillViewModel(null, AplyNo);
                ViewBag.TAR = TreasuryAccess.GetByAplyNo(AplyNo);
            }

            return PartialView();
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <param name="AccessType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData(string AccessType)
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData) && Cache.IsSet(CacheList.StockTempData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    result = Stock.ApplyAudit((List<StockViewModel>)Cache.Get(CacheList.StockTempData), data);
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    result = Stock.ApplyAudit((List<StockViewModel>)Cache.Get(CacheList.StockMainData), data);
                }
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 取得在庫股票
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetItemBook(int groupNo, bool accessType)
        {
            MSGReturnModel<StockViewModel> result = new MSGReturnModel<StockViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (groupNo == 0 && Cache.IsSet(CacheList.StockMainData))
            {
                var data = (StockViewModel)Cache.Get(CacheList.StockMainData);
                result.RETURN_FLAG = true;
            }
            else if (groupNo == -1)
            {
                Cache.Invalidate(CacheList.StockMainData);
                Cache.Set(CacheList.StockMainData, new List<StockViewModel>());
                result.RETURN_FLAG = false;
            }
            else
            {
                if (Cache.IsSet(CacheList.TreasuryAccessViewData) && accessType)
                {
                    TreasuryAccessViewModel viewdata = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data = Stock.GetDataByGroupNo(groupNo, viewdata.vAplyUnit);
                    Cache.Invalidate(CacheList.StockMainData);
                    Cache.Set(CacheList.StockMainData, _data);
                    result.RETURN_FLAG = true;
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 取得在庫股票明細
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStockDetailDate(int groupNo, int treaBatchNo)
        {
            MSGReturnModel<StockViewModel> result = new MSGReturnModel<StockViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();

            var _data = Stock.GetDetailData(groupNo, treaBatchNo);
            Cache.Invalidate(CacheList.StockTempData);
            Cache.Set(CacheList.StockTempData, _data);
            result.RETURN_FLAG = true;

            return Json(result);
        }

        /// <summary>
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(StockViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockViewModel>)Cache.Get(CacheList.StockTempData);
                model.vStatus = AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.StockTempData);
                Cache.Set(CacheList.StockTempData, setStockViewRowNum(tempData));
                result.RETURN_FLAG = true;
                result.DESCRIPTION = MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(StockViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockViewModel>)Cache.Get(CacheList.StockTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStockType = model.vStockType;
                    updateTempData.vStockNoPreamble = model.vStockNoPreamble;
                    updateTempData.vStockNoB = model.vStockNoB;
                    updateTempData.vStockNoE = model.vStockNoE;
                    updateTempData.vStockTotal = model.vStockTotal;
                    updateTempData.vDenomination = model.vDenomination;
                    updateTempData.vDenominationTotal = model.vDenominationTotal;
                    updateTempData.vNumberOfShares = model.vNumberOfShares;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.StockTempData);
                    Cache.Set(CacheList.StockTempData, setStockViewRowNum(tempData));
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.update_Fail.GetDescription();
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
        public JsonResult DeleteTempData(StockViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockViewModel>)Cache.Get(CacheList.StockTempData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.StockTempData);
                    Cache.Set(CacheList.StockTempData, setStockViewRowNum(tempData));
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.delete_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.delete_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 取出事件動作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TakeOutData(StockViewModel model, bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockMainData))
            {
                var tempData = (List<StockViewModel>)Cache.Get(CacheList.StockMainData);
                var updateTempData = tempData.FirstOrDefault(x => x.vTreaBatchNo == model.vTreaBatchNo);
                if (updateTempData != null)
                {
                    if (takeoutFlag)
                    {
                        updateTempData.vStatus = AccessInventoryType._4.GetDescription();
                    }
                    else
                    {
                        updateTempData.vStatus = AccessInventoryType._1.GetDescription();
                    }
                    updateTempData.vTakeoutFlag = takeoutFlag;
                    updateTempData.vStockDate = model.vStockDate;
                    Cache.Invalidate(CacheList.StockMainData);
                    Cache.Set(CacheList.StockMainData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData(string AccessType)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            resetStockViewModel(AccessType);
            return Json(result);
        }

        /// <summary>
        /// 取得股票資料
        /// </summary>
        /// <param name="GroupNo">股票群組編號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStockDate(string GroupNo)
        {
            var result = Stock.GetStockDate(int.Parse(GroupNo));
            
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
                case "Temp":
                    if (Cache.IsSet(CacheList.StockTempData))
                        return Json(jdata.modelToJqgridResult(setStockViewModelOrder((List<StockViewModel>)Cache.Get(CacheList.StockTempData))));
                    break;
                case "Stock":
                    if (Cache.IsSet(CacheList.StockMainData))
                        return Json(jdata.modelToJqgridResult((List<StockViewModel>)Cache.Get(CacheList.StockMainData)));
                    break;
            }
            return null;
        }

        private void resetStockViewModel(string AccessType, string AplyNo = null)
        {
            Cache.Invalidate(CacheList.StockMainData);
            Cache.Invalidate(CacheList.StockTempData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.StockMainData, new List<StockViewModel>());
                Cache.Set(CacheList.StockTempData, new List<StockViewModel>());
            }
            else
            {

            }

        }

        private List<StockViewModel> setStockViewModelOrder(List<StockViewModel> data)
        {
            if (data.Any())
            {
                data = data.OrderBy(x => x.vRowNum).ToList();
            }
            return data;
        }

        private List<StockViewModel> setStockViewRowNum(List<StockViewModel> data)
        {
            int rownum = 1;
            data.OrderBy(x => x.vStockNoPreamble)
                .ThenBy(x => x.vStockType).ToList()
                .ForEach(x =>
                {
                    x.vRowNum = rownum.ToString();
                    rownum += 1;
                });
            return data;
        }

        /// <summary>
        /// 計算總股數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Number_Of_Shares_Total()
        {
            var tempData = (List<StockViewModel>)Cache.Get(CacheList.StockTempData);

            int result = 0;

            if(tempData.Count==0)
            {
                result = 0;
            }
            else
            {
                tempData.ForEach(x =>
                {
                    //有股數才加總
                    if (x.vNumberOfShares != null)
                        result += (int)x.vNumberOfShares;
                });
            }

            return Json(result);
        }
    }
}