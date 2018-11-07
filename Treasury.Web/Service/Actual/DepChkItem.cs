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
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> TinReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {

            }
            return new Tuple<bool, string>(true, logStr);
        }

        /// <summary>
        /// 金庫進出管理作業-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        public Tuple<bool, string> TinApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt)
        {
            foreach (var itemID in itemIDs)
            {

            }
            return new Tuple<bool, string>(true, logStr);
        }
        #endregion

        #region privation function
        #endregion
    }
}