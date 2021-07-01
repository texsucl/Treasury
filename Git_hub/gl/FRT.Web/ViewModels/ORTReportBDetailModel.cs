using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// Wanpie 檢核報表 差異明細表
    /// </summary>
    public class ORTReportBDetailModel
    {
        /// <summary>
        /// 退費序號
        /// </summary>
        [Description("退費序號")]
        public string FEE_SEQ { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURR { get; set; }

        /// <summary>
        /// 左邊日期1 (匯款)
        /// </summary>
        [Description("左邊日期1 (匯款)")]
        public string Date_1_L { get; set; }

        /// <summary>
        /// 左邊日期2 (異動/退匯)
        /// </summary>
        [Description("左邊日期2 (異動/退匯)")]
        public string Date_2_L { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public string AMT_L { get; set; }

        /// <summary>
        /// 右邊日期1 (匯出/匯款)
        /// </summary>
        [Description("右邊日期1 (匯出/匯款)")]
        public string Date_1_R { get; set; }

        /// <summary>
        /// 右邊日期2 (退匯)
        /// </summary>
        [Description("右邊日期2 (退匯)")]
        public string Date_2_R { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public string AMT_R { get; set; }

        /// <summary>
        /// 邊框類型
        /// </summary>
        [Description("邊框類型")]
        public string BorderType { get; set; }

        /// <summary>
        /// 位置類型
        /// </summary>
        [Description("位置類型")]
        public string PositionType { get; set; }
    }
}