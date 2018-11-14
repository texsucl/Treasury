using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(不動產權狀)畫面
    /// </summary>
    public class CDCEstateViewModel : ICDCItem
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
        /// 存取項目冊號資料檔-冊號
        /// </summary>
        [Description("存取項目冊號資料檔-冊號")]
        public string vIB_Book_No { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-大樓名稱
        /// </summary>
        [Description("存取項目冊號資料檔-大樓名稱")]
        public string vIB_Building_Name { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-坐落
        /// </summary>
        [Description("存取項目冊號資料檔-坐落")]
        public string vIB_Located { get; set; }

        /// <summary>
        /// 存取項目冊號資料檔-備註
        /// </summary>
        [Description("存取項目冊號資料檔-備註")]
        public string vIB_Memo { get; set; }

        /// <summary>
        /// 狀別
        /// </summary>
        [Description("狀別")]
        public string vEstate_Form_No { get; set; }

        /// <summary>
        /// 狀別_異動後
        /// </summary>
        [Description("狀別_異動後")]
        public string vEstate_Form_No_Aft { get; set; }

        /// <summary>
        /// 發狀日
        /// </summary>
        [Description("發狀日")]
        public string vEstate_Date { get; set; }

        /// <summary>
        /// 發狀日_異動後
        /// </summary>
        [Description("發狀日_異動後")]
        public string vEstate_Date_Aft { get; set; }

        /// <summary>
        /// 字號
        /// </summary>
        [Description("字號")]
        public string vOwnership_Cert_No { get; set; }

        /// <summary>
        /// 字號_異動後
        /// </summary>
        [Description("字號_異動後")]
        public string vOwnership_Cert_No_Aft { get; set; }

        /// <summary>
        /// 地/建號
        /// </summary>
        [Description("地/建號")]
        public string vLand_Building_No { get; set; }

        /// <summary>
        /// 地/建號_異動後
        /// </summary>
        [Description("地/建號_異動後")]
        public string vLand_Building_No_Aft { get; set; }

        /// <summary>
        /// 門牌號
        /// </summary>
        [Description("門牌號")]
        public string vHouse_No { get; set; }

        /// <summary>
        /// 門牌號_異動後
        /// </summary>
        [Description("門牌號_異動後")]
        public string vHouse_No_Aft { get; set; }

        /// <summary>
        /// 流水號/編號
        /// </summary>
        [Description("流水號/編號")]
        public string vEstate_Seq { get; set; }

        /// <summary>
        /// 流水號/編號_異動後
        /// </summary>
        [Description("流水號/編號_異動後")]
        public string vEstate_Seq_Aft { get; set; }

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