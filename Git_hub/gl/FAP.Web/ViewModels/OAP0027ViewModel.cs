using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0027ViewModel
    {
        /// <summary>
        /// 原因代碼
        /// </summary>
        [Description("原因代碼")]
        public string reason_code { get; set; }

        /// <summary>
        /// 原因代碼(中文)
        /// </summary>
        [Description("原因代碼(中文)")]
        public string reason { get; set; }

        /// <summary>
        /// 指定轉交部門
        /// </summary>
        [Description("指定轉交部門")]
        public string referral_dep { get; set; }

        /// <summary>
        /// 指定轉交部門(中文)
        /// </summary>
        [Description("指定轉交部門(中文)")]
        public string referral_dep_name { get; set; }

        /// <summary>
        /// 資料狀態代碼
        /// </summary>
        [Description("資料狀態代碼")]
        public string data_status { get; set; }

        /// <summary>
        /// 資料狀態名稱
        /// </summary>
        [Description("資料狀態")]
        public string data_status_value { get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能代碼")]
        public string exec_action { get; set; }

        /// <summary>
        /// 執行功能名稱
        /// </summary>
        [Description("執行功能")]
        public string exec_action_value { get; set; }

        /// <summary>
        /// 建立人員
        /// </summary>
        [Description("建立人員")]
        public string create_id { get; set; }

        /// <summary>
        /// 建立人員
        /// </summary>
        [Description("建立人員")]
        public string create_datetime { get; set; }

        /// <summary>
        /// 修改人員
        /// </summary>
        [Description("修改人員")]
        public string update_id { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Description("修改時間")]
        public string update_datetime { get; set; }

        /// <summary>
        /// 覆核人員
        /// </summary>
        [Description("覆核人員")]
        public string appr_id { get; set; }

        /// <summary>
        /// 覆核時間
        /// </summary>
        [Description("覆核時間")]
        public string appr_datetime { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Description("修改時間")]
        public DateTime update_time_cpmpare { get; set; }

        /// <summary>
        /// 申請單編號
        /// </summary>
        [Description("申請單編號")]
        public string aply_no { get; set; }

        /// <summary>
        /// pkid
        /// </summary>
        [Description("pkid")]
        public string pk_id { get; set; }

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
    }
}