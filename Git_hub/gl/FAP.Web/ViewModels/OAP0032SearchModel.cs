using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0032SearchModel
    {
        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string update_date { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string update_id { get; set; }

        /// <summary>
        /// 標籤號碼
        /// </summary>
        [Description("標籤號碼")]
        public string label_no { get; set; }
    }
}