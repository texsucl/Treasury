using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL00010ViewSubModel
    {
        /// <summary>
        /// 退費項目類別
        /// </summary>
        [Description("退費項目類別")]
        public string pay_class { get; set; }

        /// <summary>
        /// 保費類別_舊
        /// </summary>
        [Description("保費類別_舊")]
        public string prem_kind_o { get; set; }

        /// <summary>
        /// 保費類別_舊_中文
        /// </summary>
        [Description("保費類別_舊_中文")]
        public string prem_kind_o_D { get; set; }

        /// <summary>
        /// 保費類別_新
        /// </summary>
        [Description("保費類別_新")]
        public string prem_kind_n { get; set; }

        /// <summary>
        /// 保費類別_新_中文
        /// </summary>
        [Description("保費類別_新_中文")]
        public string prem_kind_n_D { get; set; }


        /// <summary>
        /// 合約別_舊
        /// </summary>
        [Description("合約別_舊")]
        public string cont_type_o { get; set; }

        /// <summary>
        /// 合約別_舊_中文
        /// </summary>
        [Description("合約別_舊_中文")]
        public string cont_type_o_D { get; set; }

        /// <summary>
        /// 合約別_新
        /// </summary>
        [Description("合約別_新")]
        public string cont_type_n { get; set; }

        /// <summary>
        /// 合約別_新_中文
        /// </summary>
        [Description("合約別_新_中文")]
        public string cont_type_n_D { get; set; }

        /// <summary>
        /// 商品別_舊
        /// </summary>
        [Description("商品別_舊")]
        public string prod_type_o { get; set; }

        /// <summary>
        /// 商品別_舊_中文
        /// </summary>
        [Description("商品別_舊_中文")]
        public string prod_type_o_D { get; set; }

        /// <summary>
        /// 商品別_新
        /// </summary>
        [Description("商品別_新")]
        public string prod_type_n { get; set; }

        /// <summary>
        /// 商品別_新_中文
        /// </summary>
        [Description("商品別_新_中文")]
        public string prod_type_n_D { get; set; }

        /// <summary>
        /// 帳本別_舊
        /// </summary>
        [Description("帳本別_舊")]
        public string corp_no_o { get; set; }

        /// <summary>
        /// 帳本別_新
        /// </summary>
        [Description("帳本別_新")]
        public string corp_no_n { get; set; }

        /// <summary>
        /// 取會科否_舊
        /// </summary>
        [Description("取會科否_舊")]
        public string actnum_yn_o { get; set; }

        /// <summary>
        /// 取會科否
        /// </summary>
        [Description("取會科否_新")]
        public string actnum_yn_n { get; set; }

        /// <summary>
        /// 保費收入首年首期_舊
        /// </summary>
        [Description("保費收入首年首期_舊")]
        public string acct_code_o { get; set; }

        /// <summary>
        /// 保費收入首年首期
        /// </summary>
        [Description("保費收入首年首期_新")]
        public string acct_code_n { get; set; }

        /// <summary>
        /// 保費收入首年續期_舊
        /// </summary>
        [Description("保費收入首年續期_舊")]
        public string acct_codef_o { get; set; }

        /// <summary>
        /// 保費收入首年續期
        /// </summary>
        [Description("保費收入首年續期_新")]
        public string acct_codef_n { get; set; }

        /// <summary>
        /// 續年度_舊
        /// </summary>
        [Description("續年度_舊")]
        public string acct_coder_o { get; set; }

        /// <summary>
        /// 續年度
        /// </summary>
        [Description("續年度_新")]
        public string acct_coder_n { get; set; }

        /// <summary>
        /// 執行動作
        /// </summary>
        [Description("執行動作")]
        public string exec_action { get; set; }

        /// <summary>
        /// 執行動作中文
        /// </summary>
        [Description("執行動作中文")]
        public string exec_action_D { get; set; }

        /// <summary>
        /// pk_id
        /// </summary>
        [Description("pk_id")]
        public string pk_id { get; set; }
    }
}