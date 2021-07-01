using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0021ASearchModel
    {
        /// <summary>
        /// 接收日(起)
        /// </summary>
        [Description("接收日(起)")]
        public string rece_date_s { get; set; }

        /// <summary>
        /// 接收日(迄)
        /// </summary>
        [Description("接收日(迄)")]
        public string rece_date_e { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }
    }
}