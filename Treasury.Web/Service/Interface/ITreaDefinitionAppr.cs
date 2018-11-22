using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    interface ITreaDefinitionAppr
    {
        List<TDAApprSearchDetailViewModel> GetApprSearchDetail(TDAApprSearchViewModel data);

        /// <summary>
        ///  覆核畫面覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        MSGReturnModel<List<TDAApprSearchDetailViewModel>> Approved(TDAApprSearchViewModel searchData, List<TDAApprSearchDetailViewModel> viewModels);


        /// <summary>
        /// 覆核畫面駁回
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        MSGReturnModel<List<TDAApprSearchDetailViewModel>> Reject(TDAApprSearchViewModel searchData, List<TDAApprSearchDetailViewModel> viewModels, string apprDesc);

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<TDAApprSearchDetailViewModel> GetSearchDetail(TDAApprSearchViewModel data);
    }
}
