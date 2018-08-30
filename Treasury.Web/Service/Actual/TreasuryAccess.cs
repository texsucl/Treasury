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

namespace Treasury.Web.Service.Actual
{

    public class TreasuryAccess : ITreasuryAccess
    {
        /// <summary>
        /// 報表可以列印狀態
        /// </summary>
        private List<string> printsStatus { get; set; }

        /// <summary>
        /// 單號可以作廢狀態
        /// </summary>
        private List<string> invalidStatus { get; set; }

        /// <summary>
        /// 金庫物品存取申請覆核作業符合狀態
        /// </summary>
        private List<string> apprStatus { get; set; }

        public TreasuryAccess()
        {
            printsStatus = new List<string>();
            printsStatus.Add(Ref.AccessProjectFormStatus.B01.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.B02.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.B03.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.B04.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.C01.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.C02.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.D01.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.D02.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.D03.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.D04.ToString());
            printsStatus.Add(Ref.AccessProjectFormStatus.E01.ToString());
            invalidStatus = new List<string>();
            invalidStatus.Add(Ref.AccessProjectFormStatus.A02.ToString());
            invalidStatus.Add(Ref.AccessProjectFormStatus.A03.ToString());
            invalidStatus.Add(Ref.AccessProjectFormStatus.A04.ToString());
            invalidStatus.Add(Ref.AccessProjectFormStatus.A05.ToString());
            apprStatus = new List<string>();
            apprStatus.Add(Ref.AccessProjectFormStatus.A01.ToString());
            apprStatus.Add(Ref.AccessProjectFormStatus.A05.ToString());
        }

        #region Get Date

        /// <summary>
        /// 取得單號狀態
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        public string GetStatus(string aplyNo)
        {
            var result = string.Empty;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (_TAR != null)
                    result = _TAR.APLY_STATUS;
            }
            return result;
        }

