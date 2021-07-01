using FRT.Web.BO;
using FRT.Web.Models;
using FRT.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FRT.Web.BO.Utility;

namespace FRT.Web.Service.Interface
{
    interface IORT0106
    {
        /// <summary>
        /// 使用 id 查詢 跨系統勾稽作業檔
        /// </summary>
        /// <param name="check_id"></param>
        /// <returns></returns>
        MSGReturnModel<ORT0106ViewModel> getCheck(string check_id);


        /// <summary>
        /// 查詢現有 勾稽報表
        /// </summary>
        /// <returns></returns>
        List<SelectOption> getCross_System(string reserve1 = "");


        void updateRunFlag(FRT_CROSS_SYSTEM_CHECK schedulerModel, string runFlag);
    }
}
