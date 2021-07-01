using FAP.Web.Utilitys;
using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    interface IOAP0026
    {
        /// <summary>
        /// 查詢抽票部門權限關聯維護
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0026ViewModel> GetSearchData(OAP0026SearchModel searchModel);

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="updateDatas">申請資料</param>
        /// <param name="userId">申請人員ID</param>
        /// <returns></returns>
        MSGReturnModel ApplyDeptData(IEnumerable<OAP0026ViewModel> updateDatas, string userId);

        /// <summary>
        /// 檢核重覆資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        bool CheckSameData(OAP0026ViewModel viewModel);

        /// <summary>
        /// 查詢給付類型 & 中文
        /// </summary>
        /// <param name="valueFlag">true 內容前面帶參數</param>
        /// <returns>1.給付類型 2.中文</returns>
        List<Tuple<string, string>> getData(bool valueFlag = false);
    }
}
