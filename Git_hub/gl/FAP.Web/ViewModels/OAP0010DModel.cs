using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0010DModel
    {
        public string temp_id { get; set; }


        [Display(Name = "踐行程序")]
        public string practice { get; set; }

        [Display(Name = "証明文件")]
        public string cert_doc { get; set; }

        [Display(Name = "執行日期")]
        public string exec_date { get; set; }

        [Display(Name = "過程說明")]
        public string proc_desc { get; set; }

        [Display(Name = "異動人員")]
        public string update_id { get; set; }

        [Display(Name = "異動日期")]
        public string update_datetime { get; set; }


        public OAP0010DModel() {
            temp_id = "";

            practice = "";
            cert_doc = "";
            exec_date = "";
            proc_desc = "";

            update_id = "";
            update_datetime = "";

        }

    }
}