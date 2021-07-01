using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0029ViewModel
    {
        /// <summary>
        /// 執行動作中文 (Y=成功,R=退回)
        /// </summary>
        public string ce_result_status { get; set; }

        /// <summary>
        /// 執行動作 (Y=成功,R=退回)
        /// </summary>
        public string ce_result { get; set; }

        /// <summary>
        /// 來源
        /// </summary>
        [Description("來源")]
        public string scre_from { get; set; }

        /// <summary>
        /// 支票號碼(抽)
        /// </summary>
        [Description("支票號碼(抽)")]
        public string check_no { get; set; }

        /// <summary>
        /// 付款帳戶
        /// </summary>
        [Description("付款帳戶")]
        public string bank_code { get; set; }

        /// <summary>
        /// 支票面額
        /// </summary>
        [Description("支票面額")]
        public string amount { get; set; }

        /// <summary>
        /// 支票抬頭
        /// </summary>
        [Description("支票抬頭")]
        public string receiver { get; set; }

        /// <summary>
        /// 抽票申請單號
        /// </summary>
        [Description("抽票申請單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 抽票回覆人員
        /// </summary>
        [Description("抽票回覆人員")]
        public string ce_rply_id { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 退件原因
        /// </summary>
        [Description("退件原因")]
        public string rej_rsn { get; set; }

        /// <summary>
        /// 其他說明
        /// </summary>
        [Description("其他說明")]
        public string memo { get; set; }

        /// <summary>
        /// 是否選取
        /// </summary>
        [Description("是否選取")]
        public bool Ischecked { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool review_flag { get; set; }

        /// <summary>
        /// 修改註記
        /// </summary>
        [Description("修改註記")]
        public bool update_flag { get; set; }
    }
}