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
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Text = " ", Value = " " } };
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
        public List<SelectOption> GetBuildName(string vAplyUnit)
        {
            return getBuildNameorBookNo(vAplyUnit, "BUILDING_NAME");
        }

        /// <summary>
        /// 抓取冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        public List<SelectOption> GetBookNo(string vAplyUnit)
        {
            return getBuildNameorBookNo(vAplyUnit, "BOOK_NO");
        }

        /// <summary>
        /// 使用 存取項目冊號資料檔ITEM_BOOK 的 群組編號 抓取資料
        /// </summary>
        /// <param name="groupNo"></param>
        /// <returns></returns>
        public EstateViewModel GetData(int groupNo)
        {
            var result = new EstateViewModel();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _itemType = TreaItemType.ESTATE.ToString(); //不動產權狀
                var itemId = db.TREA_ITEM.AsNoTracking().FirstOrDefault(x => x.TREA_ITEM_TYPE == _itemType)?.ITEM_ID; //D1014
                var _ItemBooks = db.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO == groupNo && x.ITEM_ID == itemId).ToList();
                if (_ItemBooks.Any())
                {
                    result.vItem_Book = new EstateModel()
                    {
                        BOOK_NO = _ItemBooks.FirstOrDefault(x => x.COL == "BOOK_NO")?.COL_VALUE, //冊號
                        BUILDING_NAME = _ItemBooks.FirstOrDefault(x => x.COL == "BUILDING_NAME")?.COL_VALUE, //大樓名稱
                        LOCATED = _ItemBooks.FirstOrDefault(x => x.COL == "LOCATED")?.COL_VALUE, //坐落
                        MEMO = _ItemBooks.FirstOrDefault(x => x.COL == "MEMO")?.COL_VALUE //備註
                    };
                }
                var _code_type = SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                result.vDetail = db.ITEM_REAL_ESTATE.AsNoTracking().Where(x => x.GROUP_NO == groupNo)
                    .AsEnumerable().Select(x => new EstateDetailViewModel() {
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
                        changeFlag = false, //修改註記
                        vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
                    }).ToList();
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

                            #region 其它存取項目申請資料檔

                            #endregion

                            #region 存取項目冊號資料檔
                            var _ItemBook = _first.vItem_Book;
                            var _itemType = TreaItemType.ESTATE.ToString(); //不動產權狀
                            var itemId = db.TREA_ITEM.AsNoTracking().FirstOrDefault(x => x.TREA_ITEM_TYPE == _itemType)?.ITEM_ID; //D1014                           
                            var _BUILDING_NAME = _ItemBook.BUILDING_NAME?.Trim();
                            if (db.ITEM_BOOK.Any(x => x.ITEM_ID == itemId && x.COL == "BUILDING_NAME" && x.COL_VALUE == _BUILDING_NAME))
                            {
                                result.DESCRIPTION = "重複的大樓名稱";
                                return result;
                            }
                            int groupUp = 1; //群組編號
                            if (_ItemBook.BOOK_NO == null) //新增存取項目冊號資料檔
                            {
                                var _itemBooks = db.ITEM_BOOK.Where(x => x.ITEM_ID == itemId);
                                
                                if (_itemBooks.Any())
                                {
                                    groupUp = _itemBooks.Max(x => x.GROUP_NO) + 1; //群組編號 + 1 = 新的冊號
                                }
                                _ItemBook.BOOK_NO = groupUp.ToString();
                                foreach (var pro in _ItemBook.GetType().GetProperties())
                                {
                                    db.ITEM_BOOK.Add(new ITEM_BOOK()
                                    {
                                        ITEM_ID = itemId,
                                        GROUP_NO = groupUp,
                                        COL = pro.Name,
                                        COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                        COL_VALUE  = pro.GetValue(_ItemBook)?.ToString()?.Trim()
                                    });
                                }                              
                            }
                            else //修改 存取項目冊號資料檔
                            {
                                groupUp = TypeTransfer.stringToInt(_ItemBook.BOOK_NO);
                                var _ItemBooks = db.ITEM_BOOK.Where(x => x.GROUP_NO == groupUp && x.ITEM_ID == itemId).ToList();
                                foreach (var pro in _ItemBook.GetType().GetProperties())
                                {
                                    var _chang = _ItemBooks.FirstOrDefault(x => x.COL == pro.Name);
                                    if (_chang != null)
                                        _chang.COL_VALUE = pro.GetValue(_ItemBook).ToString()?.Trim();
                                }
                            }
                            #endregion

                            #region 不動產庫存資料檔
                            var details = _first.vDetail;
                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                            foreach (var item in details)
                            {
                                if (item.changeFlag) //修改資料
                                {
                                    var _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                    if (_IRE.LAST_UPDATE_DT > item.vLast_Update_Time)
                                    {
                                        result.DESCRIPTION = MessageType.already_Change.GetDescription();
                                        return result;
                                    }
                                    if (_IRE != null)
                                    {
                                        _IRE.ESTATE_FORM_NO = item.vEstate_From_No; //狀別
                                        _IRE.ESTATE_DATE = item.vEstate_Date.TaiwanDateToDate(); //發狀日
                                        _IRE.OWNERSHIP_CERT_NO = item.vOwnership_Cert_No; //字號
                                        _IRE.LAND_BUILDING_NO = item.vLand_Building_No; // 地/建號
                                        _IRE.HOUSE_NO = item.vHouse_No; //門牌號
                                        _IRE.ESTATE_SEQ = item.vEstate_Seq; //流水號/編號
                                        _IRE.MEMO = item.vMemo; //備註
                                        //_IRE.APLY_DEPT = _dept.Item1; //申請人部門
                                        //_IRE.APLY_SECT = _dept.Item2; //申請人科別
                                        //_IRE.APLY_UID = taData.vAplyUid; //申請人
                                        _IRE.LAST_UPDATE_DT = dt; //最後修改時間
                                    }
                                }
                                else
                                {
                                    //預約存入
                                    if (item.vStatus == AccessInventoryTyp._3.GetDescription())
                                    {
                                        var item_id = sysSeqDao.qrySeqNo("E3", qPreCode).ToString().PadLeft(3, '0');
                                        db.ITEM_REAL_ESTATE.Add(new ITEM_REAL_ESTATE()
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
                                        });
                                    }
                                    //預約取出
                                    else if (item.vStatus == AccessInventoryTyp._4.GetDescription())
                                    {
                                        var _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
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

        #region privateFunction

        private List<SelectOption> getBuildNameorBookNo(string vAplyUnit, string parm)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Value = " ", Text = " " } };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var groupNos = db.ITEM_REAL_ESTATE.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Select(x => x.GROUP_NO).ToList();
                var _itemType = TreaItemType.ESTATE.ToString(); //不動產權狀
                var itemId = db.TREA_ITEM.AsNoTracking().FirstOrDefault(x => x.TREA_ITEM_TYPE == _itemType)?.ITEM_ID; //D1014
                if (itemId != null && !parm.IsNullOrWhiteSpace())
                {
                    result.AddRange(db.ITEM_BOOK.AsNoTracking()
                        .Where(x => x.ITEM_ID == itemId && groupNos.Contains(x.GROUP_NO) && x.COL == parm)
                        .OrderBy(x => x.GROUP_NO)
                        .AsEnumerable().Select(x => new SelectOption()
                        {
                            Value = x.GROUP_NO.ToString(),
                            Text = x.COL_VALUE
                        }));
                }
            }
            return result;
        }

        #endregion
    }
}