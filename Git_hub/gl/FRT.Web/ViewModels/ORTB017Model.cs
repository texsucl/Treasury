using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class FBOModel
    {
        public FBOModel()
        {
            EXPORT_BANK_NAME = "北富銀敦南";
            EXPORT_NO = "737102710668";
        }

        /// <summary>
        /// checkbox 狀態
        /// </summary>
        public bool checkFlag { get; set; }

        /// <summary>
        /// 快速付款編號
        /// </summary>
        [Display(Name = "快速付款編號")]
        public string FAST_NO { get; set; }

        /// <summary>
        /// 收款人ID
        /// </summary>
        [Display(Name = "收款人ID")]
        public string PAID_ID { get; set; }

        /// <summary>
        /// 收款人戶名
        /// </summary>
        [Display(Name = "收款人戶名")]
        public string RCV_NAME { get; set; }

        /// <summary>
        /// 匯出銀行
        /// </summary>
        [Display(Name = "匯出銀行")]
        public string EXPORT_BANK_NAME { get; set; }

        /// <summary>
        /// 匯出帳號
        /// </summary>
        [Display(Name = "匯出帳號")]
        public string EXPORT_NO { get; set; }

        /// <summary>
        /// 銀行代號
        /// </summary>
        [Display(Name = "銀行代號")]
        public string BANK_CODE_SUB_BANK { get; set; }

        /// <summary>
        /// 匯款帳號
        /// </summary>
        [Display(Name = "匯款帳號")]
        public string BANK_ACT { get; set; }

        /// <summary>
        /// 匯款金額
        /// </summary>
        [Display(Name = "匯款金額")]
        public string REMIT_AMT { get; set; }

        /// <summary>
        /// 匯出日期
        /// </summary>
        [Display(Name = "匯出日期")]
        public string EXPORT_DATE { get; set; }

        /// <summary>
        /// 轉檔批號
        /// </summary>
        [Display(Name = "轉檔批號")]
        public string FILLER_20 { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        [Display(Name = "幣別")]
        public string CURRENCY { get; set; }

        /// <summary>
        /// 錯誤代碼
        /// </summary>
        [Display(Name = "錯誤代碼")]
        public string FAIL_CODE { get; set; }

        /// <summary>
        /// 匯款狀態
        /// </summary>
        [Display(Name = "匯款狀態")]
        public string REMIT_STAT { get; set; }
    }
}