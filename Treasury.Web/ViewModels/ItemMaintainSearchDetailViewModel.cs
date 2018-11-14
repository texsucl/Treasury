using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemMaintainSearchDetailViewModel: ITinItem
    {
        /// <summary>
        /// 入庫作業類型
        /// </summary>
        [Description("入庫作業類型")]
        public string vITEM_OP_TYPE { get; set; }

        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vITEM_DESC { get; set; }

        /// <summary>
        /// 存取項目代碼
        /// </summary>
        [Description("存取項目代碼")]
        public string vITEM_ID { get; set; }
        
        /// <summary>
        /// 連結資料庫
        /// </summary>
        [Description("連結資料庫")]
        public string vIS_TREA_ITEM { get; set; }

        /// <summary>
        /// 資料庫類別
        /// </summary>
        [Description("資料庫類別")]
        public string vTREA_ITEM_TYPE_VALUE { get; set; }

        /// <summary>
        /// 資料庫類別編號
        /// </summary>
        [Description("資料庫類別編號")]
        public string vTREA_ITEM_TYPE { get; set; }

        /// <summary>
        /// 資料庫代碼
        /// </summary>
        [Description("資料庫代碼")]
        public string vTREA_ITEM_NAME { get; set; }

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        [Description("資料庫名稱")]
        public string vTREA_ITEM_NAME_VALUE { get; set; }

        /// <summary>
        /// 每日進出
        /// </summary>
        [Description("每日進出")]
        public string vISDO_PERDAY { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMEMO { get; set; }

        /// <summary>
        /// 資料狀態代碼
        /// </summary>
        [Description("資料狀態代碼")]
        public string vDATA_STATUS { get; set; }

        /// <summary>
        /// 資料狀態名稱
        /// </summary>
        [Description("資料狀態")]
        public string vDATA_STATUS_VALUE { get; set; }

        /// <summary>
        /// 資料凍結人員名稱
        /// </summary>
        [Description("資料凍結人員名稱")]
        public string vFREEZE_NAME { get; set; }

        /// <summary>
        /// 資料凍結人員代號
        /// </summary>
        [Description("資料凍結人員代號")]
        public string vFREEZE_UID{ get; set; }

        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string vEXEC_ACTION { get; set; }

        /// <summary>
        /// 執行功能名稱
        /// </summary>
        [Description("執行功能名稱")]
        public string vEXEC_ACTION_VALUE { get; set; }

        /// <summary>
        /// 異動申請單號
        /// </summary>
        [Description("異動申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 最後異動日期時間
        /// </summary>
        [Description("最後異動日期時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }
        
    }
}
