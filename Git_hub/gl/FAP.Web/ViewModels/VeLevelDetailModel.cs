using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeLevelDetailModel
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
        /// 給付對象姓名
        /// </summary>
        [Description("給付對象姓名")]
        public string paid_name { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 支票號碼簡稱
        /// </summary>
        [Description("支票號碼簡稱")]
        public string check_acct_short { get; set; }

        /// <summary>
        /// 支票到期日
        /// </summary>
        [Description("支票到期日")]
        public string check_date { get; set; }

        /// <summary>
        /// 支票金額
        /// </summary>
        [Description("支票金額")]
        public string check_amt { get; set; }

        /// <summary>
        /// 原給付性質
        /// </summary>
        [Description("原給付性質")]
        public string o_paid_cd { get; set; }

        /// <summary>
        /// 清理大類
        /// </summary>
        [Description("清理大類")]
        public string level_1 { get; set; }

        /// <summary>
        /// 清理小類
        /// </summary>
        [Description("清理小類")]
        public string level_2 { get; set; }

        /// <summary>
        /// 清理階段
        /// </summary>
        [Description("清理階段")]
        public string code_value { get; set; }

        /// <summary>
        /// 清理標準
        /// </summary>
        [Description("清理標準")]
        public string std_1 { get; set; }

        /// <summary>
        /// 清理階段日期	
        /// </summary>
        [Description("清理階段日期")]
        public string clean_date { get; set; }

        /// <summary>
        /// 清理階段完成日期	
        /// </summary>
        [Description("清理階段完成日期")]
        public string clean_f_date { get; set; }

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
        /// 顯示顏色
        /// </summary>
        [Description("顯示顏色")]
        public bool showColor { get; set; }


        [Description("清理狀態")]
        public string status { get; set; }

        [Description("帳務日期")]
        public string re_paid_date { get; set; }
    }
}