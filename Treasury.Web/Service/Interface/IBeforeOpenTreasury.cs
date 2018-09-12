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
    /// 金庫登記簿執行作業-開庫前
    /// </summary>
    public interface IBeforeOpenTreasury
    {
        /// <summary>
        /// 開庫類型
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetOpenTreaType();

        /// <summary>
        /// 取得每日例行進出未確認項目
        /// </summary>
        /// <returns></returns>
        List<BeforeOpenTreasuryViewModel> GetRoutineList();

        /// <summary>
        /// 取得已入庫確認資料
        /// </summary>
        /// <returns></returns>
        List<BeforeOpenTreasuryViewModel> GetStorageList();

        /// <summary>
        /// 產生工作底稿
        /// </summary>
        /// <returns></returns>
        MSGReturnModel<IEnumerable<ITreaItem>> DraftData();

    }
}
