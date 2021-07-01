using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0045Model
    {
        [Display(Name = "設定項目")]
        public string code_type { get; set; }

        public string code_id { get; set; }

        public string code_id_desc { get; set; }

        public string proc_id { get; set; }

        public string proc_name { get; set; }

        public int std_1 { get; set; }

        public int std_2 { get; set; }

        public int std_3 { get; set; }

        public string data_status { get; set; }

        public string remark { get; set; }

        public string update_id { get; set; }

        public string update_name { get; set; }

        public string update_datetime { get; set; }

        public string appr_id { get; set; }

        public string approve_datetime { get; set; }

        public string aply_no { get; set; }
        public string exec_action { get; set; }


        public OAP0045Model() {
            code_type = "";
            code_id = "";
            code_id_desc = "";
            proc_id = "";
            proc_name = "";
            std_1 = 0;
            std_2 = 0;
            std_3 = 0;
            data_status = "";
            remark = "";
            update_id = "";
            update_name = "";
           // update_datetime = "";
            appr_id = "";
           // approve_datetime = "";
            aply_no = "";
            exec_action = "";

        }

    }
}