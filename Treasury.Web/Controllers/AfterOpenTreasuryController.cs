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
    public class AfterOpenTreasuryController : CommonController
    {
        private IAftereOpenTreasury AftereOpenTreasury;
        public AfterOpenTreasuryController()
        {
            AftereOpenTreasury = new AftereOpenTreasury();
        }

        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.opScope = GetopScope("~/AfterOpenTreasury/");
            var empty = new SelectOption() { Text = " ", Value = " " };
            var datas = AftereOpenTreasury.GetFristTimeDatas();
            datas.Item3.Insert(0, empty);
            datas.Item6.Insert(0, empty);
            ViewBag.RegisterList = new SelectList(datas.Item1, "Value", "Text");
            ViewBag.ItemOpType = new SelectList(datas.Item2, "Value", "Text");
            ViewBag.AccessType = new SelectList(datas.Item3, "Value", "Text");
            ViewBag.SealItem = new SelectList(datas.Item4, "Value", "Text");
            ViewBag.TreaItem = new SelectList(datas.Item5, "Value", "Text");
            ViewBag.Emps = new SelectList(datas.Item6, "Value", "Text");
            ViewBag.vUser_Id = AccountController.CurrentUserId;
            return View();
        }

        /// <summary>
        /// Dialog Selected Change 事件
        /// </summary>
        /// <param name="ItemOpType"></param>
        /// <param name="TreaItem"></param>
        /// <param name="AccessType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Change(string ItemOpType, string TreaItem, string AccessType)
        {
            var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
            var empty = new SelectOption() { Text = " ", Value = " " };
            var result = AftereOpenTreasury.DialogSelectedChange(ItemOpType, TreaItem, AccessType, datas);
            result.Item3.Insert(0, empty);
            result.Item4.Insert(0, empty);
            return Json(result);
        }

        /// <summary>
        /// 修改時 作業類型 2 data.vSEAL_ITEM_ID 無值 必須查DB 帶出印章
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetSeal(string TreaItem)
        {
            var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
            var result = AftereOpenTreasury.GetSealFun(TreaItem, datas);

            return Json(result);
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        public JsonResult SearchUnconfirmedData()
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            //Cache.Invalidate(CacheList.AfterOpenTreasurySearchData);
            //Cache.Set(CacheList.AfterOpenTreasurySearchData, searchModel);
            var datas = AftereOpenTreasury.GetUnconfirmedDetail();
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                Cache.Set(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 新增至金庫登記簿
        /// </summary>
        /// <returns></returns>
        public JsonResult InsertUnconfirmedData(string RegisterID)
        {
            MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            var searchData = (AfterOpenTreasurySearchViewModel)Cache.Get(CacheList.AfterOpenTreasurySearchData);
            if (Cache.IsSet(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData))
            {
                var viewModel = (List<AfterOpenTreasuryUnconfirmedDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                var datas = AftereOpenTreasury.InsertUnconfirmedDetail(RegisterID, viewModel, AccountController.CurrentUserId, searchData);
                if (datas.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData, datas.Datas);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = datas.DESCRIPTION;
                }
                else
                {
                    result.DESCRIPTION = datas.DESCRIPTION;
                }  
            }
            return Json(result);
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Search(AfterOpenTreasurySearchViewModel searchModel)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            Cache.Invalidate(CacheList.AfterOpenTreasurySearchData);
            Cache.Set(CacheList.AfterOpenTreasurySearchData, searchModel);
            var datas = AftereOpenTreasury.GetSearchDetail(searchModel);
            if (datas.Any())
            {
                Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, datas);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 檢查 實際入庫時間、實際出庫時間
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckPGTime(string registerID)
        {
            MSGReturnModel<AfterOpenTreasurySearchDetailViewModel> result = new MSGReturnModel<AfterOpenTreasurySearchDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();
            if (registerID != null && Cache.IsSet(CacheList.AfterOpenTreasurySearchDetailViewData))
            {
                var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
           
                var forCheck = datas.FirstOrDefault(x => x.vTREA_REGISTER_ID == registerID);

                //if (forCheck != null)
                if (datas.All(x => x.vACTUAL_GET_TIME != null && x.vACTUAL_PUT_TIME != null))
                {
                    result.Datas = forCheck;
                    result.RETURN_FLAG = true;
                }
                else
                {
                    if(datas.All(x => x.vAPLY_NO != null))
                    {
                        result.DESCRIPTION = "尚未輸入出入庫時間，請先登打出入庫時間!!";
                    }
                    else
                    {
                        result.DESCRIPTION = "有項目尚未入庫確認!!";
                    }
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="registerID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Apply(string registerID)
        {
            MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.Apply_Audit_Fail.GetDescription();
            if (registerID != null && Cache.IsSet(CacheList.AfterOpenTreasurySearchDetailViewData))
            {
                var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
                var searchData = (AfterOpenTreasurySearchViewModel)Cache.Get(CacheList.AfterOpenTreasurySearchData);
                result = AftereOpenTreasury.ApplyData(registerID, searchData, datas, AccountController.CurrentUserId);
            }
            if (result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, result.Datas);
            }
            return Json(result);
        }

        /// <summary>
        /// 確定存檔
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Confrimed(AfterOpenTreasurySearchViewModel searchModel)
        {
            MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.save_Fail.GetDescription();
            if (searchModel.vTREA_REGISTER_ID.Any())
            {
                Cache.Invalidate(CacheList.AfterOpenTreasurySearchData);
                Cache.Set(CacheList.AfterOpenTreasurySearchData, searchModel);
                result = AftereOpenTreasury.ConfrimedData(searchModel, AccountController.CurrentUserId);
            }
            if (result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, result.Datas);
            }
            return Json(result);
        }

        public JsonResult Insert(AfterOpenTreasuryInsertViewModel InsertModel)
        {
            MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.insert_Fail.GetDescription();
            var searchData = (AfterOpenTreasurySearchViewModel)Cache.Get(CacheList.AfterOpenTreasurySearchData);
            result = AftereOpenTreasury.InsertData(InsertModel, searchData, AccountController.CurrentUserId);
            if (result.RETURN_FLAG)
            {
                Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, result.Datas);
            }
            return Json(result);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="ActualAccEmp"></param>
        /// <param name="ActualAccType"></param>
        /// <returns></returns>
       // public JsonResult Update(string APLYNO, string ActualAccEmp, string ActualAccType, string InsertReason, string ItemId, string OpType)
        public JsonResult Update(string APLYNO, AfterOpenTreasuryInsertViewModel InsertModel)
        {
            MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            if (APLYNO != null && Cache.IsSet(CacheList.AfterOpenTreasurySearchDetailViewData))
            {
                var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
                var searchData = (AfterOpenTreasurySearchViewModel)Cache.Get(CacheList.AfterOpenTreasurySearchData);
                //result = AftereOpenTreasury.UpdateData(APLYNO, ActualAccEmp, ActualAccType, InsertReason, searchData, datas, AccountController.CurrentUserId, ItemId, OpType);
                result = AftereOpenTreasury.UpdateData(APLYNO, InsertModel, searchData, datas, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        public JsonResult UnConfirmedUpdate(string APLYNO, string ActualAccEmp)
        {
            MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            if (APLYNO != null && Cache.IsSet(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData))
            {
                var datas = (List<AfterOpenTreasuryUnconfirmedDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                result = AftereOpenTreasury.UnConfirmedUpdateDatas(APLYNO, ActualAccEmp, datas, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData, result.Datas);
                }
            }
                return Json(result);
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string APLYNO)
        {
            MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
            if (APLYNO != null && Cache.IsSet(CacheList.AfterOpenTreasurySearchDetailViewData))
            {
                var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
                var searchData = (AfterOpenTreasurySearchViewModel)Cache.Get(CacheList.AfterOpenTreasurySearchData);
                result = AftereOpenTreasury.DeleteData(APLYNO, searchData, datas, AccountController.CurrentUserId, AccountController.CustodianFlag);
                if (result.RETURN_FLAG)
                {
                    var rowData = datas.FirstOrDefault(x => x.hvAPLY_NO == APLYNO);
                    datas.Remove(rowData);
                    Cache.Invalidate(CacheList.AfterOpenTreasurySearchDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasurySearchDetailViewData, datas);
                    result.Datas = datas;
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 未確認表單刪除
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <returns></returns>
        public JsonResult UnconfirmedDelete(string APLYNO)
        {
            MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> result = new MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.delete_Fail.GetDescription();
            if (APLYNO != null && Cache.IsSet(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData))
            {
                var datas = (List<AfterOpenTreasuryUnconfirmedDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                result = AftereOpenTreasury.UnconfirmedDeleteData(APLYNO, datas, AccountController.CurrentUserId);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData, result.Datas);
                }
            }
            return Json(result);
        }

        /// <summary>
        /// 產生實際入庫人員選單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetActualUsers(string TreaItem, string ConfirmUid)
        {
            if (!TreaItem.IsNullOrWhiteSpace())
            {
                var datas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
                var empty = new SelectOption() { Text = " ", Value = " " };
                var result = AftereOpenTreasury.GetActualUserOption(TreaItem, datas);
                
                result.Remove(result.FirstOrDefault(x => x.Value == ConfirmUid));
                result.Insert(0, empty);
                return Json(result);
            }
            return null;
        }

        /// <summary>
        /// 產生實際作業項目下拉選單
        /// </summary>
        /// <param name="AccessType"></param>
        /// <returns></returns>
        public JsonResult GetActualAccessType(string AccessType, string SEAL_ID)
        {
            var empty = new SelectOption() { Text = " ", Value = " " };
            var result = AftereOpenTreasury.GetActualAccessTypeOption(SEAL_ID);
            result.Remove(result.FirstOrDefault(x => x.Value == AccessType));
            result.Insert(0, empty);
            return Json(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetConfrimedT(string registerID)
        {
            var result = AftereOpenTreasury.GetConfrimedTime(registerID);
            return Json(result);
        }

        [HttpPost]
        public JsonResult GetCacheData(jqGridParam jdta, string type)
        {
            switch (type)
            {
                case "Search":
                    var SearchDatas = (List<AfterOpenTreasurySearchDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasurySearchDetailViewData);
                    return Json(jdta.modelToJqgridResult(SearchDatas.OrderBy(x => x.vITEM_OP_TYPE).ToList()));
                case "UnconfirmedData":
                    var UnconfirmedDatas = (List<AfterOpenTreasuryUnconfirmedDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                    return Json(jdta.modelToJqgridResult(UnconfirmedDatas.OrderBy(x => x.vITEM_OP_TYPE).ToList()));
            }
            return null;
        }
    
        /// <summary>
        /// 選取事件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="takeoutFlag"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TakeOutData(string checkedmodel, bool takeoutFlag)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData))
            {
                var tempData = (List<AfterOpenTreasuryUnconfirmedDetailViewModel>)Cache.Get(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                var updateTempData = tempData.FirstOrDefault(x => x.hvAPLY_NO == checkedmodel);
                if (updateTempData != null)
                {
                    updateTempData.IsTakeout = takeoutFlag;
                    Cache.Invalidate(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData);
                    Cache.Set(CacheList.AfterOpenTreasuryUnconfirmedDetailViewData, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    result.Datas = tempData.Any(x => x.IsTakeout);
                }
                else
                {
                    result.RETURN_FLAG = false;
                    result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                }
            }
            return Json(result);
        }
    }
}