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
    /// 存入保證金
    /// </summary>
    public interface IMarginp : IApply , IAgency, ICDCAction
    {
        /// <summary>
        /// 抓取存入保證金類別設定
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetMarginp_Take_Of_Type();

        /// <summary>
        /// 抓取存入保證金物品名稱設定
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetMarginpItem();

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        List<MarginpViewModel> GetDataByAplyNo(string aplyNo);

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        List<MarginpViewModel> GetDbDataByUnit(string vAplyUnit = null,string aplyNo = null);
    }
}
