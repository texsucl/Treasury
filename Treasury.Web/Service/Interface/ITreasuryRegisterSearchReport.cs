using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 金庫進出管理作業-金庫登記簿查詢列印作業
    /// </summary>
    public interface ITreasuryRegisterSearchReport
    {
        /// <summary>
        /// 取得查詢結果資料
        /// </summary>
        /// <param name="vCreate_Date_From">入庫日期(起)</param>
        /// <param name="vCreate_Date_To">入庫日期(迄)</param>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        List<TreasuryRegisterSearch> GetSearchList(string vCreate_Date_From, string vCreate_Date_To, string vTrea_Register_Id);

        /// <summary>
        /// 取得明細資料
        /// </summary>
        /// <param name="vTrea_Register_Id">金庫登記簿單號</param>
        /// <returns></returns>
        List<TreasuryRegisterDetail> GetDetailList(string vTrea_Register_Id);

    }
}
