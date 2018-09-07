using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class SpecifiedTimeTreasuryApprSearchDetailViewModel
    {
        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vCREATE_DT { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
		/// 申請人編號
		/// </summary>
		[Description("申請人編號")]
        public string vCREATE_UID { get; set; }

        /// <summary>
		/// 申請人
		/// </summary>
		[Description("申請人")]
        public string vCREATE_NAME { get; set; }

        /// <summary>
        /// 入庫日期
        /// </summary>
        [Description("入庫日期")]
        public string vOPEN_TREA_DATE { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMEMO { get; set; }

        /// <summary>
        /// 系統區間(起)
        /// </summary>
        [Description("系統區間(起)")]
        public string vEXEC_TIME_B { get; set; }

        /// <summary>
        /// 系統區間(迄)
        /// </summary>
        [Description("系統區間(迄)")]
        public string vEXEC_TIME_E { get; set; }

        /// <summary>
        /// 開庫時間
        /// </summary>
        [Description("開庫時間")]
        public string vOPEN_TREA_TIME { get; set; }

        /// <summary>
        /// 工作類型編號
        /// </summary>
        [Description("工作類型編號")]
        public string vOPEN_TREA_REASON_ID { get; set; }

        /// <summary>
        /// 最後修改時間
        /// </summary>
         [Description("最後修改時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool vAPPR_FLAG{ get; set; }
}
}