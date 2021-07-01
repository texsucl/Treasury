using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0046Model
    {
        public string qType { get; set; }
        
        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        public string policy_seq { get; set; }

        public string id_dup { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        public string check_acct_short { get; set; }

        [Display(Name = "最後一次電訪日期")]
        public string tel_interview_datetime { get; set; }

        [Display(Name = "電訪編號")]
        public string tel_proc_no { get; set; }

        [Display(Name = "電訪人員ID")]
        public string tel_interview_id { get; set; }

        [Display(Name = "電訪處理結果")]
        public string tel_result { get; set; }

        [Display(Name = "錄音檔號")]
        public string record_no { get; set; }

        [Display(Name = "電訪對象")]
        public string called_person { get; set; }

        [Display(Name = "電話")]
        public string cust_tel { get; set; }

        [Display(Name = "電訪郵寄郵遞區號")]
        public string tel_zip_code { get; set; }

        [Display(Name = "電訪郵寄地址")]
        public string tel_addr { get; set; }

        [Display(Name = "E-mail address")]
        public string tel_mail { get; set; }

        [Display(Name = "清理大類 ")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "客戶服務櫃檯")]
        public string cust_counter { get; set; }

        [Display(Name = "臨櫃日期")]
        public string counter_date { get; set; }

        [Display(Name = "原因說明")]
        public string reason { get; set; }

        
        [Display(Name = "電訪覆核結果")]
        public string tel_appr_result { get; set; }

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

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }


        [Display(Name = "上次處理結果")]
        public string tel_result_o { get; set; }

        [Display(Name = "最後一次電訪日期")]
        public string tel_interview_f_datetime { get; set; }


        public OAP0046Model() {
            

            qType = "";

            paid_id = "";
            paid_name = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            check_no = "";
            check_acct_short = "";
            tel_interview_datetime = "";
            tel_proc_no = "";
            tel_interview_id = "";
            tel_result = "";
            record_no = "";
            called_person = "";
            cust_tel = "";
            level_1 = "";
            level_2 = "";
            cust_counter = "";
            counter_date = "";
            tel_zip_code = "";
            tel_addr = "";
            tel_mail = "";
            reason = "";
            tel_appr_result = "";


            data_status = "";
            update_id = "";
            update_name = "";
            update_datetime = null;
            appr_id = "";
            //approve_datetime = null;
            appr_stat = "";
            aply_no = "";
            exec_action = "";

            tel_result_o = "";
            tel_interview_f_datetime = "";
;
            
        }

    }
}