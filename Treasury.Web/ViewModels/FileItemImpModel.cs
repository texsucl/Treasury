using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 上傳重要物品檔案Model
    /// </summary>
    public class FileItemImpModel : IFileModel 
    {
        /// <summary>
        /// 歸檔編號
        /// </summary>
        [Description("歸檔編號")]
        public string ITEM_ID { get; set; }

        /// <summary>
        /// 物品名稱
        /// </summary>
        [Required]
        [StringLength(30, ErrorMessage = "物品名稱,不得大於30個字元")]
        [Description("物品名稱")]
        public string ITEM_NAME { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "請輸入數字")]
        [Description("數量")]
        public string QUANTITY { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [RegularExpression(@"^([0-9]{1,18})?$", ErrorMessage = "金額不符合整數或超過18位")]
        [Description("金額")]
        public string AMOUNT { get; set; }

        /// <summary>
        /// 預計存取日期
        /// </summary>
        [Description("預計存取日期")]
        public string EXPECTED_ACCESS_DATE { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [StringLength(100, ErrorMessage = "說明,不得大於100個字元")]
        [Description("說明")]
        public string DESCRIPTION { get; set; }

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