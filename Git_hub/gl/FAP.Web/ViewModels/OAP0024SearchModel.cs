using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0024SearchModel
    {
        /// <summary>
        /// 資料來源
        /// </summary>
        [Description("資料來源")]
        public string srce_from { get; set; }

        /// <summary>
        /// 新增日期(起)
        /// </summary>
        [Description("新增日期(起)")]
        public string entry_date_s { get; set; }

        /// <summary>
        /// 新增日期(迄)
        /// </summary>
        [Description("新增日期(迄)")]
        public string entry_date_e { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        [Description("新增人員")]
        public string entry_id { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 批次單號
        /// </summary>
        [Description("批次單號")]
        public string apply_no { get; set; }
   
    }
}