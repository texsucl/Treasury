using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{

    public class ORT0101Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "退匯給付方式")]
        public string ptype { get; set; }

        [Display(Name = "群組別")]
        public string group_id { get; set; }

        [Display(Name = "文字長度")]
        public string text_len { get; set; }

        [Display(Name = "編號")]
        public string ref_no { get; set; }

        [Display(Name = "原因說明")]
        public string text { get; set; }

        [Display(Name = "資料來源")]
        public string srce_from { get; set; }

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



        public ORT0101Model()
        {
            tempId = "";
            aply_no = "";
            appr_stat = "";

            ptype = "";
            group_id = "";
            text_len = "";
            ref_no = "";
            text = "";
            srce_from = "";

            data_status = "";
            exec_action = "";

            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            approve_datetime = null;

        }


    }
}