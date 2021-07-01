using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0048Model
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

        [Display(Name = "清理人員")]
        public string proc_id { get; set; }

        public string proc_name { get; set; }

        [Display(Name = "清理階段")]
        public string clean_status { get; set; }

        [Display(Name = "應完成日")]
        public string clean_shf_date { get; set; }

        [Display(Name = "實際完成日")]
        public string clean_f_date { get; set; }

        [Display(Name = "備註說明")]
        public string remark { get; set; }


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

        [Display(Name = "電訪編號")]
        public string tel_proc_no { get; set; }

        public DateTime clean_date { get; set; }

        public string std_1 { get; set; }


        public OAP0048Model() {
            qType = "";

            paid_id = "";
            paid_name = "";
            policy_no = "";
            policy_seq = "";
            id_dup = "";
            check_no = "";
            check_acct_short = "";
            proc_id = "";
            proc_name = "";
            clean_status = "";
            clean_shf_date = "";
            clean_f_date = "";
            remark = "";
            data_status = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            //approve_datetime = "";
            appr_stat = "";
            aply_no = "";
            exec_action = "";
            tel_proc_no = "";
            //clean_date = "";

            std_1 = "";
        }

    }
}