using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;


namespace FAP.Web.ViewModels
{
    /// <summary>
    /// 應付票據變更接收明細 支票檔
    /// </summary>
    public class OAP0021DetailSubModel
    {
        /// <summary>
        /// 支票號碼
        /// </summary>
        [Description("支票號碼")]
        public string check_no { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Description("金額")]
        public string amount { get; set; }

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
        /// 支票狀態
        /// </summary>
        [Description("支票狀態")]
        public string check_stat { get; set; }

        /// <summary>
        /// 支票狀態(中文)
        /// </summary>
        [Description("支票狀態(中文)")]
        public string check_stat_D { get; set; }

        /// <summary>
        /// 系統註記
        /// </summary>
        [Description("系統註記")]
        public string proc_mark { get; set; }

        /// <summary>
        /// 系統別
        /// </summary>
        [Description("系統別")]
        public string system { get; set; }

        /// <summary>
        /// 註記
        /// </summary>
        [Description("註記")]
        public string mark_type2 { get; set; }

        /// <summary>
        /// 註記(中文)
        /// </summary>
        [Description("註記(中文)")]
        public string mark_type2_D { get; set; }

        /// <summary>
        /// 新抬頭
        /// </summary>
        [Description("新抬頭")]
        public string new_head { get; set; }

        /// <summary>
        /// 付款帳戶
        /// </summary>
        [Description("付款帳戶")]
        public string bank_code { get; set; }

        /// <summary>
        /// 付款申請編號
        /// </summary>
        [Description("付款申請編號")]
        public string aply_no { get; set; }

        /// <summary>
        /// 付款申請序號
        /// </summary>
        [Description("付款申請序號")]
        public string aply_seq { get; set; }

        /// <summary>
        /// 申請編號
        /// </summary>
        [Description("申請編號")]
        public string apply_no { get; set; }

        /// <summary>
        /// 給付類型
        /// </summary>
        [Description("給付類型")]
        public string pay_class { get; set; }

        /// <summary>
        /// 行政單位
        /// </summary>
        [Description("行政單位")]
        public string adm_unit { get; set; }
    }
}