using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 異動資料比對、票據狀態餘額、變更註記、逾期未兌領、支票回存 差異明細表
    /// </summary>
    public class ORTReportDDetailModel
    {
        /// <summary>
        /// 報表編號
        /// </summary>
        [Description("報表編號")]
        public string NO { get; set; }

        /// <summary>
        /// 性質代碼：異動資料比對、票據狀態餘額、變更註記、逾期未兌、支票回存
        /// </summary>
        [Description("性質代碼")]
        public string Kind { get; set; }

        /// <summary>
        /// 對照狀態中文 (A,B,D,E)
        /// </summary>
        [Description("對照狀態中文")]
        public string STS_Name { get; set; }

        /// <summary>
        /// 對照抵兌回存別中文 (A,B,D,E)
        /// </summary>
        [Description("對照抵兌回存別中文")]
        public string Code_Name { get; set; }

        /// <summary>
        /// 支票號碼 (A,B,C,D,E)
        /// </summary>
        [Description("支票號碼")]
        public string Check_no { get; set; }

        /// <summary>
        /// 帳戶簡稱 (A,B,C,D,E)
        /// </summary>
        [Description("帳戶簡稱")]
        public string Bank_Code { get; set; }

        /// <summary>
        /// AS400金額 (A,B)
        /// </summary>
        [Description("AS400金額")]
        public decimal AS400_Amt { get; set; }

        /// <summary>
        /// Wanpie金額 (A,B)
        /// </summary>
        [Description("Wanpie金額")]
        public decimal Wanpie_Amt { get; set; }

        /// <summary>
        /// 差異金額 (A,B)
        /// </summary>
        [Description("差異金額")]
        public decimal Diff_Amt { get; set; }

        /// <summary>
        /// AS400件數 (C)
        /// </summary>
        [Description("AS400件數")]
        public int AS400_Count { get; set; }

        /// <summary>
        /// Wanpie件數 (C)
        /// </summary>
        [Description("Wanpie件數")]
        public int Wanpie_Count { get; set; }

        /// <summary>
        /// 票據金額 (D,E)
        /// </summary>
        [Description("票據金額")]
        public decimal ACT_Amt { get; set; }

        /// <summary>
        /// 應付未付金額 (D,E)
        /// </summary>
        [Description("應付未付金額")]
        public decimal UNPAY_Amt { get; set; }

        /// <summary>
        /// 差異件數 (C)
        /// </summary>
        [Description("差異件數")]
        public int Diff_Count { get; set; }

        /// <summary>
        /// 系統註記 (A,B,C,D,E)
        /// </summary>
        [Description("系統註記")]
        public string SYS_Flag { get; set; }

        /// <summary>
        /// 其他說明 (A,B,C,D,E)
        /// </summary>
        [Description("其他說明")]
        public string Remark { get; set; }
    }
}