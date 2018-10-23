using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class DepositViewModel : ITreaItem
    {
        /// <summary>
        /// 定期存單庫存資料檔
        /// </summary>
        [Description("定期存單庫存資料檔")]
        public List<Deposit_M> vDeposit_M { get; set; }

        /// <summary>
        /// 定期存單庫存資料明細檔
        /// </summary>
        [Description("定期存單庫存資料明細檔")]
        public List<Deposit_D> vDeposit_D { get; set; }
    }

    public class Deposit_M
    {
        /// <summary>
        /// 編號
        /// </summary>
        [Description("編號")]
        public int vRowNum { get; set; }

        /// <summary>
        /// 物品編號
        /// </summary>
        [Description("物品編號")]
        public string vItem_Id { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string vCurrency { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Description("交易對象")]
        public string vTrad_Partners { get; set; }

        /// <summary>
        /// 承作日期
        /// </summary>
        [Description("承作日期")]
        public string vCommit_Date { get; set; }

        /// <summary>
        /// 承作日期(民國)
        /// </summary>
        [Description("承作日期(民國)")]
        public string vCommit_Date_Tw {
            get { return DateTime.Parse(vCommit_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 到期日
        /// </summary>
        [Description("到期日")]
        public string vExpiry_Date { get; set; }

        /// <summary>
        /// 到期日(民國)
        /// </summary>
        [Description("到期日(民國)")]
        public string vExpiry_Date_Tw
        {
            get { return DateTime.Parse(vExpiry_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 計息方式
        /// </summary>
        [Description("計息方式")]
        public string vInterest_Rate_Type { get; set; }

        /// <summary>
        /// 利率%
        /// </summary>
        [Description("利率%")]
        public decimal vInterest_Rate { get; set; }

        /// <summary>
        /// 存單類型
        /// </summary>
        [Description("存單類型")]
        public string vDep_Type { get; set; }

        /// <summary>
        /// 總面額
        /// </summary>
        [Description("總面額")]
        public decimal vTotal_Denomination { get; set; }

        /// <summary>
        /// 設質否
        /// </summary>
        [Description("設質否")]
        public string vDep_Set_Quality { get; set; }

        /// <summary>
        /// 自動轉期
        /// </summary>
        [Description("自動轉期")]
        public string vAuto_Trans { get; set; }

        /// <summary>
        /// 轉期後到期日
        /// </summary>
        [Description("轉期後到期日")]
        public string vTrans_Expiry_Date { get; set; }

        /// <summary>
        /// 轉期後到期日(民國)
        /// </summary>
        [Description("轉期後到期日(民國)")]
        public string vTrans_Expiry_Date_Tw
        {
            get { return string.IsNullOrEmpty(vTrans_Expiry_Date) ? "" : DateTime.Parse(vTrans_Expiry_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 轉期次數
        /// </summary>
        [Description("轉期次數")]
        public int? vTrans_Tms { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 取出註記
        /// </summary>
        [Description("取出註記")]
        public bool vTakeoutFlag { get; set; }

        /// <summary>
        /// 訊息註記
        /// </summary>
        [Description("訊息註記")]
        public string MsgFlag { get; set; }

        /// <summary>
        /// 取出原因
        /// </summary>
        [Description("取出原因")]
        public string GetMsg { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }

    public class Deposit_D
    {
        /// <summary>
        /// 編號
        /// </summary>
        [Description("編號")]
        public string vRowNum { get; set; }

        /// <summary>
        /// 物品編號
        /// </summary>
        [Description("物品編號")]
        public string vItem_Id { get; set; }

        /// <summary>
        /// 明細流水號
        /// </summary>
        [Description("明細流水號")]
        public string vData_Seq { get; set; }

        /// <summary>
        /// 存單號碼前置碼
        /// </summary>
        [Description("存單號碼前置碼")]
        public string vDep_No_Preamble { get; set; }

        /// <summary>
        /// 存單號碼(起)
        /// </summary>
        [Description("存單號碼(起)")]
        public string vDep_No_B { get; set; }

        /// <summary>
        /// 存單號碼(迄)
        /// </summary>
        [Description("存單號碼(迄)")]
        public string vDep_No_E { get; set; }

        /// <summary>
        /// 存單號碼尾碼
        /// </summary>
        [Description("存單號碼尾碼")]
        public string vDep_No_Tail { get; set; }

        /// <summary>
        /// 存單張數
        /// </summary>
        [Description("存單張數")]
        public int vDep_Cnt { get; set; }

        /// <summary>
        /// 單張面額
        /// </summary>
        [Description("單張面額")]
        public decimal vDenomination { get; set; }

        /// <summary>
        /// 面額小計
        /// </summary>
        [Description("面額小計")]
        public decimal vSubtotal_Denomination { get; set; }
    }

    public class DepositReportGroupData
    {
        /// <summary>
        /// 台幣/外幣
        /// </summary>
        [Description("台幣/外幣")]
        public string isNTD { get; set; }

        /// <summary>
        /// 存單類型 (0:一般+NCD,1:一般,2:NCD)
        /// </summary>
        [Description("存單類型(0:一般+NCD,1:一般,2:NCD)")]
        public string vDep_Type { get; set; }
    }
}