using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0026ViewModel
    {
        /// <summary>
        /// 部門代碼
        /// </summary>
        [Description("部門代碼")]
        public string unit_code { get; set; }

        /// <summary>
        /// 部門代碼(中文)
        /// </summary>
        [Description("部門代碼(中文)")]
        public string unit_code_value { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型")]
        public string ap_paid { get; set; }

        /// <summary>
        /// 給付類型(中文)
        /// </summary>
        [Description("給付類型(中文)")]
        public string ap_paid_value { get; set; }



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
        /// 建立時間
        /// </summary>
        [Description("建立時間")]
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