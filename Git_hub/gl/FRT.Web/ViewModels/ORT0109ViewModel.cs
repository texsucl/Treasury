using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FRT.Web.ViewModels
{
    public class ORT0109ViewModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Description("帳號")]
        public string bank_acct_no { get; set; }

        /// <summary>
        /// 銀行簡稱
        /// </summary>
        [Description("銀行簡稱")]
        public string bank_acct_make_out { get; set; } 

        /// <summary>
        /// 資料狀態代碼 SYS_CD => RT ,CODE_TYPE =>DATA_STATUS
        /// </summary>
        [Description("資料狀態代碼")]
        public string data_status { get; set; }

        /// <summary>
        /// 資料狀態名稱
        /// </summary>
        [Description("資料狀態")]
        public string data_status_value { get; set; }

        /// <summary>
        /// 執行功能 sys_code => EXEC_ACTION
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
        /// 建立人員(名子)
        /// </summary>
        [Description("建立人員(名子)")]
        public string create_name { get; set; }

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
        /// 修改人員(名子)
        /// </summary>
        [Description("修改人員(名子)")]
        public string update_name { get; set; }

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
        /// 覆核人員(名子)
        /// </summary>
        [Description("覆核人員(名子)")]
        public string appr_name { get; set; }

        /// <summary>
        /// 覆核時間
        /// </summary>
        [Description("覆核時間")]
        public string appr_datetime { get; set; }

        /// <summary>
        /// 修改時間
        /// </summary>
        [Description("修改時間")]
        public DateTime update_time_compare { get; set; }

        /// <summary>
        /// 銀存銷帳不比對帳號作業異動檔_pk_id S0109 + 系統日期YYYYMMDD + 5碼流水號
        /// </summary>
        [Description("銀存銷帳不比對帳號作業異動檔_pk_id")]
        public string his_pk_id { get; set; }

        /// <summary>
        /// 銀存銷帳不比對帳號作業 pk_id B0109 + 系統日期YYYYMMDD + 5碼流水號
        /// </summary>
        [Description("銀存銷帳不比對帳號作業 pk_id")]
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