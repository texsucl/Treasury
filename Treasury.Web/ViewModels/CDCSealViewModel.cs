﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 資料庫異動(印章)畫面
    /// </summary>
    public class CDCSealViewModel : ICDCItem
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
        /// 資料庫名稱
        /// </summary>
        [Description("入庫日期")]
        public string vPUT_Date { get; set; }

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
        public string vCHARGE_DEPT { get; set; }

        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCHARGE_DEPT_Name { get; set; }

        /// <summary>
        /// 權責科別ID
        /// </summary>
        [Description("權責科別ID")]
        public string vCHARGE_SECT { get; set; }

        /// <summary>
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCHARGE_SECT_Name{ get; set; }

        /// <summary>
        /// 權責單位
        /// </summary>
        [Description("權責單位")]
        public string vCHARGE_Name { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSeal_Desc { get; set; }

        /// <summary>
        /// 印章內容_異動後
        /// </summary>
        [Description("印章內容_異動後")]
        public string vSeal_Desc_AFT { get; set; }

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