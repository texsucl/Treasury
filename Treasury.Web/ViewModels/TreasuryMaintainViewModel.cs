using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMaintainViewModel: ITinItem
    {
        /// <summary>
        /// 金庫設備編號
        /// </summary>
        [Description("金庫設備編號")]
        public string vTrea_Equip_Id { get; set; }

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
        /// 設備名稱
        /// </summary>
        [Description("設備名稱")]
        public string vEquip_Name { get; set; }

        /// <summary>
        /// 控管模式
        /// </summary>
        [Description("控管模式")]
        public string vControl_Mode { get; set; }

        /// <summary>
        /// 常用數量
        /// </summary>
        [Description("常用數量")]
        public int? vNormal_Cnt { get; set; }

        /// <summary>
        /// 備份數量
        /// </summary>
        [Description("備份數量")]
        public int? vReserve_Cnt { get; set; }

        /// <summary>
        /// 總數量
        /// </summary>
        [Description("總數量")]
        public int? vSum_Cnt { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

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

    public class TreasuryMaintainChangeRecordViewModel : ITinItem
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
        /// 設備名稱
        /// </summary>
        [Description("設備名稱")]
        public string vEquip_Name { get; set; }

        /// <summary>
        /// 設備名稱(修改前)
        /// </summary>
        [Description("設備名稱(修改前)")]
        public string vEquip_Name_B { get; set; }

        /// <summary>
        /// 控管模式名稱
        /// </summary>
        [Description("控管模式名稱")]
        public string vControl_Mode_Name { get; set; }

        /// <summary>
        /// 控管模式名稱(修改前)
        /// </summary>
        [Description("控管模式(修改前)")]
        public string vControl_Mode_B_Name { get; set; }

        /// <summary>
        /// 常用數量
        /// </summary>
        [Description("常用數量")]
        public int? vNormal_Cnt { get; set; }

        /// <summary>
        /// 常用數量(修改前)
        /// </summary>
        [Description("常用數量(修改前)")]
        public int? vNormal_Cnt_B { get; set; }

        /// <summary>
        /// 備份數量
        /// </summary>
        [Description("備份數量")]
        public int? vReserve_Cnt { get; set; }

        /// <summary>
        /// 備份數量(修改前)
        /// </summary>
        [Description("備份數量(修改前)")]
        public int? vReserve_Cnt_B { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMemo { get; set; }

        /// <summary>
        /// 備註(修改前)
        /// </summary>
        [Description("備註(修改前)")]
        public string vMemo_B { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 停用註記(修改前)
        /// </summary>
        [Description("停用註記(修改前)")]
        public string vIs_Disabled_B { get; set; }

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

    public class TreasuryMaintainSearchViewModel : ITinItem
    {
        /// <summary>
        /// 控管模式
        /// </summary>
        [Description("控管模式")]
        public string vControl_Mode { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vLast_Update_Uid { get; set; }
    }

    public class TreasuryMaintainChangeRecordSearchViewModel : ITinItem
    {
        /// <summary>
        /// 控管模式
        /// </summary>
        [Description("控管模式")]
        public string vControl_Mode { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAppr_Status { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vLast_Update_Uid { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }

        /// <summary>
        /// 金庫設備編號
        /// </summary>
        [Description("金庫設備編號")]
        public string vTrea_Equip_Id { get; set; }
    }
}