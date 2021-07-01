using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB018Model
    {
        /// <summary>
        /// 轉檔批號
        /// </summary>
        [Display(Name = "轉檔批號")]
        public string TRANSFER_NO { get; set; }

        /// <summary>
        /// 匯款日期
        /// </summary>
        [Display(Name = "匯款日期")]
        public string VOUCHER_DATE { get; set; }

        /// <summary>
        /// 轉檔人員
        /// </summary>
        [Display(Name = "轉檔人員")]
        public string TRANSFER_ID { get; set; }

        /// <summary>
        /// 匯款筆數
        /// </summary>
        [Display(Name = "匯款筆數")]
        public string VOUCHER_COUNT { get; set; }

        /// <summary>
        /// 匯款金額
        /// </summary>
        [Display(Name = "匯款金額")]
        public string VOUCHER_AMT { get; set; }

    }
}