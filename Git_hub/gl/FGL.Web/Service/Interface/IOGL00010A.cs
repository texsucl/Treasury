using FGL.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FGL.Web.BO.Utility;

namespace FGL.Web.Service.Interface
{
    interface IOGL00010A
    {
        /// <summary>
        /// 查詢待覆核資料
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        MSGReturnModel<List<OGL00010ViewModel>> GetSearchData(string payclass, string userId);

        /// <summary>
        /// 核可
        /// </summary>
        /// <param name="datas">核可資料</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        MSGReturnModel ApprovedData(IEnumerable<OGL00010ViewModel> _datas, string userId);

        /// <summary>
        /// 駁回
        /// </summary>
        /// <param name="datas">駁回資料</param>
        /// <param name="userId">執行人員</param>
        /// <returns></returns>
        MSGReturnModel RejectedData(IEnumerable<OGL00010ViewModel> datas, string userId);
    }
}
