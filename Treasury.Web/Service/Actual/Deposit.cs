﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using System.ComponentModel;

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
namespace Treasury.Web.Service.Actual
{
    public class Deposit : Common, IDeposit
    {
        public Deposit()
        {

        }

        #region GetData
        /// <summary>
        /// 計息方式
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetInterest_Rate_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "INTEREST_RATE_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 存單類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetDep_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DEP_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 是否標籤
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetYN_Flag()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "YN_FLAG")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 使用 交易對象 抓取在庫定期單明細資料
        /// </summary>
        /// <param name="vTrad_Partners">交易對象</param>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        public List<Deposit_D> GetDataByTradPartners(string vTrad_Partners, string vAplyNo = null)
        {
            var result = new List<Deposit_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                //庫存狀態
                List<string> Status = new List<string> { "1", "3", "4", "8" };

                //取得符合交易對象的物品編號
                var ItemIdList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => x.TRAD_PARTNERS == vTrad_Partners)
                    .Where(x=>Status.Contains(x.INVENTORY_STATUS))
                    .Select(x => x.ITEM_ID).ToList();

                //有申請單時排除申請單號對應物品編號
                if(!vAplyNo.IsNullOrWhiteSpace())
                {
                    List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                    ItemIdList.RemoveAll(x => oldItemIds.Contains(x));
                }

