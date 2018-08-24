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
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class EstateController : CommonController
    {
        private IEstate Estate;

        public EstateController()
        {
            Estate = new Estate();
        }

        /// <summary>
        /// 不動產 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.ESTATE_From_No = new SelectList(Estate.GetEstateFromNo(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;
            var _dActType = GetActType(type, AplyNo); 
            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (data.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(), "Value", "Text");
                }
                else if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(data.vAplyUnit), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(data.vAplyUnit), "Value", "Text");
                }                
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                ResetEstateViewModel();
            }
            else
            {
                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                ViewBag.dAccess = viewModel.vAccessType;
                if (viewModel.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(), "Value", "Text");
                }
                else if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString() && _dActType)
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(viewModel.vAplyUnit, AplyNo), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(viewModel.vAplyUnit, AplyNo), "Value", "Text");
                }
                else if (viewModel.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_dActType)
                {
                    ViewBag.ESTATE_Book_No = new SelectList(Estate.GetBookNo(viewModel.vAplyUnit), "Value", "Text");
                    ViewBag.ESTATE_Building_Name = new SelectList(Estate.GetBuildName(viewModel.vAplyUnit), "Value", "Text");
                }
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                ResetEstateViewModel(AplyNo, _dActType);
                var _data = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                ViewBag.group = _data.vGroupNo;
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
            var _detail = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
            if (!_detail.Any())
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                var _data = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                _data.vItem_Book = model;
                _data.vDetail = _detail;
                List<EstateViewModel> _datas = new List<EstateViewModel>();
                _datas.Add(_data);
                if (data.vAccessType == Ref.AccessProjectTradeType.G.ToString() && !_detail.Any(x => x.vtakeoutFlag))
                {
                    result.DESCRIPTION = "無申請任何資料";
                }
                else
                {
                    result = Estate.ApplyAudit(_datas, data);
                    if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                    {
                        new TreasuryAccessController().ResetSearchData();
                    }
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
        /// 變更大樓名稱時抓取項目測號資料AND明細資料
        /// </summary>
        /// <param name="groupNo">存取項目測號群組編號</param>
        /// <param name="accessType">存入(true)或取出(flase)</param>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetItemBook(int groupNo,bool accessType,string aplyNo)
        {
            MSGReturnModel<EstateModel> result = new MSGReturnModel<EstateModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (groupNo == 0 && Cache.IsSet(CacheList.ESTATEAllData)) //單純檢視畫面
            {
                var data = (EstateViewModel)Cache.Get(CacheList.ESTATEAllData);
                result.RETURN_FLAG = true;
                result.Datas = data.vItem_Book;
            }
            else if (groupNo == -1) //大樓名稱選擇為第一項(空白),需清除明細資料
            {
                Cache.Invalidate(CacheList.ESTATEData);
                Cache.Set(CacheList.ESTATEData, new List<EstateDetailViewModel>());
                result.RETURN_FLAG = false;
            }
            else
            {
                if (Cache.IsSet(CacheList.TreasuryAccessViewData) && !accessType) //取出狀態
                {
                    TreasuryAccessViewModel viewdata = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                    var _data = Estate.GetDataByGroupNo(groupNo, viewdata.vAplyUnit, aplyNo);
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
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
                bool sameFlag = //位(字號;地/建號;門牌號;流水號/編號等欄位)建置相同值時,系統提醒建相同資料的訊息(但不影響資料的建置)
                    tempData.Where(x => x.vOwnership_Cert_No?.Trim() == model.vLand_Building_No?.Trim(), !model.vOwnership_Cert_No.IsNullOrWhiteSpace()).Any() ||
                    tempData.Where(x => x.vLand_Building_No?.Trim() == model.vLand_Building_No?.Trim(), !model.vLand_Building_No.IsNullOrWhiteSpace()).Any() ||
                    tempData.Where(x => x.vHouse_No?.Trim() == model.vHouse_No?.Trim() , !model.vHouse_No.IsNullOrWhiteSpace()).Any() ||
                    tempData.Where(x => x.vEstate_Seq?.Trim() == model.vEstate_Seq?.Trim() , !model.vEstate_Seq.IsNullOrWhiteSpace()).Any();
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.ESTATEData);
                Cache.Set(CacheList.ESTATEData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription() + (sameFlag? "</br>您建置的明細資料重複敬請確認,謝謝!" : string.Empty);
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
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    var tempData2 = tempData.Where(x => x.vItemId == updateTempData.vItemId).ToList();
                    bool sameFlag = //位(字號;地/建號;門牌號;流水號/編號等欄位)建置相同值時,系統提醒建相同資料的訊息(但不影響資料的建置)
                        tempData2.Where(x => x.vOwnership_Cert_No?.Trim() == model.vLand_Building_No?.Trim(), !model.vOwnership_Cert_No.IsNullOrWhiteSpace()).Any() ||
                        tempData2.Where(x => x.vLand_Building_No?.Trim() == model.vLand_Building_No?.Trim(), !model.vLand_Building_No.IsNullOrWhiteSpace()).Any() ||
                        tempData2.Where(x => x.vHouse_No?.Trim() == model.vHouse_No?.Trim(), !model.vHouse_No.IsNullOrWhiteSpace()).Any() ||
                        tempData2.Where(x => x.vEstate_Seq?.Trim() == model.vEstate_Seq?.Trim(), !model.vEstate_Seq.IsNullOrWhiteSpace()).Any();
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
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription() + (sameFlag ? "</br>您建置的明細資料重複敬請確認,謝謝!" : string.Empty);
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
        public JsonResult DeleteTempData(EstateDetailViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
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
        public JsonResult TakeOutData(EstateDetailViewModel model,bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ESTATEData))
            {
                var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
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
                    Cache.Invalidate(CacheList.ESTATEData);
                    Cache.Set(CacheList.ESTATEData, tempData);
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
            ResetEstateViewModel();
            return Json(new MSGReturnModel<string>());
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
                return Json(jdata.modelToJqgridResult(
                    ((List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData)).OrderBy(x => x.vItemId).ToList()));
            return null;
        }

        /// <summary>
        /// 不動產預設資料
        /// </summary>
        /// <param name="AccessType"></param>
        /// <param name="AplyNo"></param>
        private void ResetEstateViewModel( string AplyNo = null,bool EditFlag = false)
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
                var data = Estate.GetDataByAplyNo(AplyNo, EditFlag);
                Cache.Set(CacheList.ESTATEAllData, data);
                Cache.Set(CacheList.ESTATEData, data.vDetail);
            }
        }

    }
}