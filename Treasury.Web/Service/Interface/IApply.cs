using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface IApply
    {
        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="insertDatas">新增明細資料</param>
        /// <param name="taData">申請資料</param>
        /// <returns></returns>
        MSGReturnModel<ITreaItem> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData);

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
