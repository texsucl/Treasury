using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0015Model
    {
        [Display(Name = "保局範圍")]
        public List<FAP_VE_CODE> fscRangeList { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "統計截止區間")]
        public string date_e { get; set; }



        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "系統別")]
        public string system { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "身份證重覆別")]
        public string id_dup { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }


        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "回存票金額")]
        public string main_amt { get; set; }

        public string rpt_status { get; set; }


        public OAP0015Model() {
            status = "";
            date_e = "";

            check_date = "";
            system = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            check_no = "";
            check_amt = "";
            main_amt = "";

            rpt_status = "";
        }

    }
}