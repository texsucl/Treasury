using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 已銷帳/未銷帳 差異明細表
    /// </summary>
    public class ORTReportCDetailModel
    {
        /// <summary>
        /// 交易序號 / 代收編號
        /// </summary>
        [Description("交易序號 / 代收編號")]
        public string NO { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        [Description("交易日期")]
        public string DATE { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURRENCY { get; set; }

        /// <summary>
        /// AS400金額
        /// </summary>
        [Description("AS400金額")]
        public string AS400_AMT { get; set; }

        /// <summary>
        /// Wanpie 金額
        /// </summary>
        [Description("Wanpie 金額")]
        public string Wanpie_AMT { get; set; }

        /// <summary>
        /// 差異金額
        /// </summary>
        [Description("差異金額")]
        public string Diff_AMT { get; set; }


    }
}