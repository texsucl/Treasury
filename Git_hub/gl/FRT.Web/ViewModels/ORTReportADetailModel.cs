using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// AS400 檢核報表  差異明細表
    /// </summary>
    public class ORTReportADetailModel
    {
        /// <summary>
        /// 退費序號/匯款狀態
        /// </summary>
        [Description("退費序號/匯款狀態")]
        public string FEE_SEQ { get; set; }

        /// <summary>
        /// 匯款狀態
        /// </summary>
        [Description("匯款狀態")]
        public string Status { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURR { get; set; }

        /// <summary>
        /// 匯款日期(左)
        /// </summary>
        [Description("匯款日期(左)")]
        public string Date_L { get; set; }

        /// <summary>
        /// 異動日期(左)/比對檔名(左)
        /// </summary>
        [Description("異動日期(左)/比對檔名(左)")]
        public string UPDDate_L { get; set; }

        /// <summary>
        /// 金額(左)
        /// </summary>
        [Description("金額(左)")]
        public string Amt_L { get; set; }

        /// <summary>
        /// 金額(左) decimal
        /// </summary>
        [Description("金額(左) decimal")]
        public decimal? Amt_L_D { get; set; }

        /// <summary>
        /// 匯款日期(右)
        /// </summary>
        [Description("匯款日期(右)")]
        public string Date_R { get; set; }

        /// <summary>
        /// 異動日期(右)/比對檔名(右)
        /// </summary>
        [Description("異動日期(右)/比對檔名(右)")]
        public string UPDDate_R { get; set; }

        /// <summary>
        /// 金額(右)
        /// </summary>
        [Description("金額(右)")]
        public string Amt_R { get; set; }

        /// <summary>
        /// 金額(右) decimal
        /// </summary>
        [Description("金額(右) decimal")]
        public decimal? Amt_R_D { get; set; }

        /// <summary>
        /// 處理序號
        /// </summary>
        [Description("處理序號")]
        public string PRO_NO { get; set; }

        /// <summary>
        /// 框線版型
        /// </summary>
        [Description("框線版型")]
        public string BorderType { get; set; }

        /// <summary>
        /// 位置類型
        /// </summary>
        [Description("位置類型")]
        public string PositionType { get; set; }
    }
}