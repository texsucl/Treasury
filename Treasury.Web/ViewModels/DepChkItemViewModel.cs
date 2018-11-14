using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class DepChkItemViewModel : ITinItem
    {
        /// <summary>
        /// 交易別
        /// </summary>
        [Description("交易別")]
        public string vAccess_Type { get; set; }

        /// <summary>
        /// 資料順序
        /// </summary>
        [Description("資料順序")]
        public int vIsortby { get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string vExec_Action { get; set; }

        /// <summary>
        /// 執行功能名稱
        /// </summary>
        [Description("執行功能名稱")]
        public string vExec_Action_Name { get; set; }

        /// <summary>
        /// 檢核項目說明
        /// </summary>
        [Description("檢核項目說明")]
        public string vDep_Chk_Item_Desc { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 項目順序
        /// </summary>
        [Description("項目順序")]
        public int vItem_Order { get; set; }

        /// <summary>
        /// 取代變數
        /// </summary>
        [Description("取代變數")]
        public string vReplace { get; set; }

        /// <summary>
        /// 資料狀態
        /// </summary>
        [Description("資料狀態")]
        public string vData_Status { get; set; }

        /// <summary>
        /// 資料狀態名稱
        /// </summary>
        [Description("資料狀態名稱")]
        public string vData_Status_Name { get; set; }

        /// <summary>
        /// 最後更新時間 (庫存資料才有)
        /// </summary>
        [Description("最後更新時間")]
        public DateTime? vLast_Update_Dt { get; set; }

        /// <summary>
        /// 資料凍結人員姓名
        /// </summary>
        [Description("資料凍結人員姓名")]
        public string vFreeze_Uid_Name { get; set; }

        /// <summary>
        /// 異動申請單號
        /// </summary>
        [Description("異動申請單號")]
        public string vAply_No { get; set; }
    }

    public class DepChkItemChangeRecordViewModel : ITinItem
    {
        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string vAply_Date { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vAply_Uid_Name { get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string vExec_Action_Name { get; set; }

        /// <summary>
        /// 檢核項目說明
        /// </summary>
        [Description("檢核項目說明")]
        public string vDep_Chk_Item_Desc { get; set; }

        /// <summary>
        /// 檢核項目說明(修改前)
        /// </summary>
        [Description("檢核項目說明(修改前)")]
        public string vDep_Chk_Item_Desc_B { get; set; }

        /// <summary>
        /// 停用註記名稱
        /// </summary>
        [Description("停用註記名稱")]
        public string vIs_Disabled_Name { get; set; }

        /// <summary>
        /// 停用註記名稱(修改前)
        /// </summary>
        [Description("停用註記名稱(修改前)")]
        public string vIs_Disabled_B_Name { get; set; }

        /// <summary>
        /// 項目順序
        /// </summary>
        [Description("項目順序")]
        public int vItem_Order { get; set; }

        /// <summary>
        /// 項目順序(修改前)
        /// </summary>
        [Description("項目順序(修改前)")]
        public int? vItem_Order_B { get; set; }

        /// <summary>
        /// 取代變數
        /// </summary>
        [Description("取代變數")]
        public string vReplace { get; set; }

        /// <summary>
        /// 取代變數(修改前)
        /// </summary>
        [Description("取代變數(修改前)")]
        public string vReplace_B { get; set; }

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAppr_Status_Name { get; set; }

        /// <summary>
        /// 覆核意見
        /// </summary>
        [Description("覆核意見")]
        public string vAppr_Desc { get; set; }
    }

    public class DepChkItemSearchViewModel : ITinItem
    {
        /// <summary>
        /// 停用狀態
        /// </summary>
        [Description("停用狀態")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vLast_Update_Uid { get; set; }
    }

    public class DepChkItemChangeRecordSearchViewModel : ITinItem
    {
        /// <summary>
        /// 交易別
        /// </summary>
        [Description("交易別")]
        public string vAccess_Type { get; set; }

        /// <summary>
        /// 資料順序
        /// </summary>
        [Description("資料順序")]
        public int vIsortby { get; set; }
    }
}