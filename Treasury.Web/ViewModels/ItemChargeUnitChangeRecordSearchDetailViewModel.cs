using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemChargeUnitChangeRecordSearchDetailViewModel : ITinItem
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
        /// 經辦
        /// </summary>
        [Description("經辦")]
        public string vCHARGE_UID { get; set; }

        /// <summary>
        /// 經辦(修改前)
        /// </summary>
        [Description("經辦(修改前)")]
        public string vCHARGE_UID_B { get; set; }

        /// <summary>
        /// mail部主管
        /// </summary>
        [Description("mail部主管")]
        public string vIS_MAIL_DEPT_MGR { get; set; }

        /// <summary>
        /// mail部主管(修改前)
        /// </summary>
        [Description("mail部主管(修改前)")]
        public string vIS_MAIL_DEPT_MGR_B { get; set; }

        /// <summary>
        /// mail科主管
        /// </summary>
        [Description("mail科主管")]
        public string vIS_MAIL_SECT_MGR { get; set; }

        /// <summary>
        /// mail科主管(修改前)
        /// </summary>
        [Description("mail科主管(修改前)")]
        public string vIS_MAIL_SECT_MGR_B { get; set; }

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

        /// <summary>
        /// 保管單位存取項目編號
        /// </summary>
        [Description("保管單位存取項目編號")]
        public string vCHARGE_UNIT_ID { get; set; }

        /// <summary>
        /// 權責部門
        /// </summary>
        [Description("權責部門")]
        public string vCHARGE_DEPT_VALUE { get; set; }

        /// <summary>
        /// 權責單位
        /// </summary>
        [Description("權責單位")]
        public string vCHARGE_SECT_VALUE { get; set; }

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        [Description("資料庫名稱")]
        public string vTREA_ITEM_NAME_VALUE { get; set; }
    }
}