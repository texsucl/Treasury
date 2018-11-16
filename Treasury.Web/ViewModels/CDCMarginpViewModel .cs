using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(存入保證金)畫面
    /// </summary>
    public class CDCMarginpViewModel : ICDCItem
    {
        /// <summary>
        /// 物品單號
        /// </summary>
        [Description("物品單號")]
        public string vItem_Id { get; set; }

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
        /// 冊號
        /// </summary>
        [Description("冊號")]
        public string vBook_No { get; set; }

        /// <summary>
        /// 冊號_異動後
        /// </summary>
        [Description("冊號_異動後")]
        public string vBook_No_AFT { get; set; }

        /// <summary>
        /// 存入申請人
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
        /// 存入保證金類別
        /// </summary>
        [Description("存入保證金類別")]
        public string vMargin_Take_Of_Type { get; set; }

        /// <summary>
        /// 存入保證金類別_異動後
        /// </summary>
        [Description("存入保證金類別_異動後")]
        public string vMargin_Take_Of_Type_AFT { get; set; }

        /// <summary>
        /// 交易對象
        /// </summary>
        [Description("交易對象")]
        public string vTrad_Partners { get; set; }

        /// <summary>
        /// 交易對象_異動後
        /// </summary>
        [Description("交易對象_異動後")]
        public string vTrad_Partners_AFT { get; set; }

        /// <summary>
        /// 歸檔編號
        /// </summary>
        [Description("歸檔編號")]
        public string vlItem_Id { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public decimal? vAmount { get; set; }

        /// <summary>
        /// 金額_異動後
        /// </summary>
        [Description("金額_異動後")]
        public decimal? vAmount_AFT { get; set; }

        /// <summary>
        /// 物品名稱
        /// </summary>
        [Description("物品名稱")]
        public string vMargin_Item { get; set; }

        /// <summary>
        /// 物品名稱_異動後
        /// </summary>
        [Description("物品名稱_異動後")]
        public string vMargin_Item_AFT { get; set; }

        /// <summary>
        /// 物品發行人
        /// </summary>
        [Description("物品發行人")]
        public string vMargin_Item_Issuer { get; set; }

        /// <summary>
        /// 物品發行人_異動後
        /// </summary>
        [Description("物品發行人_異動後")]
        public string vMargin_Item_Issuer_AFT { get; set; }

        /// <summary>
        /// 質押標的號碼
        /// </summary>
        [Description("質押標的號碼")]
        public string vPledge_Item_No { get; set; }

        /// <summary>
        /// 質押標的號碼_異動後
        /// </summary>
        [Description("質押標的號碼_異動後")]
        public string vPledge_Item_No_AFT { get; set; }

        /// <summary>
        /// 有效期間(起)
        /// </summary>
        [Description("有效期間(起)")]
        public string vEffective_Date_B { get; set; }

        /// <summary>
        /// 有效期間(起)_異動後
        /// </summary>
        [Description("有效期間(起)_異動後")]
        public string vEffective_Date_B_AFT { get; set; }

        /// <summary>
        /// 有效期間(迄)
        /// </summary>
        [Description("有效期間(迄)")]
        public string vEffective_Date_E { get; set; }

        /// <summary>
        /// 有效期間(迄)_異動後
        /// </summary>
        [Description("有效期間(迄)_異動後")]
        public string vEffective_Date_E_AFT { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Description("說明")]
        public string vDescription { get; set; }

        /// <summary>
        /// 說明_異動後
        /// </summary>
        [Description("說明_異動後")]
        public string vDescription_AFT { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 備註_異動後
        /// </summary>
        [Description("備註_異動後")]
        public string vMemo_AFT { get; set; }

        /// <summary>
        /// 取出註記
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