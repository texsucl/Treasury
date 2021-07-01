using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class ADModel
    {
        public string user_id { get; set; }


        [Display(Name = "姓名")]
        public string name { get; set; }

        [Display(Name = "email")]
        public string e_mail { get; set; }

        [Display(Name = "單位代碼")]
        public string department { get; set; }



        public ADModel() {
            user_id = "";
            name = "";
            e_mail = "";
            department = "";

        }

    }
}