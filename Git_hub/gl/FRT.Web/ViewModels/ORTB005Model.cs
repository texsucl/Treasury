using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    [Serializable]
    public class ORTB005Model
    {

        [Display(Name = "傳送日")]
        public string field081 { get; set; }

        public string field081B { get; set; }

        public string field081E { get; set; }


        [Display(Name = "來源")]
        public string resource { get; set; }

        [Display(Name = "類型")]
        public string paidType { get; set; }


        [Display(Name = "帳本")]
        public string corpNo { get; set; }

        [Display(Name = "幣別")]
        public string currency { get; set; }

        [Display(Name = "下送SQL編號")]
        public string sqlNo { get; set; }

        [Display(Name = "憑證編號")]
        public string vhrNo1 { get; set; }

        [Display(Name = "匯款金額")]
        public string remitAmt { get; set; }

        public string dataCnt { get; set; }


        [Display(Name = "確認人員")]
        public string acptId { get; set; }

        public ORTB005Model() {
            field081 = "";
            field081B = "";
            field081E = "";
            resource = "";
            paidType = "";
            corpNo = "";
            currency = "";
            sqlNo = "";
            vhrNo1 = "";
            remitAmt = "";
            dataCnt = "";
            acptId = "";
        }

    }
}