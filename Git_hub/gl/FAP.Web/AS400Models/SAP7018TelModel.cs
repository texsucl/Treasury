using FAP.Web.BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FAP.Web.AS400Models
{
    public class SAP7018TelModel
    {
        
        public string tel_type { get; set; }

        public string tel { get; set; }

        public string upd_date { get; set; }

        public string address { get; set; }


        public SAP7018TelModel()
        {
            tel_type = "";
            tel = "";
            upd_date = "";
            address = "";
        }
    }

}