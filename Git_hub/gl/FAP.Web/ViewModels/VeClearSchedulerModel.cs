using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeClearSchedulerModel
    {
        /// <summary>
        /// 電訪編號
        /// </summary>
        [Description("電訪編號")]
        public string tel_proc_no { get; set; }

        /// <summary>
        /// 給付對象 ID
        /// </summary>
        [Description("給付對象 ID")]
        public string paid_id { get; set; }

        /// <summary>
        /// 給付對象
        /// </summary>
        [Description("給付對象")]
        public string paid_name { get; set; }

        /// <summary>
        /// 追蹤人員
        /// </summary>
        [Description("追蹤人員")]
        public string proc_id { get; set; }

        /// <summary>
        /// 逾期天數
        /// </summary>
        [Description("逾期天數")]
        public string VE_day { get; set; }

        /// <summary>
        /// 逾期原因
        /// </summary>
        [Description("逾期原因")]
        public string VE_memo { get; set; }

        /// <summary>
        /// 第一次電訪處理結果_統計
        /// </summary>
        [Description("第一次電訪處理結果_統計")]
        public string tel_result_first { get; set; }

        /// <summary>
        /// 最後一次電訪日期
        /// </summary>
        [Description("最後一次電訪日期")]
        public string tel_interview_date { get; set; }

        /// <summary>
        /// 電訪處理結果_統計
        /// </summary>
        [Description("電訪處理結果_統計")]
        public string tel_result_last { get; set; }

        /// <summary>
        /// 第一次追踨標準
        /// </summary>
        [Description("第一次追踨標準")]
        public string tel_result_cnt_1 { get; set; }

        /// <summary>
        /// 第一次追踨日期
        /// </summary>
        [Description("第一次追踨日期")]
        public string tel_interview_date_1 { get; set; }

        /// <summary>
        /// 第一次追踨標準
        /// </summary>
        [Description("第二次追踨標準")]
        public string tel_result_cnt_2 { get; set; }

        /// <summary>
        /// 第一次追踨日期
        /// </summary>
        [Description("第二次追踨日期")]
        public string tel_interview_date_2 { get; set; }

        /// <summary>
        /// 第一次追踨標準
        /// </summary>
        [Description("第三次追踨標準")]
        public string tel_result_cnt_3 { get; set; }

        /// <summary>
        /// 第一次追踨日期
        /// </summary>
        [Description("第三次追踨日期")]
        public string tel_interview_date_3 { get; set; }

        /// <summary>
        /// 處理結果已達次數
        /// </summary>
        [Description("處理結果已達次數")]
        public int tel_result_cnt { get; set; } 
    }
}