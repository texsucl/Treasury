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
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 電子憑證
/// 初版作者：20180723 張家華
/// 修改歷程：20180723 張家華 
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
    public class CAController : CommonController
    {
        private ICA CA;

        public CAController()
        {
            CA = new CA();
        }

        /// <summary>
        /// 電子憑證 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, OpenPartialViewType type)
        {
            var _dActType = GetActType(type, AplyNo);
            ViewBag.CAUse = new SelectList(CA.GetCA_Use(), "Value", "Text"); 
            ViewBag.CADesc = new SelectList(CA.GetCA_Desc(), "Value", "Text"); 
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetCAViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetCAViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }
            ViewBag.dActType = _dActType;
            return PartialView();
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
            var _detail = (List<CAViewModel>)Cache.Get(CacheList.CAData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (List<CAViewModel>)Cache.Get(CacheList.CAData);
                if (data.vAccessType == AccessProjectTradeType.G.ToString() && !_data.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = CA.ApplyAudit(_data, data);
                    if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                    {
                        new TreasuryAccessController().ResetSearchData();
                    }
                }
            }
            else
            {
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
        public JsonResult InsertTempData(CAViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CAData))
            {
                var tempData = (List<CAViewModel>)Cache.Get(CacheList.CAData);
                model.vStatus = AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.CAData);
                Cache.Set(CacheList.CAData, tempData);
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
        public JsonResult UpdateTempData(CAViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CAData))
            {
                var tempData = (List<CAViewModel>)Cache.Get(CacheList.CAData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vCA_Desc = model.vCA_Desc;
                    updateTempData.vCA_Use = model.vCA_Use;
                    updateTempData.vCA_Number = model.vCA_Number;
                    updateTempData.vBank = model.vBank;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.CAData);
                    Cache.Set(CacheList.CAData, tempData);
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
        public JsonResult DeleteTempData(CAViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CAData))
            {
                var tempData = (List<CAViewModel>)Cache.Get(CacheList.CAData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.CAData);
                    Cache.Set(CacheList.CAData, tempData);
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
        public JsonResult TakeOutData(CAViewModel model,bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CAData))
            {
                var tempData = (List<CAViewModel>)Cache.Get(CacheList.CAData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
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
                    updateTempData.vtakeoutFlag = takeoutFlag;
                    Cache.Invalidate(CacheList.CAData);
                    Cache.Set(CacheList.CAData, tempData);
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
            resetCAViewModel( AccessType);
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
            if (Cache.IsSet(CacheList.CAData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CAViewModel>)Cache.Get(CacheList.CAData)).OrderBy(x=>x.vItemId).ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 電子憑證預設資料
        /// </summary>
        /// <param name="ActType">修改狀態</param>
        /// <param name="AccessType">存入 or 取出</param>
        /// <param name="AplyNo">單號</param>
        private void resetCAViewModel(string AccessType , string AplyNo = null, bool ActType = true)
        {
            Cache.Invalidate(CacheList.CAData);     
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.CAData, new List<CAViewModel>());
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.CAData, CA.GetDbDataByUnit(data.vAplyUnit, AplyNo));//只抓庫存
                }              
            }
            else
            {
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.CAData, CA.GetDataByAplyNo(AplyNo));//抓單號
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    if (ActType && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        Cache.Set(CacheList.CAData, CA.GetDbDataByUnit(data.vAplyUnit, AplyNo));//抓庫存+單號
                    }
                    else
                    {
                        Cache.Set(CacheList.CAData, CA.GetDataByAplyNo(AplyNo));//抓單號
                    }
                }
            }
        }

    }
}