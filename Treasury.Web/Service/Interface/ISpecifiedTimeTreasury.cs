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
	}
}
