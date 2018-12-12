using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class ItemChargeUnitInsertViewModel
    {
        /// <summary>
        /// 保管單位存取項目編號
        /// </summary>
        [Description("保管單位存取項目編號")]
        public string vCHARGE_UNIT_ID { get; set; }

        /// <summary>
        /// 資料庫名稱代碼
        /// </summary>
        [Description("資料庫名稱代碼")]
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
        /// 權責科別
        /// </summary>
        [Description("權責科別")]
        public string vCHARGE_SECT { get; set; }
        /// <summary>
        /// 經辦
        /// </summary>
        [Description("經辦")]
        public string vCHARGE_UID { get; set; }

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
        /// 停用註記
        /// </summary>
        [Description("停用註記")]
        public string vIs_Disabled { get; set; }
    }
}