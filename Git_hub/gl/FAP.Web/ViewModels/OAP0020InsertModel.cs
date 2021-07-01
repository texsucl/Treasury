﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0020InsertModel
    {
        /// <summary>
        /// 主檔或明細檔  M or D
        /// </summary>
        [Description("主檔或明細檔")]

        public string type { get; set; }

        /// <summary>
        /// 主檔 (部門檔)
        /// </summary>
        [Description("主檔 (部門檔)")]

        public string dep_id { get; set; }

        /// <summary>
        /// 主檔 (部門檔) name
        /// </summary>
        [Description("主檔 (部門檔) name")]

        public string dep_name { get; set; }

        /// <summary>
        /// 明細檔 (科別檔)
        /// </summary>
        [Description("明細檔 (科別檔)")]

        public string division { get; set; }

        /// <summary>
        /// 窗口人員
        /// </summary>
        [Description("窗口人員")]

        public string apt_id { get; set; }

        /// <summary>
        /// 窗口人員
        /// </summary>
        [Description("窗口人員")]

        public string apt_name { get; set; }

        /// <summary>
        /// 窗口電子信箱
        /// </summary>
        [Description("窗口電子信箱")]

        public string email { get; set; }
    }
}