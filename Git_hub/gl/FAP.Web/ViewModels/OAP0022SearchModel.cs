using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0022SearchModel
    {
        /// <summary>
        /// 覆核日(起)
        /// </summary>
        [Description("覆核日(起)")]
        public string appr_s { get; set; }

        /// <summary>
        /// 覆核日(迄)
        /// </summary>
        [Description("覆核日(迄)")]
        public string appr_e { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        [Description("接收人")]
        public string rece_id { get; set; }

        /// <summary>
        /// 表單號碼
        /// </summary>
        [Description("表單號碼")]
        public string report_no { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }
    }
}