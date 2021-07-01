using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0049Model
    {
        [Display(Name = "流水號")]
        public int seq_no { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "電訪編號")]
        public string tel_proc_no { get; set; }

        [Display(Name = "資料類別")]
        public string data_type { get; set; }

        public string data_type_desc { get; set; }

        [Display(Name = "處理人員")]
        public string proc_id { get; set; }

        public string proc_name { get; set; }

        [Display(Name = "處理結果/清理階段")]
        public string proc_status { get; set; }

        public string proc_status_desc { get; set; }

        [Display(Name = "處理日期")]
        public string proc_datetime { get; set; }

        [Display(Name = "原因說明")]
        public string reason { get; set; }

        public string aply_no { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        public string appr_name { get; set; }

        [Display(Name = "覆核日期時間")]
        public string appr_datetime { get; set; }

        [Display(Name = "電訪覆核結果")]
        public string appr_status { get; set; }
        public string appr_status_desc { get; set; }



        public OAP0049Model() {
            seq_no = 0;
            paid_id = "";
            tel_proc_no = "";
            data_type = "";
            data_type_desc = "";
            proc_id = "";
            proc_name = "";
            proc_status = "";
            proc_status_desc = "";
            proc_datetime = "";
            appr_id = "";
            appr_name = "";
            appr_datetime = "";
            appr_status = "";
            appr_status_desc = "";
            reason = "";
            aply_no = "";
        }

    }
}