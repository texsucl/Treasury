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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存出保證金
/// 初版作者：20180730 侯蔚鑫
/// 修改歷程：20180730 侯蔚鑫 
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
    public class MargingController : CommonController
    {
        // GET: Marging
        private IMarging Marging;

        public MargingController()
        {
            Marging = new Marging();
        }

        /// <summary>
        /// 存出保證金 新增畫面
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">金庫物品存取主畫面ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.OPVT = type;
            ViewBag.dMarging_Dep_Type = new SelectList(Marging.GetMargingType(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;

            var _dActType = GetActType(type, AplyNo);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetMargingViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetMargingViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        /// <summary>
        /// 存出保證金 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCMargingViewModel>)Marging.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            ViewBag.dMarging_Dep_Type = new SelectList(Marging.GetMargingType(), "Value", "Text");
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCMargingData);
            Cache.Set(CacheList.CDCMargingData, _data);
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
            if (Cache.IsSet(CacheList.CDCMargingData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCMargingViewModel>)Cache.Get(CacheList.CDCMargingData))
                    .OrderBy(x => x.vPut_Date) //入庫日期
                    .ThenBy(x => x.vAply_Uid) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vMargin_Dep_Type) //類別
                    .ThenBy(x => x.vTrad_Partners) //交易對象
                    //.ThenBy(x => x.) //號碼
                    .ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            if (Cache.IsSet(CacheList.TreasuryAccessViewData) && Cache.IsSet(CacheList.MargingData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                var _data = (List<MargingpViewModel>)Cache.Get(CacheList.MargingData);

                if (AccountController.CustodianFlag) //保管科
                {
                    if (_data.Any(x => x.vBook_No.IsNullOrWhiteSpace()))
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Ref.MessageType.book_No_Error.GetDescription();
                        return Json(result);
                    }
                }

                //取出勾選判斷
                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    //判斷至少勾選一筆資料
                    var vDetail = ((List<MargingpViewModel>)Cache.Get(CacheList.MargingData)).Where(x => x.vTakeoutFlag == true).ToList();
                    if (!vDetail.Any())
                    {
                        _data = new List<MargingpViewModel>();
                    }
                }

                result = Marging.ApplyAudit(_data, data);
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
            var _detail = (List<CDCMargingViewModel>)Cache.Get(CacheList.CDCMargingData);
            if (!_detail.Any(x => x.vAFTFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = Marging.CDCApplyAudit(_detail.Where(x => x.vAFTFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCMargingData);
                    Cache.Set(CacheList.CDCMargingData, result.Datas);
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
        public JsonResult UpdateDbData(CDCMargingViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCMargingData))
            {
                var dbData = (List<CDCMargingViewModel>)Cache.Get(CacheList.CDCMargingData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItem_PK == model.vItem_PK);
                if (updateTempData != null)
                {
                    var _vMargin_Dep_Type_AFT = model.vMargin_Dep_Type.CheckAFT(updateTempData.vMargin_Dep_Type);
                    if (_vMargin_Dep_Type_AFT.Item2)
                        updateTempData.vMargin_Dep_Type_AFT = _vMargin_Dep_Type_AFT.Item1;
                    var _vTrad_Partners_AFT = model.vTrad_Partners.CheckAFT(updateTempData.vTrad_Partners);
                    if (_vTrad_Partners_AFT.Item2)
                        updateTempData.vTrad_Partners_AFT = _vTrad_Partners_AFT.Item1;
                    var _vAmount_AFT = TypeTransfer.decimalNToString(model.vAmount).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vAmount));
                    if (_vAmount_AFT.Item2)
                        updateTempData.vAmount_AFT = TypeTransfer.stringToDecimal(_vAmount_AFT.Item1);
                    var _vWorkplace_Code_AFT = model.vWorkplace_Code.CheckAFT(updateTempData.vWorkplace_Code);
                    if (_vWorkplace_Code_AFT.Item2)
                        updateTempData.vWorkplace_Code_AFT = _vWorkplace_Code_AFT.Item1;
                    var _vDescription_AFT = model.vDescription.CheckAFT(updateTempData.vDescription);
                    if (_vDescription_AFT.Item2)
                        updateTempData.vDescription_AFT = _vDescription_AFT.Item1;
                    var _vMemo_AFT = model.vMemo.CheckAFT(updateTempData.vMemo);
                    if (_vMemo_AFT.Item2)
                        updateTempData.vMemo_AFT = _vMemo_AFT.Item1;
                    var _vBook_No_AFT = model.vBook_No.CheckAFT(updateTempData.vBook_No);
                    if (_vBook_No_AFT.Item2)
                        updateTempData.vBook_No_AFT = _vBook_No_AFT.Item1;
                    updateTempData.vAFTFlag = _vMargin_Dep_Type_AFT.Item2 || _vTrad_Partners_AFT.Item2 || _vAmount_AFT.Item2 || _vWorkplace_Code_AFT.Item2 || _vDescription_AFT.Item2 || _vMemo_AFT.Item2 || _vBook_No_AFT.Item2;
                    Cache.Invalidate(CacheList.CDCMargingData);
                    Cache.Set(CacheList.CDCMargingData, dbData);
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
            if (Cache.IsSet(CacheList.CDCMargingData))
            {
                var dbData = (List<CDCMargingViewModel>)Cache.Get(CacheList.CDCMargingData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItem_PK == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vBook_No_AFT = null;
                    updateTempData.vMargin_Dep_Type_AFT = null;
                    updateTempData.vTrad_Partners_AFT = null;
                    updateTempData.vAmount_AFT = null;
                    updateTempData.vWorkplace_Code_AFT = null;
                    updateTempData.vDescription_AFT = null;
                    updateTempData.vMemo_AFT = null;
                    updateTempData.vAFTFlag = false;
                    Cache.Invalidate(CacheList.CDCMargingData);
                    Cache.Set(CacheList.CDCMargingData, dbData);
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
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData(string AccessType)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            resetMargingViewModel(AccessType);
            return Json(result);
        }

        /// <summary>
        /// 新增存出保證金明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(MargingpViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingpViewModel>)Cache.Get(CacheList.MargingData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.MargingData);
                Cache.Set(CacheList.MargingData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改存出保證金明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(MargingpViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingpViewModel>)Cache.Get(CacheList.MargingData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItem_PK == model.vItem_PK);
                if (updateTempData != null)
                {
                    updateTempData.vTrad_Partners = model.vTrad_Partners;
                    updateTempData.vMargin_Dep_Type = model.vMargin_Dep_Type;
                    updateTempData.vAmount = model.vAmount;
                    updateTempData.vWorkplace_Code = model.vWorkplace_Code;
                    updateTempData.vDescription = model.vDescription;
                    updateTempData.vMemo = model.vMemo;
                    updateTempData.vBook_No = model.vBook_No;
                    Cache.Invalidate(CacheList.MargingData);
                    Cache.Set(CacheList.MargingData, tempData);
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
        /// 刪除存出保證金明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(MargingpViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingpViewModel>)Cache.Get(CacheList.MargingData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItem_PK == model.vItem_PK);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.MargingData);
                    Cache.Set(CacheList.MargingData, tempData);
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
        public JsonResult TakeOutData(MargingpViewModel model, bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingpViewModel>)Cache.Get(CacheList.MargingData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItem_PK == model.vItem_PK);
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
                    Cache.Invalidate(CacheList.MargingData);
                    Cache.Set(CacheList.MargingData, tempData);
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
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.MargingData))
                return Json(jdata.modelToJqgridResult(
                    ((List<MargingpViewModel>)Cache.Get(CacheList.MargingData)).OrderBy(x => x.vItem_PK).ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 設定存出保證金Cache資料
        /// </summary>
        /// <param name="AccessType">申請狀態</param>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="EditFlag">修改狀態</param>
        /// <returns></returns>
        private void resetMargingViewModel(string AccessType, string AplyNo = null, bool EditFlag = false)
        {
            Cache.Invalidate(CacheList.MargingData);
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);

            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.MargingData, new List<MargingpViewModel>());
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.MargingData, Marging.GetDbDataByUnit(data.vAplyUnit));//只抓庫存
                }
            }
            else
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.MargingData, Marging.GetDataByAplyNo(AplyNo));//抓單號
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    if (EditFlag && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        Cache.Set(CacheList.MargingData, Marging.GetDbDataByUnit(data.vAplyUnit, AplyNo));//抓庫存+單號
                    }
                    else
                    {
                        Cache.Set(CacheList.MargingData, Marging.GetDataByAplyNo(AplyNo));//抓單號
                    }
                }
            }
        }
    }
}