using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// AS400 檢核報表 
    /// </summary>
    public class ORTReportAModel
    {
        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string CURR { get; set; }

        /// <summary>
        /// 檔案別
        /// </summary>
        [Description("檔案別")]
        public string FileName { get; set; }

        /// <summary>
        /// 項目
        /// </summary>
        [Description("項目")]
        public string Item { get; set; }

        /// <summary>
        /// A&F系統項目
        /// </summary>
        [Description("A&F系統項目")]
        public string AF_count { get; set; }

        /// <summary>
        /// A&F系統金額
        /// </summary>
        [Description("A&F系統金額")]
        public string AF_amt { get; set; }

        /// <summary>
        /// A系統項目
        /// </summary>
        [Description("A系統項目")]
        public string A_count { get; set; }

        /// <summary>
        /// A系統金額
        /// </summary>
        [Description("A系統金額")]
        public string A_amt { get; set; }

        /// <summary>
        /// F系統項目
        /// </summary>
        [Description("F系統項目")]
        public string F_count { get; set; }

        /// <summary>
        /// F系統金額
        /// </summary>
        [Description("F系統金額")]
        public string F_amt { get; set; }

        /// <summary>
        /// 邊框類型
        /// </summary>
        [Description("邊框類型")]
        public string BorderType { get; set; }

        /// <summary>
        /// 顏色類型
        /// </summary>
        [Description("顏色類型")]
        public string ColorType { get; set; }

        /// <summary>
        /// 位置類型
        /// </summary>
        [Description("位置類型")]
        public string PositionType { get; set; }
    }
}