using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITREAReviewWork
    {
        /// <summary>
        /// 取得初始資料
        /// </summary>
        /// <returns></returns>
        List<TREAReviewWorkDetailViewModel> GetSearchDatas(string cUserId);

        /// <summary>
        /// 單號查Details
        /// </summary>
        /// <returns></returns>
        List<TREAReviewWorkSearchDetailViewModel> GetDetailsDatas(string RegisterId);

        /// <summary>
        /// 核准
        /// </summary>
        /// <returns></returns>
        MSGReturnModel<List<TREAReviewWorkDetailViewModel>> InsertApplyData(List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId);

        /// <summary>
        /// 駁回
        /// </summary>
        /// <returns></returns>
        MSGReturnModel<List<TREAReviewWorkDetailViewModel>> RejectData(string RejectReason, List<TREAReviewWorkDetailViewModel> ViewModel, string cUserId);
    }
}
