using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0053Model
    {

        [Display(Name = "調閱完成日")]
        public string proc_date_b { get; set; }

        public string proc_date_e { get; set; }
        

        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }
        public string fsc_range_desc { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對姓名")]
        public string paid_name { get; set; }

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

        [Display(Name = "調閱完成日")]
        public string proc_date { get; set; }

        [Display(Name = "帳務日期")]
        public string re_paid_date { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }
        public string status_desc { get; set; }

        [Display(Name = "給付細項")]
        public string paid_code { get; set; }
        public string paid_code_desc { get; set; }

        [Display(Name = "備註說明")]
        public string remark { get; set; }

        [Display(Name = "結案日期")]
        public string closed_date { get; set; }


        public OAP0053Model() {
            proc_date_b = "";
            proc_date_e = "";

            fsc_range = "";
            fsc_range_desc = "";
            paid_id = "";
            paid_name = "";
            check_acct_short = "";
            check_no = "";
            check_date = "";
            check_amt = 0;
            o_paid_cd = "";
            o_paid_cd_desc = "";
            proc_date = "";
            re_paid_date = "";
            status = "";
            status_desc = "";
            paid_code = "";
            paid_code_desc = "";
            remark = "";

            closed_date = "";
        }

    }
}