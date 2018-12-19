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

/// <summary>
/// 功能說明：金庫進出管理作業-定存檢核表項目設定
/// 初版作者：20181106 侯蔚鑫
/// 修改歷程：20181106 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
namespace Treasury.Web.Controllers
{
    [Authorize]
    [CheckSessionFilterAttribute]
    public class DepChkItemController : CommonController
    {
        // GET: DepChkItem
        private IDepChkItem DepChkItem;

        public DepChkItemController()
        {
            DepChkItem = new DepChkItem();
        }
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryMaintain/");
            ViewBag.dIs_Disabled_Search = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", true), "Value", "Text");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED"), "Value", "Text");

            List<SelectOption> Access_Type = new List<SelectOption>();
            Access_Type.Add(new SelectOption() { Text = "存入", Value = "P" });
            Access_Type.Add(new SelectOption() { Text = "取出", Value = "G" });
            ViewBag.dAccess_Type = new SelectList(Access_Type, "Value", "Text");

            return View();
        }

        /// <summary>
        /// 定存檢核表項目異動紀錄
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">查詢ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecordView(string AplyNo, DepChkItemChangeRecordSearchViewModel data)
        {
            var _data = DepChkItem.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.DepChkItemChangeRecordSearchDataList);
            Cache.Set(CacheList.DepChkItemChangeRecordSearchDataList, _data);
            return PartialView();
        }

        /// <summary>
        /// 定存檢核表項目排序調整
        /// </summary>
        /// <param name="Access_Type">交易別</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult OrderView(string Access_Type)
        {
            var _data = DepChkItem.GetOrderData(Access_Type);
            Cache.Invalidate(CacheList.DepChkItemOrderSearchDataList);
            Cache.Set(CacheList.DepChkItemOrderSearchDataList, _data);

            ViewBag.CheckOrderData = DepChkItem.CheckOrderData(Access_Type);

            return PartialView();
        }

        /// <summary>
        /// 定存檢核表項目查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(DepChkItemSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.DepChkItemSearchData);
            Cache.Set(CacheList.DepChkItemSearchData, searchModel);

            var datas = (List<DepChkItemViewModel>)DepChkItem.GetSearchData(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                Cache.Set(CacheList.DepChkItem_P_SearchDataList, datas.Where(x => x.vAccess_Type == "P").ToList());
                Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                Cache.Set(CacheList.DepChkItem_G_SearchDataList, datas.Where(x => x.vAccess_Type == "G").ToList());
                result.RETURN_FLAG = true;
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
            if (Cache.IsSet(CacheList.DepChkItemSearchData) && Cache.IsSet(CacheList.DepChkItem_P_SearchDataList) && Cache.IsSet(CacheList.DepChkItem_G_SearchDataList))
            {
                var data = (DepChkItemSearchViewModel)Cache.Get(CacheList.DepChkItemSearchData);
                data.vLast_Update_Uid = AccountController.CurrentUserId;

                List<DepChkItemViewModel> _data = new List<DepChkItemViewModel>();

                _data.AddRange((List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_P_SearchDataList));
                _data.AddRange((List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_G_SearchDataList));

                result = DepChkItem.TinApplyAudit(_data.Where(x => x.vExec_Action != null && x.vData_Status == "1").ToList(), data);
            }
            else
            {
                result.RETURN_FLAG = false;
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 覆核排序資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyOrderTempData(List<DepChkItemViewModel> saveData)
        {
            MSGReturnModel<IEnumerable<ITinItem>> result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = true;//預設成功
            //定存檢核表項目順序檢查
            foreach(var item in saveData)
            {
                var _CheckData = saveData.Where(x => x.vIsortby != item.vIsortby).ToList();
                foreach(var check in _CheckData)
                {
                    if(check.vItem_Order==item.vItem_Order)
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "定存檢核表項目順序有重覆";
                    }
                }
            }

            if(result.RETURN_FLAG)
            {
                DepChkItemSearchViewModel data = new DepChkItemSearchViewModel();
                data.vLast_Update_Uid = AccountController.CurrentUserId;
                result = DepChkItem.TinOrderApplyAudit(saveData, data);
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
            var data = (DepChkItemSearchViewModel)Cache.Get(CacheList.DepChkItemSearchData);
            var datas = (List<DepChkItemViewModel>)DepChkItem.GetSearchData(data);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                Cache.Set(CacheList.DepChkItem_P_SearchDataList, datas.Where(x => x.vAccess_Type == "P").ToList());
                Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                Cache.Set(CacheList.DepChkItem_G_SearchDataList, datas.Where(x => x.vAccess_Type == "G").ToList());
            }
            return Json(result);
        }

        /// <summary>
        /// 取消申請(清空tempData)
        /// </summary>
        /// <param name="Access_Type">交易別</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetOrderTempData(string Access_Type)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            var _data = DepChkItem.GetOrderData(Access_Type);
            Cache.Invalidate(CacheList.DepChkItemOrderSearchDataList);
            Cache.Set(CacheList.DepChkItemOrderSearchDataList, _data);

            return Json(result);
        }

        /// <summary>
        /// 新增定存檢核表項目明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(DepChkItemViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (model.vAccess_Type == "P" && Cache.IsSet(CacheList.DepChkItem_P_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_P_SearchDataList);
                model.vExec_Action = "A";
                model.vExec_Action_Name = "新增";
                model.vData_Status = "1";
                model.vData_Status_Name = "可異動";
                tempData.Add(model);
                Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                Cache.Set(CacheList.DepChkItem_P_SearchDataList, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            else if (model.vAccess_Type == "G" && Cache.IsSet(CacheList.DepChkItem_G_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_G_SearchDataList);
                model.vExec_Action = "A";
                model.vExec_Action_Name = "新增";
                model.vData_Status = "1";
                model.vData_Status_Name = "可異動";
                tempData.Add(model);
                Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                Cache.Set(CacheList.DepChkItem_G_SearchDataList, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改定存檢核表項目明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(DepChkItemViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            if (model.vAccess_Type == "P" && Cache.IsSet(CacheList.DepChkItem_P_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_P_SearchDataList);
                var updateTempData = tempData.FirstOrDefault(x => x.vAccess_Type == model.vAccess_Type && x.vIsortby == model.vIsortby);
                if (updateTempData != null)
                {
                    updateTempData.vExec_Action = (updateTempData.vExec_Action == "A") ? "A" : "U";
                    updateTempData.vExec_Action_Name = (updateTempData.vExec_Action == "A") ? "新增" : "修改";
                    updateTempData.vDep_Chk_Item_Desc = model.vDep_Chk_Item_Desc;
                    updateTempData.vIs_Disabled = model.vIs_Disabled;
                    updateTempData.vReplace = model.vReplace;
                    Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                    Cache.Set(CacheList.DepChkItem_P_SearchDataList, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                }
            }
            else if (model.vAccess_Type == "G" && Cache.IsSet(CacheList.DepChkItem_G_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_G_SearchDataList);
                var updateTempData = tempData.FirstOrDefault(x => x.vAccess_Type == model.vAccess_Type && x.vIsortby == model.vIsortby);
                if (updateTempData != null)
                {
                    updateTempData.vExec_Action = (updateTempData.vExec_Action == "A") ? "A" : "U";
                    updateTempData.vExec_Action_Name = (updateTempData.vExec_Action == "A") ? "新增" : "修改";
                    updateTempData.vDep_Chk_Item_Desc = model.vDep_Chk_Item_Desc;
                    updateTempData.vIs_Disabled = model.vIs_Disabled;
                    updateTempData.vReplace = model.vReplace;
                    Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                    Cache.Set(CacheList.DepChkItem_G_SearchDataList, tempData);
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
        /// 刪除定存檢核表項目明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(DepChkItemViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            if (model.vAccess_Type == "P" && Cache.IsSet(CacheList.DepChkItem_P_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_P_SearchDataList);
                var deleteTempData = tempData.FirstOrDefault(x => x.vAccess_Type == model.vAccess_Type && x.vIsortby == model.vIsortby);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.DepChkItem_P_SearchDataList);
                    Cache.Set(CacheList.DepChkItem_P_SearchDataList, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
                }
            }
            else if (model.vAccess_Type == "G" && Cache.IsSet(CacheList.DepChkItem_G_SearchDataList))
            {
                var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_G_SearchDataList);
                var deleteTempData = tempData.FirstOrDefault(x => x.vAccess_Type == model.vAccess_Type && x.vIsortby == model.vIsortby);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.DepChkItem_G_SearchDataList);
                    Cache.Set(CacheList.DepChkItem_G_SearchDataList, tempData);
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
        /// 重新排序
        /// </summary>
        /// <param name="Access_Type"></param>
        /// <param name="Isortby"></param>
        /// <param name="Old_Order"></param>
        /// <param name="New_Order"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetOrder(string Access_Type, int Isortby, int Old_Order, int New_Order)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            var tempData = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItemOrderSearchDataList);

            //本次順序修改
            var Updata = tempData.FirstOrDefault(x => x.vAccess_Type == Access_Type && x.vIsortby == Isortby);
            Updata.vItem_Order = New_Order;

            //判斷移動順序
            if (New_Order < Old_Order)//往前移動
            {
                int UpOtherOrder = New_Order + 1;
                var UpOtherData = tempData
                    .Where(x => x.vAccess_Type == Access_Type && x.vIsortby != Isortby)
                    .Where(x => x.vItem_Order >= New_Order && x.vItem_Order < Old_Order)
                    .OrderBy(x => x.vItem_Order);

                foreach(var item in UpOtherData)
                {
                    item.vItem_Order = UpOtherOrder;
                    UpOtherOrder++;
                }
            }
            else//往後移動
            {
                int UpOtherOrder = Old_Order;
                var UpOtherData = tempData
                    .Where(x => x.vAccess_Type == Access_Type && x.vIsortby != Isortby)
                    .Where(x => x.vItem_Order > Old_Order && x.vItem_Order <= New_Order)
                    .OrderBy(x => x.vItem_Order);

                foreach (var item in UpOtherData)
                {
                    item.vItem_Order = UpOtherOrder;
                    UpOtherOrder++;
                }
            }

            Cache.Invalidate(CacheList.DepChkItemOrderSearchDataList);
            Cache.Set(CacheList.DepChkItemOrderSearchDataList, tempData);

            result.RETURN_FLAG = true;

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
                case "P":
                    if (Cache.IsSet(CacheList.DepChkItem_P_SearchDataList))
                        return Json(jdata.modelToJqgridResult(((List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_P_SearchDataList)).OrderBy(x => x.vItem_Order).ToList()));
                    break;
                case "G":
                    if (Cache.IsSet(CacheList.DepChkItem_G_SearchDataList))
                        return Json(jdata.modelToJqgridResult(((List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItem_G_SearchDataList)).OrderBy(x => x.vItem_Order).ToList()));
                    break;
                case "O":
                    if (Cache.IsSet(CacheList.DepChkItemChangeRecordSearchDataList))
                        return Json(jdata.modelToJqgridResult(((List<DepChkItemChangeRecordViewModel>)Cache.Get(CacheList.DepChkItemChangeRecordSearchDataList)).OrderBy(x => x.vAply_No).ToList()));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 定存檢核表項目順序查詢資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult qryOrderData()
        {
            List<DepChkItemViewModel> rows = (List<DepChkItemViewModel>)Cache.Get(CacheList.DepChkItemOrderSearchDataList);
            rows = rows.OrderBy(x => x.vItem_Order).ToList();

            var jsonData = new { success = true, orderList = rows };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
    }
}