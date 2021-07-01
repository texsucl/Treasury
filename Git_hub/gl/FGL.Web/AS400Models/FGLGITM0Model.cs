using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FGL.Web.AS400Models
{
    public class FGLGITM0Model
    {
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

        [Display(Name = "最高投保年齡")]
        public string age { get; set; }

        [Display(Name = "保留文字 10(1)")]
        public string field10_1 { get; set; }

        [Display(Name = "保留文字 10(2)")]
        public string field10_2 { get; set; }

        [Display(Name = "保留文字 10(2)")]
        public string field10_3 { get; set; }

        [Display(Name = "保留日期 08(1)")]
        public string field08_1 { get; set; }

        [Display(Name = "保留日期 08(2)")]
        public string field08_2 { get; set; }

        [Display(Name = "保留日期 08(3)")]
        public string field08_3 { get; set; }

        [Display(Name = "新增人員")]
        public string entry_id { get; set; }

        [Display(Name = "新增日期")]
        public string entry_date { get; set; }

        [Display(Name = "新增時間")]
        public string entry_time { get; set; }

        [Display(Name = "異動人員")]
        public string upd_id { get; set; }

        [Display(Name = "異動日期")]
        public string upd_date { get; set; }

        [Display(Name = "異動時間")]
        public string upd_time { get; set; }

        public FGLGITM0Model()
        {
            item_type = "";
            item = "";
            sys_type = "";
            year = "";
            prem_y_tp = "";
            age = "";
            field10_1 = "";
            field10_2 = "";
            field10_3 = "";
            field08_1 = "";
            field08_2 = "";
            field08_3 = "";
            entry_id = "";
            entry_date = "";
            entry_time = "";
            upd_id = "";
            upd_date = "";
            upd_time = "";
            
        }
    }
}