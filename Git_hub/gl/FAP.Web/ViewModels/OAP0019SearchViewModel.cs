using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0019SearchViewModel
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
        /// 覆核單位代碼
        /// </summary>
        [Description("覆核單位代碼")]
        public string appr_unit { get; set; }

        /// <summary>
        /// 承辦單位代碼
        /// </summary>
        [Description("承辦單位代碼")]
        public string user_unit { get; set; }
    }
}