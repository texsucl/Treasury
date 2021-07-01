using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0023SearchModel
    {
        /// <summary>
        /// 查詢類型
        /// </summary>
        [Description("類型")]
        public string type { get; set; }

        /// <summary>
        /// 開票日(起)
        /// </summary>
        [Description("開票日(起)")]
        public string stat_date_s { get; set; }

        /// <summary>
        /// 開票日(迄)
        /// </summary>
        [Description("開票日(迄)")]
        public string stat_date_e { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型")]
        public string pay_class { get; set; }

        /// <summary>
        /// 支票號碼1(起)
        /// </summary>
        [Description("支票號碼1(起)")]
        public string check_no_1_s { get; set; }

        /// <summary>
        /// 支票號碼1(迄)
        /// </summary>
        [Description("支票號碼1(迄)")]
        public string check_no_1_e { get; set; }

        /// <summary>
        /// 支票號碼2(起)
        /// </summary>
        [Description("支票號碼2(起)")]
        public string check_no_2_s { get; set; }

        /// <summary>
        /// 支票號碼2(迄)
        /// </summary>
        [Description("支票號碼2(迄)")]
        public string check_no_2_e { get; set; }

        /// <summary>
        /// 支票號碼3(起)
        /// </summary>
        [Description("支票號碼3(起)")]
        public string check_no_3_s { get; set; }

        /// <summary>
        /// 支票號碼3(迄)
        /// </summary>
        [Description("支票號碼3(迄)")]
        public string check_no_3_e { get; set; }
    }
}