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
    /// 金庫進出管理作業-金庫設備維護作業
    /// </summary>
    public interface ITreasuryMaintain : ITinAction
    {
        /// <summary>
        /// 檢核設備名稱
        /// </summary>
        /// <param name="vEquip_Name">設備名稱</param>
        /// <returns></returns>
        bool Check_Equip_Name(string vEquip_Name);

    }
}
