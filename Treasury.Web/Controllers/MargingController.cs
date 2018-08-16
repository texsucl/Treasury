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
namespace Treasury.WebControllers
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
            ViewBag.dMargin_Dep_Type = new SelectList(Marging.GetMargingType(), "Value", "Text");
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
                var _data = (List<MargingViewModel>)Cache.Get(CacheList.MargingData);

                //取出勾選判斷
                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    //判斷至少勾選一筆資料
                    var vDetail = ((List<MargingViewModel>)Cache.Get(CacheList.MargingData)).Where(x => x.vTakeoutFlag == true).ToList();
                    if (!vDetail.Any())
                    {
                        _data = new List<MargingViewModel>();
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
        public JsonResult InsertTempData(MargingViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingViewModel>)Cache.Get(CacheList.MargingData);
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
        public JsonResult UpdateTempData(MargingViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingViewModel>)Cache.Get(CacheList.MargingData);
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
        public JsonResult DeleteTempData(MargingViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingViewModel>)Cache.Get(CacheList.MargingData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItem_PK == model.vItem_PK);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.MargingData);
                    Cache.Set(CacheList.MargingData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
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
        public JsonResult TakeOutData(MargingViewModel model, bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.MargingData))
            {
                var tempData = (List<MargingViewModel>)Cache.Get(CacheList.MargingData);
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
                    ((List<MargingViewModel>)Cache.Get(CacheList.MargingData)).OrderBy(x => x.vItem_Id).ToList()
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
                    Cache.Set(CacheList.MargingData, new List<MargingViewModel>());
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