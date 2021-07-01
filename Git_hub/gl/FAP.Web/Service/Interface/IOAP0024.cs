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
    interface IOAP0024
    {

        /// <summary>
        /// 查詢 應付票據簽收資料–維護(尚未簽收)
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        MSGReturnModel<List<OAP0024Model>> Search_OAP0024(OAP0024SearchModel searchModel);


        /// <summary>
        /// 修改 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MSGReturnModel updateOAP0024(OAP0024Model model);

        /// <summary>
        /// 刪除 應付票據簽收資料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        MSGReturnModel deleteOAP0024(OAP0024Model model);

        /// <summary>
        /// 依部門找尋 應付票據簽收窗口 明細檔
        /// </summary>
        /// <param name="dep_id"></param>
        /// <returns></returns>
        List<SelectOption> getupdateDatas(string dep_id);
    }
}
