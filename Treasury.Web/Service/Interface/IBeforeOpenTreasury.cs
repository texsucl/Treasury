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
        /// 金庫登記簿
        /// </summary>
        /// <returns></returns>
        TreaOpenRec GetTreaOpenRec(); 

        /// <summary>
        /// 取得每日例行進出未確認項目
        /// </summary>
        /// <returns></returns>
        List<BeforeOpenTreasuryViewModel> GetRoutineList(string TreaRegisterId);

        /// <summary>
        /// 取得已入庫確認資料
        /// </summary>
        /// <param name="TreaRegisterId">金庫登記簿單號</param>
        /// <returns></returns>
        List<BeforeOpenTreasuryViewModel> GetStorageList(string TreaRegisterId);

        /// <summary>
        /// 產生工作底稿
        /// </summary>
        /// <param name="currentUserId">目前使用者ID</param>
        /// <param name="Trea_Register_Id">金庫開庫單號</param>
        /// <returns></returns>
        MSGReturnModel<IEnumerable<ITreaItem>> DraftData(string currentUserId, string Trea_Register_Id);

    }
}
