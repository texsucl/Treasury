using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class TreasuryReportViewModel 
    {
        /// <summary>
        /// 庫存表名稱下拉式選單
        /// </summary>
        [Description("庫存表名稱下拉式選單")]
        public List<SelectOption> vjobProject { get; set; }

        /// <summary>
        /// 股票編號
        /// </summary>
        [Description("股票編號")]
        public List<SelectOption> vName{ get; set; }
        
        /// <summary>
        /// 不動產冊號
        /// </summary>
        [Description("不動產冊號")]
        public List<SelectOption> vBook_No { get; set; }
       
        /// <summary>
        /// 定期存單交易對象
        /// </summary>
        [Description("定期存單交易對象")]
        public List<SelectOption> vTRAD_Partners { get; set; }

        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public List<SelectOption> vdept { get; set; }

    }
}