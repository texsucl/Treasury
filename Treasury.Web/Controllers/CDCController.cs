using Treasury.WebActionFilter;
using System;
using Treasury.Web.Properties;
using System.Web.Mvc;
using Treasury.Web.Service.Interface;
using Treasury.Web.Service.Actual;
using System.Collections.Generic;
using Treasury.WebUtility;
using Treasury.Web.Controllers;
using Treasury.Web.ViewModels;
using System.Linq;
using Treasury.Web.Enum;
using static Treasury.Web.Enum.Ref;
/// <summary>
/// 功能說明：資料查詢異動作業
/// 初版作者：20180828 卓建毅
/// 修改歷程：2018 
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
    public class CDCController : CommonController
    {
        private ICDC CDC;
        private IEstate Estate;
        public CDCController()
        {
            CDC = new CDC();
            Estate = new Estate();
        }

        /// <summary>
        /// 資料查詢異動作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var All = new SelectOption() { Text = "All", Value = "All" };
            var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/CDC/");
            var viewModel = CDC.GetItemId();
            return View(viewModel);
        }

        /// <summary>
        /// 資料庫異動覆核作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Appr()
        {
            ViewBag.opScope = GetopScope("~/CDC/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            return View();
        }

        /// <summary>
        /// 權責查詢異動作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult ChargeIndex()
        {
            var _CustodyFlag = Convert.ToBoolean(Session["CustodyFlag"]);
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.CustodyFlag = _CustodyFlag;
            ViewBag.opScope = GetopScope("~/CDC/");
            var data = CDC.GetChargeData();
            Cache.Invalidate(CacheList.CDCChargeSearchViewModel);
            Cache.Set(CacheList.CDCChargeSearchViewModel, data);
            var _dept = data.ChargeData.Select(x => x.vCharge_Dept).Distinct()
                .Select(x => new SelectOption()
                {
                    Value = x,
                    Text = data.ChargeData.FirstOrDefault(y => y.vCharge_Dept == x)?.vCharge_Dept_Name
                }).ToList();
            _dept.Insert(0, new SelectOption() { Text = " ", Value = " " });
            ViewBag.vCharge_Dept = new SelectList(_dept
                , "Value", "Text");
            return View(); 
        }

        /// <summary>
        /// 權責異動覆核作業 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult ChargeAppr()
        {
            ViewBag.opScope = GetopScope("~/CDC/");
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            ViewBag.hCREATE_User = userInfo.EMP_ID;
            ViewBag.hCREATE_Dep = userInfo.DPT_ID;
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchAppr(CDCApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.CDCApprSearchData);
            Cache.Set(CacheList.CDCApprSearchData, searchModel);
            var datas = CDC.GetApprSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                Cache.Set(CacheList.CDCApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }

            return Json(result);
        }

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Appraisal(List<string> AplyNos)
        {
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCApprSearchData);
                result = CDC.Approved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Reject(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCApprSearchData);
                result = CDC.Reject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCApprSearchDetailViewData, result.Datas);
                }
            }
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
                case "Appr":
                    var ApprDatas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCApprSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(ApprDatas));
                case "ChargeAppr":
                    var ChargeApprDatas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCChargeApprSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(ChargeApprDatas));
            }
            return null;
        }

        /// <summary>
        /// 由權責單位查詢 明細資料
        /// </summary>
        /// <param name="treaItem"></param>
        /// <param name="dept"></param>
        /// <param name="sect"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetChargeDetailData(string treaItem, string dept, string sect)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (!EnumUtil.GetValues<TreaItemType>().Any(y => y.ToString() == treaItem))
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == treaItem);
                var datas = CDC.GetChargeDetailData(type, dept, sect);
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                switch (type)
                {
                    case TreaItemType.D1012:
                        data.BillData = new List<CDCBillViewModel>();
                        data.BillData.AddRange(datas.BillData);
                        break;
                    case TreaItemType.D1024:
                        data.CaData = new List<CDCCAViewModel>();
                        data.CaData.AddRange(datas.CaData);
                        break;
                    case TreaItemType.D1013:
                        data.DepositData = new CDCDepositViewModel();
                        data.DepositData.vDeposit_M.AddRange(datas.DepositData.vDeposit_M);
                        break;
                    case TreaItemType.D1014:
                        data.EstateData = new List<CDCEstateViewModel>();
                        data.EstateData.AddRange(datas.EstateData);
                        break;
                    case TreaItemType.D1018:
                        data.ItemImpData = new List<CDCItemImpViewModel>();
                        data.ItemImpData.AddRange(datas.ItemImpData);
                        break;
                    case TreaItemType.D1016:
                        data.MargingData = new List<CDCMargingViewModel>();
                        data.MargingData.AddRange(datas.MargingData);
                        break;
                    case TreaItemType.D1017:
                        data.MarginpData = new List<CDCMarginpViewModel>();
                        data.MarginpData.AddRange(datas.MarginpData);
                        break;
                    case TreaItemType.D1008:
                    case TreaItemType.D1009:
                    case TreaItemType.D1010:
                    case TreaItemType.D1011:
                        data.SealData = new List<CDCSealViewModel>();
                        data.SealData.AddRange(datas.SealData);
                        break;
                    case TreaItemType.D1015:
                        data.StockData = new List<CDCStockViewModel>();
                        data.StockData.AddRange(datas.StockData);
                        break;

                }
                Cache.Invalidate(CacheList.CDCChargeSearchViewModel);
                Cache.Set(CacheList.CDCChargeSearchViewModel, data);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = string.Empty;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult AllCheck(string treaItem, string checkType)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (!EnumUtil.GetValues<TreaItemType>().Any(y => y.ToString() == treaItem))
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == treaItem);
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                switch (type)
                {
                    case TreaItemType.D1012:
                        data.BillData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1024:
                        data.CaData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1013:
                        data.DepositData.vDeposit_M.ForEach(x => x.vAftFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1014:
                        data.EstateData.ForEach(x => x.vAftFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1018:
                        data.ItemImpData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1016:
                        data.MargingData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1017:
                        data.MarginpData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1008:
                    case TreaItemType.D1009:
                    case TreaItemType.D1010:
                    case TreaItemType.D1011:
                        data.SealData.ForEach(x => x.vAFTFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                    case TreaItemType.D1015:
                        data.StockData.ForEach(x => x.vAftFlag = (x.vStatus == "1" && checkType == "Y"));
                        break;
                }
                Cache.Invalidate(CacheList.CDCChargeSearchViewModel);
                Cache.Set(CacheList.CDCChargeSearchViewModel, data);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = string.Empty;
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult Check(string itemId, bool flag, string treaItem)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (!EnumUtil.GetValues<TreaItemType>().Any(y => y.ToString() == treaItem))
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == treaItem);
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                switch (type)
                {
                    case TreaItemType.D1012:
                        data.BillData.First(x => x.vItemId == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1024:
                        data.CaData.First(x => x.vItemId == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1013:
                        data.DepositData.vDeposit_M.First(x => x.vItemId == itemId).vAftFlag = flag;
                        break;
                    case TreaItemType.D1014:
                        data.EstateData.First(x => x.vItemId == itemId).vAftFlag = flag;
                        break;
                    case TreaItemType.D1018:
                        data.ItemImpData.First(x => x.vItemId == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1016:
                        data.MargingData.First(x => x.vItem_PK == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1017:
                        data.MarginpData.First(x => x.vItem_Id == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1008:
                    case TreaItemType.D1009:
                    case TreaItemType.D1010:
                    case TreaItemType.D1011:
                        data.SealData.First(x => x.vItemId == itemId).vAFTFlag = flag;
                        break;
                    case TreaItemType.D1015:
                        data.StockData.First(x => x.vItemId == itemId).vAftFlag = flag;
                        break;
                }
                Cache.Invalidate(CacheList.CDCChargeSearchViewModel);
                Cache.Set(CacheList.CDCChargeSearchViewModel, data);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = string.Empty;
            }
            return Json(result);
        }

        /// <summary>
        /// 抓取科別
        /// </summary>
        /// <param name="dept"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCharge_Sect(string dept)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                result.AddRange(data.ChargeData
                    .Where(x => x.vCharge_Dept == dept)
                    .Select(x => x.vCharge_Sect).Distinct()
                .Select(x => new SelectOption()
                {
                    Value = x,
                    Text = data.ChargeData.First(y => y.vCharge_Sect == x).vCharge_Sect_Name
                }));
            }
            return Json(result);
        }

        /// <summary>
        /// 抓取可以調整的項目
        /// </summary>
        /// <param name="dept"></param>
        /// <param name="sect"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTreaItem(string dept, string sect)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                result.AddRange(data.ChargeData
                    .Where(x => x.vCharge_Dept == dept)
                    .Where(x => x.vCharge_Sect == sect, !sect.IsNullOrWhiteSpace())
                    .Select(x => x.type)
                .Select(x => new SelectOption()
                {
                    Value = x.ToString(),
                    Text = x.GetDescription()
                }));
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢可調整部門
        /// </summary>
        /// <param name="treaItem"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCharge_Dept_AFT(string treaItem)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
            result.AddRange(CDC.GetChargeDept(treaItem));
            return Json(result);
        }

        /// <summary>
        /// 查詢可調整科別
        /// </summary>
        /// <param name="treaItem"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCharge_Sect_AFT(string treaItem, string dept, string sect)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
            result.AddRange(CDC.GetChargeSect(treaItem, dept));
            result = result.Where(x => x.Value != sect).ToList();
            return Json(result);
        }

        /// <summary>
        /// 權責單位異動申請覆核
        /// </summary>
        /// <param name="treaItem"></param>
        /// <param name="dept"></param>
        /// <param name="sect"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeAppr(string treaItem, string dept, string sect)
        {
            MSGReturnModel<List<SelectOption>> result = new MSGReturnModel<List<SelectOption>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.login_Time_Out.GetDescription();
            if (!EnumUtil.GetValues<TreaItemType>().Any(y => y.ToString() == treaItem))
            {
                result.DESCRIPTION = MessageType.parameter_Error.GetDescription();
                return Json(result);
            }
            if (Cache.IsSet(CacheList.CDCChargeSearchViewModel))
            {
                var type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == treaItem);
                var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
                var _result = CDC.ChargeAppr(data, new CDCSearchViewModel()
                {
                    CHARGE_DEPT_AFT = dept,
                    CHARGE_SECT_AFT = sect,
                    vJobProject = treaItem,
                    vCreate_Uid = AccountController.CurrentUserId
                });
                result.RETURN_FLAG = _result.RETURN_FLAG;
                result.DESCRIPTION = _result.DESCRIPTION;
            }
            return Json(result);            
        }

        /// <summary>
        /// 權責單位異動查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeSearchAppr(CDCApprSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.CDCChargeApprSearchData);
            Cache.Set(CacheList.CDCChargeApprSearchData, searchModel);
            var datas = CDC.GetChargeApprSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.CDCChargeApprSearchDetailViewData);
                Cache.Set(CacheList.CDCChargeApprSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }

            return Json(result);
        }

        /// <summary>
        /// 權責單位異動覆核
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeAppraisal(List<string> AplyNos)
        {
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCChargeApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCChargeApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCChargeApprSearchData);
                result = CDC.ChargeApproved(searchData, datas);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCChargeApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCChargeApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 權責單位異動駁回
        /// </summary>
        /// <param name="AplyNos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChargeReject(List<string> AplyNos, string apprDesc)
        {
            MSGReturnModel<List<CDCApprSearchDetailViewModel>> result =
                new MSGReturnModel<List<CDCApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (AplyNos.Any() && Cache.IsSet(CacheList.CDCChargeApprSearchDetailViewData))
            {
                var datas = (List<CDCApprSearchDetailViewModel>)Cache.Get(CacheList.CDCChargeApprSearchDetailViewData);
                foreach (var item in datas.Where(x => AplyNos.Contains(x.vAply_No)))
                {
                    item.vCheckFlag = true;
                }
                var searchData = (CDCApprSearchViewModel)Cache.Get(CacheList.CDCChargeApprSearchData);
                result = CDC.ChargeReject(searchData, datas, apprDesc);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCChargeApprSearchDetailViewData);
                    Cache.Set(CacheList.CDCChargeApprSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// jqgrid cache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetChargeCacheData(jqGridParam jdata, string type)
        {
            var _type = EnumUtil.GetValues<TreaItemType>().First(y => y.ToString() == type);
            var data = (CDCChargeViewModel)Cache.Get(CacheList.CDCChargeSearchViewModel);
            switch (_type)
            {
                case TreaItemType.D1012:
                    return Json(jdata.modelToJqgridResult(data.BillData));
                case TreaItemType.D1024:
                    return Json(jdata.modelToJqgridResult(data.CaData));
                case TreaItemType.D1013:
                    return Json(jdata.modelToJqgridResult(data.DepositData.vDeposit_M));
                case TreaItemType.D1014:
                    return Json(jdata.modelToJqgridResult(data.EstateData));
                case TreaItemType.D1018:
                    return Json(jdata.modelToJqgridResult(data.ItemImpData));
                case TreaItemType.D1016:
                    return Json(jdata.modelToJqgridResult(data.MargingData));
                case TreaItemType.D1017:
                    return Json(jdata.modelToJqgridResult(data.MarginpData));
                case TreaItemType.D1008:
                case TreaItemType.D1009:
                case TreaItemType.D1010:
                case TreaItemType.D1011:
                    return Json(jdata.modelToJqgridResult(data.SealData));
                case TreaItemType.D1015:
                    return Json(jdata.modelToJqgridResult(data.StockData));
            }
            return null;
        }
    }
}