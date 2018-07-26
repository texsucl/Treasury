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
    /// 申請覆核
    /// </summary>
    public interface IApply
    {
        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="insertDatas">新增明細資料</param>
        /// <param name="taData">申請資料</param>
        /// <returns></returns>
        MSGReturnModel<IEnumerable<ITreaItem>> ApplyAudit(IEnumerable<ITreaItem> insertDatas, TreasuryAccessViewModel taData);
    }
}
