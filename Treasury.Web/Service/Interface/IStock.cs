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
    /// 股票
    /// </summary>
    public interface IStock : IApply
    {
        /// <summary>
        /// 股票編號(新增股票)
        /// </summary>
        /// <returns></returns>
        int GetMaxStockNo();

        /// <summary>
        /// 股票資料
        /// </summary>
        /// <returns></returns>
        List<ItemBookStock> GetStockDate(int GroupNo);

        /// <summary>
        /// 股票名稱
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetStockName(string vAplyUnit = null);

        /// <summary>
        /// 區域
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetAreaType();

        /// <summary>
        /// 類型
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetStockType();

        /// <summary>
        /// 抓取明細資料
        /// </summary>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        IEnumerable<ITreaItem> GetTempData(string aplyNo);

        /// <summary>
        /// 使用 群組編號 抓取在庫股票資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="vAplyUnit">申請部門</param>
        /// <returns></returns>
        List<StockViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit);

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取在庫股票明細資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        List<StockViewModel> GetDetailData(int groupNo, int treaBatchNo);

    }
}
