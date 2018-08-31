using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Treasury.Web.Controllers;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;
using Treasury.Web.Enum;

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
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class StockController : CommonController
    {
        // GET: Stock
        private IStock Stock;

        public StockController()
        {
            Stock = new Stock();
        }

        /// <summary>
        /// 股票 新增畫面
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">金庫物品存取主畫面ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.dStock_Area_Type = new SelectList(Stock.GetAreaType(), "Value", "Text");
            ViewBag.dStock_Type = new SelectList(Stock.GetStockType(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;

            var _dActType = GetActType(type, AplyNo);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (data.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(), "Value", "Text");
                }
                else if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(data.vAplyUnit), "Value", "Text");
                }

                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetStockViewModel();
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);

                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                if (viewModel.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(), "Value", "Text");
                }
                else if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString() && _dActType)
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(viewModel.vAplyUnit, AplyNo), "Value", "Text");
                }
                else if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_dActType)
                {
                    ViewBag.dStock_Name = new SelectList(Stock.GetStockName(viewModel.vAplyUnit), "Value", "Text");
                }
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetStockViewModel(AplyNo);
                var _data = (StockViewModel)Cache.Get(CacheList.StockData);
                ViewBag.group = _data.vStockDate.GroupNo;

            }

            ViewBag.dActType = _dActType;

            return PartialView();
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <param name="vStockDate">股票資料</param>
        /// <param name="vStockModel">股票模型</param>
        /// <param name="AccessType">申請作業</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData(ItemBookStock vStockDate, ItemBookStockModel vStockModel, string AccessType)
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData) && Cache.IsSet(CacheList.StockData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (StockViewModel)Cache.Get(CacheList.StockData);
                _data.vStockDate = vStockDate;
                _data.vStockModel = vStockModel;
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    _data.vDetail = (List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData);
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    //判斷至少勾選一筆資料
                    var vDetail = ((List<StockDetailViewModel>)Cache.Get(CacheList.StockMainData)).Where(x => x.vTakeoutFlag == true).ToList();
                    if(vDetail.Any())
                    {
                        _data.vDetail = (List<StockDetailViewModel>)Cache.Get(CacheList.StockMainData);
                    }
                    else
                    {
                        _data.vDetail = new List<StockDetailViewModel>();
                    }
                }

                List<StockViewModel> _datas = new List<StockViewModel>();
                _datas.Add(_data);
                result = Stock.ApplyAudit(_datas, data);
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 取得在庫股票
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetItemBook(int groupNo, string aplyNo)
        {
            MSGReturnModel<StockDetailViewModel> result = new MSGReturnModel<StockDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (groupNo == 0 && Cache.IsSet(CacheList.StockMainData))
            {
                var data = (StockDetailViewModel)Cache.Get(CacheList.StockMainData);
                result.RETURN_FLAG = true;
            }
            else if (groupNo == -1)
            {
                Cache.Invalidate(CacheList.StockMainData);
                Cache.Set(CacheList.StockMainData, new List<StockDetailViewModel>());
                result.RETURN_FLAG = false;
            }
            else
            {
                if (Cache.IsSet(CacheList.TreasuryAccessViewData))
                {
                    TreasuryAccessViewModel viewdata = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data = Stock.GetDataByGroupNo(groupNo, viewdata.vAplyUnit, aplyNo);
                    Cache.Invalidate(CacheList.StockMainData);
                    Cache.Invalidate(CacheList.StockTempData);
                    Cache.Set(CacheList.StockMainData, _data);
                    Cache.Set(CacheList.StockTempData, new List<StockDetailViewModel>());
                    result.RETURN_FLAG = true;
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 取得在庫股票明細
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStockDetailDate(int groupNo, int treaBatchNo)
        {
            MSGReturnModel<StockDetailViewModel> result = new MSGReturnModel<StockDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            var _data = Stock.GetDetailData(groupNo, treaBatchNo);
            Cache.Invalidate(CacheList.StockTempData);
            Cache.Set(CacheList.StockTempData, _data);
            result.RETURN_FLAG = true;

            return Json(result);
        }

        /// <summary>
        /// 新增明細資料
        /// </summary>
        /// <param name="model">股票明細資料</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(StockDetailViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.StockTempData);
                Cache.Set(CacheList.StockTempData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改明細資料
        /// </summary>
        /// <param name="model">股票明細資料</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(StockDetailViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStockType = model.vStockType;
                    updateTempData.vStockNoPreamble = model.vStockNoPreamble;
                    updateTempData.vStockNoB = model.vStockNoB;
                    updateTempData.vStockNoE = model.vStockNoE;
                    updateTempData.vStockTotal = model.vStockTotal;
                    updateTempData.vAmount_Per_Share = model.vAmount_Per_Share;
                    updateTempData.vSingle_Number_Of_Shares = model.vSingle_Number_Of_Shares;
                    updateTempData.vDenomination = model.vDenomination;
                    updateTempData.vDenominationTotal = model.vDenominationTotal;
                    updateTempData.vNumberOfShares = model.vNumberOfShares;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.StockTempData);
                    Cache.Set(CacheList.StockTempData, tempData);
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
        /// <param name="model">股票明細資料</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(StockDetailViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.StockTempData);
                    Cache.Set(CacheList.StockTempData, tempData);
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
        /// 取出事件動作
        /// </summary>
        /// <param name="model">股票明細資料</param>
        /// <param name="takeoutFlag">取出註記</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TakeOutData(StockDetailViewModel model, bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.StockMainData))
            {
                var tempData = (List<StockDetailViewModel>)Cache.Get(CacheList.StockMainData);
                var updateTempData = tempData.FirstOrDefault(x => x.vTreaBatchNo == model.vTreaBatchNo);
                if (updateTempData != null)
                {
                    if (takeoutFlag)
                    {
                        updateTempData.vStatus = Ref.AccessInventoryType._4.GetDescription();
                    }
                    else
                    {
                        updateTempData.vStatus = Ref.AccessInventoryType._1.GetDescription();
                    }
                    updateTempData.vTakeoutFlag = takeoutFlag;
                    Cache.Invalidate(CacheList.StockMainData);
                    Cache.Set(CacheList.StockMainData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.vTakeoutFlag);
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
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            resetStockViewModel();
            return Json(result);
        }

        /// <summary>
        /// 取得股票資料
        /// </summary>
        /// <param name="GroupNo">股票群組編號</param>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStockDate(string GroupNo, string vAplyNo = null)
        {
            var result = Stock.GetStockDate(int.Parse(GroupNo), vAplyNo);
            
            return Json(result);
        }

        /// <summary>
        /// 取得在庫股票明細
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStockCheck(string StockName)
        {
            MSGReturnModel<List<ItemBookStock>> result = new MSGReturnModel<List<ItemBookStock>>();
            
            try
            {
                result.Datas = Stock.GetStockCheck(StockName);
                result.RETURN_FLAG = true;
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
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
                case "Temp":
                    if (Cache.IsSet(CacheList.StockTempData))
                        return Json(jdata.modelToJqgridResult(((List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData)).OrderBy(x=>x.vItemId).ToList()));
                    break;
                case "Stock":
                    if (Cache.IsSet(CacheList.StockMainData))
                        return Json(jdata.modelToJqgridResult(((List<StockDetailViewModel>)Cache.Get(CacheList.StockMainData)).OrderBy(x=>x.vTreaBatchNo).ToList()));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 設定股票Cache資料
        /// </summary>
        /// <param name="GroupNo">申請單號</param>
        /// <param name="GroupNo">修改狀態</param>
        /// <returns></returns>
        private void resetStockViewModel(string AplyNo = null, bool EditFlag = false)
        {
            Cache.Invalidate(CacheList.StockData);
            Cache.Invalidate(CacheList.StockMainData);
            Cache.Invalidate(CacheList.StockTempData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Set(CacheList.StockData, new StockViewModel());
                Cache.Set(CacheList.StockMainData, new List<StockDetailViewModel>());
                Cache.Set(CacheList.StockTempData, new List<StockDetailViewModel>());
            }
            else
            {
                var data = Stock.GetDataByAplyNo(AplyNo, EditFlag);
                Cache.Set(CacheList.StockData, data);

                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                if (viewModel.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.StockTempData, data.vDetail);
                }
                else if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.StockMainData, data.vDetail);
                    Cache.Set(CacheList.StockTempData, new List<StockDetailViewModel>());
                }
            }

        }

        /// <summary>
        /// 計算總股數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Number_Of_Shares_Total()
        {
            var tempData = (List<StockDetailViewModel>)Cache.Get(CacheList.StockTempData);

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