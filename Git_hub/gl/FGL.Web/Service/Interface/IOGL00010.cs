using FGL.Web.ViewModels;
using System.Collections.Generic;
using static FGL.Web.BO.Utility;

namespace FGL.Web.Service.Interface
{
    interface IOGL00010
    {
        /// <summary>
        /// 查詢 退費類別維護
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <returns></returns>
        MSGReturnModel<List<OGL00010ViewModel>> GetSearchData(string payclass);

        /// <summary>
        /// 申請異動資料
        /// </summary>
        /// <param name="applyDatas">異動資料</param>
        /// <param name="userid">執行人員</param>
        /// <returns></returns>
        MSGReturnModel ApplyData(IEnumerable<OGL00010ViewModel> applyDatas, string userid);

        /// <summary>
        /// 檢核 是否有該退費項目類別
        /// </summary>
        /// <param name="payclass">退費項目類別</param>
        /// <param name="action">動作</param>
        /// <returns></returns>
        bool CheckData(string payclass,string action);
    }
}
