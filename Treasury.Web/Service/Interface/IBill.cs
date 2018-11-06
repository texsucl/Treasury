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
    /// 空白票據
    /// </summary>
    public interface IBill : IApply, IAgency , ICDCAction
    {
        /// <summary>
        /// 類型
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetCheckType();

        /// <summary>
        /// 發票行庫
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetIssuing_Bank();

        /// <summary>
        /// 抓取明細資料
        /// </summary>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        IEnumerable<ITreaItem> GetTempData(string aplyNo);

        /// <summary>
        /// 抓取庫存明細資料
        /// </summary>
        /// <param name="vAplyUnit">申請部門</param>
        /// <param name="inventoryStatus">庫存狀態</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        IEnumerable<ITreaItem> GetDayData(string vAplyUnit = null, string inventoryStatus = null, string aplyNo = null);
    }
}
