using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels 
{
    public class OAP0024Model
    {
        /// <summary>
        /// 來源
        /// </summary>
        [Description("來源")]
        public string srce_from { get; set; }

        /// <summary>
        /// 來源(中文)
        /// </summary>
        [Description("來源(中文)")]
        public string srce_from_D { get; set; }

        /// <summary>
        /// 新增日期
        /// </summary>
        [Description("新增日期")]
        public string entry_date { get; set; }

        /// <summary>
        /// 新增人員
        /// </summary>
        [Description("新增人員")]
        public string entry_id { get; set; }

        /// <summary>
        /// 新增人員(中文)
        /// </summary>
        [Description("新增人員(中文)")]
        public string entry_id_D { get; set; }

        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 票面金額
        /// </summary>
        [Description("票面金額")]
        public string amount { get; set; }

        /// <summary>
        /// 支票到期日
        /// </summary>
        [Description("支票到期日")]
        public string check_date { get; set; }

        /// <summary>
        /// 單號
        /// </summary>
        [Description("單號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 收件人員
        /// </summary>
        [Description("收件人員")]
        public string apply_id  { get; set; }

        /// <summary>
        /// 收件人員(中文)
        /// </summary>
        [Description("收件人員(中文)")]
        public string apply_id_D { get; set; }

        /// <summary>
        /// 收件單位
        /// </summary>
        [Description("收件單位")]
        public string unit_code { get; set; }

        /// <summary>
        /// 收件單位(中文)
        /// </summary>
        [Description("收件單位(中文)")]
        public string unit_code_D { get; set; }

        /// <summary>
        /// 付款對象
        /// </summary>
        [Description("付款對象")]
        public string receiver { get; set; }
    }
}