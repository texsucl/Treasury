using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0047ProcModel
    {
        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        public string data_type { get; set; }

        public string data_type_desc { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        public string update_name { get; set; }

        [Display(Name = "異動日期")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        public string appr_name { get; set; }

        [Display(Name = "覆核日期")]
        public string approve_datetime { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "電訪覆核結果")]
        public string tel_appr_result { get; set; }

        [Display(Name = "電訪處理結果")]
        public string tel_result { get; set; }

        public OAP0047ProcModel() {
            aply_no = "";
            data_type = "";
            data_type_desc = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            appr_name = "";
            approve_datetime = "";
            appr_stat = "";
            tel_appr_result = "";
            tel_result = "";
        }

    }
}