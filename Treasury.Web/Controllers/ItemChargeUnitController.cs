using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Treasury.Web.Enum;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;

namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class ItemChargeUnitController : CommonController
    {
        // GET: ItemChargeUnit
        private IItemChargeUnit ItemChargeUnit;

        public ItemChargeUnitController()
        {
            ItemChargeUnit = new ItemChargeUnit();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/ItemChargeUnit/");
            List<SelectOption> dCharge_Dept = new List<SelectOption>();
            List<SelectOption> dCharge_Dept_MOD = new List<SelectOption>();
            List<SelectOption> dTrea_Item= new List<SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };
            
            var dResult_MOD = ItemChargeUnit.FirstDropDown();
            var dResult = ItemChargeUnit.FirstDropDown();
            dCharge_Dept = dResult.Item1;
            dTrea_Item = dResult.Item2;
            dCharge_Dept.Insert(0, All);
            dTrea_Item.Insert(0, All);

            //ViewBag.dTREA_ITEM_NAME = new SelectList(new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME", true), "Value", "Text"); 
            //ViewBag.dTREA_ITEM_NAME_MOD = new SelectList(new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME"), "Value", "Text");
            ViewBag.dTREA_ITEM = new SelectList(dTrea_Item, "Value", "Text");
            ViewBag.dTREA_ITEM_MOD = new SelectList(dResult_MOD.Item2, "Value", "Text");
            ViewBag.dCHARGE_DEPT = new SelectList(dCharge_Dept, "Value", "Text");
            ViewBag.dCHARGE_DEPT_MOD = new SelectList(dResult_MOD.Item1, "Value", "Text");
            ViewBag.dYN_FLAG_MOD = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG"), "Value", "Text");
            return View();
        }

        [HttpPost]
        public JsonResult Change(string Charge_Dept, string Charge_Sect)
        {
            var All = new SelectOption() { Text = "All", Value = "All" };
            var result = ItemChargeUnit.DialogSelectedChange(Charge_Dept, Charge_Sect);
            if(result.Item1.Count > 0)
                result.Item1.Insert(0, All);
            if (result.Item2.Count > 0)
                result.Item2.Insert(0, All);
            return Json(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(ItemChargeUnitSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.ItemChargeUnitSearchData);
            Cache.Set(CacheList.ItemChargeUnitSearchData, searchModel);

            var datas = ItemChargeUnit.GetSearchData(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.ItemChargeUnitSearchDetailViewData);
                Cache.Set(CacheList.ItemChargeUnitSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢異動紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeRecord()
        {
            List<SelectOption> dTrea_Item_CR = new List<SelectOption>();
            List<SelectOption> dCharge_Dept_CR = new List<SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };
            var dResult = ItemChargeUnit.FirstDropDown();
            dTrea_Item_CR = dResult.Item2;
            dTrea_Item_CR.Insert(0, All);
            dCharge_Dept_CR = dResult.Item1;
            dCharge_Dept_CR.Insert(0, All);
            ViewBag.dTREA_ITEM_CR = new SelectList(dTrea_Item_CR, "Value", "Text");
            ViewBag.dCHARGE_DEPT_CR = new SelectList(dCharge_Dept_CR, "Value", "Text");
            ViewBag.dAppr_Status = new SelectList(new Service.Actual.Common().GetSysCode("APPR_STATUS", true), "Value", "Text");
            return PartialView();
        }

        public ActionResult ChangeRecordView(string AplyNo, ItemChargeUnitChangeRecordSearchViewModel data)
        {
            List<ItemChargeUnitChangeRecordSearchDetailViewModel> result = new List<ItemChargeUnitChangeRecordSearchDetailViewModel>();
            var _data = (List<ItemChargeUnitChangeRecordSearchDetailViewModel>)ItemChargeUnit.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.ItemChargeUnitChangeRecordSearchDetailViewData);
            Cache.Set(CacheList.ItemChargeUnitChangeRecordSearchDetailViewData, _data);

            ViewBag.hAplyNo = AplyNo;
            return PartialView();
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITinItem>> result = new MSGReturnModel<IEnumerable<ITinItem>>();
            if (Cache.IsSet(CacheList.ItemChargeUnitSearchData) && Cache.IsSet(CacheList.ItemChargeUnitSearchDetailViewData))
            {
                var SearchModel = (ItemChargeUnitSearchViewModel)Cache.Get(CacheList.ItemChargeUnitSearchData);
                SearchModel.vCUSER_ID = AccountController.CurrentUserId;
                var _data = (List<ItemChargeUnitSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitSearchDetailViewData);
                result = ItemChargeUnit.TinApplyAudit(_data.Where(x => x.vEXEC_ACTION != null && x.vDATA_STATUS == "1").ToList(), SearchModel);
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
        public JsonResult ResetTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var searchModel = (ItemChargeUnitSearchViewModel)Cache.Get(CacheList.ItemChargeUnitSearchData);
            var _data = ItemChargeUnit.GetSearchData(searchModel);
            Cache.Invalidate(CacheList.ItemChargeUnitSearchDetailViewData);
            Cache.Set(CacheList.ItemChargeUnitSearchDetailViewData, _data);

            return Json(result);
        }

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <returns></returns>
        //public JsonResult ResetTempData(string AplyNo)
        //{
        //    MSGReturnModel<string> result = new MSGReturnModel<string>();
        //    var searchModel = (ItemChargeUnitSearchViewModel)Cache.Get(CacheList.ItemChargeUnitSearchData);
        //    var datas = ItemChargeUnit.ResetData(AplyNo, searchModel, AccountController.CurrentUserId);
        //}

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(ItemChargeUnitInsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemChargeUnitSearchDetailViewData))
            {
                var tempData = (List<ItemChargeUnitSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitSearchDetailViewData);
                var _TREA_ITEM_NAME = new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME");
                var _DATA_STATUS = new Service.Actual.Common().GetSysCode("DATA_STATUS");
                var _EMPS = new Service.Actual.Common().GetEmps();
                var _DEPTS = new Service.Actual.Common().GetDepts();
                var cUserId = AccountController.CurrentUserId;
                var _ItemChargeUnitSearchDetailVM = new ItemChargeUnitSearchDetailViewModel()
                {
                    vCHARGE_UNIT_ID = model.vCHARGE_UNIT_ID,
                    vTREA_ITEM_NAME = model.vTREA_ITEM_NAME,
                    vTREA_ITEM_NAME_VALUE = model.vTREA_ITEM_NAME_VALUE,
                    vCHARGE_DEPT = model.vCHARGE_DEPT,
                    vCHARGE_DEPT_VALUE = !model.vCHARGE_DEPT.IsNullOrWhiteSpace() ? _EMPS.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == model.vCHARGE_DEPT)?.DPT_NAME?.Trim() : null,
                    vCHARGE_SECT = model.vCHARGE_SECT,
                    vCHARGE_SECT_VALUE = !model.vCHARGE_SECT.IsNullOrWhiteSpace() ? _EMPS.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == model.vCHARGE_SECT)?.DPT_NAME?.Trim() : null,
                    vIS_MAIL_DEPT_MGR = model.vIS_MAIL_DEPT_MGR,
                    vIS_MAIL_SECT_MGR = model.vIS_MAIL_SECT_MGR,
                    vCHARGE_UID = model.vCHARGE_UID,
                    vCHARGE_NAME = !model.vCHARGE_UID.IsNullOrWhiteSpace() ? _EMPS.FirstOrDefault(y => y.USR_ID == model.vCHARGE_UID)?.EMP_NAME?.Trim() : null,
                    vDATA_STATUS = "1",
                    vDATA_STATUS_VALUE = _DATA_STATUS.FirstOrDefault(x => x.Value == "1")?.Text?.Trim(),
                    //vFREEZE_UID = cUserId,
                    //vFREEZE_NAME = _EMPS.FirstOrDefault(y => y.USR_ID == cUserId)?.EMP_NAME?.Trim(),
                    vEXEC_ACTION = "A",
                    vEXEC_ACTION_VALUE = "新增",
                    vIS_DISABLED = "N"
                };
                tempData.Add(_ItemChargeUnitSearchDetailVM);
                Cache.Invalidate(CacheList.ItemChargeUnitSearchDetailViewData);
                Cache.Set(CacheList.ItemChargeUnitSearchDetailViewData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }

                return Json(result);
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(ItemChargeUnitInsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemChargeUnitSearchDetailViewData))
            {
                var tempData = (List<ItemChargeUnitSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitSearchDetailViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.vCHARGE_UNIT_ID == model.vCHARGE_UNIT_ID);
                if (updateTempData != null)
                {
                    var _EMPS = new Service.Actual.Common().GetEmps();
                    var _TREA_ITEM_NAME = new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME");

                    updateTempData.vEXEC_ACTION = updateTempData.vEXEC_ACTION == "A" ? "A" : "U";
                    updateTempData.vEXEC_ACTION_VALUE = updateTempData.vEXEC_ACTION_VALUE == "A" ? "新增" : "修改";
                    updateTempData.vIS_MAIL_DEPT_MGR = model.vIS_MAIL_DEPT_MGR;
                    updateTempData.vIS_MAIL_SECT_MGR = model.vIS_MAIL_SECT_MGR;

                    Cache.Invalidate(CacheList.ItemChargeUnitSearchDetailViewData);
                    Cache.Set(CacheList.ItemChargeUnitSearchDetailViewData, tempData);
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
        /// 刪除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(ItemChargeUnitInsertViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemChargeUnitSearchDetailViewData))
            {
                var tempData = (List<ItemChargeUnitSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitSearchDetailViewData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vCHARGE_UNIT_ID == model.vCHARGE_UNIT_ID);
                if (deleteTempData != null)
                {
                    //判斷是否新增資料
                    if (deleteTempData.vEXEC_ACTION == "A")
                    {
                        tempData.Remove(deleteTempData);
                    }
                    else
                    {
                        deleteTempData.vEXEC_ACTION = "D";
                        deleteTempData.vEXEC_ACTION_VALUE = "刪除";
                    }
                    Cache.Invalidate(CacheList.ItemChargeUnitSearchDetailViewData);
                    Cache.Set(CacheList.ItemChargeUnitSearchDetailViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                    result.Datas = tempData.Where(x => x.vEXEC_ACTION != null).Any();
                }
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Search":
                    var Datas = (List<ItemChargeUnitSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(Datas.OrderBy(x => x.vCHARGE_UNIT_ID).ToList()));
                case "RecordSearch":
                    var RecordDatas = (List<ItemChargeUnitChangeRecordSearchDetailViewModel>)Cache.Get(CacheList.ItemChargeUnitChangeRecordSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(RecordDatas.OrderBy(x => x.vAply_No).ToList()));
            }
            return null;
        }
    }
}