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
    interface IOAP0026A
    {
        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        List<OAP0026ViewModel> GetSearchData(OAP0026SearchModel searchModel, string userid);

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="apprDatas">待核可資料</param>
        /// <param name="userId">核可ID</param>
        /// <returns></returns>
        MSGReturnModel ApprovedData(IEnumerable<OAP0026ViewModel> apprDatas, string userId);

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="rejDatas">待駁回資料</param>
        /// <param name="userId">駁回Id</param>
        /// <returns></returns>
        MSGReturnModel RejectedData(IEnumerable<OAP0026ViewModel> rejDatas, string userId);
    }
}
