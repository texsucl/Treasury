using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryRegistration 
    {
        #region Get

        /// <summary>
        /// 
        /// </summary>
        TreasuryRegistrationViewModel GetItemId();

        ///// <summary>
        ///// 取得 人員基本資料
        ///// </summary>
        ///// <param name="cUserID">userId</param>
        ///// <returns></returns>
        //BaseUserInfoModel GetUserInfo(string cUserID);

        ///// <summary>
        ///// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        ///// </summary>
        ///// <param name="cUserID">userId</param>
        ///// <param name="custodyFlag">管理科Flag</param>
        ///// <param name="unit">科別指定</param>
        ///// <returns></returns>
        //Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, BaseUserInfoModel> TreasuryAccessDetail(string cUserID, bool custodyFlag, string unit = null);

        ///// <summary>
        ///// 申請單位 變更時 變更申請人
        ///// </summary>
        ///// <param name="DPT_CD"></param>
        ///// <returns></returns>
        //List<SelectOption> ChangeUnit(string DPT_CD);

        ///// <summary>
        ///// 查詢
        ///// </summary>
        ///// <param name="data">畫面資料</param>
        ///// <returns></returns>

        //List<TreasuryAccessSearchDetailViewModel> GetSearchDetail(TreasuryAccessSearchViewModel data);

        ///// <summary>
        ///// 查詢申請單紀錄資料by單號
        ///// </summary>
        ///// <param name="aplyNo"></param>
        ///// <returns></returns>
        //TreasuryAccessViewModel GetByAplyNo(string aplyNo);

        ///// <summary>
        ///// 取得 存入or取出
        ///// </summary>
        ///// <param name="aplyNo"></param>
        ///// <returns></returns>
        //string GetAccessType(string aplyNo);

        ///// <summary>
        ///// 取得是否可以修改狀態
        ///// </summary>
        ///// <param name="aplyNo"></param>
        ///// <param name="uid"></param>
        ///// <param name="actionType"></param>
        ///// <returns></returns>
        //bool GetActType(string aplyNo, string uid, List<string> actionType);

        ///// <summary>
        ///// 取得單號狀態
        ///// </summary>
        ///// <param name="aplyNo"></param>
        ///// <returns></returns>
        //string GetStatus(string aplyNo);

        ///// <summary>
        ///// 使用單號抓取 申請表單資料
        ///// </summary>
        ///// <param name="aplyNo">單號</param>
        ///// <returns></returns>
        //TreasuryAccessViewModel GetTreasuryAccessViewModel(string aplyNo);

        ///// <summary>
        ///// 金庫物品存取申請覆核作業 覆核查詢 
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //List<TreasuryAccessApprSearchDetailViewModel> GetApprSearchDetail(TreasuryAccessApprSearchViewModel data);

        #endregion

        //#region Save

        ///// <summary>
        ///// 取消申請
        ///// </summary>
        ///// <param name="searchData">金庫物品存取主畫面查詢ViewModel</param>
        ///// <param name="data">申請表單查詢顯示區塊ViewModel</param>
        ///// <returns></returns>
        //MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Cancel(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data);

        ///// <summary>
        ///// 作廢
        ///// </summary>
        ///// <param name="searchData">金庫物品存取主畫面查詢ViewModel</param>
        ///// <param name="data">申請表單查詢顯示區塊ViewModel</param>
        ///// <returns></returns>
        //MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Invalidate(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data);


        ///// <summary>
        ///// 覆核畫面覆核
        ///// </summary>
        ///// <param name="searchData">金庫物品覆核畫面查詢ViewModel</param>
        ///// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        ///// <returns></returns>
        //MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Approved(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels);


        ///// <summary>
        ///// 覆核畫面駁回
        ///// </summary>
        ///// <param name="searchData">金庫物品覆核畫面查詢ViewModel</param>
        ///// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        ///// <param name="apprDesc">駁回意見</param>
        ///// <returns></returns>
        //MSGReturnModel<List<TreasuryAccessApprSearchDetailViewModel>> Reject(TreasuryAccessApprSearchViewModel searchData, List<TreasuryAccessApprSearchDetailViewModel> viewModels, string apprDesc);

        ///// <summary>
        ///// 修改申請單記錄檔nnn
        ///// </summary> 
        ///// <param name="data">修改資料</param>
        ///// <param name="custodianFlag">是否為保管科</param>
        ///// <param name="searchData">申請表單查詢顯示區塊ViewModel</param>
        ///// <returns></returns>
        //MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> updateAplyNo(TreasuryAccessViewModel data, bool custodianFlag, TreasuryAccessSearchViewModel searchData);
        //#endregion

    }
}
