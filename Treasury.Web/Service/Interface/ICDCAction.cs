using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 庫存異動作業 事件
    /// </summary>
    public interface ICDCAction
    {
        #region Get
        /// <summary>
        /// 查詢庫存異動資料
        /// </summary>
        /// <param name="searchModel">異動畫面查詢ViwModel</param>
        /// <param name="APLY_NO">資料庫異動申請單紀錄檔 申請單號</param>
        /// <param name="charge_Dept">權責部門</param>
        /// <param name="charge_Sect">權責科別</param>
        /// <returns></returns>
        IEnumerable<ICDCItem> GetCDCSearchData(CDCSearchViewModel searchModel,string aply_No = null,string charge_Dept = null,string charge_Sect = null);
        #endregion

        #region Save

        /// <summary>
        /// 庫存異動資料-申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <param name="searchModel">異動畫面查詢ViwModel</param>
        /// <returns></returns>
        MSGReturnModel<IEnumerable<ICDCItem>> CDCApplyAudit(IEnumerable<ICDCItem> saveData, CDCSearchViewModel searchModel);

        /// <summary>
        /// 庫存異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        Tuple<bool,string> CDCReject(TreasuryDBEntities db, List<string> itemIDs,string logStr , DateTime dt);

        /// <summary>
        /// 庫存異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        Tuple<bool,string> CDCApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt);

        /// <summary>
        /// 庫存權責異動資料-駁回
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">駁回的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        Tuple<bool, string> CDCChargeReject(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt);

        /// <summary>
        /// 庫存權責異動資料-覆核
        /// </summary>
        /// <param name="db">Entities</param>
        /// <param name="itemIDs">覆核的申請單號</param>
        /// <param name="logStr">log</param>
        /// <param name="dt">執行時間</param>
        /// <returns></returns>
        Tuple<bool, string> CDCChargeApproved(TreasuryDBEntities db, List<string> itemIDs, string logStr, DateTime dt);
        #endregion

    }
}
