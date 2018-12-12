using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(定期存單)畫面
    /// </summary>
    public class CDCDepositViewModel : ICDCItem
    {
        public CDCDepositViewModel() {
            vDeposit_M = new List<CDCDeposit_M>();
            vDeposit_D = new List<CDCDeposit_D>();
        }

        /// <summary>
        /// 定期存單庫存資料檔
        /// </summary>
        [Description("定期存單庫存資料檔")]
        public List<CDCDeposit_M> vDeposit_M { get; set; }

        /// <summary>
        /// 定期存單庫存資料明細檔
        /// </summary>
        [Description("定期存單庫存資料明細檔")]
        public List<CDCDeposit_D> vDeposit_D { get; set; }
    }
    public class CDCDeposit_M
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItemId { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 入庫日期
        /// </summary>
        [Description("入庫日期")]
        public string vPut_Date { get; set; }

        /// <summary>
        /// 入庫日期
        /// </summary>
        [Description("取出日期")]
        public string vGet_Date { get; set; }

        /// <summary>
        /// 取出申請人
        /// </summary>
        [Description("取出申請人")]
        public string vGet_Uid_Name { get; set; }

        /// <summary>
        /// 存入申請人ID
        /// </summary>
        [Description("存入申請人ID")]
        public string vAply_Uid { get; set; }

        /// <summary>
        /// 存入申請人
        /// </summary>
        [Description("存入申請人")]
        public string vAply_Uid_Name { get; set; }

        /// <summary>
        /// 權責部門ID
        /// </summary>
        [Description("權責部門ID")]
        public string vCharge_Dept { get; set; }

        /// <summary>
        /// 權責部門ID_異動後
        /// </summary>
        [Description("權責部門ID_異動後")]
        public string vCharge_Dept_AFT { get; set; }

        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCharge_Dept_Name { get; set; }

        /// <summary>
        /// 權責部門_異動後
        /// </summary>
        [Description("權責部門_異動後")]
        public string vCharge_Dept_Name_AFT { get; set; }

        /// <summary>
        /// 權責科別ID
        /// </summary>
        [Description("權責科別ID")]
        public string vCharge_Sect { get; set; }

        /// <summary>
        /// 權責科別ID_異動後
        /// </summary>
        [Description("權責科別ID_異動後")]
        public string vCharge_Sect_AFT { get; set; }

        /// <summary>
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCharge_Sect_Name { get; set; }

        /// <summary>
        /// 權責科別_異動後
        /// </summary>
        [Description("權責科別_異動後")]
        public string vCharge_Sect_Name_AFT { get; set; }

        /// <summary>
        /// 權責單位
        /// </summary>
        [Description("權責單位")]
        public string vCharge_Name { get; set; }

        /// <summary>
        /// 權責單位_異動後
        /// </summary>
        [Description("權責單位_異動後")]
        public string vCharge_Name_AFT { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Description("幣別")]
        public string vCurrency { get; set; }

        /// <summary>
        /// 幣別_異動後
        /// </summary>
        [Description("幣別_異動後")]
        public string vCurrency_Aft { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Description("交易對象")]
        public string vTrad_Partners { get; set; }

        /// <summary>
        /// 交易對象_異動後
        /// </summary>
        [Description("交易對象_異動後")]
        public string vTrad_Partners_Aft { get; set; }

        /// <summary>
        /// 承作日期
        /// </summary>
        [Description("承作日期")]
        public string vCommit_Date { get; set; }

        /// <summary>
        /// 承作日期_異動後
        /// </summary>
        [Description("承作日期_異動後")]
        public string vCommit_Date_Aft { get; set; }

        /// <summary>
        /// 承作日期(民國)
        /// </summary>
        [Description("承作日期(民國)")]
        public string vCommit_Date_Tw
        {
            get { return DateTime.Parse(vCommit_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 承作日期(民國)_異動後
        /// </summary>
        [Description("承作日期(民國)_異動後")]
        public string vCommit_Date_Tw_Aft
        {
            get { return string.IsNullOrEmpty(vCommit_Date_Aft) ? "" : DateTime.Parse(vCommit_Date_Aft).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 到期日
        /// </summary>
        [Description("到期日")]
        public string vExpiry_Date { get; set; }

        /// <summary>
        /// 到期日_異動後
        /// </summary>
        [Description("到期日_異動後")]
        public string vExpiry_Date_Aft { get; set; }

        /// <summary>
        /// 到期日(民國)
        /// </summary>
        [Description("到期日(民國)")]
        public string vExpiry_Date_Tw
        {
            get { return DateTime.Parse(vExpiry_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 到期日(民國)_異動後
        /// </summary>
        [Description("到期日(民國)_異動後")]
        public string vExpiry_Date_Tw_Aft
        {
            get { return string.IsNullOrEmpty(vExpiry_Date_Aft) ? "" : DateTime.Parse(vExpiry_Date_Aft).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 計息方式
        /// </summary>
        [Description("計息方式")]
        public string vInterest_Rate_Type { get; set; }

        /// <summary>
        /// 計息方式_異動後
        /// </summary>
        [Description("計息方式_異動後")]
        public string vInterest_Rate_Type_Aft { get; set; }

        /// <summary>
        /// 利率%
        /// </summary>
        [Description("利率%")]
        public decimal vInterest_Rate { get; set; }

        /// <summary>
        /// 利率%_異動後
        /// </summary>
        [Description("利率%_異動後")]
        public decimal? vInterest_Rate_Aft { get; set; }

        /// <summary>
        /// 存單類型
        /// </summary>
        [Description("存單類型")]
        public string vDep_Type { get; set; }

        /// <summary>
        /// 存單類型_異動後
        /// </summary>
        [Description("存單類型_異動後")]
        public string vDep_Type_Aft { get; set; }

        /// <summary>
        /// 總面額
        /// </summary>
        [Description("總面額")]
        public decimal vTotal_Denomination { get; set; }

        /// <summary>
        /// 總面額_異動後
        /// </summary>
        [Description("總面額_異動後")]
        public decimal? vTotal_Denomination_Aft { get; set; }

        /// <summary>
        /// 設質否
        /// </summary>
        [Description("設質否")]
        public string vDep_Set_Quality { get; set; }

        /// <summary>
        /// 設質否_異動後
        /// </summary>
        [Description("設質否_異動後")]
        public string vDep_Set_Quality_Aft { get; set; }

        /// <summary>
        /// 自動轉期
        /// </summary>
        [Description("自動轉期")]
        public string vAuto_Trans { get; set; }

        /// <summary>
        /// 自動轉期_異動後
        /// </summary>
        [Description("自動轉期_異動後")]
        public string vAuto_Trans_Aft { get; set; }

        /// <summary>
        /// 轉期後到期日
        /// </summary>
        [Description("轉期後到期日")]
        public string vTrans_Expiry_Date { get; set; }

        /// <summary>
        /// 轉期後到期日_異動後
        /// </summary>
        [Description("轉期後到期日_異動後")]
        public string vTrans_Expiry_Date_Aft { get; set; }

        /// <summary>
        /// 轉期後到期日(民國)
        /// </summary>
        [Description("轉期後到期日(民國)")]
        public string vTrans_Expiry_Date_Tw
        {
            get { return string.IsNullOrEmpty(vTrans_Expiry_Date) ? "" : DateTime.Parse(vTrans_Expiry_Date).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 轉期後到期日(民國)_異動後
        /// </summary>
        [Description("轉期後到期日(民國)_異動後")]
        public string vTrans_Expiry_Date_Tw_Aft
        {
            get { return string.IsNullOrEmpty(vTrans_Expiry_Date_Aft) ? "" : DateTime.Parse(vTrans_Expiry_Date_Aft).DateToTaiwanDate(9); }
        }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 備註_異動後
        /// </summary>
        [Description("備註_異動後")]
        public string vMemo_Aft { get; set; }

        /// <summary>
        /// 轉期次數
        /// </summary>
        [Description("轉期次數")]
        public int? vTrans_Tms { get; set; }

        /// <summary>
        /// 轉期次數_異動後
        /// </summary>
        [Description("轉期次數_異動後")]
        public int? vTrans_Tms_Aft { get; set; }

        /// <summary>
        /// 已轉期次數
        /// </summary>
        [Description("已轉期次數")]
        public int? vAlready_Trans_Tms { get; set; }

        /// <summary>
        /// 已轉期次數_異動後
        /// </summary>
        [Description("已轉期次數_異動後")]
        public int? vAlready_Trans_Tms_Aft { get; set; }

        /// <summary>
        /// 異動註記
        /// </summary>
        [Description("異動註記")]
        public bool vAftFlag { get; set; }

        /// <summary>
        /// 轉期註記
        /// </summary>
        [Description("轉期註記")]
        public bool vTransFlag { get; set; }

        /// <summary>
        /// 確認是否轉期
        /// </summary>
        [Description("確認是否轉期")]
        public string sAutoTransFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }

    }

    public class CDCDeposit_D
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItemId { get; set; }

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
        /// 存單號碼前置碼_異動後
        /// </summary>
        [Description("存單號碼前置碼_異動後")]
        public string vDep_No_Preamble_Aft { get; set; }

        /// <summary>
        /// 存單號碼(起)
        /// </summary>
        [Description("存單號碼(起)")]
        public string vDep_No_B { get; set; }

        /// <summary>
        /// 存單號碼(起)_異動後
        /// </summary>
        [Description("存單號碼(起)_異動後")]
        public string vDep_No_B_Aft { get; set; }

        /// <summary>
        /// 存單號碼(迄)
        /// </summary>
        [Description("存單號碼(迄)")]
        public string vDep_No_E { get; set; }

        /// <summary>
        /// 存單號碼(迄)_異動後
        /// </summary>
        [Description("存單號碼(迄)_異動後")]
        public string vDep_No_E_Aft { get; set; }

        /// <summary>
        /// 存單號碼尾碼
        /// </summary>
        [Description("存單號碼尾碼")]
        public string vDep_No_Tail { get; set; }

        /// <summary>
        /// 存單號碼尾碼_異動後
        /// </summary>
        [Description("存單號碼尾碼_異動後")]
        public string vDep_No_Tail_Aft { get; set; }

        /// <summary>
        /// 存單張數
        /// </summary>
        [Description("存單張數")]
        public int vDep_Cnt { get; set; }

        /// <summary>
        /// 存單張數_異動後
        /// </summary>
        [Description("存單張數_異動後")]
        public int? vDep_Cnt_Aft { get; set; }

        /// <summary>
        /// 單張面額
        /// </summary>
        [Description("單張面額")]
        public decimal vDenomination { get; set; }

        /// <summary>
        /// 單張面額_異動後
        /// </summary>
        [Description("單張面額_異動後")]
        public decimal? vDenomination_Aft { get; set; }

        /// <summary>
        /// 面額小計
        /// </summary>
        [Description("面額小計")]
        public decimal vSubtotal_Denomination { get; set; }

        /// <summary>
        /// 面額小計_異動後
        /// </summary>
        [Description("面額小計_異動後")]
        public decimal? vSubtotal_Denomination_Aft { get; set; }

        /// <summary>
        /// 異動註記
        /// </summary>
        [Description("異動註記")]
        public bool vAftFlag { get; set; }

    }
}