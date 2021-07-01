using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    interface IOAP0018A
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0018AViewModel> GetReviewSearchDetail(OAP0018ASearchViewModel searchModel);

        /// <summary>
        /// 查詢HIS
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0018AHisViewModel> GetHisData(string aply_no);
        
        /// <summary>
        ///  覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        MSGReturnModel<List<OAP0018AViewModel>> ApprovedData(OAP0018ASearchViewModel searchData, List<OAP0018AViewModel> viewModels);

        /// <summary>
        /// 覆核畫面駁回
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        MSGReturnModel<List<OAP0018AViewModel>> RejectedData(OAP0018ASearchViewModel searchData, List<OAP0018AViewModel> viewModels, string apprDesc);
    }
}
