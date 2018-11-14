using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailContentHistorySearchViewModel : ITinItem
    {
        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 覆核結果
        /// </summary>
        [Description("覆核結果")]
        public string vAPPR_STATUS { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAply_Date { get; set; }

        /// <summary>
        /// 使用者ID
        /// </summary>
        [Description("使用者ID")]
        public string UserId { get; set; }
    }
}