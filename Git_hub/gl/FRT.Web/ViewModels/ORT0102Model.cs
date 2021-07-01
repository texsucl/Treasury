using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{

    public class ORT0102Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "退匯給付方式")]
        public string ptype { get; set; }

        [Display(Name = "系統別")]
        public string system { get; set; }

        [Display(Name = "群組別")]
        public string group_id { get; set; }

        [Display(Name = "資料來源")]
        public string srce_from { get; set; }

        [Display(Name = "資料類別")]
        public string srce_kind { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }


        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        public string update_name { get; set; }

        [Display(Name = "異動日期time")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public string approve_datetime { get; set; }

        public string ref_no { get; set; }


        public ORT0102Model()
        {
            tempId = "";
            aply_no = "";
            appr_stat = "";


            ptype = "";
            group_id = "";
            system = "";
            srce_from = "";
            srce_kind = "";
            data_status = "";
            
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            approve_datetime = null;

            ref_no = "";

        }


    }
}