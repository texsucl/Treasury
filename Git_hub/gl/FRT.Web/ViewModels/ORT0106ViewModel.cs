using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FRT.Web.ViewModels
{
    public class ORT0106ViewModel
    {
        /// <summary>
        /// id
        /// </summary>
        [Description("Id")]
        public string id { get; set; }

        /// <summary>
        /// 報表title
        /// </summary>
        [Description("報表title")]
        public string title { get; set; }

        /// <summary>
        /// 報表title 日期
        /// </summary>
        [Description("報表title 日期")]
        public string title_Date { get; set;}

        /// <summary>
        /// date_s
        /// </summary>
        [Description("date_s")]
        public string date_s { get; set; }

        /// <summary>
        /// date_e
        /// </summary>
        [Description("date_e")]
        public string date_e { get; set; }

        /// <summary>
        /// 執行狀態
        /// </summary>
        [Description("執行狀態")]
        public string runFlag { get; set; }

        /// <summary>
        /// 報表名稱
        /// </summary>
        [Description("報表名稱")]
        public string className { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        [Description("截止日期")]
        public string deadline { get; set; }
    }
}