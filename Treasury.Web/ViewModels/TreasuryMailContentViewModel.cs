using System;
using System.ComponentModel;

namespace Treasury.Web.ViewModels
{
    public class TreasuryMailContentViewModel : ITinItem
    {
        /// <summary>
        /// 內文編號
        /// </summary>
        [Description("內文編號")]
        public string vMAIL_CONTENT_ID { get; set; }

        /// <summary>
        /// 發送主旨
        /// </summary>
        [Description("發送主旨")]
        public string vMAIL_SUBJECT { get; set; }

        /// <summary>
        /// 發送內文
        /// </summary>
        [Description("發送內文")]
        public string vMAIL_CONTENT { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 停用註記(中文)
        /// </summary>
        [Description("停用註記(中文)")]
        public string vIS_DISABLED_D { get; set; }

        /// <summary>
        /// 庫存狀態
        /// </summary>
        [Description("庫存狀態")]
        public string vStatus { get; set; }

        /// <summary>
        /// 庫存狀態(中文)
        /// </summary>
        [Description("庫存狀態(中文)")]
        public string vStatus_D { get; set; }

        /// <summary>
        /// 申請單單號
        /// </summary>
        [Description("申請單單號")]
        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 資料凍結Id
        /// </summary>
        [Description("資料凍結Id")]
        public string vFREEZE_UID { get; set; }

        /// <summary>
        /// 資料凍結人員
        /// </summary>
        [Description("資料凍結人員")]
        public string vFREEZE_UID_Name { get; set; }

        /// <summary>
        /// 最後異動Id
        /// </summary>
        [Description("最後異動Id")]
        public string vLAST_UPDATE_UID { get; set; }

        /// <summary>
        /// 最後異動人員
        /// </summary>
        [Description("最後異動人員")]
        public string vLAST_UPDATE_UID_Name { get; set; }

        /// <summary>
        /// 最後異動日期
        /// </summary>
        [Description("最後異動日期")]
        public string vLAST_UPDATE_DATE { get; set; }

        /// <summary>
        /// 最後異動時間 
        /// </summary>
        [Description("最後異動時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }
    }
}