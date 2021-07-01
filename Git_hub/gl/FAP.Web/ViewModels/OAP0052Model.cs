using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0052Model
    {
        [Display(Name = "電訪編號")]
        public string tel_proc_no { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }
        public string o_paid_cd_desc { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "給付帳務日")]
        public string re_paid_date { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }


        [Display(Name = "清理階段")]
        public string clean_status { get; set; }

        [Display(Name = "清理標準")]
        public decimal clean_std_day { get; set; }

        [Display(Name = "清理階段完成日期")]
        public decimal clean_f_date { get; set; }

        [Display(Name = "逾期天數")]
        public string overdue_day { get; set; }

        [Display(Name = "逾期原因")]
        public string overdue_reason { get; set; }

        [Display(Name = "清理階段日期")]
        public string clean_date { get; set; }



        public OAP0052Model() {
            tel_proc_no = "";
            paid_id = "";
            paid_name = "";
            check_acct_short = "";
            check_no = "";
            check_date = "";
            check_amt = "";
            o_paid_cd = "";
            o_paid_cd_desc = "";
            status = "";
            re_paid_date = "";
            level_1 = "";
            level_2 = "";
            clean_status = "";
            clean_std_day = 0;
            clean_f_date = 0;
            overdue_day = "";
            overdue_reason = "";
            clean_date = "";

        }

    }
}