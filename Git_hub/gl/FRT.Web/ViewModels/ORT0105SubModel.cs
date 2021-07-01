using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FRT.Web.ViewModels
{
    public class ORT0105SubViewModel
    {
        /// <summary>
        /// 平台
        /// </summary>
        [Description("平台")]
        public string platform { get; set; }

        /// <summary>
        /// 檔案代碼
        /// </summary>
        [Description("檔案代碼")]
        public string file_code { get; set; }

        /// <summary>
        /// 檔案中文
        /// </summary>
        [Description("檔案中文")]
        public string file_code_d { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string memo { get; set; }

        ///// <summary>
        ///// 資料狀態代碼
        ///// </summary>
        //[Description("資料狀態代碼")]
        //public string data_status { get; set; }

        ///// <summary>
        ///// 資料狀態名稱
        ///// </summary>
        //[Description("資料狀態")]
        //public string data_status_value { get; set; }

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
        /// 跨系統勾稽作業主檔_pk_id B0105 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業主檔_pk_id")]
        public string check_id { get; set; }

        /// <summary>
        /// 跨系統勾稽作業明細檔_id B0106 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業明細檔_id")]
        public string sub_id { get; set; }


        /// <summary>
        /// 跨系統勾稽作業異動檔_pk_id S0105 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業異動檔_pk_id")]
        public string his_check_id { get; set; }

        /// <summary>
        /// 跨系統勾稽作業明細異動檔_pk_id S0106 + 系統日期YYYY(民國年) + 4碼流水號
        /// </summary>
        [Description("跨系統勾稽作業明細異動檔_pk_id")]
        public string his_sub_id { get; set; }

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