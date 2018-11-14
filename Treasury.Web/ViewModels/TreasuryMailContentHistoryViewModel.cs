using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailContentHistoryViewModel : ITinItem
    {
        /// <summary>
        /// 申請單單號
        /// </summary>
        [Description("申請單單號")]
        public string APLY_NO { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string APLY_UID { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string APLY_DT { get; set; }

        /// <summary>
        /// 發送主旨(修改前)
        /// </summary>
        [Description("發送主旨(修改前)")]
        public string vMAIL_SUBJECT_B { get; set; }

        /// <summary>
        /// 發送主旨(修改後)
        /// </summary>
        [Description("發送主旨(修改後)")]
        public string vMAIL_SUBJECT { get; set; }

        /// <summary>
        /// 發送內文(修改前)
        /// </summary>
        [Description("發送內文(修改前)")]
        public string vMAIL_CONTENT_B { get; set; }

        /// <summary>
        /// 發送內文(修改後)
        /// </summary>
        [Description("發送內文(修改後)")]
        public string vMAIL_CONTENT { get; set; }

        /// <summary>
        /// 停用註記(修改前)
        /// </summary>
        [Description("停用註記(修改前)")]
        public string vIS_DISABLED_B { get; set; }

        /// <summary>
        /// 停用註記(修改後)
        /// </summary>
        [Description("停用註記(修改後)")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 對應功能明細
        /// </summary>
        [Description("對應功能明細")]
        public List<TreasuryMailReceivelViewModel> subData { get; set; }

        /// <summary>
        /// 對應功能註記
        /// </summary>
        [Description("對應功能註記")]
        public string FunFlag {get;set;}

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAPPR_STATUS { get; set; }

        /// <summary>
        /// 覆核意見
        /// </summary>
        [Description("覆核意見")]
        public string vAPPR_DESC { get; set; }
    }
}