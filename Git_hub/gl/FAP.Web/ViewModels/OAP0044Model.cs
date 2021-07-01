using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0044Model
    {
        public string temp_id { get; set; }

        [Display(Name = "原派件人員")]
        public string tel_interview_id_o { get; set; }

        [Display(Name = "新派件人員")]
        public string tel_interview_id_n { get; set; }

        [Display(Name = "電訪人員ID")]
        public string tel_interview_id { get; set; }

        public string tel_interview_name { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        public string paid_name { get; set; }


        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票號碼簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }



        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        public string update_name { get; set; }

        [Display(Name = "異動日期")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期")]
        public DateTime approve_datetime { get; set; }

        public string tel_appr_result { get; set; }

        [Display(Name = "派件日")]
        public string dispatch_date { get; set; }


        public OAP0044Model()
        {
            temp_id = "";
            tel_interview_id_o = "";
            tel_interview_id_n = "";
            tel_interview_id = "";
            paid_id = "";
            paid_name = "";
            check_no = "";
            check_acct_short = "";
            check_date = "";
            check_amt = 0;
            o_paid_cd = "";
            system = "";
            appr_stat = "";
            aply_no = "";
            data_status = "";
            update_id = "";
            update_name = "";
            update_datetime = "";

            appr_id = "";
            //approve_datetime = "";
            tel_appr_result = "";

            dispatch_date = "";
        }

    }
}