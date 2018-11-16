using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(重要物品)畫面
    /// </summary>
    public class CDCItemImpViewModel : ICDCItem
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
        /// 歸檔編號        
        /// /// </summary>
        [Description("歸檔編號")]
        public string vlItem_Id { get; set; }

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
        /// 重要物品名稱
        /// </summary>
        [Description("重要物品名稱")]
        public string vItemImp_Name { get; set; }

        /// <summary>
        /// 重要物品名稱_異動後
        /// </summary>
        [Description("重要物品名稱_異動後")]
        public string vItemImp_Name_AFT { get; set; }

        /// <summary>
        /// 重要物品數量
        /// </summary>
        [Description("重要物品數量")]
        public int? vItemImp_Quantity { get; set; }

        /// <summary>
        /// 重要物品剩餘數量
        /// </summary>
        [Description("重要物品剩餘數量")]
        public int vItemImp_Remaining { get; set; }

        /// <summary>
        /// 重要物品數量_異動後
        /// </summary>
        [Description("重要物品數量_異動後")]
        public int? vItemImp_Remaining_AFT { get; set; }

        /// <summary>
        /// 重要物品金額
        /// </summary>
        [Description("重要物品金額")]
        public decimal? vItemImp_Amount { get; set; }

        /// <summary>
        /// 重要物品金額_異動後
        /// </summary>
        [Description("重要物品金額_異動後")]
        public decimal? vItemImp_Amount_AFT { get; set; }

        /// <summary>
        /// 重要物品預計提取日期
        /// </summary>
        [Description("重要物品預計提取日期")]
        public string vItemImp_Expected_Date { get; set; }

        /// <summary>
        /// 重要物品預計提取日期_異動後
        /// </summary>
        [Description("重要物品預計提取日期_異動後")]
        public string vItemImp_Expected_Date_AFT { get; set; }

        /// <summary>
        /// 說明
        /// </summary>
        [Description("說明")]
        public string vItemImp_Description { get; set; }

        /// <summary>
        /// 說明_異動後
        /// </summary>
        [Description("說明_異動後")]
        public string vItemImp_Description_AFT { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vItemImp_MEMO { get; set; }

        /// <summary>
        /// 備註_異動後
        /// </summary>
        [Description("備註_異動後")]
        public string vItemImp_MEMO_AFT { get; set; }

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