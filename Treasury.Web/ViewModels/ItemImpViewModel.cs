using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class ItemImpViewModel : ITreaItem
    {
        /// <summary>
        /// 歸檔編號(同物品單號)
        /// </summary>
        [Description("歸檔編號")]
        public string vShowItemId { get; set; }

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
        /// 重要物品名稱
        /// </summary>
        [Description("重要物品名稱")]
        public string vItemImp_Name { get; set; }

        /// <summary>
        /// 重要物品數量
        /// </summary>
        [Description("重要物品數量")]
        public int vItemImp_Quantity { get; set; }

        /// <summary>
        /// 重要物品剩餘數量
        /// </summary>
        [Description("重要物品剩餘數量")]
        public int vItemImp_Remaining { get; set; }

        /// <summary>
        /// 重要物品取出數量
        /// </summary>
        [Description("重要物品取出數量")]
        public int? vItemImp_G_Quantity { get; set; }

        /// <summary>
        /// 重要物品金額
        /// </summary>
        [Description("重要物品金額")]
        public decimal? vItemImp_Amount { get; set; }

        /// <summary>
        /// 重要物品預計提取日期
        /// </summary>
        [Description("重要物品預計提取日期")]
        public String vItemImp_Expected_Date { get; set; }

        ///// <summary>
        ///// 重要物品預計提取日期西元年
        ///// </summary>
        //[Description("重要物品預計提取日期")]
        //public String vItemImp_Expected_Date_2 { get; set; }

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