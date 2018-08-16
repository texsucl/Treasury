using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class MargingpViewModel : ITreaItem
    {
        /// <summary>
        /// 網頁PK
        /// </summary>
        [Description("網頁PK")]
        public string vItem_PK { get; set; }

        /// <summary>
        /// 歸檔編號
        /// </summary>
        [Description("歸檔編號")]
        public string vItem_Id { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Description("交易對象")]
        public string vTrad_Partners { get; set; }

        /// <summary>
        /// 存出保證金類別
        /// </summary>
        [Description("存出保證金類別")]
        public string vMargin_Dep_Type { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public decimal? vAmount { get; set; }

        /// <summary>
        /// 職場代號
        /// </summary>
        [Description("職場代號")]
        public string vWorkplace_Code { get; set; }

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
        public string vBook_No { get; set; }

        /// <summary>
        /// 取出註記
        /// </summary>
        [Description("取出註記")]
        public bool vTakeoutFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}