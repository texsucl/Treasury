using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0051Model
    {
        [Display(Name = "報表類別")]
        public string rpt_type { get; set; }

        [Display(Name = "歸戶條件")]
        public string cnt_type { get; set; }

        [Display(Name = "電訪標準覆核日")]
        public string tel_std_appr_date_b { get; set; }

        public string tel_std_appr_date_e { get; set; }

        [Display(Name = "電訪日期")]
        public string tel_interview_f_datetime_b { get; set; }

        public string tel_interview_f_datetime_e { get; set; }

        [Display(Name = "派件日期")]
        public string dispatch_date_b { get; set; }

        public string dispatch_date_e { get; set; }

        
        

        public string temp_id { get; set; }

        [Display(Name = "電訪處理結果")]
        public string tel_result { get; set; }

        [Display(Name = "電訪人員ID")]
        public string tel_interview_id { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "件數")]
        public decimal cnt { get; set; }

        [Display(Name = "金額級距")]
        public string amt_range { get; set; }
        public string amt_range_desc { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "電訪日期")]
        public string tel_interview_f_datetime { get; set; }

        public string tel_interview_ym { get; set; }

        [Display(Name = "給付細項")]
        public string paid_code { get; set; }

        public string range_flag { get; set; }

        [Display(Name = "電訪日期")]
        public string dispatch_date { get; set; }

        public string dispatch_ym { get; set; }



        public OAP0051Model() {
            rpt_type = "";
            cnt_type = "";
            tel_std_appr_date_b = "";
            tel_std_appr_date_e = "";
            tel_interview_f_datetime_b = "";
            tel_interview_f_datetime_e = "";
            dispatch_date_b = "";
            dispatch_date_e = "";


            temp_id = "";
            system = "";
            check_acct_short = "";
            check_no = "";
            tel_result = "";
            tel_interview_id = "";
            check_amt = 0;
            cnt = 0;
            amt_range = "";
            amt_range_desc = "";
            status = "";
            tel_interview_f_datetime = "";
            tel_interview_ym = "";
            paid_code = "";
            dispatch_date = "";
            dispatch_ym = "";
            range_flag = "";
        }

    }
}