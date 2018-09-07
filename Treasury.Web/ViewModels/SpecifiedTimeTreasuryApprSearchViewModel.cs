using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class SpecifiedTimeTreasuryApprSearchViewModel
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
		/// 金庫登記簿單號
		/// </summary>
		[Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

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