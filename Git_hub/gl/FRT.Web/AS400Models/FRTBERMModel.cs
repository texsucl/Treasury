using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FRTBERMModel
    {
        public string errCode { get; set; }

        public string errDesc { get; set; }

        public string errBelong { get; set; }

        public string transCode { get; set; }


        public FRTBERMModel()
        {
            errCode = "";
            errDesc = "";
            errBelong = "";
            transCode = "";
        }
    }
}