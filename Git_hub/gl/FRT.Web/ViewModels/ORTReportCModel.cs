using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 已銷帳 / 未銷帳 總計參數 
    /// </summary>
    public class ORTReportCModel
    {
        /// <summary>
        /// AS400 總金額
        /// </summary>
        [Description("AS400 總金額")]
        public string AS400_AMT { get; set; }

        /// <summary>
        /// AS400 總筆數
        /// </summary>
        [Description("AS400 總筆數")]
        public string AS400_Count { get; set; }

        /// <summary>
        /// Wanpie 總金額
        /// </summary>
        [Description("Wanpie 總金額")]
        public string Wanpie_AMT { get; set; }

        /// <summary>
        /// Wanpie 總筆數
        /// </summary>
        [Description("Wanpie 總筆數")]
        public string Wanpie_Count { get; set; }

        /// <summary>
        /// 差異數 金額
        /// </summary>
        [Description("差異數 金額")]
        public string Diff_AMT { get; set; }

        /// <summary>
        /// 差異數 筆數
        /// </summary>
        [Description("差異數 筆數")]
        public string Diff_Count { get; set; }

        /// <summary>
        /// 比對結果
        /// </summary>
        [Description("比對結果")]
        public string Compare_Result { get; set; }

        /// <summary>
        /// 未銷帳截止日
        /// </summary>
        [Description("未銷帳截止日")]
        public string Deadline { get; set; }
    }
}