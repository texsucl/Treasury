using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 空白票券
/// 初版作者：20180604 張家華
/// 修改歷程：20180604 張家華 
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
    public class Bill : Common, IBill
    {
        public Bill()
        {

        }

        #region GetData

        /// <summary>
        /// 明細資料(空白票據)
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public IEnumerable<ITreaItem> GetTempData(string aplyNo)
        {
            var result = new List<BillViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _code = "3"; //預約存入 , 4=> 預約取出
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == Ref.SysCodeType.INVENTORY_TYPE.ToString()).ToList(); //抓庫存狀態設定
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                    _code = _TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString() ? "3" :
                           _TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.G.ToString() ? "4" : "3";
                result = db.BLANK_NOTE_APLY.AsNoTracking()
                    .Where(x => x.APLY_NO == aplyNo)
                    .OrderBy(x => x.ISSUING_BANK)
                    .ThenBy(x => x.CHECK_TYPE)
                    .ThenBy(x => x.CHECK_NO_TRACK)
                    .AsEnumerable()
                    .Select((x, y) => BlankNoteAplyToBillViewModel(x, _code, y, _Inventory_types,true)).ToList();
            }

            return result;
        }

        /// <summary>
        /// 抓取庫存明細資料
        /// </summary>
        /// <param name="vAplyUnit">申請部門</param>
        /// <param name="inventoryStatus">庫存狀態</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        public IEnumerable<ITreaItem> GetDayData(string vAplyUnit = null, string inventoryStatus = null, string aplyNo = null)
        {
            var result = new List<BillViewModel>();         
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var _TAR = db.TREA_APLY_REC.AsNoTracking()
                         .FirstOrDefault(x => x.APLY_NO == aplyNo);
                    if (_TAR != null)
                        vAplyUnit = _TAR.APLY_UNIT;
                }
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString();
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                     result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存資料表只抓庫存 其他由申請紀錄檔 抓取
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT== dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(),!dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .AsEnumerable()
                    .Select(x => ItemBlankNoteToBillViewModel(x, _Inventory_types)));
            }

            if (inventoryStatus != "1") //排除只抓庫存其他都需要申請紀錄檔
            {
                result.AddRange(getTreaAplyRec(vAplyUnit));
            }

            return result;
        }

        /// <summary>
        /// 抓取申請紀錄檔
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        private List<BillViewModel> getTreaAplyRec(string vAplyUnit)
        {
            var result = new List<BillViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var nonShowStatus = new List<string>()
                {
                    Ref.AccessProjectFormStatus.E01.ToString(),
                    Ref.AccessProjectFormStatus.E02.ToString(), //申請人刪除狀態 (等同庫存有需排除)
                    Ref.AccessProjectFormStatus.E03.ToString(),
                };
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == Ref.SysCodeType.INVENTORY_TYPE.ToString()).ToList();
                db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => !nonShowStatus.Contains(x.APLY_STATUS)) //要確認是否須排除不能顯示的
                    .Where(x => x.APLY_UNIT == vAplyUnit).ToList()
                    .ForEach(x =>
                    {
                        var _accessType = x.ACCESS_TYPE; //P(存入) , G(取出)
                        var code = _accessType == "P" ? "3"  //預約存入
                        : _accessType == "G" ? "4"  //預約取出
                        : "";
                        result.AddRange(db.BLANK_NOTE_APLY.AsNoTracking()
                        .Where(y => y.APLY_NO == x.APLY_NO)
                        .AsEnumerable()
                        .Select((a, b) => BlankNoteAplyToBillViewModel(a, code, b, _Inventory_types,false)));
                    });
            }
            return result;
        }

        /// <summary>
        /// 發票行庫
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetIssuing_Bank()
        {
            var result = new List<SelectOption>() { new SelectOption() { Text = " ",Value = " "} };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                    .Where(x => x.ISSUING_BANK != null)
                    .Select(x => x.ISSUING_BANK)                
                    .Distinct().OrderBy(x => x).AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Text = x,
                        Value = x
                    }));
            }
            return result;
        }

        /// <summary>
        /// 類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetCheckType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "CHECK_TYPE")
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
        /// 查詢CDC資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="aply_No">資料庫異動申請單紀錄檔  INVENTORY_CHG_APLY 單號</param>
        /// <returns></returns>
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null, string charge_Dept = null, string charge_Sect = null)
        {
            List<CDCBillViewModel> result = new List<CDCBillViewModel>();

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
                    result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.CHARGE_DEPT == charge_Dept , !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect , !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select((x) => new CDCBillViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vBill_Issuing_Bank = x.ISSUING_BANK,
                            vBill_Issuing_Bank_AFT = x.ISSUING_BANK_AFT,
                            vBill_Check_Type = x.CHECK_TYPE,
                            vBill_Check_Type_AFT = x.CHECK_TYPE_AFT,
                            vBill_Check_No_Track = x.CHECK_NO_TRACK,
                            vBill_Check_No_Track_AFT = x.CHECK_NO_TRACK_AFT,
                            vBill_Check_No_B = x.CHECK_NO_B,
                            vBill_Check_No_B_AFT = x.CHECK_NO_B_AFT,
                            vBill_Check_No_E = x.CHECK_NO_E,
                            vBill_Check_No_E_AFT = x.CHECK_NO_E_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidNameByBill(itemIds);
                            result.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.FirstOrDefault(y => y.itemId == x.vItemId)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.BLANK_NOTE_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCBillViewModel()
                        {
                            vItemId = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vAply_Uid = x.APLY_UID,
                            vAply_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                            vCharge_Dept = x.CHARGE_DEPT,
                            vCharge_Dept_AFT = x.CHARGE_DEPT_AFT,
                            vCharge_Dept_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim(),
                            vCharge_Dept_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_DEPT_AFT)?.DPT_NAME?.Trim(),
                            vCharge_Sect = x.CHARGE_SECT,
                            vCharge_Sect_AFT = x.CHARGE_SECT_AFT,
                            vCharge_Sect_Name = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim(),
                            vCharge_Sect_Name_AFT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x.CHARGE_SECT_AFT)?.DPT_NAME?.Trim(),
                            vBill_Issuing_Bank = x.ISSUING_BANK,
                            vBill_Issuing_Bank_AFT = x.ISSUING_BANK_AFT,
                            vBill_Check_Type = x.CHECK_TYPE,
                            vBill_Check_Type_AFT = x.CHECK_TYPE_AFT,
                            vBill_Check_No_Track = x.CHECK_NO_TRACK,
                            vBill_Check_No_Track_AFT = x.CHECK_NO_TRACK_AFT,
                            vBill_Check_No_B = x.CHECK_NO_B,
                            vBill_Check_No_B_AFT = x.CHECK_NO_B_AFT,
                            vBill_Check_No_E = x.CHECK_NO_E,
                            vBill_Check_No_E_AFT = x.CHECK_NO_E_AFT,
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
                    //取得流水號
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                    var datas = (List<BillViewModel>)insertDatas;
                    string logStr = string.Empty; //log         
                    var item_Seq = "E2"; //空白票卷流水號開頭編碼         
                    if (!taData.vAplyNo.IsNullOrWhiteSpace()) //修改已存在申請單
                    {                     
                        if (datas.Any())
                        {
                            using (TreasuryDBEntities db = new TreasuryDBEntities())
                            {
                                var _APLY_STATUS = Ref.AccessProjectFormStatus.A01.ToString(); //表單申請
                                
                                #region 申請單紀錄檔
                                var _TAR = db.TREA_APLY_REC.First(x => x.APLY_NO == taData.vAplyNo);
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

                                #region 空白票據申請資料檔
                                List<BLANK_NOTE_APLY> updates = new List<BLANK_NOTE_APLY>();
                                List<BLANK_NOTE_APLY> inserts = new List<BLANK_NOTE_APLY>();

                                foreach (var item in datas)
                                {
                                    if (item.vItemId.StartsWith(item_Seq)) // 舊有資料
                                    {
                                        var _BNA = db.BLANK_NOTE_APLY.First(
                                             x => x.ITEM_ID == item.vItemId &&
                                                  x.APLY_NO == taData.vAplyNo);
                                        _BNA.CHECK_TYPE = item.vCheckType;
                                        _BNA.ISSUING_BANK = item.vIssuingBank;
                                        _BNA.CHECK_NO_TRACK = item.vCheckNoTrack;
                                        _BNA.CHECK_NO_B = item.vCheckNoB;
                                        _BNA.CHECK_NO_E = item.vCheckNoE;
                                        updates.Add(_BNA);
                                        logStr += _BNA.modelToString(logStr);
                                    }
                                    else
                                    {
                                        var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                        var _BNA = new BLANK_NOTE_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = $@"{item_Seq}{item_id}",
                                            CHECK_TYPE = item.vCheckType,
                                            ISSUING_BANK = item.vIssuingBank,
                                            CHECK_NO_TRACK = item.vCheckNoTrack,
                                            CHECK_NO_B = item.vCheckNoB,
                                            CHECK_NO_E = item.vCheckNoE
                                        };
                                        inserts.Add(_BNA);
                                        logStr += _BNA.modelToString(logStr);
                                    }
                                }

                                var ups = updates.Select(x => x.ITEM_ID).ToList();

                                db.BLANK_NOTE_APLY.RemoveRange(
                                    db.BLANK_NOTE_APLY.Where(x => x.APLY_NO == taData.vAplyNo && !ups.Contains( x.ITEM_ID)).ToList());
                                db.BLANK_NOTE_APLY.AddRange(inserts);

                                #endregion

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
                                        log.CFUNCTION = "申請覆核-修改空白票據";
                                        log.CACTION = "U";
                                        log.CCONTENT = logStr;
                                        LogDao.Insert(log, _TAR.CREATE_UID);
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
                    else //新增申請單
                    {
                        //取出只抓狀態為預約取出的資料
                        if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                        {
                            datas = datas.Where(x => x.vStatus == Ref.AccessInventoryType._4.GetDescription()).ToList();
                        }
                        if (datas.Any())
                        {
                            using (TreasuryDBEntities db = new TreasuryDBEntities())
                            {
                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);

                                logStr = data.Item2;

                                #endregion

                                string _ITEM_BLANK_NOTE_ITEM_ID = null; //紀錄空白票據申請資料檔 對應空白票據庫存資料檔 物品編號                              

                                bool _changFlag = false;
                                datas.ForEach(x =>
                                {
                                    #region 取出時要把空白票據資料 做切段動作 
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //取出時要把空白票據資料 做切段動作
                                    {
                                        var _blank_Note = db.ITEM_BLANK_NOTE.FirstOrDefault(y => y.ITEM_ID == x.vItemId);
                                        if (_blank_Note != null)
                                        {
                                            if (_blank_Note.LAST_UPDATE_DT > x.vLast_Update_Time || _blank_Note.INVENTORY_STATUS != "1")
                                            {
                                                _changFlag = true;
                                            }
                                            else
                                            {
                                                _ITEM_BLANK_NOTE_ITEM_ID = _blank_Note.ITEM_ID;
                                                //全部取出
                                                if (x.vTakeOutE == _blank_Note.CHECK_NO_E)
                                                {
                                                    _blank_Note.INVENTORY_STATUS = "4"; //預約取出
                                                }
                                                //分段取出
                                                else
                                                {
                                                    _blank_Note.CHECK_NO_B = (TypeTransfer.stringToInt(x.vTakeOutE) + 1).ToString().PadLeft(7, '0');
                                                }
                                                _blank_Note.LAST_UPDATE_DT = dt;
                                            }
                                        }
                                        else
                                        {
                                            _changFlag = true;
                                        }
                                    }
                                    #endregion

                                    #region 空白票據申請資料檔
                                    var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                    var _BNA = new BLANK_NOTE_APLY()
                                    {
                                        APLY_NO = data.Item1,
                                        ITEM_ID = $@"{item_Seq}{item_id}",
                                        CHECK_TYPE = x.vCheckType,
                                        ISSUING_BANK = x.vIssuingBank,
                                        CHECK_NO_TRACK = x.vCheckNoTrack,
                                        CHECK_NO_B = x.vCheckNoB,
                                        CHECK_NO_E = taData.vAccessType == Ref.AccessProjectTradeType.P.ToString() ? x.vCheckNoE : x.vTakeOutE,
                                        ITEM_BLANK_NOTE_ITEM_ID = _ITEM_BLANK_NOTE_ITEM_ID
                                    };
                                    db.BLANK_NOTE_APLY.Add(_BNA);
                                    logStr += _BNA.modelToString(logStr);
                                    #endregion
                                });
                                if (_changFlag)
                                {
                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                    return result;
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
                                        log.CFUNCTION = "申請覆核-新增空白票據";
                                        log.CACTION = "A";
                                        log.CCONTENT = logStr;
                                        LogDao.Insert(log, taData.vCreateUid);
                                        #endregion

                                        result.RETURN_FLAG = true;
                                        result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{data.Item1}");
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
        /// 申請刪除 & 作廢 空白票據資料庫要復原的事件
        /// </summary>
        /// <param name="db"></param>
        /// <param name="aply_No"></param>
        /// <param name="logStr"></param>
        /// <param name="dt"></param>
        /// <param name="deleFlag"></param>
        /// <returns></returns>
        public Tuple<bool,string> Recover(TreasuryDBEntities db,string aply_No,string logStr,DateTime dt,bool deleFlag)
        {
            var _changeFlag = false;
            //以「申請單號」為鍵項讀取【空白票據申請資料檔】
            var _BLANK_NOTE_APLY = db.BLANK_NOTE_APLY.Where(x => x.APLY_NO == aply_No).ToList();
            _BLANK_NOTE_APLY
                .ForEach(x =>
                {
                    var _CHECK_NO_E = TypeTransfer.stringToInt(x.CHECK_NO_E) + 1;
                                    //依【空白票據申請資料檔】的「ITEM_BLANK_NOTE_ITEM_ID」查【空白票據庫存資料檔】
                                    //異動欄位：「庫存狀態」= 1在庫
                                    //最後異動日期時間：系統時間
                    var _ITEM_BLANK_NOTE = db.ITEM_BLANK_NOTE.FirstOrDefault(y => y.ITEM_ID == x.ITEM_BLANK_NOTE_ITEM_ID);
                    if (_ITEM_BLANK_NOTE != null)
                    {
                        if (_ITEM_BLANK_NOTE.INVENTORY_STATUS == "1" &&
                        TypeTransfer.stringToInt(_ITEM_BLANK_NOTE.CHECK_NO_B) == _CHECK_NO_E) //在庫 且 申請資料的迄號(+1) = 庫存起號
                                        {
                            _ITEM_BLANK_NOTE.CHECK_NO_B = x.CHECK_NO_B;
                            _ITEM_BLANK_NOTE.LAST_UPDATE_DT = dt;
                            logStr += _ITEM_BLANK_NOTE.modelToString(logStr);
                        }
                        else if (
                        _ITEM_BLANK_NOTE.INVENTORY_STATUS == "4" &&
                        _ITEM_BLANK_NOTE.CHECK_NO_B == x.CHECK_NO_B &&
                        _ITEM_BLANK_NOTE.CHECK_NO_E == x.CHECK_NO_E) //全部都被預約取出 且 起訖號相同
                                        {
                            _ITEM_BLANK_NOTE.INVENTORY_STATUS = "1";
                            _ITEM_BLANK_NOTE.LAST_UPDATE_DT = dt;
                            logStr += _ITEM_BLANK_NOTE.modelToString(logStr);
                        }
                        else //其他情形 須新增一筆 空白票據庫存資料檔
                        {
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            var cId = sysSeqDao.qrySeqNo("E2", string.Empty).ToString().PadLeft(8, '0');
                            var _newITEM_BLANK_NOTE = _ITEM_BLANK_NOTE.ModelConvert<ITEM_BLANK_NOTE, ITEM_BLANK_NOTE>();
                            _newITEM_BLANK_NOTE.ITEM_ID = $@"E2{cId}";
                            _newITEM_BLANK_NOTE.INVENTORY_STATUS = "1";
                            _newITEM_BLANK_NOTE.CHECK_NO_B = x.CHECK_NO_B;
                            _newITEM_BLANK_NOTE.CHECK_NO_E = x.CHECK_NO_E;
                            _newITEM_BLANK_NOTE.CREATE_DT = dt;
                            _newITEM_BLANK_NOTE.LAST_UPDATE_DT = dt;
                            db.ITEM_BLANK_NOTE.Add(_newITEM_BLANK_NOTE);
                            logStr += _newITEM_BLANK_NOTE.modelToString(logStr);
                        }
                    }
                    else
                    {
                        _changeFlag = true;
                    }
                });
            if (_changeFlag)
            {
                return new Tuple<bool, string>(false, logStr);
            }
            else
            {
                if(deleFlag)
                    db.BLANK_NOTE_APLY.RemoveRange(_BLANK_NOTE_APLY);
                return new Tuple<bool, string>(true, logStr);
            }
            
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態資料要復原
            {
                return Recover(db, aply_No, logStr, dt, false);
            }
            else
            {
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態資料要復原
            {
                return Recover(db, aply_No, logStr, dt, true);
            }
            else
            {
                db.BLANK_NOTE_APLY.RemoveRange(db.BLANK_NOTE_APLY.Where(x => x.APLY_NO == aply_No));
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
                foreach (CDCBillViewModel model in saveData)
                {
                    var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                    if (_Bill != null && !changFlag)
                    {
                        if (_Bill.LAST_UPDATE_DT > model.vLast_Update_Time || _Bill.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }
                        if (!changFlag)
                        {
                            _Bill.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _Bill.ISSUING_BANK_AFT = model.vBill_Issuing_Bank_AFT;
                            _Bill.CHECK_TYPE_AFT = model.vBill_Check_Type_AFT;
                            _Bill.CHECK_NO_TRACK_AFT = model.vBill_Check_No_Track_AFT;
                            _Bill.CHECK_NO_B_AFT = model.vBill_Check_No_B_AFT;
                            _Bill.CHECK_NO_E_AFT = model.vBill_Check_No_E_AFT;
                            _Bill.LAST_UPDATE_DT = dt;

                            logStr += _Bill.modelToString(logStr);

                            var _BNA = new BLANK_NOTE_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _Bill.ITEM_ID,
                                ISSUING_BANK = model.vBill_Issuing_Bank_AFT,
                                CHECK_TYPE = model.vBill_Check_Type_AFT.IsNullOrEmpty() ? string.Empty : model.vBill_Check_Type_AFT,
                                CHECK_NO_TRACK = model.vBill_Check_No_Track_AFT.IsNullOrEmpty() ? string.Empty : model.vBill_Check_No_Track_AFT,
                                CHECK_NO_B = model.vBill_Check_No_B_AFT.IsNullOrEmpty() ? string.Empty : model.vBill_Check_No_B_AFT,
                                CHECK_NO_E = model.vBill_Check_No_E_AFT.IsNullOrEmpty() ? string.Empty : model.vBill_Check_No_E_AFT
                            };

                            db.BLANK_NOTE_APLY.Add(_BNA);

                            logStr += _BNA.modelToString(logStr);
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
                    log.CFUNCTION = "申請覆核-資料庫異動:空白票據";
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
                var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Bill != null)
                {
                    _Bill.INVENTORY_STATUS = "1"; //在庫
                    _Bill.ISSUING_BANK_AFT = null;
                    _Bill.CHECK_TYPE_AFT = null;
                    _Bill.CHECK_NO_TRACK_AFT = null;
                    _Bill.CHECK_NO_B_AFT = null;
                    _Bill.CHECK_NO_E_AFT = null;
                    _Bill.LAST_UPDATE_DT = dt;
                    logStr += _Bill.modelToString(logStr);
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
                var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Bill != null)
                {
                    _Bill.INVENTORY_STATUS = "1"; //在庫
                    _Bill.ISSUING_BANK = GetNewValue(_Bill.ISSUING_BANK, _Bill.ISSUING_BANK_AFT);
                    _Bill.ISSUING_BANK_AFT = null;
                    _Bill.CHECK_TYPE = GetNewValue(_Bill.CHECK_TYPE, _Bill.CHECK_TYPE_AFT);
                    _Bill.CHECK_TYPE_AFT = null;
                    _Bill.CHECK_NO_TRACK = GetNewValue(_Bill.CHECK_NO_TRACK, _Bill.CHECK_NO_TRACK_AFT);
                    _Bill.CHECK_NO_TRACK_AFT = null;
                    _Bill.CHECK_NO_B = GetNewValue(_Bill.CHECK_NO_B, _Bill.CHECK_NO_B_AFT);
                    _Bill.CHECK_NO_B_AFT = null;
                    _Bill.CHECK_NO_E = GetNewValue(_Bill.CHECK_NO_E, _Bill.CHECK_NO_E_AFT);
                    _Bill.CHECK_NO_E_AFT = null;
                    _Bill.LAST_UPDATE_DT = dt;
                    logStr += _Bill.modelToString(logStr);
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
                var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Bill != null)
                {
                    _Bill.INVENTORY_STATUS = "1"; //在庫
                    _Bill.CHARGE_DEPT_AFT = null;
                    _Bill.CHARGE_SECT_AFT = null;
                    _Bill.LAST_UPDATE_DT = dt;
                    logStr += _Bill.modelToString(logStr);
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
                var _Bill = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Bill != null)
                {
                    _Bill.INVENTORY_STATUS = "1"; //在庫
                    _Bill.CHARGE_DEPT = _Bill.CHARGE_DEPT_AFT;
                    _Bill.CHARGE_DEPT_AFT = null;
                    _Bill.CHARGE_SECT = _Bill.CHARGE_SECT_AFT;
                    _Bill.CHARGE_SECT_AFT = null;
                    _Bill.LAST_UPDATE_DT = dt;
                    logStr += _Bill.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        #endregion

        #region privation function

        /// <summary>
        /// 明細資料 ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="num"></param>
        /// <param name="Inventory_types"></param>
        /// <param name="tempFlag"></param>
        /// <returns></returns>
        private BillViewModel BlankNoteAplyToBillViewModel(BLANK_NOTE_APLY data, string code ,int num, List<SYS_CODE> Inventory_types,bool tempFlag)
        {
            return new BillViewModel()
            {
                vRowNum = (num + 1).ToString(),
                vItemId = data.ITEM_ID,
                vAplyNo = data.APLY_NO,
                vDataSeq = null,
                vStatus = Inventory_types.FirstOrDefault(x => x.CODE == code)?.CODE_VALUE,
                vIssuingBank = data.ISSUING_BANK,
                vCheckType = data.CHECK_TYPE,
                vCheckNoTrack = data.CHECK_NO_TRACK,
                vCheckNoB = data.CHECK_NO_B,
                vCheckNoE = data.CHECK_NO_E,
                vCheckTotalNum = tempFlag ?
                ((code == "3") ?
                (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString() : string.Empty) :
                (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString(),
                vTakeOutTotalNum = tempFlag ? 
                ((code == "4") ?
                (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString() : string.Empty) : null,
                vTakeOutE = tempFlag ? ((code == "4") ? data.CHECK_NO_E : string.Empty) : null
            };
        }

      
        /// <summary>
        /// 庫存資料ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Inventory_types">庫存狀態</param>
        /// <returns></returns>
        private BillViewModel ItemBlankNoteToBillViewModel(ITEM_BLANK_NOTE data,List<SYS_CODE> Inventory_types)
        {
            return new BillViewModel()
            {
                vItemId = data.ITEM_ID,
                vAplyNo = null,
                vDataSeq = null,
                vStatus = Inventory_types.FirstOrDefault(x => x.CODE == data.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態
                vIssuingBank = data.ISSUING_BANK,
                vCheckType = data.CHECK_TYPE, 
                vCheckNoTrack = data.CHECK_NO_TRACK, 
                vCheckNoB = data.CHECK_NO_B,
                vCheckNoE = data.CHECK_NO_E,
                vCheckTotalNum = (TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString(),
                vLast_Update_Time = data.LAST_UPDATE_DT
            };
        }

        #endregion
    }

}