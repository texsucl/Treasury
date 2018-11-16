﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Treasury.Web.ViewModels;
using Treasury.WebUtility;

namespace Treasury.Web.Service.Interface
{
    public interface ICDC
    {
        #region Get
        /// <summary>
        /// 
        /// </summary>
        CDCViewModel GetItemId();

        /// <summary>
        /// 查詢資料異動作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<CDCApprSearchDetailViewModel> GetApprSearchDetail(CDCApprSearchViewModel data);

        /// <summary>
        /// 查詢權責異動作業 覆核查詢 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        List<CDCApprSearchDetailViewModel> GetChargeApprSearchDetail(CDCApprSearchViewModel data);
        #endregion

        #region Save

        /// <summary>
        /// 權責單位調整申請
        /// </summary>
        /// <param name="data"></param>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        MSGReturnModel<CDCChargeViewModel> ChargeAppr(CDCChargeViewModel data, CDCSearchViewModel searchModel);

        /// <summary>
        /// 權責覆核畫面覆核
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        MSGReturnModel<List<CDCApprSearchDetailViewModel>> ChargeApproved(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels);

        /// <summary>
        /// 權責覆核畫面駁回
        /// </summary>
        /// <param name="searchData"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        MSGReturnModel<List<CDCApprSearchDetailViewModel>> ChargeReject(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels);

        /// <summary>
        /// 覆核畫面覆核
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <returns></returns>
        MSGReturnModel<List<CDCApprSearchDetailViewModel>> Approved(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels);

        /// <summary>
        /// 覆核畫面駁回
        /// </summary>
        /// <param name="searchData">資料異動覆核畫面查詢ViewModel</param>
        /// <param name="viewModels">覆核表單查詢顯示區塊ViewModel</param>
        /// <param name="apprDesc">駁回意見</param>
        /// <returns></returns>
        MSGReturnModel<List<CDCApprSearchDetailViewModel>> Reject(CDCApprSearchViewModel searchData, List<CDCApprSearchDetailViewModel> viewModels, string apprDesc);

        #endregion

    }
}
