using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0030SearchModel
    {
        /// <summary>
        /// 用印申請日(起)
        /// </summary>
        [Description("用印申請日(起)")]
        public string applyDt_s { get; set; }

        /// <summary>
        /// 用印申請日(起)
        /// </summary>
        [Description("用印申請日(起)")]
        public string applyDt_e { get; set; }

        /// <summary>
        /// 表單單號
        /// </summary>
        [Description("表單單號")]
        public string report_no { get; set; }

    }
}