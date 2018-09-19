using System;
using System.Collections.Generic;
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
using System.Data.Entity.Infrastructure;
/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存入保證金
/// 初版作者：20180810 王菁萱
/// 修改歷程：20180810 王菁萱 
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
    public class Marginp : Common, IMarginp
    {
        public Marginp()
        {

        }

        #region Get Date

        /// <summary>
        /// 抓取 存入保證金類別 項目
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMarginp_Take_Of_Type()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "MARGIN_TAKE_OF_TYPE")
                    .Select(x => new SelectOption()
                    {
                        Value = x.CODE,
                        Text = x.CODE_VALUE
                    }));
            }
            return result;
        }

        /// <summary>
        /// 抓取 存入保證金物品名稱 項目
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMarginpItem()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "MARGIN_ITEM")
                .Select(x => new SelectOption() {
                    Value = x.CODE,
                    Text = x.CODE_VALUE
                }));     
            }
            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<MarginpViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<MarginpViewModel>();
            //result.
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用歸檔編號去存入保證金庫存資料檔抓取資料
                    var details = db.ITEM_DEP_RECEIVED.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        result = GetDetailModel(details, _Inventory_types).ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        public List<MarginpViewModel> GetDbDataByUnit(string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<MarginpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_DEP_RECEIVED.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x =>
                    new MarginpViewModel() {
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                        vMarginp_Trad_Partners = x.TRAD_PARTNERS,
                        vItemId = x.ITEM_ID,
                        vMarginp_Amount = TypeTransfer.decimalNToString(x.AMOUNT),
                        vMarginp_Item = x.MARGIN_ITEM,
                        vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                        vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,
                        //vMarginp_Effective_Date_B = x.EFFECTIVE_DATE_B == null ? null : x.EFFECTIVE_DATE_B.Value.DateToTaiwanDate(9,true),
                        vMarginp_Effective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                        //vMarginp_Effective_Date_E = x.EFFECTIVE_DATE_E == null ? null : x.EFFECTIVE_DATE_E.Value.DateToTaiwanDate(9, true),
                        vMarginp_Effective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                        vDescription = x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vMarginp_Book_No = x.BOOK_NO,
                        vtakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                        result.AddRange(db.ITEM_DEP_RECEIVED.AsNoTracking().Where(
                        x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                        .Select(x =>
                         new MarginpViewModel()
                         {
                             vStatus = Ref.AccessInventoryType._4.GetDescription(),
                             vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                             vMarginp_Trad_Partners = x.TRAD_PARTNERS,
                             vItemId = x.ITEM_ID,
                             vMarginp_Amount = TypeTransfer.decimalNToString(x.AMOUNT),
                             vMarginp_Item = x.MARGIN_ITEM,
                             vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                             vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,
                             vMarginp_Effective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                             //vMarginp_Effective_Date_B_2 = x.EFFECTIVE_DATE_B == null ? null : x.EFFECTIVE_DATE_B.Value.DateToTaiwanDate(9, true),
                             vMarginp_Effective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                             //vMarginp_Effective_Date_E_2 = x.EFFECTIVE_DATE_E == null ? null : x.EFFECTIVE_DATE_E.Value.DateToTaiwanDate(9, true),
                             vDescription = x.DESCRIPTION,//說明
                             vMemo = x.MEMO,
                             vMarginp_Book_No = x.BOOK_NO,
                             vtakeoutFlag = true,
                             vLast_Update_Time = x.LAST_UPDATE_DT
                         }));
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢CDC資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="aply_No">資料庫異動申請單紀錄檔  INVENTORY_CHG_APLY 單號</param>
        /// <returns></returns>
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null)
        {
            List<CDCMarginpViewModel> result = new List<CDCMarginpViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var PUT_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_From);
                    var PUT_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_To).DateToLatestTime();
                    var GET_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_From);
                    var GET_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_To).DateToLatestTime();
                    result.AddRange(db.ITEM_DEP_RECEIVED.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_To.Value, GET_DATE_To != null)
                        .AsEnumerable()
                        .Select((x) => new CDCMarginpViewModel()
                        {
                            vItem_Id = x.ITEM_ID,
                            vlItem_Id = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.ToString("yyyy/MM/dd"),
                            vBook_No = x.BOOK_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vCHARGE_DEPT = x.CHARGE_DEPT,
                            vCHARGE_DEPT_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME,
                            vCHARGE_SECT = x.CHARGE_SECT,
                            vCHARGE_SECT_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME,
                            vMargin_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                            vMargin_Take_Of_Type_AFT = x.MARGIN_TAKE_OF_TYPE_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_AFT = x.TRAD_PARTNERS_AFT,
                            vAmount = x.AMOUNT,
                            vAmount_AFT = x.AMOUNT_AFT,
                            vMargin_Item = x.MARGIN_ITEM,
                            vMargin_Item_AFT = x.MARGIN_ITEM_AFT,
                            vMargin_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                            vMargin_Item_Issuer_AFT = x.MARGIN_ITEM_ISSUER_AFT,
                            vPledge_Item_No = x.PLEDGE_ITEM_NO,
                            vPledge_Item_No_AFT = x.PLEDGE_ITEM_NO_AFT,
                            vEffective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                            vEffective_Date_B_AFT = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B_AFT),
                            vEffective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                            vEffective_Date_E_AFT = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E_AFT),
                            vDescription = x.DESCRIPTION,
                            vDescription_AFT = x.DESCRIPTION_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_DEP_RECEIVED.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCMarginpViewModel()
                        {
                            vItem_Id = x.ITEM_ID,
                            vlItem_Id = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.ToString("yyyy/MM/dd"),
                            vBook_No = x.BOOK_NO,
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,
                            vCHARGE_DEPT = x.CHARGE_DEPT,
                            vCHARGE_DEPT_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME,
                            vCHARGE_SECT = x.CHARGE_SECT,
                            vCHARGE_SECT_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME,
                            vMargin_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,
                            vMargin_Take_Of_Type_AFT = x.MARGIN_TAKE_OF_TYPE_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_AFT = x.TRAD_PARTNERS_AFT,
                            vAmount = x.AMOUNT,
                            vAmount_AFT = x.AMOUNT_AFT,
                            vMargin_Item = x.MARGIN_ITEM,
                            vMargin_Item_AFT = x.MARGIN_ITEM_AFT,
                            vMargin_Item_Issuer = x.MARGIN_ITEM_ISSUER,
                            vMargin_Item_Issuer_AFT = x.MARGIN_ITEM_ISSUER_AFT,
                            vPledge_Item_No = x.PLEDGE_ITEM_NO,
                            vPledge_Item_No_AFT = x.PLEDGE_ITEM_NO_AFT,
                            vEffective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),
                            vEffective_Date_B_AFT = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B_AFT),
                            vEffective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),
                            vEffective_Date_E_AFT = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E_AFT),
                            vDescription = x.DESCRIPTION,
                            vDescription_AFT = x.DESCRIPTION_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                result.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCHARGE_SECT_Name.IsNullOrWhiteSpace() ? x.vCHARGE_SECT_Name : x.vCHARGE_DEPT_Name;
                });
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 申請覆核 存入保證金
        /// </summary>
        /// <param name="insertDatas">資料</param>
        /// <param name="taData">申請單資料</param>
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
                    var datas = (List<MarginpViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            //var item_Seq = "X"; //存入保證金流水號開頭編碼
                            //String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                           
                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請

                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                    _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例72
                                _TAR.APLY_STATUS = _APLY_STATUS;
                                _TAR.LAST_UPDATE_DT = dt;

                                logStr += _TAR.modelToString(logStr);
                                #endregion

                                #region 申請單歷程檔
                                var _ARH = new APLY_REC_HIS()
                                {
                                    APLY_NO = taData.vAplyNo,
                                    APLY_STATUS = _APLY_STATUS,
                                    PROC_DT = dt,
                                    PROC_UID = taData.vCreateUid
                                };
                                logStr += _ARH.modelToString(logStr);
                                db.APLY_REC_HIS.Add(_ARH);
                                #endregion

                                #region 存入保證金庫存資料檔

                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId 
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<string> cancelItemIds = new List<string>(); //取消 itemId
                                List<ITEM_DEP_RECEIVED> inserts = new List<ITEM_DEP_RECEIVED>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {

                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _IDR = new ITEM_DEP_RECEIVED();
                                            //依類別取得對應歸檔編號                                     
                                            if (item.vItemId.StartsWith("X") || item.vItemId.StartsWith("Y") || item.vItemId.StartsWith("Z")) //舊有資料
                                            {
                                                _IDR = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IDR.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IDR.MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type; //類別
                                                _IDR.TRAD_PARTNERS = item.vMarginp_Trad_Partners; //交易對象
                                                _IDR.AMOUNT = TypeTransfer.stringToDecimal(item.vMarginp_Amount); //金額
                                                _IDR.MARGIN_ITEM = item.vMarginp_Item; //物品名稱 
                                                _IDR.MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer;//物品發行人
                                                _IDR.PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No;//質押標的號碼
                                                _IDR.EFFECTIVE_DATE_B = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_B);//有效期間(起)
                                                _IDR.EFFECTIVE_DATE_E = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_E);//有效期間(迄)
                                                _IDR.DESCRIPTION = item.vDescription;//說明
                                                _IDR.MEMO = item.vMemo; //備註
                                                _IDR.BOOK_NO = item.vMarginp_Book_No;//冊號
                                                updateItemIds.Add(item.vItemId);
                                                logStr += _IDR.modelToString(logStr);
                                            }
                                            else
                                            {
                                                string item_id = string.Empty;
                                                switch (item.vMarginp_Take_Of_Type)
                                                {
                                                    case "1":
                                                        item_id = sysSeqDao.qrySeqNo("X", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"X{item_id}";
                                                        break;
                                                    case "2":
                                                        item_id = sysSeqDao.qrySeqNo("Y", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"Y{item_id}";
                                                        break;
                                                    case "3":
                                                        item_id = sysSeqDao.qrySeqNo("Z", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"Z{item_id}";
                                                        break;
                                                    default:
                                                        result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                                                        return result;
                                                }

                                                _IDR = new ITEM_DEP_RECEIVED()
                                                {
                                                    ITEM_ID = $@"{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type, //類別
                                                    TRAD_PARTNERS = item.vMarginp_Trad_Partners, //交易對象
                                                    AMOUNT = TypeTransfer.stringToDecimal(item.vMarginp_Amount), //金額
                                                    MARGIN_ITEM = item.vMarginp_Item, //物品名稱 
                                                    MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer,//物品發行人
                                                    PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No,//質押標的號碼
                                                    EFFECTIVE_DATE_B = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_B),//有效期間(起)
                                                    EFFECTIVE_DATE_E = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_E),//有效期間(迄)
                                                    DESCRIPTION = item.vDescription,//說明
                                                    MEMO = item.vMemo, //備註
                                                    BOOK_NO = item.vMarginp_Book_No,//冊號
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                    //PUT_DATE = dt, //存入日期時間
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };
                                                inserts.Add(_IDR);
                                                logStr += _IDR.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        var _IDR = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_IDR.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vtakeoutFlag)
                                        {
                                            if (_IDR.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _IDR.INVENTORY_STATUS = "4"; //預約取出
                                                _IDR.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IDR.ITEM_ID);
                                            }
                                            else if (_IDR.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_IDR.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IDR.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _IDR.INVENTORY_STATUS = "1"; //預約取出
                                                _IDR.LAST_UPDATE_DT = dt;  //最後修改時間
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_DEP_RECEIVED.RemoveRange(db.ITEM_DEP_RECEIVED.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_DEP_RECEIVED.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                {
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x
                                    }));
                                }
                                #endregion
                            }
                            else
                            {
                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion

                                #region 存入保證金庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id =  string.Empty;
                                            //var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                                                                        //依類別取得對應歸檔編號
                                            switch(item.vMarginp_Take_Of_Type)
                                            {
                                                   case "1" :
                                                    item_id = sysSeqDao.qrySeqNo("X", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"X{item_id}";
                                                    break;
                                                     case "2" :
                                                    item_id = sysSeqDao.qrySeqNo("Y", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"Y{item_id}";
                                                    break;
                                                     case "3" :
                                                    item_id = sysSeqDao.qrySeqNo("Z", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"Z{item_id}";
                                                    break;
                                                    default:
                                                    result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                                                    return result;
                                            }          


                                            var _IC = new ITEM_DEP_RECEIVED()
                                            {
                                                ITEM_ID = $@"{item_id}", //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                MARGIN_TAKE_OF_TYPE = item.vMarginp_Take_Of_Type, //類別
                                                TRAD_PARTNERS = item.vMarginp_Trad_Partners, //交易對象
                                                AMOUNT = TypeTransfer.stringToDecimal(item.vMarginp_Amount), //金額
                                                MARGIN_ITEM = item.vMarginp_Item, // 物品名稱
                                                MARGIN_ITEM_ISSUER = item.vMarginp_Item_Issuer,//物品發行人
                                                PLEDGE_ITEM_NO = item.vMarginp_Pledge_Item_No,//質押標的號碼
                                                EFFECTIVE_DATE_B = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_B),//有效期間(起)
                                                EFFECTIVE_DATE_E = TypeTransfer.stringToDateTimeN(item.vMarginp_Effective_Date_E),//有效期間(迄)
                                                DESCRIPTION=item.vDescription,//說明
                                                MEMO = item.vMemo, //備註
                                                BOOK_NO = item.vMarginp_Book_No,//冊號
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                                           //PUT_DATE = dt, //存入日期時間
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            db.ITEM_DEP_RECEIVED.Add(_IC);
                                            logStr += _IC.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            var _IC = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IC_Item_Id = _IC.ITEM_ID;
                                            if (_IC.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IC.INVENTORY_STATUS = "4"; //預約取出
                                                                        //_IRE.GET_DATE = dt; //取出日期時間
                                            _IC.LAST_UPDATE_DT = dt;  //最後修改時間
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    if (!_IC_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IC_Item_Id
                                        });
                                    }
                                    #endregion
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
                                        log.CFUNCTION = "申請覆核-新增存入保證金";
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
            var itemIds = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態存入保證金庫存資料檔要復原
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "7"; //改為已取消
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
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
            var otherItemAplys = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == aply_No).ToList();
            var itemIds = otherItemAplys.Select(y => y.ITEM_ID).ToList();
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態存入保證金庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除存入保證金庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_DEP_RECEIVED.RemoveRange(db.ITEM_DEP_RECEIVED.Where(x => itemIds.Contains(x.ITEM_ID)));
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
        }

        /// <summary>
        /// 庫存異動資料-申請覆核
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ICDCItem>> CDCApplyAudit(IEnumerable<ICDCItem> saveData, CDCSearchViewModel searchModel)
        {
            MSGReturnModel<IEnumerable<ICDCItem>> result = new MSGReturnModel<IEnumerable<ICDCItem>>();
            result.RETURN_FLAG = false;
            string logStr = string.Empty;
            DateTime dt = DateTime.Now;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                bool changFlag = false;
                var _data = SaveINVENTORY_CHG_APLY(db, searchModel, logStr, dt);
                logStr = _data.Item2;
                foreach (CDCMarginpViewModel model in saveData)
                {
                    var _Marginp = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == model.vItem_Id);
                    if (_Marginp != null && !changFlag)
                    {
                        if (_Marginp.LAST_UPDATE_DT > model.vLast_Update_Time || _Marginp.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }
                        if (!changFlag)
                        {
                            _Marginp.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _Marginp.MARGIN_TAKE_OF_TYPE_AFT = model.vMargin_Take_Of_Type_AFT;
                            _Marginp.TRAD_PARTNERS_AFT = model.vTrad_Partners_AFT;
                            _Marginp.AMOUNT_AFT = model.vAmount_AFT;
                            _Marginp.MARGIN_ITEM_AFT = model.vMargin_Item_AFT;
                            _Marginp.MARGIN_ITEM_ISSUER_AFT = model.vMargin_Item_Issuer_AFT;
                            _Marginp.PLEDGE_ITEM_NO_AFT = model.vPledge_Item_No_AFT;
                            _Marginp.EFFECTIVE_DATE_B_AFT = TypeTransfer.stringToDateTimeN(model.vEffective_Date_B_AFT);
                            _Marginp.EFFECTIVE_DATE_E_AFT = TypeTransfer.stringToDateTimeN(model.vEffective_Date_E_AFT);
                            _Marginp.DESCRIPTION_AFT = model.vDescription_AFT;
                            _Marginp.MEMO_AFT = model.vMemo_AFT;
                            _Marginp.LAST_UPDATE_DT = dt;

                            logStr = _Marginp.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _Marginp.ITEM_ID
                            };
                            logStr = _OIA.modelToString(logStr);
                        }
                    }
                    else
                    {
                        changFlag = true;
                    }
                }
                if (changFlag)
                {
                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                }
                else
                {
                    db.SaveChanges();
                    #region LOG
                    //新增LOG
                    Log log = new Log();
                    log.CFUNCTION = "申請覆核-資料庫異動:存入保證金";
                    log.CACTION = "A";
                    log.CCONTENT = logStr;
                    LogDao.Insert(log, searchModel.vCreate_Uid);
                    #endregion
                    result.RETURN_FLAG = true;
                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"申請單號:{_data.Item1}");
                    result.Datas = GetCDCSearchData(searchModel);
                }
            }
            return result;
        }

        /// <summary>
        /// 庫存異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Marginp = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marginp != null)
                {
                    _Marginp.INVENTORY_STATUS = "1"; //在庫
                    _Marginp.MARGIN_TAKE_OF_TYPE_AFT = null;
                    _Marginp.TRAD_PARTNERS_AFT = null;
                    _Marginp.AMOUNT_AFT = null;
                    _Marginp.MARGIN_ITEM_AFT = null;
                    _Marginp.MARGIN_ITEM_ISSUER_AFT = null;
                    _Marginp.PLEDGE_ITEM_NO_AFT = null;
                    _Marginp.EFFECTIVE_DATE_B_AFT = null;
                    _Marginp.EFFECTIVE_DATE_E_AFT = null;
                    _Marginp.DESCRIPTION_AFT = null;
                    _Marginp.MEMO_AFT = null;
                    _Marginp.LAST_UPDATE_DT = dt;
                    logStr = _Marginp.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _Marginp = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marginp != null)
                {
                    _Marginp.INVENTORY_STATUS = "1"; //在庫
                    _Marginp.MARGIN_TAKE_OF_TYPE = _Marginp.MARGIN_TAKE_OF_TYPE_AFT;
                    _Marginp.MARGIN_TAKE_OF_TYPE_AFT = null;
                    _Marginp.TRAD_PARTNERS = _Marginp.TRAD_PARTNERS_AFT;
                    _Marginp.TRAD_PARTNERS_AFT = null;
                    _Marginp.AMOUNT = _Marginp.AMOUNT_AFT;
                    _Marginp.AMOUNT_AFT = null;
                    _Marginp.MARGIN_ITEM = _Marginp.MARGIN_ITEM_AFT;
                    _Marginp.MARGIN_ITEM_AFT = null;
                    _Marginp.MARGIN_ITEM_ISSUER = _Marginp.MARGIN_ITEM_ISSUER_AFT;
                    _Marginp.MARGIN_ITEM_ISSUER_AFT = null;
                    _Marginp.PLEDGE_ITEM_NO = _Marginp.PLEDGE_ITEM_NO_AFT;
                    _Marginp.PLEDGE_ITEM_NO_AFT = null;
                    _Marginp.EFFECTIVE_DATE_B = _Marginp.EFFECTIVE_DATE_B_AFT;
                    _Marginp.EFFECTIVE_DATE_B_AFT = null;
                    _Marginp.EFFECTIVE_DATE_E = _Marginp.EFFECTIVE_DATE_E_AFT;
                    _Marginp.EFFECTIVE_DATE_E_AFT = null;
                    _Marginp.DESCRIPTION = _Marginp.DESCRIPTION_AFT;
                    _Marginp.DESCRIPTION_AFT = null;
                    _Marginp.MEMO = _Marginp.MEMO_AFT;
                    _Marginp.MEMO_AFT = null;
                    _Marginp.LAST_UPDATE_DT = dt;
                    logStr = _Marginp.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        #endregion

        #region privateFunction

        /// <summary>
        /// 存入保證金資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<MarginpViewModel> GetDetailModel(IEnumerable<ITEM_DEP_RECEIVED> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new MarginpViewModel()
            {
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vMarginp_Take_Of_Type = x.MARGIN_TAKE_OF_TYPE,//類別
                vMarginp_Trad_Partners = x.TRAD_PARTNERS,//交易對象
                vItemId = x.ITEM_ID,//歸檔編號
                vMarginp_Amount = TypeTransfer.decimalNToString(x.AMOUNT),//金額
                vMarginp_Item = x.MARGIN_ITEM, // 物品名稱
                vMarginp_Item_Issuer = x.MARGIN_ITEM_ISSUER,// 物品發行人
                vMarginp_Pledge_Item_No = x.PLEDGE_ITEM_NO,//質押標的號碼
                vMarginp_Effective_Date_B = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_B),//有效期間(起)
                //vMarginp_Effective_Date_B = x.EFFECTIVE_DATE_B == null ? null : x.EFFECTIVE_DATE_B.Value.DateToTaiwanDate(9),
                vMarginp_Effective_Date_E = TypeTransfer.dateTimeNToString(x.EFFECTIVE_DATE_E),//有效期間(迄) 
                //vMarginp_Effective_Date_E = x.EFFECTIVE_DATE_E == null ? null : x.EFFECTIVE_DATE_E.Value.DateToTaiwanDate(9),
                vDescription =x.DESCRIPTION,//說明
                vMemo = x.MEMO,//備註
                vMarginp_Book_No = x.BOOK_NO,//冊號 
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}

