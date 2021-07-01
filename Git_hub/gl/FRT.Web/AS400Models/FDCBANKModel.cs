using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FRT.Web.AS400Models
{
    public class FDCBANKModel
    {
        public string bankNo { get; set; }
        public string bankName { get; set; }
        public string shrBq { get; set; }

        public FDCBANKModel()
        {
            bankNo = "";
            bankName = "";
            shrBq = "";
        }
    }
}