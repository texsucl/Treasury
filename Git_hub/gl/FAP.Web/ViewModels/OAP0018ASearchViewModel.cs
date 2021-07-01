using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0018ASearchViewModel
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
        /// 目前使用者
        /// </summary>
        [Description("目前使用者")]
        public string current_uid { get; set; }
    }
}