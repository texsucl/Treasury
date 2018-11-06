using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    public class AlreadyConfirmedSearchViewModel
    {
        /// <summary>
        /// 日期(起)
        /// </summary>
        [Description("日期(起)")]
        public string vDT_From { get; set; }

        /// <summary>
        /// 申請日期(迄)
        /// </summary>
        [Description("申請日期(迄)")]
        public string vDT_To { get; set; }

        /// <summary>
        /// 金庫登記簿單號
        /// </summary>
        [Description("金庫登記簿單號")]
        public string vTREA_REGISTER_ID { get; set; }

        ///// <summary>
        ///// 是否確認
        ///// </summary>
        //[Description("是否確認")]
        //public string vIsConfirmed { get; set; }

        /// <summary>
        /// 開庫類型
        /// </summary>
        [Description("開庫類型")]
        public string vOPEN_TREA_TYPE { get; set; }

        /// <summary>
        /// 確認人員
        /// </summary>
        [Description("確認人員")]
        public string vConfirm_Id { get; set; }
    }
}