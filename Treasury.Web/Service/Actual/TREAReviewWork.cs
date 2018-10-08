﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using Treasury.Web.Enum;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebDaos;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class TREAReviewWork : ITREAReviewWork
    {
        /// <summary>
        /// 取得初始資料
        /// </summary>
        /// <returns></returns>
        public List<TREAReviewWorkDetailViewModel> GetSearchDatas()
        {
            List<TREAReviewWorkDetailViewModel> result = new List<TREAReviewWorkDetailViewModel>();
            string status = Ref.AccessProjectFormStatus.D02.ToString(); //金庫登記簿覆核中

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking();
                var _OPEN_TREA_TYPE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE").ToList();
                var _FORM_STATUS = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                result = _TREA_OPEN_REC
                    .Where(x => x.REGI_STATUS == status)
                    .AsEnumerable()
                    .Select(x => new TREAReviewWorkDetailViewModel {
                        vOPEN_TREA_TYPE = _OPEN_TREA_TYPE.FirstOrDefault(y => y.CODE == x.OPEN_TREA_TYPE)?.CODE_VALUE,
                        vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        hvTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        vOPEN_TREA_TIME = x.OPEN_TREA_TIME,
                        vACTUAL_PUT_TIME = x.ACTUAL_PUT_TIME?.ToString("HH:mm"),
                        vACTUAL_GET_TIME = x.ACTUAL_GET_TIME?.ToString("HH:mm"),
                        vREGI_STATUS = _FORM_STATUS.FirstOrDefault(y => y.CODE == x.REGI_STATUS)?.CODE_VALUE,
                        vLAST_UPDATE_DT = x.LAST_UPDATE_DT,
                        vOPEN_TREA_DATE = x.OPEN_TREA_DATE.ToString("yyyy/MM/dd"),
                        Ischecked = false
                    }).ToList();
            }
            return result;
        }

        public List<TREAReviewWorkSearchDetailViewModel> GetDetailsDatas(string RegisterId)
        {
            List<TREAReviewWorkSearchDetailViewModel> result = new List<TREAReviewWorkSearchDetailViewModel>();

            if (!RegisterId.IsNullOrWhiteSpace())
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking();
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                    var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();
                    var _SYS_CODE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();

                    result = _TREA_APLY_REC.Where(x => x.TREA_REGISTER_ID == RegisterId)
                        .AsEnumerable()
                        .Select(x => new TREAReviewWorkSearchDetailViewModel() {
                            vITeM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE,
                            vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,
                            vSEAL_ITEM = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(y => y.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(z => z.APLY_NO == x.APLY_NO).ITEM_ID)?.SEAL_DESC : null,
                            vACCESS_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACCESS_TYPE)?.CODE_VALUE : null,
                            vAPLY_NO = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "3" ? x.APLY_NO : null,
                            vACCESS_REASON = x.ACCESS_REASON,
                            vCONFIRM_NAME = x.CONFIRM_UID != null ? GetUserInfo(x.CONFIRM_UID)?.EMP_Name : null,
                            vACTUAL_ACCESS_TYPE = x.ACTUAL_ACCESS_TYPE != x.ACCESS_TYPE? _SYS_CODE.FirstOrDefault(y => y.CODE == x.ACTUAL_ACCESS_TYPE)?.CODE_VALUE : null,
                            vACTUAL_ACCESS_NAME = x.ACTUAL_ACCESS_UID != x.CONFIRM_UID ? GetUserInfo(x.ACTUAL_ACCESS_UID)?.EMP_Name : null,
                            vTREA_REGISTER_ID = x. TREA_REGISTER_ID
                        }).ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TREAReviewWorkDetailViewModel>> InsertApplyData(List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId)
        {
            var result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!ViewModel.Any())
            {
                return result;
            }
            DateTime dt = DateTime.Now;
            string logStr = string.Empty; //Log
            string status = Ref.AccessProjectFormStatus.E01.ToString(); // 已完成出入庫，通知申請人員
            List<string> approvedList = new List<string>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                foreach(var item in ViewModel.Where(x => x.Ischecked))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID);
                    var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    _TREA_OPEN_REC.REGI_STATUS = status;
                    _TREA_OPEN_REC.REGI_APPR_UID = cUserId;
                    _TREA_OPEN_REC.REGI_APPR_DT = dt;
                    _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;

                    logStr += _TREA_OPEN_REC.modelToString(logStr);
                    approvedList.Add(item.hvTREA_REGISTER_ID);

                    var _TREA_APLY_REC = db.TREA_APLY_REC.Where(y => y.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID)
                        .Where(y => y.APLY_STATUS == "D02")
                        .ToList();
                    _TREA_APLY_REC.ForEach(y => {
                        y.APLY_STATUS = status;
                        y.LAST_UPDATE_UID = cUserId;
                        y.LAST_UPDATE_DT = dt;

                        logStr += y.modelToString(logStr);

                        var vITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID)?.ITEM_OP_TYPE;
                        var _OTHER_ITEM_APLY_ITEM_ID = db.OTHER_ITEM_APLY.FirstOrDefault(x => x.APLY_NO == y.APLY_NO)?.ITEM_ID;
                        var _ITEM_SEAL = db.ITEM_SEAL.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);
                        if (vITEM_OP_TYPE == "2")
                        {
                            if(_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID).TREA_ITEM_TYPE == "SEAL")
                            {
                                switch (y.ACCESS_TYPE)
                                {
                                    //存入
                                    case "P":
                                    case "S":
                                    case "A":
                                    case "B":
                                        _ITEM_SEAL.INVENTORY_STATUS = "1";
                                        break;
                                    //取出
                                    case "G":
                                        _ITEM_SEAL.INVENTORY_STATUS = "6";
                                        break;
                                }
                                _ITEM_SEAL.LAST_UPDATE_DT = dt;
                                logStr += _ITEM_SEAL.modelToString(logStr);
                            }
                        }
                        else if(vITEM_OP_TYPE == "3")
                        {
                            if (_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID).TREA_ITEM_TYPE == "SEAL")
                            {
                                switch (y.ACCESS_TYPE)
                                {
                                    //存入
                                    case "P":
                                    case "S":
                                    case "A":
                                    case "B":
                                        _ITEM_SEAL.INVENTORY_STATUS = "1";
                                        break;
                                    //取出
                                    case "G":
                                        _ITEM_SEAL.INVENTORY_STATUS = "2";
                                        break;
                                }
                                _ITEM_SEAL.LAST_UPDATE_DT = dt;
                                logStr += _ITEM_SEAL.modelToString(logStr);
                            }
                        }
                        //
                        switch(_TREA_ITEM.FirstOrDefault(x => x.ITEM_ID == y.ITEM_ID)?.TREA_ITEM_TYPE)
                        {
                            case "BILL": // 空白票據
                                var _BLANK_NOTE_APLY_ITEM_ID = db.BLANK_NOTE_APLY.FirstOrDefault(x => x.APLY_NO == y.APLY_NO)?.ITEM_ID;

                                var _ITEM_BLANK_NOTE = db.ITEM_BLANK_NOTE.FirstOrDefault(x => x.ITEM_ID == _BLANK_NOTE_APLY_ITEM_ID);

                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_BLANK_NOTE.INVENTORY_STATUS = "1";
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_BLANK_NOTE.INVENTORY_STATUS = "2";
                                }
                                _ITEM_BLANK_NOTE.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_BLANK_NOTE.modelToString(logStr);
                                break;
                            case "ESTATE": // 不動產
                                var _ITEM_REAL_ESTATE = db.ITEM_REAL_ESTATE.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);
                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_REAL_ESTATE.INVENTORY_STATUS = "1";
                                    _ITEM_REAL_ESTATE.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_REAL_ESTATE.INVENTORY_STATUS = "2";
                                    _ITEM_REAL_ESTATE.GET_DATE = dt;
                                }
                                _ITEM_REAL_ESTATE.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_REAL_ESTATE.modelToString(logStr);
                                break;
                            case "CA": // 電子憑證
                                var _ITEM_CA = db.ITEM_CA.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);

                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_CA.INVENTORY_STATUS = "1";
                                    _ITEM_CA.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_CA.INVENTORY_STATUS = "2";
                                    _ITEM_CA.GET_DATE = dt;
                                }

                                _ITEM_CA.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_CA.modelToString(logStr);
                                break;
                            case "DEPOSIT": //定期存單
                                var _ITEM_DEP_ORDER_M = db.ITEM_DEP_ORDER_M.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);

                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "1";
                                    _ITEM_DEP_ORDER_M.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    if (_ITEM_DEP_ORDER_M.DEP_SET_QUALITY == "N")
                                        _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "2";
                                    else
                                        _ITEM_DEP_ORDER_M.INVENTORY_STATUS = "6";

                                    _ITEM_DEP_ORDER_M.GET_DATE = dt;
                                }
                                _ITEM_DEP_ORDER_M.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_DEP_ORDER_M.modelToString(logStr);
                                break;
                            case "STOCK": //股票
                                var _ITEM_STOCK = db.ITEM_STOCK.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);

                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_STOCK.INVENTORY_STATUS = "1";
                                    _ITEM_STOCK.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_STOCK.INVENTORY_STATUS = "2";
                                    _ITEM_STOCK.GET_DATE = dt;
                                }
                                _ITEM_STOCK.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_STOCK.modelToString(logStr);
                                break;
                            case "MARGING": //存出保證金
                                var _ITEM_REFUNDABLE_DEP = db.ITEM_REFUNDABLE_DEP.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);

                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_REFUNDABLE_DEP.INVENTORY_STATUS = "1";
                                    _ITEM_REFUNDABLE_DEP.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_REFUNDABLE_DEP.INVENTORY_STATUS = "2";
                                    _ITEM_REFUNDABLE_DEP.GET_DATE = dt;
                                }
                                _ITEM_REFUNDABLE_DEP.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_REFUNDABLE_DEP.modelToString(logStr);
                                break;
                            case "MARGINP": //存入保證金
                                var _ITEM_DEP_RECEIVED = db.ITEM_DEP_RECEIVED.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);
                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_DEP_RECEIVED.INVENTORY_STATUS = "1";
                                    _ITEM_DEP_RECEIVED.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_DEP_RECEIVED.INVENTORY_STATUS = "2";
                                    _ITEM_DEP_RECEIVED.GET_DATE = dt;
                                }
                                _ITEM_DEP_RECEIVED.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_DEP_RECEIVED.modelToString(logStr);
                                break;
                            case "ITEMIMP": //重要物品
                                var _ITEM_IMPO = db.ITEM_IMPO.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);
                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_IMPO.INVENTORY_STATUS = "1";
                                    _ITEM_IMPO.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_IMPO.INVENTORY_STATUS = "2";
                                    _ITEM_IMPO.GET_DATE = dt;
                                }
                                _ITEM_IMPO.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_IMPO.modelToString(logStr);
                                break;
                            case "ITEMOTH": //其他物品
                                var _ITEM_OTHER = db.ITEM_OTHER.FirstOrDefault(x => x.ITEM_ID == _OTHER_ITEM_APLY_ITEM_ID);
                                if (y.ACCESS_TYPE == "P")
                                {
                                    _ITEM_OTHER.INVENTORY_STATUS = "1";
                                    _ITEM_OTHER.PUT_DATE = dt;
                                }
                                else if (y.ACCESS_TYPE == "G")
                                {
                                    _ITEM_OTHER.INVENTORY_STATUS = "2";
                                    _ITEM_OTHER.GET_DATE = dt;
                                }
                                _ITEM_OTHER.LAST_UPDATE_DT = dt;

                                logStr += _ITEM_OTHER.modelToString(logStr);
                                break;
                        }
                        #region 申請單歷程檔
                        var ARH = new APLY_REC_HIS()
                        {
                            APLY_NO = y.APLY_NO,
                            APLY_STATUS = status,
                            PROC_UID = cUserId,
                            PROC_DT = dt
                        };
                        logStr += ARH.modelToString(logStr);
                        db.APLY_REC_HIS.Add(ARH);
                        #endregion
                        approvedList.Add(y.APLY_NO);
                    });
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
                        log.CFUNCTION = "覆核-金庫登記簿覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", approvedList)} 覆核成功";
                        result.Datas = GetSearchDatas();
                    }
                    catch(DbUpdateException ex)
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
        /// <param name="RejectReason"></param>
        /// <param name="ViewModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TREAReviewWorkDetailViewModel>> RejectData(string RejectReason, List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId)
        {
            var result = new MSGReturnModel<List<TREAReviewWorkDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription();

            if (!ViewModel.Any())
            {
                return result;
            }

            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            string status = Ref.AccessProjectFormStatus.D04.ToString(); // 金庫登記簿覆核退回
            List<string> rejectList = new List<string>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                foreach(var item in ViewModel.Where(x => x.Ischecked))
                {
                    var _TREA_OPEN_REC = db.TREA_OPEN_REC.FirstOrDefault(x => x.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID);
                    if (_TREA_OPEN_REC == null) //找不到該單號
                    {
                        result.DESCRIPTION = Ref.MessageType.not_Find_Any.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    if (_TREA_OPEN_REC.LAST_UPDATE_DT > item.vLAST_UPDATE_DT) //資料已備更新
                    {
                        result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription(null, $"單號:{item.hvTREA_REGISTER_ID}");
                        return result;
                    }
                    _TREA_OPEN_REC.REGI_STATUS = status;
                    _TREA_OPEN_REC.REGI_APPR_UID = cUserId;
                    _TREA_OPEN_REC.REGI_APPR_DT = dt;
                    _TREA_OPEN_REC.REGI_APPR_DESC = RejectReason.Trim();
                    _TREA_OPEN_REC.LAST_UPDATE_UID = cUserId;
                    _TREA_OPEN_REC.LAST_UPDATE_DT = dt;

                    logStr += _TREA_OPEN_REC.modelToString(logStr);
                    rejectList.Add(item.hvTREA_REGISTER_ID);

                    var _TREA_APLY_REC = db.TREA_APLY_REC.Where(y => y.TREA_REGISTER_ID == item.hvTREA_REGISTER_ID)
                        .Where(y => y.APLY_STATUS == "D02")
                        .ToList();
                    _TREA_APLY_REC.ForEach(y => {
                       y.APLY_STATUS = status;
                       y.LAST_UPDATE_UID = cUserId;
                       y.LAST_UPDATE_DT = dt;
                       logStr += y.modelToString(logStr);
                   });
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
                        log.CFUNCTION = "駁回-金庫登記簿覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, cUserId);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", rejectList)} 已駁回";
                        result.Datas = GetSearchDatas();
                    }
                    catch(DbUpdateException ex)
                    {
                        result.DESCRIPTION = ex.exceptionMessage();
                    }
                }
            }
            return result;
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
    }
}