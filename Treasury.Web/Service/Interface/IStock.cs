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
        List<SelectOption> GetStockName();

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

    }
}
