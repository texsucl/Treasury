using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
	public interface ISpecifiedTimeTreasury
	{
		/// <summary>
		/// 取金庫存取項目
		/// </summary>
		/// <returns></returns>
		Tuple<List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, List<CheckBoxListInfo>, string> GetTreaItem();

		/// <summary>
		/// 查詢
		/// </summary>
		/// <param name="data">畫面資料</param>
		/// <returns></returns>
		List<SpecifiedTimeTreasurySearchDetailViewModel> GetSearchDetail(SpecifiedTimeTreasurySearchViewModel data);

		/// <summary>
		/// 新增申請覆核
		/// </summary>
		/// <param name="data">畫面資料</param>
		/// <param name="currentUserId">目前使用者</param>
		/// /// <param name="searchData"></param>
		/// <returns></returns>
		MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> InsertApplyData(SpecifiedTimeTreasuryApplyViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData);

		/// <summary>
		/// 修改申請覆核
		/// </summary>
		/// <param name="data">畫面資料</param>
		/// <param name="currntUserId">目前使用者</param>
		/// <param name="searchData"></param>
		/// <returns></returns>
		MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> UpdateApplyData(SpecifiedTimeTreasuryUpdateViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData);
		
		/// <summary>
		/// 取消申請覆核
		/// </summary>
		/// <param name="data">畫面資料</param>
		/// <param name="currntUserId">目前使用者</param>
		/// <param name="searchData"></param>
		/// <returns></returns>
		MSGReturnModel<List<SpecifiedTimeTreasurySearchDetailViewModel>> CancelApplyData(SpecifiedTimeTreasuryCancelViewModel data, string currentUserId, SpecifiedTimeTreasurySearchViewModel searchData);

        /// <summary>
        /// 覆核作業查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<SpecifiedTimeTreasuryApprSearchDetailViewModel> GetApprSearchData(SpecifiedTimeTreasuryApprSearchViewModel data);

        /// <summary>
        /// 覆核
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <param name="ViewModel"></param>
        /// <param name="SearchData"></param>
        /// <returns></returns>
        MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> ApproveData(List<string> RegisterNo, List<SpecifiedTimeTreasuryApprSearchDetailViewModel> ViewModel, SpecifiedTimeTreasuryApprSearchViewModel SearchData);
        
        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <param name="RejectReason"></param>
        /// <param name="ViewModel"></param>
        /// <param name="SearchData"></param>
        /// <returns></returns>
        MSGReturnModel<List<SpecifiedTimeTreasuryApprSearchDetailViewModel>> RejectData(List<string> RegisterNo, string RejectReason, List<SpecifiedTimeTreasuryApprSearchDetailViewModel> ViewModel, SpecifiedTimeTreasuryApprSearchViewModel SearchData);

        /// <summary>
        /// 工作項目
        /// </summary>
        /// <param name="RegisterNo"></param>
        /// <returns></returns>
        List<SpecifiedTimeTreasuryApprReasonDetailViewModel> GetReasonDetail(List<string> RegisterNo);

        /// <summary>
        /// 檢查開庫紀錄檔是否有狀態不為E01的單號
        /// </summary>
        /// <returns></returns>
        List<string> CheckRegisterId();
    }
}
