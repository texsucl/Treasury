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
    interface IOAP0027
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0027ViewModel> GetSearchData(OAP0027SearchModel searchModel);

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="updateDatas">申請資料</param>
        /// <param name="userId">申請人員ID</param>
        /// <returns></returns>
        MSGReturnModel ApplyDeptData(IEnumerable<OAP0027ViewModel> updateDatas, string userId);

        /// <summary>
        /// 檢核重覆資料
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        bool CheckSameData(OAP0027ViewModel viewModel);

        /// <summary>
        /// 查詢抽票原因 代碼 & 中文
        /// </summary>
        /// <param name="valueFlag"></param>
        /// <returns></returns>
        List<Tuple<string, string>> getData(bool valueFlag = false);
    }
}
