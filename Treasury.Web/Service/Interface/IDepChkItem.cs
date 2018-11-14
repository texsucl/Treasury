using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 金庫進出管理作業-定存檢核表項目設定
    /// </summary>
    public interface IDepChkItem : ITinAction
    {
        /// <summary>
        /// 依交易別 抓取排序資料
        /// </summary>
        /// <param name="vAccess_Type">交易別</param>
        /// <returns></returns>
        List<DepChkItemViewModel> GetOrderData(string vAccess_Type);

        /// <summary>
        /// 依交易別 檢查是否有排序資料
        /// </summary>
        /// <param name="vAccess_Type">交易別</param>
        /// <returns></returns>
        bool CheckOrderData(string vAccess_Type);

        /// <summary>
        /// 金庫進出管理作業-順序調整申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        MSGReturnModel<IEnumerable<ITinItem>> TinOrderApplyAudit(IEnumerable<ITinItem> saveData, ITinItem searchModel);
    }
}
