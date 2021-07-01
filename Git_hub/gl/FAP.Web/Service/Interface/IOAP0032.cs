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
    interface IOAP0032
    {
        /// <summary>
        /// 查詢 信封標籤檔案-恢復作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        MSGReturnModel<List<OAP0031ViewModel>> Search_OAP0032(OAP0032SearchModel searchModel);

        /// <summary>
        /// 清空大宗號碼
        /// </summary>
        /// <param name="label_nos">標籤號碼</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        MSGReturnModel Clearbulk_no(IEnumerable<string> label_nos, string userid);

        /// <summary>
        /// 清空標籤號碼
        /// </summary>
        /// <param name="label_nos">標籤號碼</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        MSGReturnModel Clearlabel_no(IEnumerable<string> label_nos, string userid);
    }
}
