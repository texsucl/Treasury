using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.ViewModels
{
    public class OGL00009Model
    {
        public string tempId { get; set; }

        [Display(Name = "覆核單號")]
        public string aply_no { get; set; }

        [Display(Name = "覆核狀態")]
        public string appr_stat { get; set; }


        [Display(Name = "商品類別")]
        public string item_type { get; set; }

        [Display(Name = "險種代號")]
        public string item { get; set; }

        [Display(Name = "系統別")]
        public string sys_type { get; set; }

        [Display(Name = "保險期間一年期")]
        public string year { get; set; }

        [Display(Name = "繳費年期類別")]
        public string prem_y_tp { get; set; }

        [Display(Name = "最高承保年齡")]
        public string age { get; set; }

        [Display(Name = "執行功能")]
        public string exec_action { get; set; }

        [Display(Name = "商品類別_異動後")]
        public string item_type_n { get; set; }

        [Display(Name = "險種代號_異動後")]
        public string item_n { get; set; }

        [Display(Name = "系統別_異動後")]
        public string sys_type_n { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        public string update_name { get; set; }

        [Display(Name = "異動日期time")]
        public string update_datetime { get; set; }

        [Display(Name = "覆核人員")]
        public string appr_id { get; set; }

        [Display(Name = "覆核日期time")]
        public string appr_datetime { get; set; }

        [Display(Name = "資料狀態")]
        public string data_status { get; set; }



        public OGL00009Model() {
            tempId = "";
            aply_no = "";
            appr_stat = "";

            item_type = "";
            item = "";
            sys_type = "";
            year = "";
            prem_y_tp = "";
            age = "";
            exec_action = "";
            item_type_n = "";
            item_n = "";
            sys_type_n = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            appr_datetime = "";
            data_status = "";
            
        }

    }
}