        /// <summary>
        /// 使用單號抓取 申請表單資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        public TreasuryAccessViewModel GetTreasuryAccessViewModel(string aplyNo)
        {
            var result = new TreasuryAccessViewModel();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var TAR = db.TREA_APLY_REC.AsNoTracking()
                    .FirstOrDefault(x => x.APLY_NO == aplyNo);
                if (TAR != null)
                {
                    result.vAplyNo = TAR.APLY_NO;
                    result.vAccessReason = TAR.ACCESS_REASON;
                    result.vAccessType = TAR.ACCESS_TYPE;
                    result.vAplyUid = TAR.APLY_UID;
                    result.vAplyUnit = TAR.APLY_UNIT;
                    result.vItem = TAR.ITEM_ID;
                    result.vExpectedAccessDate = TAR.EXPECTED_ACCESS_DATE?.ToString("yyyy/MM/dd");
                    result.vCreateUid = TAR.CREATE_UID;
                    result.vCreateUnit = TAR.CREATE_UNIT;
                    result.vLastUpdateTime = TAR.LAST_UPDATE_DT;
                }
            }
            return result;
        }

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
        /// 取得是否可以修改狀態
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="uid"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public bool GetActType(string aplyNo,string uid, List<string> actionType)
        {
            bool result = false;
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var _TAR = db.TREA_APLY_REC.AsNoTracking()
                            .FirstOrDefault(x => x.APLY_NO == aplyNo && x.CREATE_UID == uid);
                if (_TAR != null)
                    result = actionType.Contains(_TAR.APLY_STATUS);
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

        /// <summary>
        /// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        /// </summary>
        /// <param name="cUserID">userId</param>
        /// <param name="custodyFlag">管理科Flag</param>
        /// <param name="unit">科別指定</param>
        /// <returns></returns>
        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, BaseUserInfoModel> TreasuryAccessDetail(string cUserID, bool custodyFlag, string unit= null)
        {
            List<SelectOption> applicationProject = new List<SelectOption>(); //申請項目
            List<SelectOption> applicationUnit = new List<SelectOption>(); //申請單位
            List<SelectOption> applicant = new List<SelectOption>(); //申請人
            var empty = new SelectOption() { Text = string.Empty, Value = string.Empty };
            BaseUserInfoModel user = GetUserInfo(cUserID); //填表人 資料
            try
            {
                using (DB_INTRAEntities dbINTRA = new DB_INTRAEntities())
                {
                    var depts = dbINTRA.VW_OA_DEPT.AsNoTracking().ToList();
                    using (TreasuryDBEntities db = new TreasuryDBEntities())
                    {
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
                                if (unit != null)
                                {
                                    var _first = applicationUnit.FirstOrDefault(x=>x.Value == unit);
                                    if (_first != null)
                                    {
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
                            }
                            if (!applicant.Any())
                                applicant.Add(empty);
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

                            if (user != null)
                            {
                                applicationUnit.Add(new SelectOption()
                                {
                                    Value = user.DPT_ID,
                                    Text = user.DPT_Name 
                                });
                                applicant.Add(new SelectOption()
                                { 
                                    Value = user.EMP_ID,
                                    Text = $@"{user.EMP_ID}({user.EMP_Name})"
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

            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, BaseUserInfoModel>(applicationProject, applicationUnit, applicant, user);
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
        /// <param name="data">金庫物品存取主畫面查詢ViewModel</param>
        /// <returns></returns>
        public List<TreasuryAccessSearchDetailViewModel> GetSearchDetail(TreasuryAccessSearchViewModel data)
        {
            List<TreasuryAccessSearchDetailViewModel> result = new List<TreasuryAccessSearchDetailViewModel>();

            if (!data.vItem.Any() || !data.vAplyUnit.Any()) //無查詢項目 or 申請單位 表示沒有權限查詢
                return result;
            var depts = GetDepts();
            var emps = GetEmps();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();
                DateTime? _vActualAccessDate_S = TypeTransfer.stringToDateTimeN(data.vActualAccessDate_S);
                DateTime? _vActualAccessDate_E = TypeTransfer.stringToDateTimeN(data.vActualAccessDate_E).DateToLatestTime();
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
                        Ref.AccessProjectFormStatus.D03.ToString(),
                        Ref.AccessProjectFormStatus.E01.ToString()}; //狀態須符合這兩個
                    _data = _data.Where(x => status.Contains(x.APLY_STATUS))
                        .Join(db.TREA_OPEN_REC.AsNoTracking()
                        .Where(x => x.REGI_APPR_DT >= _vActualAccessDate_S, _vActualAccessDate_S != null)
                        .Where(x => x.REGI_APPR_DT <= _vActualAccessDate_E, _vActualAccessDate_E != null),
                        x => x.TREA_REGISTER_ID,
                        y => y.TREA_REGISTER_ID,
                        (x, y) => x);
                }
                var dataTAR = _data.ToList();
                var TRIDs = dataTAR.Where(x => x.TREA_REGISTER_ID != null).Select(x => x.TREA_REGISTER_ID).ToList();
                var dataTOR = db.TREA_OPEN_REC.AsNoTracking().Where(x => TRIDs.Contains(x.TREA_REGISTER_ID)).ToList();
                result.AddRange(
                    from TAR in dataTAR
                    join TOR in dataTOR
                    on TAR.TREA_REGISTER_ID equals TOR.TREA_REGISTER_ID into temp
                    from TOR in temp.DefaultIfEmpty()
                    select new TreasuryAccessSearchDetailViewModel
                    {
                        vACCESS_REASON = TAR.ACCESS_REASON,
                        vAPLY_DT = TypeTransfer.dateTimeNToString(TAR.APLY_DT),
                        vREGI_APPR_DT = TypeTransfer.dateTimeNToString(TOR?.REGI_APPR_DT),
                        vAPLY_NO = TAR.APLY_NO,
                        vAPLY_STATUS = TAR.APLY_STATUS,
                        vAPLY_STATUS_D = formStatus.FirstOrDefault(x => x.CODE == TAR.APLY_STATUS)?.CODE_VALUE,
                        vAPLY_UNIT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == TAR.APLY_UNIT)?.DPT_NAME,
                        vAPLY_UID = TAR.APLY_UID,
                        vAPLY_UID_NAME = emps.FirstOrDefault(x => x.USR_ID == TAR.APLY_UID)?.EMP_NAME,
                        vCancleFlag = TAR.APLY_STATUS == Ref.AccessProjectFormStatus.A01.ToString() && TAR.CREATE_UID == data.vCreateUid ? "Y" : "N",
                        vInvalidFlag = invalidStatus.Contains(TAR.APLY_STATUS) &&
                              TAR.CREATE_UID == data.vCreateUid ? "Y" : "N",
                        vPrintFlag = printsStatus.Contains(TAR.APLY_STATUS) ? "Y" : "N",
                        vItem = TAR.ITEM_ID,
                        vItemDec = treaItems.FirstOrDefault(x => x.ITEM_ID == TAR.ITEM_ID)?.ITEM_DESC,
                        vDESC = !TAR.APLY_APPR_DESC.IsNullOrWhiteSpace() ? TAR.APLY_APPR_DESC : TAR.CUSTODY_APPR_DESC,
                        vACCESS_TYPE = TAR.ACCESS_TYPE,
                        vLast_Update_Time = TAR.LAST_UPDATE_DT
                    });
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
                    result.vAplyUnit = data.APLY_UNIT;
                    result.vAplyUid = data.APLY_UID;
                    result.vChargeUnit = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME;
                    //result.vAplyUid = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME;
                    result.vAccessType = data.ACCESS_TYPE == "P" ? "存入" : data.ACCESS_TYPE == "G" ? "取出" : ""; //存入(P) or 取出(G)
                    result.vExpectedAccessDate = TypeTransfer.dateTimeNToString(data.EXPECTED_ACCESS_DATE);
                    result.vCreateDt = TypeTransfer.dateTimeNToString(data.CREATE_DT);
                    var _createEmp = emps.FirstOrDefault(x => x.USR_ID == data.CREATE_UID);
                    result.vCreateUnit = depts.FirstOrDefault(y => y.DPT_CD.Trim() == _createEmp?.DPT_CD?.Trim())?.DPT_NAME;
                    result.vCreateUid = _createEmp?.EMP_NAME;
                    result.vAccessReason = data.ACCESS_REASON;
                    result.vLastUpdateTime = data.LAST_UPDATE_DT;
                }
            }
            return result;
        }

        /// <summary>
        /// 金庫物品存取申請覆核作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<TreasuryAccessApprSearchDetailViewModel> GetApprSearchDetail(TreasuryAccessApprSearchViewModel data)
        {
            List<TreasuryAccessApprSearchDetailViewModel> result = new List<TreasuryAccessApprSearchDetailViewModel>();
            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var depts = GetDepts();
                var emps = GetEmps();
                var treaItems = db.TREA_ITEM.AsNoTracking().Where(x => x.ITEM_OP_TYPE == "3").ToList();
                DateTime? _vAPLY_DT_S = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_S);
                DateTime? _vAPLY_DT_E = TypeTransfer.stringToDateTimeN(data.vAPLY_DT_E).DateToLatestTime();
                result = db.TREA_APLY_REC.AsNoTracking()
                    .Where(x => x.APLY_DT >= _vAPLY_DT_S, _vAPLY_DT_S != null) //申請日期(起)
                    .Where(x => x.APLY_DT <= _vAPLY_DT_E, _vAPLY_DT_E != null) //申請日期(迄)
                    .Where(x => x.APLY_NO == data.vAPLY_NO, !data.vAPLY_NO.IsNullOrWhiteSpace()) //申請單號
                    .Where(x => x.CREATE_UNIT == data.vCreateUnit && apprStatus.Contains(x.APLY_STATUS)) //相同部門 & 符合狀態 的資料
                    .AsEnumerable()
                    .Select(x => TreaAplyRecToTAASDViewModel(data.vCreateUid, x, treaItems, depts, emps)).ToList();
            }
            return result;
        }

        /// <summary>
        /// 獲取 員工資料
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
        /// 獲取 部門資料
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

        #endregion

        #region Save Data

        /// <summary>
        /// 刪除申請 (刪除資料)
        /// </summary>
        /// <param name="searchData">金庫物品存取主畫面查詢ViewModel</param>
        /// <param name="data">申請表單查詢顯示區塊ViewModel</param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Cancel(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data)
        {
            var result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
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


                    #region 刪除申請
                    var sampleFactory = new SampleFactory();
                    var getAgenct = sampleFactory.GetAgenct(EnumUtil.GetValues<Ref.TreaItemType>().First(x => x.ToString() == _TREA_APLY_REC.ITEM_ID));
                    if (getAgenct != null)
                    {
                        var _recover = getAgenct.CancelApply(db, _TREA_APLY_REC.APLY_NO, _TREA_APLY_REC.ACCESS_TYPE, logStr, dt);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr = _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
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
                            log.CFUNCTION = "刪除申請-金庫物品存取申請作業";
                            log.CACTION = "D";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, searchData.vCreateUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = $@"單號{data.vAPLY_NO} 已刪除!";
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
        /// 作廢 (保留資料)
        /// </summary>
        /// <param name="searchData">金庫物品存取主畫面查詢ViewModel</param>
        /// <param name="data">申請表單查詢顯示區塊ViewModel</param>
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Invalidate(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data)
        {
            var result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
            result.RETURN_FLAG = false;
            result.DESCRIPTION = Ref.MessageType.already_Change.GetDescription();
            DateTime dt = DateTime.Now;
            string logStr = string.Empty;
            //取得流水號
            var _status = Ref.AccessProjectFormStatus.E02.ToString();
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
                    logStr += _TREA_APLY_REC.modelToString(logStr);

                    #region 作廢
                    var sampleFactory = new SampleFactory();
                    var getAgenct = sampleFactory.GetAgenct(EnumUtil.GetValues<Ref.TreaItemType>().First(x => x.ToString() == _TREA_APLY_REC.ITEM_ID));
                    if (getAgenct != null)
                    {
                        var _recover = getAgenct.ObSolete(db, _TREA_APLY_REC.APLY_NO, _TREA_APLY_REC.ACCESS_TYPE, logStr, dt);
                        if (!_recover.Item1) //失敗
                        {
                            return result;
                        }
                        logStr += _recover.Item2;
                    }
                    else
                    {
                        return result;
                    }
                    #endregion

                    #region 申請單歷程檔
                    //「取消申請」：新增「E02」申請人刪除的狀態資料。
                    var ARH = new APLY_REC_HIS()
                    {
                        APLY_NO = _TREA_APLY_REC.APLY_NO,
                        APLY_STATUS = _status,
                        PROC_UID = searchData.vCreateUid,
                        PROC_DT = dt
                    };
                    logStr += ARH.modelToString(logStr);

                    db.APLY_REC_HIS.Add(ARH);

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
                            log.CACTION = "U";
                            log.CCONTENT = logStr;
                            LogDao.Insert(log, searchData.vCreateUid);
                            #endregion

                            result.RETURN_FLAG = true;
                            result.DESCRIPTION = $@"單號{data.vAPLY_NO} 已作廢!";
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
        /// 覆核畫面覆核
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
                    var aplyStatus = Ref.AccessProjectFormStatus.B01.ToString(); // 狀態 => 申請單位覆核完成，保管科確認中
                    if (_TREA_APLY_REC.CUSTODY_UID != null &&
                        _TREA_APLY_REC.CUSTODY_UID == _TREA_APLY_REC.CREATE_UID)
                       //新增人員等於保管科人員 狀態 => 入庫確認中
                    {
                        aplyStatus = Ref.AccessProjectFormStatus.C01.ToString();
                    }
                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    // 申請單位
                    if (aplyStatus == Ref.AccessProjectFormStatus.B01.ToString())
                    {
                        _TREA_APLY_REC.APLY_APPR_UID = searchData.vCreateUid;
                        _TREA_APLY_REC.APLY_APPR_DT = dt;
                    }
                    // 保管科單位
                    else
                    {
                        _TREA_APLY_REC.CUSTODY_APPR_UID = searchData.vCreateUid;
                        _TREA_APLY_REC.CUSTODY_APPR_DT = dt;
                    }
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
                        log.CFUNCTION = "覆核-金庫物品存取覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 覆核成功!";
                        result.Datas = GetApprSearchDetail(searchData);
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
        /// 覆核畫面駁回
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

                    var aplyStatus = Ref.AccessProjectFormStatus.A02.ToString(); // 狀態 => 申請單位覆核駁回

                    _TREA_APLY_REC.LAST_UPDATE_DT = dt;
                    _TREA_APLY_REC.APLY_STATUS = aplyStatus;
                    _TREA_APLY_REC.LAST_UPDATE_UID = searchData.vCreateUid;
                    // 保管科單位
                    if (_TREA_APLY_REC.CUSTODY_UID != null &&
                        _TREA_APLY_REC.CUSTODY_UID == _TREA_APLY_REC.CREATE_UID)
                    {
                        _TREA_APLY_REC.CUSTODY_APPR_DESC = apprDesc;
                        _TREA_APLY_REC.CUSTODY_APPR_DT = dt;
                        _TREA_APLY_REC.CUSTODY_APPR_UID = searchData.vCreateUid;
                    }
                    // 申請單位
                    else
                    {
                        _TREA_APLY_REC.APLY_APPR_DESC = apprDesc;
                        _TREA_APLY_REC.APLY_APPR_DT = dt;
                        _TREA_APLY_REC.APLY_APPR_UID = searchData.vCreateUid;
                    }
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
                        log.CFUNCTION = "駁回-金庫物品存取覆核作業";
                        log.CACTION = "U";
                        log.CCONTENT = logStr;
                        LogDao.Insert(log, searchData.vCreateUid);
                        #endregion

                        result.RETURN_FLAG = true;
                        result.DESCRIPTION = $"申請單號 : {string.Join(",", aplynos)} 已駁回!";
                        result.Datas = GetApprSearchDetail(searchData);
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
        /// <returns></returns>
        public MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> updateAplyNo(TreasuryAccessViewModel data,bool custodianFlag, TreasuryAccessSearchViewModel searchData)
        {
            MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> result = new MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>>();
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
                    if (custodianFlag)
                    {
                        updateData.APLY_UNIT = data.vAplyUnit;
                        updateData.APLY_UID = data.vAplyUid;
                    }
                    updateData.ACCESS_REASON = data.vAccessReason;
                    updateData.EXPECTED_ACCESS_DATE = TypeTransfer.stringToDateTimeN(data.vExpectedAccessDate);
                    updateData.LAST_UPDATE_DT = DateTime.Now;
                    try
                    {
                        db.SaveChanges();
                        result.DESCRIPTION = Ref.MessageType.update_Success.GetDescription();
                        result.RETURN_FLAG = true;
                        result.Datas = GetSearchDetail(searchData);                       
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
                vAPLY_DT = data.APLY_DT?.DateToTaiwanDate(9),
                vAPLY_NO = data.APLY_NO,
                vAPLY_STATUS = data.APLY_STATUS,
                vAPLY_STATUS_D = formStatus.FirstOrDefault(x => x.CODE == data.APLY_STATUS)?.CODE_VALUE,
                vAPLY_UNIT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME,
                vAPLY_UID = data.APLY_UID,
                vAPLY_UID_NAME = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME,
                vCancleFlag = data.APLY_STATUS == Ref.AccessProjectFormStatus.A01.ToString() && data.CREATE_UID == userId ? "Y" : "N",
                vInvalidFlag = invalidStatus.Contains(data.APLY_STATUS) &&
                              data.CREATE_UID == userId ? "Y" : "N",
                vPrintFlag = printsStatus.Contains(data.APLY_STATUS) ? "Y" : "N",
                vItem = data.ITEM_ID,
                vItemDec = treaItems.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC,
                vDESC = !data.APLY_APPR_DESC.IsNullOrWhiteSpace() ? data.APLY_APPR_DESC : data.CUSTODY_APPR_DESC,
                vACCESS_TYPE = data.ACCESS_TYPE,
                vLast_Update_Time = data.LAST_UPDATE_DT
            };
        }

        private TreasuryAccessApprSearchDetailViewModel TreaAplyRecToTAASDViewModel(
            string userId,
            TREA_APLY_REC data,
            List<TREA_ITEM> treaItems,
            List<VW_OA_DEPT> depts,
            List<V_EMPLY2> emps
            )
        {
            return new TreasuryAccessApprSearchDetailViewModel()
            {
                vItem = data.ITEM_ID,
                vItemDec = treaItems.FirstOrDefault(x => x.ITEM_ID == data.ITEM_ID)?.ITEM_DESC,
                vAPLY_DT = data.APLY_DT?.DateToTaiwanDate(9),
                vAPLY_NO = data.APLY_NO,
                vAPLY_UNIT = depts.FirstOrDefault(y => y.DPT_CD.Trim() == data.APLY_UNIT)?.DPT_NAME,
                vAPLY_UID = data.APLY_UID,
                vAPLY_UID_NAME = emps.FirstOrDefault(x => x.USR_ID == data.APLY_UID)?.EMP_NAME,
                vAPPRFlag = data.CREATE_UID != userId,
                vACCESS_REASON = data.ACCESS_REASON,
                vLast_Update_Time = data.LAST_UPDATE_DT
            };
        }

        #endregion
    }
}