﻿using System;
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
/// 功能說明：金庫進出管理作業-保管資料發送維護作業
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
    public class ItemChargeUnit : Common, IItemChargeUnit
    {
        /// <summary>
        /// Selected Change 事件
        /// </summary>
        /// <param name="Charge_Dept"></param>
        /// <param name="Charge_Sect"></param>
        /// <param name="Charge_Uid"></param>
        public Tuple<List<SelectOption>, List<SelectOption>> DialogSelectedChange(string Charge_Dept, string Charge_Sect = null)
        {
            List<SelectOption> dCharge_Sect = new List<SelectOption>();
            List<SelectOption> dCharge_Uid = new List<SelectOption>();
            string _Charge_Sect = string.Empty;

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                var _VW_OA_DEPT = dbIntra.VW_OA_DEPT.AsNoTracking();
                var _V_EMPLY2 = dbIntra.V_EMPLY2.AsNoTracking();

                dCharge_Sect = _VW_OA_DEPT.Where(x => x.UP_DPT_CD == Charge_Dept, Charge_Dept != "All")
                    .AsEnumerable()
                    .Select(x => new SelectOption() {
                        Value = x.DPT_CD?.Trim(),
                        Text = x.DPT_NAME?.Trim()
                    }).ToList();

                if (Charge_Sect.IsNullOrWhiteSpace())
                    _Charge_Sect = dCharge_Sect.FirstOrDefault()?.Value;
                else
                    _Charge_Sect = Charge_Sect;

                dCharge_Uid = _V_EMPLY2
                    .Where(x => x.DPT_CD == _Charge_Sect, _Charge_Sect != "All")
                    .AsEnumerable()
                    .Select(x => new SelectOption() {
                        Value = x.USR_ID?.Trim(),
                        Text = x.EMP_NAME?.Trim()
                    }).ToList();
            }
            return new Tuple<List<SelectOption>, List<SelectOption>>(dCharge_Sect, dCharge_Uid);
        }
        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>> FirstDropDown()
        {
            List<SelectOption> CHARGE_DEPT = new List<SelectOption>();
            List<SelectOption> TREA_ITEM = new List<SelectOption>();

            using (DB_INTRAEntities dbIntra = new DB_INTRAEntities())
            {
                var _VW_OA_DEPT = dbIntra.VW_OA_DEPT.AsNoTracking();

                CHARGE_DEPT = _VW_OA_DEPT.Where(x => x.Dpt_type == "03")
                    .AsEnumerable()
                    .Select(x => new SelectOption() {
                        Value = x.DPT_CD?.Trim(),
                        Text = x.DPT_NAME?.Trim()
                    }).ToList();
            }
            using(TreasuryDBEntities db = new TreasuryDBEntities())
            {
                TREA_ITEM = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.DAILY_FLAG == "N")
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.ITEM_ID,
                        Text = x.ITEM_DESC
                    }).ToList();
            }
            return new Tuple<List<SelectOption>, List<SelectOption>>(CHARGE_DEPT, TREA_ITEM);
        }

        /// <summary>
        /// 異動紀錄查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <param name="aply_No">申請單號</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetChangeRecordSearchData(ITinItem searchModel, string aply_No = null)
        {
            var searchData = (ItemChargeUnitChangeRecordSearchViewModel)searchModel;
            List<ItemChargeUnitChangeRecordSearchDetailViewModel> result = new List<ItemChargeUnitChangeRecordSearchDetailViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Appr_Status = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();

                if (aply_No.IsNullOrWhiteSpace())
                {
                    result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                        .Where(x => x.ITEM_ID == searchData.vTREA_ITEM_NAME, searchData.vTREA_ITEM_NAME != "All")
                        .Where(x => x.CHARGE_DEPT == searchData.vCHARGE_DEPT, searchData.vCHARGE_DEPT != "All")
                        .Where(x => x.CHARGE_SECT == searchData.vCHARGE_SECT, searchData.vCHARGE_SECT != "All")
                        .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                        .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                        .AsEnumerable()
                        .Join(db.ITEM_CHARGE_UNIT.AsNoTracking()
                        .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null)
                        .AsEnumerable(),
                        ICUH => ICUH.ITEM_ID,
                        ICU => ICU.ITEM_ID,
                        (ICUH, ICU) => new ItemChargeUnitChangeRecordSearchDetailViewModel
                        {
                            vFreeze_Dt = ICU.FREEZE_DT?.ToString("yyyy/MM/dd"),
                            vAply_No = ICUH.APLY_NO,
                            vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == ICU.FREEZE_UID)?.EMP_NAME?.Trim(),
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == ICUH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vCHARGE_UID = emps.FirstOrDefault(x => x.USR_ID == ICUH.CHARGE_UID)?.EMP_NAME?.Trim(),
                            vCHARGE_UID_B = ICUH.CHARGE_UID_B,
                            vIS_MAIL_DEPT_MGR = ICUH.IS_MAIL_DEPT_MGR,
                            vIS_MAIL_DEPT_MGR_B = ICUH.IS_MAIL_DEPT_MGR_B,
                            vIS_MAIL_SECT_MGR = ICUH.IS_MAIL_SECT_MGR,
                            vIS_MAIL_SECT_MGR_B = ICUH.IS_MAIL_SECT_MGR_B,
                            vIS_DISABLED = ICUH.IS_DISABLED,
                            vIS_DISABLED_B = ICUH.IS_DISABLED_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == ICUH.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = ICUH.APPR_DESC
                        }
                        ).ToList();
                }
                else
                {
                    result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .AsEnumerable()
                        .Join(db.ITEM_CHARGE_UNIT.AsNoTracking()
                        .AsEnumerable(),
                        ICUH => ICUH.ITEM_ID,
                        ICU => ICU.ITEM_ID,
                        (ICUH, ICU) => new ItemChargeUnitChangeRecordSearchDetailViewModel
                        {
                            vFreeze_Dt = ICU.FREEZE_DT?.ToString("yyyy/MM/dd"),
                            vAply_No = ICUH.APLY_NO,
                            vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == ICU.FREEZE_UID)?.EMP_NAME?.Trim(),
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == ICUH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vCHARGE_UID = emps.FirstOrDefault(x => x.USR_ID == ICUH.CHARGE_UID)?.EMP_NAME?.Trim(),
                            vCHARGE_UID_B = ICUH.CHARGE_UID_B,
                            vIS_MAIL_DEPT_MGR = ICUH.IS_MAIL_DEPT_MGR,
                            vIS_MAIL_DEPT_MGR_B = ICUH.IS_MAIL_DEPT_MGR_B,
                            vIS_MAIL_SECT_MGR = ICUH.IS_MAIL_SECT_MGR,
                            vIS_MAIL_SECT_MGR_B = ICUH.IS_MAIL_SECT_MGR_B,
                            vIS_DISABLED = ICUH.IS_DISABLED,
                            vIS_DISABLED_B = ICUH.IS_DISABLED_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == ICUH.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = ICUH.APPR_DESC,
                            vCHARGE_UNIT_ID = ICUH.CHARGE_UNIT_ID
                        }
                        ).ToList();
                }
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
            var searchData = (ItemChargeUnitSearchViewModel)searchModel;
            List<ItemChargeUnitSearchDetailViewModel> result = new List<ItemChargeUnitSearchDetailViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var depts = GetDepts();
                var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.AsNoTracking();
                var _ITEM_CHARGE_UNIT_HIS = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking();
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking();

                result = _ITEM_CHARGE_UNIT
                    .Where(x => x.CHARGE_DEPT == searchData.vCHARGE_DEPT, searchData.vCHARGE_DEPT != "All")
                    .Where(x => x.CHARGE_SECT == searchData.vCHARGE_SECT, searchData.vCHARGE_SECT != "All")
                    .Where(x => x.CHARGE_UID == searchData.vCHARGE_UID, searchData.vCHARGE_UID != "All")
                    .AsEnumerable()
                    .Join(_TREA_ITEM
                    .Where(x => x.TREA_ITEM_NAME == searchData.vTREA_ITEM_NAME, searchData.vTREA_ITEM_NAME != "All")
                    //.Where(x => x.DAILY_FLAG == "N") //每日進出 = N
                    .AsEnumerable(),
                    ICU => ICU.ITEM_ID,
                    TI => TI.ITEM_ID,
                    (ICU, TI) => new ItemChargeUnitSearchDetailViewModel {
                        vEXEC_ACTION = _ITEM_CHARGE_UNIT_HIS.FirstOrDefault(y => y.CHARGE_UNIT_ID == ICU.CHARGE_UNIT_ID)?.EXEC_ACTION?.Trim(),
                        vEXEC_ACTION_VALUE = _SYS_CODE.Where(y => y.CODE_TYPE == "EXEC_ACTION").ToList().FirstOrDefault(y => y.CODE == _ITEM_CHARGE_UNIT_HIS.FirstOrDefault(z => z.CHARGE_UNIT_ID == ICU.CHARGE_UNIT_ID)?.EXEC_ACTION?.Trim())?.CODE_VALUE?.Trim(),
                        vTREA_ITEM_NAME = TI.ITEM_ID,
                        vTREA_ITEM_NAME_VALUE = TI.ITEM_DESC,
                        vCHARGE_DEPT = ICU.CHARGE_DEPT,
                        vCHARGE_DEPT_VALUE = !ICU.CHARGE_DEPT.IsNullOrWhiteSpace()? depts.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == ICU.CHARGE_DEPT)?.DPT_NAME?.Trim() : null,
                        vCHARGE_SECT = ICU.CHARGE_SECT,
                        vCHARGE_SECT_VALUE = !ICU.CHARGE_SECT.IsNullOrWhiteSpace()? emps.FirstOrDefault(y => y.DPT_CD!= null && y.DPT_CD.Trim() == ICU.CHARGE_SECT)?.DPT_NAME?.Trim() : null,
                        vIS_MAIL_DEPT_MGR = ICU.IS_MAIL_DEPT_MGR,
                        vIS_MAIL_SECT_MGR = ICU.IS_MAIL_SECT_MGR,
                        vCHARGE_UID = ICU.CHARGE_UID,
                        vCHARGE_NAME = !ICU.CHARGE_UID.IsNullOrWhiteSpace()? emps.FirstOrDefault(y => y.USR_ID == ICU.CHARGE_UID)?.EMP_NAME?.Trim() : null,
                        vDATA_STATUS = ICU.DATA_STATUS,
                        vDATA_STATUS_VALUE = !ICU.DATA_STATUS.IsNullOrWhiteSpace()? _SYS_CODE.FirstOrDefault(y => y.CODE_TYPE == "DATA_STATUS" && y.CODE == ICU.DATA_STATUS)?.CODE_VALUE?.Trim() : null,
                        vFREEZE_UID = ICU.FREEZE_UID,
                        vFREEZE_NAME = !ICU.FREEZE_UID.IsNullOrWhiteSpace()? emps.FirstOrDefault(y => y.USR_ID == ICU.FREEZE_UID)?.EMP_NAME?.Trim() : null,
                        vIS_DISABLED = ICU.IS_DISABLED,
                        vCHARGE_UNIT_ID = ICU.CHARGE_UNIT_ID,
                        vLAST_UPDATE_DT = ICU.LAST_UPDATE_DT,
                        vAPLY_NO = _ITEM_CHARGE_UNIT_HIS.FirstOrDefault(y => y.CHARGE_UNIT_ID == ICU.CHARGE_UNIT_ID)?.APLY_NO?.Trim()
                    }).ToList();
            }
                return result;
        }

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <param name="searchModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        public MSGReturnModel<string> ResetData(string AplyNo, ItemChargeUnitSearchViewModel searchModel, string cUserId)
        {
            MSGReturnModel<string> result = new MSGReturnModel<string>();
            DateTime dt = DateTime.Now;
            try
            {
                if (AplyNo != null)
                {
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
                        var _ITEM_CHARGE_UNIT_HIS = db.ITEM_CHARGE_UNIT_HIS.FirstOrDefault(x => x.APLY_NO == AplyNo);

                        if(_ITEM_CHARGE_UNIT_HIS.APLY_UID == cUserId)
                        {
                            var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == _ITEM_CHARGE_UNIT_HIS.CHARGE_UNIT_ID);
                            switch (_ITEM_CHARGE_UNIT_HIS.EXEC_ACTION)
                            {
                                case "A":
                                    _ITEM_CHARGE_UNIT_HIS.APPR_STATUS = "4";

                                    db.ITEM_CHARGE_UNIT.Remove(_ITEM_CHARGE_UNIT);
                                    break;
                                case "U":
                                    _ITEM_CHARGE_UNIT_HIS.APPR_STATUS = "4";

                                    _ITEM_CHARGE_UNIT.DATA_STATUS = "1";
                                    _ITEM_CHARGE_UNIT.LAST_UPDATE_DT = dt;
                                    _ITEM_CHARGE_UNIT.LAST_UPDATE_UID = cUserId;
                                    _ITEM_CHARGE_UNIT.FREEZE_DT = null;
                                    _ITEM_CHARGE_UNIT.FREEZE_UID = null;
                                break;
                            }
                        }
                        else
                        {
                            result.DESCRIPTION = "非申請者無法取消申請";
                        }      
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
        /// 保管資料發送維護作業-申請覆核
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        public MSGReturnModel<IEnumerable<ITinItem>> TinApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel)
        {
            var searchData = (ItemChargeUnitSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {
                if (saveData != null)
                {
                    var datas = (List<ItemChargeUnitSearchDetailViewModel>)saveData;
                    if (datas.Any())
                    {
                        using (TreasuryDBEntities db = new TreasuryDBEntities())
                        {
                            //取得流水號
                            SysSeqDao sysSeqDao = new SysSeqDao();
                            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
                            string _Aply_No = string.Empty;
                            var cId = sysSeqDao.qrySeqNo("G8", qPreCode).ToString().PadLeft(3, '0');
                            _Aply_No = $@"G8{qPreCode}{cId}";//G8 + 系統日期YYYMMDD(民國年) + 3碼流水號
                            string logStr = string.Empty; //log

                            foreach (var item in datas)
                            {
                                var _CHARGE_UNIT_ID = string.Empty;
                                var _ICU = new ITEM_CHARGE_UNIT();
                                # region 保管單位設定檔
                                //判斷執行功能
                                switch (item.vEXEC_ACTION)
                                {
                                    case "A"://新增
                                        _CHARGE_UNIT_ID = sysSeqDao.qrySeqNo("D5", string.Empty).ToString().PadLeft(3, '0');
                                        _CHARGE_UNIT_ID = $@"D5{_CHARGE_UNIT_ID}";
                                        _ICU = new ITEM_CHARGE_UNIT()
                                        {
                                            CHARGE_UNIT_ID = _CHARGE_UNIT_ID,
                                            ITEM_ID = item.vTREA_ITEM_NAME,
                                            CHARGE_DEPT = item.vCHARGE_DEPT,
                                            CHARGE_SECT = item.vCHARGE_SECT,
                                            CHARGE_UID = item.vCHARGE_UID,
                                            IS_MAIL_DEPT_MGR = item.vIS_MAIL_DEPT_MGR,
                                            IS_MAIL_SECT_MGR = item.vIS_MAIL_SECT_MGR,
                                            IS_DISABLED = "N",
                                            DATA_STATUS = "2",//凍結中
                                            CREATE_DT = dt,
                                            CREATE_UID = searchData.vCUSER_ID,
                                            LAST_UPDATE_DT = dt,
                                            LAST_UPDATE_UID = searchData.vCUSER_ID,
                                            FREEZE_UID = searchData.vCUSER_ID,
                                            FREEZE_DT = dt
                                        };
                                        db.ITEM_CHARGE_UNIT.Add(_ICU);
                                        logStr += "|";
                                        logStr += _ICU.modelToString();
                                        break;
                                    case "U"://修改
                                        _ICU = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == item.vCHARGE_UNIT_ID);
                                        if (_ICU.LAST_UPDATE_DT != null && _ICU.LAST_UPDATE_DT > item.vLAST_UPDATE_DT)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _CHARGE_UNIT_ID = item.vCHARGE_UNIT_ID;
                                        _ICU.DATA_STATUS = "2"; //凍結中
                                        _ICU.LAST_UPDATE_UID = searchData.vCUSER_ID;
                                        _ICU.LAST_UPDATE_DT = dt;
                                        _ICU.FREEZE_UID = searchData.vCUSER_ID;
                                        _ICU.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _ICU.modelToString();
                                        break;
                                    case "D"://刪除
                                        _ICU = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == item.vCHARGE_UNIT_ID);
                                        if (_ICU.LAST_UPDATE_DT != null && _ICU.LAST_UPDATE_DT > item.vLAST_UPDATE_DT)
                                        {
                                            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
                                            return result;
                                        }
                                        _CHARGE_UNIT_ID = item.vCHARGE_UNIT_ID;
                                        _ICU.DATA_STATUS = "2";//凍結中
                                        _ICU.LAST_UPDATE_UID = searchData.vCUSER_ID;
                                        _ICU.LAST_UPDATE_DT = dt;
                                        _ICU.FREEZE_UID = searchData.vCUSER_ID;
                                        _ICU.FREEZE_DT = dt;
                                        logStr += "|";
                                        logStr += _ICU.modelToString();
                                        break;
                                    default:
                                        break;
                                }
                                #endregion
                                #region 保管單位設定異動檔
                                var _ICU_Data = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == item.vCHARGE_UNIT_ID);
                                if(_ICU_Data == null)
                                {
                                    var _ICUH = new ITEM_CHARGE_UNIT_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        CHARGE_UNIT_ID = _CHARGE_UNIT_ID,
                                        EXEC_ACTION = item.vEXEC_ACTION,
                                        ITEM_ID = item.vTREA_ITEM_NAME,
                                        CHARGE_DEPT = item.vCHARGE_DEPT,
                                        CHARGE_SECT = item.vCHARGE_SECT,
                                        CHARGE_UID = item.vCHARGE_UID,
                                        IS_MAIL_DEPT_MGR = item.vIS_MAIL_DEPT_MGR,
                                        IS_MAIL_SECT_MGR = item.vIS_MAIL_SECT_MGR,
                                        IS_DISABLED = "N",
                                        //CHARGE_UID_B = _ICU_Data.CHARGE_UID,
                                        //IS_MAIL_DEPT_MGR_B = _ICU_Data.IS_MAIL_DEPT_MGR,
                                        //IS_MAIL_SECT_MGR_B = _ICU_Data.IS_MAIL_SECT_MGR,
                                        //IS_DISABLED_B = "N",
                                        APLY_DATE = dt,
                                        APLY_UID = searchData.vCUSER_ID,
                                        APPR_STATUS = "1" //表單申請
                                    };
                                    db.ITEM_CHARGE_UNIT_HIS.Add(_ICUH);
                                    logStr += "|";
                                    logStr += _ICUH.modelToString();
                                }
                                else
                                {
                                    var _ICUH = new ITEM_CHARGE_UNIT_HIS()
                                    {
                                        APLY_NO = _Aply_No,
                                        CHARGE_UNIT_ID = _CHARGE_UNIT_ID,
                                        EXEC_ACTION = item.vEXEC_ACTION,
                                        ITEM_ID = item.vTREA_ITEM_NAME,
                                        CHARGE_DEPT = item.vCHARGE_DEPT,
                                        CHARGE_SECT = item.vCHARGE_SECT,
                                        CHARGE_UID = item.vCHARGE_UID,
                                        IS_MAIL_DEPT_MGR = item.vIS_MAIL_DEPT_MGR,
                                        IS_MAIL_SECT_MGR = item.vIS_MAIL_SECT_MGR,
                                        IS_DISABLED = "N",
                                        CHARGE_UID_B = _ICU_Data.CHARGE_UID,
                                        IS_MAIL_DEPT_MGR_B = _ICU_Data.IS_MAIL_DEPT_MGR,
                                        IS_MAIL_SECT_MGR_B = _ICU_Data.IS_MAIL_SECT_MGR,
                                        IS_DISABLED_B = "N",
                                        APLY_DATE = dt,
                                        APLY_UID = searchData.vCUSER_ID,
                                        APPR_STATUS = "1" //表單申請
                                    };
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
                                    log.CFUNCTION = "申請覆核-保管資料發送維護";
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
            catch(Exception ex)
            {
                result.DESCRIPTION = ex.exceptionMessage();
            }
            return result;
        }

        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId)
        {
            throw new NotImplementedException();
        }

        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> aplyNos, string logStr, DateTime dt, string userId, string desc)
        {
            throw new NotImplementedException();
        }
    }
}