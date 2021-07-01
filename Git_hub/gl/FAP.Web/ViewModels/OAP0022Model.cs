using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0022Model
    {
        public OAP0022Model() {
            //checkFlag = true;
        }
        public bool checkFlag { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 接收日
        /// </summary>
        [Description("接收日")]
        public string rece_date { get; set; }

        /// <summary>
        /// 接收時間
        /// </summary>
        [Description("接收時間")]
        public string rece_time { get; set; }

        /// <summary>
        /// 註記
        /// </summary>
        [Description("註記")]
        public string mark_type2 { get; set; }

        /// <summary>
        /// 接收人員
        /// </summary>
        [Description("接收人員")]
        public string rece_id { get; set; }

        /// <summary>
        /// 表單號碼
        /// </summary>
        [Description("表單號碼")]
        public string report_no { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }
    }
}