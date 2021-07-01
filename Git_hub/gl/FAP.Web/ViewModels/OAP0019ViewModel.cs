using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0019ViewModel
    {
        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能代碼")]
        public string exec_action { get; set; }

        /// <summary>
        /// 執行功能名稱
        /// </summary>
        [Description("執行功能")]
        public string exec_action_value { get; set; }

        /// <summary>
        /// 功能名稱
        /// </summary>
        [Description("功能名稱")]
        public string fun_value { get; set; }

        /// <summary>
        /// 功能名稱(前)
        /// </summary>
        [Description("功能名稱(前)")]
        public string fun_value_before { get; set; }

        /// <summary>
        /// 覆核單位代碼
        /// </summary>
        [Description("覆核單位名稱")]
        public string appr_unit_name { get; set; }

        /// <summary>
        /// 覆核單位代碼(前)
        /// </summary>
        [Description("覆核單位名稱(前)")]
        public string appr_unit_name_before { get; set; }

        /// <summary>
        /// 承辦單位代碼
        /// </summary>
        [Description("承辦單位名稱")]
        public string user_unit_name { get; set; }

        /// <summary>
        /// 承辦單位代碼(前)
        /// </summary>
        [Description("承辦單位名稱(前)")]
        public string user_unit_name_before { get; set; }

        /// <summary>
        /// 備註(前)
        /// </summary>
        [Description("備註(前)")]
        public string memo { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string memo_before { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string apply_name { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        [Description("異動時間")]
        public string apply_time { get; set; }

        /// <summary>
        /// 覆核人員
        /// </summary>
        [Description("覆核人員")]
        public string appr_name { get; set; }

        /// <summary>
        /// 覆核時間
        /// </summary>
        [Description("覆核時間")]
        public string appr_time { get; set; }
    }
}