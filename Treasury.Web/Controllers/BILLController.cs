using Treasury.WebActionFilter;
using System;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.ViewModels;
using static Treasury.Web.Enum.Ref;
using System.Linq;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 初始畫面
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
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
    public class BILLController : CommonController
    {
        private IBill Bill;

        public BILLController()
        {
            Bill = new Bill();
        }

        /// <summary>
        /// 空白票據 新增畫面
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InsertView(string AplyNo)
        {
            ViewBag.dBILL_Check_Type = new SelectList(Bill.GetCheckType(), "Value", "Text");
            var ibs = Bill.GetIssuing_Bank();
            ibs.Add(new SelectOption() { Text = "1", Value = "1" });
            ibs.Add(new SelectOption() { Text = "2", Value = "2" });
            ViewBag.dBILL_Issuing_Bank = new SelectList(ibs, "Value", "Text");
            Cache.Invalidate(CacheList.BILLTempData);
            Cache.Invalidate(CacheList.BILLDayData);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Set(CacheList.BILLTempData, new List<BillViewModel>());
                Cache.Set(CacheList.BILLDayData, (List<BillViewModel>)Bill.GetDayData());
            }
            else
            {

            }
            return PartialView();
        }

        [HttpPost]
        public JsonResult ApplyTempData(TreasuryAccessViewModel data)
        {
            MSGReturnModel<ITreaItem> result = new MSGReturnModel<ITreaItem>();
            data.vCreateUid = AccountController.CurrentUserId;
            result = Bill.ApplyAudit((List<BillViewModel>)Cache.Get(CacheList.BILLTempData), data);
            return Json(result);
        }

        /// <summary>
        /// 新增明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                model.vStatus = "預約存入";
                tempData.Add(model);
                Cache.Invalidate(CacheList.BILLTempData);
                Cache.Set(CacheList.BILLTempData, tempData);
                var dayData = (List<BillViewModel>)Bill.GetDayData();
                dayData.AddRange(tempData);
                Cache.Invalidate(CacheList.BILLDayData);
                Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
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
        public JsonResult UpdateTempData(BillViewModel model )
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var dayData =  (List<BillViewModel>)Bill.GetDayData();                
                var updateTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (updateTempData != null )
                {
                    updateTempData.vIssuingBank = model.vIssuingBank;
                    updateTempData.vCheckType = model.vCheckType;
                    updateTempData.vCheckNoTrack = model.vCheckNoTrack;
                    updateTempData.vCheckNoB = model.vCheckNoB;
                    updateTempData.vCheckNoE = model.vCheckNoE;
                    updateTempData.vCheckTotalNum = model.vCheckTotalNum;
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, tempData);
                    dayData.AddRange(tempData);
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
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
        public JsonResult DeleteTempData(BillViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.BILLTempData))
            {
                var tempData = (List<BillViewModel>)Cache.Get(CacheList.BILLTempData);
                var dayData = (List<BillViewModel>)Bill.GetDayData();
                var deleteTempData = tempData.FirstOrDefault(x => x.vItemId == model.vItemId);
                if (deleteTempData != null )
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.BILLTempData);
                    Cache.Set(CacheList.BILLTempData, tempData);
                    dayData.AddRange(tempData);
                    Cache.Invalidate(CacheList.BILLDayData);
                    Cache.Set(CacheList.BILLDayData, setBillViewModelGroup(dayData));
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
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            Cache.Invalidate(CacheList.BILLTempData);
            Cache.Invalidate(CacheList.BILLDayData);
            Cache.Set(CacheList.BILLTempData, new List<BillViewModel>());
            Cache.Set(CacheList.BILLDayData, (List<BillViewModel>)Bill.GetDayData());
            return Json(result);
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Temp":
                    if (Cache.IsSet(CacheList.BILLTempData))
                        return Json(jdata.modelToJqgridResult(setBillViewModelOrder((List<BillViewModel>)Cache.Get(CacheList.BILLTempData))));
                    break;
                case "Day":
                    if (Cache.IsSet(CacheList.BILLDayData))
                        return Json(jdata.modelToJqgridResult((List<BillViewModel>)Cache.Get(CacheList.BILLDayData)));
                    break;
            }
            return null;
        }

        private List<BillViewModel> setBillViewModelOrder(List<BillViewModel> data)
        {
            if (data.Any())
            {
                data = data.OrderBy(x => x.vRowNum).ToList();
            }
            return data;
        }

        /// <summary>
        /// 加入 小計&總計
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<BillViewModel> setBillViewModelGroup(List<BillViewModel> data)
        {
            var newResult = new List<BillViewModel>();
            if (data.Any())
            {
                var total = data.Select(x => TypeTransfer.stringToInt(x.vCheckTotalNum)).Sum();
                int rownum = 1;
                data.GroupBy(x => new { x.vIssuingBank, x.vCheckType })
                    .OrderBy(x => x.Key.vIssuingBank)
                    .ThenBy(x => x.Key.vCheckType).ToList()
                    .ForEach(x =>
                    {
                        foreach (var item in x)
                        {
                            item.vRowNum = rownum.ToString();
                            newResult.Add(item);
                            rownum += 1;
                        }
                        newResult.Add(new BillViewModel()
                        {
                            vStatus = "小計",
                            vIssuingBank = x.Key.vIssuingBank,
                            vCheckType = x.Key.vCheckType,
                            vCheckTotalNum = x.Select(y => TypeTransfer.stringToInt(y.vCheckTotalNum)).Sum().ToString()
                        });
                    });
                newResult.Add(new BillViewModel()
                {
                    vStatus = "總計",
                    vCheckTotalNum = total.ToString()
                });
            }           
            return newResult;
        }
    }
}