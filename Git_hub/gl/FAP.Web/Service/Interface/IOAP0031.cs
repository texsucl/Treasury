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
    interface IOAP0031
    {
        /// <summary>
        /// 信封標籤檔案作業
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        List<OAP0031ViewModel> Search_OAP0031(OAP0031SearchModel searchModel);

        /// <summary>
        /// 產出標籤號碼
        /// </summary>
        /// <param name="models">傳入資料</param>
        /// <param name="userid">使用者ID</param>
        /// <returns></returns>
        MSGReturnModel SetLabel_No(IEnumerable<OAP0031ViewModel> models, string userid);

        /// <summary>
        /// 匯入 大宗掛號號碼
        /// </summary>
        /// <param name="models">傳入資料</param>
        /// <param name="userid">使用者ID</param>
        /// <returns></returns>
        MSGReturnModel Setbulk_no(IEnumerable<OAP0031ViewModel> models, string userid);
    }
}
