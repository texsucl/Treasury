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
    interface IOAP0029A
    {

        /// <summary>
        /// 查詢 應付票據抽票結果確認功能
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<OAP0029ViewModel> GetSearchData(OAP0029SearchModel searchModel, string userId);

        /// <summary>
        /// 覆核 選擇的案例
        /// </summary>
        /// <param name="apprDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel ApprovedData(IEnumerable<OAP0029ViewModel> apprDatas, string userId);

        /// <summary>
        /// 駁回 選擇的案例
        /// </summary>
        /// <param name="rejDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel RejectedData(IEnumerable<OAP0029ViewModel> rejDatas, string userId);
    }
}
