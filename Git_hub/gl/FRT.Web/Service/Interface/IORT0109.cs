using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FRT.Web.BO.Utility;

namespace FRT.Web.Service.Interface
{
    /// <summary>
    /// 跨系統資料庫勾稽銀存銷帳不比對帳號 Interface
    /// </summary>
    interface IORT0109
    {
        /// <summary>
        /// 查詢 跨系統資料庫勾稽銀存銷帳不比對帳號
        /// </summary>
        /// <returns></returns>
        MSGReturnModel<List<ORT0109ViewModel>> GetSearchData(string bank_acct_no = null);

        /// <summary>
        /// 申請 跨系統資料庫勾稽銀存銷帳不比對帳號
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel ApplyDeptData(IEnumerable<ORT0109ViewModel> updateDatas, string userId);

        /// <summary>
        /// 檢核重覆資料 & 是否有存在Wanpie帳號基本資料輸入(UUU050201017771QS)檔案
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Tuple<bool, string, string> CheckData(ORT0109ViewModel viewModel);

    }
}
