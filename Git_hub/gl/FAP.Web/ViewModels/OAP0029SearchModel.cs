using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0029SearchModel
    {
        /// <summary>
        /// 回覆日期
        /// </summary>
        [Description("回覆日期")]
        public string ce_rply_dt { get; set; }

        /// <summary>
        /// 寄送方式
        /// </summary>
        [Description("寄送方式")]
        public string send_style { get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string ce_result_status { get; set; }

        /// <summary>
        /// 抽件人員
        /// </summary>
        [Description("抽件人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

    }
}