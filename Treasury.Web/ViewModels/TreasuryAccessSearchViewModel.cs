using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 金庫物品存取主畫面查詢ViewModel
    /// </summary>
    public class TreasuryAccessSearchViewModel
    {
        /// <summary>
        /// 存取項目
        /// </summary>
        [Description("存取項目")]
        public List<string> vItem { get; set; }

        /// <summary>
        /// 申請日期(開始)
        /// </summary>
        [Description("申請日期(開始)")]
        public string vAPLY_DT_S { get; set; }

        /// <summary>
        /// 申請日期(結束)
        /// </summary>
        [Description("申請日期(結束)")]
        public string vAPLY_DT_E { get; set; }

        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]

        public string vAPLY_NO { get; set; }

        /// <summary>
        /// 申請單位
        /// </summary>
        [Description("申請單位")]
        public List<string> vAplyUnit { get; set; }


        /// <summary>
        /// 實際存取日期(開始)
        /// </summary>
        [Description("實際存取日期(開始)")]
        public string vActualAccessDate_S { get; set; }

        /// <summary>
        /// 實際存取日期(結束)
        /// </summary>
        [Description("實際存取日期(結束)")]
        public string vActualAccessDate_E { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        /// <summary>
        /// 填表單位
        /// </summary>
        [Description("填表單位")]
        public string vCreateUnit { get; set; }

        /// <summary>
        /// 填表人員
        /// </summary>
        [Description("填表人員")]
        public string vCreateUid { get; set; }

        /// <summary>
        /// 保管科人員
        /// </summary>
        [Description("保管科人員")]
        public bool vCustodianFlag { get; set; }
    }
}