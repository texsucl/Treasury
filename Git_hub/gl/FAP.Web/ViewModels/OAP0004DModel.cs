using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0004DModel
    {


        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼／匯費序號")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }


        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "支票金額")]
        public Int64 check_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "帳務日期")]
        public string re_paid_date { get; set; }

        [Display(Name = "再給付日期")]
        public string re_paid_date_n { get; set; }

        [Display(Name = "再給付方式")]
        public string re_paid_type { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "結案日期")]
        public string closed_date { get; set; }

        [Display(Name = "給付細項")]
        public string paid_code { get; set; }

        [Display(Name = "派件狀態")]
        public string dispatch_status { get; set; }

        [Display(Name = "清理階段")]
        public string clean_status { get; set; }

        [Display(Name = "處理結果")]
        public string tel_result { get; set; }

        [Display(Name = "電訪覆核結果")]
        public string tel_appr_result { get; set; }

        [Display(Name = "寄信次數")]
        public int send_cnt { get; set; }

        [Display(Name = "階段狀態")]
        public string stage_status { get; set; }

        [Display(Name = "階段1日期")]
        public string stage_1_date { get; set; }

        [Display(Name = "階段2日期")]
        public string stage_2_date { get; set; }

        [Display(Name = "階段3日期")]
        public string stage_3_date { get; set; }

        [Display(Name = "階段4日期")]
        public string stage_4_date { get; set; }

        [Display(Name = "階段5日期")]
        public string stage_5_date { get; set; }

        [Display(Name = "階段6日期")]
        public string stage_6_date { get; set; }

        [Display(Name = "階段7日期")]
        public string stage_7_date { get; set; }

        [Display(Name = "派件日期")]
        public string dispatch_date { get; set; }

        [Display(Name = "第一次電訪人員")]
        public string tel_interview_id { get; set; }


        public OAP0004DModel() {
            fsc_range = "";
            system = "";
            check_no = "";
            check_acct_short = "";
            paid_id = "";
            paid_name = "";
            check_amt = 0;
            check_date = "";
            re_paid_date = "";
            re_paid_date_n = "";
            re_paid_type = "";
            status = "";
            closed_date = "";

            paid_code = "";
            dispatch_status = "";
            tel_appr_result = "";
            clean_status = "";
            tel_result = "";

            send_cnt = 0;
            stage_status = "";
            stage_1_date = "";
            stage_2_date = "";
            stage_3_date = "";
            stage_4_date = "";
            stage_5_date = "";
            stage_6_date = "";
            stage_7_date = "";

            dispatch_date = "";
            tel_interview_id = "";

        }
    }
}