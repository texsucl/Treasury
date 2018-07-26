using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class CAViewModel : ITreaItem
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 電子憑證用途
        /// </summary>
        [Description("電子憑證用途")]
        public string vCA_Use { get; set; }

        /// <summary>
        /// 電子憑證品項
        /// </summary>
        [Description("電子憑證品項")]
        public string vCA_Desc { get; set; }

        /// <summary>
        /// 銀行/廠商
        /// </summary>
        [Description("銀行/廠商")]
        public string vBank { get; set; }

        /// <summary>
        /// 電子憑證號碼
        /// </summary>
        [Description("電子憑證號碼")]
        public string vCA_Number { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 取出註記
        /// </summary>
        [Description("取出註記")]
        public bool vtakeoutFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}