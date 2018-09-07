using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class MarginpViewModel : ITreaItem
    {

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 類別
        /// </summary>
        [Description("類別")]
        public string vMarginp_Take_Of_Type { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Description("交易對象")]
        public string vMarginp_Trad_Partners { get; set; }

        /// <summary>
        /// 歸檔編號
        /// </summary>
        [Description("歸檔編號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public string  vMarginp_Amount { get; set; }

        /// <summary>
        /// 物品名稱
        /// </summary>
        [Description("物品名稱")]
        public string vMarginp_Item { get; set; }

        /// <summary>
        /// 物品發行人
        /// </summary>
        [Description("物品發行人")]
        public string vMarginp_Item_Issuer { get; set; }

        /// <summary>
        /// 質押標的號碼
        /// </summary>
        [Description("質押標的號碼")]
        public string vMarginp_Pledge_Item_No { get; set; }

        /// <summary>
        /// 有效期間(起)
        /// </summary>
        //[Description("有效期間(起)")]
        //public string vMarginp_Effective_Date_B { get; set; }

        /// <summary>
        /// 有效期間(起)日期
        /// </summary>
        [Description("有效期間(起)")]
        public String vMarginp_Effective_Date_B { get; set; }

        /// <summary>
        /// 有效期間(迄)
        /// </summary>
        //[Description("有效期間(迄)")]
        //public string vMarginp_Effective_Date_E { get; set; }

        /// <summary>
        /// 有效期間(迄)日期
        /// </summary>
        [Description("有效期間(迄)")]
        public String vMarginp_Effective_Date_E { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Description("說明")]
        public string vDescription { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 冊號
        /// </summary>
        [Description("冊號")]
        public string vMarginp_Book_No { get; set; }

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