using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 跨系統勾稽報表
    /// </summary>
    public class ORT0105AP2Model
    {
        /// <summary>
        /// 匯費序號
        /// </summary>
        [Description("匯費序號")]
        public string FEE_SEQ { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURR { get; set; }

        /// <summary>
        /// 基準日期/匯款日期/匯出日期
        /// </summary>
        [Description("基準日期/匯款日期/匯出日期")]
        public string REMIT_DATE { get; set; }

        /// <summary>
        /// 異動日期/表單日期
        /// </summary>
        [Description("異動日期/表單日期")]
        public string UPD_DATE { get; set; }

        /// <summary>
        /// 退匯日期
        /// </summary>
        [Description("退匯日期")]
        public string FAIL_DATE { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public decimal? AMT { get; set; }
    }
}