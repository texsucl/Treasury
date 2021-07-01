using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0031SearchModel
    {
        /// <summary>
        /// 新增日(起)
        /// </summary>
        [Description("新增日(起)")]
        public string create_date_s { get; set; }

        /// <summary>
        /// 新增日(迄)
        /// </summary>
        [Description("新增日(迄)")]
        public string create_date_e { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }

        /// <summary>
        /// 標籤號碼空白
        /// </summary>
        [Description("標籤號碼空白")]
        public string label_no_flag { get; set; }
    }
}