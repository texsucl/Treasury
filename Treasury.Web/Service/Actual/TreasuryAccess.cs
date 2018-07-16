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
using static Treasury.Web.Enum.Ref;

namespace Treasury.Web.Service.Actual
{

    public class TreasuryAccess : ITreasuryAccess
    {
        /// <summary>
        /// 報表可以列印狀態
        /// </summary>
        private List<string> printsStatus { get; set; }

        public TreasuryAccess()
        {
            printsStatus = new List<string>();
            printsStatus.Add(AccessProjectFormStatus.A02.ToString());
            printsStatus.Add(AccessProjectFormStatus.B01.ToString());
            printsStatus.Add(AccessProjectFormStatus.B02.ToString());
            printsStatus.Add(AccessProjectFormStatus.B03.ToString());
            printsStatus.Add(AccessProjectFormStatus.B04.ToString());
            printsStatus.Add(AccessProjectFormStatus.C01.ToString());
            printsStatus.Add(AccessProjectFormStatus.C02.ToString());
            printsStatus.Add(AccessProjectFormStatus.D01.ToString());
            printsStatus.Add(AccessProjectFormStatus.D02.ToString());
            printsStatus.Add(AccessProjectFormStatus.D03.ToString());
            printsStatus.Add(AccessProjectFormStatus.D04.ToString());
            printsStatus.Add(AccessProjectFormStatus.E01.ToString());
        }

        #region Get Date

        /// <summary>
        /// 取得 存入or取出
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public string GetAccessType(string aplyNo)
        {
            var result = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                    result = _TAR.ACCESS_TYPE;
            }
            return result;
        }

