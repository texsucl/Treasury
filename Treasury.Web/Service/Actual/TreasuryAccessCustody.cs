using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebBO;
using Treasury.WebDaos;
using Treasury.WebUtility;
using Treasury.Web.Enum;
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{

    public class TreasuryAccessCustody : Common , ITreasuryAccessCustody
    {
        /// <summary>
        /// 保管單位承辦作業
        /// </summary>
        private List<string> custodyStatus { get; set; }

        /// <summary>
        /// 保管單位覆核作業
        /// </summary>
        private string apprStatus { get; set; }

        public TreasuryAccessCustody()
        {
            custodyStatus = new List<string>() {
                Ref.AccessProjectFormStatus.B01.ToString(),
                Ref.AccessProjectFormStatus.B02.ToString(),
                Ref.AccessProjectFormStatus.B03.ToString(),
                Ref.AccessProjectFormStatus.B04.ToString()
            };
            apprStatus = Ref.AccessProjectFormStatus.B02.ToString();
        }

        #region Get Date

        /// <summary>
        /// 保管單位承辦作業 查詢
        /// </summary>
        /// <param name="data">保管單位承辦作業 查詢</param>
        /// <returns></returns>
        public List<TreasuryAccessApprSearchDetailViewModel> GetCustodySearchDetail(TreasuryAccessApprSearchViewModel data)
        {
            List<TreasuryAccessApprSearchDetailViewModel> result = new List<TreasuryAccessApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                var emps = GetEmps();
                var formStatus = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                var treaItems = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "3").ToList();
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();
                result = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.APLY_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.APLY_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.APLY_NO == data.vAPLY_NO, !data.vAPLY_NO.IsNullOrWhiteSpace()) //申請單號
                    .Where(x => custodyStatus.Contains(x.APLY_STATUS)) //符合狀態 的資料
                    .AsEnumerable()
                    .Select(x => TreaAplyRecToTAASDViewModel(data.vCreateUid, x, treaItems, depts, emps, formStatus))
                    .OrderByDescending(x => x.vAPLY_NO).ToList();
            }
            return result;
        }

        /// <summary>
        /// 金庫物品存取申請覆核作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<TreasuryAccessApprSearchDetailViewModel> GetCustodyApprSearchDetail(TreasuryAccessApprSearchViewModel data)
        {
            List<TreasuryAccessApprSearchDetailViewModel> result = new List<TreasuryAccessApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                var emps = GetEmps();
                var formStatus = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                var treaItems = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "3").ToList();
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();
                result = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.APLY_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.APLY_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.APLY_NO == data.vAPLY_NO, !data.vAPLY_NO.IsNullOrWhiteSpace()) //申請單號
                    .Where(x => x.APLY_STATUS == apprStatus) //符合狀態 的資料
                    .AsEnumerable()
                    .Select(x => TreaAplyRecToTAASDViewModel(data.vCreateUid, x, treaItems, depts, emps, formStatus, false))
                    .OrderByDescending(x=>x.vAPLY_NO).ToList();
            }
            return result;
        }

        #endregion

        #region Save Data

        /// <summary>
        /// 保管單位承辦作業-覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> CustodyApproved(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.parameter_Error.GetDescription();

            if (!viewModels.Any())
                return result;

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();

                List<string> checks = new List<string>() { TreaItemType.D1016.ToString(), TreaItemType.D1017.ToString() };

                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == item.vAPLY_NO);
                    if (_TREA_APLY_REC == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > item.vLast_Update_Time) //資料已經被更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    if (_TREA_APLY_REC.ITEM_ID == TreaItemType.D1016.ToString())
                    {
                        var ids = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == _TREA_APLY_REC.APLY_NO).Select(x => x.ITEM_ID).Distinct().ToList();
                        var items = db.ITEM_REFUNDABLE_DEP.AsNoTracking()
                            .Where(x => ids.Contains(x.ITEM_ID)).AsEnumerable()
                            .Where(x => x.BOOK_NO.IsNullOrWhiteSpace())
                            .Select(x => x.ITEM_ID).ToList();
                        if (items.Any())
                        {
                            result.DESCRIPTION = MessageType.book_No_Error.GetDescription($@"存出保證金 歸檔編號:{string.Join(",", items)} , 冊號 不得為空值");
                            return result;
                        }
                    }
                    else if (_TREA_APLY_REC.ITEM_ID == TreaItemType.D1017.ToString())
                    {
                        var ids = db.OTHER_ITEM_APLY.AsNoTracking().Where(x => x.APLY_NO == _TREA_APLY_REC.APLY_NO).Select(x => x.ITEM_ID).Distinct().ToList();
                        var items = db.ITEM_DEP_RECEIVED.AsNoTracking()
                            .Where(x => ids.Contains(x.ITEM_ID)).AsEnumerable()
                            .Where(x => x.BOOK_NO.IsNullOrWhiteSpace())
                            .Select(x => x.ITEM_ID).ToList();
                        if (items.Any())
                        {
                            result.DESCRIPTION = MessageType.book_No_Error.GetDescription($@"存入保證金 歸檔編號:{string.Join(",", items)} , 冊號 不得為空值");
                            return result;
                        }
                    }
                    aplynos.Add(item.vAPLY_NO);
                    var aplyStatus = Ref.AccessProjectFormStatus.B02.ToString(); // 狀態 => 保管科覆核中
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.CUSTODY_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;

                    logStr += _TREA_APLY_REC.modelToString(logStr);

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);
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
                        log.CFUNCTION = "覆核-保管單位承辦作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 覆核成功!";
                        result.Datas = GetCustodySearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

                

            return result;
        }

        /// <summary>
        /// 保管單位承辦作業-駁回
        /// </summary>
        /// <param name="searchData">查詢資料</param>
        /// <param name="viewModels">畫面Cache資料</param>
        /// <param name="apprDesc">駁回訊息</param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> CustodyReject(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels, string apprDesc)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();

            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.parameter_Error.GetDescription();

            if (!viewModels.Any())
                return result;

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();

                foreach (var item in viewModels.Where(x => x.vCheckFlag))
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == item.vAPLY_NO);
                    if (_TREA_APLY_REC == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > item.vLast_Update_Time) //資料已經被更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    aplynos.Add(item.vAPLY_NO);
                    var aplyStatus = Ref.AccessProjectFormStatus.A03.ToString(); // 狀態 => 保管科承辦覆核駁回
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.CUSTODY_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    if (!apprDesc.IsNullOrWhiteSpace())
                        _TREA_APLY_REC.CUSTODY_APPR_DESC = apprDesc;

                    logStr += _TREA_APLY_REC.modelToString(logStr);

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);
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
                        log.CFUNCTION = "駁回-保管單位承辦作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 駁回成功!";
                        result.Datas = GetCustodySearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 保管單位覆核作業-覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Approved(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels)
        {
            var result = new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!viewModels.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x=>x.vCheckFlag))
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC
                        .FirstOrDefault(x => x.APLY_NO == item.vAPLY_NO);
                    if (_TREA_APLY_REC == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null,$"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > item.vLast_Update_Time) //資料已經被更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    aplynos.Add(item.vAPLY_NO);
                    var aplyStatus = Ref.AccessProjectFormStatus.C01.ToString(); // 狀態 => 保管科覆核完成覆核，待入庫人員確認中

                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.CUSTODY_APPR_UID = searchData.vCreateUid;
                    _TREA_APLY_REC.CUSTODY_APPR_DT = dt;

                    logStr += _TREA_APLY_REC.modelToString(logStr);

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);

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
                        log.CFUNCTION = "覆核-保管單位覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 覆核成功!";
                        result.Datas = GetCustodyApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 保管單位覆核作業-駁回
        /// </summary>
        /// <param name="searchData">金庫物品覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Reject(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels ,string apprDesc)
        {
            var result = new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var aplynos = new List<string>();
                foreach (var item in viewModels.Where(x=>x.vCheckFlag))
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC
                        .FirstOrDefault(x => x.APLY_NO == item.vAPLY_NO);
                    if (_TREA_APLY_REC == null) //找不到該筆單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > item.vLast_Update_Time) //資料已經被更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vAPLY_NO}");
                        return result;
                    }
                    aplynos.Add(item.vAPLY_NO);

                    var aplyStatus = Ref.AccessProjectFormStatus.B04.ToString(); // 狀態 => 保管科覆核退回，保管科確認中

                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    if(!apprDesc.IsNullOrWhiteSpace())
                        _TREA_APLY_REC.CUSTODY_APPR_DESC = apprDesc;
                    _TREA_APLY_REC.CUSTODY_APPR_DT = dt;
                    _TREA_APLY_REC.CUSTODY_APPR_UID = searchData.vCreateUid;

                    logStr += _TREA_APLY_REC.modelToString(logStr);

                    #region 申請單歷程檔
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = aplyStatus,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);

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
                        log.CFUNCTION = "駁回-保管單位覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 已駁回!";
                        result.Datas = GetCustodyApprSearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 修改申請單記錄檔
        /// </summary>
        /// <param name="data">修改資料</param>
        /// <param name="custodianFlag">是否為保管科</param>
        /// <param name="searchData">申請表單查詢顯示區塊ViewModel</param>
        /// <param name="userId">userId</param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> updateAplyNo(TreasuryAccessViewModel data, bool custodianFlag, TreasuryAccessApprSearchViewModel searchData, string userId)
        {
            MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>>();
            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var updateData = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == data.vAplyNo);
                if (updateData != null)
                {
                    if (updateData.LAST_UPDATE_DT > data.vLastUpdateTime)
                    {
                        return result;
                    }
                    updateData.LAST_UPDATE_UID = userId;
                    updateData.ACCESS_REASON = data.vAccessReason;
                    updateData.EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(data.vExpectedAccessDate);
                    updateData.LAST_UPDATE_DT = DateTime.Now;
                    try
                    {
                        db.SaveChanges();
                        result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                        result.RETURN_FLAG = true;
                        result.Datas = GetCustodySearchDetail(searchData);
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
        }

        #endregion

        #region private function

        private TreasuryAccessApprSearchDetailViewModel TreaAplyRecToTAASDViewModel(
            string userId,
            TREA_APLY_REC data,
            List<TREA_ITEM> treaItems,
            List<VW_OA_DEPT> depts,
            List<V_EMPLY2> emps,
            List<SYS_CODE> formStatus,
            bool vAPPRFlag = true
            )
        {
            return new TreasuryAccessApprSearchDetailViewModel()
            {
                vItem = data.ITEM_ID,
                vACCESS_TYPE = data.ACCESS_TYPE,
                vAPLY_STATUS = data.APLY_STATUS,
                vAPLY_STATUS_D = formStatus.FirstOrDefault(x => x.CODE == data.APLY_STATUS)?.CODE_VALUE,
                vItemDec = treaItems.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC,
                vAPLY_DT = data.APLY_DT?.ToString("yyyy/MM/dd"),
                vAPLY_NO = data.APLY_NO,
                vAPLY_UNIT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME,
                vAPLY_UID = data.APLY_UID,
                vAPLY_UID_NAME = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME,
                vCUSTODY_UID = emps.FirstOrDefault(x => x.USR_ID == data.CUSTODY_UID)?.EMP_NAME,
                vAPPR_DESC = data.CUSTODY_APPR_DESC.IsNullOrWhiteSpace() ? data.APLY_APPR_DESC : data.CUSTODY_APPR_DESC,
                vAPPRFlag = vAPPRFlag || ( data.CUSTODY_UID != userId),
                vACCESS_REASON = data.ACCESS_REASON,
                vLast_Update_Time = data.LAST_UPDATE_DT
            };
        }

        #endregion
    }
}