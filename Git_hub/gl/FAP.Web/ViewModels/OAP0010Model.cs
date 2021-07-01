using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0010Model
    {
        public string temp_id { get; set; }


        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號 ")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼 ")]
        public string id_dup { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "帳務日期")]
        public string re_paid_date { get; set; }

        [Display(Name = "再給付方式")]
        public string re_paid_type { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "結案編號")]
        public string closed_no { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        [Display(Name = "異動日期")]
        public string update_datetime { get; set; }


        public OAP0010Model() {
            temp_id = "";

            paid_id = "";
            paid_name = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            check_no = "";
            check_acct_short = "";
            status = "";
            fsc_range = "";
            check_date = "";
            check_amt = "";
            re_paid_date = "";
            re_paid_type = "";
            level_1 = "";
            level_2 = "";
            closed_no = "";
            update_id = "";
            update_datetime = "";
        }

    }
}