using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0004Model
    {
        public string temp_id { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date_b { get; set; }

        public string check_date_e { get; set; }



        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼／匯費序號")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "案號人員別")]
        public string member_id { get; set; }

        [Display(Name = "案號")]
        public string change_id { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "現在保局範圍")]
        public string fsc_range { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }
        

        [Display(Name = "修改指定保局範圍")]
        public string fsc_range_n { get; set; }

        [Display(Name = "備註")]
        public string memo { get; set; }

        [Display(Name = "異動筆數")]
        public string update_cnt { get; set; }


        [Display(Name = "申請人")]
        public string update_id { get; set; }
        public string update_name { get; set; }

        [Display(Name = "申請日期")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期")]
        public string approve_datetime { get; set; }


        public OAP0004Model() {
            aply_no = "";
            appr_stat = "";
            check_date = "";
            check_date_b = "";
            check_date_e = "";
            system = "";
            check_no = "";
            check_acct_short = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            member_id = "";
            change_id = "";
            paid_id = "";
            fsc_range = "";
            fsc_range_n = "";
            check_amt = "";
            memo = "";
            update_cnt = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            approve_datetime = "";

        }
    }
}