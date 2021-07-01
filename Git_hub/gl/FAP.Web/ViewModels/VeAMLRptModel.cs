using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeAMLRptModel
    {
        [Display(Name = "信函編號")]
        public string report_no { get; set; }

        [Display(Name = "支票號碼／匯費序號")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "保單號碼")]
        public string policy_no { get; set; }

        [Display(Name = "保單序號")]
        public string policy_seq { get; set; }

        [Display(Name = "重覆碼")]
        public string id_dup { get; set; }

        [Display(Name = "支票金額")]
        public string check_amt { get; set; }

        [Display(Name = "支票到期日")]
        public string check_date { get; set; }

        [Display(Name = "大系統別")]
        public string system { get; set; }

        [Display(Name = "服務中心人員")]
        public string c_serv_id { get; set; }

        public string c_serv_nm { get; set; }

        [Display(Name = "行庫代號")]
        public string r_bank_cd { get; set; }

        public string r_sub_bank { get; set; }

        [Display(Name = "匯款帳號")]
        public string r_bank_act { get; set; }

        [Display(Name = "給付對象戶名")]
        public string c_paid_nm { get; set; }

        [Display(Name = "實體支票")]
        public string check_flag { get; set; }

        public string data_flag { get; set; }

        public string aml_desc { get; set; }

        public VeAMLRptModel() {

            report_no = "";
            check_no = "";
            check_acct_short = "";
            paid_id = "";
            paid_name = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            check_amt = "";
            check_date = "";
            system = "";
            c_serv_id = "";
            c_serv_nm = "";
            r_bank_cd = "";
            r_sub_bank = "";
            r_bank_act = "";
            c_paid_nm = "";
            check_flag = "";
            data_flag = "";
            aml_desc = "";
        }

    }
}