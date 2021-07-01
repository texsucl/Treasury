using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class FRTITEM0
    {
        /// <summary>
        /// 退費項目類別
        /// </summary>
        [Description("退費項目類別")]
        public string PAY_CLASS { get; set; }

        /// <summary>
        /// 險種否
        /// </summary>
        [Description("險種否")]
        public string ITEM_YN { get; set; }

        /// <summary>
        /// 年次否
        /// </summary>
        [Description("年次否")]
        public string YEAR_YN { get; set; }

        /// <summary>
        /// 保費類別否
        /// </summary>
        [Description("保費類別否")]
        public string PREM_YN { get; set; }

        /// <summary>
        /// 費用單位否
        /// </summary>
        [Description("費用單位否")]
        public string UNIT_YN { get; set; }

        /// <summary>
        /// 送金單否
        /// </summary>
        [Description("送金單否")]
        public string RECP_YN { get; set; }

        /// <summary>
        /// 合約別否
        /// </summary>
        [Description("合約別否")]
        public string CONT_YN { get; set; }

        /// <summary>
        /// 帳本否
        /// </summary>
        [Description("帳本否")]
        public string CORP_YN { get; set; }

        /// <summary>
        /// 保費類別
        /// </summary>
        [Description("保費類別")]
        public string PREM_KIND { get; set; }

        /// <summary>
        /// 合約別
        /// </summary>
        [Description("合約別")]
        public string CONT_TYPE { get; set; }

        /// <summary>
        /// 商品別
        /// </summary>
        [Description("商品別")]
        public string PROD_TYPE { get; set; }

        /// <summary>
        /// 帳本別
        /// </summary>
        [Description("帳本別")]
        public string CORP_NO { get; set; }

        /// <summary>
        /// 取會科否
        /// </summary>
        [Description("取會科否")]
        public string ACTNUM_YN { get; set; }

        /// <summary>
        /// 保費收入首年首期
        /// </summary>
        [Description("保費收入首年首期")]
        public string ACCT_CODE { get; set; }

        /// <summary>
        /// 保費收入首年續期
        /// </summary>
        [Description("保費收入首年續期")]
        public string ACCT_CODEF { get; set; }

        /// <summary>
        /// 續年度
        /// </summary>
        [Description("續年度")]
        public string ACCT_CODER { get; set; }

        /// <summary>
        /// 異動人員
        /// </summary>
        [Description("異動人員")]
        public string UPD_ID{ get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        [Description("異動日期")]
        public string UPD_DATE { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        [Description("異動時間")]
        public string UPD_TIME { get; set; }
    }
}