using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FRT.Web.ViewModels
{
    public class ORTB016Model
    {
        public string frt_word_Id { get; set; }

        [Display(Name = "申請單號")]
        public string aplyNo { get; set; }
       
        [Display(Name = "系統別")]
        public string frt_sys_type { get; set; }

        [Display(Name = "資料來源")]
        public string frt_srce_from { get; set; }

        [Display(Name = "資料類別")]
        public string frt_srce_kind { get; set; }

        [Display(Name = "存摺顯示字樣")]
        public string frt_memo_apx { get; set; }

        [Display(Name = "eACH交易代碼")]
        public string frt_achcode { get; set; }

        [Display(Name = "資料狀態")]
        public string dataStatus { get; set; }

        [Display(Name = "執行功能")]
        public string status { get; set; }
        //public string statusDesc { get; set; }

        [Display(Name = "異動人員")]
        public string updId { get; set; }

        public string updateUName { get; set; }

        [Display(Name = "異動日期")]
        public string updDatetime { get; set; }

        [Display(Name = "覆核人員")]
        public string apprId { get; set; }

        public string apprName { get; set; }

        [Display(Name = "覆核日期")]
        public string apprDt { get; set; }


    }
}