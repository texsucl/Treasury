using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 上傳不動產檔案Model
    /// </summary>
    public class FileEstateModel : IFileModel 
    {
        /// <summary>
        /// 群組編號
        /// </summary>
        //[Required]
        //[RegularExpression(@"^[0-9]+$", ErrorMessage = "請輸入數字")]
        [Description("群組編號")]
        public string GROUP_NO { get; set; }

        /// <summary>
        /// 狀別
        /// </summary>
        [Required]
        [StringLength(8, ErrorMessage = "狀別,不得大於8個字元")]
        [Description("狀別")]
        public string ESTATE_FORM_NO { get; set; }

        /// <summary>
        /// 發狀日
        /// </summary>
        [Required]
        [Description("發狀日")]
        public string ESTATE_DATE { get; set; }

        private string _ESTATE_DATE_AD = string.Empty;

        /// <summary>
        /// 發狀日(西元)
        /// </summary>
        [Description("發狀日(西元)")]
        public string ESTATE_DATE_AD { get {
                return _ESTATE_DATE_AD;
            }
            private set {
                if (!this.ESTATE_DATE.IsNullOrWhiteSpace())
                {
                    DateTime? dt = TypeTransfer.stringToADDateTimeN(ESTATE_DATE);
                    if (dt != null)
                        _ESTATE_DATE_AD = TypeTransfer.dateTimeNToString(dt);
                }
            } }

        /// <summary>
        /// 字號
        /// </summary>
        [StringLength(16, ErrorMessage = "字號,不得大於16個字元")]
        [Description("字號")]
        public string OWNERSHIP_CERT_NO { get; set;}

        /// <summary>
        /// 地/建號
        /// </summary>
        [StringLength(10, ErrorMessage = "地/建號,不得大於10個字元")]
        [Description("地/建號")]
        public string LAND_BUILDING_NO { get; set; }

        /// <summary>
        /// 門牌號
        /// </summary>
        [StringLength(30, ErrorMessage = "門牌號,不得大於30個字元")]
        [Description("門牌號")]
        public string HOUSE_NO {get;set;}

        /// <summary>
        /// 流水號/編號
        /// </summary>
        [StringLength(16, ErrorMessage = "流水號/編號,不得大於16個字元")]
        [Description("流水號/編號")]
        public string ESTATE_SEQ { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(200, ErrorMessage = "備註,不得大於200個字元")]
        [Description("備註")]       
        public string MEMO { get; set; }

        /// <summary>
        /// 申請人
        /// </summary>
        //[Required]
        //[RegularExpression(@"^[0-9A-z]{5}$", ErrorMessage = "申請人需等於5個字元")]
        [Description("申請人")]
        public string APLY_UID { get; set; }
    }
}