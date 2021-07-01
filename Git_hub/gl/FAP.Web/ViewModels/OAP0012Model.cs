using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0012Model
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

        [Display(Name = "歸戶條件")]
        public string cnt_type { get; set; }


        public string level_1 { get; set; }
        public string level_2 { get; set; }
        public string cnt { get; set; }
        public decimal amt { get; set; }

        public string stat_id { get; set; }


        public OAP0012Model() {
            date_b = "";
            date_e = "";
            cnt_type = "";

            level_1 = "";
            level_2 = "";
            cnt = "0";
            //amt = 0;
            stat_id = "";
        }

    }
}