        /// <summary>
        /// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        /// </summary>
        /// <param name="cUserID">userId</param>
        /// <param name="custodyFlag">管理科Flag</param>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, string, string> TreasuryAccessDetail(string cUserID, bool custodyFlag)
        {
            List<SelectOption> applicationProject = new List<SelectOption>(); //申請項目
            List<SelectOption> applicationUnit = new List<SelectOption>(); //申請單位
            List<SelectOption> applicant = new List<SelectOption>(); //申請人
            string createUser = string.Empty; //填表人
            string createDep = string.Empty; //填表單位
            try
            {
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    var depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
                        var _emply = dbINTRA.V_EMPLY2.AsNoTracking()
                            .FirstOrDefault(x => x.USR_ID == cUserID);
                        if (_emply != null)
                        {
                            createUser = _emply.EMP_NAME;
                            createDep = _emply.DPT_NAME;
                        }
                        #region 保管科人員
                        if (custodyFlag) //是保管科人員
                        {
                            applicationProject = db.TREA_ITEM.AsNoTracking()
                                .Where(x => x.IS_DISABLED == "N" && x.ITEM_OP_TYPE == "3") //「入庫作業類型=3」且啟用中的存取項目
                                .OrderBy(x => x.ITEM_ID)
                                .AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = x.ITEM_ID,
                                    Text = x.ITEM_DESC
                                }).ToList();

                            var Units = new List<string>();

                            //自【保管單位設定檔】中挑出啟用中的單位
                            db.ITEM_CHARGE_UNIT.AsNoTracking()
                                .Where(x => x.IS_DISABLED == "N").ToList()
                                .ForEach(x => {
                                    if (!x.CHARGE_SECT.IsNullOrWhiteSpace())
                                        Units.Add(x.CHARGE_SECT.Trim());
                                    else
                                        Units.Add(x.CHARGE_DEPT.Trim());
                                });



                            applicationUnit = Units.Distinct().OrderBy(x => x)
                                .AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value = x,
                                    Text = depts.FirstOrDefault(y => y.DPT_CD.Trim() == x)?.DPT_NAME
                                }).ToList();
                            if (applicationUnit.Any())
                            {
                                var _first = applicationUnit.First();
                                applicant = dbINTRA.V_EMPLY2.AsNoTracking()
                                    .Where(x => x.DPT_CD == _first.Value)
                                    .AsEnumerable()
                                    .Select(x => new SelectOption()
                                    {
                                        Value = x.USR_ID,
                                        Text = $@"{x.USR_ID}({x.EMP_NAME})"
                                    }).ToList();
                            }

                        }
                        #endregion
                        #region 非保管科人員
                        else
                        {
                            applicationProject =
                                db.CODE_USER_ROLE.AsNoTracking()
                                .Where(x => x.USER_ID == cUserID) //登入者所擁有的角色
                                .Join(db.CODE_ROLE_ITEM.AsNoTracking()
                                .Where(x => x.AUTH_TYPE == "2"), //表單申請權限=Y
                                x => x.ROLE_ID,
                                y => y.ROLE_ID,
                                (x, y) => y
                                ).Join(db.TREA_ITEM.AsNoTracking(), //金庫存取作業設定檔
                                x => x.ITEM_ID,
                                y => y.ITEM_ID,
                                (x, y) => y
                                ).AsEnumerable()
                                .Select(x => new SelectOption()
                                {
                                    Value =x.ITEM_ID,
                                    Text = x.ITEM_DESC
                                }).ToList();

                            if (_emply != null)
                            {
                                applicationUnit.Add(new SelectOption()
                                {
                                    Value = _emply.DPT_CD?.Trim(),
                                    Text = _emply.DPT_NAME
                                });
                                applicant.Add(new SelectOption()
                                {
                                    Value = _emply.USR_ID,
                                    Text = $@"{_emply.USR_ID}({_emply.EMP_NAME})"
                                });
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.exceptionMessage();
                throw ex;
            }

            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, string, string>(applicationProject, applicationUnit, applicant, createUser, createDep);
        }

        /// <summary>
        /// 申請單位 變更時 變更申請人
        /// </summary>
        /// <param name="DPT_CD">單位</param>
        /// <returns></returns>
        public List<SelectOption> ChangeUnit(string DPT_CD)
        {
            List<SelectOption> results = new List<SelectOption>();
            using (DB_INTRAEntities db = new DB_INTRAEntities())
            {
                results = db.V_EMPLY2.AsNoTracking()
                  .Where(x => x.DPT_CD == DPT_CD)
                  .AsEnumerable()
                  .Select(x => new SelectOption()
                  {
                      Value = x.USR_ID,
                      Text = $@"{x.USR_ID}({x.EMP_NAME})"
                  }).ToList();
            }
            return results;
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<TreasuryAccessSearchDetailViewModel> GetSearchDetail(TreasuryAccessSearchViewModel data)
        {
            List<TreasuryAccessSearchDetailViewModel> result = new List<TreasuryAccessSearchDetailViewModel>();
            var depts = new List<VW_OA_DEPT>();
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E);
                DateTime? _vActualAccessDate_S = TypeTransfer.stringToDateTimeN(data.vActualAccessDate_S);
                DateTime? _vActualAccessDate_E = TypeTransfer.stringToDateTimeN(data.vActualAccessDate_E);
                var formStatus = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                var treaItems = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "3").ToList();

                var _data = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => data.vItem.Contains(x.ITEM_ID) && data.vAplyUnit.Contains(x.APLY_UNIT)) //項目 & 申請單位 
                    .Where(x => x.APLY_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.APLY_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.APLY_NO == data.vAPLY_NO, !data.vAPLY_NO.IsNullOrWhiteSpace()) //申請單號
                    .Where(x => x.TREA_REGISTER_ID == data.vTREA_REGISTER_ID, !data.vTREA_REGISTER_ID.IsNullOrWhiteSpace()); //金庫登記簿單號

                if (_vActualAccessDate_S != null || _vActualAccessDate_E != null) //實際存取
                {
                    var status = new List<string>() {
                        AccessProjectFormStatus.D03.ToString(),
                        AccessProjectFormStatus .E01.ToString()}; //狀態須符合這兩個
                    _data = _data.Where(x => status.Contains(x.APLY_STATUS))
                        .Join(db.TREA_OPEN_REC.AsNoTracking()
                        .Where(x => x.REGI_APPR_DT >= _vActualAccessDate_S, _vActualAccessDate_S != null)
                        .Where(x => x.REGI_APPR_DT <= _vActualAccessDate_E, _vActualAccessDate_E != null),
                        x => x.TREA_REGISTER_ID,
                        y => y.TREA_REGISTER_ID,
                        (x, y) => x);
                }
                result.AddRange(_data.AsEnumerable()
                    .Select(x => TreaAplyRecToTASDViewModel(data.vCreateUid, x, treaItems, formStatus, depts, emps)));
            }
            return result;
        }

        /// <summary>
        /// 查詢申請單紀錄資料by單號
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public TreasuryAccessViewModel GetByAplyNo(string aplyNo)
        {
            var result = new TreasuryAccessViewModel();
            var depts = new List<VW_OA_DEPT>();
            var emps = new List<V_EMPLY2>();
            using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
            {
                depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
                emps = dbINTRA.V_EMPLY2.AsNoTracking().ToList();
            }
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var formStatus = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "FORM_STATUS").ToList();
                var treaItemTypes = db.TREA_ITEM.AsNoTracking().Where(x => x.IS_DISABLED == "N" && x.ITEM_OP_TYPE == "3").ToList();
                var data = db.TREA_APLY_REC.AsNoTracking().FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (data != null)
                {
                    result.vAplyNo = data.APLY_NO;
                    result.vItem = treaItemTypes.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC;
                    result.vAplyUnit = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME;
                    result.vAplyUid = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME;
                    result.vAccessType = data.ACCESS_TYPE == "P" ? "存入" : data.ACCESS_TYPE == "G" ? "取出" : ""; //存入(P) or 取出(G)
                    result.vExpectedAccessDate = data.EXPECTED_ACCESS_DATE?.ToSimpleTaiwanDate();
                    result.vCreateDt = data.CREATE_DT?.ToSimpleTaiwanDate();
                    var _createEmp = emps.FirstOrDefault(x => x.USR_ID == data.CREATE_UID);
                    result.vCreateUnit = depts.FirstOrDefault(y => y.DPT_CD.Trim() == _createEmp?.DPT_CD?.Trim())?.DPT_NAME;
                    result.vCreateUid = _createEmp?.EMP_NAME;
                    result.vAccessReason = data.ACCESS_REASON;
                }
            }
            return result;
        }

        /// <summary>
        /// 覆核查詢 (待完成)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<TreasuryAccessApprSearchDetailViewModel> GetApprSearchDetail(TreasuryAccessApprSearchViewModel data)
        {
            List<TreasuryAccessApprSearchDetailViewModel> result = new List<TreasuryAccessApprSearchDetailViewModel>();
            return result;
        }
        #endregion

        #region Save Data

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Cancel(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data)
        {
            var result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.already_Change.GetDescription();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            var _status = AccessProjectFormStatus.E02.ToString();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == data.vAPLY_NO);
                if (_TREA_APLY_REC != null)
                {
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > data.vLast_Update_Time)
                    {
                        return result;
                    }
                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    _TREA_APLY_REC.APLY_STATUS = _status;
                    logStr += _TREA_APLY_REC.modelToString();
                    if (_TREA_APLY_REC.ACCESS_TYPE == AccessProjectTradeType.G.ToString()) //取出情況下 取號須加回庫存
                    {
                        #region BILL (空白票據)
                        if (_TREA_APLY_REC.ITEM_ID == TreaItemType.D1012.ToString()) //空白票據
                        {
                            var _recover = new Bill().Recover(db, _TREA_APLY_REC.APLY_NO, logStr, dt);
                            if (!_recover.Item1) //失敗
                            {
                                return result;
                            }
                            logStr = _recover.Item2;
                        }
                        #endregion
                    }

                    #region 申請單歷程檔
                    //「取消申請」：新增「E02」申請人刪除的狀態資料。
                    db.APLY_REC_HIS.Add(
                    new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = _status,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    });

                    #endregion
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
                            log.CFUNCTION = "取消申請-金庫物品存取申請作業";
                            log.CACTION = "U";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, searchData.vCreateUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = $@"單號{data.vAPLY_NO} 取消申請成功!";
                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
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
        /// 作廢
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Invalidate(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data)
        {
            var result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = MessageType.already_Change.GetDescription();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            //取得流水號
            SysSeqDao sysSeqDao = new SysSeqDao();
            String qPreCode = DateUtil.getCurChtDateTime().Split(' ')[0];
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TREA_APLY_REC = db.TREA_APLY_REC.FirstOrDefault(x => x.APLY_NO == data.vAPLY_NO);
                if (_TREA_APLY_REC != null)
                {
                    if (_TREA_APLY_REC.LAST_UPDATE_DT > data.vLast_Update_Time)
                    {
                        return result;
                    }
                    logStr += _TREA_APLY_REC.modelToString();
                    if (_TREA_APLY_REC.ACCESS_TYPE == AccessProjectTradeType.G.ToString()) //取出情況下 取號須加回庫存
                    {
                        #region BILL (空白票據)
                        if (_TREA_APLY_REC.ITEM_ID == TreaItemType.D1012.ToString()) //空白票據
                        {
                            var _recover = new Bill().Recover(db, _TREA_APLY_REC.APLY_NO, logStr, dt);
                            if (!_recover.Item1) //失敗
                            {
                                return result;
                            }
                            logStr = _recover.Item2;
                        }
                        #endregion

                    }


                    #region 刪除 空白票據申請資料檔
                    db.BLANK_NOTE_APLY.RemoveRange(db.BLANK_NOTE_APLY.Where(x => x.APLY_NO == _TREA_APLY_REC.APLY_NO));
                    #endregion

                    #region 刪除 申請單歷程檔
                    db.APLY_REC_HIS.RemoveRange(db.APLY_REC_HIS.Where(x => x.APLY_NO == _TREA_APLY_REC.APLY_NO));
                    #endregion

                    #region 刪除 申請單紀錄檔
                    db.TREA_APLY_REC.Remove(_TREA_APLY_REC);
                    #endregion

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
                            log.CFUNCTION = "作廢-金庫物品存取申請作業";
                            log.CACTION = "D";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, searchData.vCreateUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = $@"單號{data.vAPLY_NO} 作廢成功!";
                        }
                        catch (DbUpdateException ex)
                        {
                            result.DESCRIPTION = ex.exceptionMessage();
                        }
                    }
                }
                if (result.RETURN_FLAG)
                {
                    result.Datas = GetSearchDetail(searchData);
                }
            }
            return result;
        }

        #endregion

        #region private function

        /// <summary>
        /// 資料庫資料轉檔 ViewModel
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="data"></param>
        /// <param name="treaItemTypes"></param>
        /// <param name="formStatus"></param>
        /// <param name="depts"></param>
        /// <param name="emps"></param>
        /// <returns></returns>
        private TreasuryAccessSearchDetailViewModel TreaAplyRecToTASDViewModel(
            string userId,
            TREA_APLY_REC data,
            List<TREA_ITEM> treaItems,
            List<SYS_CODE> formStatus,
            List<VW_OA_DEPT> depts,
            List<V_EMPLY2> emps)
        {
            return new TreasuryAccessSearchDetailViewModel()
            {
                vACCESS_REASON = data.ACCESS_REASON,
                vAPLY_DT = data.APLY_DT?.ToString("yyyy/MM/dd"),
                vAPLY_NO = data.APLY_NO,
                vAPLY_STATUS = data.APLY_STATUS,
                vAPLY_STATUS_D = formStatus.FirstOrDefault(x => x.CODE == data.APLY_STATUS)?.CODE_VALUE,
                vAPLY_UNIT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME,
                vAPLY_UID = data.APLY_UID,
                vAPLY_UID_NAME = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME,
                vCancleFlag = data.APLY_STATUS == AccessProjectFormStatus.A01.ToString() && data.CREATE_UID == userId ? "Y" : "N",
                vInvalidFlag = (data.APLY_STATUS == AccessProjectFormStatus.A04.ToString() ||
                             data.APLY_STATUS == AccessProjectFormStatus.A03.ToString()) &&
                              data.CREATE_UID == userId ? "Y" : "N",
                vPrintFlag = printsStatus.Contains(data.APLY_STATUS) ? "Y" : "N",
                vItem = data.ITEM_ID,
                vItemDec = treaItems.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC,
                vDESC = !data.APLY_APPR_DESC.IsNullOrWhiteSpace() ? data.APLY_APPR_DESC : data.CUSTODY_APPR_DESC,
                vACCESS_TYPE = data.ACCESS_TYPE,
                vLast_Update_Time = data.LAST_UPDATE_DT
            };
        }

        #endregion
    }
}