using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailContentSearchViewModel : ITinItem
    {
        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 申請單單號
        /// </summary>
        [Description("申請單單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 使用者ID
        /// </summary>
        [Description("使用者ID")]
        public string UserId { get; set; }
    }
}