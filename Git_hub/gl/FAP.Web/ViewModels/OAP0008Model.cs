using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0008Model
    {
        public string temp_id { get; set; }

        public string data_status { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "踐行程序")]
        public string practice { get; set; }

        [Display(Name = "証明文件")]
        public string cert_doc { get; set; }

        [Display(Name = "執行日期")]
        public string exec_date { get; set; }

        [Display(Name = "清理大類")]
        public string level_1_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2_1 { get; set; }

        [Display(Name = "踐行程序(一)")]
        public string practice_1 { get; set; }

        [Display(Name = "証明文件(一)")]
        public string cert_doc_1 { get; set; }

        [Display(Name = "執行日期(一)")]
        public string exec_date_1 { get; set; }

        [Display(Name = "過程說明")]
        public string proc_desc { get; set; }
        

        [Display(Name = "異動人員")]
        public string update_id { get; set; }
        public string update_name { get; set; }

        [Display(Name = "異動日期time")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public string approve_datetime { get; set; }

        [Display(Name = "資料來源")]
        public string srce_from { get; set; }


        [Display(Name = "清理狀態")]
        public string status { get; set; }

        public OAP0008Model() {
            temp_id = "";
            data_status = "";

            aply_no = "";
            appr_stat = "";
            exec_action = "";

            paid_id = "";
            paid_name = "";
            check_no = "";
            check_acct_short = "";
            level_1 = "";
            level_2 = "";
            practice = "";
            cert_doc = "";
            exec_date = "";

            level_1_1 = "";
            level_2_1 = "";
            practice_1 = "";
            cert_doc_1 = "";
            exec_date_1 = "";
            proc_desc = "";

            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";

            srce_from = "";
            status = "";
        }

    }
}