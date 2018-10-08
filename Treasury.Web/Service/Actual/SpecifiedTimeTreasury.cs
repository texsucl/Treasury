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

namespace Treasury.Web.Service.Actual
{
    public class SpecifiedTimeTreasury : ISpecifiedTimeTreasury
    {
        public SpecifiedTimeTreasury()
        {

        }

        /// <summary>
        /// 取金庫存取項目
        /// </summary>
        /// <returns></returns>
        public Tuple<List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, string> GetTreaItem()
        {
            List<CheckBoxListInfo> result1 = new List<CheckBoxListInfo>();
            List<CheckBoxListInfo> result2 = new List<CheckBoxListInfo>();
            List<CheckBoxListInfo> result3 = new List<CheckBoxListInfo>();
            List<CheckBoxListInfo> result4 = new List<CheckBoxListInfo>();
            string mailId = "";
            try
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    //作業類型1
                    var _data1 = db.TREA_ITEM.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == "1")
                        .OrderBy(x => x.ITEM_ID);
                    result1.AddRange(_data1.AsEnumerable()
                        .Select(x => new CheckBoxListInfo()
                        {
                            DisplayText = x.ITEM_DESC,
                            Value = x.ITEM_ID,
                            IsChecked = false
                        }
                        ));
                    //作業類型2
                    var _data2 = db.TREA_ITEM.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == "2")
                        .OrderBy(x => x.ITEM_ID);
                    result2.AddRange(_data2.AsEnumerable()
                        .Select(x => new CheckBoxListInfo()
                        {
                            DisplayText = x.ITEM_DESC,
                            Value = x.ITEM_ID,
                            IsChecked = false
                        }
                        ));
                    //作業類型3
                    var _data3 = db.TREA_ITEM.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == "3")
                        .OrderBy(x => x.ITEM_ID);
                    result3.AddRange(_data3.AsEnumerable()
                        .Select(x => new CheckBoxListInfo()
                        {
                            DisplayText = x.ITEM_DESC,
                            Value = x.ITEM_ID,
                            IsChecked = false
                        }
                        ));
                    //作業類型4
                    var _data4 = db.TREA_ITEM.AsNoTracking()
                        .Where(x => x.ITEM_OP_TYPE == "4")
                        .OrderBy(x => x.ITEM_ID);
                    result4.AddRange(_data4.AsEnumerable()
                        .Select(x => new CheckBoxListInfo()
                        {
                            DisplayText = x.ITEM_DESC,
                            Value = x.ITEM_ID,
                            IsChecked = false
                        }
                        ));
                    //內文編號(EmailId)
                    var _emailId = db.MAIL_RECEIVE.AsNoTracking()
                        .Where(x => x.FUNC_ID == "0000000013");
                    mailId = _emailId.Select(x => x.MAIL_CONTENT_ID).FirstOrDefault().PadLeft(2, '0');
                }
            }
            catch (Exception ex)
            {
                var message = ex.exceptionMessage();
                throw ex;
            }
            return new Tuple<List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, string>(result1, result2, result3, result4, mailId);
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<SpecifiedTimeTreasurySearchDetailViewModel> GetSearchDetail(SpecifiedTimeTreasurySearchViewModel data)
        {

            List<SpecifiedTimeTreasurySearchDetailViewModel> result = new List<SpecifiedTimeTreasurySearchDetailViewModel>();

            if (!data.vAPLY_DT_S.Any() || !data.vAPLY_DT_E.Any()) // 無查詢日期		
                return result;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();

                var _data = db.TREA_OPEN_REC.AsNoTracking()
                    .Where(x => x.OPEN_TREA_TYPE == "2") //開庫類型: 2.指定時間開庫
                    .Where(x => x.CREATE_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.CREATE_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID, !data.vTREA_REGISTER_ID.IsNullOrWhiteSpace()) //工作單號   
                    .OrderBy(x => x.CREATE_DT);

                var _item = db.TREA_ITEM.AsNoTracking()
                    .Select(x => new { x.ITEM_ID, x.ITEM_DESC }).ToList();

                var _query =
                    _data.Join(db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "APPR_STATUS"),
                    x => x.APPR_STATUS, z => z.CODE, (x, z) => new { open = x, sysCode = z })
                    .AsEnumerable()
                    .Select((x, index) => new SpecifiedTimeTreasurySearchDetailViewModel()
                    {
                        vINDEX = (index + 1).ToString(),
                        vCREATE_DT = x.open.CREATE_DT?.ToString("yyyy/MM/dd"),
                        vTREA_REGISTER_ID = x.open.TREA_REGISTER_ID,
                        vAPLY_STATUS = x.sysCode.CODE_VALUE,
                        vOPEN_TREA_REASON = "取" +
                        string.Join("及", x.open.OPEN_TREA_REASON.Split(';').ToList()
                        .Select(z => _item.FirstOrDefault(y => y.ITEM_ID == z)?.ITEM_DESC)
                        .Where(z => z != null)
                        ),
                        vOPEN_TREA_TIME = x.open.OPEN_TREA_TIME,
                        vMEMO = x.open.MEMO,
                        vOPEN_TREA_DATE = x.open.OPEN_TREA_DATE.ToString("yyyy/MM/dd"),
                        vOPEN_TREA_REASON_ID = x.open.OPEN_TREA_REASON,
                        vEXEC_TIME_B = x.open.EXEC_TIME_B,
                        vEXEC_TIME_E = x.open.EXEC_TIME_E,
                        vCREATE_UID = x.open.CREATE_UID,
                        vAPLY_STATUS_ID = x.open.APPR_STATUS,
                        vAPPR_UID = x.open.APPR_UID
                    });

                result.AddRange(_query);
            }
            return result;
        }

        /// <summary>
        /// 新增申請覆核
        /// </summary>
        /// <param name="data"></param>
        /// <param name="currentUserId">目前使用者ID</param>
        /// <returns></returns>
        public MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> InsertApplyData(SpecifiedTimeTreasuryApplyViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.Apply_Audit_Fail.GetDescription();
            string logStr = string.Empty; //log    
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _data = db.TREA_OPEN_REC
                    .Add(MappingTableForInsert(data, currentUserId));

                logStr += _data.modelToString(logStr);

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
                        log.CFUNCTION = "申請覆核";
                        log.CACTION = "C";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, currentUserId);
                        #endregion
                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = Ref.MessageType.Apply_Audit_Success.GetDescription(); ;
                    }
                    catch (DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }

                if (result.RETURN_FLAG)
                {
                    result.Datas = GetSearchDetail(searchData);
                }
            }
            return result;
        }

        /// <summary>
        /// 修改申請覆核
        /// </summary>
        /// <param name="data"></param>
        /// <param name="currntUserId">目前使用者ID</param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> UpdateApplyData(SpecifiedTimeTreasuryUpdateViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            string logStr = string.Empty; //log    
                                          //bool 登入者與申請者為同一人
            if (data.vCREATE_UID == currentUserId)
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _data = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID);
                    //統一取系統時間
                    DateTime dt = DateTime.Now;
                    if (_data != null)
                    {
                        _data.EXEC_TIME_B = data.vEXEC_TIME_B;
                        _data.EXEC_TIME_E = data.vEXEC_TIME_E;
                        _data.OPEN_TREA_TIME = data.vOPEN_TREA_TIME;
                        _data.MEMO = data.vMEMO;
                        _data.OPEN_TREA_REASON = string.Join(";", data.vOPEN_TREA_REASON_ID.ToArray());
                        _data.APPR_STATUS = "1";
                        _data.LAST_UPDATE_UID = currentUserId;
                        _data.LAST_UPDATE_DT = dt;

                        logStr += _data.modelToString(logStr);

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
                                log.CFUNCTION = "修改申請";
                                log.CACTION = "U";
                                log.CCONTENT = logStr;
                                LogDao.Insert(log, currentUserId);
                                #endregion

                                result.RETURN_FLAG = true;
                                result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                            }
                            catch (DbUpdateException ex)
                            {
                                result.DESCRIPTION = ex.exceptionMessage();
                            }
                        }
                    }
                    else
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                    }
                }
            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetSearchDetail(searchData);
            }
            return result;
        }

        public MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> CancelApplyData(SpecifiedTimeTreasuryCancelViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData)
        {
            var result = new MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.update_Fail.GetDescription();
            string logStr = string.Empty; //log    
            //bool 登入者與申請者為同一人
            if (data.vCREATE_UID == currentUserId)
            {
                if (data.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._1).ToString() || data.vAPLY_STATUS_ID == ((int)Ref.ApplyStatus._3).ToString())
                {
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
                        var _data = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID);
                        //統一取系統時間
                        DateTime dt = DateTime.Now;
                        if (_data != null)
                        {
                            if (_data.APPR_UID == null)
                            {
                                db.TREA_OPEN_REC.Remove(_data);
                            }
                            else
                            {
                                _data.APPR_STATUS = "4";
                                _data.LAST_UPDATE_UID = currentUserId;
                                _data.LAST_UPDATE_DT = dt;
                            }

                            logStr += _data.modelToString(logStr);

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

                                    if (data.vAPPR_UID == null)
                                    {
                                        result.DESCRIPTION = Ref.MessageType.delete_Success.GetDescription();
                                        #region LOG
                                        //新增LOG
                                        Log log = new Log();
                                        log.CFUNCTION = "取消覆核(作廢)";
                                        log.CACTION = "D";
                                        log.CCONTENT = logStr;
                                        LogDao.Insert(log, currentUserId);
                                        #endregion

                                    }
                                    else
                                    {
                                        result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription(); ;
                                        #region LOG
                                        //新增LOG
                                        Log log = new Log();
                                        log.CFUNCTION = "取消覆核";
                                        log.CACTION = "U";
                                        log.CCONTENT = logStr;
                                        LogDao.Insert(log, currentUserId);
                                        #endregion
                                    }

                                    result.RETURN_FLAG = true;

                                }
                                catch (DbUpdateException ex)
                                {
                                    result.DESCRIPTION = ex.exceptionMessage();
                                }
                            }
                        }
                        else
                        {
                            result.DESCRIPTION = Ref.MessageType.not_Find_Audit_Data.GetDescription();
                        }
                    }
                }
                else
                {
                    result.DESCRIPTION = Ref.MessageType.parameter_Error.GetDescription();
                }

            }
            if (result.RETURN_FLAG)
            {
                result.Datas = GetSearchDetail(searchData);
            }
            return result;
        }

        /// <summary>
        /// 覆核作業查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<SpecifiedTimeTreasuryApprSearchDetailViewModel> GetApprSearchData(SpecifiedTimeTreasuryApprSearchViewModel data)
        {
            List<SpecifiedTimeTreasuryApprSearchDetailViewModel> result = new List<SpecifiedTimeTreasuryApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                var emps = GetEmps();
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();
                result = db.TREA_OPEN_REC.AsNoTracking()
                    .Where(x => x.OPEN_TREA_TYPE == "2")
                    .Where(x => x.CREATE_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.CREATE_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID, !data.vTREA_REGISTER_ID.IsNullOrWhiteSpace()) //工作單號
                    .Where(x => x.CREATE_UNIT == data.vCreateUnit) //相同部門
                    .Where(x => x.APPR_STATUS == "1")
                    .AsEnumerable()
                    .Select(x => new SpecifiedTimeTreasuryApprSearchDetailViewModel()
                    {
                        vCREATE_DT = x.CREATE_DT?.ToString("yyyy/MM/dd"),
                        vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        vCREATE_UID = x.CREATE_UID,
                        vCREATE_NAME = emps.FirstOrDefault(y => y.USR_ID == x.CREATE_UID)?.EMP_NAME,
                        vOPEN_TREA_DATE = x.OPEN_TREA_DATE.ToString("yyyy/MM/dd"),
                        vEXEC_TIME_B = x.EXEC_TIME_B,
                        vEXEC_TIME_E = x.EXEC_TIME_E,
                        vOPEN_TREA_TIME = x.OPEN_TREA_TIME,
                        vMEMO = x.MEMO,
                        vOPEN_TREA_REASON_ID = x.OPEN_TREA_REASON,
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT,
                        vAPPR_FLAG = x.CREATE_UID != data.vCreateUid
                    }).ToList();
            }
            return result;
        }

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <param name="ViewModel"></param>
        /// <param name="SearchData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> ApproveData(List<string> RegisterNo, List<SpecifiedTimeTreasuryApprSearchDetailViewModel> ViewModel, SpecifiedTimeTreasuryApprSearchViewModel SearchData)
        {
            var result = new MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!ViewModel.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStrC = string.Empty; //combin Log

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_ITEM = db.TREA_ITEM.Where(x => x.ITEM_OP_TYPE == "3").Select(x => x.ITEM_ID).ToList();

                foreach (var item in ViewModel.Where(x => RegisterNo.Contains(x.vTREA_REGISTER_ID)))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.vTREA_REGISTER_ID);
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vTREA_REGISTER_ID}");
                        return result;
                    }
                    var applyStatus = ((int)Ref.ApplyStatus._2).ToString(); // 狀態: 覆核完成
                    string logStrB = string.Empty; //修改前 Log
                    string logStrA = string.Empty; //修改後 Log

                    logStrB += _TREA_OPEN_REC.modelToString(logStrB);

                    _TREA_OPEN_REC.APPR_STATUS = applyStatus;
                    _TREA_OPEN_REC.LAST_UPDATE_UID = SearchData.vCreateUid;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;
                    _TREA_OPEN_REC.APPR_UID = SearchData.vCreateUid;
                    _TREA_OPEN_REC.APPR_DT = dt;

                    logStrA += _TREA_OPEN_REC.modelToString(logStrA);

                    //#region 申請單紀錄檔
                    ////取得流水號
                    //List<string> workList = _TREA_OPEN_REC.OPEN_TREA_REASON.Split(';').ToList();
                    //foreach (var workItem in workList)
                    //{
                    //    if (!_TREA_ITEM.Contains(workItem))
                    //    {
                    //        SysSeqDao sysSeqDao = new SysSeqDao();
                    //        string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                    //        var cId = sysSeqDao.qrySeqNo("G6", qPreCode).ToString().PadLeft(3, '0');

                    //        var _TREA_APLY_REC = db.TREA_APLY_REC
                    //            .Add(new TREA_APLY_REC()
                    //            {
                    //                APLY_NO = $@"G6{qPreCode}{cId}", ////申請單號 G6+系統日期YYYMMDD(民國年)+3碼流水號
                    //                APLY_FROM = "S",
                    //                TREA_REGISTER_ID = item.vTREA_REGISTER_ID,
                    //                ITEM_ID = workItem,
                    //                APLY_STATUS = "C01",
                    //                EXPECTED_ACCESS_DATE = _TREA_OPEN_REC.OPEN_TREA_DATE,
                    //                APLY_UNIT = _TREA_OPEN_REC.CREATE_UNIT,
                    //                APLY_UID = _TREA_OPEN_REC.CREATE_UID,
                    //                APLY_DT = _TREA_OPEN_REC.CREATE_DT,
                    //                APLY_APPR_UID = SearchData.vCreateUid,
                    //                APLY_APPR_DT = dt,
                    //                CREATE_UID = SearchData.vCreateUid,
                    //                CREATE_DT = dt,
                    //                LAST_UPDATE_UID = SearchData.vCreateUid,
                    //                LAST_UPDATE_DT = dt,
                    //                CREATE_UNIT = SearchData.vCreateUnit
                    //            });
                    //        logStrA += _TREA_APLY_REC.modelToString(logStrA);
                    //    } 
                    //}
                    //#endregion

                    #region 申請單暫存檔
                    //取得流水號
                    List<string> workList = _TREA_OPEN_REC.OPEN_TREA_REASON.Split(';').ToList();
                    if(workList.Count != 0)
                    {
                        var data = (from n in db.TREA_APLY_TEMP select n);
                        db.TREA_APLY_TEMP.RemoveRange(data);
                    }
                    foreach (var workItem in workList)
                    {
                        if (!_TREA_ITEM.Contains(workItem))
                        {
                            var _TREA_APLY_TEMP = db.TREA_APLY_TEMP
                                .Add(new TREA_APLY_TEMP()
                                {
                                    ITEM_ID = workItem,
                                });
                        }
                    }
                    #endregion

                        logStrC += string.Format("修改前{0}^^修改後{1}", logStrB, logStrA);
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
                        log.CFUNCTION = "覆核-指定開庫覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStrC;
                        LogDao.Insert(log, SearchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", RegisterNo)} 覆核成功";
                        result.Datas = GetApprSearchData(SearchData);
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
        /// 駁回
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <param name="RejectReason"></param>
        /// <param name="ViewModel"></param>
        /// <param name="SearchData"></param>
        /// <returns></returns>
        public MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> RejectData(List<string> RegisterNo, string RejectReason, List<SpecifiedTimeTreasuryApprSearchDetailViewModel> ViewModel, SpecifiedTimeTreasuryApprSearchViewModel SearchData)
        {
            var result = new MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            DateTime dt = DateTime.Now;
            string logStrC = string.Empty; //combin Log
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                foreach (var item in ViewModel.Where(x => RegisterNo.Contains(x.vTREA_REGISTER_ID)))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.vTREA_REGISTER_ID);
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.vTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.vTREA_REGISTER_ID}");
                        return result;
                    }
                    var applyStatus = ((int)Ref.ApplyStatus._3).ToString(); // 狀態: 退回
                    string logStrB = string.Empty; //修改前 Log
                    string logStrA = string.Empty; //修改後 Log

                    logStrB += _TREA_OPEN_REC.modelToString(logStrB);

                    _TREA_OPEN_REC.APPR_STATUS = applyStatus;
                    _TREA_OPEN_REC.LAST_UPDATE_UID = SearchData.vCreateUid;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;
                    _TREA_OPEN_REC.APPR_DESC = RejectReason;
                    _TREA_OPEN_REC.APPR_UID = SearchData.vCreateUid;
                    _TREA_OPEN_REC.APPR_DT = dt;

                    logStrA += _TREA_OPEN_REC.modelToString(logStrA);
                    logStrC += string.Format("修改前{0}^^修改後{1}", logStrB, logStrA);
                }
                //檢核欄位
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
                        log.CFUNCTION = "駁回-指定開庫覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStrC;
                        LogDao.Insert(log, SearchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", RegisterNo)} 已駁回";
                        result.Datas = GetApprSearchData(SearchData);
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
        /// 查詢工作項目
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <returns></returns>
        public List<SpecifiedTimeTreasuryApprReasonDetailViewModel> GetReasonDetail(List<string> RegisterNo)
        {
            List<SpecifiedTimeTreasuryApprReasonDetailViewModel> result = new List<SpecifiedTimeTreasuryApprReasonDetailViewModel>();
            using(TreasuryDBEntities db = new TreasuryDBEntities())
            {
                result = db.TREA_ITEM.AsNoTracking()
                    .Where(x => RegisterNo.Contains(x.ITEM_ID))
                    .AsEnumerable()
                    .Select(x => new SpecifiedTimeTreasuryApprReasonDetailViewModel() {
                        vITEM_DESC = x.ITEM_DESC,
                        vITEM_OP_TYPE = x.ITEM_OP_TYPE
                    })
                    .ToList();
            }
            return result;
        }

        /// <summary>
        /// 員工資料
        /// </summary>
        /// <returns></returns>
        protected List<V_EMPLY2> GetEmps()
        {
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
            return emps;
        }

        /// <summary>
        /// 部門資料
        /// </summary>
        /// <returns></returns>
        protected List<VW_OA_DEPT> GetDepts()
        {
            var depts = new List<VW_OA_DEPT>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
            }
            return depts;
        }

        /// <summary>
        /// 取得 人員基本資料
        /// </summary>
        /// <param name="cUserID"></param>
        /// <returns></returns>
        public BaseUserInfoModel GetUserInfo(string cUserID)
        {
            BaseUserInfoModel user = new BaseUserInfoModel();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                var _emply = dbINTRA.V_EMPLY2.AsNoTracking().FirstOrDefault(x => x.USR_ID == cUserID);
                if (_emply != null)
                {
                    user.EMP_ID = cUserID;
                    user.EMP_Name = _emply.EMP_NAME?.Trim();
                    user.DPT_ID = _emply.DPT_CD?.Trim();
                    user.DPT_Name = _emply.DPT_NAME?.Trim();
                }
            }
            return user;
        }

        private TREA_OPEN_REC MappingTableForInsert(SpecifiedTimeTreasuryApplyViewModel data, string currentUserId)
        {
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            string qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var cId = sysSeqDao.qrySeqNo("W", qPreCode).ToString().PadLeft(2, '0');

            //統一取系統時間
            DateTime dt = DateTime.Now;
            return new TREA_OPEN_REC()
            {
                TREA_REGISTER_ID = $@"W{qPreCode}{cId}", //申請單號 W+系統日期YYYMMDD(民國年)+2碼流水號,
                OPEN_TREA_TYPE = "2", //開庫類型: 指定時間開庫
                OPEN_TREA_REASON = string.Join(";", data.vOPEN_TREA_REASON_ID.ToArray()),
                OPEN_TREA_TIME = data.vOPEN_TREA_TIME,
                EXEC_TIME_B = data.vEXEC_TIME_B,
                EXEC_TIME_E = data.vEXEC_TIME_E,
                MEMO = data.vMEMO,
                APPR_STATUS = "1", //覆核狀態: 表單申請							  
                REGI_STATUS = "C02", // 登記簿狀態					 
                CREATE_UID = currentUserId,
                CREATE_DT = dt,
                LAST_UPDATE_UID = currentUserId,
                LAST_UPDATE_DT = dt,
                OPEN_TREA_DATE = Convert.ToDateTime(data.vOPEN_TREA_DATE),
                CREATE_UNIT = GetUserInfo(currentUserId).DPT_ID
            };
        }
    }
}