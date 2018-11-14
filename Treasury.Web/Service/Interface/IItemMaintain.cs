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
    /// 金庫進出管理作業-金庫存取項目維護作業
    /// </summary>
    public interface IItemMaintain : ITinAction
    {
        /// <summary>
        /// 存取項目名稱
        /// </summary>
        /// <param name="vItem_Name"></param>
        /// <returns></returns>
        bool Check_Item_Name(string vItem_Name);

        /// <summary>
        /// 異動資料中入庫作業類型 Change事件
        /// </summary>
        /// <returns></returns>
        List<SelectOption> OpTypeSelectedChange(string vTREA_OP_TYPE);
    }
}
