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
        private IEstate Estate;
        private ITreasuryAccess TreasuryAccess;

        public EstateController()
        {
            Estate = new Estate();
            TreasuryAccess = new TreasuryAccess();
        }

        /// <summary>
        /// 不動產 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data)
        {
            ViewBag.ESTATE_From_No = new SelectList(Estate.GetEstateFromNo(), "Value", "Text");
            ViewBag.dActType = AplyNo.IsNullOrWhiteSpace();
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (data.vAccessType == AccessProjectTradeType.P.ToString())
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(), "Value", "Text");
                }
                else if (data.vAccessType == AccessProjectTradeType.G.ToString())
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(data.vAplyUnit), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(data.vAplyUnit), "Value", "Text");
                }                
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetEstateViewModel();
            }
            else
            {
                ViewBag.ESTATE_Book_No = new SelectList(new List<SelectOption>(), "Value", "Text");
                ViewBag.ESTATE_Building_Name = new SelectList(new List<SelectOption>(), "Value", "Text");
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);
                resetEstateViewModel(AplyNo);
            }
            return PartialView();
        }

        [HttpPost]
        public JsonResult ApplyTempData(EstateModel model)
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            var _detail = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;
                var _data = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                _data.vItem_Book = model;
                _data.vDetail = _detail;
                List<EstateViewModel> _datas = new List<EstateViewModel>();
                _datas.Add(_data);
                result = Estate.ApplyAudit(_datas, data);
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetItemBook(int groupNo,bool accessType)
        {
            MSGReturnModel<EstateModel> result = new MSGReturnModel<EstateModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (groupNo == 0 && Cache.IsSet(CacheList.ESTATEAllData))
            {
                var data = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                result.RETURN_FLAG = true;
                result.Datas = data.vItem_Book;
            }
            else if (groupNo == -1)
            {
                Cache.Invalidate(CacheList.ESTATEData);
                Cache.Set(CacheList.ESTATEData, new List<EstateDetailViewModel>());
                result.RETURN_FLAG = false;
            }
            else
            {
                if (Cache.IsSet(CacheList.TreasuryAccessViewData) && accessType)
                {
                    TreasuryAccessViewModel viewdata = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data = Estate.GetDataByGroupNo(groupNo, viewdata.vAplyUnit);
                    Cache.Invalidate(CacheList.ESTATEData);
                    Cache.Set(CacheList.ESTATEData, _data);
                }
                var data = Estate.GetItemBook(groupNo);
                if (Cache.IsSet(CacheList.ESTATEAllData) && !data.BOOK_NO.IsNullOrWhiteSpace())
                {
                    var vdata = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                    vdata.vItem_Book = data;
                    Cache.Invalidate(CacheList.ESTATEAllData);
                    Cache.Set(CacheList.ESTATEAllData, vdata);
                    result.RETURN_FLAG = true;
                    result.Datas = data;
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
        public JsonResult InsertTempData(EstateDetailViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
                model.vStatus = AccessInventoryTyp._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.ESTATEData);
                Cache.Set(CacheList.ESTATEData, tempData);
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
        public JsonResult UpdateTempData(EstateDetailViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vEstate_From_No = model.vEstate_From_No;
                    updateTempData.vEstate_Date = model.vEstate_Date;
                    updateTempData.vOwnership_Cert_No = model.vOwnership_Cert_No;
                    updateTempData.vLand_Building_No = model.vLand_Building_No;
                    updateTempData.vHouse_No = model.vHouse_No;
                    updateTempData.vEstate_Seq = model.vEstate_Seq;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.ESTATEData);
                    Cache.Set(CacheList.ESTATEData, tempData);
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
        public JsonResult DeleteTempData(EstateDetailViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.ESTATEData);
                    Cache.Set(CacheList.ESTATEData, tempData);
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
        public JsonResult TakeOutData(EstateDetailViewModel model,bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null)
                {
                    if (takeoutFlag)
                    {
                        updateTempData.vStatus = AccessInventoryTyp._4.GetDescription();                    
                    }
                    else
                    {
                        updateTempData.vStatus = AccessInventoryTyp._1.GetDescription();
                    }
                    updateTempData.vtakeoutFlag = takeoutFlag;
                    Cache.Invalidate(CacheList.ESTATEData);
                    Cache.Set(CacheList.ESTATEData, tempData);
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
            resetEstateViewModel();
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
            if (Cache.IsSet(CacheList.ESTATEData))
                return Json(jdata.modelToJqgridResult((List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData)));
            return null;
        }

        /// <summary>
        /// 不動產預設資料
        /// </summary>
        /// <param name="AccessType"></param>
        /// <param name="AplyNo"></param>
        private void resetEstateViewModel(string AplyNo = null)
        {
            Cache.Invalidate(CacheList.ESTATEAllData);
            Cache.Invalidate(CacheList.ESTATEData);          
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Set(CacheList.ESTATEAllData, new EstateViewModel());
                Cache.Set(CacheList.ESTATEData, new List<EstateDetailViewModel>());
            }
            else
            {
                var data = Estate.GetDataByAplyNo(AplyNo);
                Cache.Set(CacheList.ESTATEAllData, data);
                Cache.Set(CacheList.ESTATEData, data.vDetail);
            }
        }

    }
}