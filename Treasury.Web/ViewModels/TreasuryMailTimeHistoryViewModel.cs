using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailTimeHistoryViewModel : ITinItem
    {
        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string vAPLY_DATE { get; set; }

        /// <summary>
        /// 工作單號
        /// </summary>
        [Description("工作單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string vAPLY_UID_Name { get; set; }

        /// <summary>
        /// 執行
        /// </summary>
        [Description("執行")]
        public string Act{get;set;}

        /// <summary>
        /// 發送時間
        /// </summary>
        [Description("發送時間")]
        public string vSEND_TIME { get; set; }

        /// <summary>
        /// 發送時間(修改前)
        /// </summary>
        [Description("發送時間(修改前)")]
        public string vSEND_TIME_B { get; set; }

        /// <summary>
        /// 程式編號
        /// </summary>
        [Description("程式編號")]
        public string vFUNC_ID { get; set; }

        /// <summary>
        /// 程式編號(修改前)
        /// </summary>
        [Description("程式編號(修改前)")]
        public string vFUNC_ID_B { get; set; }

        /// <summary>
        /// 間隔時間
        /// </summary>
        [Description("間隔時間")]
        public string vINTERVAL_MIN { get; set; }

        /// <summary>
        /// 間隔時間(修改前)
        /// </summary>
        [Description("間隔時間(修改前)")]
        public string vINTERVAL_MIN_B { get; set; }

        /// <summary>
        /// 時間(起)
        /// </summary>
        [Description("時間(起)")]
        public string vEXEC_TIME_B { get; set; }

        /// <summary>
        /// 時間(起)(修改前)
        /// </summary>
        [Description("時間(起)(修改前)")]
        public string vEXEC_TIME_B_B { get; set; }

        /// <summary>
        /// 時間(迄)
        /// </summary>
        [Description("時間(迄)")]
        public string vEXEC_TIME_E { get; set; }

        /// <summary>
        /// 時間(迄)(修改前)
        /// </summary>
        [Description("時間(迄)(修改前)")]
        public string vEXEC_TIME_E_B { get; set; }

        /// <summary>
        /// 開庫時間
        /// </summary>
        [Description("開庫時間")]
        public string vTREA_OPEN_TIME { get; set; }

        /// <summary>
        /// 開庫時間(修改前)
        /// </summary>
        [Description("開庫時間(修改前)")]
        public string vTREA_OPEN_TIME_B { get; set; }

        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 內文編號(修改前)
        /// </summary>
        [Description("內文編號(修改前)")]
        public string vMAIL_CONTENT_ID_B { get; set; }


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
        /// 覆核狀態
        /// </summary>
        [Description("覆核狀態")]
        public string vAPPR_STATUS { get; set; }

        /// <summary>
        /// 覆核意見
        /// </summary>
        [Description("覆核意見")]
        public string vAPPR_DESC { get; set; }
    }
}