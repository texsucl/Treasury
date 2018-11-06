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
/// 功能說明：金庫進出管理作業-入庫人員確認作業
/// 初版作者：20180906 李彥賢
/// 修改歷程：20180906 李彥賢
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
    public class ConfirmStorageController : CommonController
    {
        private IConfirmStorage ConfirmStorage;

        public ConfirmStorageController()
        {
            ConfirmStorage = new ConfirmStorage();
        }
        // GET: ConfirmStorage
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/ConfirmStorage/");
            var data = ConfirmStorage.GetFirstTimeData(AccountController.CurrentUserId);
            var userInfo = TreasuryAccess.GetUserInfo(AccountController.CurrentUserId);
            var plsSelect = new SelectOption() { Text = "請選擇", Value = "0" };
            ViewBag.hEMP_ID = userInfo.EMP_ID;
            ViewBag.lEMP_Name = userInfo.EMP_Name;
            ViewBag.hDPT_ID = userInfo.DPT_ID;
            ViewBag.lDPT_Name = userInfo.DPT_Name;
            ViewBag.cUSER_ID = AccountController.CurrentUserId;
            //ViewBag.storageType = new SelectList(data.Item1, "Value", "Text");
            ViewBag.sealItem = new SelectList(data.Item1, "Value", "Text");
            ViewBag.accessType = new SelectList(data.Item2, "Value", "Text");
            ViewBag.treaItem = new SelectList(data.Item3, "value", "Text");
            ViewBag.itemOpType = new SelectList(data.Item4, "value", "Text");
            ViewBag.register_ID = data.Item5;
            ViewBag.itemID = string.Join(";", data.Item6);
            return View();
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(ConfirmStorageSearchViewModel searchModel, string itemId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            List<string> itemIdList = new List<string>();
            itemIdList.AddRange(itemId.Split(';'));
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.ConfirmStorageSearchData);
            Cache.Set(CacheList.ConfirmStorageSearchData, searchModel);

            var datas = ConfirmStorage.GetSearchDetail(searchModel, AccountController.CurrentUserId);
            datas = datas.Where(x => itemIdList.Contains(x.vITEM_ID)).ToList();
            //if (datas.Any())
            //{              
                Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                Cache.Set(CacheList.ConfirmStorageSearchDetailViewData,datas);
                result.RETURN_FLAG = true;
            //}
            return Json(result);
        }
        /// <summary>
        /// 查作業類型 & 印鑑內容下拉選單
        /// </summary>
        /// <param name="ItemId">大項ID (D1008)</param>
        /// <param name="AccessType">P,G</param>
        /// <param name="vSEAL_ITEM_ID">印章ID</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetItemId(string ItemId, string AccessType,string vSEAL_ITEM_ID = null,string _ItemId = null)
        {
            
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
            List<string> sealIdList = new List<string>();
            ViewData.ForEach(x => {
                //if (x.vACCESS_TYPE_CODE == AccessType)
                //if(x.vITEM_ID == "D1023" && x.vAPLY_UID == cUserId)

                if(x.vSEAL_ITEM_ID != null && x.vSEAL_ITEM_ID != ItemId)
                    sealIdList.Add(x.vSEAL_ITEM_ID);
            });
            var result = ConfirmStorage.GetItemOpType(ItemId, AccessType, sealIdList, _ItemId);
            return Json(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OpTypeId">1,2,3,4</param>
        /// <param name="ItemIdList">D1008,D1009</param>
        /// <param name="AccessType">P,G</param>
        /// <param name="vSEAL_ITEM_ID">印章ID</param>
        /// <returns></returns>
        public JsonResult OpTypeChange(string OpTypeId, List<string>ItemIdList, string AccessType,string vSEAL_ITEM_ID = null,string ItemId = null, bool RemoveRowData = true, string RegisterId = null)
        {
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
            var cUserId = AccountController.CurrentUserId;
            List<string> rowItemIdList = new List<string>();
            List<string> sealIdList = new List<string>();
            ViewData.ForEach(x => {
                if (x.vITEM_ID != null && RemoveRowData == true && x.vITeM_OP_TYPE != "2" && x.vITEM_ID != "D1023")
                    rowItemIdList.Add(x.vITEM_ID);
                if (x.vITEM_ID != null && RemoveRowData == true && x.vITEM_ID == "D1023" && x.vAPLY_UID == cUserId)
                    rowItemIdList.Add(x.vITEM_ID);
                if (x.vSEAL_ITEM_ID != null && vSEAL_ITEM_ID != x.vSEAL_ITEM_ID)
                    sealIdList.Add(x.vSEAL_ITEM_ID);
            });
            var result = ConfirmStorage.ItemOpTypeChange(OpTypeId, ItemIdList, AccessType, sealIdList, rowItemIdList, ItemId, RegisterId, cUserId);
            return Json(result);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="InsertModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Insert(ConfirmStorageInsertViewModel InsertModel)
        {
            //MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            //result.RETURN_FLAG = false;
            //result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            //var searchData = (ConfirmStorageSearchViewModel)Cache.Get(CacheList.ConfirmStorageSearchData);
            //result = ConfirmStorage.InsertData(InsertModel, searchData);
            //if (result.RETURN_FLAG)
            //{
            //    Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
            //    Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, result.Datas);
            //}
            List<ConfirmStorageSearchDetailViewModel> data = new List<ConfirmStorageSearchDetailViewModel>();

            MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
           
            if(InsertModel != null)
            {
                ViewData.Add(new ConfirmStorageSearchDetailViewModel()
                {
                    vITeM_OP_TYPE = InsertModel.vITeM_OP_TYPE,
                    vITEM_ID = InsertModel.vITEM_ID,
                    vSEAL_ITEM_ID = InsertModel.vSEAL_ITEM_ID,
                    vACCESS_TYPE = InsertModel.vACCESS_TYPE,
                    vACCESS_TYPE_CODE = InsertModel.vACCESS_TYPE_CODE,
                    vACCESS_REASON = InsertModel.vACCESS_REASON,
                    vCONFIRM_UID = InsertModel.vCurrentUid,
                    vITEM = InsertModel.vITEM,
                    hITEM = InsertModel.vITEM,
                    vSEAL_ITEM = InsertModel.vSEAL_ITEM,
                    vAPLY_UID = AccountController.CurrentUserId,
                    uuid = InsertModel.uuid
                });

                Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, ViewData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }

            return Json(result);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="InsertModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(ConfirmStorageInsertViewModel updateModel, string registerId)
        {
            //MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            //result.RETURN_FLAG = false;
            //result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            //var searchData = (ConfirmStorageSearchViewModel)Cache.Get(CacheList.ConfirmStorageSearchData);
            //result = ConfirmStorage.UpdateData(updateModel, searchData);
            //if (result.RETURN_FLAG)
            //{
            //    Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
            //    Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, result.Datas);
            //}
            MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            var searchData = (ConfirmStorageSearchViewModel)Cache.Get(CacheList.ConfirmStorageSearchData);
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
            bool is_CreateUser = false;

            if(updateModel != null)
            {
                var newViewData = ViewData
                .Where(x => x.vITeM_OP_TYPE != "3")
                .FirstOrDefault(x => x.uuid == updateModel.uuid);

                if (updateModel.v_IS_CHECKED == null)
                {
                    //if(newViewData.vCONFIRM_UID != null && newViewData.vITEM_ID == "D1023")
                    //{
                    //    is_CreateUser = ConfirmStorage.CheckIsCreateUser(AccountController.CurrentUserId, registerId);
                    //    if (!is_CreateUser)
                    //    {
                    //        result.DESCRIPTION = "非申請人，無法修改";
                    //        result.RETURN_FLAG = false;
                    //        return Json(result);
                    //    }
                    //}
                    newViewData.vSEAL_ITEM_ID = updateModel.vSEAL_ITEM_ID;
                    newViewData.vSEAL_ITEM = updateModel.vSEAL_ITEM;
                    newViewData.vACCESS_TYPE = updateModel.vACCESS_TYPE;
                    newViewData.vACCESS_REASON = updateModel.vACCESS_REASON;
                    newViewData.vACCESS_TYPE_CODE = updateModel.vACCESS_TYPE_CODE;
                    Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                    Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, ViewData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                }
                else
                {
                    if (newViewData.vITEM_ID == "D1023" && newViewData.vAPLY_UID != AccountController.CurrentUserId)
                    {
                        result.DESCRIPTION = "非申請人，無法修改";
                        result.RETURN_FLAG = false;
                        return Json(result);
                    }
                    result = ConfirmStorage.UpdateData(updateModel, searchData, newViewData.hvAPLY_NO);
                    if (result.RETURN_FLAG)
                    {
                        Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                        Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, result.Datas);
                    }
                }
 
            }

            return Json(result);
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="DeleteModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(ConfirmStorageDeleteViewModel DeleteModel)
        {
            
            MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
            var searchData = (ConfirmStorageSearchViewModel)Cache.Get(CacheList.ConfirmStorageSearchData);
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
            var rowData = ViewData
                          .Where(x => x.uuid == DeleteModel.uuid)
                         .FirstOrDefault();
            if (searchData.v_IS_CHECKED == null)
            {
                //MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
                //result.RETURN_FLAG = false;
                //result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();

                if (DeleteModel != null)
                {
                    ViewData.Remove(rowData);

                    Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                    Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, ViewData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                }
            }
            else
            {
                if (rowData.vITEM_ID == "D1023" && rowData.vAPLY_UID != AccountController.CurrentUserId)
                {
                    result.DESCRIPTION = "非申請人，無法修改";
                    result.RETURN_FLAG = false;
                    return Json(result);
                }
                result = ConfirmStorage.DeteleData(DeleteModel, searchData);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                    Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

    /// <summary>
    /// Jqgrid Cache Data
    /// </summary>
    /// <param name="jdta"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdta, string type, string itemId = null)
        {
            switch (type)
            {
                case "Search":
                    List<string> itemIdList = new List<string>();
                    itemIdList.AddRange(itemId.Split(';'));
                    var SearchDatas = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
                    return Json(jdta.modelToJqgridResult(SearchDatas.Where(x => itemIdList.Contains(x.vITEM_ID)).OrderBy(x => x.vITeM_OP_TYPE).ToList()));
            }
            return null;
        }

        public JsonResult Confirm(List<string> ConfirmModel,string register_ID, string v_IS_CHECKED)
        {
            MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> result = new MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            var searchData = (ConfirmStorageSearchViewModel)Cache.Get(CacheList.ConfirmStorageSearchData);
            var ViewData = (List<ConfirmStorageSearchDetailViewModel>)Cache.Get(CacheList.ConfirmStorageSearchDetailViewData);
           
            if(v_IS_CHECKED == null)
            {
                result = ConfirmStorage.ConfirmData(ConfirmModel, searchData, ViewData, AccountController.CurrentUserId, register_ID);
            }
            else
            {
                result = ConfirmStorage.ConfirmAlreadyData(ConfirmModel, searchData, ViewData, AccountController.CurrentUserId, register_ID);
            }

            

            if (result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.ConfirmStorageSearchDetailViewData);
                Cache.Set(CacheList.ConfirmStorageSearchDetailViewData, result.Datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

     }    
}