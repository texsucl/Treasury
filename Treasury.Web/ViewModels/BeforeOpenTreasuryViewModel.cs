using System;
using System.Collections.Generic;
using System.ComponentModel;
using Treasury.WebUtility;

namespace Treasury.Web.ViewModels
{
    public class BeforeOpenTreasuryViewModel
    {
        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public string vItem_Desc { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSeal_Desc { get; set; }

        /// <summary>
        /// 作業別
        /// </summary>
        [Description("作業別")]
        public string vAccess_Type { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 入庫原因
        /// </summary>
        [Description("入庫原因")]
        public string vAccess_Reason { get; set; }

        /// <summary>
        /// 確認人員
        /// </summary>
        [Description("確認人員")]
        public string vConfirm { get; set; }
    }

    public class BeforeOpenTreasurySeal
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAply_No { get; set; }

        /// <summary>
        /// 物品編號
        /// </summary>
        [Description("物品編號")]
        public string vItem_Id { get; set; }

        /// <summary>
        /// 印章內容
        /// </summary>
        [Description("印章內容")]
        public string vSeal_Desc { get; set; }
    }

    public class TreaOpenRec
    {
        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTreaRegisterId { get; set; }

        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOpenTreaTypeName { get; set; }
    }
}