using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ISeal : IApply
    {

        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        List<SealViewModel> GetDataByAplyNo(string aplyNo);

        /// <summary>
        /// 查詢庫存資料
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="vAplyUnit"></param>
        /// <returns></returns>
        List<SealViewModel> GetDbDataByUnit(string itemId, string vAplyUnit = null);
    }
}
