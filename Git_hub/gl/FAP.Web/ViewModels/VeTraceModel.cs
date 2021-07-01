using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class VeTraceModel
    {
        public string temp_id { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }

        [Display(Name = "設定項目")]
        public string code_type { get; set; }

        [Display(Name = "代碼")]
        public string code_id { get; set; }

        [Display(Name = "代碼值")]
        public string code_value { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }

        [Display(Name = "備註")]
        public string remark { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }
        public string update_name { get; set; }

        [Display(Name = "異動日期time")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public string approve_datetime { get; set; }


        public VeTraceModel() {
            aply_no = "";
            appr_stat = "0";
            exec_action = "";

            code_type = "";
            code_id = "";
            code_value = "";
            data_status = "";
            remark = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";

        }

    }
}