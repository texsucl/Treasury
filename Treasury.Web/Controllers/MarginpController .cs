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

       /// <summary>
       /// 抓取存入保證金物品名稱
       /// </summary>
       /// <returns></returns>
        public JsonResult GetMarginpItem(string MARGIN_ITEM)
        {
            var MarginpItems = new List<string>();
            var data = Marginp.GetMarginpItem();
            switch (MARGIN_ITEM.ToString()) {
                case "1":
                    MarginpItems = new List<string>(){ "1", "2" };
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
            transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                transType(model);
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
            transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vMarginp_Take_Of_Type = model.vMarginp_Take_Of_Type;
                    updateTempData.vMarginp_Trad_Partners = model.vMarginp_Trad_Partners;
                    updateTempData.vMarginp_Item = model.vItemId;
                    updateTempData.vMarginp_Amount = model.vMarginp_Amount;
                    updateTempData.vMarginp_Item = model.vMarginp_Item;
                    updateTempData.vMarginp_Item_Issuer = model.vMarginp_Item_Issuer;
                    updateTempData.vMarginp_Pledge_Item_No = model.vMarginp_Pledge_Item_No;
                    updateTempData.vMarginp_Effective_Date_B = model.vMarginp_Effective_Date_B;
                    updateTempData.vMarginp_Effective_Date_E = model.vMarginp_Effective_Date_E;
                    updateTempData.vDescription = model.vDescription;
                    updateTempData.vMemo = model.vMemo;
                    updateTempData.vMarginp_Book_No = model.vMarginp_Book_No;                 
                    transType(updateTempData);
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
            transType(model);
            if (Cache.IsSet(CacheList.MarginpData))
            {
                var tempData = (List<MarginpViewModel>)Cache.Get(CacheList.MarginpData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.MarginpData);
                    Cache.Set(CacheList.MarginpData, tempData);
                    transType(model);
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
            transType(model);
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
        private void transType(MarginpViewModel model)
        {
            if (model != null)
            {
                var date_B = TypeTransfer.stringToDateTimeN(model.vMarginp_Effective_Date_B);
                model.vMarginp_Effective_Date_B_2 =
                    (date_B == null ? null : date_B.Value.DateToTaiwanDate(9));
                var date_E = TypeTransfer.stringToDateTimeN(model.vMarginp_Effective_Date_E);
                model.vMarginp_Effective_Date_E_2 =
                    (date_E == null ? null : date_E.Value.DateToTaiwanDate(9));
            } 
        }
    }
 }