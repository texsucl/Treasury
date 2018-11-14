using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(電子憑證)畫面
    /// </summary>
    public class CDCCAViewModel : ICDCItem
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
        public string vPUT_Date { get; set; }

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
        public string vAPLY_UID { get; set; }

        /// <summary>
        /// 存入申請人
        /// </summary>
        [Description("存入申請人")]
        public string vAPLY_UID_Name { get; set; }

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
        /// 用途
        /// </summary>
        [Description("用途")]
        public string vCA_Use { get; set; }

        /// <summary>
        /// 用途_異動後
        /// </summary>
        [Description("用途_異動後")]
        public string vCA_Use_AFT { get; set; }

        /// <summary>
        /// 類型
        /// </summary>
        [Description("類型")]
        public string vCA_Desc { get; set; }

        /// <summary>
        /// 類型_異動後
        /// </summary>
        [Description("類型_異動後")]
        public string vCA_Desc_AFT { get; set; }

        /// <summary>
        /// 銀行
        /// </summary>
        [Description("銀行")]
        public string vCA_Bank { get; set; }

        /// <summary>
        /// 銀行_異動後
        /// </summary>
        [Description("銀行_異動後")]
        public string vCA_Bank_AFT { get; set; }

        /// <summary>
        /// 號碼
        /// </summary>
        [Description("號碼")]
        public string vCA_Number { get; set; }

        /// <summary>
        /// 號碼_異動後
        /// </summary>
        [Description("號碼_異動後")]
        public string vCA_Number_AFT { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vCA_Memo { get; set; }

        /// <summary>
        /// 備註_異動後
        /// </summary>
        [Description("備註_異動後")]
        public string vCA_Memo_AFT { get; set; }

        /// <summary>
        /// 異動註記
        /// </summary>
        [Description("異動註記")]
        public bool vAFTFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }
    }
}