                result.AddRange(GetDetailModel(db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x=>ItemIdList.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    ));
            }

            return result;
        }

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <param name="vTransExpiryDateFrom">定存單到期日(起)</param>
        /// <param name="vTransExpiryDateTo">定存單到期日(迄)</param>
        /// <returns></returns>
        public DepositViewModel GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null, string vTransExpiryDateFrom = null, string vTransExpiryDateTo = null)
        {
            var result = new DepositViewModel();
            List<Deposit_M> MasterDataList = new List<Deposit_M>();
            List<Deposit_D> DetailDataList = new List<Deposit_D>();
            DateTime DateFrom, DateTo;

            DateTime.TryParse(vTransExpiryDateFrom, out DateFrom);
            DateTime.TryParse(vTransExpiryDateTo, out DateTo);

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();

                #region 取得總項資料
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    MasterDataList = GetMasterModel(
                        db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                        .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                        .Where(x => x.TRANS_EXPIRY_DATE >= DateFrom, !vTransExpiryDateFrom.IsNullOrWhiteSpace())
                        .Where(x => x.TRANS_EXPIRY_DATE <= DateTo, !vTransExpiryDateTo.IsNullOrWhiteSpace())
                        .Where(x => x.INVENTORY_STATUS == "1") //庫存
                        .AsEnumerable()
                        , false, _Inventory_types).ToList();
                }
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList();

                    MasterDataList.AddRange(
                        GetMasterModel(
                            db.ITEM_DEP_ORDER_M.AsNoTracking()
                            .Where(x => itemIds.Contains(x.ITEM_ID))
                            .AsEnumerable()
                            , true, _Inventory_types).ToList()
                        );
                }

                result.vDeposit_M = MasterDataList;
                #endregion

                #region 取得明細資料
                var MasterItemIdList = MasterDataList.Select(x => x.vItem_Id).ToList();

                DetailDataList = GetDetailModel(
                    db.ITEM_DEP_ORDER_D.AsNoTracking()
                    .Where(x => MasterItemIdList.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    ).ToList();

                result.vDeposit_D = DetailDataList;
                #endregion
            }

            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        public DepositViewModel GetDataByAplyNo(string vAplyNo)
        {
            var result = new DepositViewModel();
            List<Deposit_M> MasterDataList = new List<Deposit_M>();
            List<Deposit_D> DetailDataList = new List<Deposit_D>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    //使用物品編號去定期存單庫存資料明細檔抓取資料
                    var _IDOD_DataList = db.ITEM_DEP_ORDER_D.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    if (_IDOM_DataList.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        int vRowNum = 1;
                        foreach (var MasterData in _IDOM_DataList)
                        {
                            MasterDataList.AddRange(
                                GetMasterModel(
                                _IDOM_DataList.Where(x => x.ITEM_ID == MasterData.ITEM_ID).ToList()
                                , true, _Inventory_types, vRowNum.ToString()).ToList()
                                );

                            DetailDataList.AddRange(
                                GetDetailModel(
                                _IDOD_DataList.Where(x => x.ITEM_ID == MasterData.ITEM_ID).ToList()
                                , vRowNum.ToString()).ToList()
                                );

                            vRowNum++;
                        }

                        result.vDeposit_M = MasterDataList;
                        result.vDeposit_D = DetailDataList;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 依申請單號取得列印群組筆數
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        public List<DepositReportGroupData> GetReportGroupData(string vAplyNo)
        {
            List<DepositReportGroupData> result = new List<DepositReportGroupData>();
            DepositReportGroupData GroupData = new DepositReportGroupData();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);

                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去定期存單庫存資料檔抓取資料
                    var _IDOM_DataList = db.ITEM_DEP_ORDER_M.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                    //台幣
                    var GroupDataList_TWD = _IDOM_DataList.Where(x => x.CURRENCY == "TWD").GroupBy(x => new { x.DEP_TYPE });

                    foreach(var item in GroupDataList_TWD)
                    {
                        GroupData = new DepositReportGroupData { isTWD = "Y", vDep_Type = item.Key.DEP_TYPE };

                        result.Add(GroupData);
                    }

                    //外幣
                    var GroupDataList_NTWD = _IDOM_DataList.Where(x => x.CURRENCY != "TWD").GroupBy(x => new { x.DEP_TYPE });

                    foreach (var item in GroupDataList_NTWD)
                    {
                        GroupData = new DepositReportGroupData { isTWD = "N", vDep_Type = item.Key.DEP_TYPE };

                        result.Add(GroupData);
                    }
                }
            }

            return result;
        }
        #endregion

        #region SaveData
        /// <summary>
        /// 申請覆核動作
        /// </summary>
        /// <param name="insertDatas"></param>
        /// <param name="taData"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITreaItem>> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (insertDatas != null)
                {
                    var datas = (List<DepositViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();

                            string logStr = string.Empty; //log
                            var item_Seq = "E6"; //定期存單流水號開頭編碼    
                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (taData.vAplyNo.IsNullOrWhiteSpace()) //新增申請單
                            {
                                String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                                var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion

                                #region 儲存資料
                                var _first = datas.First();
                                var MasterDataList = _first.vDeposit_M;
                                foreach (var item in MasterDataList)
                                {
                                    var _IDOM_Item_Id = string.Empty;
                                    var _IDOM = new ITEM_DEP_ORDER_M();
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            _IDOM = new ITEM_DEP_ORDER_M()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}",  //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                CURRENCY = item.vCurrency,  //幣別
                                                TRAD_PARTNERS = item.vTrad_Partners,    //交易對象
                                                COMMIT_DATE = DateTime.Parse(item.vCommit_Date),    //承作日期
                                                EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date),    //到期日
                                                INTEREST_RATE_TYPE = item.vInterest_Rate_Type,  //計息方式
                                                INTEREST_RATE = item.vInterest_Rate,    //利率%
                                                DEP_TYPE = item.vDep_Type,  //存單類型
                                                TOTAL_DENOMINATION = item.vTotal_Denomination,  //總面額
                                                DEP_SET_QUALITY = item.vDep_Set_Quality,    //設質否(N/Y)
                                                AUTO_TRANS = item.vAuto_Trans,  //自動轉期(N/Y)
                                                TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date),    //轉期後到期日
                                                TRANS_TMS = item.vTrans_Tms,    //轉期次數
                                                MEMO = item.vMemo,  //備註
                                                APLY_DEPT = _dept.Item1,    //申請人部門
                                                APLY_SECT = _dept.Item2,    //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1,  //權責部門
                                                CHARGE_SECT = _dept.Item2,  //權責科別
                                                LAST_UPDATE_DT = dt //最後修改時間
                                            };
                                            _IDOM_Item_Id = _IDOM.ITEM_ID;
                                            db.ITEM_DEP_ORDER_M.Add(_IDOM);
                                            logStr += "|";
                                            logStr += _IDOM.modelToString();
                                            #endregion

                                            #region 定期存單庫存資料明細檔
                                            var DetailDataList = _first.vDeposit_D.Where(x => x.vItem_Id == item.vItem_Id).ToList();
                                            int seq = 1;
                                            foreach (var subItem in DetailDataList)
                                            {
                                                var _IDOD = new ITEM_DEP_ORDER_D();
                                                _IDOD = new ITEM_DEP_ORDER_D()
                                                {
                                                    ITEM_ID = _IDOM_Item_Id,    //物品編號
                                                    DATA_SEQ = seq, //明細流水號
                                                    DEP_NO_PREAMBLE = subItem.vDep_No_Preamble, //存單號碼前置碼
                                                    DEP_NO_B = subItem.vDep_No_B,   //存單號碼(起)
                                                    DEP_NO_E = subItem.vDep_No_E,   //存單號碼(迄)
                                                    DEP_NO_TAIL = subItem.vDep_No_Tail, //存單號碼尾碼
                                                    DEP_CNT = subItem.vDep_Cnt, //存單張數
                                                    DENOMINATION = subItem.vDenomination,   //面額
                                                    SUBTOTAL_DENOMINATION = subItem.vSubtotal_Denomination  //面額小計
                                                };
                                                db.ITEM_DEP_ORDER_D.Add(_IDOD);
                                                logStr += "|";
                                                logStr += _IDOD.modelToString();
                                                seq++;
                                            }
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        //只抓取預約取出及預約取出，計庫存
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription() || item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                            _IDOM_Item_Id = _IDOM.ITEM_ID;
                                            if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }

                                            if(item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                            {
                                                _IDOM.INVENTORY_STATUS = "4"; //預約取出
                                            }
                                            else if(item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                            {
                                                _IDOM.INVENTORY_STATUS = "5"; //預約取出，計庫存
                                            }
                                            _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                            #endregion
                                        }
                                    }

                                    #region 其它存取項目申請資料檔
                                    if (!_IDOM_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IDOM_Item_Id
                                        });
                                    }
                                    #endregion

                                }
                                #endregion
                            }
                            else //修改申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                    _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString();
                                #endregion

                                #region 申請單歷程檔
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _TAR.APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
                                #endregion

                                #region 儲存資料
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_DEP_ORDER_M> inserts_Master = new List<ITEM_DEP_ORDER_M>(); //新增總項資料
                                List<ITEM_DEP_ORDER_D> inserts_Detail = new List<ITEM_DEP_ORDER_D>(); //新增明細資料

                                var _first = datas.First();
                                var MasterDataList = _first.vDeposit_M;

                                foreach (var item in MasterDataList)
                                {
                                    var _IDOM_Item_Id = string.Empty;
                                    var _IDOM = new ITEM_DEP_ORDER_M();
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            #region 定期存單庫存資料檔
                                            if (item.vItem_Id.StartsWith(item_Seq))  //明細修改
                                            {
                                                _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                                _IDOM_Item_Id = _IDOM.ITEM_ID;
                                                if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }

                                                _IDOM.CURRENCY = item.vCurrency;   //幣別
                                                _IDOM.TRAD_PARTNERS = item.vTrad_Partners;   //交易對象
                                                _IDOM.COMMIT_DATE = DateTime.Parse(item.vCommit_Date); //承作日期
                                                _IDOM.EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date); //到期日
                                                _IDOM.INTEREST_RATE_TYPE = item.vInterest_Rate_Type;   //計息方式
                                                _IDOM.INTEREST_RATE = item.vInterest_Rate;   //利率%
                                                _IDOM.DEP_TYPE = item.vDep_Type;    //存單類型
                                                _IDOM.TOTAL_DENOMINATION = item.vTotal_Denomination;    //總面額
                                                _IDOM.DEP_SET_QUALITY = item.vDep_Set_Quality;  //設質否
                                                _IDOM.AUTO_TRANS = item.vAuto_Trans;    //自動轉期
                                                _IDOM.TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date);  //轉期後到期日
                                                _IDOM.TRANS_TMS = item.vTrans_Tms;  //轉期次數
                                                _IDOM.MEMO = item.vMemo; //備註
                                                _IDOM.LAST_UPDATE_DT = dt;   //最後修改時間

                                                updateItemIds.Add(item.vItem_Id);

                                                logStr += "|";
                                                logStr += _IDOM.modelToString();
                                            }
                                            else //明細新增
                                            {
                                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                                _IDOM = new ITEM_DEP_ORDER_M()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}",  //歸檔編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    CURRENCY = item.vCurrency,  //幣別
                                                    TRAD_PARTNERS = item.vTrad_Partners,    //交易對象
                                                    COMMIT_DATE = DateTime.Parse(item.vCommit_Date),    //承作日期
                                                    EXPIRY_DATE = DateTime.Parse(item.vExpiry_Date),    //到期日
                                                    INTEREST_RATE_TYPE = item.vInterest_Rate_Type,  //計息方式
                                                    INTEREST_RATE = item.vInterest_Rate,    //利率%
                                                    DEP_TYPE = item.vDep_Type,  //存單類型
                                                    TOTAL_DENOMINATION = item.vTotal_Denomination,  //總面額
                                                    DEP_SET_QUALITY = item.vDep_Set_Quality,    //設質否(N/Y)
                                                    AUTO_TRANS = item.vAuto_Trans,  //自動轉期(N/Y)
                                                    TRANS_EXPIRY_DATE = string.IsNullOrEmpty(item.vTrans_Expiry_Date) ? DateTime.Parse(item.vExpiry_Date) : DateTime.Parse(item.vTrans_Expiry_Date),    //轉期後到期日
                                                    TRANS_TMS = item.vTrans_Tms,    //轉期次數
                                                    MEMO = item.vMemo,  //備註
                                                    APLY_DEPT = _dept.Item1,    //申請人部門
                                                    APLY_SECT = _dept.Item2,    //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1,  //權責部門
                                                    CHARGE_SECT = _dept.Item2,  //權責科別
                                                    LAST_UPDATE_DT = dt //最後修改時間
                                                };
                                                _IDOM_Item_Id = _IDOM.ITEM_ID;
                                                inserts_Master.Add(_IDOM);
                                                logStr += "|";
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                            #endregion

                                            #region 定期存單庫存資料明細檔
                                            //先刪除定期存單庫存資料明細檔
                                            db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => x.ITEM_ID == item.vItem_Id));

                                            //依輸入資料新增定期存單庫存資料明細檔
                                            var DetailDataList = _first.vDeposit_D.Where(x => x.vItem_Id == item.vItem_Id).ToList();
                                            int seq = 1;
                                            foreach (var subItem in DetailDataList)
                                            {
                                                var _IDOD = new ITEM_DEP_ORDER_D();
                                                _IDOD = new ITEM_DEP_ORDER_D()
                                                {
                                                    ITEM_ID = _IDOM_Item_Id,    //物品編號
                                                    DATA_SEQ = seq, //明細流水號
                                                    DEP_NO_PREAMBLE = subItem.vDep_No_Preamble, //存單號碼前置碼
                                                    DEP_NO_B = subItem.vDep_No_B,   //存單號碼(起)
                                                    DEP_NO_E = subItem.vDep_No_E,   //存單號碼(迄)
                                                    DEP_NO_TAIL = subItem.vDep_No_Tail, //存單號碼尾碼
                                                    DEP_CNT = subItem.vDep_Cnt, //存單張數
                                                    DENOMINATION = subItem.vDenomination,   //面額
                                                    SUBTOTAL_DENOMINATION = subItem.vSubtotal_Denomination  //面額小計
                                                };
                                                inserts_Detail.Add(_IDOD);
                                                logStr += "|";
                                                logStr += _IDOD.modelToString();
                                                seq++;
                                            }
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                        if (_IDOM.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vTakeoutFlag)
                                        {
                                            if (_IDOM.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                //判斷狀態
                                                if(item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                                {
                                                    _IDOM.INVENTORY_STATUS = "4"; //預約取出
                                                }
                                                else if (item.vStatus == Ref.AccessInventoryType._5.GetDescription())
                                                {
                                                    _IDOM.INVENTORY_STATUS = "5"; //預約取出，計庫存
                                                }
                                                _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IDOM.ITEM_ID);
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                            else if (_IDOM.INVENTORY_STATUS == "4"|| _IDOM.INVENTORY_STATUS == "5") //原先為預約取出或預約取出，計庫存
                                            {
                                                updateItemIds.Add(_IDOM.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IDOM.INVENTORY_STATUS == "4" || _IDOM.INVENTORY_STATUS == "5") //原先為在庫
                                            {
                                                _IDOM.INVENTORY_STATUS = "1"; //預約取出
                                                _IDOM.LAST_UPDATE_DT = dt;  //最後修改時間
                                                logStr += _IDOM.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_ORDER_M.RemoveRange(db.ITEM_DEP_ORDER_M.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts_Master.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_DEP_ORDER_M.AddRange(inserts_Master);
                                    db.ITEM_DEP_ORDER_D.AddRange(inserts_Detail);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                {

                                }
                                #endregion
                            }

                            #region Save Db
                            var validateMessage = db.GetValidationErrors().getValidateString();
                            if (validateMessage.Any())
                            {
                                result.DESCRIPTION = validateMessage;
                            }
                            else
                            {
                                try
                                {
                                    db.SaveChanges();

                                    #region LOG
                                    //新增LOG
                                    Log log = new Log();
                                    log.CFUNCTION = "申請覆核-新增定期存單";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, taData.vCreateUid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}");
                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                            #endregion

                        }
                    }
                    else
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }

        /// <summary>
        /// 刪除申請
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CancelApply(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            return Process(db, aply_No, logStr, dt, access_Type, true);
        }

        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="access_Type">取出或存入</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        public Tuple<bool, string> ObSolete(TreasuryDBEntities db, string aply_No, string access_Type, string logStr, DateTime dt)
        {
            return Process(db, aply_No, logStr, dt, access_Type, false);
        }
        #endregion

        #region privation function
        /// <summary>
        /// 申請刪除 & 作廢 定期存單資料庫要處理的事件
        /// </summary>
        /// <param name="db"></param>
        /// <param name="aply_No"></param>
        /// <param name="logStr"></param>
        /// <param name="dt"></param>
        /// <param name="accessType"></param>
        /// <param name="deleFlag"></param>
        /// <returns></returns>
        public Tuple<bool, string> Process(TreasuryDBEntities db, string aply_No, string logStr, DateTime dt, string accessType, bool deleFlag)
        {
            var _changeFlag = false;

            var _TAR = db.TREA_APLY_REC.AsNoTracking()
            .FirstOrDefault(x => x.APLY_NO == aply_No);

            if (_TAR != null)
            {
                //使用單號去其他存取項目檔抓取物品編號
                var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                //使用物品編號去定期存單庫存資料檔抓取資料
                var details = db.ITEM_DEP_ORDER_M.AsNoTracking()
                    .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                if (details.Any())
                {
                    if (accessType == Ref.AccessProjectTradeType.G.ToString()) //取出狀態處理作業
                    {
                        foreach (var item in details)
                        {
                            var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                            _IDOM.INVENTORY_STATUS = "1"; //返回在庫
                            _IDOM.LAST_UPDATE_DT = dt;
                            logStr += _IDOM.modelToString(logStr);
                        }

                        //刪除其他存取項目檔
                        if (deleFlag)
                            db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));

                    }
                    else if (accessType == Ref.AccessProjectTradeType.P.ToString())    //存入狀態處理作業
                    {
                        //判斷申請刪除 & 作廢
                        if (deleFlag)
                        {
                            db.ITEM_DEP_ORDER_M.RemoveRange(db.ITEM_DEP_ORDER_M.Where(x => OIAs.Contains(x.ITEM_ID)));
                            db.ITEM_DEP_ORDER_D.RemoveRange(db.ITEM_DEP_ORDER_D.Where(x => OIAs.Contains(x.ITEM_ID)));
                            db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));
                        }
                        else
                        {
                            foreach (var item in details)
                            {
                                var _IDOM = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                                _IDOM.INVENTORY_STATUS = "7"; //已取消
                                _IDOM.LAST_UPDATE_DT = dt;
                                logStr += _IDOM.modelToString(logStr);
                            }
                        }
                    }
                }
                else
                {
                    _changeFlag = true;
                }
            }
            else
            {
                _changeFlag = true;
            }

            if (_changeFlag)
            {
                return new Tuple<bool, string>(false, logStr);
            }
            else
            {
                return new Tuple<bool, string>(true, logStr);
            }
        }

        /// <summary>
        /// 定期存單資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vTakeoutFlag"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<Deposit_M> GetMasterModel(IEnumerable<ITEM_DEP_ORDER_M> data,Boolean vTakeoutFlag, List<SYS_CODE> _Inventory_types, string vRowNum = null)
        {
            return data.Select(x => new Deposit_M()
            {
                vRowNum = vRowNum,  //編號
                vItem_Id = x.ITEM_ID,   //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,   //代碼.庫存狀態 
                vCurrency = x.CURRENCY, //幣別
                vTrad_Partners = x.TRAD_PARTNERS,   //交易對象
                vCommit_Date = x.COMMIT_DATE.ToString("yyyy/MM/dd"),    //承作日期
                vExpiry_Date = x.EXPIRY_DATE.ToString("yyyy/MM/dd"),    //到期日
                vInterest_Rate_Type = x.INTEREST_RATE_TYPE, //計息方式
                vInterest_Rate = x.INTEREST_RATE,   //利率%
                vDep_Type = x.DEP_TYPE, //存單類型
                vTotal_Denomination = x.TOTAL_DENOMINATION, //總面額
                vDep_Set_Quality = x.DEP_SET_QUALITY,   //設質否
                vAuto_Trans = x.AUTO_TRANS, //自動轉期
                vTrans_Expiry_Date = DateTime.Parse(x.TRANS_EXPIRY_DATE.ToString()).ToString("yyyy/MM/dd"), //轉期後到期日
                vTrans_Tms = x.TRANS_TMS,   //轉期次數
                vMemo = x.MEMO, //備註
                vTakeoutFlag = vTakeoutFlag,    //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT    //最後修改時間
            });
        }

        /// <summary>
        /// 在庫定期存單明細資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vRowNum"></param>
        /// <returns></returns>
        private IEnumerable<Deposit_D> GetDetailModel(IEnumerable<ITEM_DEP_ORDER_D> data, string vRowNum = null)
        {
            return data.Select(x => new Deposit_D()
            {
                vRowNum = vRowNum,  //編號
                vItem_Id = x.ITEM_ID,   //物品編號
                vData_Seq = x.DATA_SEQ.ToString(),  //明細流水號
                vDep_No_Preamble = x.DEP_NO_PREAMBLE,   //存單號碼前置碼
                vDep_No_B = x.DEP_NO_B, //存單號碼(起)
                vDep_No_E = x.DEP_NO_E, //存單號碼(迄)
                vDep_No_Tail = x.DEP_NO_TAIL,   //存單號碼尾碼
                vDep_Cnt = x.DEP_CNT,   //存單張數
                vDenomination = x.DENOMINATION, //單張面額
                vSubtotal_Denomination = x.SUBTOTAL_DENOMINATION    //面額小計
            });
        }

        #endregion
    }
}