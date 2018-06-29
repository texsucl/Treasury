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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 不動產權狀
/// 初版作者：20180628 張家華
/// 修改歷程：20180628 張家華 
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
    public class EstateController : CommonController
    {
        private IBill Bill;
        private ITreasuryAccess TreasuryAccess;

        public EstateController()
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
            //ViewBag.dBILL_Check_Type = new SelectList(Bill.GetCheckType(), "Value", "Text");
            //var ibs = Bill.GetIssuing_Bank();
            //ViewBag.dBILL_Issuing_Bank = new SelectList(ibs, "Value", "Text");
            //ViewBag.dActType = AplyNo.IsNullOrWhiteSpace();
            //if (AplyNo.IsNullOrWhiteSpace())
            //{
            //    Cache.Invalidate(CacheList.TreasuryAccessViewData);
            //    Cache.Set(CacheList.TreasuryAccessViewData, data);
            //    resetBillViewModel(data.vAccessType);
            //}
            //else
            //{
            //    ViewBag.dAccess = Bill.GetAccessType(AplyNo);
            //    resetBillViewModel(null, AplyNo);
            //    ViewBag.TAR = TreasuryAccess.GetByAplyNo(AplyNo);
            //}
            return PartialView();
        }

        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                result = Bill.ApplyAudit((List<BillViewModel>)Cache.Get(CacheList.BILLTempData), data);
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
                model.vStatus = AccessInventoryTyp._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.BILLTempData);
                Cache.Set(CacheList.BILLTempData, setBillViewRowNum(tempData));
                var dayData = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                dayData.AddRange(tempData.ModelConvert<BillViewModel, BillViewModel>());
                Cache.Invalidate(CacheList.BILLDayData);
                //Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
                Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(dayData));
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
                    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(tempData));
                    dayData.AddRange(tempData.ModelConvert<BillViewModel,BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    //Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(dayData));
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
                    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(tempData));
                    dayData.AddRange(tempData.ModelConvert<BillViewModel,BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    //Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(dayData));
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
                    updateTempData.vStatus = AccessInventoryTyp._4.GetDescription();
                    updateTempData.vTakeOutE = model.vTakeOutE;
                    updateTempData.vTakeOutTotalNum = model.vTakeOutTotalNum;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(tempData));
                    var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = getOut(_data2);
                    _data2.AddRange(tempData.ModelConvert<BillViewModel, BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
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
                    updateTempData.vStatus = AccessInventoryTyp._1.GetDescription();
                    updateTempData.vTakeOutE = null;
                    updateTempData.vTakeOutTotalNum = null;
                    updateTempData.vReMainTotalNum = null;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(tempData));
                    var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = getOut(_data2);
                    _data2.AddRange(tempData.ModelConvert<BillViewModel,BillViewModel>());
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
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
            resetBillViewModel(AccessType);
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
                    if (Cache.IsSet(CacheList.BILLTempData))
                        return Json(jdata.modelToJqgridResult(setBillViewModelOrder((List<BillViewModel>)Cache.Get(CacheList.BILLTempData))));
                    break;
                case "Day":
                    if (Cache.IsSet(CacheList.BILLDayData))
                        return Json(jdata.modelToJqgridResult((List<BillViewModel>)Cache.Get(CacheList.BILLDayData)));
                    break;
            }
            return null;
        }

        private void resetBillViewModel(string AccessType,string AplyNo = null)
        {
            Cache.Invalidate(CacheList.BILLTempData);
            Cache.Invalidate(CacheList.BILLDayData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, new List<BillViewModel>());
                    //Cache.Set(CacheList.BILLDayData, setBillViewModelGroup((List<BillViewModel>)Bill.GetDayData(data.vAplyUnit)));
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup((List<BillViewModel>)Bill.GetDayData(data.vAplyUnit)));                   
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    var _data = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit, "1");//只抓庫存
                    var _data2 = (List<BillViewModel>)Bill.GetDayData(data.vAplyUnit);
                    _data2 = getOut(_data2);
                    _data2.AddRange(_data.ModelConvert<BillViewModel, BillViewModel>());
                    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(_data));
                    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                }
            }
            else
            {
                //var _data = (List<BillViewModel>)Bill.GetTempData(AplyNo);
                //var _data2 = (List<BillViewModel>)Bill.GetDayData(null,null,AplyNo);
                //var _AccessType = Bill.GetAccessType(AplyNo);
                //if (_AccessType == AccessProjectTradeType.P.ToString())
                //{
                //    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(_data));
                //    //Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(_data2));
                //    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                //}
                //if (_AccessType == AccessProjectTradeType.G.ToString())
                //{
                //    Cache.Set(CacheList.BILLTempData, setBillViewRowNum(_data));
                //    Cache.Set(CacheList.BILLDayData, setBillTakeOutViewModelGroup(_data2));
                //}
            }

        }

        private List<BillViewModel> getOut(List<BillViewModel> data)
        {
            return data.Where(x => x.vStatus != AccessInventoryTyp._1.GetDescription()).ToList();
        }

        private List<BillViewModel> setBillViewModelOrder(List<BillViewModel> data)
        {
            if (data.Any())
            {
                data = data.OrderBy(x => x.vRowNum).ToList();
            }
            return data;
        }

        private List<BillViewModel> setBillViewRowNum(List<BillViewModel> data)
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
        private List<BillViewModel> setBillTakeOutViewModelGroup(List<BillViewModel> data)
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
                        if (x.vStatus == AccessInventoryTyp._3.GetDescription())
                        {
                            x.vReMainTotalNum = x.vCheckTotalNum;
                            _intReMainTotalNum += TypeTransfer.stringToInt(x.vReMainTotalNum);
                            x.vCheckTotalNum = "";
                        }
                        else
                        {
                            var _vReMainTotalNum =
                            (x.vStatus == AccessInventoryTyp._1.GetDescription() || !x.vTakeOutE.IsNullOrWhiteSpace()) ?
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