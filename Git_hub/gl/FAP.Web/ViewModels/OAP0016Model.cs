using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0016Model
    {
        [Display(Name = "保局範圍")]
        public List<FAP_VE_CODE> fscRangeList { get; set; }

        [Display(Name = "清理大類")]
        public List<FAP_VE_CODE> level1List { get; set; }

        [Display(Name = "清理小類")]
        public List<FAP_VE_CODE> level2List { get; set; }

        [Display(Name = "統計截止區間")]
        public string date_b { get; set; }
        public string date_e { get; set; }

        [Display(Name = "清理狀態")]
        public string status { get; set; }

        [Display(Name = "歸戶條件")]
        public string cnt_type { get; set; }


        public string fsc_range { get; set; }

        public string re_paid_date { get; set; }

        public string closed_date { get; set; }

        public string exec_date { get; set; }

        public string rpt_status { get; set; }

        public string cnt { get; set; }
        public string amt { get; set; }

        public string stat_id { get; set; }

        public string paid_id { get; set; }

        public string check_no { get; set; }


        public OAP0016Model() {
            date_b = "";
            date_e = "";
            status = "";
            cnt_type = "";

            fsc_range = "";
            re_paid_date = "";
            closed_date = "";
            exec_date = "";
            rpt_status = "";
            cnt = "0";
            amt = "";
            stat_id = "";

            paid_id = "";
            check_no = "";
        }

    }
}