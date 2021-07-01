using FRT.Web.BO;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FRT.Web.BO.Utility;

namespace FRT.Web.Service.Interface
{
    interface IORT0109A
    {
        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        MSGReturnModel<List<ORT0109ViewModel>> GetSearchData(string userid);

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="apprDatas">待核可資料</param>
        /// <param name="userId">核可ID</param>
        /// <returns></returns>
        MSGReturnModel ApprovedData(IEnumerable<ORT0109ViewModel> apprDatas, string userId);

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="rejDatas">待駁回資料</param>
        /// <param name="userId">駁回Id</param>
        /// <returns></returns>
        MSGReturnModel RejectedData(IEnumerable<ORT0109ViewModel> rejDatas, string userId);

    }
}
