using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0054Model
    {


        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }
        public string o_paid_cd_desc { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }
        public string status_desc { get; set; }



        public OAP0054Model() {
            paid_id = "";
            check_acct_short = "";
            check_no = "";
            check_date = "";
            check_amt = 0;
            o_paid_cd = "";
            o_paid_cd_desc = "";
            system = "";
            status = "";
            status_desc = "";

        }
    }
}