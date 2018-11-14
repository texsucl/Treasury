using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(空白票據)畫面
    /// </summary>
    public class CDCBillViewModel : ICDCItem
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
        /// 發票行
        /// </summary>
        [Description("發票行")]
        public string vBill_Issuing_Bank { get; set; }

        /// <summary>
        /// 發票行_異動後
        /// </summary>
        [Description("發票行_異動後")]
        public string vBill_Issuing_Bank_AFT { get; set; }

        /// <summary>
        /// 支票類型
        /// </summary>
        [Description("支票類型")]
        public string vBill_Check_Type { get; set; }

        /// <summary>
        /// 支票類型_異動後
        /// </summary>
        [Description("支票類型_異動後")]
        public string vBill_Check_Type_AFT { get; set; }

        /// <summary>
        /// 支票號碼英文字軌
        /// </summary>
        [Description("支票號碼英文字軌")]
        public string vBill_Check_No_Track { get; set; }

        /// <summary>
        /// 支票號碼英文字軌_異動後
        /// </summary>
        [Description("支票號碼英文字軌_異動後")]
        public string vBill_Check_No_Track_AFT { get; set; }

        /// <summary>
        /// 支票號碼(起)
        /// </summary>
        [Description("支票號碼(起)")]
        public string vBill_Check_No_B { get; set; }

        /// <summary>
        /// 支票號碼(起)_異動後
        /// </summary>
        [Description("支票號碼(起)_異動後")]
        public string vBill_Check_No_B_AFT { get; set; }

        /// <summary>
        /// 支票號碼(迄)
        /// </summary>
        [Description("支票號碼(迄)")]
        public string vBill_Check_No_E { get; set; }

        /// <summary>
        /// 支票號碼(迄)_異動後
        /// </summary>
        [Description("支票號碼(迄)_異動後")]
        public string vBill_Check_No_E_AFT { get; set; }

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