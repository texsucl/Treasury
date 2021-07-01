using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORT0002Model
    {
        public string tempId { get; set; }

        [Display(Name = "申請單號")]
        public string aply_No { get; set; }

        [Display(Name = "繳款原因")]
        public string rm_resn { get; set; }

        [Display(Name = "系統日前")]
        public string sys_day { get; set; }

        [Display(Name = "匯款次數")]
        public string remit_cnt { get; set; }

        [Display(Name = "匯款金額")]
        public string remit_amt { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        public string update_name { get; set; }

        [Display(Name = "異動日期時間")]
        public string update_datetime { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "執行功能")]
        public string status { get; set; }
        public string status_desc { get; set; }


        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        public string appr_name { get; set; }

        [Display(Name = "覆核日期")]
        public string appr_datetime { get; set; }



        public ORT0002Model() {
            tempId = "";
            aply_No = "";
            rm_resn = "";
            sys_day = "";
            remit_cnt = "";
            remit_amt = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            dataStatus = "";
            status = "";
            status_desc = "";
            appr_stat = "";
            appr_id = "";
            appr_name = "";
            appr_datetime = "";
         
        }

    }
}