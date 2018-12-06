using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 上傳存入保證金檔案Model
    /// </summary>
    public class FileMarginpModel : IFileModel 
    {
        /// <summary>
        /// 歸檔編號
        /// </summary>
        [Description("歸檔編號")]
        public string ITEM_ID { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Required(ErrorMessage = "TRAD_PARTNERS(交易對象) 欄位是必要項。")]
        [StringLength(30, ErrorMessage = "交易對象欄位,不得大於30個字元。")]
        [Description("交易對象")]
        public string TRAD_PARTNERS { get; set; }

        /// <summary>
        /// 存入保證金類別
        /// </summary>
        [Required(ErrorMessage = "MARGIN_TAKE_OF_TYPE(存入保證金類別) 欄位是必要項。")]
        [Description("存入保證金類別")]
        public string MARGIN_TAKE_OF_TYPE { get; set; }

        /// <summary>
        /// 存入保證金類別代碼
        /// </summary>       
        [Description("存入保證金類別代碼")]
        public string MARGIN_TAKE_OF_TYPE_CODE { get; set; }

        /// <summary>
        /// 保證物品
        /// </summary>
        [Required(ErrorMessage = "MARGIN_ITEM(保證物品) 欄位是必要項。")]
        [Description("保證物品")]
        public string MARGIN_ITEM { get; set; }

        /// <summary>
        /// 保證物品代碼
        /// </summary>
        [Description("保證物品代碼")]
        public string MARGIN_ITEM_CODE { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Required]
        [RegularExpression(@"^([0-9]{1,18})?$", ErrorMessage = "金額不符合整數或超過18位")]
        [Description("金額")]
        public string AMOUNT { get; set; }

        /// <summary>
        /// 物品發行人
        /// </summary>
        [Required(ErrorMessage = "MARGIN_ITEM_ISSUER(物品發行人) 欄位是必要項。")]
        [StringLength(10, ErrorMessage = "物品發行人,不得大於10個字元。")]
        [Description("物品發行人")]
        public string MARGIN_ITEM_ISSUER { get; set; }

        /// <summary>
        /// 質押標的號碼
        /// </summary>
        [Required(ErrorMessage = "PLEDGE_ITEM_NO(質押標的號碼) 欄位是必要項。")]
        [StringLength(16, ErrorMessage = "質押標的號碼,不得大於16個字元")]
        [Description("質押標的號碼")]
        public string PLEDGE_ITEM_NO { get; set; }

        /// <summary>
        /// 有效區間(起)
        /// </summary>
        [Description("有效區間(起)")]
        public string EFFECTIVE_DATE_B { get; set; }

        /// <summary>
        /// 有效區間(迄)
        /// </summary>
        [Description("有效區間(迄)")]
        public string EFFECTIVE_DATE_E { get; set; }

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
        /// 冊號
        /// </summary>
        [Required(ErrorMessage = "BOOK_NO(冊號) 欄位是必要項。")]
        [StringLength(5, ErrorMessage = "冊號,不得大於5個字元")]
        [Description("冊號")]
        public string BOOK_NO { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        [Required(ErrorMessage = "APLY_UID(申請人) 欄位是必要項。")]
        [RegularExpression(@"^[0-9A-z]{5}$", ErrorMessage = "申請人ID須符合英文+數字5碼。")]
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