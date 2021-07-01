using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    interface IOAP0018
    {
        #region Get
        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="searchModel">查詢ViwModel</param>
        /// <returns></returns>
        List<OAP0018ViewModel> GetSearchData(OAP0018SearchViewModel searchModel);

        /// <summary>
        /// 檢查收據與部門TABLE 是否有相同資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CheckSameData(OAP0018InsertViewModel model, string mod);
        #endregion

        #region Save
        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="saveData">申請覆核的資料</param>
        /// <returns></returns>
        MSGReturnModel<string> ApplyDeptData(List<OAP0018ViewModel> saveData);
        #endregion
    }
}
