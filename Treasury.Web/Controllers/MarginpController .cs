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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存入保證金
/// 初版作者：20180806 王菁萱
/// 修改歷程：20180806 王菁萱 
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
    public class MarginpController : CommonController
    {
        private IMarginp Marginp;

        public MarginpController()
        {
            Marginp = new Marginp();
        }

        /// <summary>
        /// 存入保證金 新增畫面
        /// </summary>
        /// <returns></returns>resetMarginpViewModel
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.OPVT = type;
            var _dActType = GetActType(type, AplyNo);
            ViewBag.MarginpType = new SelectList(Marginp.GetMarginp_Take_Of_Type(), "Value", "Text"); 
            ViewBag.MarginpItem = new SelectList(Marginp.GetMarginpItem(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetMarginpViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetMarginpViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        // <summary>
        /// 存入保證金 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCMarginpViewModel>)Marginp.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            ViewBag.dMargin_Take_Of_Type = new SelectList(Marginp.GetMarginp_Take_Of_Type(), "Value", "Text");
            ViewBag.dMargin_Item = new SelectList(Marginp.GetMarginpItem(), "Value", "Text");
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCMarginpData);
            Cache.Set(CacheList.CDCMarginpData, _data);
            return PartialView();
        }

        /// <summary>
        /// jqgrid CDCcache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCDCCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.CDCMarginpData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCMarginpViewModel>)Cache.Get(CacheList.CDCMarginpData))
                    .OrderBy(x => x.vPut_Date) //入庫日期
                    .ThenBy(x => x.vAply_Uid) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vMargin_Take_Of_Type) //類別
                    .ThenBy(x => x.vTrad_Partners) //交易對象
                                                   //.ThenBy(x => x.vTrad_Partners) //號碼?
                    .ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 抓取存入保證金物品名稱
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMarginpItem(string MARGIN_ITEM)
        {
            var MarginpItems = new List<string>();
            var data = Marginp.GetMarginpItem();
            switch (MARGIN_ITEM.ToString())
            {
                case "1":
                    MarginpItems = new List<string>() { "1", "2" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
                case "2":
                    MarginpItems = new List<string>() { "3" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
                case "3":
                    MarginpItems = new List<string>() { "4" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
            }
            return Json(data);
    }

        /// <summary>
        /// 抓取存入保證金物品名稱
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCDCMarginpItem(string MARGINITEM)
        {
            var MarginpItems = new List<string>();
            var data = Marginp.GetMarginpItem();
            switch (MARGINITEM.ToString())
            {
                case "1":
                    MarginpItems = new List<string>() { "1", "2" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
                case "2":
                    MarginpItems = new List<string>() { "3" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
                case "3":
                    MarginpItems = new List<string>() { "4" };
                    data = data.Where(x => MarginpItems.Contains(x.Value)).ToList();
                    break;
            }
            return Json(data);
        }

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            var _detail = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);

                if (AccountController.CustodianFlag) //保管科
                {
                    if (_data.Any(x => x.vMarginp_Book_No.IsNullOrWhiteSpace()))
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Ref.MessageType.book_No_Error.GetDescription();
                        return Json(result);
                    }
                }

                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_data.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = Marginp.ApplyAudit(_data, data);
                    if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                    {
                        new TreasuryAccessController().ResetSearchData();
                    }
                }
            }
            else
            {
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
            var _detail = (List<CDCMarginpViewModel>)Cache.Get(CacheList.CDCMarginpData);
            if (!_detail.Any(x => x.vAFTFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = Marginp.CDCApplyAudit(_detail.Where(x => x.vAFTFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCMarginpData);
                    Cache.Set(CacheList.CDCMarginpData, result.Datas);
                }
            }
            else
            {
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbData(CDCMarginpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCMarginpData))
            {
                var dbData = (List<CDCMarginpViewModel>)Cache.Get(CacheList.CDCMarginpData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id);
                if (updateTempData != null)
                {
                    var _vMargin_Take_Of_Type_AFT = model.vMargin_Take_Of_Type.CheckAFT(updateTempData.vMargin_Take_Of_Type);
                    if (_vMargin_Take_Of_Type_AFT.Item2)
                        updateTempData.vMargin_Take_Of_Type_AFT = _vMargin_Take_Of_Type_AFT.Item1;
                    var _vTrad_Partners_AFT = model.vTrad_Partners.CheckAFT(updateTempData.vTrad_Partners);
                    if (_vTrad_Partners_AFT.Item2)
                        updateTempData.vTrad_Partners_AFT = _vTrad_Partners_AFT.Item1;
                    var _vAmount_AFT = TypeTransfer.decimalNToString(model.vAmount).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vAmount));
                    if (_vAmount_AFT.Item2)
                        updateTempData.vAmount_AFT = TypeTransfer.stringToDecimal(_vAmount_AFT.Item1);
                    var _vMargin_Item_AFT = model.vMargin_Item.CheckAFT(updateTempData.vMargin_Item);
                    if (_vMargin_Item_AFT.Item2)
                        updateTempData.vMargin_Item_AFT = _vMargin_Item_AFT.Item1;
                    var _vMargin_Item_Issuer_AFT = model.vMargin_Item_Issuer.CheckAFT(updateTempData.vMargin_Item_Issuer);
                    if (_vMargin_Item_Issuer_AFT.Item2)
                        updateTempData.vMargin_Item_Issuer_AFT = _vMargin_Item_Issuer_AFT.Item1;
                    var _vPledge_Item_No_AFT = model.vPledge_Item_No.CheckAFT(updateTempData.vPledge_Item_No);
                    if (_vPledge_Item_No_AFT.Item2)
                        updateTempData.vPledge_Item_No_AFT = _vPledge_Item_No_AFT.Item1;
                    var _vEffective_Date_B_AFT = model.vEffective_Date_B.CheckAFT(updateTempData.vEffective_Date_B);
                    if (_vEffective_Date_B_AFT.Item2)
                        updateTempData.vEffective_Date_B_AFT = _vEffective_Date_B_AFT.Item1;
                    var _vEffective_Date_E_AFT = model.vEffective_Date_E.CheckAFT(updateTempData.vEffective_Date_E);
                    if (_vEffective_Date_E_AFT.Item2)
                        updateTempData.vEffective_Date_E_AFT = _vEffective_Date_E_AFT.Item1;
                    var _vDescription_AFT = model.vDescription.CheckAFT(updateTempData.vDescription);
                    if (_vDescription_AFT.Item2)
                        updateTempData.vDescription_AFT = _vDescription_AFT.Item1;
                    var _vMemo_AFT = model.vMemo.CheckAFT(updateTempData.vMemo);
                    if (_vMemo_AFT.Item2)
                        updateTempData.vMemo_AFT = _vMemo_AFT.Item1;
                    var _vBook_No_AFT = model.vBook_No.CheckAFT(updateTempData.vBook_No);
                    if (_vBook_No_AFT.Item2)
                        updateTempData.vBook_No_AFT = _vBook_No_AFT.Item1;
                    updateTempData.vAFTFlag = _vMargin_Take_Of_Type_AFT.Item2 || _vTrad_Partners_AFT.Item2 || _vAmount_AFT.Item2 || _vMargin_Item_AFT.Item2 || _vMargin_Item_Issuer_AFT.Item2 || _vPledge_Item_No_AFT.Item2 || _vEffective_Date_B_AFT.Item2 || _vEffective_Date_E_AFT.Item2 || _vDescription_AFT.Item2 || _vMemo_AFT.Item2 || _vBook_No_AFT.Item2;
                    Cache.Invalidate(CacheList.CDCMarginpData);
                    Cache.Set(CacheList.CDCMarginpData, dbData);
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
            if (Cache.IsSet(CacheList.CDCMarginpData))
            {
                var dbData = (List<CDCMarginpViewModel>)Cache.Get(CacheList.CDCMarginpData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItem_Id == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vMargin_Take_Of_Type_AFT = null;
                    updateTempData.vTrad_Partners_AFT = null;
                    updateTempData.vAmount_AFT = null;
                    updateTempData.vMargin_Item_AFT = null;
                    updateTempData.vMargin_Item_Issuer_AFT = null;
                    updateTempData.vPledge_Item_No_AFT = null;
                    updateTempData.vEffective_Date_B_AFT = null;
                    updateTempData.vEffective_Date_E_AFT = null;
                    updateTempData.vDescription_AFT = null;
                    updateTempData.vMemo_AFT = null;
                    updateTempData.vAFTFlag = false;
                    Cache.Invalidate(CacheList.CDCMarginpData);
                    Cache.Set(CacheList.CDCMarginpData, dbData);
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
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(MarginpViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                //transType(model);
                tempData.Add(model);
                Cache.Invalidate(CacheList.MarginpData);
                Cache.Set(CacheList.MarginpData, tempData);
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
        public JsonResult UpdateTempData(MarginpViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
           // transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vMarginp_Take_Of_Type = model.vMarginp_Take_Of_Type;
                    updateTempData.vMarginp_Trad_Partners = model.vMarginp_Trad_Partners;
                    updateTempData.vItemId = model.vItemId;
                    updateTempData.vMarginp_Amount = model.vMarginp_Amount;
                    updateTempData.vMarginp_Item = model.vMarginp_Item;
                    updateTempData.vMarginp_Item_Issuer = model.vMarginp_Item_Issuer;
                    updateTempData.vMarginp_Pledge_Item_No = model.vMarginp_Pledge_Item_No;
                    updateTempData.vMarginp_Effective_Date_B = model.vMarginp_Effective_Date_B;
                    updateTempData.vMarginp_Effective_Date_E = model.vMarginp_Effective_Date_E;
                    updateTempData.vDescription = model.vDescription;
                    updateTempData.vMemo = model.vMemo;
                    updateTempData.vMarginp_Book_No = model.vMarginp_Book_No;                 
                    //transType(updateTempData);
                    Cache.Invalidate(CacheList.MarginpData);
                    Cache.Set(CacheList.MarginpData, tempData);
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
        public JsonResult DeleteTempData(MarginpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.MarginpData);
                    Cache.Set(CacheList.MarginpData, tempData);
                    //transType(model);
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
        public JsonResult TakeOutData(MarginpViewModel model,bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
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
                    updateTempData.vtakeoutFlag = takeoutFlag;
                    Cache.Invalidate(CacheList.MarginpData);
                    Cache.Set(CacheList.MarginpData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.vtakeoutFlag == true);
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
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            resetMarginpViewModel( AccessType);
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
            if (Cache.IsSet(CacheList.MarginpData))
                return Json(jdata.modelToJqgridResult(
                    ((List<MarginpViewModel>)Cache.Get(CacheList.MarginpData)).OrderBy(x=>x.vItemId).ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 存入保證金預設資料
        /// </summary>
        /// <param name="ActType">修改狀態</param>
        /// <param name="AccessType">存入 or 取出</param>
        /// <param name="AplyNo">單號</param>
        private void resetMarginpViewModel(string AccessType , string AplyNo = null, bool ActType = true)
        {
            Cache.Invalidate(CacheList.MarginpData);     
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.MarginpData, new List<MarginpViewModel>());
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.MarginpData, Marginp.GetDbDataByUnit(data.vAplyUnit, AplyNo));//只抓庫存
                }              
            }
            else
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.MarginpData, Marginp.GetDataByAplyNo(AplyNo));//抓單號
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    if (ActType && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        Cache.Set(CacheList.MarginpData, Marginp.GetDbDataByUnit(data.vAplyUnit, AplyNo));//抓庫存+單號
                    }
                    else
                    {
                        Cache.Set(CacheList.MarginpData, Marginp.GetDataByAplyNo(AplyNo));//抓單號
                    }
                }
            }
        }
        /// <summary>
        /// 西元年轉民國年
        /// </summary>
        //private void transType(MarginpViewModel model)
        //{
        //    if (model != null)
        //    {
        //        var date_B = TypeTransfer.stringToDateTimeN(model.vMarginp_Effective_Date_B);
        //        model.vMarginp_Effective_Date_B_2 =
        //            (date_B == null ? null : date_B.Value.DateToTaiwanDate(9));
        //        var date_E = TypeTransfer.stringToDateTimeN(model.vMarginp_Effective_Date_E);
        //        model.vMarginp_Effective_Date_E_2 =
        //            (date_E == null ? null : date_E.Value.DateToTaiwanDate(9));
        //    }
        //}
    }
 }