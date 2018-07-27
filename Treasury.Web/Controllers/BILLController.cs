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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 空白票券
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
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
    public class BillController : CommonController
    {
        private IBill Bill;
        private ITreasuryAccess TreasuryAccess;

        public BillController()
        {
            Bill = new Bill();
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 空白票據 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data)
        {
            ViewBag.dBILL_Check_Type = new SelectList(Bill.GetCheckType(), "Value", "Text");
            var ibs = Bill.GetIssuing_Bank();
            ViewBag.dBILL_Issuing_Bank = new SelectList(ibs, "Value", "Text");
            var _dActType = false;           
            if (AplyNo.IsNullOrWhiteSpace())
            {
                _dActType = AplyNo.IsNullOrWhiteSpace();
                ViewBag.dAccess = null;
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                ResetBillViewModel(data.vAccessType);
            }
            else
            {
                _dActType = TreasuryAccess.GetActType(AplyNo,AccountController.CurrentUserId, Aply_Appr_Type);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                ViewBag.dAccess = viewModel.vAccessType;
                if (viewModel.vAccessType == AccessProjectTradeType.G.ToString())
                {
                    _dActType = false; ///空白票據 取出預設只能檢視
                }
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                ResetBillViewModel(viewModel.vAccessType, AplyNo);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData(string AplyNo)
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData) && Cache.IsSet(CacheList.BILLTempData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                result = Bill.ApplyAudit((List<BillViewModel>)Cache.Get(CacheList.BILLTempData), data);
                if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                {
                    new TreasuryAccessController().ResetSearchData();
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
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                model.vStatus = AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.BILLTempData);
                Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(tempData));
                var dayData = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                if (!data.vAplyNo.IsNullOrWhiteSpace())
                {
                    dayData = dayData.Where(x => x.vAplyNo != data.vAplyNo).ToList();
                }
                dayData.AddRange(tempData.ModelConvert<BillViewModel, BillViewModel>());
                Cache.Invalidate(CacheList.BILLDayData);
                Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(dayData));
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
        public JsonResult UpdateTempData(BillViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var dayData =  (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vIssuingBank = model.vIssuingBank;
                    updateTempData.vCheckType = model.vCheckType;
                    updateTempData.vCheckNoTrack = model.vCheckNoTrack;
                    updateTempData.vCheckNoB = model.vCheckNoB;
                    updateTempData.vCheckNoE = model.vCheckNoE;
                    updateTempData.vCheckTotalNum = model.vCheckTotalNum;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(tempData));
                    if (!data.vAplyNo.IsNullOrWhiteSpace())
                    {
                        dayData = dayData.Where(x => x.vAplyNo != data.vAplyNo).ToList();
                    }
                    dayData.AddRange(tempData.ModelConvert<BillViewModel, BillViewModel>());             
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(dayData));
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
        public JsonResult DeleteTempData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var dayData = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(tempData));
                    if (!data.vAplyNo.IsNullOrWhiteSpace())
                    {
                        dayData = dayData.Where(x => x.vAplyNo != data.vAplyNo).ToList();
                    }
                    dayData.AddRange(tempData.ModelConvert<BillViewModel,BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(dayData));
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
        public JsonResult TakeOutData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStatus = AccessInventoryType._4.GetDescription();
                    updateTempData.vTakeOutE = model.vTakeOutE;
                    updateTempData.vTakeOutTotalNum = model.vTakeOutTotalNum;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(tempData));
                    var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = GetOut(_data2);
                    _data2.AddRange(tempData.ModelConvert<BillViewModel, BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
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
        /// 重設事件動作(復原該筆庫存)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RepeatData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStatus = AccessInventoryType._1.GetDescription();
                    updateTempData.vTakeOutE = null;
                    updateTempData.vTakeOutTotalNum = null;
                    updateTempData.vReMainTotalNum = null;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(tempData));
                    var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = GetOut(_data2);
                    _data2.AddRange(tempData.ModelConvert<BillViewModel,BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
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
            ResetBillViewModel(AccessType);
            return Json(new MSGReturnModel<string>());
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
                    if (Cache.IsSet(CacheList.BILLTempData))
                        return Json(jdata.modelToJqgridResult(SetBillViewModelOrder((List<BillViewModel>)Cache.Get(CacheList.BILLTempData))));
                    break;
                case "Day":
                    if (Cache.IsSet(CacheList.BILLDayData))
                        return Json(jdata.modelToJqgridResult((List<BillViewModel>)Cache.Get(CacheList.BILLDayData)));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 重設空白票券
        /// </summary>
        /// <param name="AccessType"></param>
        /// <param name="AplyNo"></param>
        private void ResetBillViewModel(string AccessType,string AplyNo = null)
        {
            Cache.Invalidate(CacheList.BILLTempData);
            Cache.Invalidate(CacheList.BILLDayData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, new List<BillViewModel>());
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup((List<BillViewModel>)Bill.GetDayData(data.vAplyUnit)));                   
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    var _data = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit, "1");//只抓庫存
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = GetOut(_data2);
                    _data2.AddRange(_data.ModelConvert<BillViewModel, BillViewModel>());
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(_data));
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
                }
            }
            else
            {
                var _data = (List<BillViewModel>)Bill.GetTempData(AplyNo);
                var _data2 = (List<BillViewModel>)Bill.GetDayData(null,null,AplyNo);
                var _AccessType = TreasuryAccess.GetAccessType(AplyNo);
                if (_AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(_data));
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
                }
                if (_AccessType == AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(_data));
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
                }
            }

        }

        /// <summary>
        /// 取出過濾在庫資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<BillViewModel> GetOut(List<BillViewModel> data)
        {
            return data.Where(x => x.vStatus != AccessInventoryType._1.GetDescription()).ToList();
        }

        /// <summary>
        /// 輸出資料排序
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<BillViewModel> SetBillViewModelOrder(List<BillViewModel> data)
        {
            if (data.Any())
            {
                data = data.OrderBy(x => x.vRowNum).ToList();
            }
            return data;
        }

        /// <summary>
        /// 加入排序
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<BillViewModel> SetBillViewRowNum(List<BillViewModel> data)
        {
            int rownum = 1;
            data.OrderBy(x => x.vIssuingBank)
                .ThenBy(x => x.vCheckType).ToList()
                .ForEach(x =>
            {
                x.vRowNum = rownum.ToString();
                rownum += 1;
            });
            return data;
        }

        /// <summary>
        /// 加入 小計&總計
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<BillViewModel> SetBillTakeOutViewModelGroup(List<BillViewModel> data)
        {
            var result = new List<BillViewModel>();
            if (data.Any())
            {
                int _vCheckTotalNum = 0;
                int _intReMainTotalNum = 0;
                int rownum = 1;
                data = data.OrderBy(x => x.vIssuingBank)
                    .ThenBy(x => x.vCheckType)
                    .ToList();

                data.ForEach(x =>
                    {
                        x.vRowNum = rownum.ToString();
                        if (x.vStatus == AccessInventoryType._3.GetDescription())
                        {
                            x.vReMainTotalNum = x.vCheckTotalNum;
                            _intReMainTotalNum += TypeTransfer.stringToInt(x.vReMainTotalNum);
                            x.vCheckTotalNum = "";
                        }
                        else
                        {
                            var _vReMainTotalNum =
                            (x.vStatus == AccessInventoryType._1.GetDescription() || !x.vTakeOutE.IsNullOrWhiteSpace()) ?
                            TypeTransfer.stringToInt(x.vCheckTotalNum) - TypeTransfer.stringToInt(x.vTakeOutTotalNum) : 0;
                            _intReMainTotalNum += _vReMainTotalNum;
                            x.vReMainTotalNum = _vReMainTotalNum == 0 ? "" : _vReMainTotalNum.ToString();
                        }

                        rownum += 1;
                    });

                data.GroupBy(x => new { x.vIssuingBank, x.vCheckType })
                    .OrderBy(x => x.Key.vIssuingBank)
                    .ThenBy(x => x.Key.vCheckType)
                    .ToList()
                    .ForEach(x =>
                {
                    result.AddRange(x);
                    //資料欄位狀態文字為未包含'取出'的資料 - 料欄位狀態文字為包含'取出'的資料
                    var _groupvCheckTotalNum = x.Sum(y => TypeTransfer.stringToInt(y.vCheckTotalNum));
                    //var _groupvCheckTotalNum =
                    //(x.Where(y => !y.vStatus.Contains(AccessProjectTradeType.G.GetDescription()) || !y.vTakeOutE.IsNullOrWhiteSpace())
                    //  .Sum(y => TypeTransfer.stringToInt(y.vCheckTotalNum)) -
                    // x.Where(y => y.vStatus.Contains(AccessProjectTradeType.G.GetDescription()) && y.vTakeOutE.IsNullOrWhiteSpace())
                    //  .Sum(y => TypeTransfer.stringToInt(y.vCheckTotalNum)));
                    var _group = new BillViewModel()
                    {
                        vStatus = "小計",
                        vIssuingBank = x.Key.vIssuingBank,
                        vCheckType = x.Key.vCheckType,
                        vCheckTotalNum = _groupvCheckTotalNum.ToString(),
                        vReMainTotalNum = x.Sum(y => TypeTransfer.stringToInt(y.vReMainTotalNum)).ToString()
                    };
                    _vCheckTotalNum += _groupvCheckTotalNum;
                    result.Add(_group);
                });
                result.Add(new BillViewModel()
                {
                    vStatus = "總計",
                    vCheckTotalNum = _vCheckTotalNum.ToString(),
                    vReMainTotalNum = _intReMainTotalNum.ToString()
                });
            }
            return result;
        }
    }
}