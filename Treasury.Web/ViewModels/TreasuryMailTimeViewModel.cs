using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailTimeViewModel : ITinItem
    {
        /// <summary>
        /// 執行功能
        /// </summary>
        [Description("執行功能")]
        public string vAction { get; set; }

        /// <summary>
        /// 發送時間
        /// </summary>
        [Description("發送時間")]
        public string vSEND_TIME { get; set; }

        /// <summary>
        /// 程式編號
        /// </summary>
        [Description("程式編號")]
        public string vFUNC_ID { get; set; }

        /// <summary>
        /// 間隔時間
        /// </summary>
        [Description("間隔時間")]
        public string vINTERVAL_MIN { get; set; }

        /// <summary>
        /// 時間(起)
        /// </summary>
        [Description("時間(起)")]
        public string vEXEC_TIME_B { get; set; }

        /// <summary>
        /// 時間(迄)
        /// </summary>
        [Description("時間(迄)")]
        public string vEXEC_TIME_E { get; set; }

        /// <summary>
        /// 開庫時間
        /// </summary>
        [Description("開庫時間")]
        public string vTREA_OPEN_TIME { get; set; }

        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Description("備註")]
        public string vMEMO { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 停用註記(中文)
        /// </summary>
        [Description("停用註記(中文)")]
        public string vIS_DISABLED_NAME { get; set; }

        /// <summary>
        /// 資料狀態
        /// </summary>
        [Description("資料狀態")]
        public string vDATA_STATUS { get; set; }

        /// <summary>
        /// 資料狀態(中文)
        /// </summary>
        [Description("資料狀態(中文)")]
        public string vDATA_STATUS_NAME { get; set; }

        /// <summary>
        /// 資料凍結人員
        /// </summary>
        [Description("資料凍結人員")]
        public string vFREEZE_UID_Name { get; set; }

        /// <summary>
        /// 最後異動人員
        /// </summary>
        [Description("最後異動人員")]
        public string vLAST_UPDATE_UID_Name { get; set; }

        /// <summary>
        /// 最後異動時間(顯示)
        /// </summary>
        [Description("最後異動時間(顯示)")]
        public string vLAST_UPDATE_DT_Show { get; set; }

        /// <summary>
        /// 發送時間定義編號 
        /// </summary>
        [Description("發送時間定義編號")]
        public string vMAIL_TIME_ID {get;set;}

        /// <summary>
        /// 申請單單號
        /// </summary>
        [Description("申請單單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// 修改判定
        /// </summary>
        [Description("修改判定")]
        public bool updateFlag { get; set; }

        /// <summary>
        /// 最後異動時間 
        /// </summary>
        [Description("最後異動時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }
    }
}