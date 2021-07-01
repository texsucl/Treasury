using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0009Model
    {

        [Display(Name = "在檔位置")]
        public string linePos { get; set; }

        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "郵遞區號")]
        public string zip_code { get; set; }

        [Display(Name = "地址")]
        public string address { get; set; }

        [Display(Name = "檢核訊息")]
        public string msg { get; set; }

        public string chkFlag { get; set; }

        [Display(Name = "地址類別")]
        public string addr_type { get; set; }

        public OAP0009Model() {
            linePos = "";
            paid_id = "";
            paid_name = "";
            zip_code = "";
            address = "";
            msg = "";
            chkFlag = "";
            addr_type = "";
        }

    }
}