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
namespace Treasury.WebControllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class SealController : CommonController
    {
        private ISeal Seal;
        private ITreasuryAccess TreasuryAccess;

        public SealController()
        {
            Seal = new Seal();
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 印章 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data)
        {
            var _dActType = false;
            if (AplyNo.IsNullOrWhiteSpace())
            {
                _dActType = AplyNo.IsNullOrWhiteSpace();
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetSealViewModel(data.vAccessType);
            }
            else
            {
                _dActType = TreasuryAccess.GetActType(AplyNo,AccountController.CurrentUserId, Aply_Appr_Type);
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetSealViewModel(viewModel.vAccessType, AplyNo , _dActType);
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
                data.vCreateUid = AccountController.CurrentUserId;
                var _data = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                if (data.vAccessType == AccessProjectTradeType.G.ToString() && !_data.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = Seal.ApplyAudit(_data, data);
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
        public JsonResult InsertTempData(SealViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                model.vStatus = AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.SEALData);
                Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult UpdateTempData(SealViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vSeal_Desc = model.vSeal_Desc;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult DeleteTempData(SealViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
        public JsonResult TakeOutData(SealViewModel model,bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.SEALData))
            {
                var tempData = (List<SealViewModel>)Cache.Get(CacheList.SEALData);
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
                    Cache.Invalidate(CacheList.SEALData);
                    Cache.Set(CacheList.SEALData, tempData);
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
            resetSealViewModel(AccessType);
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
            if (Cache.IsSet(CacheList.SEALData))
                return Json(jdata.modelToJqgridResult(
                    ((List<SealViewModel>)Cache.Get(CacheList.SEALData)).OrderBy(x=>x.vItemId).ToList()
                    ));
            return null;
        }

        /// <summary>
        /// 印章預設資料
        /// </summary>
        /// <param name="AccessType"></param>
        /// <param name="AplyNo"></param>
        private void resetSealViewModel(string AccessType, string AplyNo = null, bool EditFlag = false)
        {
            Cache.Invalidate(CacheList.SEALData);     
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.SEALData, new List<SealViewModel>());
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.SEALData, Seal.GetDbDataByUnit(data.vItem, data.vAplyUnit));//只抓庫存
                }              
            }
            else
            {
                if (AccessType == AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.SEALData, Seal.GetDataByAplyNo(AplyNo));
                }
                if (AccessType == AccessProjectTradeType.G.ToString())
                {
                    Cache.Set(CacheList.SEALData, Seal.GetDbDataByUnit(data.vItem, data.vAplyUnit , AplyNo));//只抓庫存
                }
            }
        }

    }
}