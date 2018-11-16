using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    /// <summary>
    /// 金庫進出管理作業-mail發送內文設定檔維護作業
    /// </summary>
    public interface ITreasuryMailContent : ITinAction
    {
        /// <summary>
        /// 查詢內文編號
        /// </summary>
        /// <param name="allFlag"></param>
        /// <param name="disabledFlag"></param>
        /// <returns></returns>
        List<SelectOption> Get_MAIL_ID(bool allFlag = true, bool disabledFlag = false);

        /// <summary>
        /// 查詢新增的功能編號
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetFUNC_ID(List<string> func_Ids);

        /// <summary>
        /// 查詢資料By內文編號
        /// </summary>
        /// <param name="MAIL_CONTENT_ID"></param>
        /// <returns></returns>
        ITinItem GetUpdateData(string MAIL_CONTENT_ID);

        /// <summary>
        /// 查詢 mail發送對象設定檔
        /// </summary>
        /// <param name="MAIL_CONTENT_ID"></param>
        /// <param name="aply_No"></param>
        /// <returns></returns>
        List<TreasuryMailReceivelViewModel> GetReceiveData(string MAIL_CONTENT_ID, string aply_No = null);
    }
}
