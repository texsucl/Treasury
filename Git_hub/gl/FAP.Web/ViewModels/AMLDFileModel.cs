using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class AMLDFileModel
    {

        [Display(Name = "AML狀態")]
        public string stat_code { get; set; }

        [Display(Name = "客戶編號")]
        public string cin_no { get; set; }

        [Display(Name = "給付對象姓名")]
        public string paid_name { get; set; }

        [Display(Name = "查詢來源")]
        public string source_id { get; set; }

        [Display(Name = "查詢單位")]
        public string unit { get; set; }

        [Display(Name = "ＡＭＬ交易序號")]
        public string trans_id { get; set; }

        [Display(Name = "保單角色")]
        public string role_id { get; set; }


        public AMLDFileModel() {
            stat_code = "";
            cin_no = "";
            paid_name = "";
            source_id = "";
            unit = "";
            trans_id = "";
            role_id = "";
        }

    }
}