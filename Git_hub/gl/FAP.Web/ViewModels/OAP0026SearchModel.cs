using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0026SearchModel
    {
        /// <summary>
        /// 異動日期(起)
        /// </summary>
        [Description("異動日期(起)")]
        public string update_time_start { get; set; }

        /// <summary>
        /// 異動日期(迄)
        /// </summary>
        [Description("異動日期(迄)")]
        public string update_time_end { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 部門代碼
        /// </summary>
        [Description("部門代碼")]
        public string unit_code { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型")]
        public string ap_paid { get; set; }

        /// <summary>
        /// 目前使用者
        /// </summary>
        [Description("目前使用者")]
        public string current_uid { get; set; }
    }
}