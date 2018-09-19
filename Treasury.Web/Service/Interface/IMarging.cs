using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 存出保證金
    /// </summary>
    public interface IMarging : IApply, IAgency ,ICDCAction
    {
        /// <summary>
        /// 類別
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetMargingType();

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="vAplyNo">取出單號</param>
        /// <returns></returns>
        List<MargingpViewModel> GetDbDataByUnit(string vAplyUnit = null, string vAplyNo = null);

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="vAplyNo">申請單號</param
        /// <returns></returns>
        List<MargingpViewModel> GetDataByAplyNo(string vAplyNo);
    }
}