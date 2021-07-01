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
    /// 比對報表勾稽_批次定義(OPEN跨系統勾稽)
    /// </summary>
    interface IORT0105
    {
        /// <summary>
        /// 查詢 跨系統勾稽作業_批次定義
        /// </summary>
        /// <returns></returns>
        MSGReturnModel<List<ORT0105ViewModel>> GetSearchData(string type = null, string kind = null);

        /// <summary>
        /// 申請 跨系統勾稽作業_批次定義
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel ApplyDeptData(IEnumerable<ORT0105ViewModel> updateDatas, string userId);

        /// <summary>
        /// 檢核重覆資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        bool CheckSameData(ORT0105ViewModel viewModel);

        /// <summary>
        /// 執行頻率(中文)
        /// </summary>
        /// <param name="frequency">頻率類別</param>
        /// <param name="frequency_value">頻率參數</param>
        /// <returns></returns>
        string frequency_d(string frequency, int frequency_value);

        /// <summary>
        /// 資料區間起始日(中文)
        /// </summary>
        /// <param name="start_date_type">資料區間起始日類別</param>
        /// <param name="start_date_value">資料區間起始日類別參數</param>
        /// <returns></returns>
        string start_date_d(string start_date_type, string start_date_value);
    }
}
