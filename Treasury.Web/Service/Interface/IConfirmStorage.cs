using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface IConfirmStorage
    {
        /// <summary>
        /// 初始資資料
        /// </summary>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, string, List<string>> GetFirstTimeData(string cUserId);

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<ConfirmStorageSearchDetailViewModel> GetSearchDetail(ConfirmStorageSearchViewModel data);

        /// <summary>
        /// 查作業類型 & 印鑑內容下拉選單
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Tuple<string, List<SelectOption>> GetItemOpType(string data, string AccessType, List<string> SealIdList, string ItemId = null);

        /// <summary>
        /// 查作業類型 & 印鑑內容下拉選單
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>> ItemOpTypeChange(string data, List<string> ItemIdList, string AccessType, List<string> SealIdList, List<string> RowItemIdList, string ItemId = null);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> InsertData(ConfirmStorageInsertViewModel data, ConfirmStorageSearchViewModel searchData);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> UpdateData(ConfirmStorageInsertViewModel data, ConfirmStorageSearchViewModel searchData);
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> DeteleData(ConfirmStorageDeleteViewModel data, ConfirmStorageSearchViewModel searchData);

        /// <summary>
        /// 確認入庫
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchData"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        MSGReturnModel<List<ConfirmStorageSearchDetailViewModel>> ConfirmData(List<string> data, ConfirmStorageSearchViewModel searchData, List<ConfirmStorageSearchDetailViewModel> viewData, string cUserId,string register_ID);
        
    }
}
