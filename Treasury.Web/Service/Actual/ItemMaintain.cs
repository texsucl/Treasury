using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-金庫存取項目維護作業
/// 初版作者：20181107 李彥賢
/// 修改歷程：20181107 李彥賢
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
    public class ItemMaintain : Common, IItemMaintain
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vItem_Name">存取項目名稱</param>
        /// <returns></returns>
        public bool Check_Item_Name(string vItem_Name)
        {
            bool _Check_Item_Name = false;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.IS_DISABLED == "N")
                    .FirstOrDefault(x => x.ITEM_DESC == vItem_Name);

                if(_TREA_ITEM != null)
                {
                    _Check_Item_Name = true;
                }
            }
            return _Check_Item_Name;
        }

        public IEnumerable<ITinItem> GetChangeRecordSearchData(ITinItem searchModel, string aply_No = null)
        {
            var searchData = (ItemMaintainChangeRecordSearchViewModel)searchModel;
            List<ItemMaintainChangeRecordSearchDetailViewModel> result = new List<ItemMaintainChangeRecordSearchDetailViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Appr_Status = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();


                if (aply_No.IsNullOrWhiteSpace())
                {
                    //var _TREA_ITEM = db.TREA_ITEM.AsNoTracking()
                    //    .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null).ToList();
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking().ToList();
                    result = db.TREA_ITEM_HIS.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == searchData.vItem_Op_Type, searchData.vItem_Op_Type != "All")
                        .Where(x => x.ITEM_ID == searchData.vTrea_IItem, searchData.vTrea_IItem != "All")
                        .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                        .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                        .Where(x => x.APLY_UID == searchData.vLast_Update_Uid, !searchData.vLast_Update_Uid.IsNullOrWhiteSpace())
                        .AsEnumerable()
                        .Select(x => new ItemMaintainChangeRecordSearchDetailViewModel {
                            //vFreeze_Dt = x.EXEC_ACTION != "A" ? _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.FREEZE_DT?.ToString("yyyy/MM/dd"): null,
                            vFreeze_Dt = x.APLY_DATE.ToString("yyyy/MM/dd"),
                            vAply_No = x.APLY_NO,
                            //vFreeze_Uid_Name = x.EXEC_ACTION != "A" ? emps.FirstOrDefault(y => y.USR_ID == _TREA_ITEM.FirstOrDefault(z => z.ITEM_ID == x.ITEM_ID)?.FREEZE_UID)?.EMP_NAME.Trim(): null,
                            vFreeze_Uid_Name = !x.APLY_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME.Trim() : null,
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vIS_TREA_ITEM = x.IS_TREA_ITEM,
                            vIS_TREA_ITEM_B = x.IS_TREA_ITEM_B,
                            vDAILY_FLAG = x.DAILY_FLAG,
                            vDAILY_FLAG_B = x.DAILY_FLAG_B,
                            vTREA_ITEM_NAME = x.TREA_ITEM_NAME,
                            vTREA_ITEM_NAME_B = x.TREA_ITEM_NAME_B,
                            vIS_DISABLED = x.IS_DISABLED,
                            vIS_DISABLED_B = x.IS_DISABLED_B,
                            vMEMO = x.MEMO,
                            vMEMO_B = x.MEMO_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = x.APPR_DESC,
                            vITEM_DESC = x.ITEM_DESC
                            //vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        }).ToList();


                    //result = db.TREA_ITEM_HIS.AsNoTracking()
                    //    .Where(x => x.ITEM_OP_TYPE == searchData.vItem_Op_Type, searchData.vItem_Op_Type != "All")
                    //    .Where(x => x.ITEM_ID == searchData.vTrea_IItem, searchData.vTrea_IItem != "All")
                    //    .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                    //    .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                    //    .AsEnumerable()
                    //    .Join(db.TREA_ITEM.AsNoTracking()
                    //    .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null)
                    //    .AsEnumerable(),
                    //    TIH => TIH.ITEM_ID,
                    //    TI => TI.ITEM_ID,
                    //    (TIH, TI) => new ItemMaintainChangeRecordSearchDetailViewModel
                    //    {
                    //        vFreeze_Dt = TI.FREEZE_DT?.ToString("yyyy/MM/dd"),
                    //        vAply_No = TIH.APLY_NO,
                    //        vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == TI.FREEZE_UID)?.EMP_NAME.Trim(),
                    //        vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == TIH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                    //        vIS_TREA_ITEM = TIH.IS_TREA_ITEM,
                    //        vIS_TREA_ITEM_B = TIH.IS_TREA_ITEM_B,
                    //        vDAILY_FLAG = TIH.DAILY_FLAG,
                    //        vDAILY_FLAG_B = TIH.DAILY_FLAG_B,
                    //        vTREA_ITEM_NAME = TIH.TREA_ITEM_NAME,
                    //        vTREA_ITEM_NAME_B = TIH.TREA_ITEM_NAME_B,
                    //        vIS_DISABLED = TIH.IS_DISABLED,
                    //        vIS_DISABLED_B = TIH.IS_DISABLED_B,
                    //        vMEMO = TIH.MEMO,
                    //        vMEMO_B = TIH.MEMO_B,
                    //        vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == TIH.APPR_STATUS)?.CODE_VALUE.Trim(),
                    //        vAPPR_DESC = TIH.APPR_DESC,
                    //    }).ToList();
                }
                else
                {
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking().ToList();
                    result = db.TREA_ITEM_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .Where(x => x.ITEM_ID == searchData.vTrea_IItem, searchData.vTrea_IItem != null)
                        .AsEnumerable()
                        .Select(x => new ItemMaintainChangeRecordSearchDetailViewModel
                        {
                            //vFreeze_Dt = x.EXEC_ACTION != "A" ? _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.FREEZE_DT?.ToString("yyyy/MM/dd") : null,
                            vFreeze_Dt = x.APLY_DATE.ToString("yyyy/MM/dd"),
                            vAply_No = x.APLY_NO,
                            //vFreeze_Uid_Name = x.EXEC_ACTION != "A" ? emps.FirstOrDefault(y => y.USR_ID == _TREA_ITEM.FirstOrDefault(z => z.ITEM_ID == x.ITEM_ID)?.FREEZE_UID)?.EMP_NAME.Trim() : null,
                            vFreeze_Uid_Name = !x.APLY_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME.Trim() : null,
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vIS_TREA_ITEM = x.IS_TREA_ITEM,
                            vIS_TREA_ITEM_B = x.IS_TREA_ITEM_B,
                            vDAILY_FLAG = x.DAILY_FLAG,
                            vDAILY_FLAG_B = x.DAILY_FLAG_B,
                            vTREA_ITEM_NAME = x.TREA_ITEM_NAME,
                            vTREA_ITEM_NAME_B = x.TREA_ITEM_NAME_B,
                            vIS_DISABLED = x.IS_DISABLED,
                            vIS_DISABLED_B = x.IS_DISABLED_B,
                            vMEMO = x.MEMO,
                            vMEMO_B = x.MEMO_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = x.APPR_DESC,
                            vITEM_DESC = x.ITEM_DESC
                            //vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        }).ToList();

                    //result = db.TREA_ITEM_HIS.AsNoTracking()
                    //    .Where(x => x.APLY_NO == aply_No)
                    //    .Where(x => x.ITEM_ID == searchData.vTrea_IItem, searchData.vTrea_IItem != null)
                    //    .AsEnumerable()
                    //    .Join(db.TREA_ITEM.AsNoTracking()
                    //    .AsEnumerable(),
                    //    TIH => TIH.ITEM_ID,
                    //    TI => TI.ITEM_ID,
                    //     (TIH, TI) => new ItemMaintainChangeRecordSearchDetailViewModel
                    //     {
                    //         vFreeze_Dt = TI.FREEZE_DT?.ToString("yyyy/MM/dd"),
                    //         vAply_No = TIH.APLY_NO,
                    //         vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == TI.FREEZE_UID)?.EMP_NAME.Trim(),
                    //         vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == TIH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                    //         vIS_TREA_ITEM = TIH.IS_TREA_ITEM,
                    //         vIS_TREA_ITEM_B = TIH.IS_TREA_ITEM_B,
                    //         vDAILY_FLAG = TIH.DAILY_FLAG,
                    //         vDAILY_FLAG_B = TIH.DAILY_FLAG_B,
                    //         vTREA_ITEM_NAME = TIH.TREA_ITEM_NAME,
                    //         vTREA_ITEM_NAME_B = TIH.TREA_ITEM_NAME_B,
                    //         vIS_DISABLED = TIH.IS_DISABLED,
                    //         vIS_DISABLED_B = TIH.IS_DISABLED_B,
                    //         vMEMO = TIH.MEMO,
                    //         vMEMO_B = TIH.MEMO_B,
                    //         vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == TIH.APPR_STATUS)?.CODE_VALUE.Trim(),
                    //         vAPPR_DESC = TIH.APPR_DESC,
                    //         vITEM_ID = TIH.ITEM_ID
                    //     }).ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchData">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (ItemMaintainSearchViewModel)searchModel;
            List<ItemMaintainSearchDetailViewModel> result = new List<ItemMaintainSearchDetailViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking();
                var _TREA_ITEM_HIS = db.TREA_ITEM_HIS.AsNoTracking().Where(x => x.APPR_DATE == null).ToList();

                result.AddRange(db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.ITEM_OP_TYPE == searchData.vITEM_OP_TYPE, searchData.vITEM_OP_TYPE != "All")
                    .Where(x => x.DAILY_FLAG == searchData.vISDO_PERDAY, searchData.vISDO_PERDAY != "All")
                    .Where(x => x.IS_DISABLED == searchData.vIS_DISABLED, searchData.vIS_DISABLED != "All")
                    .AsEnumerable()
                    .Select(x => new ItemMaintainSearchDetailViewModel()
                    {
                        vITEM_OP_TYPE = x.ITEM_OP_TYPE,
                        vITEM_DESC = x.ITEM_DESC,
                        vITEM_ID = x.ITEM_ID,
                        vIS_TREA_ITEM = x.IS_TREA_ITEM,
                        vTREA_ITEM_TYPE = x.TREA_ITEM_TYPE,
                        vTREA_ITEM_TYPE_VALUE = !x.TREA_ITEM_TYPE.IsNullOrWhiteSpace() ? _SYS_CODE.FirstOrDefault(y => y.CODE_TYPE == "TREA_ITEM_TYPE" && y.CODE == x.TREA_ITEM_TYPE)?.CODE_VALUE : null,
                        vTREA_ITEM_NAME = x.TREA_ITEM_NAME,
                        vTREA_ITEM_NAME_VALUE = !x.TREA_ITEM_NAME.IsNullOrWhiteSpace() ? _SYS_CODE.FirstOrDefault(y => y.CODE_TYPE == "TREA_ITEM_NAME" && y.CODE == x.TREA_ITEM_NAME)?.CODE_VALUE : null,
                        vISDO_PERDAY = x.DAILY_FLAG,
                        vIS_DISABLED = x.IS_DISABLED,
                        vMEMO = x.MEMO,
                        vDATA_STATUS = x.DATA_STATUS,
                        vDATA_STATUS_VALUE = !x.DATA_STATUS.IsNullOrWhiteSpace() ? _SYS_CODE.FirstOrDefault(y => y.CODE_TYPE == "DATA_STATUS" && y.CODE == x.DATA_STATUS)?.CODE_VALUE : null,
                        vFREEZE_UID = x.FREEZE_UID,
                        vFREEZE_NAME = !x.FREEZE_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim() : null,
                        vEXEC_ACTION = _TREA_ITEM_HIS.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.EXEC_ACTION?.Trim(),
                        vEXEC_ACTION_VALUE = _SYS_CODE.Where(y => y.CODE_TYPE == "EXEC_ACTION").ToList().FirstOrDefault(y => y.CODE == _TREA_ITEM_HIS.FirstOrDefault(z => z.ITEM_ID == x.ITEM_ID)?.EXEC_ACTION?.Trim())?.CODE_VALUE?.Trim(),
                        vAply_No = _TREA_ITEM_HIS.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.APLY_NO?.Trim(),
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT
                    }).ToList());
            }
            return result;
        }

        public List<SelectOption> OpTypeSelectedChange(string vTREA_OP_TYPE)
        {
            List<SelectOption> treaItem = new List<SelectOption>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                treaItem = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.ITEM_OP_TYPE == vTREA_OP_TYPE, vTREA_OP_TYPE != "All")
                    .Where(x => x.DATA_STATUS == "2")
                    .Select(x => new SelectOption() {
                        Value = x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();
            }
            return treaItem;
        }

        //public List<SelectOption> GetTreaItem()
        //{
        //    List<SelectOption> treaItem = new List<SelectOption>();

        //    using (TreasuryDBEntities db = new TreasuryDBEntities())
        //    {
        //        treaItem = db.TREA_ITEM.AsNoTracking()
        //    }
        //}

        /// <summary>
        /// 金庫進出管理作業-申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITinItem>> TinApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel)
        {
            var searchData = (ItemMaintainSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if(saveData != null)
                {
                    var datas = (List<ItemMaintainSearchDetailViewModel>)saveData;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];  
                            var cId = sysSeqDao.qrySeqNo("G1", qPreCode).ToString().PadLeft(3, '0');
                            string _Aply_No = string.Empty;
                            _Aply_No = $@"G1{qPreCode}{cId}"; //申請單號 G1+系統日期YYYMMDD(民國年)+3碼流水號
                            string logStr = string.Empty; //log

                            foreach(var item in datas)
                            {
                                var _Trea_Item_Id = string.Empty;
                                var _TI = new TREA_ITEM();
                                # region 金庫存取作業設定檔
                                //判斷執行功能
                                switch (item.vEXEC_ACTION)
                                {
                                    case "A"://新增
                                        _Trea_Item_Id = "";
                                        //_Trea_Item_Id = sysSeqDao.qrySeqNo("D1", string.Empty).ToString().PadLeft(3, '0');
                                        //_Trea_Item_Id = $@"D1{_Trea_Item_Id}";
                                        //_TI = new TREA_ITEM()
                                        //{
                                        //    ITEM_ID = _Trea_Item_Id,
                                        //    ITEM_DESC = item.vITEM_DESC,
                                        //    IS_TREA_ITEM = item.vIS_TREA_ITEM,
                                        //    TREA_ITEM_NAME = item.vTREA_ITEM_NAME,
                                        //    TREA_ITEM_TYPE = item.vTREA_ITEM_TYPE,
                                        //    ITEM_OP_TYPE = item.vITEM_OP_TYPE,
                                        //    DAILY_FLAG = item.vISDO_PERDAY,
                                        //    IS_DISABLED = item.vIS_DISABLED,
                                        //    MEMO = item.vMEMO,
                                        //    DATA_STATUS = "2",//凍結中
                                        //    CREATE_DT = dt,
                                        //    CREATE_UID = searchData.vCUSER_ID,
                                        //    LAST_UPDATE_DT =dt,
                                        //    LAST_UPDATE_UID = searchData.vCUSER_ID,
                                        //    FREEZE_UID = searchData.vCUSER_ID,
                                        //    FREEZE_DT = dt
                                        //};
                                        //db.TREA_ITEM.Add(_TI);
                                        //logStr += "|";
                                        //logStr += _TI.modelToString();
                                        break;
                                    case "U"://修改
                                        _TI = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == item.vITEM_ID);
                                        if(_TI.LAST_UPDATE_DT != null && _TI.LAST_UPDATE_DT > item.vLAST_UPDATE_DT)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _Trea_Item_Id = item.vITEM_ID;
                                        _TI.DATA_STATUS = "2";//凍結中
                                        _TI.LAST_UPDATE_UID = searchData.vCUSER_ID;
                                        _TI.LAST_UPDATE_DT = dt;
                                        _TI.FREEZE_UID = searchData.vCUSER_ID;
                                        _TI.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _TI.modelToString();
                                        break;
                                    case "D"://刪除
                                        _TI = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == item.vITEM_ID);
                                        if (_TI.LAST_UPDATE_DT != null && _TI.LAST_UPDATE_DT > item.vLAST_UPDATE_DT)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _Trea_Item_Id = item.vITEM_ID;
                                        _TI.DATA_STATUS = "2";//凍結中
                                        _TI.LAST_UPDATE_UID = searchData.vCUSER_ID;
                                        _TI.LAST_UPDATE_DT = dt;
                                        _TI.FREEZE_UID = searchData.vCUSER_ID;
                                        _TI.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _TI.modelToString();
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                                #region 金庫存取作業異動檔
                                var _TI_Data = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == item.vITEM_ID);
                                if(_TI_Data == null)
                                {
                                    var _TIH = new TREA_ITEM_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        ITEM_ID = _Trea_Item_Id,
                                        EXEC_ACTION = item.vEXEC_ACTION,
                                        ITEM_DESC = item.vITEM_DESC,
                                        IS_TREA_ITEM = item.vIS_TREA_ITEM,
                                        TREA_ITEM_NAME = item.vTREA_ITEM_NAME,
                                        TREA_ITEM_TYPE = item.vTREA_ITEM_TYPE,
                                        ITEM_OP_TYPE = item.vITEM_OP_TYPE,
                                        DAILY_FLAG = item.vISDO_PERDAY,
                                        IS_DISABLED = item.vIS_DISABLED,
                                        MEMO = item.vMEMO,
                                        APLY_DATE = dt,
                                        APLY_UID = searchData.vCUSER_ID,
                                        APPR_STATUS = "1" //表單申請
                                    };
                                    db.TREA_ITEM_HIS.Add(_TIH);
                                    logStr += "|";
                                    logStr += _TIH.modelToString();
                                }
                                else
                                {
                                    var _TIH = new TREA_ITEM_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        ITEM_ID = _Trea_Item_Id,
                                        EXEC_ACTION = item.vEXEC_ACTION,
                                        ITEM_DESC = item.vITEM_DESC,
                                        IS_TREA_ITEM = item.vIS_TREA_ITEM,
                                        TREA_ITEM_NAME = item.vTREA_ITEM_NAME,
                                        TREA_ITEM_TYPE = item.vTREA_ITEM_TYPE,
                                        ITEM_OP_TYPE = item.vITEM_OP_TYPE,
                                        DAILY_FLAG = item.vISDO_PERDAY,
                                        IS_DISABLED = item.vIS_DISABLED,
                                        MEMO = item.vMEMO,
                                        APLY_DATE = dt,
                                        APLY_UID = searchData.vCUSER_ID,
                                        APPR_STATUS = "1", //表單申請
                                        DAILY_FLAG_B = _TI_Data.DAILY_FLAG,
                                        IS_DISABLED_B = _TI_Data.IS_DISABLED,
                                        IS_TREA_ITEM_B = _TI_Data.IS_TREA_ITEM,
                                        TREA_ITEM_TYPE_B = _TI_Data.TREA_ITEM_TYPE,
                                        TREA_ITEM_NAME_B = _TI_Data.TREA_ITEM_NAME,
                                        MEMO_B = _TI_Data.MEMO
                                    };
                                    db.TREA_ITEM_HIS.Add(_TIH);
                                    logStr += "|";
                                    logStr += _TIH.modelToString();
                                }
                                #endregion
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
                                    log.CFUNCTION = "申請覆核-金庫存取項目作業";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, searchData.vCUSER_ID);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_Aply_No}");
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

        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId)
        {
            SysSeqDao sysSeqDao = new SysSeqDao();
            foreach (var aplyNo in aplyNos)
            {
                var _TREA_ITEM_HISList = db.TREA_ITEM_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if (_TREA_ITEM_HISList.Any())
                {
                    foreach (var TreaItemHis in _TREA_ITEM_HISList)
                    {
                        var _TREA_ITEM_ID = string.Empty;
                        //金庫存取作業設定檔
                        var _Treaitem = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == TreaItemHis.ITEM_ID);

                        if (_Treaitem != null)
                        {
                            _Treaitem.DATA_STATUS = "1";//可異動
                            _Treaitem.IS_TREA_ITEM = TreaItemHis.IS_TREA_ITEM;
                            _Treaitem.TREA_ITEM_NAME = TreaItemHis.TREA_ITEM_NAME;
                            _Treaitem.TREA_ITEM_TYPE = TreaItemHis.TREA_ITEM_TYPE;
                            _Treaitem.ITEM_OP_TYPE = TreaItemHis.ITEM_OP_TYPE;
                            _Treaitem.DAILY_FLAG = TreaItemHis.DAILY_FLAG;
                            _Treaitem.IS_DISABLED = TreaItemHis.IS_DISABLED;
                            _Treaitem.MEMO = TreaItemHis.MEMO;
                            _Treaitem.FREEZE_DT = null;
                            _Treaitem.FREEZE_UID = null;

                            //判斷是否刪除
                            if (TreaItemHis.EXEC_ACTION == "D")
                            {
                                _Treaitem.IS_DISABLED = "Y";
                            }
                            _Treaitem.APPR_UID = userId;
                            _Treaitem.APPR_DT = dt;
                            logStr += _Treaitem.modelToString(logStr);
                        }
                        else
                        {
                            _TREA_ITEM_ID = $@"D1{sysSeqDao.qrySeqNo("D1", string.Empty).ToString().PadLeft(3, '0')}";
                            //新增至TREA_ITEM
                            var TI = new TREA_ITEM()
                            {
                                ITEM_ID = _TREA_ITEM_ID,
                                ITEM_DESC = TreaItemHis.ITEM_DESC,
                                IS_TREA_ITEM = TreaItemHis.IS_TREA_ITEM,
                                TREA_ITEM_NAME = TreaItemHis.TREA_ITEM_NAME,
                                TREA_ITEM_TYPE = TreaItemHis.TREA_ITEM_TYPE,
                                ITEM_OP_TYPE = TreaItemHis.ITEM_OP_TYPE,
                                DAILY_FLAG = TreaItemHis.DAILY_FLAG,
                                IS_DISABLED = TreaItemHis.IS_DISABLED,
                                MEMO = TreaItemHis.MEMO,
                                DATA_STATUS = "1", //可異動
                                CREATE_UID = TreaItemHis.APLY_UID,
                                CREATE_DT = dt,
                                LAST_UPDATE_UID = TreaItemHis.APLY_UID,
                                LAST_UPDATE_DT = dt,
                                APPR_UID = userId,
                                APPR_DT = dt
                        };
                            logStr += TI.modelToString(logStr);
                            db.TREA_ITEM.Add(TI);
                        }

                        //金庫存取作業異動檔
                        var _TREA_ITEM_His = db.TREA_ITEM_HIS.FirstOrDefault(x => x.APLY_NO == TreaItemHis.APLY_NO && x.HIS_ID == TreaItemHis.HIS_ID);
                        if (_TREA_ITEM_His != null)
                        {
                            if(TreaItemHis.ITEM_ID.IsNullOrWhiteSpace())
                            {
                                _TREA_ITEM_His.ITEM_ID = _TREA_ITEM_ID;
                            }
                            _TREA_ITEM_His.APPR_STATUS = "2";//覆核完成
                            _TREA_ITEM_His.APPR_DATE = dt;
                            _TREA_ITEM_His.APPR_UID = userId;

                            logStr = _TREA_ITEM_His.modelToString(logStr);
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, logStr);
                        }
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId, string desc)
        {
            foreach (var aplyNo in aplyNos)
            {
                var _TREA_ITEM_HISList = db.TREA_ITEM_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if (_TREA_ITEM_HISList.Any())
                {
                    foreach (var TreaItemHis in _TREA_ITEM_HISList)
                    {
                        //金庫存取作業設定檔
                        var _Treaitem = db.TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == TreaItemHis.ITEM_ID);
                        if(_Treaitem != null)
                        {
                            _Treaitem.DATA_STATUS = "1";//可異動
                            _Treaitem.APPR_UID = userId;
                            _Treaitem.APPR_DT = dt;
                            _Treaitem.FREEZE_DT = null;
                            _Treaitem.FREEZE_UID = null;

                            logStr += _Treaitem.modelToString(logStr);
                        }
                        else
                        {
                            //return new Tuple<bool, string>(false, logStr);
                        }
                        //金庫存取作業異動檔
                        var _TREA_ITEM_His = db.TREA_ITEM_HIS.FirstOrDefault(x => x.APLY_NO == TreaItemHis.APLY_NO && x.HIS_ID == TreaItemHis.HIS_ID);
                        if (_TREA_ITEM_His != null)
                        {
                            _TREA_ITEM_His.APPR_STATUS = "3";//退回
                            _TREA_ITEM_His.APPR_DATE = dt;
                            _TREA_ITEM_His.APPR_UID = userId;
                            _TREA_ITEM_His.APPR_DESC = desc;

                            logStr += _TREA_ITEM_His.modelToString(logStr);
                        }
                        else
                        {
                            return new Tuple<bool, string>(false, logStr);
                        }
                    }
                }
                else
                {
                    return new Tuple<bool, string>(false, logStr);
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }
    }
}