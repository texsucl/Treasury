﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Treasury.Web.ViewModels
{
    /// <summary>
    /// 金庫物品存取主畫面ViewModel
    /// </summary>
    public class TreasuryAccessViewModel
    {
        /// <summary>
        /// 申請單號
        /// </summary>
        [Description("申請單號")]
        public string vAplyNo { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        [Description("申請項目")]
        public string vItem { get; set; }

        /// <summary>
        /// 申請單位
        /// </summary>
        [Description("申請單位")]
        public string vAplyUnit { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string vAplyUid { get; set; }

        /// <summary>
        /// 申請原因
        /// </summary>
        [Description("申請原因")]
        public string vAccessReason { get; set; }

        /// <summary>
        /// 預計存取日期
        /// </summary>
        [Description("預計存取日期")]
        public string vExpectedAccessDate { get; set; }

        /// <summary>
        /// 申請作業
        /// </summary>
        [Description("申請作業")]
        public string vAccessType { get; set; }

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
    }
}