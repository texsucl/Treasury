using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class StockViewModel : ITreaItem
    {
        /// <summary>
        /// 股票資料
        /// </summary>
        [Description("股票資料")]
        public ItemBookStock vStockDate { get; set; }

        /// <summary>
        /// 股票模型
        /// </summary>
        [Description("股票模型")]
        public ItemBookStockModel vStockModel { get; set; }

        /// <summary>
        /// 股票庫存資料檔
        /// </summary>
        [Description("股票庫存資料檔")]
        public List<StockDetailViewModel> vDetail { get; set; }
    }

    public class ItemBookStock
    {
        /// <summary>
        /// 存入資料
        /// </summary>
        [Description("存入資料")]
        public string StockFeaturesType { get; set; }

        /// <summary>
        /// 群組編號
        /// </summary>
        [Description("群組編號")]
        public int GroupNo { get; set; }

        /// <summary>
        /// 股票名稱
        /// </summary>
        [Description("股票名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 區域
        /// </summary>
        [Description("區域")]
        public string Area { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string Memo { get; set; }

        /// <summary>
        /// 下一次入庫批號
        /// </summary>
        [Description("下一次入庫批號")]
        public string Next_Batch_No { get; set; }
    }

    public class ItemBookStockModel
    {
        /// <summary>
        /// 區域
        /// </summary>
        [Description("區域")]
        public string AREA { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string MEMO { get; set; }

        /// <summary>
        /// 股票名稱
        /// </summary>
        [Description("股票名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 下一次入庫批號
        /// </summary>
        [Description("下一次入庫批號")]
        public string NEXT_BATCH_NO { get; set; }
    }

    public class StockDetailViewModel
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
        /// 取出註記
        /// </summary>
        [Description("取出註記")]
        public bool vTakeoutFlag { get; set; }

        /// <summary>
        /// 入庫批號
        /// </summary>
        [Description("入庫批號")]
        public int vTreaBatchNo { get; set; }

        /// <summary>
        /// 類型
        /// </summary>
        [Description("類型")]
        public string vStockType { get; set; }

        /// <summary>
        /// 序號前置碼
        /// </summary>
        [Description("序號前置碼")]
        public string vStockNoPreamble { get; set; }

        /// <summary>
        /// 序號(起)
        /// </summary>
        [Description("序號(起)")]
        public string vStockNoB { get; set; }

        /// <summary>
        /// 序號(迄)
        /// </summary>
        [Description("序號(迄)")]
        public string vStockNoE { get; set; }

        /// <summary>
        /// 張數
        /// </summary>
        [Description("張數")]
        public int? vStockTotal { get; set; }

        /// <summary>
        /// 每股金額
        /// </summary>
        [Description("每股金額")]
        public decimal? vAmount_Per_Share { get; set; }

        /// <summary>
        /// 單張股數
        /// </summary>
        [Description("單張股數")]
        public decimal? vSingle_Number_Of_Shares { get; set; }

        /// <summary>
        /// 單張面額
        /// </summary>
        [Description("單張面額")]
        public decimal? vDenomination { get; set; }

        /// <summary>
        /// 面額小計
        /// </summary>
        [Description("面額小計")]
        public decimal? vDenominationTotal { get; set; }

        /// <summary>
        /// 股數小計
        /// </summary>
        [Description("股數小計")]
        public decimal? vNumberOfShares { get; set; }

        /// <summary>
        /// 備註說明
        /// </summary>
        [Description("備註說明")]
        public string vMemo { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Description("申請日期")]
        public string vAplyDate { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Description("申請人")]
        public string vAplyName { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }

        /// <summary>
        /// 總股數
        /// </summary>
        [Description("總股數")]
        public int vNumberOfSharesTotal { get; set; }
    }
}