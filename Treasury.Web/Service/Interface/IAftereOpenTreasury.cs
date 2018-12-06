using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface IAftereOpenTreasury
    {
        /// <summary>
        /// 初始資料
        /// </summary>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>> GetFristTimeDatas();

        /// <summary>
        /// Dialog Selected Change 事件
        /// </summary>
        /// <param name="ItemOpType"></param>
        /// <param name="TreaItem"></param>
        /// <param name="AccessType"></param>
        /// <returns></returns>
        Tuple<List<SelectOption>, List<SelectOption>, List<SelectOption>, List<SelectOption>> DialogSelectedChange(string ItemOpType, string TreaItem, string AccessType, List<AfterOpenTreasurySearchDetailViewModel> ViewDatas);

        /// <summary>
        /// 修改時 作業類型 2 data.vSEAL_ITEM_ID 無值 必須查DB 帶出印章
        /// </summary>
        /// <param name="TreaItem"></param>
        /// <returns></returns>
        List<SelectOption> GetSealFun(string TreaItem, List<AfterOpenTreasurySearchDetailViewModel> ViewDatas);

        /// <summary>
        /// 產生實際入庫人員選單
        /// </summary>
        /// <param name="treaItemId"></param>
        /// <returns></returns>
        List<SelectOption> GetActualUserOption(string treaItemId, List<AfterOpenTreasurySearchDetailViewModel> ViewDatas);

        /// <summary>
        /// 產生實際作業別下拉選單
        /// </summary>
        /// <returns></returns>
        List<SelectOption> GetActualAccessTypeOption(string SEAL_ID);

        Tuple<string, string> GetConfrimedTime(string RegisterNo);

        /// <summary>
        /// 查詢未確認表單資料
        /// </summary>
        /// <returns></returns>
        List<AfterOpenTreasuryUnconfirmedDetailViewModel> GetUnconfirmedDetail();

        /// <summary>
        /// 查詢
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<AfterOpenTreasurySearchDetailViewModel> GetSearchDetail(AfterOpenTreasurySearchViewModel searchData);

        /// <summary>
        /// 申請覆核
        /// </summary>
        /// <param name="registerID"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> ApplyData(string registerID, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId);

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="InsertModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> InsertData(AfterOpenTreasuryInsertViewModel InsertModel, AfterOpenTreasurySearchViewModel searchData, string cUserId);

        /// <summary>
        /// 新增至金庫登記簿
        /// </summary>
        /// <param name="InsertModel"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> InsertUnconfirmedDetail(string RegisterID, List<AfterOpenTreasuryUnconfirmedDetailViewModel> InsertModel, string cUserId, AfterOpenTreasurySearchViewModel SearchData);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="searchData"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
       // MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> UpdateData(string APLYNO, string ActualAccEmp, string ActualAccType, string InsertReason, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId, string ItemId, string OpType);
        MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> UpdateData(string APLYNO, AfterOpenTreasuryInsertViewModel InsertModel, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId);

        /// <summary>
        /// 未確認修改
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="ActualAccEmp"></param>
        /// <param name="ActualAccType"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> UnConfirmedUpdateDatas(string APLYNO, string ActualAccEmp, List<AfterOpenTreasuryUnconfirmedDetailViewModel> viewModels, string cUserId);
        
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="deleteModel"></param>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> DeleteData(string APLYNO, AfterOpenTreasurySearchViewModel searchData, List<AfterOpenTreasurySearchDetailViewModel> viewModels, string cUserId, bool custodyFlag);

        /// <summary>
        /// 未確認表單刪除
        /// </summary>
        /// <param name="APLYNO"></param>
        /// <param name="viewModels"></param>
        /// <param name="cUserId"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasuryUnconfirmedDetailViewModel>> UnconfirmedDeleteData(string APLYNO, List<AfterOpenTreasuryUnconfirmedDetailViewModel> viewModels, string cUserId);

        /// <summary>
        /// 確定存檔
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        MSGReturnModel<List<AfterOpenTreasurySearchDetailViewModel>> ConfrimedData(AfterOpenTreasurySearchViewModel searchData, string cUserId);
    }
}
