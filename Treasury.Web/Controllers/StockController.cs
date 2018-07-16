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
            ViewBag.tStock_No = Stock.GetMaxStockNo() + 1;
            ViewBag.dStock_Name = new SelectList(Stock.GetStockName(), "Value", "Text");
            ViewBag.dStock_Area_Type = new SelectList(Stock.GetAreaType(), "Value", "Text");
            ViewBag.dStock_Type = new SelectList(Stock.GetStockType(), "Value", "Text");

            ViewBag.CustodianFlag = AccountController.CustodianFlag;
            ViewBag.dActType = AplyNo.IsNullOrWhiteSpace();
            if (AplyNo.IsNullOrWhiteSpace())
            {
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
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData) && Cache.IsSet(CacheList.BILLTempData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                result = Stock.ApplyAudit((List<StockViewModel>)Cache.Get(CacheList.StockTempData), data);
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            }
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
                model.vStatus = AccessInventoryTyp._3.GetDescription();
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
                    updateTempData.vDenomination_Total = model.vDenomination_Total;
                    updateTempData.vNumber_Of_Shares = model.vNumber_Of_Shares;
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
                case "Day":
                    if (Cache.IsSet(CacheList.BILLDayData))
                        return Json(jdata.modelToJqgridResult((List<StockViewModel>)Cache.Get(CacheList.BILLDayData)));
                    break;
            }
            return null;
        }

        private void resetStockViewModel(string AccessType, string AplyNo = null)
        {
            Cache.Invalidate(CacheList.StockTempData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.StockTempData, new List<StockViewModel>());
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    //var _data = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit, "1");//只抓庫存
                    //var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    //_data2 = getOut(_data2);
                    //_data2.AddRange(_data.ModelConvert<BillViewModel, BillViewModel>());
                    //Cache.Set(CacheList.BILLTempData, setBillViewRowNum(_data));
                    //Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                }
            }
            else
            {
                //var _data = (List<StockViewModel>)Stock.GetTempData(AplyNo);
                //var _data2 = (List<BillViewModel>)Bill.GetDayData(null, null, AplyNo);
                //var _AccessType = TreasuryAccess.GetAccessType(AplyNo);
                //if (_AccessType == AccessProjectTradeType.P.ToString())
                //{
                //    Cache.Set(CacheList.StockTempData, setStockViewRowNum(_data));
                //    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                //}
                //if (_AccessType == AccessProjectTradeType.G.ToString())
                //{
                //    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(_data));
                //    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                //}
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

    }
}