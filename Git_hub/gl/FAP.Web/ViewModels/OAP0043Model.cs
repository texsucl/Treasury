using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0043Model
    {
        public string key { get; set; }
        public string temp_id { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_status { get; set; }

        public string execActionDesc { get; set; }

        [Display(Name = "申請人")]
        public string create_id { get; set; }

        [Display(Name = "申請時間")]
        public string create_dt { get; set; }

        [Display(Name = "設定項目")]
        public string type { get; set; }

        public string type_desc { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }


        [Display(Name = "保局範圍")]
        public string fsc_range { get; set; }
        public string fsc_range_desc { get; set; }

        [Display(Name = "級距")]
        public string amt_range { get; set; }
        
        public string amt_range_desc { get; set; }

        public int cnt { get; set; }
        public int cnt_noId { get; set; }

        [Display(Name = "支票金額")]
        public decimal check_amt { get; set; }

        [Display(Name = "系統別")]
        public string system { get; set; }

        [Display(Name = "支票號碼")]
        public string check_no { get; set; }

        [Display(Name = "支票帳號簡稱")]
        public string check_acct_short { get; set; }

        [Display(Name = "給付對象 ID")]
        public string paid_id { get; set; }

        [Display(Name = "第一次電訪人員")]
        public string proc_id { get; set; }

        public string proc_name { get; set; }

        [Display(Name = "派件百分比")]
        public string std_1 { get; set; }

        [Display(Name = "分派件數")]
        public string std_2 { get; set; }



        public OAP0043Model() {
            key = "";
            temp_id = "";
            aply_no = "";
            appr_status = "";
            execActionDesc = "";
            create_id = "";
            create_dt = "";

            type = "";
            type_desc = "";
            data_status = "";
            fsc_range = "";
            fsc_range_desc = "";
            amt_range = "";
            amt_range_desc = "";
            cnt = 0;
            cnt_noId = 0;
            check_amt = 0;

            system = "";
            check_no = "";
            check_acct_short = "";
            paid_id = "";

            proc_id = "";
            proc_name = "";
            std_1 = "";
            std_2 = "";

        }

    }
}