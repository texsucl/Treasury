using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// AS400_匯款件檢核報表
    /// </summary>
    public class ORT0105NP1Model
    {
        /// <summary>
        /// 比對日期
        /// </summary>
        [Description("比對日期")]
        public string Date { get; set; }

        /// <summary>
        /// 票據狀態
        /// </summary>
        [Description("票據狀態")]
        public string ACCT_STS { get; set; }

        /// <summary>
        /// 對照狀態中文
        /// </summary>
        [Description("對照狀態中文")]
        public string STS_Name { get; set; }

        /// <summary>
        /// 抵兌回存別
        /// </summary>
        [Description("抵兌回存別")]
        public string PAY_Code { get; set; }

        /// <summary>
        /// 對照抵兌回存別中文
        /// </summary>
        [Description("對照抵兌回存別中文")]
        public string Code_Name { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string Check_no { get; set; }

        /// <summary>
        /// 帳戶簡稱
        /// </summary>
        [Description("帳戶簡稱")]
        public string Bank_Code { get; set; }

        /// <summary>
        /// AS400金額
        /// </summary>
        [Description("AS400金額")]
        public decimal? AS400_Amt { get; set; }

        /// <summary>
        /// Wanpie金額
        /// </summary>
        [Description("Wanpie金額")]
        public string Wanpie_Amt { get; set; }

        /// <summary>
        /// 差異金額
        /// </summary>
        [Description("差異金額")]
        public string Diff_Amt { get; set; }

        /// <summary>
        /// AS400件數
        /// </summary>
        [Description("AS400件數")]
        public string AS400_Count { get; set; }

        /// <summary>
        /// Wanpie件數
        /// </summary>
        [Description("Wanpie件數")]
        public string Wanpie_Count { get; set; }

        /// <summary>
        /// 票據金額
        /// </summary>
        [Description("票據金額")]
        public string ACT_Amt { get; set; }

        /// <summary>
        /// 應付未付金額
        /// </summary>
        [Description("應付未付金額")]
        public string UNPAY_Amt { get; set; }


        /// <summary>
        /// 差異件數
        /// </summary>
        [Description("差異件數")]
        public string Diff_Count { get; set; }

        /// <summary>
        /// 系統註記
        /// </summary>
        [Description("系統註記")]
        public string SYS_Flag { get; set; }

        /// <summary>
        /// 其他說明
        /// </summary>
        [Description("其他說明")]
        public string Remark { get; set; }
    }
}