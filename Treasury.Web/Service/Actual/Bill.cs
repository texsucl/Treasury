﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{
    public class Bill : IBill
    {
        protected INTRA intra { private set; get; }

        public Bill()
        {
            intra = new INTRA();
        }

        #region GetData

        public string GetAccessType(string aplyNo)
        {
            var result = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                    result = _TAR.ACCESS_TYPE;
            }
            return result;
        }

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
                    .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList(); //抓庫存狀態設定
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                    _code = _TAR.ACCESS_TYPE == AccessProjectTradeType.P.ToString() ? "3" :
                           _TAR.ACCESS_TYPE == AccessProjectTradeType.G.ToString() ? "4" : "3";
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
                    //result.AddRange((List<BillViewModel>)GetTempData(aplyNo));
                    var _TAR = db.TREA_APLY_REC.AsNoTracking()
                         .FirstOrDefault(x => x.APLY_NO == aplyNo);
                    if (_TAR != null)
                        vAplyUnit = _TAR.APLY_UNIT;
                }
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList();
                     result.AddRange(db.ITEM_BLANK_NOTE.AsNoTracking()
                    .Where(x => x.INVENTORY_STATUS == inventoryStatus, !inventoryStatus.IsNullOrWhiteSpace()) //取出時只抓庫存狀態為庫存的項目
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
                    AccessProjectFormStatus.E02.ToString()
                };
                var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
                var _Inventory_types = sys_codes
                    .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList();
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

        #endregion

        #region SaveData
        /// <summary>
        /// 申請覆核動作
        /// </summary>
        /// <param name="insertDatas"></param>
        /// <param name="taData"></param>
        /// <returns></returns>
        public MSGReturnModel<ITreaItem> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<ITreaItem>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;
            try
            {
                if (insertDatas != null)
                {
                    var datas = (List<BillViewModel>)insertDatas;

                    //取出只抓狀態為預約取出的資料
                    if (taData.vAccessType == AccessProjectTradeType.G.ToString())
                    {
                        datas = datas.Where(x => x.vStatus == AccessInventoryTyp._4.GetDescription()).ToList();
                    }
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();

                            string _ITEM_BLANK_NOTE_ITEM_ID = null;
                            string logStr = string.Empty;

                            if (taData.vAccessType == AccessProjectTradeType.G.ToString())
                            {
                                bool _changFlag = false;
                                datas.ForEach(x =>
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
                                                _blank_Note.CHECK_NO_B = (TypeTransfer.stringToInt(x.vTakeOutE) + 1).ToString().PadLeft(8,'0');
                                            }                                     
                                            _blank_Note.LAST_UPDATE_DT = dt;
                                        }
                                    }
                                    else
                                    {
                                        _changFlag = true;                                       
                                    }
                                });
                                if (_changFlag)
                                {
                                    result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                    return result;
                                }
                            }

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                            #region 申請單紀錄檔
                            var _TAR = new TREA_APLY_REC()
                            {
                                APLY_NO = $@"G6{qPreCode}{cId}", //申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                                APLY_FROM = AccessProjectStartupType.M.ToString(), //人工
                                ITEM_ID = taData.vItem,
                                ACCESS_TYPE = taData.vAccessType, //存入(P) or 取出(G)
                                ACCESS_REASON = taData.vAccessReason,
                                APLY_STATUS = AccessProjectFormStatus.A01.ToString(), //表單申請
                                EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(taData.vExpectedAccessDate),
                                APLY_UNIT = taData.vAplyUnit,
                                APLY_UID = taData.vAplyUid,
                                APLY_DT = dt,
                                CREATE_UID = taData.vCreateUid,
                                CREATE_DT = dt,
                                LAST_UPDATE_UID = taData.vCreateUid,
                                LAST_UPDATE_DT = dt
                            };
                            logStr += _TAR.modelToString();
                            db.TREA_APLY_REC.Add(_TAR);
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

                            #region 空白票據申請資料檔
                            int seq = 0;
                            datas.ForEach(x =>
                            {
                                var item_id = sysSeqDao.qrySeqNo("E2", qPreCode).ToString().PadLeft(3, '0');
                                seq += 1;
                                var _BNA = new BLANK_NOTE_APLY() {
                                    APLY_NO = _TAR.APLY_NO,
                                    DATA_SEQ = seq,
                                    ITEM_ID = $@"E2{qPreCode}{item_id}",
                                    CHECK_TYPE = x.vCheckType,
                                    ISSUING_BANK = x.vIssuingBank,
                                    CHECK_NO_TRACK = x.vCheckNoTrack,
                                    CHECK_NO_B = x.vCheckNoB,
                                    CHECK_NO_E = taData.vAccessType == AccessProjectTradeType.P.ToString() ? x.vCheckNoE : x.vTakeOutE,
                                    ITEM_BLANK_NOTE_ITEM_ID = _ITEM_BLANK_NOTE_ITEM_ID
                                };
                                db.BLANK_NOTE_APLY.Add(_BNA);
                                logStr += "|";
                                logStr += _BNA.modelToString() ;
                            });
                            #endregion

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
                                    LogDao.Insert(log, _TAR.CREATE_UID);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}");
                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
                else
                {
                    result.DESCRIPTION = MessageType.not_Find_Audit_Data.GetDescription();
                }
            }
            catch (Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }

            return result;
        }
        #endregion

        #region privation function

        private BillViewModel BlankNoteAplyToBillViewModel(BLANK_NOTE_APLY data, string code ,int num, List<SYS_CODE> Inventory_types,bool tempFlag)
        {
            return new BillViewModel()
            {
                vRowNum = (num + 1).ToString(),
                vItemId = data.ITEM_ID,
                vAplyNo = data.APLY_NO,
                vDataSeq = data.DATA_SEQ.ToString(),
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
        /// 
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