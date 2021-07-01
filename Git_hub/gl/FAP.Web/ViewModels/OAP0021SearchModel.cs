using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0021SearchModel
    {
        /// <summary>
        /// 申請日(起)
        /// </summary>
        [Description("申請日(起)")]
        public string apply_date_s { get; set; }

        /// <summary>
        /// 申請日(迄)
        /// </summary>
        [Description("申請日(迄)")]
        public string apply_date_e { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }
    }
}