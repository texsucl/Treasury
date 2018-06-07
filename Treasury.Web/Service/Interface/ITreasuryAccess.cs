using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryAccess 
    {
        /// <summary>
        /// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        /// </summary>
        /// <param name="cUserID">userId</param>
        /// <param name="custodyFlag">管理科Flag</param>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>> TreasuryAccessDetail(string cUserID, bool custodyFlag);

        /// <summary>
        /// 申請單位 變更時 變更申請人
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        List<SelectOption> ChangeUnit(string DPT_CD);
    }
}
