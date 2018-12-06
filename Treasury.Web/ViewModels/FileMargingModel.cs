using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 上傳存出保證金檔案Model
    /// </summary>
    public class FileMargingModel : IFileModel 
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
        /// 存出保證金類別
        /// </summary>
        [Required(ErrorMessage = "MARGIN_DEP_TYPE(存出保證金類別) 欄位是必要項。")]
        [Description("存出保證金類別")]
        public string MARGIN_DEP_TYPE { get; set; }

        /// <summary>
        /// 存出保證金類別代碼
        /// </summary>       
        [Description("存出保證金類別代碼")]
        public string MARGIN_DEP_TYPE_CODE { get; set; }

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
        [StringLength(7, ErrorMessage = "職場代號,不得大於7個字元。")]
        [Description("職場代號")]
        public string WORKPLACE_CODE { get; set; }

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