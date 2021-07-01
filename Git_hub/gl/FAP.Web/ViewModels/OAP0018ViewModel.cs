using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0018ViewModel
    {
        /// <summary>
        /// 功能代碼
        /// </summary>
        [Description("功能代碼")]
        public string fun_id { get; set; }

        /// <summary>
        /// 功能名稱
        /// </summary>
        [Description("功能名稱")]
        public string fun_value { get; set; }

        /// <summary>
        /// 覆核單位代碼
        /// </summary>
        [Description("覆核單位代碼")]
        public string appr_unit { get; set; }

        /// <summary>
        /// 覆核單位代碼
        /// </summary>
        [Description("覆核單位名稱")]
        public string appr_unit_name { get; set; }

        /// <summary>
        /// 承辦單位代碼
        /// </summary>
        [Description("承辦單位代碼")]
        public string user_unit { get; set; }

        /// <summary>
        /// 承辦單位代碼
        /// </summary>
        [Description("承辦單位名稱")]
        public string user_unit_name { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string memo { get; set; }

        /// <summary>
        /// 資料狀態代碼
        /// </summary>
        [Description("資料狀態代碼")]
        public string data_status { get; set; }

        /// <summary>
        /// 資料狀態名稱
        /// </summary>
        [Description("資料狀態")]
        public string data_status_value { get; set; }

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
        /// 修改人員
        /// </summary>
        [Description("修改人員")]
        public string update_name { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Description("修改時間")]
        public string update_time { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Description("修改時間")]
        public DateTime update_time_cpmpare { get; set; }

        /// <summary>
        /// pkid
        /// </summary>
        [Description("pkid")]
        public string pk_id { get; set; }
    }
}