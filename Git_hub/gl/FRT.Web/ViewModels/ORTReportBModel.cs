using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// Wanpie 檢核報表
    /// </summary>
    public class ORTReportBModel
    {
        /// <summary>
        /// 明細內容
        /// </summary>
        [Description("明細內容")]
        public string Item { get; set; }

        /// <summary>
        /// AS400檔案
        /// </summary>
        [Description("AS400檔案)")]
        public string AS400_File { get; set; }

        /// <summary>
        /// Wanpie匯款檔
        /// </summary>
        [Description("Wanpie檔案")]
        public string Wanpie_File { get; set; }

        /// <summary>
        /// 差異件數
        /// </summary>
        [Description("差異件數")]
        public string Diff_value { get; set; }

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