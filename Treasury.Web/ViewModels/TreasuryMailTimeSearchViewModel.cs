using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailTimeSearchViewModel : ITinItem
    {
        /// <summary>
        /// 程式名稱
        /// </summary>
        [Description("程式名稱")]
        public string vFunc_ID { get; set; }

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAPPR_STATUS { get; set; }

        /// <summary>
        /// 發送時間定義編號
        /// </summary>
        [Description("發送時間定義編號")]
        public string vMAIL_TIME_ID { get; set; }

        /// <summary>
        /// 申請單單號
        /// </summary>
        [Description("申請單單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        [Description("UserId")]
        public string userId { get; set; }
    }
}