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
    public interface IStock : IApply, IAgency, ICDCAction
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
        List<ItemBookStock> GetStockDate(int GroupNo, string vAplyNo = null);

        /// <summary>
        /// 股票名稱
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetStockName(string vAplyUnit = null, string vAplyNo = null);

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
        /// 使用 群組編號 抓取在庫股票資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="vAplyUnit">申請部門</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        List<StockDetailViewModel> GetDataByGroupNo(int groupNo, string vAplyUnit, string aplyNo = null);

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取在庫股票明細資料
        /// </summary>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <returns></returns>
        List<StockDetailViewModel> GetDetailData(int groupNo, int treaBatchNo);

        /// <summary>
        /// 使用 群組編號及入庫批號 抓取異動在庫股票明細資料
        /// </summary>
        /// <param name="searchModel">CDC 查詢畫面條件</param>
        /// <param name="groupNo">群組編號</param>
        /// <param name="treaBatchNo">入庫批號</param>
        /// <param name="aplyNo">申請單號</param>
        /// <returns></returns>
        List<CDCStockViewModel> GetCDCDetailData(CDCSearchViewModel searchModel, int groupNo, int treaBatchNo, string aplyNo);

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <param name="EditFlag">可否修改,可以也需抓取庫存資料</param>
        /// <returns></returns>
        StockViewModel GetDataByAplyNo(string aplyNo, bool EditFlag = false);

        /// <summary>
        /// 股票名稱模糊比對
        /// </summary>
        /// <param name="StockName">股票名稱</param>
        /// <returns></returns>
        List<ItemBookStock> GetStockCheck(string StockName);
    }
}
