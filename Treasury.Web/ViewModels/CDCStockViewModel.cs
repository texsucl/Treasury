using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(股票)畫面
    /// </summary>
    public class CDCStockViewModel : ICDCItem
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
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }
        /// <summary>
        /// 存取項目冊號資料檔-編號
        /// </summary>
        [Description("存取項目冊號資料檔-編號")]
        public string vIB_Group_No { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-股票名稱
        /// </summary>
        [Description("存取項目冊號資料檔-股票名稱")]
        public string vIB_Name { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-入庫批號
        /// </summary>
        [Description("存取項目冊號資料檔-入庫批號")]
        public int vTrea_Batch_No { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-區域
        /// </summary>
        [Description("存取項目冊號資料檔-區域")]
        public string vIB_Area { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-備註
        /// </summary>
        [Description("存取項目冊號資料檔-備註")]
        public string vIB_Memo { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-總股數
        /// </summary>
        [Description("存取項目冊號資料檔-總股數")]
        public int vNumber_Of_Shares_Total { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-總股數_異動後
        /// </summary>
        [Description("存取項目冊號資料檔-總股數_異動後")]
        public int vNumber_Of_Shares_Total_Aft { get; set; }

        /// <summary>
        /// 股票類型
        /// </summary>
        [Description("股票類型")]
        public string vStock_Type { get; set; }

        /// <summary>
        /// 股票類型_異動後
        /// </summary>
        [Description("股票類型_異動後")]
        public string vStock_Type_Aft { get; set; }

        /// <summary>
        /// 股票序號前置代碼
        /// </summary>
        [Description("股票序號前置代碼")]
        public string vStock_No_Preamble { get; set; }

        /// <summary>
        /// 股票序號前置代碼_異動後
        /// </summary>
        [Description("股票序號前置代碼_異動後")]
        public string vStock_No_Preamble_Aft { get; set; }

        /// <summary>
        /// 股票序號(起)
        /// </summary>
        [Description("股票序號(起)")]
        public string vStock_No_B { get; set; }

        /// <summary>
        /// 股票序號(起)_異動後
        /// </summary>
        [Description("股票序號(起)_異動後")]
        public string vStock_No_B_Aft { get; set; }

        /// <summary>
        /// 股票序號(迄)
        /// </summary>
        [Description("股票序號(迄)")]
        public string vStock_No_E { get; set; }

        /// <summary>
        /// 股票序號(迄)_異動後
        /// </summary>
        [Description("股票序號(迄)_異動後")]
        public string vStock_No_E_Aft { get; set; }

        /// <summary>
        /// 張數
        /// </summary>
        [Description("張數")]
        public int? vStock_Cnt { get; set; }

        /// <summary>
        /// 張數_異動後
        /// </summary>
        [Description("張數_異動後")]
        public int? vStock_Cnt_Aft { get; set; }

        /// <summary>
        /// 每股金額
        /// </summary>
        [Description("每股金額")]
        public decimal? vAmount_Per_Share { get; set; }

        /// <summary>
        /// 每股金額_異動後
        /// </summary>
        [Description("每股金額_異動後")]
        public decimal? vAmount_Per_Share_Aft { get; set; }

        /// <summary>
        /// 單張股數
        /// </summary>
        [Description("單張股數")]
        public decimal? vSingle_Number_Of_Shares { get; set; }

        /// <summary>
        /// 單張股數_異動後
        /// </summary>
        [Description("單張股數_異動後")]
        public decimal? vSingle_Number_Of_Shares_Aft { get; set; }

        /// <summary>
        /// 單張面額
        /// </summary>
        [Description("單張面額")]
        public decimal? vDenomination { get; set; }

        /// <summary>
        /// 單張面額_異動後
        /// </summary>
        [Description("單張面額_異動後")]
        public decimal? vDenomination_Aft { get; set; }

        /// <summary>
        /// 面額小計
        /// </summary>
        [Description("面額小計")]
        public decimal? vDenominationTotal { get; set; }

        /// <summary>
        /// 面額小計_異動後
        /// </summary>
        [Description("面額小計_異動後")]
        public decimal? vDenominationTotal_Aft { get; set; }

        /// <summary>
        /// 股數小計
        /// </summary>
        [Description("股數小計")]
        public decimal? vNumberOfShares { get; set; }

        /// <summary>
        /// 股數小計_異動後
        /// </summary>
        [Description("股數小計_異動後")]
        public decimal? vNumberOfShares_Aft { get; set; }

        /// <summary>
        /// 備註說明
        /// </summary>
        [Description("備註說明")]
        public string vMemo { get; set; }

        /// <summary>
        /// 備註說明_異動後
        /// </summary>
        [Description("備註說明_異動後")]
        public string vMemo_Aft { get; set; }

        /// <summary>
        /// 異動註記
        /// </summary>
        [Description("異動註記")]
        public bool vAftFlag { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Time { get; set; }

    }

}