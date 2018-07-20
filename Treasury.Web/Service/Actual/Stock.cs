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
using System.ComponentModel;

namespace Treasury.Web.Service.Actual
{
    public class Stock : IStock
    {
        protected INTRA intra { private set; get; }

        public Stock()
        {
            intra = new INTRA();
        }
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
                    .Where(x => x.ITEM_ID == TreaItemType.D1015.ToString())
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
                var vObj = from A in db.ITEM_BOOK.Where(x => x.ITEM_ID == TreaItemType.D1015.ToString() & x.COL == "AREA")
                           join M in db.ITEM_BOOK.Where(x => x.ITEM_ID == TreaItemType.D1015.ToString() & x.COL == "MEMO") on A.GROUP_NO equals M.GROUP_NO
                           join NBN in db.ITEM_BOOK.Where(x => x.ITEM_ID == TreaItemType.D1015.ToString() & x.COL == "NEXT_BATCH_NO") on A.GROUP_NO equals NBN.GROUP_NO
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
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        public List<SelectOption> GetStockName(string vAplyUnit = null)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Value = " ", Text = " " } };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var groupNos = new List<int>();
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    groupNos = db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .Select(x => x.GROUP_NO).ToList();
                }

                result.AddRange(db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == TreaItemType.D1015.ToString() && x.COL == "NAME")
                    .Where(x => groupNos.Contains(x.GROUP_NO), !vAplyUnit.IsNullOrWhiteSpace())
                    .OrderBy(x => x.GROUP_NO)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
                        Value = x.GROUP_NO.ToString(),
                        Text = x.COL_VALUE
                    }));
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

        /// <summary>
        /// 使用 群組編號 抓取在庫股票資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="vAplyUnit">申請部門</param>
        /// <returns></returns>
        public List<StockViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit)
        {
            var result = new List<StockViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var _emply = intra.getEmply();   //抓取員工資料

                result =
                    getMainModel(db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable(), _emply).ToList();
            }

            return result;
        }

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取在庫股票明細資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        public List<StockViewModel> GetDetailData(int groupNo, int treaBatchNo)
        {
            var result = new List<StockViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result =
                    getdetailModel(db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.TREA_BATCH_NO == treaBatchNo)
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()).ToList();
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
                        datas = datas.Where(x => x.vStatus == AccessInventoryType._4.GetDescription()).ToList();
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

                            #region 存取項目冊號資料檔
                            var _first = datas.First();
                            var _StockModel = _first.vStockModel;
                            bool insertGroupFlag = false;
                            //判斷申請作業
                            if (taData.vAccessType == AccessProjectTradeType.P.ToString())
                            {
                                //判斷存入資料
                                switch (_first.vStockDate.StockFeaturesType)
                                {
                                    case "StockInsert"://新增股票
                                        _first.vStockDate.GroupNo = GetMaxStockNo() + 1;
                                        foreach (var pro in _StockModel.GetType().GetProperties())
                                        {
                                            db.ITEM_BOOK.Add(new ITEM_BOOK()
                                            {
                                                ITEM_ID = taData.vItem,
                                                GROUP_NO = _first.vStockDate.GroupNo,
                                                COL = pro.Name,
                                                COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                                COL_VALUE = pro.GetValue(_StockModel)?.ToString()?.Trim()
                                            });
                                        }
                                        insertGroupFlag = true;
                                        break;
                                    case "StockFromDB"://從資料庫選取股票
                                        var _ItemBooks = db.ITEM_BOOK.Where(x => x.GROUP_NO == _first.vStockDate.GroupNo && x.ITEM_ID == taData.vItem).ToList();
                                        foreach (var pro in _StockModel.GetType().GetProperties())
                                        {
                                            if (!(pro.Name == "NAME"))
                                            {
                                                var _chang = _ItemBooks.FirstOrDefault(x => x.COL == pro.Name);
                                                if (_chang != null)
                                                    _chang.COL_VALUE = pro.GetValue(_StockModel)?.ToString()?.Trim();
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            #endregion

                            #region 儲存資料
                            //判斷申請作業-存入
                            if (taData.vAccessType == AccessProjectTradeType.P.ToString())
                            {
                                datas.ForEach(x =>
                                {
                                    var item_id = sysSeqDao.qrySeqNo("E7", string.Empty).ToString().PadLeft(8, '0');

                                    #region 其它存取項目申請資料檔
                                    var _OIA = new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = _TAR.APLY_NO,
                                        ITEM_ID = $@"E7{item_id}"
                                    };
                                    db.OTHER_ITEM_APLY.Add(_OIA);
                                    logStr += "|";
                                    logStr += _OIA.modelToString();
                                    #endregion

                                    #region 股票申請資料檔
                                    var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                    var _IS = new ITEM_STOCK()
                                    {
                                        ITEM_ID = $@"E7{item_id}", //物品編號
                                        INVENTORY_STATUS = "3", //預約存入
                                        GROUP_NO = _first.vStockDate.GroupNo, //群組編號
                                        TREA_BATCH_NO = int.Parse(_first.vStockDate.Next_Batch_No),  //入庫批號
                                        STOCK_TYPE = x.vStockType,    //股票類型
                                        STOCK_NO_PREAMBLE = x.vStockNoPreamble,   //序號前置碼
                                        STOCK_NO_B = x.vStockNoB.ToString(),  //股票序號(起)
                                        STOCK_NO_E = x.vStockNoE.ToString(),  //股票序號(迄)
                                        STOCK_CNT = x.vStockTotal,    //股票張數
                                        DENOMINATION = x.vDenomination,   //面額
                                        NUMBER_OF_SHARES = x.vNumberOfShares,   //股數
                                        MEMO = x.vMemo,//備註
                                        APLY_DEPT = _dept.Item1, //申請人部門
                                        APLY_SECT = _dept.Item2, //申請人科別
                                        APLY_UID = taData.vAplyUid, //申請人
                                        CHARGE_DEPT = _dept.Item1, //權責部門
                                        CHARGE_SECT = _dept.Item2, //權責科別
                                        PUT_DATE = dt, //存入日期時間
                                        LAST_UPDATE_DT = dt, //最後修改時間
                                    };
                                    db.ITEM_STOCK.Add(_IS);
                                    logStr += "|";
                                    logStr += _IS.modelToString();
                                    #endregion
                                });
                            }
                            //判斷申請作業-取出
                            if (taData.vAccessType == AccessProjectTradeType.G.ToString())
                            {
                                foreach(var item in datas)
                                {
                                    //取得股票明細資料
                                    var StockDetail = db.ITEM_STOCK.AsNoTracking()
                                                        .Where(x => x.GROUP_NO == item.vStockDate.GroupNo)
                                                        .Where(x => x.TREA_BATCH_NO == item.vTreaBatchNo)
                                                        .Where(x => x.INVENTORY_STATUS == "1") //庫存
                                                        .AsEnumerable().ToList();

                                    foreach(var detail in StockDetail)
                                    {
                                        #region 股票申請資料檔
                                        var _IS = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == detail.ITEM_ID);
                                        if (_IS.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _IS.INVENTORY_STATUS = "4"; //預約取出
                                        _IS.GET_DATE = dt;  //取出日期時間
                                        _IS.LAST_UPDATE_DT = dt;  //最後修改時間
                                        #endregion

                                        #region 其它存取項目申請資料檔
                                        var _OIA = new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = detail.ITEM_ID
                                        };
                                        db.OTHER_ITEM_APLY.Add(_OIA);
                                        logStr += "|";
                                        logStr += _OIA.modelToString();
                                        #endregion
                                    }
                                }
                            }
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
                                    var addstr = insertGroupFlag ? (",新增股票:" + _first.vStockDate.Name) : string.Empty;
                                    result.DESCRIPTION = MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}{addstr}");
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

        /// <summary>
        /// 在庫股票主檔資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_emply"></param>
        /// <returns></returns>
        private IEnumerable<StockViewModel> getMainModel(IEnumerable<ITEM_STOCK> data, List<V_EMPLY2> _emply)
        {
            data = data.Distinct(new ItemStockComparer());
            return data.Select(x => new StockViewModel()
            {
                vTakeoutFlag = false, //取出註記
                vTreaBatchNo = x.TREA_BATCH_NO,   //入庫批號
                vPutDate = string.IsNullOrEmpty(x.PUT_DATE.ToString()) ? "" : DateTime.Parse(x.PUT_DATE.ToString()).ToString("yyyy/MM/dd"),    //申請日期
                vAplyName = _emply.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME,   //申請人
                vNumberOfSharesTotal = GetNumberOfSharesTotal(x.GROUP_NO, x.TREA_BATCH_NO), //總股數
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        /// <summary>
        /// 在庫股票明細資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_emply"></param>
        /// <returns></returns>
        private IEnumerable<StockViewModel> getdetailModel(IEnumerable<ITEM_STOCK> data)
        {
            return data.Select(x => new StockViewModel()
            {
                vTreaBatchNo = x.TREA_BATCH_NO,   //入庫批號
                vStockType = x.STOCK_TYPE,    //類型
                vStockNoPreamble = x.STOCK_NO_PREAMBLE,   //序號前置碼
                vStockNoB = int.Parse(x.STOCK_NO_B),  //序號(起)
                vStockNoE = int.Parse(x.STOCK_NO_E),  //序號(迄)
                vStockTotal = x.STOCK_CNT,    //張數
                vDenomination = x.DENOMINATION,   //單張面額
                vDenominationTotal = x.STOCK_CNT * x.DENOMINATION,  //面額小計
                vNumberOfShares = x.NUMBER_OF_SHARES,   //股數
                vMemo = x.MEMO  //備註說明
            });
        }

        //在庫股票Distinct
        class ItemStockComparer : IEqualityComparer<ITEM_STOCK>
        {
            public bool Equals(ITEM_STOCK x, ITEM_STOCK y)
            {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.TREA_BATCH_NO == y.TREA_BATCH_NO && x.PUT_DATE == y.PUT_DATE && x.APLY_UID == y.APLY_UID;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(ITEM_STOCK ItemStock)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(ItemStock, null)) return 0;

                //Get hash code for the TreaBatchNo field.
                int hashItemStockTreaBatchNo = ItemStock.TREA_BATCH_NO.GetHashCode();

                //Get hash code for the PutDate field if it is not null.
                int hashItemStockPutDate = ItemStock.PUT_DATE == null ? 0 : ItemStock.PUT_DATE.GetHashCode();

                //Get hash code for the AplyUid field if it is not null.
                int hashItemStockAplyUid = ItemStock.APLY_UID == null ? 0 : ItemStock.APLY_UID.GetHashCode();

                //Calculate the hash code for the product.
                return hashItemStockTreaBatchNo ^ hashItemStockPutDate ^ hashItemStockAplyUid;
            }
        }

        //計算總股數
        private int GetNumberOfSharesTotal(int GroupNo, int TreaBatchNo)
        {
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var NumberOfSharesTotal= db.ITEM_STOCK.AsNoTracking()
                    .Where(x => x.GROUP_NO == GroupNo)
                    .Where(x => x.TREA_BATCH_NO == TreaBatchNo)
                    .Sum(x => x.NUMBER_OF_SHARES);

                return (int)NumberOfSharesTotal;
            }
        }
        #endregion
    }
}