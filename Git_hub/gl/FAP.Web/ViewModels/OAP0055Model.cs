using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0055Model
    {


        [Display(Name = "給付對象ID")]
        public string paid_id { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "郵遞區號")]
        public string zip_code { get; set; }

        [Display(Name = "地址")]
        public string address { get; set; }


        [Display(Name = "地址類別")]
        public string addr_type { get; set; }

        public OAP0055Model() {
            paid_id = "";
            paid_name = "";
            zip_code = "";
            address = "";
            addr_type = "";
        }

    }
}