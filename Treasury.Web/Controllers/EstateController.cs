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
using System.IO;
using System;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 不動產權狀
/// 初版作者：20180628 張家華
/// 修改歷程：20180628 張家華 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：侯蔚鑫
/// 需求單號：
/// 修改內容：資料異動功能-不動產權狀
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
            ViewBag.OPVT = type;
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
        /// 不動產 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCEstateViewModel>)Estate.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            ViewBag.dEstate_Building_Name = new SelectList(Estate.GetBuildName(), "Value", "Text");
            ViewBag.dEstate_From_No = new SelectList(Estate.GetEstateFromNo(), "Value", "Text");
            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCEstateData);
            Cache.Set(CacheList.CDCEstateData, _data);
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
        /// 對資料庫進行大樓名稱的模糊比對
        /// </summary>
        /// <param name="building_Name">大樓名稱</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckItemBook(string building_Name)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
            if (!building_Name.IsNullOrWhiteSpace())
            {
                result = Estate.GetCheckItemBook(building_Name);              
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
                var tempData2 = tempData.Where(x => x.vItemId != model.vItemId).ToList();
                bool sameFlag = //位(字號;地/建號;門牌號;流水號/編號等欄位)建置相同值時,系統提醒建相同資料的訊息(但不影響資料的建置)
                    tempData2.Where(x => x.vOwnership_Cert_No != null).Where(x => x.vOwnership_Cert_No?.Trim() == model.vOwnership_Cert_No?.Trim(), !model.vOwnership_Cert_No.IsNullOrWhiteSpace()).Any() ||
                    tempData2.Where(x => x.vLand_Building_No != null).Where(x => x.vLand_Building_No?.Trim() == model.vLand_Building_No?.Trim(), !model.vLand_Building_No.IsNullOrWhiteSpace()).Any() ||
                    tempData2.Where(x => x.vHouse_No != null).Where(x => x.vHouse_No?.Trim() == model.vHouse_No?.Trim() , !model.vHouse_No.IsNullOrWhiteSpace()).Any() ||
                    tempData2.Where(x => x.vEstate_Seq != null).Where(x => x.vEstate_Seq?.Trim() == model.vEstate_Seq?.Trim() , !model.vEstate_Seq.IsNullOrWhiteSpace()).Any();
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
                    var tempData2 = tempData.Where(x => x.vItemId != updateTempData.vItemId).ToList();
                    bool sameFlag = //位(字號;地/建號;門牌號;流水號/編號等欄位)建置相同值時,系統提醒建相同資料的訊息(但不影響資料的建置)
                        ((!model.vOwnership_Cert_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vOwnership_Cert_No != null).Where(x => x.vOwnership_Cert_No?.Trim() == model.vOwnership_Cert_No?.Trim()).Any()) ||
                        ((!model.vLand_Building_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vLand_Building_No != null).Where(x => x.vLand_Building_No?.Trim() == model.vLand_Building_No?.Trim()).Any()) ||
                        ((!model.vHouse_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vHouse_No != null).Where(x => x.vHouse_No?.Trim() == model.vHouse_No?.Trim()).Any()) ||
                        ((!model.vEstate_Seq.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vEstate_Seq != null).Where(x => x.vEstate_Seq?.Trim() == model.vEstate_Seq?.Trim()).Any());
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
                    ((List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData))
                    //.OrderBy(x => x.vItemId).ToList()
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
            if (Cache.IsSet(CacheList.CDCEstateData))
                return Json(jdata.modelToJqgridResult(
                    ((List<CDCEstateViewModel>)Cache.Get(CacheList.CDCEstateData))
                    .OrderBy(x => x.vPut_Date) //入庫日期
                    .ThenBy(x => x.vAply_Uid) //存入申請人
                    .ThenBy(x => x.vCharge_Dept) //權責部門
                    .ThenBy(x => x.vCharge_Sect) //權責科別
                    .ThenBy(x => x.vIB_Book_No) //存取項目冊號資料檔-冊號
                    .ThenBy(x => x.vEstate_Form_No) //狀別
                    .ToList()
                    ));
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

        /// <summary>
        /// 修改資料庫資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbData(CDCEstateViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCEstateData))
            {
                var dbData = (List<CDCEstateViewModel>)Cache.Get(CacheList.CDCEstateData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId);

                //直接對DB改坐落及備註
                var _vIB_Located_Aft = model.vIB_Located.CheckAFT(updateTempData.vIB_Located); //坐落
                var _vIB_Memo_Aft = model.vIB_Memo.CheckAFT(updateTempData.vIB_Memo);  //備註
                if (_vIB_Located_Aft.Item2 || _vIB_Memo_Aft.Item2)
                {
                    var datas = Estate.UpdateDBITEM_BOOK(model, updateTempData, AccountController.CurrentUserId);
                    result.RETURN_FLAG = datas.RETURN_FLAG;
                    result.DESCRIPTION = datas.DESCRIPTION;
                    if (!datas.RETURN_FLAG)
                    {   
                        return Json(result);
                    }
                    else
                    {
                        dbData.ForEach(x => {
                            x.vIB_Located = _vIB_Located_Aft.Item1;
                            x.vIB_Memo = _vIB_Memo_Aft.Item1;
                        });
                        //updateTempData.vIB_Located = _vIB_Located_Aft.Item1;
                        //updateTempData.vIB_Memo = _vIB_Memo_Aft.Item1;
                    }
                }

                if (updateTempData != null)
                {
                    var _vEstate_Form_No_Aft = model.vEstate_Form_No.CheckAFT(updateTempData.vEstate_Form_No);
                    if (_vEstate_Form_No_Aft.Item2)
                        updateTempData.vEstate_Form_No_Aft = _vEstate_Form_No_Aft.Item1;
                    var _vEstate_Date_Aft = model.vEstate_Date.CheckAFT(updateTempData.vEstate_Date);
                    if (_vEstate_Date_Aft.Item2)
                        updateTempData.vEstate_Date_Aft = _vEstate_Date_Aft.Item1;
                    var _vOwnership_Cert_No_Aft = model.vOwnership_Cert_No.CheckAFT(updateTempData.vOwnership_Cert_No);
                    if (_vOwnership_Cert_No_Aft.Item2)
                        updateTempData.vOwnership_Cert_No_Aft = _vOwnership_Cert_No_Aft.Item1;
                    var _vLand_Building_No_Aft = model.vLand_Building_No.CheckAFT(updateTempData.vLand_Building_No);
                    if (_vLand_Building_No_Aft.Item2)
                        updateTempData.vLand_Building_No_Aft = _vLand_Building_No_Aft.Item1;
                    var _vHouse_No_Aft = model.vHouse_No.CheckAFT(updateTempData.vHouse_No);
                    if (_vHouse_No_Aft.Item2)
                        updateTempData.vHouse_No_Aft = _vHouse_No_Aft.Item1;
                    var _vEstate_Seq_Aft = model.vEstate_Seq.CheckAFT(updateTempData.vEstate_Seq);
                    if (_vEstate_Seq_Aft.Item2)
                        updateTempData.vEstate_Seq_Aft = _vEstate_Seq_Aft.Item1;
                    var _vMemo_Aft = model.vMemo.CheckAFT(updateTempData.vMemo);
                    if (_vMemo_Aft.Item2)
                        updateTempData.vMemo_Aft = _vMemo_Aft.Item1;

                    updateTempData.vAftFlag = _vEstate_Form_No_Aft.Item2 || _vEstate_Date_Aft.Item2 || _vOwnership_Cert_No_Aft.Item2 || _vLand_Building_No_Aft.Item2 || _vHouse_No_Aft.Item2 || _vEstate_Seq_Aft.Item2|| _vMemo_Aft.Item2;
                    Cache.Invalidate(CacheList.CDCEstateData);
                    Cache.Set(CacheList.CDCEstateData, dbData);
                    result.Datas = dbData.Any(x => x.vAftFlag);
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
            if (Cache.IsSet(CacheList.CDCEstateData))
            {
                var dbData = (List<CDCEstateViewModel>)Cache.Get(CacheList.CDCEstateData);
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vEstate_Form_No_Aft = null;
                    updateTempData.vEstate_Date_Aft = null;
                    updateTempData.vOwnership_Cert_No_Aft = null;
                    updateTempData.vLand_Building_No_Aft = null;
                    updateTempData.vHouse_No_Aft = null;
                    updateTempData.vEstate_Seq_Aft = null;
                    updateTempData.vMemo_Aft = null;
                    updateTempData.vAftFlag = false;
                    Cache.Invalidate(CacheList.CDCEstateData);
                    Cache.Set(CacheList.CDCEstateData, dbData);
                    result.Datas = dbData.Any(x => x.vAftFlag);
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
        /// 申請資料庫異動覆核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyDbData()
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            var _detail = (List<CDCEstateViewModel>)Cache.Get(CacheList.CDCEstateData);
            if (!_detail.Any(x => x.vAftFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                result = Estate.CDCApplyAudit(_detail.Where(x => x.vAftFlag).ToList(), data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCEstateData);
                    Cache.Set(CacheList.CDCEstateData, result.Datas);
                }
            }
            else
            {
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }


        /// <summary>
        /// 選擇檔案後點選資料上傳觸發(不動產)
        /// </summary>
        /// <param name="FileModel"></param>
        /// <returns>MSGReturnModel</returns>
        [HttpPost]
        public JsonResult Estate_Upload()
        {
            MSGReturnModel<JsonResult> result = new MSGReturnModel<JsonResult>();
            try
            {
                #region 前端無傳送檔案進來


                if (!Request.Files.AllKeys.Any())
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.upload_Not_Find.GetDescription();
                    return Json(result);
                }

                var FileModel = Request.Files["UploadedFile"];
                //string type = Request.Form["type"];

                #endregion 前端無傳送檔案進來

                #region 前端檔案大小不符或不為Excel檔案(驗證)

                //ModelState
                if (!ModelState.IsValid)
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.excel_Validate.GetDescription();
                    return Json(result);
                }

                #endregion 前端檔案大小不符或不為Excel檔案(驗證)

                #region 上傳檔案

                string pathType = Path.GetExtension(FileModel.FileName)
                                       .Substring(1); //上傳的檔案類型

                var fileName = string.Format("{0}.{1}",
                    Ref.ExcelName.Estate.GetDescription(),
                    pathType); //固定轉成此名稱

                //Cache.Invalidate(CacheList.A59ExcelName); //清除 Cache
                //Cache.Set(CacheList.A59ExcelName, fileName); //把資料存到 Cache

                #region 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                string projectFile = Server.MapPath("~/" + SetFile.FileUploads); //專案資料夾
                string path = Path.Combine(projectFile, fileName);

                FileRelated.createFile(projectFile); //檢查是否有FileUploads資料夾,如果沒有就新增

                //呼叫上傳檔案 function
                result = FileRelated.FileUpLoadinPath<JsonResult>(path, FileModel);
                if (!result.RETURN_FLAG)
                    return Json(result);

                #endregion 檢查是否有FileUploads資料夾,如果沒有就新增 並加入 excel 檔案

                #region 讀取Excel資料 使用ExcelDataReader 並且組成 json

                var stream = FileModel.InputStream;
                List<FileEstateModel> dataModel = new List<FileEstateModel>();
                var Estateresult = new FileService().getExcel(pathType, path, Ref.ExcelName.Estate);
                if (Estateresult.Item1.IsNullOrWhiteSpace())
                    dataModel = Estateresult.Item2.Cast<FileEstateModel>().ToList();
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Estateresult.Item1;
                    return Json(result);
                }
                if (dataModel.Count > 0)
                {
                    if (Cache.IsSet(CacheList.ESTATEData))
                    {
                        var tempData = (List<EstateDetailViewModel>)Cache.Get(CacheList.ESTATEData);
                        dataModel.ForEach(x =>
                        {
                            tempData.Add(new EstateDetailViewModel()
                            {
                                vItemId = Guid.NewGuid().ToString(),
                                vEstate_From_No = x.ESTATE_FORM_NO,
                                vEstate_Date = TypeTransfer.dateTimeNToString(TypeTransfer.stringToADDateTimeN(x.ESTATE_DATE)),
                                vEstate_Seq = x.ESTATE_SEQ,
                                vHouse_No = x.HOUSE_NO,
                                vLand_Building_No = x.LAND_BUILDING_NO,
                                vMemo = x.MEMO,
                                vOwnership_Cert_No = x.OWNERSHIP_CERT_NO,
                                vStatus = Ref.AccessInventoryType._3.GetDescription()
                            });
                        });
                        Cache.Invalidate(CacheList.ESTATEData);
                        Cache.Set(CacheList.ESTATEData, tempData);
                        List<string> sames = new List<string>();
                        tempData.ForEach(y =>
                        {
                            var tempData2 = tempData.Where(x => x.vItemId != y.vItemId).ToList();
                            bool sameFlag = //位(字號;地/建號;門牌號;流水號/編號等欄位)建置相同值時,系統提醒建相同資料的訊息(但不影響資料的建置)
                                ((!y.vOwnership_Cert_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vOwnership_Cert_No != null).Where(x => x.vOwnership_Cert_No?.Trim() == y.vOwnership_Cert_No?.Trim()).Any()) ||
                                ((!y.vLand_Building_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vLand_Building_No != null).Where(x => x.vLand_Building_No?.Trim() == y.vLand_Building_No?.Trim()).Any()) ||
                                ((!y.vHouse_No.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vHouse_No != null).Where(x => x.vHouse_No?.Trim() == y.vHouse_No?.Trim()).Any()) ||
                                ((!y.vEstate_Seq.IsNullOrWhiteSpace()) && tempData2.Where(x => x.vEstate_Seq != null).Where(x => x.vEstate_Seq?.Trim() == y.vEstate_Seq?.Trim()).Any());
                            if (sameFlag)
                            {
                                sames.Add($@"字號:{y.vOwnership_Cert_No},地/建號:{y.vLand_Building_No},門牌號:{y.vHouse_No},流水號/編號:{y.vEstate_Seq}");
                            }
                        });
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Ref.MessageType.upload_Success.GetDescription();
                        if (sames.Any())
                            result.DESCRIPTION = Ref.MessageType.upload_Success.GetDescription(null, "<br/>以下資料欄位驗證有重複<br/>" + string.Join("<br/>", sames));
                    }                   
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.data_Not_Compare.GetDescription();
                }

                #endregion 讀取Excel資料 使用ExcelDataReader 並且組成 json

                #endregion 上傳檔案
            }
            catch (Exception ex)
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = ex.Message;
            }
            return Json(result);
        }

    }
}