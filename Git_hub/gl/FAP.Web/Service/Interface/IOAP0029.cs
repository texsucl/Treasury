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
    interface IOAP0029
    {
        /// <summary>
        /// 查詢 應付票據抽票結果回覆功能
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<OAP0029ViewModel> GetSearchData(OAP0029SearchModel searchModel, string userId);

        /// <summary>
        /// 回覆 接收 or 駁回
        /// </summary>
        /// <param name="updateDatas"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MSGReturnModel ApplyDeptData(IEnumerable<OAP0029ViewModel> updateDatas, string userId);

        /// <summary>
        /// 使用支票號碼 獲得資料 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        MSGReturnModel<OAP0029ViewModel> getCheckNo(OAP0029ViewModel searchModel);

        /// <summary>
        /// 設定支票號碼
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userid"></param>
        MSGReturnModel setCheckNo(OAP0029ViewModel model, string userid);
    }
}
