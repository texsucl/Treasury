using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeCleanModel
    {

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
        public string check_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "帳務日期")]
        public string re_paid_date { get; set; }

        [Display(Name = "再給付方式")]
        public string re_paid_type { get; set; }







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

        [Display(Name = "回存金額")]
        public string main_amt { get; set; }

        [Display(Name = "原給付性質")]
        public string o_paid_cd { get; set; }

        public string data_flag { get; set; }

        [Display(Name = "假扣押編號")]
        public string filler_10 { get; set; }

        public string filler_16 { get; set; }

        public string err_msg { get; set; }

        public string aply_no { get; set; }

        public string aply_seq { get; set; }

        public VeCleanModel() {

            system = "";
            check_no = "0";
            check_acct_short = "";
            paid_id = "";
            paid_name = "";
            check_amt = "";
            check_date = "";
            re_paid_date = "";
            re_paid_type = "";


            policy_no = "";
            policy_seq = "";
            id_dup = "";
            member_id = "";
            change_id = "";
            main_amt = "";
            o_paid_cd = "";

            data_flag = "";
            filler_10 = "";
            filler_16 = "";

            err_msg = "";

            aply_no = "";
            aply_seq = "";

        }

    }
}