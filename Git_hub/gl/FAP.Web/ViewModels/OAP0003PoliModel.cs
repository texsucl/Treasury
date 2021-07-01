using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0003PoliModel
    {
        public string temp_id { get; set; }

        [Display(Name = "系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "帳戶簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "案號")]
        public string change_id { get; set; }

        [Display(Name = "給付項目")]
        public string o_paid_cd { get; set; }

        [Display(Name = "回存金額")]
        public string main_amt { get; set; }




        [Display(Name = "信函編號")]
        public string report_no { get; set; }

        [Display(Name = "給付ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象")]
        public string paid_name { get; set; }

        [Display(Name = "處理階段")]
        public string r_status { get; set; }

        public string filler_14 { get; set; }


        public OAP0003PoliModel() {
            system = "";
            check_no = "";
            check_acct_short = "";
            policy_no = "";
            policy_seq = "0";
            id_dup = "";
            change_id = "";
            o_paid_cd = "";
            main_amt = "";

            report_no = "";
            paid_id = "";
            paid_name = "0";
            r_status = "";

            filler_14 = "";
        }

    }
}