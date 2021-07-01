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
    interface IOAP0030
    {
        /// <summary>
        /// 查詢 用印檢視確認
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0022Model> GetSearchData(OAP0030SearchModel searchModel);

        /// <summary>
        /// 執行 用印檢視確認
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel SetStatus(IEnumerable<OAP0022Model> updateDatas, string userId);
    }
}
