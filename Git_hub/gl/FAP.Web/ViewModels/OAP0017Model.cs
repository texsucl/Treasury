using FAP.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0017Model
    {
        [Display(Name = "清理狀態")]
        public string status { get; set; }
        public string paid_id { get; set; }

        public string check_no { get; set; }

        public string check_acct_short { get; set; }

        public string level_1 { get; set; }

        public string level_2 { get; set; }

        public string practice { get; set; }

        public string cert_doc { get; set; }

        public string exec_date { get; set; }

        public string proc_desc { get; set; }

        public string seq { get; set; }


        public OAP0017Model() {
            status = "";

            paid_id = "";
            check_no = "";
            check_acct_short = "";
            level_1 = "";
            level_2 = "";
            practice = "";
            cert_doc = "";
            exec_date = "";
            proc_desc = "";
            seq = "";
        }
    }
}