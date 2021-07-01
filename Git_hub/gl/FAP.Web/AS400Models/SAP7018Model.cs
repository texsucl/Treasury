using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class SAP7018Model
    {
        
        public string system { get; set; }

        public string policy_no { get; set; }

        public string policy_seq { get; set; }

        public string id_dup { get; set; }

        public string rtn_code { get; set; }

        public string mobile { get; set; }

        public string tel { get; set; }

        public SAP7018Model()
        {
            system = "";
            policy_no = "";
            policy_seq = "";
            rtn_code = "";
            tel = "";
        }
    }

}