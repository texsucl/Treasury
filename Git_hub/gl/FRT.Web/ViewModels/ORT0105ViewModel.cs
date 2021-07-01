using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FRT.Web.ViewModels
{
    public class ORT0105ViewModel
    {
        /// <summary>
        /// 類別 SYS_CD => RT , CODE_TYPE =>GOJ_TYPE
        /// </summary>
        [Description("類別")]
        public string type { get; set; }

        /// <summary>
        /// 類別(中文) 
        /// </summary>
        [Description("類別(中文)")]
        public string type_d { get; set; }

        /// <summary>
        /// 性質 SYS_CD => RT , CODE_TYPE => GOJ_TYPE_{類別}_GROUP
        /// </summary>
        [Description("性質")]
        public string kind { get; set; }

        /// <summary>
        /// 性質(中文) 
        /// </summary>
        [Description("性質(中文)")]
        public string kind_d { get; set; }

        /// <summary>
        /// 頻率類別 m => 月 , d => 日
        /// </summary>
        [Description("頻率類別")]
        public string frequency { get; set; }

        /// <summary>
        /// 頻率參數 月 => 執行工作天 , 日 => 執行基準日
        /// </summary>
        [Description("頻率參數")]
        public int frequency_value { get; set; }

        /// <summary>
        /// 頻率(中文)
        /// </summary>
        [Description("頻率(中文)")]
        public string frequency_d { get; set; }

        /// <summary>
        /// 執行時間
        /// </summary>
        [Description("執行時間")]
        public TimeSpan scheduler_time { get; set; }

        /// <summary>
        /// 執行時間(小時)
        /// </summary>
        [Description("執行時間(小時)")]
        public int scheduler_time_hh { get; set; }

        /// <summary>
        /// 執行時間(分鐘)
        /// </summary>
        [Description("執行時間(分鐘)")]
        public int scheduler_time_mm { get; set; }

        /// <summary>
        /// 執行時間(中文)
        /// </summary>
        [Description("執行時間(中文)")]
        public string scheduler_time_d { get; set; }

        /// <summary>
        /// 資料區間起始日類別
        /// </summary>
        [Description("資料區間起始日類別")]
        public string start_date_type { get; set; }

        /// <summary>
        /// 資料區間起始日類別參數
        /// </summary>
        [Description("資料區間起始日類別參數")]
        public string start_date_value { get; set; }

        /// <summary>
        /// 資料區間起始日(中文)
        /// </summary>
        [Description("資料區間起始日(中文)")]
        public string start_date_d { get; set; }

        /// <summary>
        /// Mail群組
        /// </summary>
        [Description("Mail群組")]
        public string mail_group { get; set; }

        /// <summary>
        /// MailKey
        /// </summary>
        [Description("MailKey")]
        public string mail_key { get; set; }

        /// <summary>
        /// Mail群組(中文)
        /// </summary>
        [Description("Mail群組(中文)")]
        public string mail_group_d { get; set; }

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
        /// 跨系統勾稽作業異動檔_pk_id S0105 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業異動檔_pk_id")]
        public string his_check_id { get; set; }

        /// <summary>
        /// 跨系統勾稽作業 pk_id B0105 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業 pk_id")]
        public string check_id { get; set; }

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
        /// 明細資料
        /// </summary>
        [Description("明細資料")]
        public List<ORT0105SubViewModel> subDatas { get; set; } = new List<ORT0105SubViewModel>();
    }
}