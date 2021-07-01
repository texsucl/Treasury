using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0011Model
    {
        public string temp_id { get; set; }

        public string data_status { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }


        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        [Display(Name = "清理大類")]
        public string level_1 { get; set; }

        [Display(Name = "清理小類")]
        public string level_2 { get; set; }

        [Display(Name = "支票金額合計")]
        public string check_amt { get; set; }

        [Display(Name = "申請人員")]
        public string update_id { get; set; }
        public string update_name { get; set; }

        [Display(Name = "申請日期時間")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public string approve_datetime { get; set; }

        [Display(Name = "結案編號")]
        public string closed_no { get; set; }

        [Display(Name = "結案日期")]
        public string closed_date { get; set; }

        [Display(Name = "過程說明")]
        public string proc_desc { get; set; }


        
        public string memo { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "結案報表說明")]
        public string closed_desc { get; set; }


        public OAP0011Model() {
            temp_id = "";
            data_status = "";

            aply_no = "";
            appr_stat = "";

            paid_id = "";
            paid_name = "";
            check_no = "";
            check_acct_short = "";
            check_date = "";
            o_paid_cd = "";
            level_1 = "";
            level_2 = "";
            check_amt = "";

            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            closed_no = "";
            proc_desc = "";
            memo = "";
            closed_date = "";
            status = "";
            closed_desc = "";
        }

    }
}