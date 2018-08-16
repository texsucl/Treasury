using System.Collections.Generic;
using System.Linq;
using System;
using System.Web.Mvc;
using Treasury.Web.Controllers;
using Treasury.Web.Service.Actual;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebActionFilter;
using Treasury.WebUtility;
using Treasury.Web.Enum;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 定期存單
/// 初版作者：20180803 侯蔚鑫
/// 修改歷程：20180803 侯蔚鑫 
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
    public class DepositController : CommonController
    {
        // GET: Deposit
        private IDeposit Deposit;

        public DepositController()
        {
            Deposit = new Deposit();
        }

        /// <summary>
        /// 定期存單 新增畫面
        /// </summary>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="data">金庫物品存取主畫面ViewModel</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult View(string AplyNo, TreasuryAccessViewModel data, Ref.OpenPartialViewType type)
        {
            ViewBag.dInterest_Rate_Type = new SelectList(Deposit.GetInterest_Rate_Type(), "Value", "Text");
            ViewBag.dDep_Type = new SelectList(Deposit.GetDep_Type(), "Value", "Text");
            ViewBag.dYN_Flag = new SelectList(Deposit.GetYN_Flag(), "Value", "Text");
            ViewBag.CustodianFlag = AccountController.CustodianFlag;

            var _dActType = GetActType(type, AplyNo);
            if (AplyNo.IsNullOrWhiteSpace())
            {
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, data);
                resetDepositViewModel(data.vAccessType);
            }
            else
            {
                ViewBag.dAccess = TreasuryAccess.GetAccessType(AplyNo);

                var viewModel = TreasuryAccess.GetTreasuryAccessViewModel(AplyNo);
                Cache.Invalidate(CacheList.TreasuryAccessViewData);
                Cache.Set(CacheList.TreasuryAccessViewData, viewModel);
                resetDepositViewModel(viewModel.vAccessType, AplyNo, _dActType);
            }

            ViewBag.dActType = _dActType;

            return PartialView();
        }

        /// <summary>
        /// 覆核資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyTempData()
        {
            MSGReturnModel<IEnumerable<ITreaItem>> result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = true;  //預設成功

            if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
                data.vCreateUid = AccountController.CurrentUserId;

                //判斷申請作業
                if(data.vAccessType== Ref.AccessProjectTradeType.P.ToString())
                {
                    //檢查是否有覆核資籵
                    if (((List<Deposit_M>)Cache.Get(CacheList.DepositData_M)).Count > 0 && ((List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All)).Count > 0)
                    {
                        var MasterDataList = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
                        //檢查各總項對應明細
                        foreach (var item in MasterDataList)
                        {
                            if (((List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All)).Where(x => x.vItem_Id == item.vItem_Id).ToList().Count == 0)
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "編號" + item.vRowNum + "總項沒有明細資料";
                                break;
                            }
                        }

                        //檢查通過
                        if (result.RETURN_FLAG)
                        {
                            List<DepositViewModel> _datas = new List<DepositViewModel>();
                            DepositViewModel _data = new DepositViewModel();
                            _data.vDeposit_M = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
                            _data.vDeposit_D = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                            _datas.Add(_data);

                            result = Deposit.ApplyAudit(_datas, data);
                        }
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else if(data.vAccessType== Ref.AccessProjectTradeType.G.ToString())
                {
                    //取得所有到期明細資料
                    List<Deposit_M> MasterDataList = new List<Deposit_M>();
                    MasterDataList.AddRange((List<Deposit_M>)Cache.Get(CacheList.DepositData_N));
                    MasterDataList.AddRange((List<Deposit_M>)Cache.Get(CacheList.DepositData_Y));

                    //檢查是否有覆核資籵
                    if (MasterDataList.Where(x => x.vTakeoutFlag == true).ToList().Count > 0)
                    {
                        //檢查通過
                        if (result.RETURN_FLAG)
                        {
                            List<DepositViewModel> _datas = new List<DepositViewModel>();
                            DepositViewModel _data = new DepositViewModel();
                            _data.vDeposit_M = MasterDataList;
                            _datas.Add(_data);

                            result = Deposit.ApplyAudit(_datas, data);
                        }
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }

                if (result.RETURN_FLAG && !data.vAplyNo.IsNullOrWhiteSpace())
                {
                    new TreasuryAccessController().ResetSearchData();
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
        public JsonResult ResetTempData(string AccessType)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            resetDepositViewModel(AccessType);
            return Json(result);
        }

        /// <summary>
        /// 新增定期存單總項資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData_M(Deposit_M model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_M))
            {
                var tempData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
                if (tempData.Count == 0)
                {
                    model.vRowNum = "1";
                }
                else
                {
                    model.vRowNum = (tempData.Max(x => int.Parse(x.vRowNum)) + 1).ToString();
                }
                model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                tempData.Add(model);
                Cache.Invalidate(CacheList.DepositData_M);
                Cache.Set(CacheList.DepositData_M, tempData);
                result.RETURN_FLAG = true;
                result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 新增定期存單明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertTempData_D(Deposit_D model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if(CheckDetail(model))
            {
                if (Cache.IsSet(CacheList.DepositData_D_All))
                {
                    //新增明細全部
                    var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                    tempData.Add(model);
                    Cache.Invalidate(CacheList.DepositData_D_All);
                    Cache.Set(CacheList.DepositData_D_All, tempData);

                    //依編號取出顥示明細
                    var detailData = tempData.Where(x => x.vRowNum == model.vRowNum).ToList();
                    Cache.Invalidate(CacheList.DepositData_D);
                    Cache.Set(CacheList.DepositData_D, detailData);
                    SetTotalDenomination(model.vItem_Id);

                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
                }
            }
            else
            {
                result.DESCRIPTION = "重覆資料";
            }
            return Json(result);
        }

        /// <summary>
        /// 修改定期存單總項資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData_M(Deposit_M model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_M))
            {
                var tempData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
                var updateTempData = tempData.FirstOrDefault(x => x.vItem_Id== model.vItem_Id);
                if (updateTempData != null)
                {
                    updateTempData.vCurrency = model.vCurrency;
                    updateTempData.vTrad_Partners = model.vTrad_Partners;
                    updateTempData.vCommit_Date = model.vCommit_Date;
                    updateTempData.vExpiry_Date = model.vExpiry_Date;
                    updateTempData.vInterest_Rate_Type = model.vInterest_Rate_Type;
                    updateTempData.vInterest_Rate = model.vInterest_Rate;
                    updateTempData.vDep_Type = model.vDep_Type;
                    updateTempData.vTotal_Denomination = model.vTotal_Denomination;
                    updateTempData.vDep_Set_Quality = model.vDep_Set_Quality;
                    updateTempData.vAuto_Trans = model.vAuto_Trans;
                    updateTempData.vTrans_Expiry_Date = model.vTrans_Expiry_Date;
                    updateTempData.vTrans_Tms = model.vTrans_Tms;
                    updateTempData.vMemo = model.vMemo;
                    Cache.Invalidate(CacheList.DepositData_M);
                    Cache.Set(CacheList.DepositData_M, tempData);
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
        /// 修改定期存單明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTempData_D(Deposit_D model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (CheckDetail(model))
            {
                if (Cache.IsSet(CacheList.DepositData_D_All))
                {
                    var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                    var updateTempData = tempData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id && x.vData_Seq == model.vData_Seq);
                    if (updateTempData != null)
                    {
                        //修改明細全部
                        updateTempData.vDep_No_Preamble = model.vDep_No_Preamble;
                        updateTempData.vDep_No_B = model.vDep_No_B;
                        updateTempData.vDep_No_E = model.vDep_No_E;
                        updateTempData.vDep_No_Tail = model.vDep_No_Tail;
                        updateTempData.vDep_Cnt = model.vDep_Cnt;
                        updateTempData.vDenomination = model.vDenomination;
                        updateTempData.vSubtotal_Denomination = model.vSubtotal_Denomination;
                        Cache.Invalidate(CacheList.DepositData_D_All);
                        Cache.Set(CacheList.DepositData_D_All, tempData);

                        //依編號取出顥示明細
                        var detailData = tempData.Where(x => x.vRowNum == model.vRowNum).ToList();
                        Cache.Invalidate(CacheList.DepositData_D);
                        Cache.Set(CacheList.DepositData_D, detailData);
                        SetTotalDenomination(model.vItem_Id);

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                    }
                }
            }
            else
            {
                result.DESCRIPTION = "重覆資料";
            }
            return Json(result);
        }

        /// <summary>
        /// 刪除定期存單總項資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData_M(Deposit_M model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_M))
            {
                var tempData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id);
                if (deleteTempData != null)
                {
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.DepositData_M);
                    Cache.Set(CacheList.DepositData_M, tempData);

                    //同步刪除對應明細資料
                    var detailData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                    detailData.RemoveAll(x => x.vItem_Id == model.vItem_Id);

                    Cache.Invalidate(CacheList.DepositData_D_All);
                    Cache.Set(CacheList.DepositData_D_All, detailData);
                    Cache.Invalidate(CacheList.DepositData_D);
                    Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());

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
        /// 刪除定期存單明細資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData_D(Deposit_D model)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_D_All))
            {
                var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                var deleteTempData = tempData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id && x.vData_Seq == model.vData_Seq);
                if (deleteTempData != null)
                {
                    //刪除明細全部
                    tempData.Remove(deleteTempData);
                    Cache.Invalidate(CacheList.DepositData_D_All);
                    Cache.Set(CacheList.DepositData_D_All, tempData);

                    //依編號取出顥示明細
                    var detailData = tempData.Where(x => x.vRowNum == model.vRowNum).ToList();
                    Cache.Invalidate(CacheList.DepositData_D);
                    Cache.Set(CacheList.DepositData_D, detailData);
                    SetTotalDenomination(model.vItem_Id);

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
        /// 取出事件動作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult TakeOutData(Deposit_M model, bool takeoutFlag)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_N) && Cache.IsSet(CacheList.DepositData_Y))
            {
                List<Deposit_M> tempData = new List<Deposit_M>();
                //判斷設質否
                if (model.vDep_Set_Quality=="N")
                {
                    tempData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_N);
                }
                else if (model.vDep_Set_Quality == "Y")
                {
                    tempData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_Y);
                }
                
                var updateTempData = tempData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id);
                if (updateTempData != null)
                {
                    if (takeoutFlag)
                    {
                        if (model.vDep_Set_Quality == "N")
                        {
                            updateTempData.vStatus = Ref.AccessInventoryType._4.GetDescription();
                        }
                        else if (model.vDep_Set_Quality == "Y")
                        {
                            updateTempData.vStatus = Ref.AccessInventoryType._5.GetDescription();
                        }
                    }
                    else
                    {
                        updateTempData.vStatus = Ref.AccessInventoryType._1.GetDescription();
                    }
                    updateTempData.vTakeoutFlag = takeoutFlag;

                    if (model.vDep_Set_Quality == "N")
                    {
                        Cache.Invalidate(CacheList.DepositData_N);
                        Cache.Set(CacheList.DepositData_N, tempData);
                    }
                    else if (model.vDep_Set_Quality == "Y")
                    {
                        Cache.Invalidate(CacheList.DepositData_Y);
                        Cache.Set(CacheList.DepositData_Y, tempData);
                    }

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
        /// 依物品編號取得定期存單明細
        /// </summary>
        /// <param name="vItem_Id">物品編號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetDetailData(string vItem_Id)
        {
            MSGReturnModel<StockDetailViewModel> result = new MSGReturnModel<StockDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
            var detailData = tempData.Where(x => x.vItem_Id == vItem_Id).ToList();
            Cache.Invalidate(CacheList.DepositData_D);
            Cache.Set(CacheList.DepositData_D, detailData);
            result.RETURN_FLAG = true;

            return Json(result);
        }

        /// <summary>
        /// 查詢定期存單
        /// </summary>
        /// <param name="vTransExpiryDateFrom">定存單到期日(起)</param>
        /// <param name="vTransExpiryDateTo">定存單到期日(迄)</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SearchData(string vTransExpiryDateFrom,string vTransExpiryDateTo)
        {
            MSGReturnModel<StockDetailViewModel> result = new MSGReturnModel<StockDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            Cache.Invalidate(CacheList.DepositData_N);
            Cache.Invalidate(CacheList.DepositData_Y);

            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            DepositViewModel DepositData = new DepositViewModel();

            //取得定期存單DB庫存資料
            DepositData = Deposit.GetDbDataByUnit(data.vAplyUnit,data.vAplyNo, vTransExpiryDateFrom, vTransExpiryDateTo);//抓庫存+單號

            Cache.Set(CacheList.DepositData_N, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "N").ToList());
            Cache.Set(CacheList.DepositData_Y, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "Y").ToList());

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
                case "M":
                    if (Cache.IsSet(CacheList.DepositData_M))
                        return Json(jdata.modelToJqgridResult(((List<Deposit_M>)Cache.Get(CacheList.DepositData_M)).OrderBy(x=>x.vRowNum).ToList()));
                    break;
                case "D":
                    if (Cache.IsSet(CacheList.DepositData_D))
                        return Json(jdata.modelToJqgridResult(((List<Deposit_D>)Cache.Get(CacheList.DepositData_D)).OrderBy(x=>x.vData_Seq).ToList()));
                    break;
                case "N":
                    if (Cache.IsSet(CacheList.DepositData_N))
                        return Json(jdata.modelToJqgridResult(((List<Deposit_M>)Cache.Get(CacheList.DepositData_N)).OrderBy(x => x.vItem_Id).ToList()));
                    break;
                case "Y":
                    if (Cache.IsSet(CacheList.DepositData_Y))
                        return Json(jdata.modelToJqgridResult(((List<Deposit_M>)Cache.Get(CacheList.DepositData_Y)).OrderBy(x => x.vItem_Id).ToList()));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 設定定期存單Cache資料
        /// </summary>
        /// <param name="AccessType">申請狀態</param>
        /// <param name="AplyNo">申請單號</param>
        /// <param name="EditFlag">修改狀態</param>
        /// <returns></returns>
        private void resetDepositViewModel(string AccessType, string AplyNo = null, bool EditFlag = false)
        {
            Cache.Invalidate(CacheList.DepositData_M);
            Cache.Invalidate(CacheList.DepositData_N);
            Cache.Invalidate(CacheList.DepositData_Y);
            Cache.Invalidate(CacheList.DepositData_D);
            Cache.Invalidate(CacheList.DepositData_D_All);
            var data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            DepositViewModel DepositData = new DepositViewModel();

            if (AplyNo.IsNullOrWhiteSpace())
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    Cache.Set(CacheList.DepositData_M, new List<Deposit_M>());
                    Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());
                    Cache.Set(CacheList.DepositData_D_All, new List<Deposit_D>());
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    //取得定期存單DB庫存資料
                    DepositData = Deposit.GetDbDataByUnit(data.vAplyUnit);//只抓庫存
                    Cache.Set(CacheList.DepositData_N, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "N").ToList());
                    Cache.Set(CacheList.DepositData_Y, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "Y").ToList());
                    Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());
                    Cache.Set(CacheList.DepositData_D_All, DepositData.vDeposit_D);
                }
            }
            else
            {
                if (AccessType == Ref.AccessProjectTradeType.P.ToString())
                {
                    DepositData = Deposit.GetDataByAplyNo(AplyNo);//抓單號
                    Cache.Set(CacheList.DepositData_M, DepositData.vDeposit_M);
                    Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());
                    Cache.Set(CacheList.DepositData_D_All, DepositData.vDeposit_D);
                }
                if (AccessType == Ref.AccessProjectTradeType.G.ToString())
                {
                    if (EditFlag && Aply_Appr_Type.Contains(TreasuryAccess.GetStatus(AplyNo))) //可以修改
                    {
                        DepositData = Deposit.GetDbDataByUnit(data.vAplyUnit, AplyNo);//抓庫存+單號
                        Cache.Set(CacheList.DepositData_N, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "N").ToList());
                        Cache.Set(CacheList.DepositData_Y, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "Y").ToList());
                        Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());
                        Cache.Set(CacheList.DepositData_D_All, DepositData.vDeposit_D);
                    }
                    else
                    {
                        DepositData = Deposit.GetDataByAplyNo(AplyNo);//抓單號
                        Cache.Set(CacheList.DepositData_N, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "N").ToList());
                        Cache.Set(CacheList.DepositData_Y, DepositData.vDeposit_M.Where(x => x.vDep_Set_Quality == "Y").ToList());
                        Cache.Set(CacheList.DepositData_D, new List<Deposit_D>());
                        Cache.Set(CacheList.DepositData_D_All, DepositData.vDeposit_D);
                    }
                }
            }
        }

        /// <summary>
        /// 檢查明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Boolean CheckDetail(Deposit_D model)
        {
            Boolean isDetail = true;

            TreasuryAccessViewModel data = (TreasuryAccessViewModel)Cache.Get(CacheList.TreasuryAccessViewData);
            List<Deposit_D> CheckDetailList = new List<Deposit_D>();

            #region 取得當次申請的資料
            //取出對應總項資料
            var MasterData = ((List<Deposit_M>)Cache.Get(CacheList.DepositData_M)).FirstOrDefault(x => x.vItem_Id == model.vItem_Id);
            //取出相同交易對象物品編號清單
            var Trad_Partners_List = ((List<Deposit_M>)Cache.Get(CacheList.DepositData_M)).Where(x => x.vTrad_Partners == MasterData.vTrad_Partners).Select(x=>x.vItem_Id).ToList();

            //取出符合交易對象的明細清單
            CheckDetailList.AddRange(((List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All)).Where(x=>Trad_Partners_List.Contains(x.vItem_Id)));
            #endregion

            #region 取得庫存DB資料
            List<Deposit_D> DbDetailList = Deposit.GetDataByTradPartners(MasterData.vTrad_Partners, data.vAplyNo);

            //將DB清單加入檢查清單
            CheckDetailList.AddRange(DbDetailList);
            #endregion

            #region 檢查存單號碼
            foreach (var item in CheckDetailList)
            {
                //非本筆資料
                if (item.vItem_Id != model.vItem_Id || item.vData_Seq != model.vData_Seq)
                {
                    //判斷存單號碼前置碼及存單號碼尾碼
                    if (item.vDep_No_Preamble == model.vDep_No_Preamble && item.vDep_No_Tail == model.vDep_No_Tail)
                    {
                        #region 檢查起迄區間
                        if (int.Parse(item.vDep_No_B) >= int.Parse(model.vDep_No_B))
                        {
                            if(int.Parse(item.vDep_No_B)<=int.Parse(model.vDep_No_E))
                            {
                                isDetail = false;
                            }
                        }
                        else
                        {
                            if(int.Parse(item.vDep_No_E)>=int.Parse(model.vDep_No_B))
                            {
                                isDetail = false;
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion

            return isDetail;
        }

        /// <summary>
        /// 設定總面額
        /// <param name="vItem_Id"></param>
        /// </summary>
        /// <returns></returns>
        private void SetTotalDenomination(string vItem_Id)
        {
            //計算出總面額
            var DetailDataList = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D);
            var TotalDenomination = DetailDataList.Sum(x => x.vSubtotal_Denomination);

            //修改對應總項資料的總面額
            var MasterDataList = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
            var updateTempData = MasterDataList.FirstOrDefault(x => x.vItem_Id == vItem_Id);
            updateTempData.vTotal_Denomination = TotalDenomination;
            Cache.Invalidate(CacheList.DepositData_M);
            Cache.Set(CacheList.DepositData_M, MasterDataList);
        }
    }
}