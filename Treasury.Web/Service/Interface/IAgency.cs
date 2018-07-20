using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.Models;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 作廢 OR 取消申請
    /// </summary>
    public interface IAgency
    {
        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        Tuple<bool, string> ObSolete(TreasuryDBEntities db, string aply_No, string logStr, DateTime dt);

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="db">Entity</param>
        /// <param name="aply_No">作廢單號</param>
        /// <param name="logStr">log 字串</param>
        /// <param name="dt">更新時間</param>
        /// <returns></returns>
        Tuple<bool, string> CancelApply(TreasuryDBEntities db, string aply_No, string logStr, DateTime dt);
    }
}
