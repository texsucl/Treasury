using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL00010ViewModel
    {
        /// <summary>
        /// 退費項目類別
        /// </summary>
        [Description("退費項目類別")]
        public string pay_class { get; set; }

        /// <summary>
        /// 險種否_舊
        /// </summary>
        [Description("險種否_舊")]
        public string item_yn_o { get; set; }

        /// <summary>
        /// 險種否_新
        /// </summary>
        [Description("險種否_新")]
        public string item_yn_n { get; set; }

        /// <summary>
        /// 年次否_舊
        /// </summary>
        [Description("年次否_舊")]
        public string year_yn_o { get; set; }

        /// <summary>
        /// 年次否_新
        /// </summary>
        [Description("年次否_新")]
        public string year_yn_n { get; set; }

        /// <summary>
        /// 保費類別否_舊
        /// </summary>
        [Description("保費類別否_舊")]
        public string prem_yn_o { get; set; }

        /// <summary>
        /// 保費類別否_新
        /// </summary>
        [Description("保費類別否_新")]
        public string prem_yn_n { get; set; }

        /// <summary>
        /// 費用單位否_舊
        /// </summary>
        [Description("費用單位否_舊")]
        public string unit_yn_o { get; set; }

        /// <summary>
        /// 費用單位否_新
        /// </summary>
        [Description("費用單位否_新")]
        public string unit_yn_n { get; set; }

        /// <summary>
        /// 送金單否_舊
        /// </summary>
        [Description("送金單否_舊")]
        public string recp_yn_o { get; set; }

        /// <summary>
        /// 送金單否_新
        /// </summary>
        [Description("送金單否_新")]
        public string recp_yn_n { get; set; }

        /// <summary>
        /// 合約別否_舊
        /// </summary>
        [Description("合約別否_舊")]
        public string cont_yn_o { get; set; }

        /// <summary>
        /// 合約別否_新
        /// </summary>
        [Description("合約別否_新")]
        public string cont_yn_n { get; set; }

        /// <summary>
        /// 帳本否_舊
        /// </summary>
        [Description("帳本否_舊")]
        public string corp_yn_o { get; set; }

        /// <summary>
        /// 帳本否_新
        /// </summary>
        [Description("帳本否_新")]
        public string corp_yn_n { get; set; }

        /// <summary>
        /// 資料狀態
        /// </summary>
        [Description("資料狀態")]
        public string data_status { get; set; }

        /// <summary>
        /// 資料狀態中文
        /// </summary>
        [Description("資料狀態中文")]
        public string data_status_D { get; set; }

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
        /// 申請狀態
        /// </summary>
        [Description("申請狀態")]
        public string apply_status { get; set; }

        /// <summary>
        /// 申請狀態中文
        /// </summary>
        [Description("申請狀態中文")]
        public string apply_status_D { get; set; }

        /// <summary>
        /// 說明_舊
        /// </summary>
        [Description("說明_舊")]
        public string memo_o { get; set; }

        /// <summary>
        /// 說明_新
        /// </summary>
        [Description("說明_新")]
        public string memo_n { get; set; }

        /// <summary>
        /// 最後修改時間
        /// </summary>
        [Description("最後修改時間")]
        public string LastDateTime { get; set; }

        /// <summary>
        /// 明細資料
        /// </summary>
        [Description("明細資料")]
        public List<OGL00010ViewSubModel> SubDatas { get; set; }

        /// <summary>
        /// 申請人員
        /// </summary>
        [Description("申請人員")]
        public string apply_id { get; set; }

        /// <summary>
        /// 申請時間
        /// </summary>
        [Description("申請時間")]
        public string apply_datetime { get; set; }

        /// <summary>
        /// pk_id
        /// </summary>
        [Description("pk_id")]
        public string pk_id { get; set; }

        /// <summary>
        /// 是否選取
        /// </summary>
        [Description("是否選取")]
        public bool Ischecked { get; set; }

        /// <summary>
        /// 覆核權限
        /// </summary>
        [Description("覆核權限")]
        public bool review_flag { get; set; }
    }
}