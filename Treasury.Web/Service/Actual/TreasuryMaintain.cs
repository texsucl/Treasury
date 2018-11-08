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
/// 功能說明：金庫進出管理作業-金庫設備維護作業
/// 初版作者：20181025 侯蔚鑫
/// 修改歷程：20181025 侯蔚鑫 
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
    public class TreasuryMaintain : Common, ITreasuryMaintain
    {
        #region GetData
        /// <summary>
        /// 檢核設備名稱
        /// </summary>
        /// <param name="vEquip_Name">設備名稱</param>
        /// <returns></returns>
        public bool Check_Equip_Name(string vEquip_Name)
        {
            bool _Check_Equip_Name = false;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TE = db.TREA_EQUIP.AsNoTracking()
                    .Where(x => x.IS_DISABLED == "N")
                    .FirstOrDefault(x => x.EQUIP_NAME.Contains(vEquip_Name));

                if (_TE != null)
                {
                    _Check_Equip_Name = true;
                }
            }

            return _Check_Equip_Name;
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (TreasuryMaintainSearchViewModel)searchModel;
            List<TreasuryMaintainViewModel> result = new List<TreasuryMaintainViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Trea_Equip_HisList = db.TREA_EQUIP_HIS.AsNoTracking()
                    .Where(x => x.APPR_STATUS == "1").ToList();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "YN_FLAG").ToList();
                var _Data_Status_Name = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                result.AddRange(db.TREA_EQUIP.AsNoTracking()
                    .Where(x => x.CONTROL_MODE == searchData.vControl_Mode, searchData.vControl_Mode != "All")
                    .Where(x => x.IS_DISABLED == searchData.vIs_Disabled, searchData.vIs_Disabled != "All")
                    .AsEnumerable()
                    .Select((x) => new TreasuryMaintainViewModel()
                    {
                        vTrea_Equip_Id = x.TREA_EQUIP_ID,
                        vExec_Action = _Trea_Equip_HisList.FirstOrDefault(y => y.TREA_EQUIP_ID == x.TREA_EQUIP_ID)?.EXEC_ACTION?.Trim(),
                        vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == _Trea_Equip_HisList.FirstOrDefault(z => z.TREA_EQUIP_ID == x.TREA_EQUIP_ID)?.EXEC_ACTION?.Trim())?.CODE_VALUE?.Trim(),
                        vEquip_Name = x.EQUIP_NAME,
                        vControl_Mode = x.CONTROL_MODE,
                        vNormal_Cnt = x.NORMAL_CNT,
                        vReserve_Cnt = x.RESERVE_CNT,
                        vSum_Cnt = x.NORMAL_CNT + x.RESERVE_CNT,
                        vMemo = x.MEMO,
                        vIs_Disabled = x.IS_DISABLED,
                        vIs_Disabled_Name = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE?.Trim(),
                        vData_Status = x.DATA_STATUS,
                        vData_Status_Name = _Data_Status_Name.FirstOrDefault(y => y.CODE == x.DATA_STATUS)?.CODE_VALUE?.Trim(),
                        vLast_Update_Dt = x.LAST_UPDATE_DT,
                        vFreeze_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim(),
                        vAply_No = _Trea_Equip_HisList.FirstOrDefault(y => y.TREA_EQUIP_ID == x.TREA_EQUIP_ID)?.APLY_NO?.Trim()
                    }).ToList());
            }

            return result;
        }

        /// <summary>
        /// 異動紀錄查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <param name="aply_No">申請單號</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetChangeRecordSearchData(ITinItem searchModel, string aply_No = null)
        {
            var searchData = (TreasuryMaintainChangeRecordSearchViewModel)searchModel;
            List<TreasuryMaintainChangeRecordViewModel> result = new List<TreasuryMaintainChangeRecordViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "YN_FLAG").ToList();
                var _Control_Mode = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "CONTROL_MODE").ToList();
                var _Appr_Status = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();

                if (aply_No.IsNullOrWhiteSpace())
                {
                    result = db.TREA_EQUIP_HIS.AsNoTracking()
                        .Where(x => x.CONTROL_MODE == searchData.vControl_Mode, searchData.vControl_Mode != "All")
                        .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                        .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                        .AsEnumerable()
                        .Join(db.TREA_EQUIP.AsNoTracking()
                        .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null)
                        .Where(x => x.IS_DISABLED == searchData.vIs_Disabled, searchData.vIs_Disabled != "All")
                        .AsEnumerable(),
                        TES => TES.TREA_EQUIP_ID,
                        TE => TE.TREA_EQUIP_ID,
                        (TES, TE) => new TreasuryMaintainChangeRecordViewModel
                        {
                            vFreeze_Dt = TE.FREEZE_DT?.ToString("yyyy/MM/dd"),
                            vAply_No = TES.APLY_NO,
                            vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == TE.FREEZE_UID)?.EMP_NAME.Trim(),
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == TES.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vEquip_Name = TES.EQUIP_NAME,
                            vIs_Disabled = _Is_Disabled.FirstOrDefault(x => x.CODE == TE.IS_DISABLED)?.CODE_VALUE.Trim(),
                            vControl_Mode_Name = _Control_Mode.FirstOrDefault(x => x.CODE == TES.CONTROL_MODE)?.CODE_VALUE.Trim(),
                            vControl_Mode_B_Name = _Control_Mode.FirstOrDefault(x => x.CODE == TES.CONTROL_MODE_B)?.CODE_VALUE.Trim(),
                            vNormal_Cnt = TES.NORMAL_CNT,
                            vNormal_Cnt_B = TES.NORMAL_CNT_B,
                            vReserve_Cnt = TES.RESERVE_CNT,
                            vReserve_Cnt_B = TES.RESERVE_CNT_B,
                            vMemo = TES.MEMO,
                            vMemo_B = TES.MEMO_B,
                            vAppr_Status_Name = _Appr_Status.FirstOrDefault(x => x.CODE == TES.APPR_STATUS)?.CODE_VALUE.Trim()
                        }).ToList();
                }
                else
                {
                    result = db.TREA_EQUIP_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .Where(x => x.TREA_EQUIP_ID == searchData.vTrea_Equip_Id, searchData.vTrea_Equip_Id != null)
                        .AsEnumerable()
                        .Join(db.TREA_EQUIP.AsNoTracking()
                        .AsEnumerable(),
                        TES => TES.TREA_EQUIP_ID,
                        TE => TE.TREA_EQUIP_ID,
                        (TES, TE) => new TreasuryMaintainChangeRecordViewModel
                        {
                            vFreeze_Dt = TE.FREEZE_DT?.ToString("yyyy/MM/dd"),
                            vAply_No = TES.APLY_NO,
                            vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == TE.FREEZE_UID)?.EMP_NAME.Trim(),
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == TES.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vEquip_Name = TES.EQUIP_NAME,
                            vIs_Disabled = _Is_Disabled.FirstOrDefault(x => x.CODE == TE.IS_DISABLED)?.CODE_VALUE.Trim(),
                            vControl_Mode_Name = _Control_Mode.FirstOrDefault(x => x.CODE == TES.CONTROL_MODE)?.CODE_VALUE.Trim(),
                            vControl_Mode_B_Name = _Control_Mode.FirstOrDefault(x => x.CODE == TES.CONTROL_MODE_B)?.CODE_VALUE.Trim(),
                            vNormal_Cnt = TES.NORMAL_CNT,
                            vNormal_Cnt_B = TES.NORMAL_CNT_B,
                            vReserve_Cnt = TES.RESERVE_CNT,
                            vReserve_Cnt_B = TES.RESERVE_CNT_B,
                            vMemo = TES.MEMO,
                            vMemo_B = TES.MEMO_B,
                            vAppr_Status_Name = _Appr_Status.FirstOrDefault(x => x.CODE == TES.APPR_STATUS)?.CODE_VALUE.Trim()
                        }).ToList();
                }
            }

            return result;
        }
        #endregion

        #region SaveData
        /// <summary>
        /// 金庫進出管理作業-申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITinItem>> TinApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel)
        {
            var searchData = (TreasuryMaintainSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var datas = (List<TreasuryMaintainViewModel>)saveData;
                    if(datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            string _Aply_No = string.Empty;
                            var cId = sysSeqDao.qrySeqNo("G2", qPreCode).ToString().PadLeft(3, '0');
                            _Aply_No = $@"G2{qPreCode}{cId}"; //申請單號 G2+系統日期YYYMMDD(民國年)+3碼流水號
                            string logStr = string.Empty; //log

                            foreach (var item in datas)
                            {
                                var _Trea_Equip_Id = string.Empty;
                                var _TE = new TREA_EQUIP();
                                #region 金庫設備設定檔
                                //判斷執行功能
                                switch (item.vExec_Action)
                                {
                                    case "A"://新增
                                        _Trea_Equip_Id = sysSeqDao.qrySeqNo("D2", string.Empty).ToString().PadLeft(3, '0');
                                        _Trea_Equip_Id = $@"D2{_Trea_Equip_Id}";
                                        _TE = new TREA_EQUIP()
                                        {
                                            TREA_EQUIP_ID = _Trea_Equip_Id,
                                            EQUIP_NAME = item.vEquip_Name,
                                            CONTROL_MODE = item.vControl_Mode,
                                            NORMAL_CNT = item.vNormal_Cnt,
                                            RESERVE_CNT = item.vReserve_Cnt,
                                            MEMO = item.vMemo,
                                            IS_DISABLED = "N",
                                            DATA_STATUS = "2",//凍結中
                                            CREATE_UID = searchData.vLast_Update_Uid,
                                            CREATE_DT = dt,
                                            LAST_UPDATE_UID = searchData.vLast_Update_Uid,
                                            LAST_UPDATE_DT = dt,
                                            FREEZE_UID = searchData.vLast_Update_Uid,
                                            FREEZE_DT = dt
                                        };
                                        db.TREA_EQUIP.Add(_TE);
                                        logStr += "|";
                                        logStr += _TE.modelToString();
                                        break;
                                    case "U"://修改
                                        _TE = db.TREA_EQUIP.FirstOrDefault(x => x.TREA_EQUIP_ID == item.vTrea_Equip_Id);
                                        if (_TE.LAST_UPDATE_DT != null && _TE.LAST_UPDATE_DT > item.vLast_Update_Dt)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _Trea_Equip_Id = item.vTrea_Equip_Id;
                                        _TE.DATA_STATUS = "2";//凍結中
                                        _TE.LAST_UPDATE_UID = searchData.vLast_Update_Uid;
                                        _TE.LAST_UPDATE_DT = dt;
                                        _TE.FREEZE_UID = searchData.vLast_Update_Uid;
                                        _TE.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _TE.modelToString();
                                        break;
                                    case "D"://刪除
                                        _TE = db.TREA_EQUIP.FirstOrDefault(x => x.TREA_EQUIP_ID == item.vTrea_Equip_Id);
                                        if (_TE.LAST_UPDATE_DT != null && _TE.LAST_UPDATE_DT > item.vLast_Update_Dt)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _Trea_Equip_Id = item.vTrea_Equip_Id;
                                        _TE.DATA_STATUS = "2";//凍結中
                                        _TE.LAST_UPDATE_UID = searchData.vLast_Update_Uid;
                                        _TE.LAST_UPDATE_DT = dt;
                                        _TE.FREEZE_UID = searchData.vLast_Update_Uid;
                                        _TE.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _TE.modelToString();
                                        break;
                                    default:
                                        break;
                                }
                                #endregion

                                #region 金庫設備異動檔
                                var _TE_Data = db.TREA_EQUIP.FirstOrDefault(x => x.TREA_EQUIP_ID == item.vTrea_Equip_Id);
                                if (_TE_Data == null)
                                {
                                    var _TEH = new TREA_EQUIP_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        TREA_EQUIP_ID = _Trea_Equip_Id,
                                        EXEC_ACTION = item.vExec_Action,
                                        EQUIP_NAME = item.vEquip_Name,
                                        CONTROL_MODE = item.vControl_Mode,
                                        NORMAL_CNT = item.vNormal_Cnt,
                                        RESERVE_CNT = item.vReserve_Cnt,
                                        MEMO = item.vMemo,
                                        APPR_STATUS = "1"//表單申請
                                    };
                                    db.TREA_EQUIP_HIS.Add(_TEH);
                                    logStr += "|";
                                    logStr += _TEH.modelToString();
                                }
                                else
                                {
                                    var _TEH = new TREA_EQUIP_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        TREA_EQUIP_ID = _Trea_Equip_Id,
                                        EXEC_ACTION = item.vExec_Action,
                                        EQUIP_NAME = item.vEquip_Name,
                                        CONTROL_MODE = item.vControl_Mode,
                                        NORMAL_CNT = item.vNormal_Cnt,
                                        RESERVE_CNT = item.vReserve_Cnt,
                                        MEMO = item.vMemo,
                                        APPR_STATUS = "1",//表單申請
                                        CONTROL_MODE_B = _TE_Data.CONTROL_MODE,
                                        NORMAL_CNT_B = _TE_Data.NORMAL_CNT,
                                        RESERVE_CNT_B = _TE_Data.RESERVE_CNT,
                                        MEMO_B = _TE_Data.MEMO
                                    };
                                    db.TREA_EQUIP_HIS.Add(_TEH);
                                    logStr += "|";
                                    logStr += _TEH.modelToString();
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
                                    log.CFUNCTION = "申請覆核-新增金庫設備";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, searchData.vLast_Update_Uid);
                                    #endregion

                                    result.RETURN_FLAG = true;
                                    result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(null, $@"單號為{_Aply_No}");
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
        /// 金庫進出管理作業-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <param name="desc">覆核意見</param>
        /// <returns></returns>
        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt, string userId, string desc)
        {
            foreach (var itemID in itemIDs)
            {

            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 金庫進出管理作業-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <returns></returns>
        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt, string userId)
        {
            foreach (var itemID in itemIDs)
            {

            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        #endregion
    }
}