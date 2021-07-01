using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class SGLZ001Model
    {
        
        public string numtype { get; set; }

        public string sys_type { get; set; }

        public string srce_from { get; set; }

        public string trns_itf { get; set; }

        public string sys_date { get; set; }

        public string trns_no { get; set; }

        public string rtn_code { get; set; }


        public SGLZ001Model()
        {
            numtype = "";
            sys_type = "";
            srce_from = "";
            trns_itf = "";
            sys_date = "";
            trns_no = "";
            rtn_code = "";
        }

    }
}