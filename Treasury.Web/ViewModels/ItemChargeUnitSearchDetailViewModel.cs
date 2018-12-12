using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemChargeUnitSearchDetailViewModel : ITinItem
    {
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
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCHARGE_DEPT { get; set; }

        /// <summary>
        /// 權責部門名稱
        /// </summary>
        [Description("權責部門名稱")]
        public string vCHARGE_DEPT_VALUE { get; set; }

        /// <summary>
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCHARGE_SECT { get; set; }

        /// <summary>
        /// 權責科別名稱
        /// </summary>
        [Description("權責科別名稱")]
        public string vCHARGE_SECT_VALUE { get; set; }

        /// <summary>
        /// mail部主管
        /// </summary>
        [Description("mail部主管")]
        public string vIS_MAIL_DEPT_MGR { get; set; }

        /// <summary>
        /// mail科主管
        /// </summary>
        [Description("mail科主管")]
        public string vIS_MAIL_SECT_MGR { get; set; }

        /// <summary>
        /// 經辦
        /// </summary>
        [Description("經辦")]
        public string vCHARGE_UID { get; set; }

        /// <summary>
        /// 經辦名稱
        /// </summary>
        [Description("經辦名稱")]
        public string vCHARGE_NAME { get; set; }

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
        public string vFREEZE_UID { get; set; }

        /// <summary>
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIS_DISABLED { get; set; }

        /// <summary>
        /// 保管單位存取項目編號
        /// </summary>
        [Description("保管單位存取項目編號")]
        public string vCHARGE_UNIT_ID { get; set; }

        /// <summary>
        /// 最後異動日期時間
        /// </summary>
        [Description("最後異動日期時間")]
        public DateTime? vLAST_UPDATE_DT { get; set; }

        /// <summary>
        /// 異動申請單號
        /// </summary>
        [Description("異動申請單號")]
        public string vAPLY_NO { get; set; }
    }
}