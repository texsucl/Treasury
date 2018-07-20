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
using static Treasury.Web.Enum.Ref;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;

namespace Treasury.Web.Service.Actual
{
    public class Estate : IEstate
    {
        protected INTRA intra { private set; get; }

        public Estate()
        {
            intra = new INTRA();
        }

        #region Get Date

        /// <summary>
        /// 抓取狀別 土地,建物,他項權利,其他
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetEstateFromNo()
        {
            List<SelectOption> result = new List<SelectOption>() {
                //new SelectOption() { Text = " ", Value = " " }
            };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(
                    db.SYS_CODE.AsNoTracking()
                    .Where(x=>x.CODE_TYPE == "ESTATE_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption() {
                        Value = x.CODE_VALUE,
                        Text = x.CODE_VALUE
                }));
            }
            return result;
        }

        /// <summary>
        /// 抓取大樓名稱
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        public List<SelectOption> GetBuildName(string vAplyUnit = null)
        {
            return getBuildNameorBookNo("BUILDING_NAME" , vAplyUnit);
        }

        /// <summary>
        /// 抓取冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        public List<SelectOption> GetBookNo(string vAplyUnit = null)
        {
            return getBuildNameorBookNo("BOOK_NO" , vAplyUnit );
        }

        /// <summary>
        /// 由group No 抓取 存取項目冊號資料檔
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        public EstateModel GetItemBook(int groupNo)
        {
            EstateModel result = new EstateModel();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var itemId = TreaItemType.D1014.ToString();
                result = getEstateModel(db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == itemId && x.GROUP_NO == groupNo).ToList());
            }
            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public EstateViewModel GetDataByAplyNo(string aplyNo)
        {
            var result = new EstateViewModel();
            //result.
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    result.vAplyNo = _TAR.APLY_NO;
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去不動產庫存資料檔抓取資料
                    var details = db.ITEM_REAL_ESTATE.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _groupNo = details.First().GROUP_NO;
                        result.vGroupNo = _groupNo.ToString();
                        //去存取項目冊號檔抓取冊號資料
                        var _ItemBooks = db.ITEM_BOOK.AsNoTracking()
                            .Where(x => x.GROUP_NO == _groupNo).ToList();
                        if (_ItemBooks.Any())
                        {
                            result.vItem_Book = getEstateModel(_ItemBooks);
                        }
                        var _code_type = SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        result.vDetail = getdetailModel(details, _Inventory_types).ToList();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 使用 存取項目冊號資料檔ITEM_BOOK 的 群組編號 抓取資料
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        public List<EstateDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit )
        {
            var result = new List<EstateDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var itemId = TreaItemType.D1014.ToString(); //不動產權狀
                var _ItemBooks = db.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO == groupNo && x.ITEM_ID == itemId).ToList();
                var _code_type = SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                result =
                    getdetailModel(db.ITEM_REAL_ESTATE.AsNoTracking()
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable(), _Inventory_types).ToList();
            }
            return result;
        }
        #endregion

        #region Save Data

        /// <summary>
        /// 申請覆核 不動產
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
                    var datas = (List<EstateViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        var _first = datas.First();
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log

                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            #region 申請單紀錄檔
                            var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

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
                            int groupUp = 1; //群組編號
                            bool insertGroupFlag = false;
                            if (taData.vAccessType == AccessProjectTradeType.P.ToString())
                            {
                                var _ItemBook = _first.vItem_Book;
                                var _BUILDING_NAME = _ItemBook.BUILDING_NAME?.Trim();
                                if (_ItemBook.BOOK_NO == null) //新增存取項目冊號資料檔
                                {
                                    if (db.ITEM_BOOK.Any(x => x.ITEM_ID == taData.vItem && x.COL == "BUILDING_NAME" && x.COL_VALUE == _BUILDING_NAME))
                                    {
                                        result.DESCRIPTION = "重複的大樓名稱";
                                        return result;
                                    }
                                    insertGroupFlag = true;
                                    var _itemBooks = db.ITEM_BOOK.Where(x => x.ITEM_ID == taData.vItem);

                                    if (_itemBooks.Any())
                                    {
                                        groupUp = _itemBooks.Max(x => x.GROUP_NO) + 1; //群組編號 + 1 = 新的冊號
                                    }
                                    _ItemBook.BOOK_NO = groupUp.ToString();
                                    foreach (var pro in _ItemBook.GetType().GetProperties())
                                    {
                                        db.ITEM_BOOK.Add(new ITEM_BOOK()
                                        {
                                            ITEM_ID = taData.vItem,
                                            GROUP_NO = groupUp,
                                            COL = pro.Name,
                                            COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                            COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim()
                                        });
                                    }
                                }
                                else //修改 存取項目冊號資料檔
                                {
                                    groupUp = TypeTransfer.stringToInt(_ItemBook.BOOK_NO);
                                    var _ItemBooks = db.ITEM_BOOK.Where(x => x.GROUP_NO == groupUp && x.ITEM_ID == taData.vItem).ToList();
                                    foreach (var pro in _ItemBook.GetType().GetProperties())
                                    {
                                        if (!(pro.Name == "BOOK_NO" || pro.Name == "BUILDING_NAME"))
                                        {
                                            var _chang = _ItemBooks.FirstOrDefault(x => x.COL == pro.Name);
                                            if (_chang != null)
                                                _chang.COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim();
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 不動產庫存資料檔
                            var details = _first.vDetail;
                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                            var _seq = 1;
                            //抓取有修改註記的
                            foreach (var item in details)
                            {
                                var _IRE_Item_Id = string.Empty;
                                if (taData.vAccessType == AccessProjectTradeType.P.ToString()) //存入
                                {
                                    //只抓取預約存入
                                    if (item.vStatus == AccessInventoryTyp._3.GetDescription())
                                    {
                                        var item_id = sysSeqDao.qrySeqNo("E3", qPreCode).ToString().PadLeft(8, '0');
                                        _IRE_Item_Id = item_id;
                                        var _IRE = new ITEM_REAL_ESTATE()
                                        {
                                            ITEM_ID = item_id, //物品編號
                                            INVENTORY_STATUS = "3", //預約存入
                                            GROUP_NO = groupUp, //群組編號
                                            ESTATE_FORM_NO = item.vEstate_From_No, //狀別
                                            ESTATE_DATE = item.vEstate_Date.TaiwanDateToDate(), //發狀日
                                            OWNERSHIP_CERT_NO = item.vOwnership_Cert_No, //字號
                                            LAND_BUILDING_NO = item.vLand_Building_No, // 地/建號
                                            HOUSE_NO = item.vHouse_No, //門牌號
                                            ESTATE_SEQ = item.vEstate_Seq, //流水號/編號
                                            MEMO = item.vMemo, //備註
                                            APLY_DEPT = _dept.Item1, //申請人部門
                                            APLY_SECT = _dept.Item2, //申請人科別
                                            APLY_UID = taData.vAplyUid, //申請人
                                            CHARGE_DEPT = _dept.Item1, //權責部門
                                            CHARGE_SECT = _dept.Item2, //權責科別
                                            //PUT_DATE = dt, //存入日期時間
                                            LAST_UPDATE_DT = dt, //最後修改時間
                                        };
                                        db.ITEM_REAL_ESTATE.Add(_IRE);
                                        logStr += "|";
                                        logStr += _IRE.modelToString();
                                    }
                                }
                                else if (taData.vAccessType == AccessProjectTradeType.G.ToString())//取出
                                {
                                    //只抓取預約取出
                                    if (item.vStatus == AccessInventoryTyp._4.GetDescription())
                                    {
                                        var _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        _IRE_Item_Id = _IRE.ITEM_ID;
                                        if (_IRE.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _IRE.INVENTORY_STATUS = "4"; //預約取出
                                        //_IRE.GET_DATE = dt; //取出日期時間
                                        _IRE.LAST_UPDATE_DT = dt;  //最後修改時間
                                    }
                                }


                                #region 其它存取項目申請資料檔
                                db.OTHER_ITEM_APLY.Add(
                                new OTHER_ITEM_APLY()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    //DATA_SEQ = _seq,
                                    ITEM_ID = _IRE_Item_Id
                                });
                                #endregion
                                _seq += 1;
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
                                    log.CFUNCTION = "申請覆核-新增不動產";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, _TAR.CREATE_UID);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    var addstr = insertGroupFlag ? (",新增冊號:" + groupUp.ToString()) : string.Empty;
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

        #region privateFunction

        /// <summary>
        /// 抓取大樓名稱或冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        private List<SelectOption> getBuildNameorBookNo(string parm,string vAplyUnit = null)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Value = " ", Text = " " } };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var groupNos = new List<int>();
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    groupNos = db.ITEM_REAL_ESTATE.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .Select(x => x.GROUP_NO).ToList();
                }
                var itemId = TreaItemType.D1014.ToString(); //不動產權狀
                if (!parm.IsNullOrWhiteSpace())
                {
                    result.AddRange(db.ITEM_BOOK.AsNoTracking()
                        .Where(x => x.ITEM_ID == itemId && x.COL == parm)
                        .Where(x => groupNos.Contains(x.GROUP_NO), !vAplyUnit.IsNullOrWhiteSpace())
                        .OrderBy(x => x.GROUP_NO)
                        .AsEnumerable().Select(x => new SelectOption()
                        {
                            Value = x.GROUP_NO.ToString(),
                            Text = parm == "BUILDING_NAME" ? $@"{x.COL_VALUE}(冊號:{x.GROUP_NO})" : x.COL_VALUE
                        }));
                }
            }
            return result;
        }

        private EstateModel getEstateModel(List<ITEM_BOOK> _ItemBooks)
        {
            EstateModel result = new EstateModel();
            if (_ItemBooks.Any())
            {
                result = new EstateModel()
                {
                    BOOK_NO = _ItemBooks.FirstOrDefault(x => x.COL == "BOOK_NO")?.COL_VALUE, //冊號
                    BUILDING_NAME = _ItemBooks.FirstOrDefault(x => x.COL == "BUILDING_NAME")?.COL_VALUE, //大樓名稱
                    LOCATED = _ItemBooks.FirstOrDefault(x => x.COL == "LOCATED")?.COL_VALUE, //坐落
                    MEMO = _ItemBooks.FirstOrDefault(x => x.COL == "MEMO")?.COL_VALUE //備註
                };
            }
            return result;
        }

        private IEnumerable<EstateDetailViewModel> getdetailModel(IEnumerable<ITEM_REAL_ESTATE> data,List<SYS_CODE> _Inventory_types)
        {
            return data.Select(x => new EstateDetailViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vGroupNo = x.GROUP_NO.ToString(), //群組編號
                vEstate_From_No = x.ESTATE_FORM_NO, //狀別
                vEstate_Date = x.ESTATE_DATE.DateToTaiwanDate(), //發狀日
                vOwnership_Cert_No = x.OWNERSHIP_CERT_NO, //字號
                vLand_Building_No = x.LAND_BUILDING_NO, //地/建號
                vHouse_No = x.HOUSE_NO, //門牌號
                vEstate_Seq = x.ESTATE_SEQ, //流水號/編號
                vMemo = x.MEMO, //備註
                vtakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        #endregion
    }
}