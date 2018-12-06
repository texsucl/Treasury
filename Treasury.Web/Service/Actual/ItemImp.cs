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
using Treasury.WebControllers;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 重要物品
/// 初版作者：20180810 陳宥穎
/// 修改歷程：20180810 陳宥穎 
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
    public class ItemImp : Common, IItemImp
    {
        public ItemImp()
        {

        }

        #region Get Date

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<ItemImpViewModel> GetDataByAplyNo(string aplyNo)
        {
            var result = new List<ItemImpViewModel>();
            //result.
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取歸檔編號
                    var OTHER_ITEM_APLYs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).ToList();
                    var OIAs = OTHER_ITEM_APLYs.Select(x => x.ITEM_ID).ToList();
                    //使用歸檔編號去重要物品庫存資料檔抓取資料
                    var details = db.ITEM_IMPO.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        bool _accessStatus = (_TAR.APLY_STATUS == Ref.AccessProjectFormStatus.E01.ToString()) && (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString());
                        result = GetDetailModel(details, _Inventory_types, OTHER_ITEM_APLYs, _accessStatus).ToList();
                        result.ForEach(x =>
                        {
                            x.vtakeoutFlag = x.vItemImp_G_Quantity == null ? false :
                                             (x.vItemImp_G_Quantity.Value == 0 ? false : true);
                        });
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
        public List<ItemImpViewModel> GetDbDataByUnit(string vAplyUnit = null, string aplyNo = null)
        {
            var result = new List<ItemImpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_IMPO.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1" && x.REMAINING != 0) //庫存 & 剩餘數量不能為0
                    .AsEnumerable()
                    .Select(x => 
                    new ItemImpViewModel() {
                        vShowItemId = x.ITEM_ID, //歸檔編號
                        vItemId = x.ITEM_ID,
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vItemImp_Remaining = x.REMAINING,                     
                        vItemImp_Name = x.ITEM_NAME,
                        vItemImp_Quantity = x.QUANTITY,
                        vItemImp_Amount = x.AMOUNT,
                        vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                        vDescription = x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vtakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                    
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aplyNo).ToList();
                    var itemIds = OIAs.Select(x => x.ITEM_ID).ToList();
                    itemIds.ForEach(x =>
                    {
                        var del = result.FirstOrDefault(y => y.vItemId == x);
                        if (del != null)
                            result.Remove(del);
                    });
                    db.ITEM_IMPO.AsNoTracking().Where(x =>
                    itemIds.Contains(x.ITEM_ID)).ToList()
                    .ForEach(x =>
                    {
                        var _Quantity = TypeTransfer.intNToInt(OIAs.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.Memo_I);
                        result.Add(
                            new ItemImpViewModel()
                            {
                                vShowItemId = x.ITEM_ID, //歸檔編號
                                vItemId = x.ITEM_ID,
                                vStatus = Ref.AccessInventoryType._4.GetDescription(),
                                vItemImp_G_Quantity = _Quantity,
                                vItemImp_Remaining = x.REMAINING + _Quantity,
                                vItemImp_Name = x.ITEM_NAME,
                                vItemImp_Quantity = x.QUANTITY,
                                vItemImp_Amount = x.AMOUNT,
                                vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                                vDescription = x.DESCRIPTION,
                                vMemo = x.MEMO,
                                vtakeoutFlag = true,
                                vLast_Update_Time = x.LAST_UPDATE_DT
                            });
                    });
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
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null, string charge_Dept = null, string charge_Sect = null)
        {
            List<CDCItemImpViewModel> result = new List<CDCItemImpViewModel>();

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
                    result.AddRange(db.ITEM_IMPO.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.ITEM_ID == searchModel.vItem_No, !searchModel.vItem_No.IsNullOrWhiteSpace())
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.CHARGE_DEPT == charge_Dept, !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect, !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select((x) => new CDCItemImpViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vlItem_Id = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPUT_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAPLY_UID = x.APLY_UID,
                            vAPLY_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vItemImp_Name = x.ITEM_NAME,
                            vItemImp_Name_AFT = x.ITEM_NAME_AFT,
                            vItemImp_Quantity = x.QUANTITY,
                            vItemImp_Remaining = x.REMAINING,
                            vItemImp_Remaining_AFT = x.REMAINING_AFT,
                            vItemImp_Amount = x.AMOUNT,
                            vItemImp_Amount_AFT = x.AMOUNT_AFT,
                            vItemImp_Expected_Date = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                            vItemImp_Expected_Date_AFT =TypeTransfer.dateTimeNToString( x.EXPECTED_ACCESS_DATE_AFT),
                            vItemImp_Description = x.DESCRIPTION,
                            vItemImp_Description_AFT = x.DESCRIPTION_AFT,
                            vItemImp_MEMO = x.MEMO,
                            vItemImp_MEMO_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());

                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidNameByIMPO(itemIds);
                            result.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.FirstOrDefault(y => y.itemId == x.vItemId)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_IMPO.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCItemImpViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vlItem_Id = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPUT_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAPLY_UID = x.APLY_UID,
                            vAPLY_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vItemImp_Name = x.ITEM_NAME,
                            vItemImp_Name_AFT = x.ITEM_NAME_AFT,
                            vItemImp_Quantity = x.QUANTITY,
                            vItemImp_Remaining = x.REMAINING,
                            vItemImp_Remaining_AFT = x.REMAINING_AFT,
                            vItemImp_Amount = x.AMOUNT,
                            vItemImp_Amount_AFT = x.AMOUNT_AFT,
                            vItemImp_Expected_Date =TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE),
                            vItemImp_Expected_Date_AFT = TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE_AFT),
                            vItemImp_Description = x.DESCRIPTION,
                            vItemImp_Description_AFT = x.DESCRIPTION_AFT,
                            vItemImp_MEMO = x.MEMO,
                            vItemImp_MEMO_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                }
                result.ForEach(x =>
                {
                    x.vCharge_Name = !x.vCharge_Sect_Name.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name : x.vCharge_Dept_Name;
                    x.vCharge_Name_AFT = !x.vCharge_Sect_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Sect_Name_AFT : (!x.vCharge_Dept_Name_AFT.IsNullOrWhiteSpace() ? x.vCharge_Dept_Name_AFT : null);
                });
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 申請覆核 重要物品
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
                    var datas = (List<ItemImpViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            var item_Seq = "E8"; //重要物品流水號開頭編碼

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            var _TAR = new TREA_APLY_REC(); //申請單號
                            var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請
                            
                            if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                            {
                                #region 申請單紀錄檔
                                _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
                                if (CustodyAppr.Contains(_TAR.APLY_STATUS))
                                {
                                    _APLY_STATUS = CustodyConfirmStatus;
                                    _TAR.CUSTODY_UID = AccountController.CurrentUserId; //保管單位直接帶使用者
                                    _TAR.CUSTODY_DT = dt;
                                }
                                else
                                {
                                    if (_TAR.APLY_STATUS != _APLY_STATUS) //申請紀錄檔狀態不是在表單申請狀態
                                        _APLY_STATUS = Ref.AccessProjectFormStatus.A05.ToString(); //為重新申請案例
                                }
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

                                #region 重要物品庫存資料檔

                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                List<OTHER_ITEM_APLY> oldOIAs = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList(); //原有OTHER_ITEM_APLY
                                List<string> oldItemIds = oldOIAs.Select(x => x.ITEM_ID).ToList(); //原有 itemId 
                                List<Tuple<string,int?>> updateItemIds = new List<Tuple<string,int?>>(); //更新 itemId
                                List<string> cancelItemIds = new List<string>(); //取消 itemId
                                List<ITEM_IMPO> inserts = new List<ITEM_IMPO>(); //新增資料

                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _IC_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _II = new ITEM_IMPO();

                                            if (item.vItemId.StartsWith(item_Seq)) //舊有資料
                                            {
                                                _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                { 
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _II.ITEM_NAME = item.vItemImp_Name; //重要物品名稱
                                                _II.QUANTITY = item.vItemImp_Quantity; //重要物品數量
                                                _II.REMAINING = item.vItemImp_Quantity; //重要物品剩餘數量
                                                _II.AMOUNT = item.vItemImp_Amount; //重要物品金額
                                                _II.EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date); //重要物品預計提取日期
                                                _II.DESCRIPTION = item.vDescription;//說明
                                                _II.MEMO = item.vMemo; //備註
                                                updateItemIds.Add(new Tuple<string, int?>(item.vItemId,null));
                                                logStr += _II.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');                                               
                                                _II = new ITEM_IMPO()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    ITEM_NAME = item.vItemImp_Name, //重要物品名稱
                                                    QUANTITY = item.vItemImp_Quantity, //重要物品數量
                                                    REMAINING = item.vItemImp_Quantity, //重要物品剩餘數量
                                                    AMOUNT = item.vItemImp_Amount, //重要物品金額
                                                    EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date), //重要物品預計提取日期
                                                    DESCRIPTION = item.vDescription,//說明
                                                    MEMO = item.vMemo, //備註
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                                               //PUT_DATE = dt, //存入日期時間
                                                    LAST_UPDATE_DT = dt, //最後修改時間
                                                };
                                                _IC_Item_Id = _II.ITEM_ID;
                                                inserts.Add(_II);
                                                logStr += _II.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                    {
                                        var _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vtakeoutFlag)
                                        {
                                            var old = oldOIAs.FirstOrDefault(x => x.ITEM_ID == _II.ITEM_ID);
                                            if (old != null) //原先就為取出
                                            {
                                                if (old.Memo_I != item.vItemImp_G_Quantity) //取出數量不相等 (修改)
                                                {
                                                    //(新)剩餘數量 = (舊)剩餘數量 + 原先取出數量 - 畫面取出數量
                                                    _II.REMAINING = _II.REMAINING + old.Memo_I.Value - item.vItemImp_G_Quantity.Value;
                                                    if (_II.REMAINING == 0)
                                                        _II.INVENTORY_STATUS = "4";  //剩餘數量=0,預約取出
                                                    else
                                                        _II.INVENTORY_STATUS = "1";  //改回在庫
                                                    _II.LAST_UPDATE_DT = dt;
                                                    updateItemIds.Add(new Tuple<string, int?>(_II.ITEM_ID, item.vItemImp_G_Quantity));
                                                }
                                                else //相等
                                                {
                                                    updateItemIds.Add(new Tuple<string, int?>(_II.ITEM_ID, old.Memo_I));
                                                }
                                                //else 相等
                                            }
                                            else //新增取出
                                            {
                                                _II.REMAINING = _II.REMAINING - item.vItemImp_G_Quantity.Value;
                                                if (_II.REMAINING == 0)
                                                    _II.INVENTORY_STATUS = "4"; //剩餘數量=0,預約取出
                                                _II.LAST_UPDATE_DT = dt;
                                                updateItemIds.Add(new Tuple<string, int?>(_II.ITEM_ID, item.vItemImp_G_Quantity));
                                            }
                                        }
                                        else
                                        {
                                            if (oldItemIds.Contains(item.vItemId))
                                            {
                                                var old = oldOIAs.FirstOrDefault(x => x.ITEM_ID == _II.ITEM_ID);
                                                if (old != null) //原先為取出
                                                {
                                                    _II.REMAINING = _II.REMAINING + old.Memo_I.Value;
                                                    _II.INVENTORY_STATUS = "1"; //改回在庫
                                                    _II.LAST_UPDATE_DT = dt;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Select(y=>y.Item1).ToList().Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_IMPO.RemoveRange(db.ITEM_IMPO.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_IMPO.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                {
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.Item1,
                                        Memo_I = x.Item2
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

                                #region 重要物品庫存資料檔
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in datas)
                                {
                                    var _II_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _II = new ITEM_IMPO()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                ITEM_NAME = item.vItemImp_Name, //重要物品名稱
                                                QUANTITY = item.vItemImp_Quantity, //重要物品數量
                                                REMAINING = item.vItemImp_Quantity, //重要物品剩餘數量
                                                AMOUNT = item.vItemImp_Amount, //重要物品金額
                                                EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(item.vItemImp_Expected_Date), //重要物品預計提取日期
                                                DESCRIPTION = item.vDescription,//說明
                                                MEMO = item.vMemo, //備註
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                LAST_UPDATE_DT = dt, //最後修改時間
                                            };
                                            _II_Item_Id = _II.ITEM_ID;
                                            db.ITEM_IMPO.Add(_II);
                                            logStr += _II.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vItemImp_G_Quantity != null)
                                        {
                                            var _II = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _II_Item_Id = _II.ITEM_ID;
                                            if (_II.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _II.REMAINING = (_II.REMAINING - item.vItemImp_G_Quantity.Value); //(新)剩餘數量 = (原)剩餘數量 - 取出數量
                                            if (_II.REMAINING == 0)
                                                _II.INVENTORY_STATUS = "4"; //剩餘數量=0,預約取出
                                            _II.LAST_UPDATE_DT = dt;  //最後修改時間
                                        }
                                    }

                                    #region 其它存取項目申請資料檔
                                    if (!_II_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _II_Item_Id,
                                            Memo_I = item.vItemImp_G_Quantity
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
                                        log.CFUNCTION = "申請覆核-新增重要物品";
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
            var OISs = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == aply_No).ToList();
            var itemIds = OISs.Select(x => x.ITEM_ID).ToList();
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態重要物品庫存資料檔要復原
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    var OIS = OISs.First(x => x.ITEM_ID == item.ITEM_ID);
                    item.REMAINING = item.REMAINING + OIS.Memo_I.Value;
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態重要物品庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    var OIS = otherItemAplys.First(x => x.ITEM_ID == item.ITEM_ID);
                    item.REMAINING = item.REMAINING + OIS.Memo_I.Value;
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除重要物品庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_IMPO.RemoveRange(db.ITEM_IMPO.Where(x => itemIds.Contains(x.ITEM_ID)));
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
                foreach (CDCItemImpViewModel model in saveData)
                {
                    var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                    if (_ItemImp != null && !changFlag)
                    {
                        if (_ItemImp.LAST_UPDATE_DT > model.vLast_Update_Time || _ItemImp.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }
                        if (!changFlag)
                        {
                            _ItemImp.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _ItemImp.ITEM_NAME_AFT = model.vItemImp_Name_AFT;
                            _ItemImp.REMAINING_AFT = model.vItemImp_Remaining_AFT;
                            _ItemImp.AMOUNT_AFT = model.vItemImp_Amount_AFT;
                            _ItemImp.EXPECTED_ACCESS_DATE_AFT = TypeTransfer.stringToDateTimeN(model.vItemImp_Expected_Date_AFT);
                            _ItemImp.DESCRIPTION_AFT = model.vItemImp_Description_AFT;
                            _ItemImp.MEMO_AFT = model.vItemImp_MEMO_AFT;
                            _ItemImp.LAST_UPDATE_DT = dt;

                            logStr = _ItemImp.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _ItemImp.ITEM_ID
                            };

                            db.OTHER_ITEM_APLY.Add(_OIA);

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
                    log.CFUNCTION = "申請覆核-資料庫異動:重要物品";
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
                var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_ItemImp != null)
                {
                    _ItemImp.INVENTORY_STATUS = "1"; //在庫
                    _ItemImp.ITEM_NAME_AFT = null;
                    _ItemImp.REMAINING_AFT = null;
                    _ItemImp.AMOUNT_AFT = null;
                    _ItemImp.EXPECTED_ACCESS_DATE_AFT = null;
                    _ItemImp.DESCRIPTION_AFT = null;
                    _ItemImp.MEMO_AFT = null;
                    _ItemImp.LAST_UPDATE_DT = dt;
                    logStr = _ItemImp.modelToString(logStr);
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
                var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_ItemImp != null)
                {
                    _ItemImp.INVENTORY_STATUS = "1"; //在庫
                    _ItemImp.ITEM_NAME = string.IsNullOrEmpty(_ItemImp.ITEM_NAME_AFT) ? _ItemImp.ITEM_NAME : _ItemImp.ITEM_NAME_AFT;
                    _ItemImp.ITEM_NAME_AFT = null;
                    //比較剩餘數量異動值
                    int QUANTITY_N = 0;
                    if (TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT) == 0)
                    {
                        QUANTITY_N = _ItemImp.QUANTITY;
                    }
                    else
                    {
                        //剩餘數量及剩餘數量_異動後比較
                        if (_ItemImp.REMAINING > TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT))
                        {
                            QUANTITY_N = _ItemImp.QUANTITY - (_ItemImp.REMAINING - TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT));
                        }
                        else if (_ItemImp.REMAINING < TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT))
                        {
                            QUANTITY_N = _ItemImp.QUANTITY + (TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT - _ItemImp.REMAINING));
                        }
                    }
                    _ItemImp.QUANTITY = QUANTITY_N;
                    _ItemImp.REMAINING = TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT) == 0 ? _ItemImp.REMAINING : TypeTransfer.intNToInt(_ItemImp.REMAINING_AFT);
                    _ItemImp.REMAINING_AFT = null;
                    _ItemImp.AMOUNT = string.IsNullOrEmpty(TypeTransfer.decimalNToString(_ItemImp.AMOUNT_AFT)) ? _ItemImp.AMOUNT : _ItemImp.AMOUNT_AFT;
                    _ItemImp.AMOUNT_AFT = null;
                    _ItemImp.EXPECTED_ACCESS_DATE = string.IsNullOrEmpty(TypeTransfer.dateTimeNToString(_ItemImp.EXPECTED_ACCESS_DATE_AFT)) ? _ItemImp.EXPECTED_ACCESS_DATE : _ItemImp.EXPECTED_ACCESS_DATE_AFT;
                    _ItemImp.EXPECTED_ACCESS_DATE_AFT = null;
                    _ItemImp.DESCRIPTION = string.IsNullOrEmpty(_ItemImp.DESCRIPTION_AFT) ? _ItemImp.DESCRIPTION : _ItemImp.DESCRIPTION_AFT;
                    _ItemImp.DESCRIPTION_AFT = null;
                    _ItemImp.MEMO = string.IsNullOrEmpty(_ItemImp.MEMO_AFT) ? _ItemImp.MEMO : _ItemImp.MEMO_AFT;
                    _ItemImp.MEMO_AFT = null;
                    _ItemImp.LAST_UPDATE_DT = dt;
                    logStr = _ItemImp.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_ItemImp != null)
                {
                    _ItemImp.INVENTORY_STATUS = "1"; //在庫
                    _ItemImp.CHARGE_DEPT_AFT = null;
                    _ItemImp.CHARGE_SECT_AFT = null;
                    _ItemImp.LAST_UPDATE_DT = dt;
                    logStr = _ItemImp.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 庫存權責異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> CDCChargeApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {
                var _ItemImp = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_ItemImp != null)
                {
                    _ItemImp.INVENTORY_STATUS = "1"; //在庫
                    _ItemImp.CHARGE_DEPT = _ItemImp.CHARGE_DEPT_AFT;
                    _ItemImp.CHARGE_DEPT_AFT = null;
                    _ItemImp.CHARGE_SECT = _ItemImp.CHARGE_SECT_AFT;
                    _ItemImp.CHARGE_SECT_AFT = null;
                    _ItemImp.LAST_UPDATE_DT = dt;
                    logStr = _ItemImp.modelToString(logStr);
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
        /// 重要物品資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<ItemImpViewModel> GetDetailModel(IEnumerable<ITEM_IMPO> data,List<SYS_CODE> _Inventory_types,List<OTHER_ITEM_APLY> OIAs, bool accessStatus)
        {
            return data.Select(x => new ItemImpViewModel()
            {
                vShowItemId = x.ITEM_ID, //歸檔編號
                vItemImp_Remaining = x.REMAINING, //剩餘數量
                vItemId = x.ITEM_ID, //歸檔編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vItemImp_Name = accessStatus ? x.ITEM_NAME_ACCESS : x.ITEM_NAME, //重要物品名稱
                vItemImp_G_Quantity = OIAs.FirstOrDefault(y => x.ITEM_ID == y.ITEM_ID)?.Memo_I, // 取出數量
                vItemImp_Quantity = accessStatus ? x.QUANTITY_ACCESS.HasValue ? x.QUANTITY_ACCESS.Value : -1 : x.QUANTITY, //重要物品數量
                vItemImp_Amount = accessStatus? x.AMOUNT_ACCESS : x.AMOUNT, //重要物品金額
                //vItemImp_Expected_Date_1 = x.EXPECTED_ACCESS_DATE == null ? null : x.EXPECTED_ACCESS_DATE.Value.DateToTaiwanDate(9),
                vItemImp_Expected_Date = accessStatus ? TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE_ACCESS) : TypeTransfer.dateTimeNToString(x.EXPECTED_ACCESS_DATE), //重要物品預計提取日期
                vDescription = accessStatus ? x.DESCRIPTION_ACCESS : x.DESCRIPTION,//說明
                vMemo = accessStatus? x.MEMO_ACCESS : x.MEMO, //備註
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}