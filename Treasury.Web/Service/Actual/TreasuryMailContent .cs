using System;
using System.Collections.Generic;
using System.Data.Entity;
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
/// 功能說明：金庫進出管理作業-mail發送內文設定檔維護作業
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
    public class TreasuryMailContent : Common , ITreasuryMailContent
    {
        #region GetData

        /// <summary>
        /// 查詢內文編號
        /// </summary>
        /// <param name="allFlag"></param>
        /// <returns></returns>
        public List<SelectOption> Get_MAIL_ID(bool allFlag = true,bool disabledFlag = false)
        {
            List<SelectOption> result = new List<SelectOption>();
            if (allFlag)
                result.Add(new SelectOption() { Text = "All", Value = "All" });
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result.AddRange(db.MAIL_CONTENT.AsNoTracking()
                    .Where(x => x.IS_DISABLED != "Y", disabledFlag)
                    .Select(x => x.MAIL_CONTENT_ID)
                    .Select(x => new SelectOption() { Text = x, Value = x }));
            }

            return result;
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (TreasuryMailContentSearchViewModel)searchModel;
            List<TreasuryMailContentViewModel> result = new List<TreasuryMailContentViewModel>();

            if (searchData == null)
                return result;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();

                var _Is_Disabled = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();
                var _DATA_STATUS = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                var his = db.MAIL_CONTENT_HIS.AsNoTracking()
                    .Where(x => x.APPR_DATE == null).ToList();

                result.AddRange(db.MAIL_CONTENT.AsNoTracking()
                    .Where(x => x.MAIL_CONTENT_ID == searchData.vMAIL_CONTENT_ID, searchData.vMAIL_CONTENT_ID != "All")
                    .Where(x => x.IS_DISABLED == searchData.vIS_DISABLED, searchData.vIS_DISABLED != "All")
                    .AsEnumerable()
                    .Select((x) => new TreasuryMailContentViewModel()
                    {
                        vMAIL_CONTENT_ID = x.MAIL_CONTENT_ID,
                        vMAIL_SUBJECT = x.MAIL_SUBJECT,
                        vMAIL_CONTENT = x.MAIL_CONTENT1,
                        vIS_DISABLED = x.IS_DISABLED,
                        vIS_DISABLED_D = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE,
                        vStatus = x.DATA_STATUS,
                        vStatus_D = _DATA_STATUS.FirstOrDefault(y => y.CODE == x.DATA_STATUS)?.CODE_VALUE,
                        vAPLY_NO = x.DATA_STATUS != "1" ? his.FirstOrDefault(y => y.MAIL_CONTENT_ID == x.MAIL_CONTENT_ID)?.APLY_NO : "",
                        vFREEZE_UID = x.FREEZE_UID,
                        vFREEZE_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim(),
                        vLAST_UPDATE_UID = x.LAST_UPDATE_UID,
                        vLAST_UPDATE_UID_Name = emps.FirstOrDefault(y => y.USR_ID == x.LAST_UPDATE_UID)?.EMP_NAME?.Trim(),
                        vLAST_UPDATE_DATE = x.LAST_UPDATE_DT.dateTimeToStr(),
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT
                    }).OrderBy(x=>x.vMAIL_CONTENT_ID).ToList());
            }
            return result;
        }

        /// <summary>
        /// 查詢資料By內文編號
        /// </summary>
        /// <param name="MAIL_CONTENT_ID"></param>
        /// <returns></returns>
        public ITinItem GetUpdateData(string MAIL_CONTENT_ID)
        {
            TreasuryMailContentUpdateViewModel result = new TreasuryMailContentUpdateViewModel();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();

                var _CODE_FUNC = db.CODE_FUNC.AsNoTracking().ToList();

                var _sysCodes = db.SYS_CODE.AsNoTracking().ToList();

                var _Is_Disabled = _sysCodes
                    .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();
                var _DATA_STATUS = _sysCodes
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                var data = db.MAIL_CONTENT.AsNoTracking()
                    .FirstOrDefault(x => x.MAIL_CONTENT_ID == MAIL_CONTENT_ID);
                if (data != null)
                {
                    result.vMAIL_CONTENT_ID = data.MAIL_CONTENT_ID;
                    result.vIS_DISABLED = data.IS_DISABLED;
                    result.vMAIL_SUBJECT = data.MAIL_SUBJECT;
                    result.vMAIL_CONTENT = data.MAIL_CONTENT1;
                    result.vLAST_UPDATE_DT = data.LAST_UPDATE_DT;
                    result.subData = GetReceiveData(data.MAIL_CONTENT_ID);
                }
            }

            return result;
        }

        /// <summary>
        /// 查詢新增的功能編號
        /// </summary>
        /// <returns></returns>
        public List<SelectOption> GetFUNC_ID(List<string> func_Ids)
        {
            var result = new List<SelectOption>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.CODE_FUNC.AsNoTracking()
                       .Where(x => !string.IsNullOrEmpty(x.FUNC_URL))
                       .Where(x => x.IS_DISABLED != "Y")
                       .Where(x => !func_Ids.Contains(x.FUNC_ID))
                       .AsEnumerable()
                       .Select(x => new SelectOption()
                       {
                           Value = x.FUNC_ID,
                           Text = x.FUNC_NAME
                       }).ToList() ;
            }
            return result;
        }

        /// <summary>
        /// 查詢 mail發送對象設定檔
        /// </summary>
        /// <param name="MAIL_CONTENT_ID"></param>
        /// <param name="aply_No"></param>
        /// <returns></returns>
        public List<TreasuryMailReceivelViewModel> GetReceiveData(string MAIL_CONTENT_ID, string aply_No = null)
        {
            var result = new List<TreasuryMailReceivelViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _CODE_FUNC = db.CODE_FUNC.AsNoTracking().ToList();
                if (MAIL_CONTENT_ID.IsNullOrWhiteSpace())
                {
                    var _DATA_STATUS = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();

                    result = db.MAIL_RECEIVE_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .AsEnumerable()
                        .Select(x => new TreasuryMailReceivelViewModel()
                        {
                            FUNC_ID = x.FUNC_ID,
                            FUNC_ID_Name = _CODE_FUNC.FirstOrDefault(y => y.FUNC_ID == x.FUNC_ID)?.FUNC_NAME,
                            vStatus = x.EXEC_ACTION,
                            vStatus_D = _DATA_STATUS.FirstOrDefault(y=>y.CODE == x.EXEC_ACTION)?.CODE_VALUE,
                        }).ToList();
                }
                else
                {             
                    result = db.MAIL_RECEIVE.AsNoTracking()
                        .Where(x => x.MAIL_CONTENT_ID == MAIL_CONTENT_ID)
                        .Where(x => x.DATA_STATUS == "1")
                        .AsEnumerable()
                        .Select(x => new TreasuryMailReceivelViewModel()
                        {
                            FUNC_ID = x.FUNC_ID,
                            FUNC_ID_Name = _CODE_FUNC.FirstOrDefault(y => y.FUNC_ID == x.FUNC_ID)?.FUNC_NAME,                      
                        }).ToList();
                }
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
            var searchData = (TreasuryMailContentHistorySearchViewModel)searchModel;
            List<TreasuryMailContentHistoryViewModel> result = new List<TreasuryMailContentHistoryViewModel>();

            //if (searchData == null)
            //    return result;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();

                var _CODE_FUNC = db.CODE_FUNC.AsNoTracking().ToList();

                var _sysCodes = db.SYS_CODE.AsNoTracking().ToList();

                var _EXEC_ACTION = _sysCodes
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Is_Disabled = _sysCodes
                    .Where(x => x.CODE_TYPE == "IS_DISABLED").ToList();
                var _Appr_Status = _sysCodes
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();

                DateTime? _AplyDate = null;
                if (searchData != null)
                    _AplyDate = TypeTransfer.stringToDateTimeN(searchData.vAply_Date);

                var his = db.MAIL_RECEIVE_HIS.AsNoTracking()
                    .Where(x => x.MAIL_CONTENT_ID == searchData.vMAIL_CONTENT_ID, 
                    searchData != null && 
                    !searchData.vMAIL_CONTENT_ID.IsNullOrWhiteSpace() &&
                    searchData.vMAIL_CONTENT_ID != "All")
                    .Where(x => x.APLY_NO == aply_No, !aply_No.IsNullOrWhiteSpace())
                    .ToList();

                result = db.MAIL_CONTENT_HIS.AsNoTracking()
                    .Where(x => x.MAIL_CONTENT_ID == searchData.vMAIL_CONTENT_ID, 
                    searchData != null &&
                    !searchData.vMAIL_CONTENT_ID.IsNullOrWhiteSpace() &&
                    searchData.vMAIL_CONTENT_ID != "All")
                    .Where(x => x.APPR_STATUS == searchData.vAPPR_STATUS, searchData != null && !searchData.vAPPR_STATUS.IsNullOrWhiteSpace() && searchData.vAPPR_STATUS != "All" )
                    .Where(x => DbFunctions.TruncateTime(x.APLY_DATE) == _AplyDate, _AplyDate != null)
                    .Where(x => x.APLY_NO == aply_No, !aply_No.IsNullOrWhiteSpace())
                    .AsEnumerable()
                    .Select(x => new TreasuryMailContentHistoryViewModel()
                    {
                        APLY_NO = x.APLY_NO,
                        APLY_DT = TypeTransfer.dateTimeToString(x.APLY_DATE,false),
                        APLY_UID = emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim(),
                        vIS_DISABLED_B = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED_B)?.CODE_VALUE,
                        vIS_DISABLED = _Is_Disabled.FirstOrDefault(y => y.CODE == x.IS_DISABLED)?.CODE_VALUE,
                        vMAIL_CONTENT_B = x.MAIL_CONTENT_B,
                        vMAIL_CONTENT = x.MAIL_CONTENT,
                        vMAIL_SUBJECT_B = x.MAIL_SUBJECT_B,
                        vMAIL_SUBJECT = x.MAIL_SUBJECT,
                        vAPPR_DESC = x.APPR_DESC,
                        vAPPR_STATUS = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE,
                        FunFlag = his.Any(y => y.APLY_NO == x.APLY_NO && x.MAIL_CONTENT_ID == y.MAIL_CONTENT_ID) ? "Y" : "N",
                        subData = his.Where(y => y.APLY_NO == x.APLY_NO && x.MAIL_CONTENT_ID == y.MAIL_CONTENT_ID)
                                  .AsEnumerable()
                                  .Select(z => new TreasuryMailReceivelViewModel() {
                                      FUNC_ID = z.FUNC_ID,
                                      FUNC_ID_Name = _CODE_FUNC.FirstOrDefault(y => y.FUNC_ID == z.FUNC_ID)?.FUNC_NAME,
                                      vStatus = z.EXEC_ACTION,
                                      vStatus_D = _EXEC_ACTION.FirstOrDefault(y => y.CODE == z.EXEC_ACTION)?.CODE_VALUE,
                                  }).ToList()
                    }).OrderBy(x=>x.APLY_NO).ToList();
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
            var searchData = (TreasuryMailContentSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var data = ((List<TreasuryMailContentUpdateViewModel>)saveData).FirstOrDefault();
                    if(data != null)
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];

                            string logStr = string.Empty; //log
                         
                            var _MAIL_CONTENT_ID = string.Empty;
                            MAIL_CONTENT _MC = null;
                            if (!data.vMAIL_CONTENT_ID.IsNullOrWhiteSpace()) //現有資料修改
                            {
                                _MAIL_CONTENT_ID = data.vMAIL_CONTENT_ID;
                                _MC = db.MAIL_CONTENT.First(x => x.MAIL_CONTENT_ID == _MAIL_CONTENT_ID);
                                if (_MC.DATA_STATUS != "1" )
                                {
                                    result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                    return result;
                                }
                                _MC.DATA_STATUS = "2"; //凍結中
                                _MC.LAST_UPDATE_UID = data.UserID;
                                _MC.LAST_UPDATE_DT = dt;
                                _MC.FREEZE_DT = dt;
                                _MC.FREEZE_UID = data.UserID;
                            }
                            else
                            {
                                //目前只加入異動檔
                            }

                            string _Aply_No  = $@"G4{qPreCode}{sysSeqDao.qrySeqNo("G4", qPreCode).ToString().PadLeft(3, '0')}"; //申請單號 G4+系統日期YYYMMDD(民國年)+3碼流水號

                            var MCH = new MAIL_CONTENT_HIS()
                            {
                                APLY_NO = _Aply_No,                            
                                MAIL_CONTENT_ID = _MAIL_CONTENT_ID,
                                IS_DISABLED =  data.vIS_DISABLED,
                                IS_DISABLED_B = _MC?.IS_DISABLED,
                                MAIL_SUBJECT = data.vMAIL_SUBJECT,
                                MAIL_SUBJECT_B =  _MC?.MAIL_SUBJECT,
                                MAIL_CONTENT = data.vMAIL_CONTENT,
                                MAIL_CONTENT_B =  _MC?.MAIL_CONTENT1,
                                APLY_UID = data.UserID,
                                APLY_DATE = dt,
                                APPR_STATUS = "1", //表單申請
                                EXEC_ACTION = (data.vMAIL_CONTENT_ID != null) ? "U" : "A"
                            };

                            logStr += MCH.modelToString(logStr);
                            db.MAIL_CONTENT_HIS.Add(MCH);

                            foreach (var item in data.subData)
                            {
                                var _MRH = new MAIL_RECEIVE_HIS()
                                {
                                    APLY_NO = _Aply_No,
                                    MAIL_CONTENT_ID = _MAIL_CONTENT_ID,
                                    FUNC_ID = item.FUNC_ID,
                                    EXEC_ACTION = item.vStatus
                                };
                                logStr += _MRH.modelToString(logStr);
                                db.MAIL_RECEIVE_HIS.Add(_MRH);
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
                                    LogDao.Insert(log, data.UserID);
                                    #endregion
                                    result.Datas = GetSearchData(searchModel);
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
                var MCH = db.MAIL_CONTENT_HIS.First(x => x.APLY_NO == aplyNo);
                MCH.APPR_UID = userId;
                MCH.APPR_DATE = dt;
                MCH.APPR_STATUS = "3"; //退回
                if (!desc.IsNullOrWhiteSpace())
                    MCH.APPR_DESC = desc;
                logStr += MCH.modelToString(logStr);
                if (!MCH.MAIL_CONTENT_ID.IsNullOrWhiteSpace())
                {
                    var MC = db.MAIL_CONTENT.First(x => x.MAIL_CONTENT_ID == MCH.MAIL_CONTENT_ID);
                    MC.FREEZE_DT = null;
                    MC.FREEZE_UID = null;
                    MC.APPR_UID = userId;
                    MC.APPR_DT = dt;
                    MC.DATA_STATUS = "1"; //可異動
                    logStr += MC.modelToString(logStr);
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
                var MCH = db.MAIL_CONTENT_HIS.First(x => x.APLY_NO == aplyNo);
                MCH.APPR_UID = userId;
                MCH.APPR_DATE = dt;
                MCH.APPR_STATUS = "2"; //覆核完成               
                var _MAIL_CONTENT_ID = string.Empty;
                List<MAIL_RECEIVE> MRS = new List<MAIL_RECEIVE>();
                if (!MCH.MAIL_CONTENT_ID.IsNullOrWhiteSpace())
                {
                    logStr += MCH.modelToString(logStr);
                    var MC = db.MAIL_CONTENT.First(x => x.MAIL_CONTENT_ID == MCH.MAIL_CONTENT_ID);
                    MC.FREEZE_DT = null;
                    MC.FREEZE_UID = null;
                    MC.APPR_UID = userId;
                    MC.APPR_DT = dt;
                    MC.DATA_STATUS = "1"; //可異動
                    MC.MAIL_SUBJECT = MCH.MAIL_SUBJECT;
                    MC.MAIL_CONTENT1 = MCH.MAIL_CONTENT;
                    MC.IS_DISABLED = MCH.IS_DISABLED;
                    _MAIL_CONTENT_ID = MC.MAIL_CONTENT_ID;
                    logStr += MC.modelToString(logStr);
                    MRS = db.MAIL_RECEIVE.Where(x => x.MAIL_CONTENT_ID == MCH.MAIL_CONTENT_ID).ToList();
                }
                else
                {
                    SysSeqDao sysSeqDao = new SysSeqDao();
                    _MAIL_CONTENT_ID = $@"{sysSeqDao.qrySeqNo("D4", string.Empty).ToString().PadLeft(2, '0')}";
                    MCH.MAIL_CONTENT_ID = _MAIL_CONTENT_ID;
                    logStr += MCH.modelToString(logStr);
                    var MC = new MAIL_CONTENT()
                    {
                        MAIL_CONTENT_ID = _MAIL_CONTENT_ID,
                        MAIL_SUBJECT = MCH.MAIL_SUBJECT,
                        MAIL_CONTENT1 = MCH.MAIL_CONTENT,
                        IS_DISABLED = MCH.IS_DISABLED,
                        CREATE_UID = MCH.APLY_UID,
                        CREATE_DT = MCH.APLY_DATE,
                        LAST_UPDATE_UID = MCH.APLY_UID,
                        LAST_UPDATE_DT = dt,
                        APPR_UID = userId,
                        APPR_DT = dt,
                        DATA_STATUS = "1" //可異動
                    };
                    db.MAIL_CONTENT.Add(MC);
                    logStr += MC.modelToString(logStr);
                }
                foreach (var subitem in db.MAIL_RECEIVE_HIS
                    .Where(x => x.APLY_NO == aplyNo &&
                    x.EXEC_ACTION != null))
                {
                    MAIL_RECEIVE _MR = new MAIL_RECEIVE();
                    switch (subitem.EXEC_ACTION)
                    {
                        case "A":
                            _MR = new MAIL_RECEIVE()
                            {
                                MAIL_CONTENT_ID = _MAIL_CONTENT_ID,
                                FUNC_ID = subitem.FUNC_ID,
                                DATA_STATUS = "1", //可異動
                                CREATE_UID = MCH.APLY_UID,
                                CREATE_DT = MCH.APLY_DATE,
                                APPR_UID = userId,
                                APPR_DT = dt,
                                LAST_UPDATE_UID = MCH.APLY_UID,
                                LAST_UPDATE_DT = MCH.APLY_DATE
                            };
                            logStr += _MR.modelToString(logStr);
                            db.MAIL_RECEIVE.Add(_MR);
                            break;
                        case "D":
                            _MR = MRS.FirstOrDefault(x => x.FUNC_ID == subitem.FUNC_ID);
                            if (_MR != null)
                                db.MAIL_RECEIVE.Remove(_MR);
                            break;
                    }
                }
            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        #endregion
    }
}