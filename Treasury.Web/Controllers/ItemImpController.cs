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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 重要物品
/// 初版作者：20180802 陳宥穎
/// 修改歷程：20180802 陳宥穎 
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
    public class ItemImpController : CommonController
    {
        private IItemImp ItemImp;

        public ItemImpController()
        {
            ItemImp = new ItemImp();
        }

        /// <summary>
        /// 重要物品 新增畫面
        /// </summary>
        /// <returns></returns>resetItemImpViewModel
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.OPVT = type;
            var _dActType = GetActType(type, AplyNo);
            ViewBag.CustodianFlag = AccountController.CustodianFlag;
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetItemImpViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetItemImpViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        /// <summary>
        /// 重要物品 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCItemImpViewModel>)ItemImp.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCItemImpData);
            Cache.Set(CacheList.CDCItemImpData, _data);
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
            if (Cache.IsSet(CacheList.CDCItemImpData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCItemImpViewModel>)Cache.Get(CacheList.CDCItemImpData))
                    .OrderBy(x => x.vPUT_Date) //入庫日期
                    .ThenBy(x => x.vAPLY_UID) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vItemImp_Name) //物品名稱
                    .ToList()
                    ));
            return null;
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
            
            var _detail = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_data.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = ItemImp.ApplyAudit(_data, data);
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
            var _detail = (List<CDCItemImpViewModel>)Cache.Get(CacheList.CDCItemImpData);
            if (!_detail.Any(x => x.vAFTFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = ItemImp.CDCApplyAudit(_detail.Where(x => x.vAFTFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCItemImpData);
                    Cache.Set(CacheList.CDCItemImpData, result.Datas);
                }
            }
            else
            {
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改資料庫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbData(CDCItemImpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCItemImpData))
            {
                var dbData = (List<CDCItemImpViewModel>)Cache.Get(CacheList.CDCItemImpData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    var _vItemImp_Name_AFT = model.vItemImp_Name.CheckAFT(updateTempData.vItemImp_Name);
                    if (_vItemImp_Name_AFT.Item2)
                        updateTempData.vItemImp_Name_AFT = _vItemImp_Name_AFT.Item1;
                    var _vItemImp_Remaining_AFT = TypeTransfer.intNToString(model.vItemImp_Remaining).CheckAFT(TypeTransfer.intNToString(updateTempData.vItemImp_Remaining));
                    if (_vItemImp_Remaining_AFT.Item2)
                        updateTempData.vItemImp_Remaining_AFT = TypeTransfer.stringToIntN(_vItemImp_Remaining_AFT.Item1);
                    var _vItemImp_Amount_AFT = TypeTransfer.decimalNToString(model.vItemImp_Amount).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vItemImp_Amount));
                    if (_vItemImp_Amount_AFT.Item2)
                        updateTempData.vItemImp_Amount_AFT = TypeTransfer.stringToDecimal(_vItemImp_Amount_AFT.Item1);
                    var _vItemImp_Expected_Date_AFT = model.vItemImp_Expected_Date.CheckAFT(updateTempData.vItemImp_Expected_Date);
                    if (_vItemImp_Expected_Date_AFT.Item2)
                        updateTempData.vItemImp_Expected_Date_AFT = _vItemImp_Expected_Date_AFT.Item1;
                    var _vItemImp_Description_AFT = model.vItemImp_Description.CheckAFT(updateTempData.vItemImp_Description);
                    if (_vItemImp_Description_AFT.Item2)
                        updateTempData.vItemImp_Description_AFT = _vItemImp_Description_AFT.Item1;
                    var _vItemImp_MEMO_AFT = model.vItemImp_MEMO.CheckAFT(updateTempData.vItemImp_MEMO);
                    if (_vItemImp_MEMO_AFT.Item2)
                        updateTempData.vItemImp_MEMO_AFT = _vItemImp_MEMO_AFT.Item1;
                    updateTempData.vAFTFlag = _vItemImp_Name_AFT.Item2 || _vItemImp_Remaining_AFT.Item2 || _vItemImp_Amount_AFT.Item2 || _vItemImp_Expected_Date_AFT.Item2 || _vItemImp_Description_AFT.Item2 || _vItemImp_MEMO_AFT.Item2;
                    Cache.Invalidate(CacheList.CDCItemImpData);
                    Cache.Set(CacheList.CDCItemImpData, dbData);
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
            if (Cache.IsSet(CacheList.CDCItemImpData))
            {
                var dbData = (List<CDCItemImpViewModel>)Cache.Get(CacheList.CDCItemImpData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vItemImp_Name_AFT = null;
                    updateTempData.vItemImp_Remaining_AFT = null;
                    updateTempData.vItemImp_Amount_AFT = null;
                    updateTempData.vItemImp_Expected_Date_AFT = null;
                    updateTempData.vItemImp_Description_AFT = null;
                    updateTempData.vItemImp_MEMO_AFT = null;
                    updateTempData.vAFTFlag = false;
                    Cache.Invalidate(CacheList.CDCItemImpData);
                    Cache.Set(CacheList.CDCItemImpData, dbData);
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
        public JsonResult InsertTempData(ItemImpViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.ItemImpData))
            {
                var tempData = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.ItemImpData);
                Cache.Set(CacheList.ItemImpData, tempData);
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
        public JsonResult UpdateTempData(ItemImpViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.ItemImpData))
            {
                var tempData = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vItemImp_Name = model.vItemImp_Name;
                    updateTempData.vItemImp_Quantity = model.vItemImp_Quantity;
                    updateTempData.vItemImp_Amount = model.vItemImp_Amount;
                    updateTempData.vItemImp_Expected_Date = model.vItemImp_Expected_Date;
                    //updateTempData.vItemImp_Expected_Date = model.vItemImp_Expected_Date;
                    updateTempData.vDescription = model.vDescription;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.ItemImpData);
                    Cache.Set(CacheList.ItemImpData, tempData);
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
        public JsonResult DeleteTempData(ItemImpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            //transType(model);
            if (Cache.IsSet(CacheList.ItemImpData))
            {
                var tempData = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.ItemImpData);
                    Cache.Set(CacheList.ItemImpData, tempData);
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
        public JsonResult TakeOutData(ItemImpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemImpData))
            {
                var tempData = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vtakeoutFlag = true;
                    updateTempData.vItemImp_G_Quantity = model.vItemImp_G_Quantity;
                    if(model.vItemImp_G_Quantity == null || model.vItemImp_G_Quantity.Value == 0)
                        updateTempData.vtakeoutFlag = false;
                    Cache.Invalidate(CacheList.ItemImpData);
                    Cache.Set(CacheList.ItemImpData, tempData);
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
        public JsonResult RepeatData(ItemImpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemImpData))
            {
                var tempData = (List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vItemImp_G_Quantity = null;
                    updateTempData.vtakeoutFlag = false;
                    Cache.Invalidate(CacheList.ItemImpData);
                    Cache.Set(CacheList.ItemImpData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.vtakeoutFlag);
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
            resetItemImpViewModel(AccessType);
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
            if (Cache.IsSet(CacheList.ItemImpData))
                return Json(jdata.modelToJqgridResult(
                    ((List<ItemImpViewModel>)Cache.Get(CacheList.ItemImpData)).OrderBy(x=>x.vItemId).ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 重要物品預設資料
        /// </summary>
        /// <param name="ActType">修改狀態</param>
        /// <param name="AccessType">存入 or 取出</param>
        /// <param name="AplyNo">單號</param>
        private void resetItemImpViewModel(string AccessType , string AplyNo = null, bool ActType = true)
        {
            Cache.Invalidate(CacheList.ItemImpData);
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.ItemImpData, new List<ItemImpViewModel>());
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.ItemImpData, ItemImp.GetDbDataByUnit(data.vAplyUnit, AplyNo));//只抓庫存
                }
            }
            else
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.ItemImpData, ItemImp.GetDataByAplyNo(AplyNo));//抓單號
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    if (ActType && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        Cache.Set(CacheList.ItemImpData, ItemImp.GetDbDataByUnit(data.vAplyUnit, AplyNo));//抓庫存+單號
                    }
                    else
                    {
                        Cache.Set(CacheList.ItemImpData, ItemImp.GetDataByAplyNo(AplyNo));//抓單號
                    }
                }
            }
        }

    }
}