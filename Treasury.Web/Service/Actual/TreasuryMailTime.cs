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
/// 功能說明：金庫進出管理作業-mail發送時間定義檔
/// 初版作者：20181107 張家華
/// 修改歷程：20181107 張家華 
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
    public class TreasuryMailTime : Common , ITreasuryMailTime
    {
        #region GetData

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (TreasuryMailTimeSearchViewModel)searchModel;
            List<TreasuryMailTimeViewModel> result = new List<TreasuryMailTimeViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();

                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();
                var _DATA_STATUS = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                var his = db.MAIL_TIME_HIS.AsNoTracking()
                    .Where(x => x.APPR_DATE == null).ToList();

                result.AddRange(db.MAIL_TIME.AsNoTracking()
                    .AsEnumerable()
                    .Select((x) => new TreasuryMailTimeViewModel()
                    {                        
                        vSEND_TIME = x.SEND_TIME,
                        vFUNC_ID = x.FUNC_ID,
                        vINTERVAL_MIN = x.INTERVAL_MIN?.ToString(),
                        vMAIL_CONTENT_ID = x.MAIL_CONTENT_ID,
                        vEXEC_TIME_B = x.EXEC_TIME_B,
                        vEXEC_TIME_E = x.EXEC_TIME_E,
                        vDATA_STATUS = x.DATA_STATUS,
                        vDATA_STATUS_NAME = _DATA_STATUS.FirstOrDefault(y => y.CODE == x.DATA_STATUS)?.CODE_VALUE,
                        vAplyNo = x.DATA_STATUS != "1" ? his.FirstOrDefault(y => y.MAIL_TIME_ID == x.MAIL_TIME_ID)?.APLY_NO : "",
                        vMEMO = x.MEMO,
                        vIS_DISABLED = x.IS_DISABLED,
                        vIS_DISABLED_NAME = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE,
                        vTREA_OPEN_TIME = x.TREA_OPEN_TIME,
                        vFREEZE_UID_Name = emps.FirstOrDefault(y => y.USR_ID != null && y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim(),
                        vLAST_UPDATE_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.LAST_UPDATE_UID)?.EMP_NAME?.Trim(),
                        vLAST_UPDATE_DT_Show = TypeTransfer.dateTimeNToString(x.LAST_UPDATE_DT),
                        vMAIL_TIME_ID = x.MAIL_TIME_ID,
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT,
                    }).OrderBy(x=>x.vMAIL_CONTENT_ID).ToList());
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
            var searchData = (TreasuryMailTimeSearchViewModel)searchModel;
            List<TreasuryMailTimeHistoryViewModel> result = new List<TreasuryMailTimeHistoryViewModel>();

            //if (searchData == null)
            //    return result;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();

                var _CODE_FUNC = db.CODE_FUNC.AsNoTracking().ToList();

                var _sysCodes = db.SYS_CODE.AsNoTracking().ToList();

                var _EXEC_ACTION = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Appr_Status = _sysCodes
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();
                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                 .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();

                result = db.MAIL_TIME_HIS.AsNoTracking()
                    .Where(x => x.FUNC_ID == searchData.vFunc_ID ||
                    x.FUNC_ID_B == searchData.vFunc_ID, searchData !=null &&
                    !searchData.vFunc_ID.IsNullOrWhiteSpace())
                    .Where(x => x.MAIL_TIME_ID == searchData.vMAIL_TIME_ID,
                    searchData != null && 
                    !searchData.vMAIL_TIME_ID.IsNullOrWhiteSpace())
                    .Where(x => x.APPR_STATUS == searchData.vAPPR_STATUS, 
                    searchData != null && 
                    !searchData.vAPPR_STATUS.IsNullOrWhiteSpace() &&
                    searchData.vAPPR_STATUS != "All")
                    .Where(x => x.APLY_NO == aply_No, !aply_No.IsNullOrWhiteSpace())
                    .AsEnumerable()
                    .Select(x => new TreasuryMailTimeHistoryViewModel()
                    {
                        vAPLY_DATE = TypeTransfer.dateTimeNToString(x.APLY_DATE),
                        vAPLY_NO = x.APLY_NO,
                        vAPLY_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                        Act = _EXEC_ACTION.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE,
                        vSEND_TIME = x.SEND_TIME,
                        vSEND_TIME_B = x.SEND_TIME_B,
                        vFUNC_ID = x.FUNC_ID,
                        vFUNC_ID_B = x.FUNC_ID_B,
                        vINTERVAL_MIN = x.INTERVAL_MIN?.ToString(),
                        vINTERVAL_MIN_B = x.INTERVAL_MIN_B?.ToString(),
                        vEXEC_TIME_B = x.EXEC_TIME_B,
                        vEXEC_TIME_B_B = x.EXEC_TIME_B_B,
                        vEXEC_TIME_E = x.EXEC_TIME_E,
                        vEXEC_TIME_E_B = x.EXEC_TIME_E_B,
                        vTREA_OPEN_TIME = x.TREA_OPEN_TIME,
                        vTREA_OPEN_TIME_B = x.TREA_OPEN_TIME_B,
                        vMAIL_CONTENT_ID = x.MAIL_CONTENT_ID,
                        vMAIL_CONTENT_ID_B = x.MAIL_CONTENT_ID_B,
                        vMEMO = x.MEMO,
                        vMEMO_B = x.MEMO_B,
                        vIS_DISABLED = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE,
                        vIS_DISABLED_B = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED_B)?.CODE_VALUE,
                        vAPPR_STATUS = _Appr_Status.FirstOrDefault(y=>y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        vAPPR_DESC = x.APPR_DESC
                    }).ToList();
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
            var searchData = (TreasuryMailTimeSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var datas = (List<TreasuryMailTimeViewModel>)saveData;
                    if(datas.Any(x=>x.updateFlag))
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {

                            string logStr = string.Empty; //log

                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            string _Aply_No = $@"G3{qPreCode}{sysSeqDao.qrySeqNo("G3", qPreCode).ToString().PadLeft(3, '0')}"; //申請單號 G3+系統日期YYYMMDD(民國年)+3碼流水號

                            foreach (var data in datas.Where(x=>x.updateFlag))
                            {
                                var _MT = db.MAIL_TIME.First(x => x.MAIL_TIME_ID == data.vMAIL_TIME_ID);

                                if (_MT.DATA_STATUS != "1")
                                {
                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                    return result;
                                }
                                _MT.DATA_STATUS = "2"; //凍結中
                                _MT.LAST_UPDATE_UID = searchData.userId;
                                _MT.LAST_UPDATE_DT = dt;
                                _MT.FREEZE_DT = dt;
                                _MT.FREEZE_UID = searchData.userId;
                                
                                var MTH = new MAIL_TIME_HIS()
                                {
                                    APLY_NO = _Aply_No,
                                    MAIL_TIME_ID = _MT.MAIL_TIME_ID,
                                    EXEC_ACTION = "U", //只有修改
                                    FUNC_ID = data.vFUNC_ID,
                                    FUNC_ID_B = _MT.FUNC_ID,
                                    SEND_TIME = data.vSEND_TIME,
                                    SEND_TIME_B = _MT.SEND_TIME,
                                    INTERVAL_MIN = TypeTransfer.stringToIntN(data.vINTERVAL_MIN),
                                    INTERVAL_MIN_B = _MT.INTERVAL_MIN,
                                    TREA_OPEN_TIME = data.vTREA_OPEN_TIME,
                                    TREA_OPEN_TIME_B = _MT.TREA_OPEN_TIME,
                                    EXEC_TIME_B = data.vEXEC_TIME_B,
                                    EXEC_TIME_B_B = _MT.EXEC_TIME_B,
                                    EXEC_TIME_E = data.vEXEC_TIME_E,
                                    EXEC_TIME_E_B = _MT.EXEC_TIME_E,
                                    MAIL_CONTENT_ID = data.vMAIL_CONTENT_ID,
                                    MAIL_CONTENT_ID_B = _MT.MAIL_CONTENT_ID,
                                    MEMO = data.vMEMO,
                                    MEMO_B = _MT.MEMO,
                                    IS_DISABLED = data.vIS_DISABLED,
                                    IS_DISABLED_B = _MT.IS_DISABLED,
                                    APLY_UID = searchData.userId,
                                    APLY_DATE = dt,
                                    APPR_STATUS = "1", //表單申請
                                };

                                logStr += MTH.modelToString(logStr);
                                db.MAIL_TIME_HIS.Add(MTH);

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
                                    log.CFUNCTION = "申請覆核-mail發送內文設定檔";
                                    log.CACTION = "A";
                                    log.CCONTENT = logStr;
                                    LogDao.Insert(log, searchData.userId);
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
        /// <param name="aplyNos">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <param name="desc">覆核意見</param>
        /// <returns></returns>
        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId, string desc)
        {
            foreach (var aplyNo in aplyNos)
            {
                foreach (var MTH in db.MAIL_TIME_HIS.Where(x => x.APLY_NO == aplyNo))
                {
                    MTH.APPR_UID = userId;
                    MTH.APPR_DATE = dt;
                    MTH.APPR_STATUS = "3"; //退回
                    if (!desc.IsNullOrWhiteSpace())
                        MTH.APPR_DESC = desc;
                    logStr += MTH.modelToString(logStr);
                    if (!MTH.MAIL_TIME_ID.IsNullOrWhiteSpace())
                    {
                        var MT = db.MAIL_TIME.First(x => x.MAIL_TIME_ID == MTH.MAIL_TIME_ID);
                        MT.FREEZE_DT = null;
                        MT.FREEZE_UID = null;
                        MT.APPR_UID = userId;
                        MT.APPR_DT = dt;
                        MT.DATA_STATUS = "1"; //可異動
                        logStr += MT.modelToString(logStr);
                    }
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 金庫進出管理作業-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="aplyNos">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <param name="userId">覆核人ID</param>
        /// <returns></returns>
        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId)
        {
            foreach (var aplyNo in aplyNos)
            {
                foreach (var MTH in db.MAIL_TIME_HIS.Where(x => x.APLY_NO == aplyNo))
                {
                    MTH.APPR_UID = userId;
                    MTH.APPR_DATE = dt;
                    MTH.APPR_STATUS = "2"; //覆核完成
                    logStr += MTH.modelToString(logStr);
                    var _MAIL_CONTENT_ID = string.Empty;

                    var MT = db.MAIL_TIME.First(x => x.MAIL_TIME_ID == MTH.MAIL_TIME_ID);
                    MT.FREEZE_DT = null;
                    MT.FREEZE_UID = null;
                    MT.APPR_UID = userId;
                    MT.APPR_DT = dt;
                    MT.DATA_STATUS = "1"; //可異動
                    MT.FUNC_ID = MTH.FUNC_ID;
                    MT.SEND_TIME = MTH.SEND_TIME;
                    MT.INTERVAL_MIN = MTH.INTERVAL_MIN;
                    MT.TREA_OPEN_TIME = MTH.TREA_OPEN_TIME;
                    MT.EXEC_TIME_B = MTH.EXEC_TIME_B;
                    MT.EXEC_TIME_E = MTH.EXEC_TIME_E;
                    MT.MAIL_CONTENT_ID = MTH.MAIL_CONTENT_ID;
                    MT.MEMO = MTH.MEMO;
                    MT.IS_DISABLED = MTH.IS_DISABLED;
                    logStr += MT.modelToString(logStr);
                }

            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        #endregion
    }
}