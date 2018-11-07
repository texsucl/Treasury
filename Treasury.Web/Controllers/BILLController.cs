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
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class BillController : CommonController
    {
        private IBill Bill;

        public BillController()
        {
            Bill = new Bill();
        }

        /// <summary>
        /// 空白票據 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data , Ref.OpenPartialViewType type)
        {
            ViewBag.OPVT = type;
            ViewBag.dBILL_Check_Type = new SelectList(Bill.GetCheckType(), "Value", "Text");
            var ibs = Bill.GetIssuing_Bank();
            ViewBag.dBILL_Issuing_Bank = new SelectList(ibs, "Value", "Text");
            var _dActType = GetActType(type , AplyNo);  //畫面是否可以CRUD    
            if (AplyNo.IsNullOrWhiteSpace())
            {
                ViewBag.dAccess = null;
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                ResetBillViewModel(data.vAccessType);
            }
            else
            {
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                ViewBag.dAccess = viewModel.vAccessType;
                if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    _dActType = false; //空白票據 取出預設只能檢視
                }
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                ResetBillViewModel(viewModel.vAccessType, AplyNo);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        /// <summary>
        /// 空白票據 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCBillViewModel>)Bill.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            ViewBag.dBILL_Check_Type = new SelectList(Bill.GetCheckType(), "Value", "Text");
            ViewBag.dBILL_Issuing_Bank = new SelectList(Bill.GetIssuing_Bank(), "Value", "Text");
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCBILLData);
            Cache.Set(CacheList.CDCBILLData, _data);
            Cache.Invalidate(CacheList.CDCBILLAllData);
            Cache.Set(CacheList.CDCBILLAllData, ((List<CDCBillViewModel>)Bill.GetCDCSearchData(new CDCSearchViewModel() { vTreasuryIO = "Y" }, null)));
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
                result = Bill.ApplyAudit((List<BillViewModel>)Cache.Get(CacheList.BILLTempData), data);
                if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                {
                    new TreasuryAccessController().ResetSearchData();
                }
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 申請資料庫異動覆核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyDbData()
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            var _detail = (List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLData);
            if (!_detail.Any(x => x.vAFTFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = Bill.CDCApplyAudit(_detail.Where(x => x.vAFTFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCCAData);
                    Cache.Set(CacheList.CDCCAData, result.Datas);
                }
            }
            else
            {
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
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
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var checkdata = (List<BillViewModel>)Cache.Get(CacheList.BILLDayData);
                var sameFlag = checkSameData(checkdata, model);
                if(!sameFlag.IsNullOrWhiteSpace())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = sameFlag;
                    return Json(result);
                }
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
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
        public JsonResult UpdateTempData(BillViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData) && Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                var checkdata = (List<BillViewModel>)Cache.Get(CacheList.BILLDayData);
                var sameFlag = checkSameData(checkdata, model);
                if (!sameFlag.IsNullOrWhiteSpace())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = sameFlag;
                    return Json(result);
                }
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
        public JsonResult DeleteTempData(BillViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TakeOutData(BillViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStatus = Ref.AccessInventoryType._4.GetDescription();
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
        /// 重設事件動作(復原該筆庫存)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RepeatData(BillViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vStatus = Ref.AccessInventoryType._1.GetDescription();
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
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.vStatus == Ref.AccessInventoryType._4.GetDescription());
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
        public JsonResult ResetTempData(string AccessType)
        {
            ResetBillViewModel(AccessType);
            return Json(new MSGReturnModel<string>());
        }

        /// <summary>
        /// 修改資料庫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbData(CDCBillViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCBILLData))
            {
                var dbData = (List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    var allData = (List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLAllData);
                    var msg = chechSameDataByCDC(allData, updateTempData);
                    if (!msg.IsNullOrWhiteSpace())
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = msg;
                        return Json(result);
                    }
                    var _vBill_Issuing_Banke_AFT = model.vBill_Issuing_Bank.CheckAFT(updateTempData.vBill_Issuing_Bank);
                    if (_vBill_Issuing_Banke_AFT.Item2)
                        updateTempData.vBill_Issuing_Bank_AFT = _vBill_Issuing_Banke_AFT.Item1;
                    var _vBill_Check_Type_AFT = model.vBill_Check_Type.CheckAFT(updateTempData.vBill_Check_Type);
                    if (_vBill_Check_Type_AFT.Item2)
                        updateTempData.vBill_Check_Type_AFT = _vBill_Check_Type_AFT.Item1;
                    var _vBill_Check_No_Track_AFT = model.vBill_Check_No_Track.CheckAFT(updateTempData.vBill_Check_No_Track);
                    if (_vBill_Check_No_Track_AFT.Item2)
                        updateTempData.vBill_Check_No_Track_AFT = _vBill_Check_No_Track_AFT.Item1;
                    var _vBill_Check_No_B_AFT = model.vBill_Check_No_B.CheckAFT(updateTempData.vBill_Check_No_B);
                    if (_vBill_Check_No_B_AFT.Item2)
                        updateTempData.vBill_Check_No_B_AFT = _vBill_Check_No_B_AFT.Item1;
                    var _vBill_Check_No_E_AFT = model.vBill_Check_No_E.CheckAFT(updateTempData.vBill_Check_No_E);
                    if (_vBill_Check_No_E_AFT.Item2)
                        updateTempData.vBill_Check_No_E_AFT = _vBill_Check_No_E_AFT.Item1;
                    updateTempData.vAFTFlag = _vBill_Issuing_Banke_AFT.Item2 || _vBill_Check_Type_AFT.Item2 || _vBill_Check_No_Track_AFT.Item2 || _vBill_Check_No_B_AFT.Item2 || _vBill_Check_No_E_AFT.Item2;
                    setDbAllData(model);
                    Cache.Invalidate(CacheList.CDCBILLData);
                    Cache.Set(CacheList.CDCBILLData, dbData);
                    result.Datas = dbData.Any(x => x.vAFTFlag);
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
        /// 重設資料庫資料
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RepeatDbData(string itemId)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCBILLData))
            {
                var dbData = (List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vBill_Issuing_Bank_AFT = null;
                    updateTempData.vBill_Check_Type_AFT = null;
                    updateTempData.vBill_Check_No_Track_AFT = null;
                    updateTempData.vBill_Check_No_B_AFT = null;
                    updateTempData.vBill_Check_No_E_AFT = null;
                    updateTempData.vAFTFlag = false;
                    setDbAllData(updateTempData);
                    Cache.Invalidate(CacheList.CDCBILLData);
                    Cache.Set(CacheList.CDCBILLData, dbData);
                    result.Datas = dbData.Any(x => x.vAFTFlag);
                    result.RETURN_FLAG = true;
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
        /// jqgrid CDCcache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCDCCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.CDCBILLData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLData))
                    .OrderBy(x => x.vPut_Date) //入庫日期
                    .ThenBy(x => x.vAply_Uid) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vBill_Issuing_Bank) //發票行庫
                    .ThenBy(x => x.vBill_Check_Type) //類型
                    .ThenBy(x => x.vItemId) //ID
                    .ToList()
                    ));
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
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, new List<BillViewModel>());
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup((List<BillViewModel>)Bill.GetDayData(data.vAplyUnit)));                   
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
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
                if (_AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.BILLTempData, SetBillViewRowNum(_data));
                    Cache.Set(CacheList.BILLDayData, SetBillTakeOutViewModelGroup(_data2));
                }
                if (_AccessType == Ref.AccessProjectTradeType.G.ToString())
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
            return data.Where(x => x.vStatus != Ref.AccessInventoryType._1.GetDescription()).ToList();
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
        public List<BillViewModel> SetBillViewRowNum(List<BillViewModel> data)
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
        public List<BillViewModel> SetBillTakeOutViewModelGroup(List<BillViewModel> data)
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
                        //x.vRowNum = rownum.ToString();
                        if (x.vStatus == Ref.AccessInventoryType._3.GetDescription())
                        {
                            x.vReMainTotalNum = x.vCheckTotalNum;
                            _intReMainTotalNum += TypeTransfer.stringToInt(x.vReMainTotalNum);
                            x.vCheckTotalNum = "";
                        }
                        else
                        {
                            var _vReMainTotalNum =
                            (x.vStatus == Ref.AccessInventoryType._1.GetDescription() || !x.vTakeOutE.IsNullOrWhiteSpace()) ?
                            TypeTransfer.stringToInt(x.vCheckTotalNum) - TypeTransfer.stringToInt(x.vTakeOutTotalNum) : 0;
                            _intReMainTotalNum += _vReMainTotalNum;
                            x.vReMainTotalNum = _vReMainTotalNum == 0 ? "" : _vReMainTotalNum.ToString();
                        }

                        //rownum += 1;
                    });

                data.GroupBy(x => new { x.vIssuingBank, x.vCheckType })
                    .OrderBy(x => x.Key.vIssuingBank)
                    .ThenBy(x => x.Key.vCheckType)
                    .ToList()
                    .ForEach(x =>
                {
                    rownum = 1;
                    foreach (var item in x)
                    {
                        item.vRowNum = rownum.ToString();
                        rownum += 1;
                    }
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

        /// <summary>
        /// 比對資料系統檢視存入空白票據的票據的票號(起),票號(迄)是否與下面當日庫存明細資料中的票號(起),票號(迄)重疊, 若重疊,僅訊息提醒,但不影響存入資料的建置
        /// </summary>
        /// <param name="data"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string checkSameData(List<BillViewModel> data, BillViewModel model)
        {
            string msg = string.Empty;
            List<BillViewModel> result = new List<BillViewModel>();
            try
            {
                data = data.Where(x => x.vItemId != model.vItemId &&
                        x.vItemId != null &&
                        x.vIssuingBank == model.vIssuingBank).ToList();
                result.AddRange(data.Where(x => int.Parse(x.vCheckNoB) <= int.Parse(model.vCheckNoB) && int.Parse(model.vCheckNoB) <= int.Parse(x.vCheckNoE)));
                result.AddRange(data.Where(x => int.Parse(x.vCheckNoB) <= int.Parse(model.vCheckNoE) && int.Parse(model.vCheckNoE) <= int.Parse(x.vCheckNoE)));
                result.AddRange(data.Where(x => int.Parse(x.vCheckNoB) >= int.Parse(model.vCheckNoB) && int.Parse(model.vCheckNoE) >= int.Parse(x.vCheckNoE)));
                
                if (result.Any())
                {
                    msg = $"您建置存入票號(起)/票號(迄)和下面當日庫存明細資料重疊敬請確認,謝謝!</br>重複區段</br>發票行:{model.vIssuingBank}";
                    result.Select(x => $"{string.Join(",", $@"支票號碼(起):{x.vCheckNoB},支票號碼(迄):{x.vCheckNoE}")}")
                        .Distinct().ToList()
                        .ForEach(x => msg += $"</br>{x}");
                }              
            }
            catch
            {

            }
            return msg;
        }

        /// <summary>
        ///  比對資料系統檢視存入空白票據的票據的票號(起),票號(迄)是否與庫存重複
        /// </summary>
        /// <param name="data"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private string chechSameDataByCDC(List<CDCBillViewModel> data, CDCBillViewModel model)
        {
            string msg = string.Empty;
            List<CDCBillViewModel> result = new List<CDCBillViewModel>();
            try
            {
                data = data.Where(x => x.vItemId != model.vItemId &&
                        x.vItemId != null &&
                        x.vBill_Issuing_Bank == model.vBill_Issuing_Bank).ToList();
                result.AddRange(
                    data.Where(x => 
                    (x.vBill_Check_No_B_AFT == null ? int.Parse(x.vBill_Check_No_B) : int.Parse(x.vBill_Check_No_B_AFT)) <= int.Parse(model.vBill_Check_No_B) && 
                    int.Parse(model.vBill_Check_No_B) <= (x.vBill_Check_No_E_AFT == null ? int.Parse(x.vBill_Check_No_E) : int.Parse(x.vBill_Check_No_E_AFT))));
                result.AddRange(
                    data.Where(x => 
                    (x.vBill_Check_No_B_AFT == null ? int.Parse(x.vBill_Check_No_B) : int.Parse(x.vBill_Check_No_B_AFT)) <= int.Parse(model.vBill_Check_No_E) && 
                    int.Parse(model.vBill_Check_No_E) <= (x.vBill_Check_No_E_AFT == null ? int.Parse(x.vBill_Check_No_E) : int.Parse(x.vBill_Check_No_E_AFT))));
                result.AddRange(
                    data.Where(x => 
                    (x.vBill_Check_No_B_AFT == null ? int.Parse(x.vBill_Check_No_B) : int.Parse(x.vBill_Check_No_B_AFT)) >= int.Parse(model.vBill_Check_No_B) && 
                    int.Parse(model.vBill_Check_No_E) >= (x.vBill_Check_No_E_AFT == null ? int.Parse(x.vBill_Check_No_E) : int.Parse(x.vBill_Check_No_E_AFT))));

                if (result.Any())
                {
                    msg = $"您建置存入票號(起)/票號(迄)和下面當日庫存明細資料重疊敬請確認,謝謝!</br>重複區段</br>發票行:{model.vBill_Issuing_Bank}";
                    result.Select(x => $"{string.Join(",", $@"支票號碼(起):{(x.vBill_Check_No_B_AFT ==null ? x.vBill_Check_No_B : x.vBill_Check_No_B_AFT)},支票號碼(迄):{(x.vBill_Check_No_E_AFT == null ? x.vBill_Check_No_E : x.vBill_Check_No_E_AFT)}")}")
                        .Distinct().ToList()
                        .ForEach(x => msg += $"</br>{x}");
                }
            }
            catch
            {

            }
            return msg;
        }

        /// <summary>
        /// 更新在庫資料為調整後的資料
        /// </summary>
        /// <param name="model"></param>
        private void setDbAllData(CDCBillViewModel model)
        {
            var allData = (List<CDCBillViewModel>)Cache.Get(CacheList.CDCBILLAllData);
            var remove = allData.First(x => x.vItemId == model.vItemId);
            allData.Remove(remove);
            allData.Add(model);
            Cache.Invalidate(CacheList.CDCBILLAllData);
            Cache.Set(CacheList.CDCBILLAllData, allData);
        }
    }
}