using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Actual
{
    public class AlreadyConfirmedSearch : IAlreadyConfirmedSearch
    {
        public AlreadyConfirmedSearch()
        {

        }

        public Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>> GetFirstTimeData()
        {
            List<SelectOption> IsConfirmed_List = new List<SelectOption>();
            List<SelectOption> OPEN_TREA_TYPE_List = new List<SelectOption>();
            List<SelectOption> Confirm_Id_List = new List<SelectOption>();
            IsConfirmed_List.Add(new SelectOption() { Value = "Y", Text = "已確認" });
            IsConfirmed_List.Add(new SelectOption() { Value = "N", Text = "未確認" });
            try
            {
                using (TreasuryDBEntities db = new TreasuryDBEntities())
                {
                    var _SYS_CODE_OPEN_TREA_TYPE = db.SYS_CODE.AsNoTracking();
                    var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking();

                    //開庫類型
                    OPEN_TREA_TYPE_List = _SYS_CODE_OPEN_TREA_TYPE.Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE")
                        .OrderBy(x => x.ISORTBY)
                        .Select(x => new SelectOption()
                        {
                            Value = x.CODE,
                            Text = x.CODE_VALUE
                        }).ToList();

                    //確認人員
                    Confirm_Id_List.AddRange(_TREA_APLY_REC
                        .Where(x => x.CONFIRM_UID != null)
                        .AsEnumerable()
                        .Select(x => new SelectOption()
                        {
                            Value = x.CONFIRM_UID,
                            Text = string.Format("{0}-{1}", x.CONFIRM_UID, GetUserInfo(x.CONFIRM_UID).EMP_Name)
                        }));
                    Confirm_Id_List = Confirm_Id_List.Distinct(new SelectOption_Comparer()).OrderBy(x => x.Value).ToList();
                }
            }
            catch (Exception ex)
            {
                var message = ex.exceptionMessage();
                throw ex;
            }
            return new Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>>(IsConfirmed_List, OPEN_TREA_TYPE_List, Confirm_Id_List);
        }

        public List<AlreadyConfirmedSearchDetailViewModel> GetSearchDetail(AlreadyConfirmedSearchViewModel searchData)
        {
            List<AlreadyConfirmedSearchDetailViewModel> result = new List<AlreadyConfirmedSearchDetailViewModel>();
            if (!searchData.vDT_From.Any() || !searchData.vDT_To.Any()) // 無查詢日期		
                return result;

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                DateTime? _vDT_From = TypeTransfer.stringToDateTimeN(searchData.vDT_From);
                DateTime? _vDT_To = TypeTransfer.stringToDateTimeN(searchData.vDT_To).DateToLatestTime();
                List<string> register_List = new List<string>();

                var _TREA_OPEN_REC = db.TREA_OPEN_REC.AsNoTracking();
                var _TREA_APLY_REC = db.TREA_APLY_REC.AsNoTracking().AsQueryable();
                var _SYS_CODE_OPEN_TREA_TYPE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "OPEN_TREA_TYPE").ToList();
                var _SYS_CODE_ACCESS_TYPE = db.SYS_CODE.AsNoTracking().Where(x => x.CODE_TYPE == "ACCESS_TYPE").ToList();
                var _TREA_ITEM = db.TREA_ITEM.AsNoTracking();
                var _ITEM_SEAL = db.ITEM_SEAL.AsNoTracking();
                var _OTHER_ITEM_APLY = db.OTHER_ITEM_APLY.AsNoTracking();


                if (_TREA_OPEN_REC.Where(x => x.OPEN_TREA_TYPE == searchData.vOPEN_TREA_TYPE, !searchData.vOPEN_TREA_TYPE.IsNullOrWhiteSpace()).ToList().Any())
                {
                    register_List.AddRange(_TREA_OPEN_REC.Where(x => x.OPEN_TREA_TYPE == searchData.vOPEN_TREA_TYPE, !searchData.vOPEN_TREA_TYPE.IsNullOrWhiteSpace()).Select(x => x.TREA_REGISTER_ID).ToList());
                }

                _TREA_APLY_REC = _TREA_APLY_REC
                    .Where(x => x.CONFIRM_DT >= _vDT_From, _vDT_From != null)
                    .Where(x => x.CONFIRM_DT <= _vDT_To, _vDT_To != null)
                    .Where(x => register_List.Contains(x.TREA_REGISTER_ID), register_List.Any())
                    .Where(x => x.TREA_REGISTER_ID == searchData.vTREA_REGISTER_ID, !searchData.vTREA_REGISTER_ID.IsNullOrWhiteSpace())
                    .Where(x => x.CONFIRM_UID == searchData.vConfirm_Id, !searchData.vConfirm_Id.IsNullOrWhiteSpace());

                result = _TREA_APLY_REC.AsEnumerable()
                    .Select(x => new AlreadyConfirmedSearchDetailViewModel()
                    {
                        vACTUAL_PUT_TIME = _TREA_OPEN_REC.FirstOrDefault(y => y.TREA_REGISTER_ID == x.TREA_REGISTER_ID)?.ACTUAL_PUT_TIME.dateTimeToStr(),
                        vTREA_REGISTER_ID = x.TREA_REGISTER_ID,
                        vOPEN_TREA_TYPE = _SYS_CODE_OPEN_TREA_TYPE.FirstOrDefault(y => y.CODE == _TREA_OPEN_REC.FirstOrDefault(z => z.TREA_REGISTER_ID == x.TREA_REGISTER_ID)?.OPEN_TREA_TYPE)?.CODE_VALUE,
                        vConfirm_Id = string.Format("{0}-{1}", x.CONFIRM_UID, GetUserInfo(x.CONFIRM_UID).EMP_Name),
                        vITEM_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_DESC,
                        vSEAL_DESC = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _ITEM_SEAL.FirstOrDefault(z => z.ITEM_ID == _OTHER_ITEM_APLY.FirstOrDefault(a => a.APLY_NO == x.APLY_NO).ITEM_ID)?.SEAL_DESC: null,
                        vACCESS_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE == "2" ? _SYS_CODE_ACCESS_TYPE.FirstOrDefault(z => z.CODE == x.ACCESS_TYPE)?.CODE_VALUE : null,
                        vAPLY_NO = x.APLY_NO,
                        vACCESS_REASON = x.ACCESS_REASON,
                        hITEM_ID = x.ITEM_ID,
                        hITEM_OP_TYPE = _TREA_ITEM.FirstOrDefault(y => y.ITEM_ID == x.ITEM_ID)?.ITEM_OP_TYPE
                    }).ToList();
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