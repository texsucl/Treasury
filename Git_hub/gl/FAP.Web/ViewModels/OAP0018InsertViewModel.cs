using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0018InsertViewModel
    {
        /// <summary>
        /// pkid
        /// </summary>
        [Description("pkid")]
        public string pk_id { get; set; }

        /// <summary>
        /// 功能代碼
        /// </summary>
        [Description("功能代碼")]
        public string fun_id { get; set; }

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
    }
}