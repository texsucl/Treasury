using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ITreasuryAccess 
    {
        /// <summary>
        /// 金庫進出管理作業-金庫物品存取申請作業 初始畫面顯示
        /// </summary>
        /// <param name="cUserID">userId</param>
        /// <param name="custodyFlag">管理科Flag</param>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>,string,string> TreasuryAccessDetail(string cUserID, bool custodyFlag);

        /// <summary>
        /// 申請單位 變更時 變更申請人
        /// </summary>
        /// <param name="DPT_CD"></param>
        /// <returns></returns>
        List<SelectOption> ChangeUnit(string DPT_CD);

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data">畫面資料</param>
        /// <returns></returns>

        List<TreasuryAccessSearchDetailViewModel> GetSearchDetail(TreasuryAccessSearchViewModel data);

        /// <summary>
        /// 取消申請
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Cancel(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data);

        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        MSGReturnModel<List<TreasuryAccessSearchDetailViewModel>> Invalidate(TreasuryAccessSearchViewModel searchData, TreasuryAccessSearchDetailViewModel data);

        /// <summary>
        /// 查詢申請單紀錄資料by單號
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        TreasuryAccessViewModel GetByAplyNo(string aplyNo);

        /// <summary>
        /// 取得 存入or取出
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        string GetAccessType(string aplyNo);

        /// <summary>
        /// 取得是否可以修改狀態
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <param name="uid"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        bool GetActType(string aplyNo, string uid, List<string> actionType);

        /// <summary>
        /// 取得單號狀態
        /// </summary>
        /// <param name="aplyNo"></param>
        /// <returns></returns>
        string GetStatus(string aplyNo);

        /// <summary>
        /// 使用單號抓取 申請表單資料
        /// </summary>
        /// <param name="aplyNo">單號</param>
        /// <returns></returns>
        TreasuryAccessViewModel GetTreasuryAccessViewModel(string aplyNo);
    }
}
