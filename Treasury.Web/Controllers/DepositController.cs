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
namespace Treasury.Web.Controllers
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
            ViewBag.OPVT = type;
            ViewBag.dCurrency = new SelectList(Deposit.GetCurrency(), "Value", "Text", "NTD");
            ViewBag.dTrad_Partners = new SelectList(Deposit.GetTrad_Partners(), "Value", "Text");
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
        /// 定期存單 資料庫異動畫面
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CDCView(string AplyNo, CDCSearchViewModel data, Ref.OpenPartialViewType type)
        {
            var _data = ((List<CDCDepositViewModel>)Deposit.GetCDCSearchData(data, AplyNo));
            ViewBag.Sataus = new Service.Actual.Common().GetSysCode("INVENTORY_TYPE");
            ViewBag.type = type;
            ViewBag.IO = data.vTreasuryIO;
            ViewBag.dCurrency = new SelectList(Deposit.GetCurrency(), "Value", "Text", "NTD");
            ViewBag.dTrad_Partners = new SelectList(Deposit.GetTrad_Partners(), "Value", "Text");
            ViewBag.dInterest_Rate_Type = new SelectList(Deposit.GetInterest_Rate_Type(), "Value", "Text");
            ViewBag.dDep_Type = new SelectList(Deposit.GetDep_Type(), "Value", "Text");
            ViewBag.dYN_Flag = new SelectList(Deposit.GetYN_Flag(), "Value", "Text");

            data.vCreate_Uid = AccountController.CurrentUserId;
            Cache.Invalidate(CacheList.CDCSearchViewModel);
            Cache.Set(CacheList.CDCSearchViewModel, data);
            Cache.Invalidate(CacheList.CDCDepositDataM);
            Cache.Set(CacheList.CDCDepositDataM, _data[0].vDeposit_M);
            Cache.Invalidate(CacheList.CDCDepositDataD_All);
            Cache.Set(CacheList.CDCDepositDataD_All, _data[0].vDeposit_D);
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
            //要為True 否則檢核會失敗
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
                        string dataTime = DateTime.Now.Date.ToString("yyyy/MM/dd");
                        if (MasterDataList.Where(x => x.vTakeoutFlag && x.vExpiry_Date != dataTime).Any(x=>x.GetMsg.IsNullOrWhiteSpace()))
                        {
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = "到期日不等於系統日,需要有取出原因";
                            return Json(result);
                        }

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
                    model.vRowNum = 1;
                    model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                    tempData.Add(model);
                    Cache.Invalidate(CacheList.DepositData_M);
                    Cache.Set(CacheList.DepositData_M, tempData);
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
                }
                else
                {
                    //同一申請單號內的承作日期要一樣
                    if (tempData.Select(x => x.vCommit_Date).FirstOrDefault() == model.vCommit_Date)
                    {
                        model.vRowNum = (tempData.Max(x => x.vRowNum) + 1);
                        model.vStatus = Ref.AccessInventoryType._3.GetDescription();
                        tempData.Add(model);
                        Cache.Invalidate(CacheList.DepositData_M);
                        Cache.Set(CacheList.DepositData_M, tempData);
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Ref.MessageType.insert_Success.GetDescription();
                    }
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "同一申請單號內的承作日期要一樣!";
                    }
                }
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

            if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                if (CheckDetail(model))
                {
                    if (Cache.IsSet(CacheList.DepositData_D_All))
                    {
                        var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                        var checkDetailData = tempData.Where(x => x.vRowNum == model.vRowNum).ToList();

                        if (checkDetailData.Count == 0)
                        {
                            //新增明細全部
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
                        else
                        {
                            //同編號明細檢核
                            if (checkDetailData.Select(x => x.vDep_No_Preamble).FirstOrDefault() != model.vDep_No_Preamble)
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "同一編號內定期存單明細存單號號前置碼要一樣!";
                            }
                            else if (checkDetailData.Select(x => x.vDep_No_B).FirstOrDefault().Length != model.vDep_No_B.Length)
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "同一編號內定期存單明細存單存單號碼(起)/ 存單號碼(迄)碼數要一樣!";
                            }
                            else
                            {
                                //新增明細全部
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
                    }
                }
                else
                {
                    result.DESCRIPTION = "重覆資料";
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
                var updateTempData = tempData.FirstOrDefault(x => x.vItem_Id == model.vItem_Id);

                if (tempData.Count > 1)
                {
                    //同一申請單號內的承作日期要一樣
                    if (tempData.Select(x => x.vCommit_Date).FirstOrDefault() == model.vCommit_Date)
                    {
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
                    else
                    {
                        result.RETURN_FLAG = false;
                        result.DESCRIPTION = "同一申請單號內的承作日期要一樣!";
                    }
                }
                else
                {
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

            if (Cache.IsSet(CacheList.TreasuryAccessViewData))
            {
                if (CheckDetail(model))
                {
                    if (Cache.IsSet(CacheList.DepositData_D_All))
                    {
                        var tempData = (List<Deposit_D>)Cache.Get(CacheList.DepositData_D_All);
                        var checkDetailData = tempData.Where(x => x.vRowNum == model.vRowNum).ToList();

                        if (checkDetailData.Count > 1)
                        {
                            //同編號明細檢核
                            if (checkDetailData.Select(x => x.vDep_No_Preamble).FirstOrDefault() != model.vDep_No_Preamble)
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "同一編號內定期存單明細存單號號前置碼要一樣!";
                            }
                            else if (checkDetailData.Select(x => x.vDep_No_B).FirstOrDefault().Length != model.vDep_No_B.Length)
                            {
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "同一編號內定期存單明細存單存單號碼(起)/ 存單號碼(迄)碼數要一樣!";
                            }
                            else
                            {
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
                }
                else
                {
                    result.DESCRIPTION = "重覆資料";
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
        /// 刪除定期存單總項資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteTempData_M(Deposit_M model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
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
                    result.Datas = tempData.Any();
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
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.DepositData_D_All))
            {
                var tempData_M = (List<Deposit_M>)Cache.Get(CacheList.DepositData_M);
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
                    result.Datas = tempData_M.Any();
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
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
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
                    result.Datas = tempData.Any(x => x.vTakeoutFlag);
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
        /// 依物品編號取得異動定期存單明細
        /// </summary>
        /// <param name="vItem_Id">物品編號</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetCDC_DetailData(string vItem_Id)
        {
            MSGReturnModel<StockDetailViewModel> result = new MSGReturnModel<StockDetailViewModel>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            var tempData = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);
            var detailData = tempData.Where(x => x.vItemId == vItem_Id).ToList();
            Cache.Invalidate(CacheList.CDCDepositDataD);
            Cache.Set(CacheList.CDCDepositDataD, detailData);
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
        /// 依申請單號取得列印群組筆數
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetReportGroupData(string vAplyNo)
        {
            MSGReturnModel<List<DepositReportGroupData>> result = new MSGReturnModel<List<DepositReportGroupData>>();

            result.Datas = Deposit.GetReportGroupData(vAplyNo);
            if (result.Datas.Count > 0)
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
        /// jqgrid CDCcache data
        /// </summary>
        /// <param name="jdata"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCDCCacheData(jqGridParam jdata, string type)
        {
            switch (type)
            {
                case "M":
                    if (Cache.IsSet(CacheList.CDCDepositDataM))
                        return Json(jdata.modelToJqgridResult(((List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM)).OrderBy(x => x.vItemId).ToList()));
                    break;
                case "D":
                    if (Cache.IsSet(CacheList.CDCDepositDataD))
                        return Json(jdata.modelToJqgridResult(((List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD)).OrderBy(x => x.vData_Seq).ToList()));
                    break;
            }
            return null;
        }

        /// <summary>
        /// 修改資料庫資料(主檔)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbDataM(CDCDeposit_M model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCDepositDataM))
            {
                var dbData = (List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM);
                var dbDataD = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);
                var detailData = dbDataD.Where(x => x.vItemId == model.vItemId).ToList();
                var checkLength = true;
                //明細序號檢驗
                if (detailData.Count > 1)
                {
                    foreach(var item in detailData)
                    {
                        //抓取自已的序號長度
                        var New_Dep_No_Length = string.IsNullOrEmpty(item.vDep_No_B_Aft) ? item.vDep_No_B.Length : item.vDep_No_B_Aft.Length;

                        //比對其他物品序號長度
                        var Check_detailData = detailData.Where(x => x.vData_Seq != item.vData_Seq).ToList();
                        foreach(var Check_item in Check_detailData)
                        {
                            var Check_Dep_No_Length = string.IsNullOrEmpty(Check_item.vDep_No_B_Aft) ? Check_item.vDep_No_B.Length : Check_item.vDep_No_B_Aft.Length;

                            if(New_Dep_No_Length!= Check_Dep_No_Length)
                            {
                                checkLength = false;
                                result.RETURN_FLAG = false;
                                result.DESCRIPTION = "同一編號內定期存單明細存單存單號碼(起)/ 存單號碼(迄)碼數要一樣!";
                            }
                        }
                    }
                }

                if(checkLength)
                {
                    var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId);
                    if (updateTempData != null)
                    {
                        var _vCurrency_Aft = model.vCurrency.CheckAFT(updateTempData.vCurrency);
                        if (_vCurrency_Aft.Item2)
                            updateTempData.vCurrency_Aft = _vCurrency_Aft.Item1;
                        var _vTrad_Partners_Aft = model.vTrad_Partners.CheckAFT(updateTempData.vTrad_Partners);
                        if (_vTrad_Partners_Aft.Item2)
                            updateTempData.vTrad_Partners_Aft = _vTrad_Partners_Aft.Item1;
                        var _vCommit_Date_Aft = model.vCommit_Date.CheckAFT(updateTempData.vCommit_Date);
                        if (_vCommit_Date_Aft.Item2)
                            updateTempData.vCommit_Date_Aft = _vCommit_Date_Aft.Item1;
                        var _vExpiry_Date_Aft = model.vExpiry_Date.CheckAFT(updateTempData.vExpiry_Date);
                        if (_vExpiry_Date_Aft.Item2)
                            updateTempData.vExpiry_Date_Aft = _vExpiry_Date_Aft.Item1;
                        var _vInterest_Rate_Type_Aft = model.vInterest_Rate_Type.CheckAFT(updateTempData.vInterest_Rate_Type);
                        if (_vInterest_Rate_Type_Aft.Item2)
                            updateTempData.vInterest_Rate_Type_Aft = _vInterest_Rate_Type_Aft.Item1;
                        var _vInterest_Rate_Aft = model.vInterest_Rate.CheckAFT(updateTempData.vInterest_Rate);
                        if (_vInterest_Rate_Aft.Item2)
                            updateTempData.vInterest_Rate_Aft = _vInterest_Rate_Aft.Item1;
                        var _vDep_Type_Aft = model.vDep_Type.CheckAFT(updateTempData.vDep_Type);
                        if (_vDep_Type_Aft.Item2)
                            updateTempData.vDep_Type_Aft = _vDep_Type_Aft.Item1;
                        var _vTotal_Denomination_Aft = TypeTransfer.decimalNToString(model.vTotal_Denomination).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vTotal_Denomination));
                        if (_vTotal_Denomination_Aft.Item2)
                            updateTempData.vTotal_Denomination_Aft = TypeTransfer.stringToDecimal(_vTotal_Denomination_Aft.Item1);
                        var _vDep_Set_Quality_Aft = model.vDep_Set_Quality.CheckAFT(updateTempData.vDep_Set_Quality);
                        if (_vDep_Set_Quality_Aft.Item2)
                            updateTempData.vDep_Set_Quality_Aft = _vDep_Set_Quality_Aft.Item1;
                        var _vAuto_Trans_Aft = model.vAuto_Trans.CheckAFT(updateTempData.vAuto_Trans);
                        if (_vAuto_Trans_Aft.Item2)
                            updateTempData.vAuto_Trans_Aft = _vAuto_Trans_Aft.Item1;
                        var _vTrans_Expiry_Date_Aft = model.vTrans_Expiry_Date.CheckAFT(updateTempData.vTrans_Expiry_Date);
                        if (_vTrans_Expiry_Date_Aft.Item2)
                            updateTempData.vTrans_Expiry_Date_Aft = _vTrans_Expiry_Date_Aft.Item1;
                        var _vMemo_Aft = model.vMemo.CheckAFT(updateTempData.vMemo);
                        if (_vMemo_Aft.Item2)
                            updateTempData.vMemo_Aft = _vMemo_Aft.Item1;
                        var _vTrans_Tms_Aft = TypeTransfer.intNToString(model.vTrans_Tms).CheckAFT(TypeTransfer.intNToString(updateTempData.vTrans_Tms));
                        if (_vTrans_Tms_Aft.Item2)
                            updateTempData.vTrans_Tms_Aft = TypeTransfer.stringToIntN(_vTrans_Tms_Aft.Item1);

                        updateTempData.vAftFlag = detailData.Any(x => x.vAftFlag) || _vCurrency_Aft.Item2 || _vTrad_Partners_Aft.Item2 || _vCommit_Date_Aft.Item2 ||
                            _vExpiry_Date_Aft.Item2 || _vInterest_Rate_Type_Aft.Item2 || _vInterest_Rate_Aft.Item2 || _vDep_Type_Aft.Item2 ||
                            _vTotal_Denomination_Aft.Item2 || _vDep_Set_Quality_Aft.Item2 || _vAuto_Trans_Aft.Item2 || _vTrans_Expiry_Date_Aft.Item2 ||
                            _vMemo_Aft.Item2 || _vTrans_Tms_Aft.Item2;

                        updateTempData.sAutoTransFlag = model.sAutoTransFlag;
                        if (model.sAutoTransFlag == "Y")
                        {
                            var _vAlready_Trans_Tms_Aft = model.vAlready_Trans_Tms.CheckAFT(updateTempData.vAlready_Trans_Tms);
                            if (_vAlready_Trans_Tms_Aft.Item2)
                            {
                                updateTempData.vAlready_Trans_Tms_Aft = _vAlready_Trans_Tms_Aft.Item1;
                                updateTempData.vAftFlag = true;
                            }
                        }
                        else if(model.sAutoTransFlag == "N")
                            updateTempData.vAftFlag = true;

                        Cache.Invalidate(CacheList.CDCDepositDataM);
                        Cache.Set(CacheList.CDCDepositDataM, dbData);
                        result.Datas = dbData.Any(x => x.vAftFlag);
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

            return Json(result);
        }

        /// <summary>
        /// 修改資料庫資料(明細)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateDbDataD(CDCDeposit_D model)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();

            if(CheckCDC_Detail(model))
            {
                if (Cache.IsSet(CacheList.CDCDepositDataD_All))
                {
                    var dbData = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);
                    var checkDetailData = dbData.Where(x => x.vItemId == model.vItemId).ToList();

                    if (checkDetailData.Count > 1)
                    {
                        var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId && x.vData_Seq == model.vData_Seq);

                        if (updateTempData != null)
                        {
                            var _vDep_No_Preamble_Aft = model.vDep_No_Preamble.CheckAFT(updateTempData.vDep_No_Preamble);
                            if (_vDep_No_Preamble_Aft.Item2)
                                updateTempData.vDep_No_Preamble_Aft = _vDep_No_Preamble_Aft.Item1;
                            var _vDep_No_B_Aft = model.vDep_No_B.CheckAFT(updateTempData.vDep_No_B);
                            if (_vDep_No_B_Aft.Item2)
                                updateTempData.vDep_No_B_Aft = _vDep_No_B_Aft.Item1;
                            var _vDep_No_E_Aft = model.vDep_No_E.CheckAFT(updateTempData.vDep_No_E);
                            if (_vDep_No_E_Aft.Item2)
                                updateTempData.vDep_No_E_Aft = _vDep_No_E_Aft.Item1;
                            var _vDep_No_Tail_Aft = model.vDep_No_Tail.CheckAFT(updateTempData.vDep_No_Tail);
                            if (_vDep_No_Tail_Aft.Item2)
                                updateTempData.vDep_No_Tail_Aft = _vDep_No_Tail_Aft.Item1;
                            var _vDep_Cnt_Aft = TypeTransfer.intNToString(model.vDep_Cnt).CheckAFT(TypeTransfer.intNToString(updateTempData.vDep_Cnt));
                            if (_vDep_Cnt_Aft.Item2)
                                updateTempData.vDep_Cnt_Aft = TypeTransfer.stringToIntN(_vDep_Cnt_Aft.Item1);
                            var _vDenomination_Aft = TypeTransfer.decimalNToString(model.vDenomination).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vDenomination));
                            if (_vDenomination_Aft.Item2)
                                updateTempData.vDenomination_Aft = TypeTransfer.stringToDecimal(_vDenomination_Aft.Item1);
                            var _vSubtotal_Denomination_Aft = TypeTransfer.decimalNToString(model.vSubtotal_Denomination).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vSubtotal_Denomination));
                            if (_vSubtotal_Denomination_Aft.Item2)
                                updateTempData.vSubtotal_Denomination_Aft = TypeTransfer.stringToDecimal(_vSubtotal_Denomination_Aft.Item1);

                            updateTempData.vAftFlag = _vDep_No_Preamble_Aft.Item2 || _vDep_No_B_Aft.Item2 || _vDep_No_E_Aft.Item2 || _vDep_No_Tail_Aft.Item2 ||
                                _vDep_Cnt_Aft.Item2 || _vDenomination_Aft.Item2 || _vSubtotal_Denomination_Aft.Item2;


                            //同一編號內定期存單明細存單號號前置碼一起調整
                            if (_vDep_No_Preamble_Aft.Item2)
                            {
                                foreach (var item in checkDetailData)
                                {
                                    item.vDep_No_Preamble_Aft = model.vDep_No_Preamble;
                                    item.vAftFlag = true;
                                }
                            }

                            Cache.Invalidate(CacheList.CDCDepositDataD_All);
                            Cache.Set(CacheList.CDCDepositDataD_All, dbData);

                            //依編號取出顥示明細
                            var detailData = dbData.Where(x => x.vItemId == model.vItemId).ToList();
                            Cache.Invalidate(CacheList.CDCDepositDataD);
                            Cache.Set(CacheList.CDCDepositDataD, detailData);
                            SetCDC_TotalDenomination(model.vItemId);

                            result.Datas = dbData.Any(x => x.vAftFlag);
                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                        }
                        else
                        {
                            result.RETURN_FLAG = false;
                            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
                        }
                    }
                    else
                    {
                        var updateTempData = dbData.FirstOrDefault(x => x.vItemId == model.vItemId && x.vData_Seq == model.vData_Seq);

                        if (updateTempData != null)
                        {
                            var _vDep_No_Preamble_Aft = model.vDep_No_Preamble.CheckAFT(updateTempData.vDep_No_Preamble);
                            if (_vDep_No_Preamble_Aft.Item2)
                                updateTempData.vDep_No_Preamble_Aft = _vDep_No_Preamble_Aft.Item1;
                            var _vDep_No_B_Aft = model.vDep_No_B.CheckAFT(updateTempData.vDep_No_B);
                            if (_vDep_No_B_Aft.Item2)
                                updateTempData.vDep_No_B_Aft = _vDep_No_B_Aft.Item1;
                            var _vDep_No_E_Aft = model.vDep_No_E.CheckAFT(updateTempData.vDep_No_E);
                            if (_vDep_No_E_Aft.Item2)
                                updateTempData.vDep_No_E_Aft = _vDep_No_E_Aft.Item1;
                            var _vDep_No_Tail_Aft = model.vDep_No_Tail.CheckAFT(updateTempData.vDep_No_Tail);
                            if (_vDep_No_Tail_Aft.Item2)
                                updateTempData.vDep_No_Tail_Aft = _vDep_No_Tail_Aft.Item1;
                            var _vDep_Cnt_Aft = TypeTransfer.intNToString(model.vDep_Cnt).CheckAFT(TypeTransfer.intNToString(updateTempData.vDep_Cnt));
                            if (_vDep_Cnt_Aft.Item2)
                                updateTempData.vDep_Cnt_Aft = TypeTransfer.stringToIntN(_vDep_Cnt_Aft.Item1);
                            var _vDenomination_Aft = TypeTransfer.decimalNToString(model.vDenomination).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vDenomination));
                            if (_vDenomination_Aft.Item2)
                                updateTempData.vDenomination_Aft = TypeTransfer.stringToDecimal(_vDenomination_Aft.Item1);
                            var _vSubtotal_Denomination_Aft = TypeTransfer.decimalNToString(model.vSubtotal_Denomination).CheckAFT(TypeTransfer.decimalNToString(updateTempData.vSubtotal_Denomination));
                            if (_vSubtotal_Denomination_Aft.Item2)
                                updateTempData.vSubtotal_Denomination_Aft = TypeTransfer.stringToDecimal(_vSubtotal_Denomination_Aft.Item1);

                            updateTempData.vAftFlag = _vDep_No_Preamble_Aft.Item2 || _vDep_No_B_Aft.Item2 || _vDep_No_E_Aft.Item2 || _vDep_No_Tail_Aft.Item2 ||
                                _vDep_Cnt_Aft.Item2 || _vDenomination_Aft.Item2 || _vSubtotal_Denomination_Aft.Item2;

                            Cache.Invalidate(CacheList.CDCDepositDataD_All);
                            Cache.Set(CacheList.CDCDepositDataD_All, dbData);

                            //依編號取出顥示明細
                            var detailData = dbData.Where(x => x.vItemId == model.vItemId).ToList();
                            Cache.Invalidate(CacheList.CDCDepositDataD);
                            Cache.Set(CacheList.CDCDepositDataD, detailData);
                            SetCDC_TotalDenomination(model.vItemId);

                            result.Datas = dbData.Any(x => x.vAftFlag);
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
            }
            else
            {
                result.DESCRIPTION = "重覆資料";
            }

            return Json(result);
        }

        /// <summary>
        /// 重設資料庫資料(主檔)
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RepeatDbDataM(string itemId)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCDepositDataM))
            {
                var dbData = (List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM);
                var dbDataD = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);
                var detailData = dbDataD.Where(x => x.vItemId == itemId).ToList();

                //清空主檔
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);
                if (updateTempData != null)
                {
                    updateTempData.vCurrency_Aft = null;
                    updateTempData.vTrad_Partners_Aft = null;
                    updateTempData.vCommit_Date_Aft = "";
                    updateTempData.vExpiry_Date_Aft = "";
                    updateTempData.vInterest_Rate_Type_Aft = null;
                    updateTempData.vInterest_Rate_Aft = null;
                    updateTempData.vDep_Type_Aft = null;
                    updateTempData.vTotal_Denomination_Aft = null;
                    updateTempData.vDep_Set_Quality_Aft = null;
                    updateTempData.vAuto_Trans_Aft = null;
                    updateTempData.vTrans_Expiry_Date_Aft = "";
                    updateTempData.vMemo_Aft = null;
                    updateTempData.vTrans_Tms_Aft = null;
                    updateTempData.vAlready_Trans_Tms_Aft = null;
                    updateTempData.vAftFlag = false;
                    updateTempData.sAutoTransFlag = null;

                    Cache.Invalidate(CacheList.CDCDepositDataM);
                    Cache.Set(CacheList.CDCDepositDataM, dbData);

                    //清空明細
                    foreach (var item in detailData)
                    {
                        item.vDep_No_Preamble_Aft = null;
                        item.vDep_No_B_Aft = null;
                        item.vDep_No_E_Aft = null;
                        item.vDep_No_Tail_Aft = null;
                        item.vDep_Cnt_Aft = null;
                        item.vDenomination_Aft = null;
                        item.vSubtotal_Denomination_Aft = null;
                        item.vAftFlag = false;
                    }

                    Cache.Invalidate(CacheList.CDCDepositDataD_All);
                    Cache.Set(CacheList.CDCDepositDataD_All, dbDataD);

                    result.Datas = dbData.Any(x => x.vAftFlag);
                    result.RETURN_FLAG = true;
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
        /// 重設資料庫資料(明細)
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="Data_Seq"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RepeatDbDataD(string itemId,string Data_Seq)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCDepositDataD_All))
            {
                var dbData = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);

                var checkDetailData = dbData.Where(x => x.vItemId == itemId).ToList();
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId && x.vData_Seq == Data_Seq);

                if (updateTempData != null)
                {
                    //同一編號內定期存單明細存單號號前置碼一起調整
                    if (checkDetailData.Count > 1)
                    {
                        if (!string.IsNullOrEmpty(updateTempData.vDep_No_Preamble_Aft))
                        {
                            foreach (var item in checkDetailData)
                            {
                                item.vDep_No_Preamble_Aft = null;
                                item.vAftFlag = !string.IsNullOrEmpty(item.vDep_No_B_Aft) || !string.IsNullOrEmpty(item.vDep_No_E_Aft) ||
                                    item.vDep_Cnt_Aft != null || item.vDenomination_Aft != null || item.vSubtotal_Denomination_Aft != null;
                            }
                        }
                    }

                    updateTempData.vDep_No_Preamble_Aft = null;
                    updateTempData.vDep_No_B_Aft = null;
                    updateTempData.vDep_No_E_Aft = null;
                    updateTempData.vDep_No_Tail_Aft = null;
                    updateTempData.vDep_Cnt_Aft = null;
                    updateTempData.vDenomination_Aft = null;
                    updateTempData.vSubtotal_Denomination_Aft = null;
                    updateTempData.vAftFlag = false;
                    Cache.Invalidate(CacheList.CDCDepositDataD_All);
                    Cache.Set(CacheList.CDCDepositDataD_All, dbData);

                    //依編號取出顥示明細
                    var detailData = dbData.Where(x => x.vItemId == itemId).ToList();
                    Cache.Invalidate(CacheList.CDCDepositDataD);
                    Cache.Set(CacheList.CDCDepositDataD, detailData);
                    SetCDC_TotalDenomination(itemId);

                    result.Datas = detailData.Any(x => x.vAftFlag);
                    result.RETURN_FLAG = true;
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
        /// 重設資料
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ResetData(string itemId)
        {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            if (Cache.IsSet(CacheList.CDCDepositDataD_All))
            {
                var dbData = (List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM);
                var dbDataD = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);
                //清空主檔
                var updateTempData = dbData.FirstOrDefault(x => x.vItemId == itemId);

                updateTempData.vCurrency_Aft = null;
                updateTempData.vTrad_Partners_Aft = null;
                updateTempData.vCommit_Date_Aft = "";
                updateTempData.vExpiry_Date_Aft = "";
                updateTempData.vInterest_Rate_Type_Aft = null;
                updateTempData.vInterest_Rate_Aft = null;
                updateTempData.vDep_Type_Aft = null;
                updateTempData.vTotal_Denomination_Aft = null;
                updateTempData.vDep_Set_Quality_Aft = null;
                updateTempData.vAuto_Trans_Aft = null;
                updateTempData.vTrans_Expiry_Date_Aft = "";
                updateTempData.vMemo_Aft = null;
                updateTempData.vTrans_Tms_Aft = null;
                updateTempData.vAftFlag = false;

                Cache.Invalidate(CacheList.CDCDepositDataM);
                Cache.Set(CacheList.CDCDepositDataM, dbData);

                //清空明細
                var checkDetailData = dbDataD.Where(x => x.vItemId == itemId).ToList();

                foreach (var item in checkDetailData)
                {
                    item.vDep_No_Preamble_Aft = null;
                    item.vDep_No_B_Aft = null;
                    item.vDep_No_E_Aft = null;
                    item.vDep_No_Tail_Aft = null;
                    item.vDep_Cnt_Aft = null;
                    item.vDenomination_Aft = null;
                    item.vSubtotal_Denomination_Aft = null;
                    item.vAftFlag = false;
                }

                Cache.Invalidate(CacheList.CDCDepositDataD_All);
                Cache.Set(CacheList.CDCDepositDataD_All, dbDataD);

                result.Datas = dbData.Any(x => x.vAftFlag);
                result.RETURN_FLAG = true;
            }
            return Json(result);
        }

        /// <summary>
        /// 申請資料庫異動覆核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApplyDbData()
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            var _data = new List<CDCDepositViewModel>();

            var _Deposit_M = (List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM);
            var _Deposit_D = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All);

            if (!_Deposit_M.Any(x => x.vAftFlag))
            {
                result.DESCRIPTION = "無申請任何資料";
            }
            else if (Cache.IsSet(CacheList.CDCSearchViewModel))
            {
                CDCSearchViewModel data = (CDCSearchViewModel)Cache.Get(CacheList.CDCSearchViewModel);
                _data.Add(new CDCDepositViewModel()
                {
                    vDeposit_M = _Deposit_M.Where(x => x.vAftFlag).ToList(),
                    vDeposit_D = _Deposit_D.Where(x => x.vAftFlag).ToList()
                });
                result = Deposit.CDCApplyAudit(_data, data);
                if (result.RETURN_FLAG)
                {
                    Cache.Invalidate(CacheList.CDCStockDataD);
                    Cache.Set(CacheList.CDCStockDataD, result.Datas);
                }
            }
            else
            {
                result.DESCRIPTION = Ref.MessageType.login_Time_Out.GetDescription();
            }
            return Json(result);
        }

        /// <summary>
        /// 設定取出原因
        /// </summary>
        /// <param name="ItemId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SetGetMsg(string ItemId,string msg) {
            MSGReturnModel<bool> result = new MSGReturnModel<bool>();
            if (Cache.IsSet(CacheList.DepositData_N))
            {
                var dbData = (List<Deposit_M>)Cache.Get(CacheList.DepositData_N);
                var data = dbData.FirstOrDefault(x => x.vItem_Id == ItemId);
                if (data != null)
                    data.GetMsg = msg;
                Cache.Invalidate(CacheList.DepositData_N);
                Cache.Set(CacheList.DepositData_N, dbData);
                result.RETURN_FLAG = true;
                result.Datas = true;
                result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
            }
            return Json(result);
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
        /// 檢查異動明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Boolean CheckCDC_Detail(CDCDeposit_D model)
        {
            Boolean isDetail = true;

            List<CDCDeposit_D> CheckDetailList = new List<CDCDeposit_D>();

            #region 取得當次申請的資料
            //取出對應總項資料
            var MasterData = ((List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM)).FirstOrDefault(x => x.vItemId == model.vItemId);
            //取出相同交易對象物品編號清單
            var Trad_Partners_List = ((List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM)).Where(x => x.vTrad_Partners == MasterData.vTrad_Partners).Select(x => x.vItemId).ToList();

            //取出符合交易對象的明細清單
            CheckDetailList.AddRange(((List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD_All)).Where(x => Trad_Partners_List.Contains(x.vItemId)));
            #endregion

            #region 取得庫存DB資料
            List<CDCDeposit_D> DbDetailList = Deposit.GetCDC_DataByTradPartners(MasterData.vTrad_Partners, model.vItemId, model.vData_Seq);

            //將DB清單加入檢查清單
            CheckDetailList.AddRange(DbDetailList);
            #endregion

            #region 檢查存單號碼
            foreach (var item in CheckDetailList)
            {
                //非本筆資料
                if (item.vItemId != model.vItemId || item.vData_Seq != model.vData_Seq)
                {
                    var newDep_No_Preamble = string.IsNullOrEmpty(item.vDep_No_Preamble_Aft) ? item.vDep_No_Preamble : item.vDep_No_Preamble_Aft;
                    var newDep_No_Tail = string.IsNullOrEmpty(item.vDep_No_Tail_Aft) ? item.vDep_No_Tail : item.vDep_No_Tail_Aft;
                    var newDep_No_B = string.IsNullOrEmpty(item.vDep_No_B_Aft) ? item.vDep_No_B : item.vDep_No_B_Aft;
                    var newDep_No_E = string.IsNullOrEmpty(item.vDep_No_E_Aft) ? item.vDep_No_E : item.vDep_No_E_Aft;
                    
                    //判斷存單號碼前置碼及存單號碼尾碼
                    if (newDep_No_Preamble == model.vDep_No_Preamble && newDep_No_Tail == model.vDep_No_Tail)
                    {
                        #region 檢查起迄區間
                        if (int.Parse(newDep_No_B) >= int.Parse(model.vDep_No_B))
                        {
                            if (int.Parse(newDep_No_B) <= int.Parse(model.vDep_No_E))
                            {
                                isDetail = false;
                            }
                        }
                        else
                        {
                            if (int.Parse(newDep_No_E) >= int.Parse(model.vDep_No_B))
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

        /// <summary>
        /// 設定異動總面額
        /// <param name="vItem_Id"></param>
        /// </summary>
        /// <returns></returns>
        private void SetCDC_TotalDenomination(string vItem_Id)
        {
            //計算出總面額
            var DetailDataList = (List<CDCDeposit_D>)Cache.Get(CacheList.CDCDepositDataD);

            Decimal? TotalDenomination = 0, NewTotalDenomination = 0;

            foreach(var item in DetailDataList)
            {
                if(item.vSubtotal_Denomination_Aft==null)
                {
                    NewTotalDenomination += item.vSubtotal_Denomination;
                }
                else
                {
                    NewTotalDenomination += item.vSubtotal_Denomination_Aft;
                }

                TotalDenomination += item.vSubtotal_Denomination;
            }

            NewTotalDenomination = (NewTotalDenomination == TotalDenomination) ? null : NewTotalDenomination;

            //修改對應總項資料的總面額
            var MasterDataList = (List<CDCDeposit_M>)Cache.Get(CacheList.CDCDepositDataM);
            var updateTempData = MasterDataList.FirstOrDefault(x => x.vItemId == vItem_Id);
            updateTempData.vTotal_Denomination_Aft = NewTotalDenomination;
            Cache.Invalidate(CacheList.CDCDepositDataM);
            Cache.Set(CacheList.CDCDepositDataM, MasterDataList);
        }
    }
}