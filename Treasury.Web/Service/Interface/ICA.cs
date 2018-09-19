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
    /// 電子憑證
    /// </summary>
    public interface ICA : IApply , IAgency, ICDCAction
    {
        /// <summary>
        /// 抓取電子憑證用途設定
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetCA_Use();

        /// <summary>
        /// 抓取電子憑證品項設定
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetCA_Desc();

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        List<CAViewModel> GetDataByAplyNo(string aplyNo);

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        List<CAViewModel> GetDbDataByUnit(string vAplyUnit = null,string aplyNo = null);
    }
}
