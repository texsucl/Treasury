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
    /// 重要物品
    /// </summary>
    public interface IItemImp : IApply , IAgency, ICDCAction
    {
        
        /// <summary>
        /// 使用 申請單號 抓取資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        List<ItemImpViewModel> GetDataByAplyNo(string aplyNo);

        /// <summary>
        /// 查詢畫面資料
        /// </summary>
        /// <param name="vAplyUnit">申請單位</param>
        /// <param name="aplyNo">取出單號</param>
        /// <returns></returns>
        List<ItemImpViewModel> GetDbDataByUnit(string vAplyUnit = null,string aplyNo = null);
    }
}
