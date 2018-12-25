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
    public class ItemMaintainController : CommonController
    {
        // GET: ItemMaintain
        private IItemMaintain ItemMaintain;

        public ItemMaintainController()
        {
            ItemMaintain = new ItemMaintain();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/ItemMaintain/");
            var empty = new SelectOption() { Text = " ", Value = " " };
            ViewBag.dITEM_OP_TYPE = new SelectList(new Service.Actual.Common().GetSysCode("ITEM_OP_TYPE", true), "Value", "Text");
            ViewBag.dISDO_PERDAY = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG", true), "Value", "Text");
            ViewBag.dIS_DISABLED = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", true), "Value", "Text");

            var _TREA_ITEM_TYPE = new Service.Actual.Common().GetSysCode("TREA_ITEM_TYPE");
            _TREA_ITEM_TYPE.Insert(0, empty);
            var _TREA_ITEM_NAME = new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME");
            _TREA_ITEM_NAME.Insert(0, empty);

            ViewBag.dITEM_OP_TYPE_MOD = new SelectList(new Service.Actual.Common().GetSysCode("ITEM_OP_TYPE"), "Value", "Text");
            ViewBag.dYN_FLAG_MOD = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG"), "Value", "Text");
            ViewBag.dTREA_ITEM_TYPE_MOD = new SelectList(_TREA_ITEM_TYPE, "Value", "Text");
            ViewBag.dTREA_ITEM_NAME_MOD = new SelectList(_TREA_ITEM_NAME, "Value", "Text");
            ViewBag.dIS_DISABLED_MOD = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED"), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(ItemMaintainSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.ItemMaintainSearchData);
            Cache.Set(CacheList.ItemMaintainSearchData, searchModel);

            var datas = ItemMaintain.GetSearchData(searchModel);
            //if (datas.Any())
            //{
                Cache.Invalidate(CacheList.ItemMaintainSearchDetailViewData);
                Cache.Set(CacheList.ItemMaintainSearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            //}

            return Json(result);
        }

        /// <summary>
        /// 查詢異動紀錄
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeRecord()
        {
            //var _ = ItemMaintain.GetTreaItem();
            ViewBag.dITEM_OP_TYPE = new SelectList(new Service.Actual.Common().GetSysCode("ITEM_OP_TYPE", true), "Value", "Text");

            ViewBag.dAppr_Status = new SelectList(new Service.Actual.Common().GetSysCode("APPR_STATUS", true), "Value", "Text");
            return PartialView();
        }

        /// <summary>
        /// 異動資料中入庫作業類型 Change事件
        /// </summary>
        /// <param name="vTREA_OP_TYPE"></param>
        /// <returns></returns>
        public JsonResult OpTypeChange(string vTREA_OP_TYPE)
        {
            List<SelectOption> result = new List<SelectOption>();
            var All = new SelectOption() { Text = "All", Value = "All" };
            result = ItemMaintain.OpTypeSelectedChange(vTREA_OP_TYPE);
            result.Insert(0, All);
            return Json(result);
        }

        public ActionResult ChangeRecordView(string AplyNo, ItemMaintainChangeRecordSearchViewModel data)
        {
            List<ItemMaintainChangeRecordSearchDetailViewModel> result = new List<ItemMaintainChangeRecordSearchDetailViewModel>();
            var _data = (List<ItemMaintainChangeRecordSearchDetailViewModel>)ItemMaintain.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.ItemMaintainChangeRecordSearchDetailViewData);
            Cache.Set(CacheList.ItemMaintainChangeRecordSearchDetailViewData, _data);
            return PartialView();
        }

        [HttpPost]
        public JsonResult Check_Item_Name(string vItem_Name)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var tempData = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);

            if(tempData.Where(x => x.vITEM_DESC == vItem_Name && x.vIS_DISABLED == "N").Any() || ItemMaintain.Check_Item_Name(vItem_Name))
            {
                result.RETURN_FLAG = true;
            }
            else
            {
                result.RETURN_FLAG = false;
            }
            return Json(result);
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITinItem>> result = new MSGReturnModel<IEnumerable<ITinItem>>();
            if (Cache.IsSet(CacheList.ItemMaintainSearchData) && Cache.IsSet(CacheList.ItemMaintainSearchDetailViewData))
            {
                var SearchModel = (ItemMaintainSearchViewModel)Cache.Get(CacheList.ItemMaintainSearchData);
                SearchModel.vCUSER_ID = AccountController.CurrentUserId;
                var _data = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);
                result = ItemMaintain.TinApplyAudit(_data.Where(x => x.vEXEC_ACTION != null && x.vDATA_STATUS == "1").ToList(), SearchModel);
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult InsertTempData(ItemMaintainInsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemMaintainSearchDetailViewData))
            {
                var tempData = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);
                var _DATA_STATUS = new Service.Actual.Common().GetSysCode("DATA_STATUS");
                var _TREA_ITEM_TYPE = new Service.Actual.Common().GetSysCode("TREA_ITEM_TYPE");
                var _TREA_ITEM_NAME = new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME");
                var _ItemMaintainSearchDetailVM = new ItemMaintainSearchDetailViewModel()
                {
                    vITEM_ID = model.vTrea_Item_Id,
                    vITEM_OP_TYPE = model.vOp_Type,
                    vITEM_DESC = model.vItem_Desc,
                    vIS_TREA_ITEM = model.vIs_Item,
                    vTREA_ITEM_TYPE = model.vType,
                    vTREA_ITEM_TYPE_VALUE = !model.vType.IsNullOrWhiteSpace() ? _TREA_ITEM_TYPE.FirstOrDefault(x => x.Value == model.vType)?.Text?.Trim() : null,
                    vTREA_ITEM_NAME = model.vName,
                    vTREA_ITEM_NAME_VALUE = !model.vName.IsNullOrWhiteSpace() ? _TREA_ITEM_NAME.FirstOrDefault(x => x.Value == model.vName)?.Text?.Trim() : null,
                    vISDO_PERDAY = model.vIsDo_Perday,
                    vIS_DISABLED = model.vIs_Disabled,
                    vMEMO = model.vMemo,
                    vDATA_STATUS = "1",
                    vDATA_STATUS_VALUE = _DATA_STATUS.FirstOrDefault(x => x.Value == "1")?.Text?.Trim(),
                    vEXEC_ACTION = "A",
                    vEXEC_ACTION_VALUE = "新增"
                };
                tempData.Add(_ItemMaintainSearchDetailVM);
                Cache.Invalidate(CacheList.ItemMaintainSearchDetailViewData);
                Cache.Set(CacheList.ItemMaintainSearchDetailViewData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改金庫存取項目明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(ItemMaintainInsertViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemMaintainSearchDetailViewData))
            {
                var tempData = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.vITEM_ID == model.vTrea_Item_Id);
                if (updateTempData != null)
                {
                    var _TREA_ITEM_TYPE = new Service.Actual.Common().GetSysCode("TREA_ITEM_TYPE");
                    var _TREA_ITEM_NAME = new Service.Actual.Common().GetSysCode("TREA_ITEM_NAME");

                    updateTempData.vEXEC_ACTION = updateTempData.vEXEC_ACTION == "A" ? "A" : "U";
                    updateTempData.vEXEC_ACTION_VALUE = updateTempData.vEXEC_ACTION_VALUE == "A" ? "新增" : "修改";
                    updateTempData.vIS_TREA_ITEM = model.vIs_Item;
                    updateTempData.vTREA_ITEM_TYPE = model.vType;
                    updateTempData.vTREA_ITEM_TYPE_VALUE = !model.vType.IsNullOrWhiteSpace() ? _TREA_ITEM_TYPE.FirstOrDefault(x => x.Value == model.vType)?.Text?.Trim() : null;
                    updateTempData.vTREA_ITEM_NAME = model.vName;
                    updateTempData.vTREA_ITEM_NAME_VALUE = !model.vName.IsNullOrWhiteSpace() ? _TREA_ITEM_TYPE.FirstOrDefault(x => x.Value == model.vName)?.Text?.Trim() : null;
                    updateTempData.vISDO_PERDAY = model.vIsDo_Perday;
                    updateTempData.vIS_DISABLED = model.vIs_Disabled;
                    updateTempData.vMEMO = model.vMemo;

                    Cache.Invalidate(CacheList.ItemMaintainSearchDetailViewData);
                    Cache.Set(CacheList.ItemMaintainSearchDetailViewData, tempData);
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
        /// 刪除金庫存取項目明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult DeleteTempData(ItemMaintainInsertViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.ItemMaintainSearchDetailViewData))
            {
                var tempData = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);
                var deleteTempData = tempData.FirstOrDefault(x => x.vITEM_ID == model.vTrea_Item_Id);
                if (deleteTempData != null)
                {
                    //判斷是否新增資料
                    if (deleteTempData.vEXEC_ACTION == "A")
                    {
                        tempData.Remove(deleteTempData);
                    }
                    //else
                    //{
                    //    deleteTempData.vEXEC_ACTION = "D";
                    //    deleteTempData.vEXEC_ACTION_VALUE = "刪除";
                    //}
                    Cache.Invalidate(CacheList.ItemMaintainSearchDetailViewData);
                    Cache.Set(CacheList.ItemMaintainSearchDetailViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                    result.Datas = tempData.Where(x => x.vEXEC_ACTION != null).Any();
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
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var searchModel = (ItemMaintainSearchViewModel)Cache.Get(CacheList.ItemMaintainSearchData);
            var _data = ItemMaintain.GetSearchData(searchModel);
            Cache.Invalidate(CacheList.ItemMaintainSearchDetailViewData);
            Cache.Set(CacheList.ItemMaintainSearchDetailViewData, _data);

            return Json(result);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "Search":
                    var Datas = (List<ItemMaintainSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(Datas.OrderBy(x => x.vITEM_ID).ToList()));
                case "RecordSearch":
                    var RecordDatas = (List<ItemMaintainChangeRecordSearchDetailViewModel>)Cache.Get(CacheList.ItemMaintainChangeRecordSearchDetailViewData);
                    return Json(jdata.modelToJqgridResult(RecordDatas.OrderBy(x => x.vAply_No).ToList()));
            }
            return null;
        }
    }
}