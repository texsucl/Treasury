using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 金庫物品覆核畫面查詢ViewModel
    /// </summary>
    public class TreasuryAccessApprSearchViewModel
    {
        /// <summary>
        /// 申請日期(開始)
        /// </summary>
        [Description("申請日期(開始)")]
        public string vAPLY_DT_S { get; set; }

        /// <summary>
        /// 申請日期(結束)
        /// </summary>
        [Description("申請日期(結束)")]
        public string vAPLY_DT_E { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]

        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 填表人員
        /// </summary>
        [Description("填表人員")]
        public string vCreateUid { get; set; }

        /// <summary>
        /// 填表單位
        /// </summary>
        [Description("填表單位")]
        public string vCreateUnit { get; set; }
    }
}