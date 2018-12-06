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
using Treasury.Web.Enum;
using System.ComponentModel;
using Treasury.Web.Controllers;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫物品存取申請作業 存出保證金
/// 初版作者：20180730 侯蔚鑫
/// 修改歷程：20180730 侯蔚鑫 
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
    public class Marging: Common,IMarging
    {

        public Marging()
        {

        }

        #region GetData
        /// <summary>
        /// 類別
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetMargingType()
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "MARGING_TYPE")
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
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <returns></returns>
        public List<MargingpViewModel> GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null)
        {
            var result = new List<MargingpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var dept = intra.getDept(vAplyUnit); //抓取單位
                if (!vAplyUnit.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                    .Where(x => x.CHARGE_DEPT == dept.UP_DPT_CD.Trim() && x.CHARGE_SECT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "04") //單位為科
                    .Where(x => x.CHARGE_DEPT == dept.DPT_CD.Trim(), !dept.Dpt_type.IsNullOrWhiteSpace() && dept.Dpt_type.Trim() == "03") //單位為部
                    .Where(x => x.INVENTORY_STATUS == "1") //庫存
                    .AsEnumerable()
                    .Select(x =>
                    new MargingpViewModel()
                    {
                        vItem_PK = x.ITEM_ID,
                        vItem_Id=x.ITEM_ID,
                        vStatus = Ref.AccessInventoryType._1.GetDescription(),
                        vTrad_Partners=x.TRAD_PARTNERS,
                        vMargin_Dep_Type=x.MARGIN_DEP_TYPE,
                        vAmount=x.AMOUNT,
                        vWorkplace_Code=x.WORKPLACE_CODE,
                        vDescription=x.DESCRIPTION,
                        vMemo = x.MEMO,
                        vBook_No = x.BOOK_NO,
                        vTakeoutFlag = false,
                        vLast_Update_Time = x.LAST_UPDATE_DT
                    }).ToList();
                }
                if (!vAplyNo.IsNullOrWhiteSpace())
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == vAplyNo).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_REFUNDABLE_DEP.AsNoTracking().Where(
                    x => itemIds.Contains(x.ITEM_ID)).AsEnumerable()
                    .Select(x =>
                     new MargingpViewModel()
                     {
                         vItem_PK = x.ITEM_ID,
                         vItem_Id = x.ITEM_ID,
                         vStatus = Ref.AccessInventoryType._4.GetDescription(),
                         vTrad_Partners = x.TRAD_PARTNERS,
                         vMargin_Dep_Type = x.MARGIN_DEP_TYPE,
                         vAmount = x.AMOUNT,
                         vWorkplace_Code = x.WORKPLACE_CODE,
                         vDescription = x.DESCRIPTION,
                         vMemo = x.MEMO,
                         vBook_No = x.BOOK_NO,
                         vTakeoutFlag = true,
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
        public IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel, string aply_No = null, string charge_Dept = null, string charge_Sect = null)
        {
            List<CDCMargingViewModel> result = new List<CDCMargingViewModel>();

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
                    result.AddRange(db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                        .Where(x => TreasuryIn.Contains(x.INVENTORY_STATUS), searchModel.vTreasuryIO == "Y")
                        .Where(x => x.INVENTORY_STATUS == TreasuryOut, searchModel.vTreasuryIO == "N")
                        .Where(x => x.ITEM_ID == searchModel.vItem_No, !searchModel.vItem_No.IsNullOrWhiteSpace())
                        .Where(x => x.BOOK_NO == searchModel.vItem_Book_No, !searchModel.vItem_Book_No.IsNullOrWhiteSpace())
                        //.Where(x => x.BOOK_NO == searchModel.vBookNo, !searchModel.vBookNo.IsNullOrWhiteSpace())
                        .Where(x => x.MARGIN_DEP_TYPE == searchModel.vMargin_Dep_Type, searchModel.vMargin_Dep_Type != "All")
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value >= PUT_DATE_From.Value, PUT_DATE_From != null)
                        .Where(x => x.PUT_DATE != null && x.PUT_DATE.Value <= PUT_DATE_To.Value, PUT_DATE_To != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value >= GET_DATE_From.Value, GET_DATE_From != null)
                        .Where(x => x.GET_DATE != null && x.GET_DATE.Value <= GET_DATE_To.Value, GET_DATE_To != null)
                        .Where(x => x.CHARGE_DEPT == charge_Dept, !charge_Dept.IsNullOrWhiteSpace())
                        .Where(x => x.CHARGE_SECT == charge_Sect, !charge_Sect.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select((x) => new CDCMargingViewModel()
                        {
                            vItem_PK = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vBook_No = x.BOOK_NO,
                            vBook_No_AFT = x.BOOK_NO_AFT,
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
                            vMargin_Dep_Type = x.MARGIN_DEP_TYPE,
                            vMargin_Dep_Type_AFT = x.MARGIN_DEP_TYPE_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_AFT = x.TRAD_PARTNERS_AFT,
                            vlItem_Id = x.ITEM_ID,
                            vAmount = x.AMOUNT,
                            vAmount_AFT = x.AMOUNT_AFT,
                            vWorkplace_Code = x.WORKPLACE_CODE,
                            vWorkplace_Code_AFT = x.WORKPLACE_CODE_AFT,
                            vDescription = x.DESCRIPTION,
                            vDescription_AFT = x.DESCRIPTION_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
                            vLast_Update_Time = x.LAST_UPDATE_DT
                        }).ToList());
                    if (searchModel.vTreasuryIO == "N") //取出
                    {
                        if (result.Any())
                        {
                            var itemIds = result.Select(x => x.vlItem_Id).ToList();
                            var uids = GetAplyUidName(itemIds);
                            result.ForEach(x =>
                            {
                                x.vGet_Uid_Name = uids.FirstOrDefault(y => y.itemId == x.vlItem_Id)?.getAplyUidName;
                            });
                        }
                    }
                }
                else
                {
                    var itemIds = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No).Select(x => x.ITEM_ID).ToList();
                    result.AddRange(db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                        .Where(x => itemIds.Contains(x.ITEM_ID))
                        .AsEnumerable()
                        .Select((x) => new CDCMargingViewModel()
                        {
                            vItem_PK = x.ITEM_ID,
                            vStatus = x.INVENTORY_STATUS,
                            vPut_Date = x.PUT_DATE?.dateTimeToStr(),
                            vGet_Date = x.GET_DATE?.dateTimeToStr(),
                            vBook_No = x.BOOK_NO,
                            vBook_No_AFT = x.BOOK_NO_AFT,
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
                            vMargin_Dep_Type = x.MARGIN_DEP_TYPE,
                            vMargin_Dep_Type_AFT = x.MARGIN_DEP_TYPE_AFT,
                            vTrad_Partners = x.TRAD_PARTNERS,
                            vTrad_Partners_AFT = x.TRAD_PARTNERS_AFT,
                            vlItem_Id = x.ITEM_ID,
                            vAmount = x.AMOUNT,
                            vAmount_AFT = x.AMOUNT_AFT,
                            vWorkplace_Code = x.WORKPLACE_CODE,
                            vWorkplace_Code_AFT = x.WORKPLACE_CODE_AFT,
                            vDescription = x.DESCRIPTION,
                            vDescription_AFT = x.DESCRIPTION_AFT,
                            vMemo = x.MEMO,
                            vMemo_AFT = x.MEMO_AFT,
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

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        public List<MargingpViewModel> GetDataByAplyNo(string vAplyNo)
        {
            var result = new List<MargingpViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == vAplyNo);
                if (_TAR != null)
                {
                    //使用單號去其他存取項目檔抓取物品編號
                    var OIAs = db.OTHER_ITEM_APLY.AsNoTracking()
                        .Where(x => x.APLY_NO == _TAR.APLY_NO).Select(x => x.ITEM_ID).ToList();
                    //使用物品編號去存出保證金庫存資料檔抓取資料
                    var details = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                        .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();
                    if (details.Any())
                    {
                        var _code_type = Ref.SysCodeType.INVENTORY_TYPE.ToString(); //庫存狀態
                        var _Inventory_types = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == _code_type).ToList();
                        bool _accessStatus = (_TAR.APLY_STATUS == Ref.AccessProjectFormStatus.E01.ToString()) && (_TAR.ACCESS_TYPE == Ref.AccessProjectTradeType.P.ToString());
                        result = GetDetailModel(details, _Inventory_types, _accessStatus).ToList();
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
                    var datas = (List<MargingpViewModel>)insertDatas;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            string logStr = string.Empty; //log
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
                                foreach (var item in datas)
                                {
                                    var _IRD_Item_Id = string.Empty;
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            string item_id = string.Empty;

                                            //依類別取得對應歸檔編號
                                            switch (item.vMargin_Dep_Type)
                                            {
                                                case "1":
                                                    item_id = sysSeqDao.qrySeqNo("A", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"A{item_id}";
                                                    break;
                                                case "2":
                                                    item_id = sysSeqDao.qrySeqNo("B", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"B{item_id}";
                                                    break;
                                                case "3":
                                                    item_id = sysSeqDao.qrySeqNo("C", string.Empty).ToString().PadLeft(8, '0');
                                                    item_id = $@"C{item_id}";
                                                    break;
                                                default:
                                                    result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                                                    return result;
                                            }

                                            #region 存出保證金庫存資料檔
                                            var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                            var _IRD = new ITEM_REFUNDABLE_DEP()
                                            {
                                                ITEM_ID = item_id,   //歸檔編號
                                                INVENTORY_STATUS = "3", //預約存入
                                                TRAD_PARTNERS = item.vTrad_Partners,  //交易對象
                                                MARGIN_DEP_TYPE = item.vMargin_Dep_Type,  //存出保證金類別
                                                AMOUNT = item.vAmount,    //金額
                                                WORKPLACE_CODE = item.vWorkplace_Code,    //職場代號
                                                DESCRIPTION = item.vDescription,  //說明
                                                MEMO = item.vMemo,    //備註
                                                BOOK_NO = item.vBook_No,  //冊號
                                                APLY_DEPT = _dept.Item1, //申請人部門
                                                APLY_SECT = _dept.Item2, //申請人科別
                                                APLY_UID = taData.vAplyUid, //申請人
                                                CHARGE_DEPT = _dept.Item1, //權責部門
                                                CHARGE_SECT = _dept.Item2, //權責科別
                                                LAST_UPDATE_DT = dt //最後修改時間
                                            };
                                            _IRD_Item_Id = _IRD.ITEM_ID;
                                            db.ITEM_REFUNDABLE_DEP.Add(_IRD);
                                            logStr += "|";
                                            logStr += _IRD.modelToString();
                                            #endregion
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString()) //判斷申請作業-取出
                                    {
                                        //只抓取預約取出
                                        if (item.vStatus == Ref.AccessInventoryType._4.GetDescription())
                                        {
                                            #region 存出保證金庫存資料檔
                                            var _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                            _IRD_Item_Id = _IRD.ITEM_ID;
                                            if (_IRD.LAST_UPDATE_DT > item.vLast_Update_Time)
                                            {
                                                result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                return result;
                                            }
                                            _IRD.INVENTORY_STATUS = "4"; //預約取出
                                            _IRD.LAST_UPDATE_DT = dt;  //最後修改時間
                                            #endregion
                                        }
                                    }

                                    #region 其它存取項目申請資料檔
                                    if (!_IRD_Item_Id.IsNullOrWhiteSpace())
                                    {
                                        db.OTHER_ITEM_APLY.Add(
                                        new OTHER_ITEM_APLY()
                                        {
                                            APLY_NO = _TAR.APLY_NO,
                                            ITEM_ID = _IRD_Item_Id
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

                                logStr += _TAR.modelToString();
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

                                #region 儲存資料
                                List<string> oldItemIds = db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo).Select(x => x.ITEM_ID).ToList(); //原有 itemId
                                List<string> updateItemIds = new List<string>(); //更新 itemId
                                List<ITEM_REFUNDABLE_DEP> inserts = new List<ITEM_REFUNDABLE_DEP>(); //新增資料
                                var _IRD = new ITEM_REFUNDABLE_DEP();

                                foreach(var item in datas)
                                {
                                    //判斷申請作業-存入
                                    if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString())
                                    {
                                        //只抓取預約存入
                                        if (item.vStatus == Ref.AccessInventoryType._3.GetDescription())
                                        {
                                            string TypeCode = string.Empty;

                                            //依類別取得對應歸檔編號
                                            switch (item.vMargin_Dep_Type)
                                            {
                                                case "1":
                                                    TypeCode = "A";
                                                    break;
                                                case "2":
                                                    TypeCode = "B";
                                                    break;
                                                case "3":
                                                    TypeCode = "C";
                                                    break;
                                                default:
                                                    result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                                                    return result;
                                            }

                                            if (item.vItem_PK.StartsWith(TypeCode))  //明細修改
                                            {
                                                _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.vItem_PK);
                                                if (_IRD.LAST_UPDATE_DT > item.vLast_Update_Time)
                                                {
                                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                                    return result;
                                                }
                                                _IRD.TRAD_PARTNERS = item.vTrad_Partners;   //交易對象
                                                _IRD.MARGIN_DEP_TYPE = item.vMargin_Dep_Type;   //存出保證金類別
                                                _IRD.AMOUNT = item.vAmount; //金額
                                                _IRD.WORKPLACE_CODE = item.vWorkplace_Code; //職場代號
                                                _IRD.DESCRIPTION = item.vDescription;   //說明
                                                _IRD.MEMO = item.vMemo; //備註
                                                _IRD.BOOK_NO = item.vBook_No;   //冊號
                                                _IRD.LAST_UPDATE_DT = dt;   //最後修改時間

                                                updateItemIds.Add(item.vItem_Id);

                                                logStr += "|";
                                                logStr += _IRD.modelToString();
                                            }
                                            else //明細新增
                                            {
                                                string item_id = string.Empty;

                                                //依類別取得對應歸檔編號
                                                switch (item.vMargin_Dep_Type)
                                                {
                                                    case "1":
                                                        item_id = sysSeqDao.qrySeqNo("A", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"A{item_id}";
                                                        break;
                                                    case "2":
                                                        item_id = sysSeqDao.qrySeqNo("B", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"B{item_id}";
                                                        break;
                                                    case "3":
                                                        item_id = sysSeqDao.qrySeqNo("C", string.Empty).ToString().PadLeft(8, '0');
                                                        item_id = $@"C{item_id}";
                                                        break;
                                                    default:
                                                        result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                                                        return result;
                                                }

                                                var _dept = intra.getDept_Sect(taData.vAplyUnit);
                                                _IRD = new ITEM_REFUNDABLE_DEP()
                                                {
                                                    ITEM_ID = item_id,   //歸檔編號
                                                    INVENTORY_STATUS = "3", //預約存入
                                                    TRAD_PARTNERS = item.vTrad_Partners,  //交易對象
                                                    MARGIN_DEP_TYPE = item.vMargin_Dep_Type,  //存出保證金類別
                                                    AMOUNT = item.vAmount,    //金額
                                                    WORKPLACE_CODE = item.vWorkplace_Code,    //職場代號
                                                    DESCRIPTION = item.vDescription,  //說明
                                                    MEMO = item.vMemo,    //備註
                                                    BOOK_NO = item.vBook_No,  //冊號
                                                    APLY_DEPT = _dept.Item1, //申請人部門
                                                    APLY_SECT = _dept.Item2, //申請人科別
                                                    APLY_UID = taData.vAplyUid, //申請人
                                                    CHARGE_DEPT = _dept.Item1, //權責部門
                                                    CHARGE_SECT = _dept.Item2, //權責科別
                                                    LAST_UPDATE_DT = dt //最後修改時間
                                                };
                                                inserts.Add(_IRD);
                                                logStr += _IRD.modelToString(logStr);
                                            }
                                        }
                                    }
                                    else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus)) //判斷申請作業-取出
                                    {
                                        _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.vItem_Id);
                                        if (_IRD.LAST_UPDATE_DT > item.vLast_Update_Time)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        //預約取出
                                        if (item.vTakeoutFlag)
                                        {
                                            if (_IRD.INVENTORY_STATUS == "1") //原先為在庫
                                            {
                                                _IRD.INVENTORY_STATUS = "4"; //預約取出
                                                _IRD.LAST_UPDATE_DT = dt;  //最後修改時間
                                                updateItemIds.Add(_IRD.ITEM_ID);
                                                logStr += _IRD.modelToString(logStr);
                                            }
                                            else if (_IRD.INVENTORY_STATUS == "4") //原先為預約取出
                                            {
                                                updateItemIds.Add(_IRD.ITEM_ID);
                                            }
                                        }
                                        else
                                        {
                                            if (_IRD.INVENTORY_STATUS == "4") //原先為在庫
                                            {
                                                _IRD.INVENTORY_STATUS = "1"; //預約取出
                                                _IRD.LAST_UPDATE_DT = dt;  //最後修改時間
                                                logStr += _IRD.modelToString(logStr);
                                            }
                                        }
                                    }
                                }

                                if (taData.vAccessType == Ref.AccessProjectTradeType.P.ToString()) //存入
                                {
                                    var delItemId = oldItemIds.Where(x => !updateItemIds.Contains(x)).ToList();
                                    db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => x.APLY_NO == taData.vAplyNo && delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.ITEM_REFUNDABLE_DEP.RemoveRange(db.ITEM_REFUNDABLE_DEP.Where(x => delItemId.Contains(x.ITEM_ID)).ToList());
                                    db.OTHER_ITEM_APLY.AddRange(inserts.Select(x => new OTHER_ITEM_APLY()
                                    {
                                        APLY_NO = taData.vAplyNo,
                                        ITEM_ID = x.ITEM_ID
                                    }));
                                    db.ITEM_REFUNDABLE_DEP.AddRange(inserts);
                                }
                                else if (taData.vAccessType == Ref.AccessProjectTradeType.G.ToString() && (_APLY_STATUS != CustodyConfirmStatus))//取出
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
                                    log.CFUNCTION = "申請覆核-新增存出保證金";
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
                foreach (CDCMargingViewModel model in saveData)
                {
                    var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == model.vItem_PK);
                    if (_Marging != null && !changFlag)
                    {
                        if (_Marging.LAST_UPDATE_DT > model.vLast_Update_Time || _Marging.INVENTORY_STATUS != "1")
                        {
                            changFlag = true;
                        }
                        if (!changFlag)
                        {
                            _Marging.INVENTORY_STATUS = "8"; //庫存狀態改為「8」資料庫異動中。
                            _Marging.MARGIN_DEP_TYPE_AFT = model.vMargin_Dep_Type_AFT;
                            _Marging.TRAD_PARTNERS_AFT = model.vTrad_Partners_AFT;
                            _Marging.AMOUNT_AFT = model.vAmount_AFT;
                            _Marging.WORKPLACE_CODE_AFT = model.vWorkplace_Code_AFT;
                            _Marging.DESCRIPTION_AFT = model.vDescription_AFT;
                            _Marging.MEMO_AFT = model.vMemo_AFT;
                            _Marging.LAST_UPDATE_DT = dt;
                            _Marging.BOOK_NO_AFT = model.vBook_No_AFT;

                            logStr = _Marging.modelToString(logStr);

                            var _OIA = new OTHER_ITEM_APLY()
                            {
                                APLY_NO = _data.Item1,
                                ITEM_ID = _Marging.ITEM_ID
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
                    log.CFUNCTION = "申請覆核-資料庫異動:存出保證金";
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
                var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marging != null)
                {
                    _Marging.INVENTORY_STATUS = "1"; //在庫
                    _Marging.BOOK_NO_AFT = null;
                    _Marging.MARGIN_DEP_TYPE_AFT = null;
                    _Marging.TRAD_PARTNERS_AFT = null;
                    _Marging.AMOUNT_AFT = null;
                    _Marging.WORKPLACE_CODE_AFT = null;
                    _Marging.DESCRIPTION_AFT = null;
                    _Marging.MEMO_AFT = null;
                    _Marging.LAST_UPDATE_DT = dt;
                    logStr = _Marging.modelToString(logStr);
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
                var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marging != null)
                {
                    _Marging.INVENTORY_STATUS = "1"; //在庫
                    _Marging.MARGIN_DEP_TYPE = GetNewValue(_Marging.MARGIN_DEP_TYPE, _Marging.MARGIN_DEP_TYPE_AFT);
                    _Marging.MARGIN_DEP_TYPE_AFT = null;
                    _Marging.TRAD_PARTNERS = GetNewValue(_Marging.TRAD_PARTNERS, _Marging.TRAD_PARTNERS_AFT);
                    _Marging.TRAD_PARTNERS_AFT = null;
                    _Marging.AMOUNT = string.IsNullOrEmpty(TypeTransfer.decimalNToString(_Marging.AMOUNT_AFT)) ? _Marging.AMOUNT : _Marging.AMOUNT_AFT;
                    _Marging.AMOUNT_AFT = null;
                    _Marging.WORKPLACE_CODE = GetNewValue(_Marging.WORKPLACE_CODE, _Marging.WORKPLACE_CODE_AFT);
                    _Marging.WORKPLACE_CODE_AFT = null;
                    _Marging.DESCRIPTION = GetNewValue(_Marging.DESCRIPTION, _Marging.DESCRIPTION_AFT);
                    _Marging.DESCRIPTION_AFT = null;
                    _Marging.MEMO = GetNewValue(_Marging.MEMO, _Marging.MEMO_AFT);
                    _Marging.MEMO_AFT = null;
                    _Marging.BOOK_NO = GetNewValue(_Marging.BOOK_NO, _Marging.BOOK_NO_AFT);
                    _Marging.BOOK_NO_AFT = null;
                    _Marging.LAST_UPDATE_DT = dt;
                    logStr = _Marging.modelToString(logStr);
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 申請刪除 & 作廢 存出保證金資料庫要處理的事件
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
                //使用物品編號去存出保證金庫存資料檔抓取資料
                var details = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                    .Where(x => OIAs.Contains(x.ITEM_ID)).ToList();

                if (details.Any())
                {
                    if (accessType == Ref.AccessProjectTradeType.G.ToString()) //取出狀態處理作業
                    {
                        foreach (var item in details)
                        {
                            var _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                            _IRD.INVENTORY_STATUS = "1"; //返回在庫
                            _IRD.LAST_UPDATE_DT = dt;
                            logStr += _IRD.modelToString(logStr);
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
                            db.ITEM_REFUNDABLE_DEP.RemoveRange(db.ITEM_REFUNDABLE_DEP.Where(x => OIAs.Contains(x.ITEM_ID)));
                            db.OTHER_ITEM_APLY.RemoveRange(db.OTHER_ITEM_APLY.Where(x => OIAs.Contains(x.ITEM_ID)));
                        }
                        else
                        {
                            foreach (var item in details)
                            {
                                var _IRD = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == item.ITEM_ID);
                                _IRD.INVENTORY_STATUS = "7"; //已取消
                                _IRD.LAST_UPDATE_DT = dt;
                                logStr += _IRD.modelToString(logStr);
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
                var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marging != null)
                {
                    _Marging.INVENTORY_STATUS = "1"; //在庫
                    _Marging.CHARGE_DEPT_AFT = null;
                    _Marging.CHARGE_SECT_AFT = null;
                    _Marging.LAST_UPDATE_DT = dt;
                    logStr = _Marging.modelToString(logStr);
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
                var _Marging = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == itemID);
                if (_Marging != null)
                {
                    _Marging.INVENTORY_STATUS = "1"; //在庫
                    _Marging.CHARGE_DEPT = _Marging.CHARGE_DEPT_AFT;
                    _Marging.CHARGE_DEPT_AFT = null;
                    _Marging.CHARGE_SECT = _Marging.CHARGE_SECT_AFT;
                    _Marging.CHARGE_SECT_AFT = null;
                    _Marging.LAST_UPDATE_DT = dt;
                    logStr = _Marging.modelToString(logStr);
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
        /// 存出保證金資料轉畫面資料
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_Inventory_types"></param>
        /// <returns></returns>
        private IEnumerable<MargingpViewModel> GetDetailModel(IEnumerable<ITEM_REFUNDABLE_DEP> data, List<SYS_CODE> _Inventory_types, bool accessStatus)
        {
            return data.Select(x => new MargingpViewModel()
            {
                vItem_PK = x.ITEM_ID,   //網頁PK
                vItem_Id = x.ITEM_ID, //歸檔編號
                vStatus = _Inventory_types.FirstOrDefault(y => y.CODE == x.INVENTORY_STATUS)?.CODE_VALUE,//代碼.庫存狀態 
                vTrad_Partners = accessStatus ? x.TRAD_PARTNERS_ACCESS : x.TRAD_PARTNERS, //交易對象
                vMargin_Dep_Type = accessStatus ? x.MARGIN_DEP_TYPE_ACCESS : x.MARGIN_DEP_TYPE,   //存出保證金類別
                vAmount = accessStatus ? x.AMOUNT_ACCESS : x.AMOUNT, //金額
                vWorkplace_Code = accessStatus ? x.WORKPLACE_CODE_ACCESS : x.WORKPLACE_CODE, //職場代號
                vDescription = accessStatus ? x.DESCRIPTION_ACCESS : x.DESCRIPTION,   //說明
                vMemo = accessStatus ? x.MEMO_ACCESS : x.MEMO, //備註
                vBook_No = accessStatus ? x.BOOK_NO_ACCESS : x.BOOK_NO,   //冊號
                vTakeoutFlag = false, //取出註記
                vLast_Update_Time = x.LAST_UPDATE_DT //最後修改時間
            });
        }
        #endregion
    }
}