using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0046DModel
    {
        public string temp_id { get; set; }

        [Display(Name = "電訪人員ID")]
        public string tel_interview_id { get; set; }

        [Display(Name = "電訪編號")]
        public string tel_proc_no { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        public string paid_name { get; set; }
        

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票號碼簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "回存金額")]
        public decimal main_amt { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public int policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "派案狀態")]
        public string dispatch_status { get; set; }

        [Display(Name = "電訪處理結果")]
        public string tel_result { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "電訪覆核結果")]
        public string tel_appr_result { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }

        public string update_id { get; set; }


        public OAP0046DModel() {
            tel_interview_id = "";
            tel_proc_no = "";
            status = "";
            paid_id = "";
            paid_name = "";
            check_no = "";
            check_acct_short = "";
            check_date = "";
            main_amt = 0;
            check_amt = 0;
            o_paid_cd = "";
            system = "";
            policy_no = "";
            policy_seq = 0;
            id_dup = "";
            dispatch_status = "";
            tel_result = "";
            appr_stat = "";
            aply_no = "";
            tel_appr_result = "";
            data_status = "";

            update_id = "";
        }

    }
}