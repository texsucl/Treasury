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
/// 功能說明：金庫進出管理作業-金庫設備維護作業
/// 初版作者：20181025 侯蔚鑫
/// 修改歷程：20181025 侯蔚鑫 
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
    public class TreasuryMaintainController : CommonController
    {
        // GET: TreasuryMaintain
        private ITreasuryMaintain TreasuryMaintain;

        public TreasuryMaintainController()
        {
            TreasuryMaintain = new TreasuryMaintain();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>	
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryMaintain/");
            ViewBag.dControl_Mode_Search = new SelectList(new Service.Actual.Common().GetSysCode("CONTROL_MODE", true), "Value", "Text");
            ViewBag.dIs_Disabled_Search = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG", true), "Value", "Text");
            ViewBag.dControl_Mode = new SelectList(new Service.Actual.Common().GetSysCode("CONTROL_MODE"), "Value", "Text");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG"), "Value", "Text");
            return View();
        }

        /// <summary>
        /// 金庫設備異動紀錄查詢畫面
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">異動紀錄查詢ViewModel</param>
        /// <param name="type">呼叫原始書面</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecord()
        {
            ViewBag.dControl_Mode_Search = new SelectList(new Service.Actual.Common().GetSysCode("CONTROL_MODE", true), "Value", "Text");
            ViewBag.dAppr_Status = new SelectList(new Service.Actual.Common().GetSysCode("APPR_STATUS", true), "Value", "Text");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("YN_FLAG", true), "Value", "Text");
            return PartialView();
        }

        /// <summary>
        /// 金庫設備異動紀錄
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">查詢ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecordView(string AplyNo, TreasuryMaintainChangeRecordSearchViewModel data)
        {
            var _data = TreasuryMaintain.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.TreasuryMaintainChangeRecordSearchDataList);
            Cache.Set(CacheList.TreasuryMaintainChangeRecordSearchDataList, _data);
            return PartialView();
        }

        /// <summary>
        /// 金庫設備查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(TreasuryMaintainSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryMaintainSearchData);
            Cache.Set(CacheList.TreasuryMaintainSearchData, searchModel);

            var datas = TreasuryMaintain.GetSearchData(searchModel);
            //if (datas.Any())
            //{
                Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
                Cache.Set(CacheList.TreasuryMaintainSearchDataList, datas);
                result.RETURN_FLAG = true;
            //}

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
            if (Cache.IsSet(CacheList.TreasuryMaintainSearchData) && Cache.IsSet(CacheList.TreasuryMaintainSearchDataList))
            {
                var data = (TreasuryMaintainSearchViewModel)Cache.Get(CacheList.TreasuryMaintainSearchData);
                data.vLast_Update_Uid = AccountController.CurrentUserId;

                var _data = (List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList);
                result = TreasuryMaintain.TinApplyAudit(_data.Where(x => x.vExec_Action != null && x.vData_Status == "1").ToList(), data);
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
            var data = (TreasuryMaintainSearchViewModel)Cache.Get(CacheList.TreasuryMaintainSearchData);
            var _data = TreasuryMaintain.GetSearchData(data);
            Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
            Cache.Set(CacheList.TreasuryMaintainSearchDataList, _data);
            return Json(result);
        }

        /// <summary>
        /// 檢核設備名稱
        /// </summary>
        /// <param name="vEquip_Name">設備名稱</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Check_Equip_Name(string vEquip_Name)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();

            var tempData = (List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList);

            if (tempData.Where(x => x.vEquip_Name == vEquip_Name && x.vIs_Disabled == "N").Any() || TreasuryMaintain.Check_Equip_Name(vEquip_Name))
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
        /// 新增金庫設備明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData(TreasuryMaintainViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMaintainSearchDataList))
            {
                var tempData = (List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList);
                model.vExec_Action = "A";
                model.vExec_Action_Name = "新增";
                model.vData_Status = "1";
                model.vData_Status_Name = "可異動";
                tempData.Add(model);
                Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
                Cache.Set(CacheList.TreasuryMaintainSearchDataList, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 修改金庫設備明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData(TreasuryMaintainViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMaintainSearchDataList))
            {
                var tempData = (List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList);
                var updateTempData = tempData.FirstOrDefault(x => x.vTrea_Equip_Id == model.vTrea_Equip_Id);
                if (updateTempData != null)
                {
                    updateTempData.vExec_Action = (updateTempData.vExec_Action == "A") ? "A" : "U";
                    updateTempData.vExec_Action_Name = (updateTempData.vExec_Action == "A") ? "新增" : "修改";
                    updateTempData.vEquip_Name = model.vEquip_Name;
                    updateTempData.vControl_Mode = model.vControl_Mode;
                    updateTempData.vNormal_Cnt = model.vNormal_Cnt;
                    updateTempData.vReserve_Cnt = model.vReserve_Cnt;
                    updateTempData.vSum_Cnt = model.vSum_Cnt;
                    updateTempData.vMemo = model.vMemo;
                    updateTempData.vIs_Disabled = model.vIs_Disabled;
                    Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
                    Cache.Set(CacheList.TreasuryMaintainSearchDataList, tempData);
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
        /// 刪除金庫設備明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData(TreasuryMaintainViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMaintainSearchDataList))
            {
                var tempData = (List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList);
                var deleteTempData = tempData.FirstOrDefault(x => x.vTrea_Equip_Id == model.vTrea_Equip_Id);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
                    Cache.Set(CacheList.TreasuryMaintainSearchDataList, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                    result.Datas = tempData.Where(x => x.vData_Status == "1" && x.vExec_Action != null).Any();
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
                case "S":
                    if (Cache.IsSet(CacheList.TreasuryMaintainSearchDataList))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMaintainViewModel>)Cache.Get(CacheList.TreasuryMaintainSearchDataList)).OrderBy(x => x.vTrea_Equip_Id).ToList()));
                    break;
                case "O":
                    if (Cache.IsSet(CacheList.TreasuryMaintainChangeRecordSearchDataList))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMaintainChangeRecordViewModel>)Cache.Get(CacheList.TreasuryMaintainChangeRecordSearchDataList)).OrderBy(x => x.vAply_No).ToList()));
                    break;
            }
            return null;
        }

    }
}