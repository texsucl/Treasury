using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryAccessCustody
    {
        #region Get

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data">畫面資料</param>
        /// <returns></returns>

        List<TreasuryAccessApprSearchDetailViewModel> GetCustodySearchDetail(TreasuryAccessApprSearchViewModel data);


        /// <summary>
        /// 金庫物品存取申請覆核作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<TreasuryAccessApprSearchDetailViewModel> GetCustodyApprSearchDetail(TreasuryAccessApprSearchViewModel data);

        #endregion

        #region Save

        /// <summary>
        /// 保管單位承辦作業-覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> CustodyApproved(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels);

        /// <summary>
        /// 保管單位承辦作業 駁回
        /// </summary>
        /// <param name="searchData">查詢資料</param>
        /// <param name="viewModels">畫面Cache資料</param>
        /// <param name="apprDesc">駁回訊息</param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> CustodyReject(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels, string apprDesc);

        /// <summary>
        /// 保管單位覆核作業-覆核
        /// </summary>
        /// <param name="searchData">覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Approved(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels);


        /// <summary>
        /// 保管單位覆核作業-駁回
        /// </summary>
        /// <param name="searchData">金庫物品覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Reject(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels, string apprDesc);

        /// <summary>
        /// 修改申請單記錄檔
        /// </summary>
        /// <param name="data">修改資料</param>
        /// <param name="custodianFlag">是否為保管科</param>
        /// <param name="searchData">申請表單查詢顯示區塊ViewModel</param>
        /// <param name="userId">userId</param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> updateAplyNo(TreasuryAccessViewModel data, bool custodianFlag, TreasuryAccessApprSearchViewModel searchData, string userId);

        #endregion

    }
}
