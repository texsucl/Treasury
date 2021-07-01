using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// AS400_匯款件檢核報表
    /// </summary>
    public class ORT0105AP1Model
    {
        /// <summary>
        /// 檔案
        /// </summary>
        [Description("檔案")]
        public string FileName { get; set; }

        /// <summary>
        /// 比對日期
        /// </summary>
        [Description("比對日期")]
        public string DATE { get; set; }

        /// <summary>
        /// 匯費序號
        /// </summary>
        [Description("匯費序號")]
        public string FEE_SEQ { get; set; }

        /// <summary>
        /// 處理序號
        /// </summary>
        [Description("處理序號")]
        public string PRO_NO { get; set; }

        /// <summary>
        /// 系統別
        /// </summary>
        [Description("系統別")]
        public string SYS { get; set; }

        /// <summary>
        /// 匯款狀態
        /// </summary>
        [Description("匯款狀態")]
        public string Status { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public decimal? AMT { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURR { get; set; }

        /// <summary>
        /// 退匯日期
        /// </summary>
        [Description("退匯日期")]
        public string UPD_DATE { get; set; }
    }
}