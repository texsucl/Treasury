using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0049DModel
    {

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        public OAP0049DModel() {
            fsc_range = "";
            check_no = "";
            check_acct_short = "";
            check_amt = 0;
            check_date = "";
        }

    }
}