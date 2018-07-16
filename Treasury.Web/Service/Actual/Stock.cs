using System;
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
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{
    public class Stock : IStock
    {

        #region GetData
        /// <summary>
        /// 明細資料(股票)
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public IEnumerable<ITreaItem> GetTempData(string aplyNo)
        {
            var result = new List<StockViewModel>();

            //using (TreasuryDBEntities db = new TreasuryDBEntities())
            //{
            //    var sys_codes = db.SYS_CODE.AsNoTracking().ToList();
            //    var _code = "3"; //預約存入 , 4=> 預約取出
            //    var _Inventory_types = sys_codes
            //        .Where(x => x.CODE_TYPE == SysCodeType.INVENTORY_TYPE.ToString()).ToList(); //抓庫存狀態設定
            //    var _TAR = db.TREA_APLY_REC.AsNoTracking()
            //        .FirstOrDefault(x => x.APLY_NO == aplyNo);
            //    if (_TAR != null)
            //        _code = _TAR.ACCESS_TYPE == AccessProjectTradeType.P.ToString() ? "3" :
            //               _TAR.ACCESS_TYPE == AccessProjectTradeType.G.ToString() ? "4" : "3";
            //    result = db.BLANK_NOTE_APLY.AsNoTracking()
            //        .Where(x => x.APLY_NO == aplyNo)
            //        .OrderBy(x => x.ISSUING_BANK)
            //        .ThenBy(x => x.CHECK_TYPE)
            //        .ThenBy(x => x.CHECK_NO_TRACK)
            //        .AsEnumerable()
            //        .Select((x, y) => BlankNoteAplyToBillViewModel(x, _code, y, _Inventory_types, true)).ToList();
            //}

            return result;
        }

        /// <summary>
        /// 最大股票編號(新增股票)
        /// </summary>
        /// <returns></returns>
        public int GetMaxStockNo()
        {
            int result;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == "STOCK")
                    .Max(x => x.GROUP_NO);
            }
            return result;
        }

        /// <summary>
        /// 股票資料
        /// </summary>
        /// <returns></returns>
        public List<ItemBookStock> GetStockDate(int GroupNo)
        {
            var result = new List<ItemBookStock>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var vObj = from A in db.ITEM_BOOK.Where(x => x.ITEM_ID == "STOCK" & x.COL == "AREA")
                           join M in db.ITEM_BOOK.Where(x => x.ITEM_ID == "STOCK" & x.COL == "MEMO") on A.GROUP_NO equals M.GROUP_NO
                           join NBN in db.ITEM_BOOK.Where(x => x.ITEM_ID == "STOCK" & x.COL == "NEXT_BATCH_NO") on A.GROUP_NO equals NBN.GROUP_NO
                           where A.GROUP_NO == GroupNo
                           select new ItemBookStock
                           {
                               GroupNo = A.GROUP_NO,
                               Area = A.COL_VALUE,
                               Memo = M.COL_VALUE,
                               Next_Batch_No = NBN.COL_VALUE
                           };

                result = vObj.ToList();       
            }
            return result;
        }

        /// <summary>
        /// 股票名稱
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetStockName()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == "STOCK" & x.COL== "NAME")
                    .OrderBy(x => x.GROUP_NO)
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.GROUP_NO.ToString(),
                        Text = x.COL_VALUE
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 區域
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetAreaType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "STOCK_AREA")
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
        /// 類型
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetStockType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "STOCK_TYPE")
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
        public MSGReturnModel<IEnumerable<ITreaItem>> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData)
        {
            var result = new MSGReturnModel<IEnumerable<ITreaItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;
            try
            {
                if (insertDatas != null)
                {
                    var datas = (List<StockViewModel>)insertDatas;

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

                            string logStr = string.Empty; //log

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                            #region 申請單紀錄檔
                            var _TAR = new TREA_APLY_REC()
                            {
                                APLY_NO = $@"G6{qPreCode}{cId}", //申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                                APLY_FROM = AccessProjectStartupType.M.ToString(), //人工
                                ITEM_ID = taData.vItem, //申請項目
                                ACCESS_TYPE = taData.vAccessType, //存入(P) or 取出(G)
                                ACCESS_REASON = taData.vAccessReason, //申請原因
                                APLY_STATUS = AccessProjectFormStatus.A01.ToString(), //表單申請
                                EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(taData.vExpectedAccessDate), //預計存取日期
                                APLY_UNIT = taData.vAplyUnit, //申請單位
                                APLY_UID = taData.vAplyUid, //申請人
                                APLY_DT = dt,
                                CREATE_UID = taData.vCreateUid, //新增人
                                CREATE_DT = dt,
                                LAST_UPDATE_UID = taData.vCreateUid,
                                LAST_UPDATE_DT = dt
                            };
                            if (taData.vAplyUid != taData.vCreateUid) //當申請人不是新增人(代表為覆核單位代申請)
                            {
                                _TAR.CUSTODY_UID = taData.vCreateUid; //覆核單位直接帶 新增人
                                _TAR.CONFIRM_DT = dt;
                            }
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

                            #region 其它存取項目申請資料檔
                            int seq = 0;
                            datas.ForEach(x =>
                            {
                                var item_id = sysSeqDao.qrySeqNo("E7", qPreCode).ToString().PadLeft(8, '0');
                                seq += 1;
                                var _OIA = new OTHER_ITEM_APLY()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    DATA_SEQ = seq,
                                    ITEM_ID = $@"E7{item_id}"
                                };
                                db.OTHER_ITEM_APLY.Add(_OIA);
                                logStr += "|";
                                logStr += _OIA.modelToString();
                            });
                            #endregion

                            #region 存取項目冊號資料檔
                            //判斷存入資料
                            switch(datas[0].vStockDate.StockFeaturesType)
                            {
                                case "StockInsert"://新增股票
                                    break;
                                case "StockFromDB"://從資料庫選取股票
                                    break;
                                default:
                                    break;
                            }
                            #endregion

                            #region 股票申請資料檔
                            //int seq = 0;
                            //datas.ForEach(x =>
                            //{
                            //    var item_id = sysSeqDao.qrySeqNo("E2", qPreCode).ToString().PadLeft(8, '0');
                            //    seq += 1;
                            //    var _BNA = new BLANK_NOTE_APLY()
                            //    {
                            //        APLY_NO = _TAR.APLY_NO,
                            //        DATA_SEQ = seq,
                            //        ITEM_ID = $@"E2{item_id}",
                            //        CHECK_TYPE = x.vCheckType,
                            //        ISSUING_BANK = x.vIssuingBank,
                            //        CHECK_NO_TRACK = x.vCheckNoTrack,
                            //        CHECK_NO_B = x.vCheckNoB,
                            //        CHECK_NO_E = taData.vAccessType == AccessProjectTradeType.P.ToString() ? x.vCheckNoE : x.vTakeOutE,
                            //        ITEM_BLANK_NOTE_ITEM_ID = _ITEM_BLANK_NOTE_ITEM_ID
                            //    };
                            //    db.BLANK_NOTE_APLY.Add(_BNA);
                            //    logStr += "|";
                            //    logStr += _BNA.modelToString();
                            //});
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
                                    log.CFUNCTION = "申請覆核-新增股票";
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
                            #endregion
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
        /// <summary>
        /// 明細資料 ViewModel
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="num"></param>
        /// <param name="Inventory_types"></param>
        /// <param name="tempFlag"></param>
        /// <returns></returns>
        private StockViewModel BlankNoteAplyToBillViewModel(ITEM_STOCK data, string code, int num, List<SYS_CODE> Inventory_types, bool tempFlag)
        {
            return new StockViewModel()
            {
                //vRowNum = (num + 1).ToString(),
                //vItemId = data.ITEM_ID,
                //vAplyNo = data.APLY_NO,
                //vDataSeq = data.DATA_SEQ.ToString(),
                //vStatus = Inventory_types.FirstOrDefault(x => x.CODE == code)?.CODE_VALUE,
                //vIssuingBank = data.ISSUING_BANK,
                //vCheckType = data.CHECK_TYPE,
                //vCheckNoTrack = data.CHECK_NO_TRACK,
                //vCheckNoB = data.CHECK_NO_B,
                //vCheckNoE = data.CHECK_NO_E,
                //vCheckTotalNum = tempFlag ?
                //((code == "3") ?
                //(TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString() : string.Empty) :
                //(TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString(),
                //vTakeOutTotalNum = tempFlag ?
                //((code == "4") ?
                //(TypeTransfer.stringToInt(data.CHECK_NO_E) - TypeTransfer.stringToInt(data.CHECK_NO_B) + 1).ToString() : string.Empty) : null,
                //vTakeOutE = tempFlag ? ((code == "4") ? data.CHECK_NO_E : string.Empty) : null
            };
        }


        #endregion
    }
}