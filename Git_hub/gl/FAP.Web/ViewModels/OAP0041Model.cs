using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FAP.Web.ViewModels
{
    public class OAP0041Model
    {

        public string code_type { get; set; }

        public string code_id { get; set; }

        public string code_value { get; set; }

        public string data_status { get; set; }

        public string remark { get; set; }

        public string update_id { get; set; }

        public string update_name { get; set; }

        public string update_datetime { get; set; }

        public string appr_id { get; set; }

        public string approve_datetime { get; set; }


        public string name { get; set; }
        public string e_mail { get; set; }

        public string aply_no { get; set; }
        public string exec_action { get; set; }


        public OAP0041Model() {
            code_type = "";
            code_id = "";
            code_value = "";
            data_status = "";
            remark = "";
            update_id = "";
            update_name = "";
            update_datetime = "";
            appr_id = "";
            approve_datetime = "";

            name = "";
            e_mail = "";

            aply_no = "";
            exec_action = "";

        }

    }
}