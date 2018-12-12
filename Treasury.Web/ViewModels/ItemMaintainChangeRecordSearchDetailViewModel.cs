using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemMaintainChangeRecordSearchDetailViewModel: ITinItem
    {
        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string vFreeze_Dt { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vFreeze_Uid_Name { get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string vExec_Action_Name { get; set; }

        /// <summary>
        /// 連結資料庫
        /// </summary>
        [Description("連結資料庫")]
        public string vIS_TREA_ITEM { get; set; }

        /// <summary>
        /// 連結資料庫(修改前)
        /// </summary>
        [Description("連結資料庫(修改前)")]
        public string vIS_TREA_ITEM_B { get; set; }

        /// <summary>
        /// 每日進出
        /// </summary>
        [Description("每日進出")]
        public string vDAILY_FLAG { get; set; }

        /// <summary>
        /// 每日進出(修改前)
        /// </summary>
        [Description("每日進出(修改前)")]
        public string vDAILY_FLAG_B { get; set; }

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        [Description("資料庫名稱")]
        public string vTREA_ITEM_NAME { get; set; }

        /// <summary>
        /// 資料庫名稱(修改前)
        /// </summary>
        [Description("資料庫名稱(修改前)")]
        public string vTREA_ITEM_NAME_B { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 停用註記(修改前)
        /// </summary>
        [Description("停用註記(修改前)")]
        public string vIS_DISABLED_B { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMEMO { get; set; }

        /// <summary>
        /// 備註(修改前)
        /// </summary>
        [Description("備註(修改前)")]
        public string vMEMO_B { get; set; }

        /// <summary>
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAPPR_STATUS { get; set; }

        /// <summary>
        /// 覆核意見
        /// </summary>
        [Description("覆核意見")]
        public string vAPPR_DESC { get; set; }

        /// <summary>
        /// 存取作業代碼
        /// </summary>
        [Description("存取作業代碼")]
        public string vITEM_ID { get; set; }

        /// <summary>
        /// 存取作業
        /// </summary>
        [Description("存取作業")]
        public string vITEM_DESC { get; set; }
    }
}