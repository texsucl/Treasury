using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 上傳股票檔案Model
    /// </summary>
    public class FileStockModel : IFileModel 
    {
        /// <summary>
        /// 物品編號
        /// </summary>
        [Description("物品編號")]
        public string ITEM_ID { get; set; }

        /// <summary>
        /// 物品名稱
        /// </summary>
        [Required]
        //[StringLength(30, ErrorMessage = "物品名稱,不得大於30個字元")]
        [Description("股票名稱")]
        public string STOCK_NAME { get; set; }

        /// <summary>
        /// 群組編號
        /// </summary>
        [Description("群組編號")]
        public string GROUP_NO { get; set; }

        /// <summary>
        /// 入庫批號
        /// </summary>
        [Required]
        [Description("入庫批號")]
        public string TREA_BATCH_NO { get; set; }

        /// <summary>
        /// 股票類型
        /// </summary>
        [Required]
        [Description("股票類型")]
        public string STOCK_TYPE { get; set; }

        /// <summary>
        /// 股票類型代碼
        /// </summary>
        [Description("股票類型代碼")]
        public string STOCK_TYPE_CODE { get; set; }

        /// <summary>
        /// 股票序號前置代碼
        /// </summary>
        [StringLength(7, ErrorMessage = "股票序號前置代碼,不得大於7個字元")]
        [Description("股票序號前置代碼")]
        public string STOCK_NO_PREAMBLE { get; set; }

        /// <summary>
        /// 股票序號(起)
        /// </summary>
        [StringLength(8, ErrorMessage = "股票序號(起),不得大於8個字元")]
        [RegularExpression(@"^([0-9]{1,})?$", ErrorMessage = "股票序號(起)需為數字。")]
        [Description("股票序號(起)")]
        public string STOCK_NO_B { get; set; }

        /// <summary>
        /// 股票序號(迄)
        /// </summary>
        [StringLength(8, ErrorMessage = "股票序號(迄),不得大於8個字元")]
        [RegularExpression(@"^([0-9]{1,})?$", ErrorMessage = "股票序號(迄)需為數字。")]
        [Description("股票序號(迄)")]
        public string STOCK_NO_E { get; set; }

        /// <summary>
        /// 股票張數
        /// </summary>    
        [RegularExpression(@"^([0-9]{1,})?$", ErrorMessage = "股票張數需為數字。")]
        [Description("股票張數")]
        public string STOCK_CNT { get; set; }

        /// <summary>
        /// 每股金額
        /// </summary>    
        [Description("每股金額")]
        [RegularExpression(@"^([0-9]{1,18})?$", ErrorMessage = "每股金額不符合整數或超過18位")]
        public string AMOUNT_PER_SHARE { get; set; }

        /// <summary>
        /// 單張股數
        /// </summary>    
        [Description("單張股數")]
        [RegularExpression(@"^([0-9]{1,18})?$", ErrorMessage = "單張股數不符合整數或超過18位")]
        public string SINGLE_NUMBER_OF_SHARES { get; set; }

        /// <summary>
        /// 面額
        /// </summary>    
        [Description("面額")]
        public string DENOMINATION { get; set; }

        /// <summary>
        /// 股數
        /// </summary>    
        [Description("股數")]
        public string NUMBER_OF_SHARES { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(200, ErrorMessage = "備註,不得大於200個字元")]
        [Description("備註")]       
        public string MEMO { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Required]
        [RegularExpression(@"^[0-9A-z]{5}$", ErrorMessage = "申請人需等於5個字元")]
        [Description("申請人")]
        public string APLY_UID { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string APLY_SHOW { get; set; }

        /// <summary>
        /// 申請科別
        /// </summary>
        [Description("申請科別")]
        public string APLY_SECT { get; set; }

        /// <summary>
        /// 申請部門
        /// </summary>
        [Description("申請部門")]
        public string APLY_DEPT { get; set; }

        /// <summary>
        /// 存入日期時間
        /// </summary>
        [Description("存入日期時間")]
        public string PUT_DATE { get; set; }
    }
}