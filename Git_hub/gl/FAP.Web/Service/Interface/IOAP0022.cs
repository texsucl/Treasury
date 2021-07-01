using FAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FAP.Web.BO.Utility;

namespace FAP.Web.Service.Interface
{
    public interface IOAP0022
    {
        /// <summary>
        /// 查詢 應付票據變更用印清冊
        /// </summary>
        /// <param name="app_s">覆核日(起)</param>
        /// <param name="sppr_e">覆核日(迄)</param>
        /// <param name="rece_id">接收人</param>
        /// <param name="report_no">表單號碼</param>
        /// <returns></returns>
        List<OAP0022Model> Search_OAP0022(OAP0022SearchModel searchModel);

        /// <summary>
        /// 設定 表單號碼
        /// </summary>
        /// <param name="datas">應付票據</param>
        /// <param name="userId">使用者</param>
        /// <returns></returns>
        MSGReturnModel<string> Set_OAP0022(IEnumerable<OAP0022Model> datas, string userId);
    }
}
