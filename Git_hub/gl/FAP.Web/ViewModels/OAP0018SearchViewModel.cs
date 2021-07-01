using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0018SearchViewModel
    {
        /// <summary>
        /// 覆核單位代碼
        /// </summary>
        [Description("覆核單位代碼")]
        public string  appr_unit { get; set; }

        /// <summary>
        /// 承辦單位代碼
        /// </summary>
        [Description("承辦單位代碼")]
        public string user_unit { get; set; }

        

        /// <summary>
        /// 查詢時間
        /// </summary>
        [Description("查詢時間")]
        public DateTime searchDt { get; set; }
    }
}