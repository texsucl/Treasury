using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    /// <summary>
    /// 異動資料比對、票據狀態餘額、變更註記、逾期未兌領、支票回存 總表 
    /// </summary>
    public class ORTReportDModel
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
        /// AS400系統件數(a) (A,B,C)
        /// </summary>
        [Description("AS400系統件數(a)")]
        public int AS400_SYS_Count { get; set; }

        /// <summary>
        /// AS400系統金額(A) (A,B)
        /// </summary>
        [Description("AS400系統金額(A)")]
        public decimal AS400_SYS_Amt { get; set; }

        /// <summary>
        /// AS400差異件數(b) (A,B,C)
        /// </summary>
        [Description("AS400差異件數(b)")]
        public int AS400_Diff_Count { get; set; }

        /// <summary>
        /// AS400差異金額(B) (A,B)
        /// </summary>
        [Description("AS400差異金額(B)")]
        public decimal AS400_Diff_Amt { get; set; }

        /// <summary>
        /// Wanpie差異件數(c) (A,B,C)
        /// </summary>
        [Description("Wanpie差異件數(c)")]
        public int Wanpie_Diff_Count { get; set; }

        /// <summary>
        /// Wanpie差異金額(C) (A,B)
        /// </summary>
        [Description("Wanpie差異金額(C)")]
        public decimal Wanpie_Diff_Amt { get; set; }

        /// <summary>
        /// 票據系統 件數(a) (D,E)
        /// </summary>
        [Description("票據系統 件數(a)")]
        public int ACT_Count { get; set; }

        /// <summary>
        /// 票據系統 金額(A) (D,E)
        /// </summary>
        [Description("票據系統 金額(A)")]
        public decimal ACT_Amt { get; set; }

        /// <summary>
        /// 票據差異 件數(b) (D,E)
        /// </summary>
        [Description("票據差異 件數(b)")]
        public int ACT_Diff_Count { get; set; }

        /// <summary>
        /// 票據差異 金額(B) (D,E)
        /// </summary>
        [Description("票據差異 金額(B)")]
        public decimal ACT_Diff_Amt { get; set; }

        /// <summary>
        /// 應付未付差異 件數(c) (D,E)
        /// </summary>
        [Description("應付未付差異 件數(c)")]
        public int UNPAY_Diff_Count { get; set; }

        /// <summary>
        /// 應付未付差異 金額(C) (D,E)
        /// </summary>
        [Description("應付未付差異 金額(C)")]
        public decimal UNPAY_Diff_Amt { get; set; }

        /// <summary>
        /// 調節後件數(a-b+c) (A,B,D,E)
        /// </summary>
        [Description("調節後件數(a-b+c)")]
        public int ADJ_Count { get; set; }

        /// <summary>
        /// 調節後金額(A-B+C) (A,B,D,E)
        /// </summary>
        [Description("調節後金額(A-B+C)")]
        public decimal ADJ_Amt { get; set; }

        /// <summary>
        /// Wanpie系統件數 (A,B,C)
        /// </summary>
        [Description("Wanpie系統件數")]
        public int Wanpie_SYS_Count { get; set; }

        /// <summary>
        /// Wanpie系統金額 (A,B)
        /// </summary>
        [Description("Wanpie系統金額")]
        public decimal Wanpie_SYS_Amt { get; set; }

        /// <summary>
        /// 差異件數 (C)
        /// </summary>
        [Description("差異件數")]
        public int Diff_Count { get; set; }

        /// <summary>
        /// 差異件數(差異歸類) (C)
        /// </summary>
        [Description("差異件數(差異歸類)")]
        public int Diff_Coune_D { get; set; }

        /// <summary>
        /// 檢查1 (A,B,D,E)
        /// </summary>
        [Description("檢查1")]
        public string CHECK_1 { get; set; }

        /// <summary>
        /// 檢查2 (A,B,D,E)
        /// </summary>
        [Description("檢查2")]
        public string CHECK_2 { get; set; }
    }
}