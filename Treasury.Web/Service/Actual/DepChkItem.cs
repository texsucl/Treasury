using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.Models;
using Treasury.Web.Service.Interface;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

/// <summary>
/// 功能說明：金庫進出管理作業-定存檢核表項目設定
/// 初版作者：20181107 侯蔚鑫
/// 修改歷程：20181107 侯蔚鑫 
///           需求單號：
///           初版
/// ==============================================
/// 修改日期/修改人：
/// 需求單號：
/// 修改內容：
/// ==============================================
/// </summary>
namespace Treasury.Web.Service.Actual
{
    public class DepChkItem : Common, IDepChkItem
    {
        #region GetData
        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        public IEnumerable<ITinItem> GetSearchData(ITinItem searchModel)
        {
            var searchData = (DepChkItemSearchViewModel)searchModel;
            List<DepChkItemViewModel> result = new List<DepChkItemViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {
                var emps = GetEmps();
                var _Dep_Chk_Item_HisList = db.DEP_CHK_ITEM_HIS.AsNoTracking()
                    .Where(x => x.APPR_STATUS == "1").ToList();
                var _Exec_Action = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "EXEC_ACTION").ToList();
                var _Data_Status_Name = db.SYS_CODE.AsNoTracking()
                    .Where(x => x.CODE_TYPE == "DATA_STATUS").ToList();

                result.AddRange(db.DEP_CHK_ITEM.AsNoTracking()
                    .Where(x => x.IS_DISABLED == searchData.vIs_Disabled, searchData.vIs_Disabled != "All")
                    .AsEnumerable()
                    .Select((x) => new DepChkItemViewModel()
                    {
                        vAccess_Type = x.ACCESS_TYPE,
                        vIsortby = x.ISORTBY,
                        vExec_Action = _Dep_Chk_Item_HisList.FirstOrDefault(y => y.ACCESS_TYPE == x.ACCESS_TYPE && y.ISORTBY == x.ISORTBY)?.EXEC_ACTION?.Trim(),
                        vExec_Action_Name = _Exec_Action.FirstOrDefault(y => y.CODE == _Dep_Chk_Item_HisList.FirstOrDefault(z => z.ACCESS_TYPE == x.ACCESS_TYPE && z.ISORTBY == x.ISORTBY)?.EXEC_ACTION?.Trim())?.CODE_VALUE?.Trim(),
                        vDep_Chk_Item_Desc = x.DEP_CHK_ITEM_DESC,
                        vIs_Disabled = x.IS_DISABLED,
                        vReplace = x.REPLACE,
                        vData_Status = x.DATA_STATUS,
                        vData_Status_Name = _Data_Status_Name.FirstOrDefault(y => y.CODE == x.DATA_STATUS)?.CODE_VALUE?.Trim(),
                        vLast_Update_Dt = x.LAST_UPDATE_DT,
                        vFreeze_Uid_Name = emps.FirstOrDefault(y => y.USR_ID == x.FREEZE_UID)?.EMP_NAME?.Trim(),
                        vAply_No = _Dep_Chk_Item_HisList.FirstOrDefault(y => y.ACCESS_TYPE == x.ACCESS_TYPE && y.ISORTBY == x.ISORTBY)?.APLY_NO?.Trim()
                    }).ToList());
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
            var searchData = (DepChkItemChangeRecordSearchViewModel)searchModel;
            List<DepChkItemChangeRecordViewModel> result = new List<DepChkItemChangeRecordViewModel>();

            using (TreasuryDBEntities db = new TreasuryDBEntities())
            {

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
            var searchData = (DepChkItemSearchViewModel)searchModel;
            var result = new MSGReturnModel<IEnumerable<ITinItem>>();
            result.RETURN_FLAG = false;
            DateTime dt = DateTime.Now;

            try
            {

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

            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        #endregion
    }
}