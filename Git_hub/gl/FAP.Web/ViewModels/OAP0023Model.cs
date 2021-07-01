using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0023Model
    {
        public OAP0023Model() {
            checkFlag = true;
        }
        public bool checkFlag { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 票面金額
        /// </summary>
        [Description("票面金額")]
        public string check_mount { get; set; }

        /// <summary>
        /// 付款對象
        /// </summary>
        [Description("付款對象")]
        public string receiver { get; set; }

        /// <summary>
        /// 支票到期日
        /// </summary>
        [Description("支票到期日")]
        public string check_date { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型")]
        public string pay_class { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型D")]
        public string pay_class_text { get; set; }

        /// <summary>
        /// 行政單位
        /// </summary>
        [Description("行政單位")]
        public string adm_unit { get; set; }

        /// <summary>
        /// 帳戶簡稱
        /// </summary>
        [Description("帳戶簡稱")]
        public string acct_abbr { get; set; }

        /// <summary>
        /// 簽收檔尚未簽收
        /// </summary>
        [Description("簽收檔尚未簽收")]
        public string flag  { get; set; }

        /// <summary>
        /// 簽收檔尚未簽收D
        /// </summary>
        [Description("簽收檔尚未簽收D")]
        public string flag_D { get; set; }
    }
}