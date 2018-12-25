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
using Treasury.Web.Controllers;
/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 不動產權狀
/// 初版作者：20180628 張家華
/// 修改歷程：20180628 張家華 
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
    public class Estate : Common, IEstate
    {

        public Estate()
        {
        }

        #region Get Date

        /// <summary>
        /// 抓取狀別 土地,建物,他項權利,其他
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetEstateFromNo()
        {
            List<SelectOption> result = new List<SelectOption>()
            {
                //new SelectOption() { Text = " ", Value = " " }
            };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(
                    db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "ESTATE_TYPE")
                    .OrderBy(x => x.ISORTBY)
                    .AsEnumerable().Select(x => new SelectOption()
                    {
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
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<SelectOption> GetBuildName(string vAplyUnit = null, string aplyNo = null)
        {
            return GetBuildNameorBookNo("BUILDING_NAME", vAplyUnit, aplyNo);
        }

        /// <summary>
        /// 抓取冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public List<SelectOption> GetBookNo(string vAplyUnit = null, string aplyNo = null)
        {
            return GetBuildNameorBookNo("BOOK_NO", vAplyUnit, aplyNo);
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
                var itemId = Ref.TreaItemType.D1014.ToString();
                result = GetEstateModel(db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == itemId && x.GROUP_NO == groupNo).ToList());
            }
            return result;
        }

        /// <summary>
        /// 對資料庫進行大樓名稱的模糊比對
        /// </summary>
        /// <param name="building_Name"></param>
        /// <returns></returns>
        public MSGReturnModel<string> GetCheckItemBook(string building_Name)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
            string str = string.Empty;
            if (building_Name.IsNullOrWhiteSpace())
                return result;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                building_Name = building_Name.Replace("大樓", string.Empty);
                var _item_Id = Ref.TreaItemType.D1014.ToString();
                var same = db.ITEM_BOOK.AsNoTracking().Where(x =>
                x.COL == "BUILDING_NAME" &&
                x.ITEM_ID == _item_Id &&
                x.COL_VALUE.Contains(building_Name)
                ).OrderBy(x => x.GROUP_NO)
                .AsEnumerable()
                .Select(x => $@"大樓名稱:{x.COL_VALUE},冊號:{x.GROUP_NO}")
                .ToList();
                if (same.Any())
                {
                    str += $@"比對結果:<br/>";
                    str += string.Join("<br/>", same);
                }
                result.RETURN_FLAG = true;
                result.DESCRIPTION = "沒有類似的大樓名稱!";
                result.Datas = str;
            }
            return result;
        }

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <param name="EditFlag">修改狀態:可修改取出需加入庫存資料</param>
        /// <returns></returns>
        public EstateViewModel GetDataByAplyNo(string aplyNo, bool EditFlag = false)
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
                            result.vItem_Book = GetEstateModel(_ItemBooks);
                        }
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        if (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString())//存入
                        {
                            bool _accessStatus = _TAR.APLY_STATUS == Ref.AccessProjectFormStatus.E01.ToString();//申請單號內的內容要惟持原始申請內容,不要因資料查詢異動作業-主畫面有異動資料而異動

                            result.vDetail = GetDetailModel(details, _Inventory_types, false, _accessStatus).ToList();
                        }
                        else if (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.G.ToString()) //取出
                        {
                            var _vDetail = GetDetailModel(details, _Inventory_types, true).ToList();
                            if (EditFlag) //可以修改時需加入庫存資料
                            {
                                var dept = intra.getDept(_TAR.APLY_UNIT); //抓取單位
                                _vDetail.AddRange( //加入庫存資料
                                       GetDetailModel(db.ITEM_REAL_ESTATE.AsNoTracking()
                                       .Where(x => !OIAs.Contains(x.ITEM_ID))
                                       .Where(x => x.GROUP_NO == _groupNo)
                                       .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                                       .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                                       .Where(x => x.INVENTORY_STATUS == "1") //庫存
                                       .AsEnumerable(), _Inventory_types));
                            }
                            result.vDetail = _vDetail.OrderBy(x => x.vItemId).ToList();
                        }

                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 使用 存取項目冊號資料檔ITEM_BOOK 的 群組編號 抓取資料
        /// </summary>
        /// <param name="groupNo">群組號</param>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public List<EstateDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit, string aplyNo = null)
        {
            var result = new List<EstateDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                //var itemId = TreaItemType.D1014.ToString(); //不動產權狀
                //var _ItemBooks = db.ITEM_BOOK.AsNoTracking().Where(x => x.GROUP_NO == groupNo && x.ITEM_ID == itemId).ToList();
                var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                List<string> itemIds = new List<string>();
                if (!aplyNo.IsNullOrWhiteSpace()) //有單號須加單號資料
                {
                    itemIds = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == aplyNo).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(GetDetailModel(db.ITEM_REAL_ESTATE.Where(x =>
                    x.GROUP_NO == groupNo &&
                    itemIds.Contains(x.ITEM_ID)).AsEnumerable(), _Inventory_types));
                }
                result.AddRange(
                    GetDetailModel(db.ITEM_REAL_ESTATE.AsNoTracking()
                    .Where(x => !itemIds.Contains(x.ITEM_ID), !aplyNo.IsNullOrWhiteSpace())
                    .Where(x => x.GROUP_NO == groupNo)
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable(), _Inventory_types));
            }
            result = result.OrderBy(x => x.vItemId).ToList();
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
            List<CDCEstateViewModel> result = new List<CDCEstateViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                var _item_Id = Ref.TreaItemType.D1014.ToString();
                var _Item_Book = db.ITEM_BOOK.AsNoTracking()
                    .Where(x => x.ITEM_ID == _item_Id).ToList();
                if (aply_No.IsNullOrWhiteSpace())
                {
                    var PUT_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_From);
                    var PUT_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_DT_To).DateToLatestTime();
                    var GET_DATE_From = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_From);
                    var GET_DATE_To = TypeTransfer.stringToDateTimeN(searchModel.vAPLY_ODT_To).DateToLatestTime();

                    var _Estate_Form_No_Name = db.SYS_CODE
                        .Where(x => x.CODE_TYPE == "ESTATE_TYPE")
                        .Where(x => x.CODE == searchModel.vEstate_Form_No)
                        .Select(x => x.CODE_VALUE).FirstOrDefault();
                    var Group_No = string.IsNullOrEmpty(searchModel.vBookNo) ? 0 : int.Parse(searchModel.vBookNo);

                    result.AddRange(db.ITEM_REAL_ESTATE.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.GROUP_NO == Group_No, searchModel.vBookNo != null)
                        .Where(x => x.ESTATE_FORM_NO == _Estate_Form_No_Name, searchModel.vEstate_Form_No != "All")
                        .Where(x => x.CHARGE_DEPT == charge_Dept, !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect, !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select((x) => new CDCEstateViewModel()
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
                            vIB_Book_No = x.GROUP_NO.ToString(),
                            vIB_Building_Name = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "BUILDING_NAME")?.COL_VALUE,
                            vIB_Located = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "LOCATED")?.COL_VALUE,
                            vIB_Memo = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "MEMO")?.COL_VALUE,
                            vEstate_Form_No = x.ESTATE_FORM_NO,
                            vEstate_Form_No_Aft = x.ESTATE_FORM_NO_AFT,
                            vEstate_Date = x.ESTATE_DATE?.ToString("yyyy/MM/dd"),
                            vEstate_Date_Aft = x.ESTATE_DATE_AFT?.ToString("yyyy/MM/dd"),
                            vOwnership_Cert_No = x.OWNERSHIP_CERT_NO,
                            vOwnership_Cert_No_Aft = x.OWNERSHIP_CERT_NO_AFT,
                            vLand_Building_No = x.LAND_BUILDING_NO,
                            vLand_Building_No_Aft = x.LAND_BUILDING_NO_AFT,
                            vHouse_No = x.HOUSE_NO,
                            vHouse_No_Aft = x.HOUSE_NO_AFT,
                            vEstate_Seq = x.ESTATE_SEQ,
                            vEstate_Seq_Aft = x.ESTATE_SEQ_AFT,
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());

                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vItemId).ToList();
                            var uids = GetAplyUidName(itemIds);
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
                    result.AddRange(db.ITEM_REAL_ESTATE.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCEstateViewModel()
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
                            vIB_Book_No = x.GROUP_NO.ToString(),
                            vIB_Building_Name = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "BUILDING_NAME")?.COL_VALUE,
                            vIB_Located = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "LOCATED")?.COL_VALUE,
                            vIB_Memo = _Item_Book.FirstOrDefault(y => y.GROUP_NO == x.GROUP_NO && y.COL == "MEMO")?.COL_VALUE,
                            vEstate_Form_No = x.ESTATE_FORM_NO,
                            vEstate_Form_No_Aft = x.ESTATE_FORM_NO_AFT,
                            vEstate_Date = x.ESTATE_DATE?.ToString("yyyy/MM/dd"),
                            vEstate_Date_Aft = x.ESTATE_DATE_AFT?.ToString("yyyy/MM/dd"),
                            vOwnership_Cert_No = x.OWNERSHIP_CERT_NO,
                            vOwnership_Cert_No_Aft = x.OWNERSHIP_CERT_NO_AFT,
                            vLand_Building_No = x.LAND_BUILDING_NO,
                            vLand_Building_No_Aft = x.LAND_BUILDING_NO_AFT,
                            vHouse_No = x.HOUSE_NO,
                            vHouse_No_Aft = x.HOUSE_NO_AFT,
                            vEstate_Seq = x.ESTATE_SEQ,
                            vEstate_Seq_Aft = x.ESTATE_SEQ_AFT,
                            vMemo = x.MEMO,
                            vMemo_Aft = x.MEMO_AFT,
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

                        if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                        {
                            if (!_first.vDetail.Any())
                            {
                                result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                                return result;
                            }
                        }
                        else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())
                        {
                            if (!_first.vDetail.Any(x => x.vtakeoutFlag))
                            {
                                result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                                return result;
                            }
                        }

                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            var _TAR = new TREA_APLY_REC(); //申請單號
                            bool insertGroupFlag = false; //是否為新增冊號
                            int groupUp = 1; //群組編號
                            var item_Seq = "E3"; //不動產權狀流水號開頭編碼    

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
                                db.APLY_REC_HIS.Add(
                                new APLY_REC_HIS()
                                {
                                    APLY_NO = _TAR.APLY_NO,
                                    APLY_STATUS = _APLY_STATUS,
                                    PROC_UID = _TAR.CREATE_UID,
                                    PROC_DT = dt
                                });
                                #endregion

                                #region 存取項目冊號資料檔

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
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
                                            var _IB = new ITEM_BOOK()
                                            {
                                                ITEM_ID = taData.vItem,
                                                GROUP_NO = groupUp,
                                                COL = pro.Name,
                                                COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                                COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim()
                                            };
                                            logStr += _IB.modelToString(logStr);
                                            db.ITEM_BOOK.Add(_IB);
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
                                                {
                                                    _chang.COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim();
                                                    logStr += _chang.modelToString(logStr);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region 不動產庫存資料檔
                                var details = _first.vDetail;
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);


                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_REAL_ESTATE> inserts = new List<ITEM_REAL_ESTATE>(); //新增資料

                                foreach (var item in details)
                                {
                                    var _IRE_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var _IRE = new ITEM_REAL_ESTATE();
                                            if (item.vItemId.StartsWith(item_Seq) && item.vItemId.Length == 10)
                                            {
                                                _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                                if (_IRE.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IRE.GROUP_NO = groupUp; //群組編號
                                                _IRE.ESTATE_FORM_NO = item.vEstate_From_No; //狀別
                                                _IRE.ESTATE_DATE = TypeTransfer.stringToDateTimeN(item.vEstate_Date); //發狀日
                                                _IRE.OWNERSHIP_CERT_NO = item.vOwnership_Cert_No; //字號
                                                _IRE.LAND_BUILDING_NO = item.vLand_Building_No; // 地/建號
                                                _IRE.HOUSE_NO = item.vHouse_No; //門牌號
                                                _IRE.ESTATE_SEQ = item.vEstate_Seq; //流水號/編號
                                                _IRE.MEMO = item.vMemo; //備註
                                                _IRE.LAST_UPDATE_DT = dt;

                                                updateItemIds.Add(item.vItemId);

                                                logStr += _IRE.modelToString(logStr);
                                            }
                                            else
                                            {
                                                var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                                _IRE = new ITEM_REAL_ESTATE()
                                                {
                                                    ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    GROUP_NO = groupUp, //群組編號
                                                    ESTATE_FORM_NO = item.vEstate_From_No, //狀別
                                                    ESTATE_DATE = TypeTransfer.stringToDateTimeN(item.vEstate_Date), //發狀日
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
                                                _IRE_Item_Id = _IRE.ITEM_ID;
                                                inserts.Add(_IRE);
                                            }
                                            logStr += _IRE.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                    {
                                        var _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                        _IRE_Item_Id = _IRE.ITEM_ID;
                                        if (_IRE.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出 
                                        if (item.vtakeoutFlag)
                                        {
                                            if (_IRE.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _IRE.INVENTORY_STATUS = "4"; //改為預約取出
                                                _IRE.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IRE.ITEM_ID);
                                                logStr += _IRE.modelToString(logStr);
                                            }
                                            else if (_IRE.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_IRE.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IRE.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                _IRE.INVENTORY_STATUS = "1"; //改為在庫
                                                _IRE.LAST_UPDATE_DT = dt;  //最後修改時間
                                                logStr += _IRE.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_REAL_ESTATE.RemoveRange(db.ITEM_REAL_ESTATE.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_REAL_ESTATE.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
                                {
                                    foreach (var backItemId in db.OTHER_ITEM_APLY.Where(x =>
                                     x.APLY_NO == taData.vAplyNo &&
                                     !updateItemIds.Contains(x.ITEM_ID)
                                    ).Select(x => x.ITEM_ID))
                                    {
                                        var back = db.ITEM_REAL_ESTATE.First(x => x.ITEM_ID == backItemId);
                                        back.INVENTORY_STATUS = "1";
                                    }

                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(updateItemIds.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x
                                    }));
                                }

                                #endregion

                            }
                            else //新增申請單
                            {
                                #region 申請單紀錄檔 & 申請單歷程檔

                                var data = SaveTREA_APLY_REC(db, taData, logStr, dt);
                                _TAR.APLY_NO = data.Item1;
                                logStr = data.Item2;

                                #endregion

                                #region 存取項目冊號資料檔

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
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
                                            var _IB = new ITEM_BOOK()
                                            {
                                                ITEM_ID = taData.vItem,
                                                GROUP_NO = groupUp,
                                                COL = pro.Name,
                                                COL_NAME = (pro.GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute).Description,
                                                COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim()
                                            };
                                            logStr += _IB.modelToString(logStr);
                                            db.ITEM_BOOK.Add(_IB);
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
                                                {
                                                    _chang.COL_VALUE = pro.GetValue(_ItemBook)?.ToString()?.Trim();
                                                    logStr += _chang.modelToString(logStr);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region 不動產庫存資料檔
                                var details = _first.vDetail;
                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                //抓取有修改註記的
                                foreach (var item in details)
                                {
                                    var _IRE_Item_Id = string.Empty;
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            var item_id = sysSeqDao.qrySeqNo(item_Seq, string.Empty).ToString().PadLeft(8, '0');
                                            var _IRE = new ITEM_REAL_ESTATE()
                                            {
                                                ITEM_ID = $@"{item_Seq}{item_id}", //物品編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                GROUP_NO = groupUp, //群組編號
                                                ESTATE_FORM_NO = item.vEstate_From_No, //狀別
                                                ESTATE_DATE = TypeTransfer.stringToDateTimeN(item.vEstate_Date), //發狀日
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
                                            _IRE_Item_Id = _IRE.ITEM_ID;
                                            db.ITEM_REAL_ESTATE.Add(_IRE);
                                            logStr += _IRE.modelToString(logStr);
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString())//取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            var _IRE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == item.vItemId);
                                            _IRE_Item_Id = _IRE.ITEM_ID;
                                            if (_IRE.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IRE.INVENTORY_STATUS = "4"; //預約取出
                                                                         //_IRE.GET_DATE = dt; //取出日期時間
                                            _IRE.LAST_UPDATE_DT = dt;  //最後修改時間
                                            logStr += _IRE.modelToString(logStr);
                                        }
                                    }


                                    #region 其它存取項目申請資料檔
                                    if (!_IRE_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IRE_Item_Id
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
                                    log.CFUNCTION = "申請覆核-新增不動產";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, taData.vCreateUid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    var addstr = insertGroupFlag ? (",新增冊號:" + groupUp.ToString()) : string.Empty;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_TAR.APLY_NO}{addstr}");
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態不動產權狀庫存資料檔要復原
            {
                foreach (var item in db.ITEM_REAL_ESTATE.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態改為已取消
            {
                foreach (var item in db.ITEM_REAL_ESTATE.Where(x => itemIds.Contains(x.ITEM_ID)))
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
            if (access_Type == Ref.AccessProjectTradeType.G.ToString()) //取出狀態不動產權狀庫存資料檔要復原 , 並刪除其它存取項目申請資料檔
            {
                foreach (var item in db.ITEM_REAL_ESTATE.Where(x => itemIds.Contains(x.ITEM_ID)))
                {
                    item.INVENTORY_STATUS = "1"; //復原為在庫
                    item.LAST_UPDATE_DT = dt;
                    logStr += item.modelToString(logStr);
                }
                db.OTHER_ITEM_APLY.RemoveRange(otherItemAplys);
                return new Tuple<bool, string>(true, logStr);
            }
            else //存入狀態 刪除不動產權狀庫存資料檔,其它存取項目申請資料檔
            {
                db.ITEM_REAL_ESTATE.RemoveRange(db.ITEM_REAL_ESTATE.Where(x => itemIds.Contains(x.ITEM_ID)));
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
                foreach (CDCEstateViewModel model in saveData)
                {
                    var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == model.vItemId);
                    if (_Estate != null && !changFlag)
                    {
                        if (_Estate.LAST_UPDATE_DT > model.vLast_Update_Time || _Estate.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }

                        if (!changFlag)
                        {
                            _Estate.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _Estate.ESTATE_FORM_NO_AFT = model.vEstate_Form_No_Aft;
                            _Estate.ESTATE_DATE_AFT = TypeTransfer.stringToDateTimeN(model.vEstate_Date_Aft);
                            _Estate.OWNERSHIP_CERT_NO_AFT = model.vOwnership_Cert_No_Aft;
                            _Estate.LAND_BUILDING_NO_AFT = model.vLand_Building_No_Aft;
                            _Estate.HOUSE_NO_AFT = model.vHouse_No_Aft;
                            _Estate.ESTATE_SEQ_AFT = model.vEstate_Seq_Aft;
                            _Estate.MEMO_AFT = model.vMemo_Aft;
                            _Estate.LAST_UPDATE_DT = dt;

                            logStr = _Estate.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _Estate.ITEM_ID
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
                    log.CFUNCTION = "申請覆核-資料庫異動:不動產權狀";
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
                var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Estate != null)
                {
                    _Estate.INVENTORY_STATUS = "1"; //在庫
                    _Estate.ESTATE_FORM_NO_AFT = null;
                    _Estate.ESTATE_DATE_AFT = null;
                    _Estate.OWNERSHIP_CERT_NO_AFT = null;
                    _Estate.LAND_BUILDING_NO_AFT = null;
                    _Estate.HOUSE_NO_AFT = null;
                    _Estate.ESTATE_SEQ_AFT = null;
                    _Estate.MEMO_AFT = null;
                    _Estate.LAST_UPDATE_DT = dt;
                    logStr = _Estate.modelToString(logStr);
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
                var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Estate != null)
                {
                    _Estate.INVENTORY_STATUS = "1"; //在庫
                    _Estate.ESTATE_FORM_NO = GetNewValue(_Estate.ESTATE_FORM_NO, _Estate.ESTATE_FORM_NO_AFT);
                    _Estate.ESTATE_FORM_NO_AFT = null;
                    _Estate.ESTATE_DATE = DateTime.Parse(GetNewValue(_Estate.ESTATE_DATE.ToString(), _Estate.ESTATE_DATE_AFT.ToString()));
                    _Estate.ESTATE_DATE_AFT = null;
                    _Estate.OWNERSHIP_CERT_NO = GetNewValue(_Estate.OWNERSHIP_CERT_NO, _Estate.OWNERSHIP_CERT_NO_AFT);
                    _Estate.OWNERSHIP_CERT_NO_AFT = null;
                    _Estate.LAND_BUILDING_NO = GetNewValue(_Estate.LAND_BUILDING_NO, _Estate.LAND_BUILDING_NO_AFT);
                    _Estate.LAND_BUILDING_NO_AFT = null;
                    _Estate.HOUSE_NO = GetNewValue(_Estate.HOUSE_NO, _Estate.HOUSE_NO_AFT);
                    _Estate.HOUSE_NO_AFT = null;
                    _Estate.ESTATE_SEQ = GetNewValue(_Estate.ESTATE_SEQ,_Estate.ESTATE_SEQ_AFT);
                    _Estate.ESTATE_SEQ_AFT = null;
                    _Estate.MEMO = GetNewValue(_Estate.MEMO ,_Estate.MEMO_AFT);
                    _Estate.MEMO_AFT = null;
                    _Estate.LAST_UPDATE_DT = dt;
                    logStr = _Estate.modelToString(logStr);
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
                var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Estate != null)
                {
                    _Estate.INVENTORY_STATUS = "1"; //在庫
                    _Estate.CHARGE_DEPT_AFT = null;
                    _Estate.CHARGE_SECT_AFT = null;
                    _Estate.LAST_UPDATE_DT = dt;
                    logStr = _Estate.modelToString(logStr);
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
                var _Estate = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Estate != null)
                {
                    _Estate.INVENTORY_STATUS = "1"; //在庫
                    _Estate.CHARGE_DEPT = _Estate.CHARGE_DEPT_AFT;
                    _Estate.CHARGE_DEPT_AFT = null;
                    _Estate.CHARGE_SECT = _Estate.CHARGE_SECT_AFT;
                    _Estate.CHARGE_SECT_AFT = null;
                    _Estate.LAST_UPDATE_DT = dt;
                    logStr = _Estate.modelToString(logStr);
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
        /// 抓取大樓名稱或冊號
        /// </summary>
        /// <param name="vAplyUnit"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        private List<SelectOption> GetBuildNameorBookNo(string parm, string vAplyUnit = null, string aplyNo = null)
        {
            List<SelectOption> result = new List<SelectOption>() { new SelectOption() { Value = " ", Text = " " } };
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                var groupNos = new List<int>();
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    groupNos = db.ITEM_REAL_ESTATE.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(),
                    !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(),
                    !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .Select(x => x.GROUP_NO).ToList();
                }
                if (!aplyNo.IsNullOrWhiteSpace())
                {
                    var _itemId = db.OTHER_ITEM_APLY.AsNoTracking()
                                 .FirstOrDefault(x => x.APLY_NO == aplyNo)?.ITEM_ID;
                    if (!_itemId.IsNullOrWhiteSpace())
                    {
                        groupNos.Add(db.ITEM_REAL_ESTATE.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == _itemId).GROUP_NO);
                    }
                }
                var itemId = Ref.TreaItemType.D1014.ToString(); //不動產權狀
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

        /// <summary>
        /// 項目冊號資料轉畫面資料
        /// </summary>
        /// <param name="_ItemBooks"></param>
        /// <returns></returns>
        private EstateModel GetEstateModel(List<ITEM_BOOK> _ItemBooks)
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

        /// <summary>
        /// 不動產資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <param name="takeoutFlag"></param>
        /// <returns></returns>
        private IEnumerable<EstateDetailViewModel> GetDetailModel(IEnumerable<ITEM_REAL_ESTATE> data, List<SYS_CODE> _Inventory_types, bool takeoutFlag = false, bool accessStatus = false)
        {
            return data.Select(x => new EstateDetailViewModel()
            {
                vItemId = x.ITEM_ID, //物品編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vGroupNo = x.GROUP_NO.ToString(), //群組編號
                vEstate_From_No = accessStatus? x.ESTATE_FORM_NO_ACCESS : x.ESTATE_FORM_NO, //狀別
                vEstate_Date = accessStatus? x.ESTATE_DATE_ACCESS?.ToString("yyyy/MM/dd") : TypeTransfer.dateTimeNToString(x.ESTATE_DATE), //發狀日
                vOwnership_Cert_No = accessStatus? x.OWNERSHIP_CERT_NO_ACCESS : x.OWNERSHIP_CERT_NO, //字號
                vLand_Building_No = accessStatus? x.LAND_BUILDING_NO_ACCESS : x.LAND_BUILDING_NO, //地/建號
                vHouse_No = accessStatus? x.HOUSE_NO_ACCESS : x.HOUSE_NO, //門牌號
                vEstate_Seq = accessStatus? x.ESTATE_SEQ_ACCESS : x.ESTATE_SEQ, //流水號/編號
                vMemo = accessStatus? x.MEMO_ACCESS : x.MEMO, //備註
                vtakeoutFlag = (x.INVENTORY_STATUS == "4"), //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }

        /// <summary>
        /// 修改 "坐落" & "備註"
        /// </summary>
        /// <param name="updateData"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public MSGReturnModel<string> UpdateDBITEM_BOOK(CDCEstateViewModel updateData, CDCEstateViewModel viewModel, string cUserId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = "修改存取項目冊號資料檔-坐落、備註失敗";
            string logStr = string.Empty;
            if (viewModel != null)
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _APLY_NO = db.OTHER_ITEM_APLY.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == viewModel.vItemId)?.APLY_NO;
                    if(_APLY_NO != null)
                    {
                        var _ITEM_ID = db.TREA_APLY_REC.AsNoTracking().FirstOrDefault(x => x.APLY_NO == _APLY_NO)?.ITEM_ID;
                        if(_ITEM_ID != null)
                        {
                            var _TREA_ITEM_TYPE = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == _ITEM_ID)?.TREA_ITEM_TYPE;
                            if (_TREA_ITEM_TYPE != null && _TREA_ITEM_TYPE == "ESTATE")
                            {
                                var groupUp = TypeTransfer.stringToInt(viewModel.vIB_Book_No);
                                var _ItemBooks = db.ITEM_BOOK.Where(x => x.GROUP_NO == groupUp && x.ITEM_ID == _ITEM_ID).ToList();
                                foreach (var pro in updateData.GetType().GetProperties())
                                {

                                    if ((pro.Name == "vIB_Located" || pro.Name == "vIB_Memo"))
                                    {
                                        string _col_Name = string.Empty;
                                        if (pro.Name == "vIB_Located")
                                        {
                                            _col_Name = "LOCATED";
                                        }
                                        else if (pro.Name == "vIB_Memo")
                                        {
                                            _col_Name = "MEMO";
                                        }
                                        var _chang = _ItemBooks.FirstOrDefault(x => x.COL == _col_Name);
                                        if (_chang != null)
                                        {
                                            _chang.COL_VALUE = pro.GetValue(updateData)?.ToString()?.Trim();
                                            logStr += _chang.modelToString(logStr);
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            result.DESCRIPTION = "無對應存取項目";
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = "無對應申請單號";
                    }

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
                            log.CFUNCTION = "CDC不動產-修改坐落、備註";
                            log.CACTION = "U";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, cUserId);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = "修改存取項目冊號資料檔-坐落、備註成功";

                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
            }
            return result;
        }

        #endregion
    }
}