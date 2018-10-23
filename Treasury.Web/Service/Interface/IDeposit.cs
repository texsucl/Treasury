using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface IDeposit : IApply, IAgency, ICDCAction
    {
        /// <summary>
        /// 幣別
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetCurrency();

        /// <summary>
        /// 交易對象
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetTrad_Partners();
        
        /// <summary>
        /// 計息方式
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetInterest_Rate_Type();

        /// <summary>
        /// 存單類型
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetDep_Type();

        /// <summary>
        /// 是否標籤
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetYN_Flag();

        /// <summary>
        /// 使用 交易對象 抓取在庫定期單明細資料
        /// </summary>
        /// <param name="vTrad_Partners">交易對象</param>
        /// <param name="vAplyNo">申請單號</param>
        /// <returns></returns>
        List<Deposit_D> GetDataByTradPartners(string vTrad_Partners, string vAplyNo = null);

        /// <summary>
        /// 使用 交易對象 抓取異動在庫定期單明細資料
        /// </summary>
        /// <param name="vTrad_Partners">交易對象</param>
        /// <param name="vItemId">物品單號</param>
        /// <param name="vData_Seq">明細流水號</param>
        /// <returns></returns>
        List<CDCDeposit_D> GetCDC_DataByTradPartners(string vTrad_Partners, string vItemId, string vData_Seq);

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <param name="vTransExpiryDateFrom">定存單到期日(起)</param>
        /// <param name="vTransExpiryDateTo">定存單到期日(迄)</param>
        /// <returns></returns>
        DepositViewModel GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null, string vTransExpiryDateFrom = null, string vTransExpiryDateTo = null);

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        DepositViewModel GetDataByAplyNo(string vAplyNo);

        /// <summary>
        /// 依申請單號取得列印群組資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        List<DepositReportGroupData> GetReportGroupData(string vAplyNo);

        /// <summary>
        /// 查詢定期存單交易對象
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetTRAD_Partners();
    }
}
