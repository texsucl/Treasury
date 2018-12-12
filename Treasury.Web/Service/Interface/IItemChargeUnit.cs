using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    interface IItemChargeUnit : ITinAction
    {
        /// <summary>
        /// 畫面初始
        /// </summary>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>> FirstDropDown();
        /// <summary>
        /// Selected Change 事件
        /// </summary>
        /// <param name="Charge_Dept"></param>
        /// <param name="Charge_Sect"></param>
        /// <param name="Charge_Uid"></param>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>> DialogSelectedChange(string Charge_Dept, string Charge_Sect);

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="AplyNo"></param>
        /// <param name="searchModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<string> ResetData(string AplyNo, ItemChargeUnitSearchViewModel searchModel, string cUserId);

        /// <summary>
        /// 新增時檢查項目是否已存在經辦
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CheckName(ItemChargeUnitInsertViewModel model);
    }
}
