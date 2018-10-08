using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.Enum;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryReport
    {
        #region Get
        /// <summary>
        /// 
        /// </summary>
        TreasuryReportViewModel GetItemId();

        /// <summary>
        /// 抓取部門
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<SelectOption> getDEPT(Ref.TreaItemType type);

        /// <summary>
        /// 抓取科別
        /// </summary>
        /// <param name="DEPT_ITEM">部門ID</param>
        /// <param name="type"></param>
        /// <returns></returns>
        List<SelectOption> getSECT(string DEPT_ITEM, Ref.TreaItemType type);
        #endregion

        #region Save

        #endregion

    }
}
