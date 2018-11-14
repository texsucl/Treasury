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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 印章
/// 初版作者：20180716 張家華
/// 修改歷程：20180716 張家華 
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
    public class SealController : CommonController
    {
        private ISeal Seal;

        public SealController()
        {
            Seal = new Seal();
        }

        #region View
        /// <summary>
        /// 印章 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.OPVT = type;
            var _dActType = GetActType(type, AplyNo);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetSealViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetSealViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
        }

        /// <summary>
        /// 印章 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCSealViewModel>)Seal.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCSEALData);
            Cache.Set(CacheList.CDCSEALData, _data);
            return PartialView();
        }
        #endregion

        #region Get

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata)
        {
            if (Cache.IsSet(CacheList.SEALData))
                return Json(jdata.modelToJqgridResult(
                    ((List<SealViewModel>)Cache.Get(CacheList.SEALData)).OrderBy(x => x.vItemId).ToList()
                    ));
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
            if (Cache.IsSet(CacheList.CDCSEALData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCSealViewModel>)Cache.Get(CacheList.CDCSEALData))
                    .OrderBy(x => x.vPUT_Date) //入庫日期
                    .ThenBy(x => x.vAPLY_UID) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vSeal_Desc) //印章內容
                    .ToList()
                    ));
            return null;
        }

        #endregion

        #region Save
        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData(EstateModel model)
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            var _detail = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_data.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = Seal.ApplyAudit(_data, data);
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
            var _detail = (List<CDCSealViewModel>)Cache.Get(CacheList.CDCSEALData);
            if (!_detail.Any(x=>x.vAFTFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = Seal.CDCApplyAudit(_detail.Where(x=>x.vAFTFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCSEALData);
                    Cache.Set(CacheList.CDCSEALData, result.Datas);
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
        public JsonResult UpdateDbData(CDCSealViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCSEALData))
            {
                var dbData = (List<CDCSealViewModel>)Cache.Get(CacheList.CDCSEALData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    var _vSeal_Desc_AFT = model.vSeal_Desc.CheckAFT(updateTempData.vSeal_Desc);
                    if (_vSeal_Desc_AFT.Item2)
                       updateTempData.vSeal_Desc_AFT = _vSeal_Desc_AFT.Item1;
                    var _vMemo_AFT = model.vMemo.CheckAFT(updateTempData.vMemo);
                    if (_vMemo_AFT.Item2)
                       updateTempData.vMemo_AFT = _vMemo_AFT.Item1;
                    updateTempData.vAFTFlag = _vSeal_Desc_AFT.Item2 || _vMemo_AFT.Item2;
                    Cache.Invalidate(CacheList.CDCSEALData);
                    Cache.Set(CacheList.CDCSEALData, dbData);
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
            if (Cache.IsSet(CacheList.CDCSEALData))
            {
                var dbData = (List<CDCSealViewModel>)Cache.Get(CacheList.CDCSEALData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vSeal_Desc_AFT = null;
                    updateTempData.vMemo_AFT = null;
                    updateTempData.vAFTFlag = false;
                    Cache.Invalidate(CacheList.CDCSEALData);
                    Cache.Set(CacheList.CDCSEALData, dbData);
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
        public JsonResult InsertTempData(SealViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.SEALData);
                Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult UpdateTempData(SealViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    updateTempData.vSeal_Desc = model.vSeal_Desc;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult DeleteTempData(SealViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult TakeOutData(SealViewModel model, bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
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
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
            resetSealViewModel(AccessType);
            return Json(result);
        }

        #endregion

        #region private
        /// <summary>
        /// 印章預設資料
        /// </summary>
        /// <param name="AccessType"></param>
        /// <param name="AplyNo"></param>
        /// <param name="EditFlag"></param>
        private void resetSealViewModel(string AccessType, string AplyNo = null, bool EditFlag = true)
        {
            Cache.Invalidate(CacheList.SEALData);
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.SEALData, new List<SealViewModel>());
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.SEALData, Seal.GetDbDataByUnit(data.vItem, data.vAplyUnit));//只抓庫存
                }
            }
            else
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.SEALData, Seal.GetDataByAplyNo(AplyNo));//抓單號
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    if (EditFlag && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        Cache.Set(CacheList.SEALData, Seal.GetDbDataByUnit(data.vItem, data.vAplyUnit, AplyNo));//抓庫存+單號
                    }
                    else
                    {
                        Cache.Set(CacheList.SEALData, Seal.GetDataByAplyNo(AplyNo));//抓單號
                    }
                }
            }
        }
        #endregion



    }
}