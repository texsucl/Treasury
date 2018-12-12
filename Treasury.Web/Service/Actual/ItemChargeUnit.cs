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
                {
                    _Charge_Sect = dCharge_Sect.FirstOrDefault()?.Value;

                    dCharge_Uid = _V_EMPLY2
                    .Where(x => x.DPT_CD == _Charge_Sect, !_Charge_Sect.IsNullOrWhiteSpace())
                    .Where(x => x.DPT_CD == Charge_Dept, _Charge_Sect.IsNullOrWhiteSpace())
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.USR_ID?.Trim(),
                        Text = x.EMP_NAME?.Trim()
                    }).ToList();
                }  
                else
                {
                    _Charge_Sect = Charge_Sect;

                    dCharge_Uid = _V_EMPLY2
                    .Where(x => x.DPT_CD == _Charge_Sect, _Charge_Sect != "All")
                    .AsEnumerable()
                    .Select(x => new SelectOption()
                    {
                        Value = x.USR_ID?.Trim(),
                        Text = x.EMP_NAME?.Trim()
                    }).ToList();
                }
                    

                //dCharge_Uid = _V_EMPLY2
                //    .Where(x => x.DPT_CD == _Charge_Sect, _Charge_Sect != "All")
                //    .AsEnumerable()
                //    .Select(x => new SelectOption() {
                //        Value = x.USR_ID?.Trim(),
                //        Text = x.EMP_NAME?.Trim()
                //    }).ToList();
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
                    //.Where(x => x.DAILY_FLAG == "N")
                    .Where(x => x.IS_DISABLED == "N" && x.ITEM_OP_TYPE == "3") //「入庫作業類型=3」且啟用中的存取項目
                    .OrderBy(x => x.ITEM_ID)
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
        /// 使用 部門ID 獲得 部門名稱
        /// </summary>
        /// <param name="depts"></param>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        private string getEmpName(List<VW_OA_DEPT> depts, string DPT_CD)
        {
            if (!DPT_CD.IsNullOrWhiteSpace() && depts.Any())
                return depts.FirstOrDefault(x => x.DPT_CD.Trim() == DPT_CD.Trim())?.DPT_NAME?.Trim();
            return string.Empty;
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
                var depts = GetDepts();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Appr_Status = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "APPR_STATUS").ToList();
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking()
                    .Where(x => x.ITEM_OP_TYPE == "3");

                if (aply_No.IsNullOrWhiteSpace())
                {
                    var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.AsNoTracking()
                        .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null).ToList();
                    result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                        .Where(x => x.ITEM_ID == searchData.vTREA_ITEM_NAME, searchData.vTREA_ITEM_NAME != "All")
                        .Where(x => x.CHARGE_DEPT == searchData.vCHARGE_DEPT, searchData.vCHARGE_DEPT != "All")
                        .Where(x => x.CHARGE_SECT == searchData.vCHARGE_SECT, searchData.vCHARGE_SECT != "All")
                        .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                        .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                        .AsEnumerable()
                        .Select(x => new ItemChargeUnitChangeRecordSearchDetailViewModel {
                            //vFreeze_Dt = x.EXEC_ACTION != "A" ? _ITEM_CHARGE_UNIT.FirstOrDefault(y => y.CHARGE_UNIT_ID == x.CHARGE_UNIT_ID)?.FREEZE_DT?.ToString("yyyy/MM/dd") : null,
                            vFreeze_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            //vFreeze_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == _ITEM_CHARGE_UNIT.FirstOrDefault(z => z.CHARGE_UNIT_ID == x.CHARGE_UNIT_ID)?.FREEZE_UID)?.EMP_NAME?.Trim(),
                            vFreeze_Uid_Name = !x.APLY_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim() : null,
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vCHARGE_UID = emps.FirstOrDefault(y => y.USR_ID == x.CHARGE_UID)?.EMP_NAME?.Trim(),
                            vCHARGE_UID_B = !x.CHARGE_UID_B.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.CHARGE_UID_B)?.EMP_NAME?.Trim() : null,
                            vIS_MAIL_DEPT_MGR = x.IS_MAIL_DEPT_MGR,
                            vIS_MAIL_DEPT_MGR_B = x.IS_MAIL_DEPT_MGR_B,
                            vIS_MAIL_SECT_MGR = x.IS_MAIL_SECT_MGR,
                            vIS_MAIL_SECT_MGR_B = x.IS_MAIL_SECT_MGR_B,
                            vIS_DISABLED = x.IS_DISABLED,
                            vIS_DISABLED_B = x.IS_DISABLED_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = x.APPR_DESC,
                            // vCHARGE_DEPT_VALUE = !x.CHARGE_DEPT.IsNullOrWhiteSpace() ? depts.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == x.CHARGE_DEPT)?.DPT_NAME?.Trim() : null,
                            vCHARGE_DEPT_VALUE = !x.CHARGE_DEPT.IsNullOrWhiteSpace() ? getEmpName(depts, x.CHARGE_DEPT) : null,
                            vCHARGE_SECT_VALUE = !x.CHARGE_SECT.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim().Replace(getEmpName(depts, x.CHARGE_DEPT), "")?.Trim() : null,
                            vTREA_ITEM_NAME_VALUE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        }).ToList();

                    //result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                    //    .Where(x => x.ITEM_ID == searchData.vTREA_ITEM_NAME, searchData.vTREA_ITEM_NAME != "All")
                    //    .Where(x => x.CHARGE_DEPT == searchData.vCHARGE_DEPT, searchData.vCHARGE_DEPT != "All")
                    //    .Where(x => x.CHARGE_SECT == searchData.vCHARGE_SECT, searchData.vCHARGE_SECT != "All")
                    //    .Where(x => x.APLY_NO == searchData.vAply_No, searchData.vAply_No != null)
                    //    .Where(x => x.APPR_STATUS == searchData.vAppr_Status, searchData.vAppr_Status != "All")
                    //    .AsEnumerable()
                    //    .Join(db.ITEM_CHARGE_UNIT.AsNoTracking()
                    //    .Where(x => x.FREEZE_UID == searchData.vLast_Update_Uid, searchData.vLast_Update_Uid != null)
                    //    .AsEnumerable(),
                    //    ICUH => ICUH.ITEM_ID,
                    //    ICU => ICU.ITEM_ID,
                    //    (ICUH, ICU) => new ItemChargeUnitChangeRecordSearchDetailViewModel
                    //    {
                    //        vFreeze_Dt = ICU.FREEZE_DT?.ToString("yyyy/MM/dd"),
                    //        vAply_No = ICUH.APLY_NO,
                    //        vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == ICU.FREEZE_UID)?.EMP_NAME?.Trim(),
                    //        vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == ICUH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                    //        vCHARGE_UID = emps.FirstOrDefault(x => x.USR_ID == ICUH.CHARGE_UID)?.EMP_NAME?.Trim(),
                    //        vCHARGE_UID_B = ICUH.CHARGE_UID_B,
                    //        vIS_MAIL_DEPT_MGR = ICUH.IS_MAIL_DEPT_MGR,
                    //        vIS_MAIL_DEPT_MGR_B = ICUH.IS_MAIL_DEPT_MGR_B,
                    //        vIS_MAIL_SECT_MGR = ICUH.IS_MAIL_SECT_MGR,
                    //        vIS_MAIL_SECT_MGR_B = ICUH.IS_MAIL_SECT_MGR_B,
                    //        vIS_DISABLED = ICUH.IS_DISABLED,
                    //        vIS_DISABLED_B = ICUH.IS_DISABLED_B,
                    //        vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == ICUH.APPR_STATUS)?.CODE_VALUE.Trim(),
                    //        vAPPR_DESC = ICUH.APPR_DESC
                    //    }
                    //    ).ToList();
                }
                else
                {
                    var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.AsNoTracking().ToList();
                    result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                        .Where(x => x.APLY_NO == aply_No)
                        .Where(x => x.CHARGE_UNIT_ID == searchData.vCHARGE_UNIT_ID, searchData.vCHARGE_UNIT_ID != null)
                        .AsEnumerable()
                        .Select(x => new ItemChargeUnitChangeRecordSearchDetailViewModel {
                            //vFreeze_Dt = x.EXEC_ACTION != "A" ? _ITEM_CHARGE_UNIT.FirstOrDefault(y => y.CHARGE_UNIT_ID == x.CHARGE_UNIT_ID)?.FREEZE_DT?.ToString("yyyy/MM/dd") : null,
                            vFreeze_Dt = x.APLY_DATE != null ? x.APLY_DATE.ToString("yyyy/MM/dd") : null,
                            vAply_No = x.APLY_NO,
                            //vFreeze_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == _ITEM_CHARGE_UNIT.FirstOrDefault(z => z.CHARGE_UNIT_ID == x.CHARGE_UNIT_ID)?.FREEZE_UID)?.EMP_NAME?.Trim(),
                            vFreeze_Uid_Name = !x.APLY_UID.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.APLY_UID)?.EMP_NAME?.Trim() : null,
                            vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == x.EXEC_ACTION)?.CODE_VALUE.Trim(),
                            vCHARGE_UID = emps.FirstOrDefault(y => y.USR_ID == x.CHARGE_UID)?.EMP_NAME?.Trim(),
                            vCHARGE_UID_B = !x.CHARGE_UID_B.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.USR_ID == x.CHARGE_UID_B)?.EMP_NAME?.Trim() : null,
                            vIS_MAIL_DEPT_MGR = x.IS_MAIL_DEPT_MGR,
                            vIS_MAIL_DEPT_MGR_B = x.IS_MAIL_DEPT_MGR_B,
                            vIS_MAIL_SECT_MGR = x.IS_MAIL_SECT_MGR,
                            vIS_MAIL_SECT_MGR_B = x.IS_MAIL_SECT_MGR_B,
                            vIS_DISABLED = x.IS_DISABLED,
                            vIS_DISABLED_B = x.IS_DISABLED_B,
                            vAPPR_STATUS = _Appr_Status.FirstOrDefault(y => y.CODE == x.APPR_STATUS)?.CODE_VALUE.Trim(),
                            vAPPR_DESC = x.APPR_DESC,
                            vCHARGE_UNIT_ID = x.CHARGE_UNIT_ID,
                            vCHARGE_DEPT_VALUE = !x.CHARGE_DEPT.IsNullOrWhiteSpace() ? getEmpName(depts, x.CHARGE_DEPT) : null,
                            vCHARGE_SECT_VALUE = !x.CHARGE_SECT.IsNullOrWhiteSpace() ? emps.FirstOrDefault(y => y.DPT_CD != null && y.DPT_CD.Trim() == x.CHARGE_SECT)?.DPT_NAME?.Trim().Replace(getEmpName(depts, x.CHARGE_DEPT), "")?.Trim() : null,
                            vTREA_ITEM_NAME_VALUE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC
                        }).ToList();

                    //result = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking()
                    //    .Where(x => x.APLY_NO == aply_No)
                    //    .Where(x => x.CHARGE_UNIT_ID == searchData.vCHARGE_UNIT_ID, searchData.vCHARGE_UNIT_ID != null)
                    //    .AsEnumerable()
                    //    .Join(db.ITEM_CHARGE_UNIT.AsNoTracking()
                    //    .AsEnumerable(),
                    //    ICUH => ICUH.ITEM_ID,
                    //    ICU => ICU.ITEM_ID,
                    //    (ICUH, ICU) => new ItemChargeUnitChangeRecordSearchDetailViewModel
                    //    {
                    //        vFreeze_Dt = ICU.FREEZE_DT?.ToString("yyyy/MM/dd"),
                    //        vAply_No = ICUH.APLY_NO,
                    //        vFreeze_Uid_Name = emps.FirstOrDefault(x => x.USR_ID == ICU.FREEZE_UID)?.EMP_NAME?.Trim(),
                    //        vExec_Action_Name = _Exec_Action.FirstOrDefault(x => x.CODE == ICUH.EXEC_ACTION)?.CODE_VALUE.Trim(),
                    //        vCHARGE_UID = emps.FirstOrDefault(x => x.USR_ID == ICUH.CHARGE_UID)?.EMP_NAME?.Trim(),
                    //        vCHARGE_UID_B = ICUH.CHARGE_UID_B,
                    //        vIS_MAIL_DEPT_MGR = ICUH.IS_MAIL_DEPT_MGR,
                    //        vIS_MAIL_DEPT_MGR_B = ICUH.IS_MAIL_DEPT_MGR_B,
                    //        vIS_MAIL_SECT_MGR = ICUH.IS_MAIL_SECT_MGR,
                    //        vIS_MAIL_SECT_MGR_B = ICUH.IS_MAIL_SECT_MGR_B,
                    //        vIS_DISABLED = ICUH.IS_DISABLED,
                    //        vIS_DISABLED_B = ICUH.IS_DISABLED_B,
                    //        vAPPR_STATUS = _Appr_Status.FirstOrDefault(x => x.CODE == ICUH.APPR_STATUS)?.CODE_VALUE.Trim(),
                    //        vAPPR_DESC = ICUH.APPR_DESC,
                    //        vCHARGE_UNIT_ID = ICUH.CHARGE_UNIT_ID
                    //    }
                    //    ).ToList();
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
                var _ITEM_CHARGE_UNIT_HIS = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking().Where(x => x.APPR_DATE == null).ToList();
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _SYS_CODE = db.SYS_CODE.AsNoTracking();

                result = _ITEM_CHARGE_UNIT
                    .Where(x => x.CHARGE_DEPT == searchData.vCHARGE_DEPT, searchData.vCHARGE_DEPT != "All")
                    .Where(x => x.CHARGE_SECT == searchData.vCHARGE_SECT, searchData.vCHARGE_SECT != "All")
                    .Where(x => x.CHARGE_UID == searchData.vCHARGE_UID, searchData.vCHARGE_UID != "All")
                    .AsEnumerable()
                    .Join(_TREA_ITEM
                    .Where(x => x.ITEM_ID == searchData.vTREA_ITEM_NAME, searchData.vTREA_ITEM_NAME != "All")
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
                        vAPLY_NO = ICU.DATA_STATUS != "1" ? _ITEM_CHARGE_UNIT_HIS.FirstOrDefault(y => y.CHARGE_UNIT_ID == ICU.CHARGE_UNIT_ID)?.APLY_NO?.Trim() : "",
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
            //return result;
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
                                        _CHARGE_UNIT_ID = "";
                                        //_CHARGE_UNIT_ID = sysSeqDao.qrySeqNo("D5", string.Empty).ToString().PadLeft(3, '0');
                                        //_CHARGE_UNIT_ID = $@"D5{_CHARGE_UNIT_ID}";
                                        //_ICU = new ITEM_CHARGE_UNIT()
                                        //{
                                        //    CHARGE_UNIT_ID = _CHARGE_UNIT_ID,
                                        //    ITEM_ID = item.vTREA_ITEM_NAME,
                                        //    CHARGE_DEPT = item.vCHARGE_DEPT,
                                        //    CHARGE_SECT = item.vCHARGE_SECT,
                                        //    CHARGE_UID = item.vCHARGE_UID,
                                        //    IS_MAIL_DEPT_MGR = item.vIS_MAIL_DEPT_MGR,
                                        //    IS_MAIL_SECT_MGR = item.vIS_MAIL_SECT_MGR,
                                        //    IS_DISABLED = "N",
                                        //    DATA_STATUS = "2",//凍結中
                                        //    CREATE_DT = dt,
                                        //    CREATE_UID = searchData.vCUSER_ID,
                                        //    LAST_UPDATE_DT = dt,
                                        //    LAST_UPDATE_UID = searchData.vCUSER_ID,
                                        //    FREEZE_UID = searchData.vCUSER_ID,
                                        //    FREEZE_DT = dt
                                        //};
                                        //db.ITEM_CHARGE_UNIT.Add(_ICU);
                                        //logStr += "|";
                                        //logStr += _ICU.modelToString();
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
                                        IS_DISABLED = item.vIS_DISABLED,
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
                                        IS_DISABLED = item.vIS_DISABLED,
                                        CHARGE_UID_B = _ICU_Data.CHARGE_UID,
                                        IS_MAIL_DEPT_MGR_B = _ICU_Data.IS_MAIL_DEPT_MGR,
                                        IS_MAIL_SECT_MGR_B = _ICU_Data.IS_MAIL_SECT_MGR,
                                        IS_DISABLED_B = _ICU_Data.IS_DISABLED,
                                        APLY_DATE = dt,
                                        APLY_UID = searchData.vCUSER_ID,
                                        APPR_STATUS = "1" //表單申請
                                    };
                                    db.ITEM_CHARGE_UNIT_HIS.Add(_ICUH);
                                    logStr += "|";
                                    logStr += _ICUH.modelToString();
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
            SysSeqDao sysSeqDao = new SysSeqDao();
            foreach (var aplyNo in aplyNos)
            {
                var _ITEM_CHARGE_UNIT_HISList = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if (_ITEM_CHARGE_UNIT_HISList.Any())
                {
                    foreach (var ItemChargeUnitHis in _ITEM_CHARGE_UNIT_HISList)
                    {
                        var _CHARGE_UNIT_ID = string.Empty;
                        //保管單位設定檔
                        var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == ItemChargeUnitHis.CHARGE_UNIT_ID);

                        if (_ITEM_CHARGE_UNIT != null)
                        {
                            _ITEM_CHARGE_UNIT.DATA_STATUS = "1";//可異動
                            _ITEM_CHARGE_UNIT.CHARGE_UID = ItemChargeUnitHis.CHARGE_UID;
                            _ITEM_CHARGE_UNIT.IS_MAIL_DEPT_MGR = ItemChargeUnitHis.IS_MAIL_DEPT_MGR;
                            _ITEM_CHARGE_UNIT.IS_MAIL_SECT_MGR = ItemChargeUnitHis.IS_MAIL_SECT_MGR;
                            _ITEM_CHARGE_UNIT.IS_DISABLED = ItemChargeUnitHis.IS_DISABLED;

                            _ITEM_CHARGE_UNIT.FREEZE_DT = null;
                            _ITEM_CHARGE_UNIT.FREEZE_UID = null;
                            //判斷是否刪除
                            if (ItemChargeUnitHis.EXEC_ACTION == "D")
                            {
                                _ITEM_CHARGE_UNIT.IS_DISABLED = "Y";
                            }
                            _ITEM_CHARGE_UNIT.APPR_UID = userId;
                            _ITEM_CHARGE_UNIT.APPR_DT = dt;
                            _ITEM_CHARGE_UNIT.LAST_UPDATE_UID = userId;
                            _ITEM_CHARGE_UNIT.LAST_UPDATE_DT = dt;
                            logStr += _ITEM_CHARGE_UNIT.modelToString(logStr);
                        }
                        else
                        {
                            _CHARGE_UNIT_ID = $@"D5{sysSeqDao.qrySeqNo("D5", string.Empty).ToString().PadLeft(3, '0')}";
                            //新增至ITEM_CHARGE_UNIT
                            var ICU = new ITEM_CHARGE_UNIT()
                            {
                                CHARGE_UNIT_ID = _CHARGE_UNIT_ID,
                                ITEM_ID = ItemChargeUnitHis.ITEM_ID,
                                CHARGE_DEPT = ItemChargeUnitHis.CHARGE_DEPT,
                                CHARGE_SECT = ItemChargeUnitHis.CHARGE_SECT,
                                CHARGE_UID = ItemChargeUnitHis.CHARGE_UID,
                                IS_MAIL_DEPT_MGR = ItemChargeUnitHis.IS_MAIL_DEPT_MGR,
                                IS_MAIL_SECT_MGR = ItemChargeUnitHis.IS_MAIL_SECT_MGR,
                                IS_DISABLED = ItemChargeUnitHis.IS_DISABLED,
                                DATA_STATUS = "1", //可異動
                                CREATE_UID = ItemChargeUnitHis.APLY_UID,
                                CREATE_DT = dt,
                                LAST_UPDATE_UID = userId,
                                LAST_UPDATE_DT = dt,
                                APPR_UID = userId,
                                APPR_DT = dt
                            };
                            logStr += ICU.modelToString(logStr);
                            db.ITEM_CHARGE_UNIT.Add(ICU);
                        }

                        //保管單位設定異動檔
                        var _ITEM_CHARGE_UNIT_His = db.ITEM_CHARGE_UNIT_HIS.FirstOrDefault(x => x.APLY_NO == ItemChargeUnitHis.APLY_NO && x.HIS_ID == ItemChargeUnitHis.HIS_ID);
                        if (_ITEM_CHARGE_UNIT_His != null)
                        {
                            if (_ITEM_CHARGE_UNIT_His.CHARGE_UNIT_ID.IsNullOrWhiteSpace())
                            {
                                _ITEM_CHARGE_UNIT_His.CHARGE_UNIT_ID = _CHARGE_UNIT_ID;
                            }
                            _ITEM_CHARGE_UNIT_His.APPR_STATUS = "2";//覆核完成
                            _ITEM_CHARGE_UNIT_His.APPR_DATE = dt;
                            _ITEM_CHARGE_UNIT_His.APPR_UID = userId;
                            logStr = _ITEM_CHARGE_UNIT_His.modelToString(logStr);
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
                var _ITEM_CHARGE_UNIT_HISList = db.ITEM_CHARGE_UNIT_HIS.AsNoTracking().Where(x => x.APLY_NO == aplyNo).ToList();
                if (_ITEM_CHARGE_UNIT_HISList.Any())
                {
                    foreach (var ItemChargeUnitHis in _ITEM_CHARGE_UNIT_HISList)
                    {
                        //保管單位設定檔
                        var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.FirstOrDefault(x => x.CHARGE_UNIT_ID == ItemChargeUnitHis.CHARGE_UNIT_ID);

                        if (_ITEM_CHARGE_UNIT != null)
                        {
                            _ITEM_CHARGE_UNIT.DATA_STATUS = "1";//可異動
                            _ITEM_CHARGE_UNIT.APPR_UID = userId;
                            _ITEM_CHARGE_UNIT.APPR_DT = dt;
                            _ITEM_CHARGE_UNIT.FREEZE_DT = null;
                            _ITEM_CHARGE_UNIT.FREEZE_UID = null;

                            logStr += _ITEM_CHARGE_UNIT.modelToString(logStr);
                        }
                        else
                        {
                            //return new Tuple<bool, string>(false, logStr);
                        }

                        //保管單位設定異動檔
                        var _ITEM_CHARGE_UNIT_His = db.ITEM_CHARGE_UNIT_HIS.FirstOrDefault(x => x.APLY_NO == ItemChargeUnitHis.APLY_NO && x.HIS_ID == ItemChargeUnitHis.HIS_ID);
                        if (_ITEM_CHARGE_UNIT_His != null)
                        {
                            _ITEM_CHARGE_UNIT_His.APPR_STATUS = "3";//退回
                            _ITEM_CHARGE_UNIT_His.APPR_DATE = dt;
                            _ITEM_CHARGE_UNIT_His.APPR_UID = userId;
                            _ITEM_CHARGE_UNIT_His.APPR_DESC = desc;
                            logStr += _ITEM_CHARGE_UNIT_His.modelToString(logStr);
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

        /// <summary>
        /// 檢查是否有相同欄位存在相同人名
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CheckName(ItemChargeUnitInsertViewModel model)
        {
            bool hasSameName = false;
            if(model != null)
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _ITEM_CHARGE_UNIT = db.ITEM_CHARGE_UNIT.AsNoTracking().FirstOrDefault(x => x.ITEM_ID == model.vTREA_ITEM_NAME && x.CHARGE_UID == model.vCHARGE_UID);

                    if(_ITEM_CHARGE_UNIT != null)
                    {
                        hasSameName = true;
                    }
                }
            }
            return hasSameName;
        }
    }
}