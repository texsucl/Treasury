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
/// 功能說明：金庫進出管理作業-mail發送內文設定檔維護作業
/// 初版作者：20181109 張家華
/// 修改歷程：20181109 張家華 
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
    public class TreasuryMailContentController : CommonController
    {
        // GET: TreasuryMaintain
        private ITreasuryMailContent TreasuryMailContent;

        public TreasuryMailContentController()
        {
            TreasuryMailContent = new TreasuryMailContent();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>	
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/TreasuryMailContent/");
            ViewBag.dMAIL_ID = new SelectList(TreasuryMailContent.Get_MAIL_ID(), "Value", "Text");
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", true), "Value", "Text");
            return View();
        }

        /// <summary>
        /// Mail內文設定檔 新增or修改 畫面 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(string Mail_ID)
        {
            var _detail = (TreasuryMailContentUpdateViewModel)TreasuryMailContent.GetUpdateData(Mail_ID);
            Cache.Invalidate(CacheList.TreasuryMailContentDetailData);
            Cache.Set(CacheList.TreasuryMailContentDetailData, _detail);
            Cache.Invalidate(CacheList.TreasuryMailContentReceiveData);
            Cache.Set(CacheList.TreasuryMailContentReceiveData, _detail.subData);
            return PartialView(_detail);
        }

        /// <summary>
        /// mail發送對象 畫面 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Receive(string Mail_ID, string Aply_No,string Act_Flag)
        {            
            Cache.Invalidate(CacheList.TreasuryMailContentReceiveData);
            Cache.Set(CacheList.TreasuryMailContentReceiveData, TreasuryMailContent.GetReceiveData(Mail_ID, Aply_No));
            ViewBag.Act_Flag = Act_Flag;
            return PartialView();
        }

        [HttpPost]
        public ActionResult ChangeRecord(string Mail_ID)
        {
            ViewBag.vMail_ID = Mail_ID;
            ViewBag.dMAIL_ID = new SelectList(TreasuryMailContent.Get_MAIL_ID(), "Value", "Text");
            ViewBag.dAppr_Status = new SelectList(new Service.Actual.Common().GetSysCode("APPR_STATUS", true), "Value", "Text");
            return PartialView();
        }

        /// <summary>
        /// mail發送內文設定檔異動紀錄
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">查詢ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeRecordView(string AplyNo, TreasuryMailContentHistorySearchViewModel data)
        {
            var _data = TreasuryMailContent.GetChangeRecordSearchData(data, AplyNo);
            Cache.Invalidate(CacheList.TreasuryMailContentChangeRecordData);
            Cache.Set(CacheList.TreasuryMailContentChangeRecordData, _data);
            ViewBag.dIs_Disabled = new SelectList(new Service.Actual.Common().GetSysCode("IS_DISABLED", true), "Value", "Text");
            return PartialView();
        }

        /// <summary>
        /// mail發送內文設定檔查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(TreasuryMailContentSearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.TreasuryMailContentSearchData);
            Cache.Set(CacheList.TreasuryMailContentSearchData, searchModel);

            var datas = TreasuryMailContent.GetSearchData(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.TreasuryMailContentData);
                Cache.Set(CacheList.TreasuryMailContentData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData(TreasuryMailContentUpdateViewModel model)
        {
            MSGReturnModel<IEnumerable<ITinItem>> result = new MSGReturnModel<IEnumerable<ITinItem>>();
            if (Cache.IsSet(CacheList.TreasuryMailContentSearchData))
            {
                Cache.Set(CacheList.TreasuryMailContentSearchData, new TreasuryMailContentSearchViewModel() {
                    vIS_DISABLED = "All",
                    vMAIL_CONTENT_ID = "All"
                });
            }
            if (Cache.IsSet(CacheList.TreasuryMailContentSearchData))
            {
                var data = (TreasuryMailContentSearchViewModel)Cache.Get(CacheList.TreasuryMailContentSearchData);
                data.UserId = AccountController.CurrentUserId;
                TreasuryMailContentUpdateViewModel _data = new TreasuryMailContentUpdateViewModel();
                if (Cache.IsSet(CacheList.TreasuryMailContentDetailData))
                {
                    _data = (TreasuryMailContentUpdateViewModel)Cache.Get(CacheList.TreasuryMailContentDetailData);
                }
                _data.UserID = AccountController.CurrentUserId;
                _data.vIS_DISABLED = model.vIS_DISABLED;
                _data.vMAIL_SUBJECT = model.vMAIL_SUBJECT;
                _data.vMAIL_CONTENT = model.vMAIL_CONTENT;
                _data.subData = (List<TreasuryMailReceivelViewModel>)Cache.Get(CacheList.TreasuryMailContentReceiveData);
                result = TreasuryMailContent.TinApplyAudit(new List<TreasuryMailContentUpdateViewModel>() { _data }, data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.TreasuryMailContentData);
                    Cache.Set(CacheList.TreasuryMailContentData, result.Datas);
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
        /// 取消申請(清空tempData)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetTempData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            var data = (TreasuryMaintainSearchViewModel)Cache.Get(CacheList.TreasuryMaintainSearchData);
            //var _data = TreasuryMaintain.GetSearchData(data);
            //Cache.Invalidate(CacheList.TreasuryMaintainSearchDataList);
            //Cache.Set(CacheList.TreasuryMaintainSearchDataList, _data);
            return Json(result);
        }

        /// <summary>
        /// 查詢 可以申請的
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertFuncIDData()
        {
            var result = new List<SelectOption>();
            List<string> funcIds = new List<string>();
            if (Cache.IsSet(CacheList.TreasuryMailContentReceiveData))
            {
                funcIds = ((List<TreasuryMailReceivelViewModel>)Cache.Get(CacheList.TreasuryMailContentReceiveData)).Select(x => x.FUNC_ID).ToList();
            }
            result = TreasuryMailContent.GetFUNC_ID(funcIds);
            return Json(result);
        }

        /// <summary>
        /// 新增mail發送對象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertReceivelData(TreasuryMailReceivelViewModel model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMailContentReceiveData))
            {
                var tempData = (List<TreasuryMailReceivelViewModel>)Cache.Get(CacheList.TreasuryMailContentReceiveData);
                model.vStatus_D = "新增";
                tempData.Add(model);
                Cache.Invalidate(CacheList.TreasuryMailContentReceiveData);
                Cache.Set(CacheList.TreasuryMailContentReceiveData, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 刪除mail發送對象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteReceivelData(TreasuryMailReceivelViewModel model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.TreasuryMailContentDetailData))
            {
                var tempData = (List<TreasuryMailReceivelViewModel>)Cache.Get(CacheList.TreasuryMailContentReceiveData);
                var deleteTempData = tempData.FirstOrDefault(x => x.FUNC_ID == model.FUNC_ID);
                if (deleteTempData != null)
                {
                    //判斷是否新增資料
                    if (deleteTempData.vStatus == "A")
                    {
                        tempData.Remove(deleteTempData);
                    }
                    else
                    {
                        deleteTempData.vStatus = "D";
                        deleteTempData.vStatus_D = "刪除";
                    }
                    Cache.Invalidate(CacheList.TreasuryMailContentReceiveData);
                    Cache.Set(CacheList.TreasuryMailContentReceiveData, tempData);
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
                case "Index":
                    if (Cache.IsSet(CacheList.TreasuryMailContentData))
                        return Json(jdata.modelToJqgridResult((List<TreasuryMailContentViewModel>)Cache.Get(CacheList.TreasuryMailContentData)));
                    break;
                case "Receive":
                    if (Cache.IsSet(CacheList.TreasuryMailContentReceiveData))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMailReceivelViewModel>)Cache.Get(CacheList.TreasuryMailContentReceiveData)).OrderBy(x => x.FUNC_ID).ToList()));
                    break;
                case "ChangeRecord":
                    if (Cache.IsSet(CacheList.TreasuryMailContentChangeRecordData))
                        return Json(jdata.modelToJqgridResult(((List<TreasuryMailContentHistoryViewModel>)Cache.Get(CacheList.TreasuryMailContentChangeRecordData))));
                    break;
            }
            return null;
        }

    }
}